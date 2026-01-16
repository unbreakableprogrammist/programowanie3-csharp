using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;


namespace Chat.Common.MessageHandlers;


public class MessageReader(Stream stream) : MessageHandler, IDisposable // dziedziczymy po klasie message handler dlatego mamy zmienne Headerlen i MaxMessageLen
{
    public async Task<MessageDTO?> ReadMessage(CancellationToken ct)
    {
        var header = new byte[HeaderLen]; // tworzymy buffor na headera = ile bajtow ma wiadomosc 
        int bytesRead = await ReadBuffer(header, ct);
        if (bytesRead < HeaderLen) // jesli sie nie uda wczytac to polaczenie moglo byc zerwane 
        {
            return null;
        }
        // zamieniamy to na inta
        int payloadLen = BinaryPrimitives.ReadInt32BigEndian(header);
        if (payloadLen > MaxMessageLen)
        {
            throw new TooLongMessageException("Invalid message length");
        }

        if (payloadLen < 0)
        {
            throw new InvalidMessageException("Invalid message length");
        }
        var payload = new byte[payloadLen];
        bytesRead = await ReadBuffer(payload, ct);
        if (bytesRead < payloadLen) // polaczenie zerwane 
        {
            return null;
        }
        
        // teraz deserializacja  
        string json = Encoding.UTF8.GetString(payload);
        var message = JsonConvert.DeserializeObject<MessageDTO>(json);
        if (message == null)
            throw new InvalidMessageException("Zdeserializowany obiekt to null");
        return message;
        
        
    }

    private async Task<int> ReadBuffer(byte[] buffer, CancellationToken ct)
    {
        int bytesRead = 0;
        while (bytesRead < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer, bytesRead, buffer.Length - bytesRead, ct);
            if (read == 0)
            {
                break;
            }
            bytesRead += read;
        }
        return bytesRead;
    }


    public void Dispose()
    {
        stream.Dispose();
    }
}