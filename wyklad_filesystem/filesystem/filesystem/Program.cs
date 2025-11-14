using System.IO.Compression;

class Program{
    void file_testing()
    {
        using FileStream file = File.Open("lorem.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        file.Close();
        // czytanie calego tekstu 
        string content = File.ReadAllText("lorem.txt");
        Console.WriteLine(content);

        // tworzy/nadpsuje caly tekst , ktory jest w contencie
        File.WriteAllText("test2.txt", content);

        // dodawanie do pliku 
        const string logFile = "log.txt";
        for (int i = 0; i < 100; i++)
        {
            // append dodaje na koncu pliku 
            File.AppendAllText(logFile, $"[Info] This is a log entry no. {i} \n");
        }

        // czytamy , po separatorze ( "\n") 
        string[] lines = File.ReadAllLines(logFile);
        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }

        // tworzy/nadpisuje plik lorem_lines , kazdy element tablicy = jedna linia 
        File.WriteAllLines("lorem_lines.txt", lines);

        // to samo co ze strintagmi tylko tutaj po prostu bierzemy bity  
        //byte[] bytes = File.ReadAllBytes("lorem.bin");
        //File.WriteAllBytes("lorem.bin.bak", bytes);

        //przyklad lazy porgrammingu , bedziemy czytac te linie dopoku gdy zaczniemy isc forem po lines2
        IEnumerable<string> lines2 = File.ReadLines("lorem.txt");

        // takie cos wywoluje enumeracje 
        File.AppendAllLines("lorem3.txt", lines2);
        foreach (var line in lines2)
        {
            Console.WriteLine(line);
        }

        // Otwieramy plik w trybie tylko-do-odczytu.
        // FileStream wykonuje prawdziwe operacje dyskowe.
        FileStream fs = File.OpenRead("file.bin");

        // Owijamy FileStream w BufferedStream z buforem 20 000 bajtów.
        // W TYM MOMENCIE NIC NIE JEST JESZCZE WCZYTANE.
        // Bufer zostanie napełniony dopiero przy pierwszym Read/ReadByte().
        BufferedStream bs = new BufferedStream(fs, 20_000);

        int b;

        // Czytamy bajt po bajcie z BufferedStream.
        // Pierwsze wywołanie ReadByte() napełnia wewnętrzny bufor (np. 20k bajtów).
        // Kolejne wywołania ReadByte() zwracają dane z RAM,
        // a gdy bufor się wyczerpie, BufferedStream ponownie doładuje go z pliku.
        while ((b = bs.ReadByte()) != -1)
        {
            Console.WriteLine(b);
        }

        fs.Close();
        
        // tworzymy nowy plik dane.bin.gz
        using Stream FS = new FileStream("dane.bin.gz", FileMode.Create);
        // wczytujemy caly plik FS do bufora
        using Stream BS = new BufferedStream(FS);
        // compresujemy plik 
        using Stream CS = new GZipStream(bs, CompressionMode.Compress);
        
        byte[] data = new byte[1000];
        CS.Write(data, 0, data.Length);
        // Twoje dane → GZipStream (kompresja) → BufferedStream (bufor) → FileStream (dysk)
        CS.Close();
        CS.Dispose();
        
    }

    public static void adaptery_strumieni()
    {
        using FileStream fs = File.OpenRead("lorem.txt");
        using StreamReader sr = new StreamReader(fs);

        while (sr.ReadLine() is { } line)
        {
            Console.WriteLine(line);
        }
        using FileStream fs2 = File.Open("fizzbuzz.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using StreamWriter sw = new StreamWriter(fs2);

        for (int i = 1; i <= 100; i++)
        {
            sw.Write($"{i} : ");
            if (i % 3 == 0 && i % 5 == 0)
                sw.WriteLine("FizzBuzz");
            else if (i % 3 == 0)
                sw.WriteLine("Fizz");
            else if (i % 5 == 0)
                sw.WriteLine("Buzz");
            else
                sw.WriteLine(i);
        }
    }

    static void Main(string[] args)
    {
        adaptery_strumieni();
    }
}