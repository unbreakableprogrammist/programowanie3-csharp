using System;
using System.IO; 
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http; 
using System.IO.Compression;
using MetadataExtractor; // Biblioteka z NuGet do wyciągania danych ze zdjęć
using MetadataExtractor.Formats.Exif;
using System.Linq;
using System.Text.Json; 
using System.Globalization;

class Program
{
    // Ścieżka docelowa - tutaj wpadną posortowane fotki
    public static string destinated_path = "/Users/krzysztof/Documents/csharp/Projekt1/SortedPhotos/";
    
    // Klient do łączenia się z API Nominatim (OpenStreetMap)
    private static readonly HttpClient client = new HttpClient();

    // Prosta struktura, żeby łatwiej przekazywać współrzędne
    struct GeoCoordinate { public double Lat; public double Lon; }
    
    static GeoCoordinate? GetGpsFromPhoto(string path)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(path); // tu czytamy metadane
            var gpsDir = directories.OfType<GpsDirectory>().FirstOrDefault(); // tu zbieramy same dane gps

            if (gpsDir != null)
            {
                // Jeśli zdjęcie ma GPS, to zwracamy koordynaty
                var location = gpsDir.GetGeoLocation();
                if (location != null)
                    return new GeoCoordinate { Lat = location.Latitude, Lon = location.Longitude };
            }
        }
        catch { /* Jak błąd odczytu, to trudno - traktujemy jak brak GPS */ }
        return null;
    }

    static DateTime GetDateTaken(string path)
    {
        try
        {
            // Próbujemy wyciągnąć datę zrobienia zdjęcia ("Date/Time Original")
            var directories = ImageMetadataReader.ReadMetadata(path);
            var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            var dateString = subIfdDirectory?.GetDescription(ExifSubIfdDirectory.TagDateTimeOriginal);
            if (dateString != null && DateTime.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                // Dodatkowe sprawdzenie: czasem data jest błędna (np. rok 0001), więc bierzemy tylko sensowne
                if (parsedDate.Year > 1970) return parsedDate;
            }
        }
        catch { }

        //Jeśli nie ma EXIF, bierzemy datę modyfikacji pliku z systemu
        DateTime fileTime = File.GetLastWriteTime(path);
        
        
        // zabezpiecznie przed 1601
        if (fileTime.Year < 1970) return DateTime.Now;

        return fileTime;
    }
    
    // Zamieniamy cyferki (GPS) na nazwę miasta
    static async Task<string?> GetLocationName(double lat, double lon)
    {
        try
        {
            string latStr = lat.ToString(CultureInfo.InvariantCulture);
            string lonStr = lon.ToString(CultureInfo.InvariantCulture);

            client.DefaultRequestHeaders.UserAgent.ParseAdd("MojProgramSortujacy/1.0 (krzysztof@test.com)");
            string url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={latStr}&lon={lonStr}";
            
            var response = await client.GetStringAsync(url);
            using JsonDocument doc = JsonDocument.Parse(response);
            
            if (doc.RootElement.TryGetProperty("address", out var address))
            {
                string miasto = "Nieznane";
                // Różne kraje różnie oznaczają miasta w OpenStreetMap, więc sprawdzamy kilka opcji
                if (address.TryGetProperty("city", out var c)) miasto = c.GetString() ?? "";
                else if (address.TryGetProperty("town", out var t)) miasto = t.GetString() ?? "";
                else if (address.TryGetProperty("village", out var v)) miasto = v.GetString() ?? "";
                
                string kraj = "NieznanyKraj";
                if (address.TryGetProperty("country", out var cnt)) kraj = cnt.GetString() ?? "";

                // Zwracamy w formacie "Kraj_Miasto" (z podkreślnikiem, żeby pasowało do nazwy pliku)
                return $"{kraj}_{miasto}";
            }
        }
        catch { }
        return null;
    }

    static async Task Handle_photo(string sciezkaDoPliku)
    {
        if (!File.Exists(sciezkaDoPliku)) return;

        try
        {
            string nazwaPliku = Path.GetFileName(sciezkaDoPliku);
            
            // Ignoruję pliki ukryte (zaczynające się od kropki), np .DS_Store
            if (nazwaPliku.StartsWith(".")) return;

            //Ustalamy folder na podstawie daty (Rok/Miesiąc)
            DateTime dataZdjecia = GetDateTaken(sciezkaDoPliku);
            string rok = dataZdjecia.Year.ToString();
            string miesiac = dataZdjecia.ToString("MM");
            string folderDocelowy = Path.Combine(destinated_path, rok, miesiac);

            //Jeśli jest GPS, zmieniamy nazwę pliku
            string nowaNazwaPliku = nazwaPliku;
            var coords = GetGpsFromPhoto(sciezkaDoPliku);
            
            if (coords != null)
            {
                Console.WriteLine($"[GPS] Pobieram lokalizację dla: {nazwaPliku}...");
                string? lokalizacja = await GetLocationName(coords.Value.Lat, coords.Value.Lon);

                if (!string.IsNullOrEmpty(lokalizacja))
                {
                    // Usuwam ukośniki, żeby nie psuły ścieżki
                    lokalizacja = lokalizacja.Replace("/", "").Replace("\\", ""); 
                    nowaNazwaPliku = $"{lokalizacja}_{nazwaPliku}";
                }
            }

            // Tworzenie folderów i przenoszenie
            System.IO.Directory.CreateDirectory(folderDocelowy);
            string pelnaSciezka = Path.Combine(folderDocelowy, nowaNazwaPliku);

            // Obsługa duplikatów - jak plik już jest, dodaję losowy id
            if (File.Exists(pelnaSciezka))
            {
                string nazwaBezExt = Path.GetFileNameWithoutExtension(nowaNazwaPliku);
                string ext = Path.GetExtension(nowaNazwaPliku);
                pelnaSciezka = Path.Combine(folderDocelowy, $"{nazwaBezExt}_{Guid.NewGuid().ToString().Substring(0, 4)}{ext}");
            }

            // przeniesienie
            if (File.Exists(sciezkaDoPliku))
            {
                File.Move(sciezkaDoPliku, pelnaSciezka);
                Console.WriteLine($"[SUKCES] Przeniesiono do: {rok}/{miesiac}/{Path.GetFileName(pelnaSciezka)}");
            }
        }
        catch (Exception ex)
        {
            if (!ex.Message.Contains("Could not find file"))
            {
                Console.WriteLine($"[BŁĄD] {ex.Message}");
            }
        }
    }

    // Obsługa archiwów ZIP
    static async Task Handle_zip(string sciezkaZip)
    {
        if (!File.Exists(sciezkaZip)) return;

        Console.WriteLine($"[ZIP] Rozpakowuję: {Path.GetFileName(sciezkaZip)}...");
        // Tworzę folder tymczasowy w systemie (/tmp/...) żeby nie śmiecić w Pobranych
        string tempFolder = Path.Combine(Path.GetTempPath(), "Unzipped_" + Guid.NewGuid().ToString());
        System.IO.Directory.CreateDirectory(tempFolder);

        try
        {
            ZipFile.ExtractToDirectory(sciezkaZip, tempFolder);
            string[] pliki = System.IO.Directory.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories);

            foreach (var plik in pliki)
            {
                string nazwa = Path.GetFileName(plik);
                if (nazwa.StartsWith("._") || nazwa.StartsWith(".")) continue; // Ignorujemy śmieci w ZIPie

                string ext = Path.GetExtension(plik).ToLower();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".heic")
                {
                    // Rekurencyjnie wywołuję funkcję do zdjęć
                    await Handle_photo(plik);
                    
                    // Opóźnienie, żeby nie zbanowali nas na darmowym API 
                    Thread.Sleep(1100); 
                }
            }
        }
        catch (Exception ex) { Console.WriteLine($"[BŁĄD ZIP] {ex.Message}"); }
        finally
        {
            // Sprzątanie po sobie (usuwamy folder tymczasowy)
            if (System.IO.Directory.Exists(tempFolder)) 
                System.IO.Directory.Delete(tempFolder, true);
        }
    }

    static async void OnCreate(object sender, FileSystemEventArgs e)
    {
        string nazwa = e.Name;
        string sciezka = e.FullPath;

        
        // Ignorujemy pliki zaczynające się od kropki (np. .DS_Store, ._IMG.jpg)
        if (string.IsNullOrEmpty(nazwa) || nazwa.StartsWith(".")) return;
        
        // Ignorujemy sam folder docelowy, żeby nie wpaść w pętlę nieskończoną
        if (sciezka.Contains("SortedPhotos")) return;

        // Czekamy sekundę, aż system zwolni plik (np. zakończy pobieranie)
        Thread.Sleep(1000); 

        // Ponowne sprawdzenie czy plik istnieje
        if (!File.Exists(sciezka)) return;

        string ext = Path.GetExtension(sciezka).ToLower();
        // Rozdzielamy logikę dla ZIPów i zwykłych zdjęć
        if (ext == ".zip") await Handle_zip(sciezka);
        else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".heic") await Handle_photo(sciezka);
    }

    public static void Watch(string sciezka)
    {
        if (!System.IO.Directory.Exists(sciezka)) { Console.WriteLine("Brak folderu!"); return; }

        using var watcher = new FileSystemWatcher(sciezka);
        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
        // Podpinamy naszą funkcję pod zdarzenie utworzenia pliku
        watcher.Created += (s, e) => OnCreate(s, e);
        watcher.EnableRaisingEvents = true;

        Console.WriteLine($"--- Strażnik v3.1 (Bez 1601/DotFiles) ---");
        Console.WriteLine($"Obserwuję: {sciezka}");
        Console.WriteLine("Naciśnij ENTER, aby zamknąć.");
        Console.ReadLine(); 
    }

    static Task Main(string[] args)
    {
        if (args.Length == 0) { Console.WriteLine("Podaj ścieżkę!"); return Task.CompletedTask; }
        Watch(args[0]);
        return Task.CompletedTask;
    }
}