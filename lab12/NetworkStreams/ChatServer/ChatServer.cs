using System.Net;
using System.Net.Sockets;
using Chat.Common;
using Chat.Common.MessageHandlers;

namespace ChatServer;

public class ChatServer(IPAddress address, int port)
{
    public async Task Run(CancellationToken ct)
    {
        Console.WriteLine($"Server running on port {port} ...");

        try
        {
            // Główna pętla serwera - działa dopóki nie wyłączymy programu (np. Ctrl+C)
            while (!ct.IsCancellationRequested)
            {
                // Tworzymy 'nasłuchiwacza' na danym porcie. 
                // Using gwarantuje, że port zostanie zwolniony po zakończeniu pętli.
                using var listener = new TcpListener(address, port);
                listener.Start();

                Console.WriteLine("Waiting for the first client...");
                // Program tutaj 'zasypia' (asynchronicznie) i czeka na pierwszą osobę.
                using var client1 = await listener.AcceptTcpClientAsync(ct);
                Console.WriteLine($"Client 1 connected from {client1.Client.RemoteEndPoint}");

                Console.WriteLine("Waiting for a second client...");
                // Program znowu czeka, tym razem na drugą osobę do pary.
                using var client2 = await listener.AcceptTcpClientAsync(ct);
                Console.WriteLine($"Client 2 connected from {client2.Client.RemoteEndPoint}");

                Console.WriteLine("Both clients connected - starting forwarding messages.");

                // Mamy parę! Przekazujemy ich do metody obsługującej rozmowę.
                // Używamy await, więc serwer wróci do szukania nowej pary dopiero gdy ta skończy rozmawiać.
                await HandleClientsAsync(client1, client2, ct);
                
                Console.WriteLine("Conversation finished. Ready for a new pair.");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Server shut down.");
        }
    }

    static async Task HandleClientsAsync(TcpClient client1, TcpClient client2, CancellationToken ct)
    {
        // Pobieramy strumienie (NetworkStream) - to są 'rury' do przesyłu bajtów
        await using var stream1 = client1.GetStream();
        await using var stream2 = client2.GetStream();

        // Lokalny 'wyłącznik' - jeśli jeden klient się rozłączy, użyjemy go by zatrzymać przesył drugiego
        using var messageForwardCts = new CancellationTokenSource();

        // Tworzymy narzędzia do czytania i pisania dla każdego klienta
        using var readerFromClient1 = new MessageReader(stream1);
        using var readerFromClient2 = new MessageReader(stream2);
        using var writerToClient1 = new MessageWriter(stream1);
        using var writerToClient2 = new MessageWriter(stream2);

        // URUCHAMIAMY PRZESYŁANIE (Mosty)
        // t1: czyta od klienta 1 i pisze do klienta 2
        var t1 = ForwardMessagesAsync(readerFromClient1, writerToClient2, messageForwardCts.Token);
        // t2: czyta od klienta 2 i pisze do klienta 1
        var t2 = ForwardMessagesAsync(readerFromClient2, writerToClient1, messageForwardCts.Token);
        
        // Zadanie-bezpiecznik: kończy się tylko gdy wyłączymy serwer
        var cancellationTask = Task.Delay(-1, ct);

        // WYŚCIG: Czekamy na pierwsze wydarzenie: ktoś wyszedł ALBO wyłączono serwer
        var finishedTask = await Task.WhenAny(t1, t2, cancellationTask);

        if (finishedTask == cancellationTask)
        {
            // Jeśli wyłączono serwer, wysyłamy obu klientom komunikat o zamknięciu
            await SendCancellationNotification(writerToClient1);
            await SendCancellationNotification(writerToClient2);
        }
        else
        {
            // Jeśli jeden klient wyszedł, znajdujemy tego, który został i go informujemy
            var remainingClientWriter = finishedTask == t1 ? writerToClient2 : writerToClient1;
            await SendPeerDisconnectedNotification(remainingClientWriter, ct);
        }

        // Zatrzymujemy drugie zadanie przesyłania (skoro jeden już wyszedł)
        if (!messageForwardCts.IsCancellationRequested)
            await messageForwardCts.CancelAsync();
        
        // Czekamy na uporządkowane zamknięcie obu zadań
        await Task.WhenAll(t1, t2);
        
        Console.WriteLine("Clients disconnected, pair handler finished.");
    }

    static async Task ForwardMessagesAsync(MessageReader reader, MessageWriter writer, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // 1. Odbierz wiadomość od Nadawcy
                var msg = await reader.ReadMessage(ct);
                
                // 2. Jeśli null, to znaczy że Nadawca zamknął program
                if (msg == null) break; 

                // 3. Wypisz treść na konsoli serwera (wymóg zadania)
                Console.WriteLine($"[MSG] {msg.Sender}: {msg.Content}");

                // 4. Prześlij tę samą wiadomość do Odbiorcy
                await writer.WriteMessage(msg, ct);
            }
        }
        catch (Exception)
        {
            // Błędy sieciowe przerywają pętlę przesyłania
        }
    }

    // Metody pomocnicze do wysyłania powiadomień systemowych (jako MessageDTO)
    static async Task SendCancellationNotification(MessageWriter writer) { /* ... implementacja ... */ }
    static async Task SendPeerDisconnectedNotification(MessageWriter writer, CancellationToken ct) { /* ... implementacja ... */ }
}