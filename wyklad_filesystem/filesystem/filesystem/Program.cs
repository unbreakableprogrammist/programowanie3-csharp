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

    static public void paths()
    {
        string path = Path.GetFullPath("podsumowanie.md");
        

        Console.WriteLine($"File Name: {Path.GetFileName(path)}"); // hugo.yaml
        Console.WriteLine($"Name without extension: {Path.GetFileNameWithoutExtension(path)}"); // hugo
        Console.WriteLine($"Extension: {Path.GetExtension(path)}");  // .yaml
        Console.WriteLine($"Parent Directory: {Path.GetDirectoryName(path)}"); // Workspace/csharp-site
        Console.WriteLine($"Full Path: {Path.GetFullPath(path)}"); // /home/tomasz/Workspace/csharp-site/hugo.yaml (resolves relative to the current working directory)
        Console.WriteLine($"Directory Separator: {Path.DirectorySeparatorChar}"); // \ (on Windows) / (on Linux)
    }

    static public void pliczki()
    {
        using FileStream fs = File.Open("lorem.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using StreamWriter sr = new StreamWriter(fs);
        for (int i = 0; i < 1000; i++)
        {
            char litera = (char)((i%26)+'a');
            sr.Write($"{i} : {litera} \n");
        }
        Console.WriteLine(File.GetCreationTime("lorem.txt"));
        const int GB = 1024 * 1024 * 1024;
        DriveInfo root = new DriveInfo("/");

        Console.WriteLine($"Total size: {root.TotalSize / GB} GB");
        Console.WriteLine($"Free size: {root.TotalFreeSpace / GB} GB"); // Ignoring quotas
        Console.WriteLine($"Available size: {root.AvailableFreeSpace / GB} GB");

        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            Console.WriteLine(drive.Name);
        }
        
    }

    public static void archiwa()
    {
        // katalog źródłowy do spakowania
        const string sourceDirectory = "my_files_to_zip";
// ścieżka docelowa ZIP-a
        const string zipPath = "archive.zip";
// katalog, do którego wypakujemy ZIP
        const string extractPath = "extracted_files";

// --- Przygotowanie plików do spakowania ---

// tworzymy katalog (jeśli nie istnieje, nic złego się nie stanie)
        Directory.CreateDirectory(sourceDirectory);

// tworzymy plik "file1.txt" z treścią "Hello"
        File.WriteAllText(Path.Combine(sourceDirectory, "file1.txt"), "Hello");

// tworzymy podfolder "subfolder"
        Directory.CreateDirectory(Path.Combine(sourceDirectory, "subfolder"));

// tworzymy plik "file2.txt" w podfolderze
        File.WriteAllText(Path.Combine(sourceDirectory, "subfolder", "file2.txt"), "World");


// --- Tworzenie archiwum ZIP ---

// jeśli ZIP już istnieje → usuń go, aby nowy nie robił konfliktu
        if (File.Exists(zipPath)) 
            File.Delete(zipPath);

// spakuj cały katalog sourceDirectory do pliku ZIP
        ZipFile.CreateFromDirectory(sourceDirectory, zipPath);


// --- Rozpakowanie ZIP-a ---

// jeśli katalog docelowy istnieje → usuń go rekursywnie (żeby wypakować „na czysto”)
        if (Directory.Exists(extractPath)) 
            Directory.Delete(extractPath, recursive: true);

// wypakuj ZIP do katalogu extractPath
        ZipFile.ExtractToDirectory(zipPath, extractPath);
    }
}