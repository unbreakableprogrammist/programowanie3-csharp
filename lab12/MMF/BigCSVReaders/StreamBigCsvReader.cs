namespace BigCsvReaders;

public class StreamBigCsvReader : BigCsvReader
{
    public StreamBigCsvReader(string path, char delimiter = ','): base(path, delimiter)
    {
        // Otwieramy plik z "indeksem" (.offsets)
        _offsetsFileStream = File.OpenRead(OffsetsFile);
        // Nakładamy na niego okulary do czytania liczb binarnych (bo offsety to liczby long)
        _offsetsReader = new BinaryReader(_offsetsFileStream);
        
        // Otwieramy główny plik z danymi (.csv)
        _csvFileStream = new FileStream(CsvFile, FileMode.Open);
        // Nakładamy na niego okulary do czytania tekstu (żeby mieć metodę ReadLine)
        _csvReader = new StreamReader(_csvFileStream);
    }


    protected override string ReadRow(int row)
    {
        // Otwieramy główny plik z danymi (.csv)
        _csvFileStream = new FileStream(CsvFile, FileMode.Open);
        // Nakładamy na niego okulary do czytania tekstu (żeby mieć metodę ReadLine)
        _csvReader = new StreamReader(_csvFileStream);
    }


    public override void Dispose()
    {
        // TODO

        base.Dispose();
    }
}
///
///byte flags = 0;
if (Up)    flags |= (1 << 0); // Bit 0
if (Down)  flags |= (1 << 1); // Bit 1
if (Left)  flags |= (1 << 2); // Bit 2
if (Right) flags |= (1 << 3); // Bit 3
if (Sprint) flags |= (1 << 4); // Bit 4
return new byte[] { flags };

byte flags = bytes[0];
Up    = (flags & (1 << 0)) != 0;
Down  = (flags & (1 << 1)) != 0;
// ... itd.
///
///
///
/// 