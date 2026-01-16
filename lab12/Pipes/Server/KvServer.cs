using System.IO.Pipes;
using System.Text;

namespace Server;

public class KvServer(string pipeName)
{
    public async Task Start(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // podlaczamy sie do tego samego pipe
                await using var server = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte, 
                    PipeOptions.Asynchronous);
                // czekamy az klient sie podlaczy
                await server.WaitForConnectionAsync(ct);
                // wtedy odpalamy zeby sie ogarnac clienta
                await HandleClientAsync(server, ct);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Server was cancelled");
        }
    }

    private readonly Dictionary<string, string> _db = new();
    
    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken ct)
    {
        Console.WriteLine("Client connected.");
        // czytacz 
        using var reader = new StreamReader(pipe, Encoding.UTF8);
        // pisacz
        using var writer = new StreamWriter(pipe, Encoding.UTF8) { AutoFlush = true };
        try
        {
            // dopoki nie ma cr lub pipe sie nie rozlaczy
            while (!ct.IsCancellationRequested && pipe.IsConnected)
            {
                // wez linijke ( moze byc null), czekaj na nia asynchronicznie
                string? line = await reader.ReadLineAsync(ct);
                // jesli null to finito
                if (line == null) break;
                // napisz odpowiedz 
                string response = ProcessCommand(line);
                // wpisujemy + zwalniamy watek dopoki nie wpisze sie wszystko do pipe 
                await writer.WriteLineAsync(response);
            }
        }
        catch (IOException)
        {
            Console.WriteLine("Problem with client communication");
        }
       
        Console.WriteLine("Client disconnected.");
    }
    
    private string ProcessCommand(string input)
    {
        string[] parts = input.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "ERROR Empty command";

        string cmd = parts[0].ToUpperInvariant();

        return cmd switch
        {
            "SET" => HandleSet(parts),
            "GET" => HandleGet(parts),
            "DELETE" => HandleDelete(parts),
            _ => "ERROR Unknown command"
        };
    }

    private string HandleSet(string[] parts)
    {
        if (parts.Length < 3)
            return "ERROR Usage: SET key value";

        _db[parts[1]] = parts[2];

        return "OK";
    }

    private string HandleGet(string[] parts)
    {
        if (parts.Length < 2)
            return "ERROR Usage: GET key";

        return _db.GetValueOrDefault(parts[1], "NOT_FOUND");
    }

    private string HandleDelete(string[] parts)
    {
        if (parts.Length < 2)
            return "ERROR Usage: DELETE key";

        return _db.Remove(parts[1], out _) ? "OK" : "NOT_FOUND";
    }
}

