using System.IO.Pipes; // Importuje klasy do obsługi potoków (Named Pipes)
using System.Text;     // Importuje klasy do kodowania tekstu (UTF8)

class KeyValueClient
{
    // Stała określająca czas oczekiwania na połączenie (3 sekundy) zgodnie z treścią zadania
    const int ConnectionTimeoutMs = 3000;

    public static async Task Main()
    {
        // Tworzymy instancję potoku nazwanego. 
        // "." oznacza lokalny komputer, "kv_pipe" to nazwa potoku, InOut pozwala czytać i pisać.
        using var client = new NamedPipeClientStream(".", "kv_pipe", PipeDirection.InOut);
        Console.WriteLine("Connecting...");

        try
        {
            // Próba połączenia z serwerem. Jeśli serwer nie odpowie w 3s, rzuci TimeoutException.
            // To idealnie realizuje wymóg z zadania.
            await client.ConnectAsync(ConnectionTimeoutMs);
        }
        catch (TimeoutException)
        {
            // Obsługa błędu czasu połączenia
            Console.WriteLine("Timeout");
            return;
        }
            
        Console.WriteLine("Connected!");

        // Tworzymy adaptery do czytania i pisania tekstu. 
        // UWAGA: StreamReader i StreamWriter domyślnie zamykają strumień (client) przy swoim usuwaniu.
        // Tutaj nie używasz 'using' przy nich, więc jest to bezpieczne.
        var reader = new StreamReader(client, Encoding.UTF8);
        var writer = new StreamWriter(client, Encoding.UTF8);

        Console.WriteLine("Enter commands (SET, GET, DELETE, EXIT):");

        while (true)
        {
            Console.Write("> ");
            string? cmd = Console.ReadLine(); // Pobieramy komendę od użytkownika z konsoli
            if (cmd == null)
                break;
                
            // Wyjście z programu, jeśli użytkownik wpisze "exit"
            if (cmd.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            // Wywołanie metody komunikacji z serwerem
            string? response = await GetResponse(writer, reader, cmd);
            
            // Jeśli GetResponse zwróci null, oznacza to błąd połączenia (serwer padł)
            if (response == null)
                break;
            
            Console.WriteLine(response);
        }

        Console.WriteLine("Disconnected.");
    }

    static async Task<string?> GetResponse(StreamWriter writer, StreamReader reader, string cmd)
    {
        try
        {
            
            // wpisujemy do rury asynchronicznie 
            await writer.WriteLineAsync(cmd); 
            
            // FlushAsync wymusza natychmiastowe wysłanie danych z bufora do potoku
            await writer.FlushAsync();
            
            // Oczekiwanie na odpowiedź od serwera (czyta do znaku nowej linii)
            return await reader.ReadLineAsync();
        }
        catch (IOException)
        {
            // Błąd wejścia/wyjścia (np. serwer gwałtownie przerwał połączenie)
            Console.WriteLine("Problem with server communication");
            return null;
        }
    }
}