using System.Buffers.Binary;
using System.Text;
using Newtonsoft.Json;


namespace Chat.Common.MessageHandlers;


public class MessageWriter(Stream stream) : MessageHandler, IDisposable
{
    public async Task WriteMessage(MessageDTO message, CancellationToken ct)
    {
        // mamy sobie obiekt typu message , zmieniamy go na json i zapisujemy jako ciag charow
        string json = JsonConvert.SerializeObject(message);
        // zmieniamy chary -> byte
        byte[] payload = Encoding.UTF8.GetBytes(json);
        if(payload.Length > MaxMessageLen)
            throw new InvalidMessageException("Zdeserializowany obiekt jest za duży");
        // tworzymy header , z dlugoscia naszej wiadomosci
        var header = new byte[HeaderLen];
        BinaryPrimitives.WriteInt32BigEndian(header,payload.Length);
        await stream.WriteAsync(header, 0, HeaderLen, ct);
        await stream.WriteAsync(payload, 0, payload.Length, ct);
        // od razu flushujmy ct tokena 
        await stream.FlushAsync(ct);
    }


    public void Dispose()
    {
        stream.Dispose();
    }
}
