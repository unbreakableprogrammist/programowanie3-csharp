using System;
using System.IO; 
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http; 
using System.IO.Compression;
using MetadataExtractor; 
using MetadataExtractor.Formats.Exif;
using System.Linq;
using System.Text.Json; 
using System.Globalization; 

class Program
{
    public static string destinated_path = "/Users/krzysztof/Documents/csharp/Projekt1/SortedPhotos/";
    private static readonly HttpClient client = new HttpClient();

    struct GeoCoordinate { public double Lat; public double Lon; }

    // --- 1. Wyciąganie GPS ---
    static GeoCoordinate? GetGpsFromPhoto(string path)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(path);
            var gpsDir = directories.OfType<GpsDirectory>().FirstOrDefault();

            if (gpsDir != null)
            {
                var location = gpsDir.GetGeoLocation();
                if (location != null)
                    return new GeoCoordinate { Lat = location.Latitude, Lon = location.Longitude };
            }
        }
        catch { }
        return null;
    }

    // --- 2. Data z EXIF (Z zabezpieczeniem przed rokiem 1601) ---
    static DateTime GetDateTaken(string path)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(path);
            var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            var dateString = subIfdDirectory?.GetDescription(ExifSubIfdDirectory.TagDateTimeOriginal);

            if (dateString != null && DateTime.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                if (parsedDate.Year > 1970) return parsedDate;
            }
        }
        catch { }

        // Jeśli nie ma EXIF, bierzemy datę pliku
        DateTime fileTime = File.GetLastWriteTime(path);
        
        // ZABEZPIECZENIE: Jeśli data to rok 1601 (błąd systemu), bierzemy dzisiejszą
        if (fileTime.Year < 1970) return DateTime.Now;

        return fileTime;
    }

    // --- 3. API Nominatim ---
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
                if (address.TryGetProperty("city", out var c)) miasto = c.GetString() ?? "";
                else if (address.TryGetProperty("town", out var t)) miasto = t.GetString() ?? "";
                else if (address.TryGetProperty("village", out var v)) miasto = v.GetString() ?? "";
                
                string kraj = "NieznanyKraj";
                if (address.TryGetProperty("country", out var cnt)) kraj = cnt.GetString() ?? "";

                return $"{kraj}_{miasto}";
            }
        }
        catch { }
        return null;
    }

    // --- 4. Główna logika ---
    static async Task Handle_photo(string sciezkaDoPliku)
    {
        // KLUCZOWE ZABEZPIECZENIE: Czy plik nadal istnieje?
        if (!File.Exists(sciezkaDoPliku)) return;

        try
        {
            string nazwaPliku = Path.GetFileName(sciezkaDoPliku);
            
            // Dodatkowa weryfikacja nazwy (dla pewności)
            if (nazwaPliku.StartsWith(".")) return;

            // 1. Data
            DateTime dataZdjecia = GetDateTaken(sciezkaDoPliku);
            string rok = dataZdjecia.Year.ToString();
            string miesiac = dataZdjecia.ToString("MM");
            string folderDocelowy = Path.Combine(destinated_path, rok, miesiac);

            // 2. Lokalizacja i nowa nazwa
            string nowaNazwaPliku = nazwaPliku;
            var coords = GetGpsFromPhoto(sciezkaDoPliku);
            
            if (coords != null)
            {
                Console.WriteLine($"[GPS] Pobieram lokalizację dla: {nazwaPliku}...");
                string? lokalizacja = await GetLocationName(coords.Value.Lat, coords.Value.Lon);

                if (!string.IsNullOrEmpty(lokalizacja))
                {
                    lokalizacja = lokalizacja.Replace("/", "").Replace("\\", ""); // Czyścimy znaki
                    nowaNazwaPliku = $"{lokalizacja}_{nazwaPliku}";
                }
            }

            // 3. Przenoszenie
            System.IO.Directory.CreateDirectory(folderDocelowy);
            string pelnaSciezka = Path.Combine(folderDocelowy, nowaNazwaPliku);

            // Obsługa duplikatów
            if (File.Exists(pelnaSciezka))
            {
                string nazwaBezExt = Path.GetFileNameWithoutExtension(nowaNazwaPliku);
                string ext = Path.GetExtension(nowaNazwaPliku);
                pelnaSciezka = Path.Combine(folderDocelowy, $"{nazwaBezExt}_{Guid.NewGuid().ToString().Substring(0, 4)}{ext}");
            }

            // Ostatnie sprawdzenie przed ruchem
            if (File.Exists(sciezkaDoPliku))
            {
                File.Move(sciezkaDoPliku, pelnaSciezka);
                Console.WriteLine($"[SUKCES] Przeniesiono do: {rok}/{miesiac}/{Path.GetFileName(pelnaSciezka)}");
            }
        }
        catch (Exception ex)
        {
            // Ignorujemy błędy "file not found" które mogą wystąpić przy plikach systemowych
            if (!ex.Message.Contains("Could not find file"))
            {
                Console.WriteLine($"[BŁĄD] {ex.Message}");
            }
        }
    }

    static async Task Handle_zip(string sciezkaZip)
    {
        if (!File.Exists(sciezkaZip)) return;

        Console.WriteLine($"[ZIP] Rozpakowuję: {Path.GetFileName(sciezkaZip)}...");
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
                    await Handle_photo(plik);
                    Thread.Sleep(1100); 
                }
            }
        }
        catch (Exception ex) { Console.WriteLine($"[BŁĄD ZIP] {ex.Message}"); }
        finally
        {
            if (System.IO.Directory.Exists(tempFolder)) 
                System.IO.Directory.Delete(tempFolder, true);
        }
    }

    static async void OnCreate(object sender, FileSystemEventArgs e)
    {
        string nazwa = e.Name;
        string sciezka = e.FullPath;

        // --- FILTER ANTY-MAC ---
        // Ignorujemy pliki zaczynające się od kropki (np. .DS_Store, ._IMG.jpg)
        if (string.IsNullOrEmpty(nazwa) || nazwa.StartsWith(".")) return;
        
        // Ignorujemy folder docelowy
        if (sciezka.Contains("SortedPhotos")) return;

        // Czekamy chwilę na ustabilizowanie się pliku
        Thread.Sleep(1000); 

        // Ponowne sprawdzenie czy plik istnieje (bo mógł zniknąć, jeśli był tymczasowy)
        if (!File.Exists(sciezka)) return;

        string ext = Path.GetExtension(sciezka).ToLower();
        if (ext == ".zip") await Handle_zip(sciezka);
        else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".heic") await Handle_photo(sciezka);
    }

    public static void Watch(string sciezka)
    {
        if (!System.IO.Directory.Exists(sciezka)) { Console.WriteLine("Brak folderu!"); return; }

        using var watcher = new FileSystemWatcher(sciezka);
        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
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