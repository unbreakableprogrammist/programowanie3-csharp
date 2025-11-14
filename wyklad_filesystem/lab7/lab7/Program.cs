using System.IO.Compression;

class Program
{
    public static void Watch(string path)
    {
        var watcher = new FileSystemWatcher(path);
        watcher.NotifyFilter = NotifyFilters.FileName;
        watcher.Created += OnCreated;
        watcher.Error += OnError;
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        void Photos(string path)
        {
            var Time = File.GetCreationTime(path);
            string new_path = Path.Combine("./sorted", Time.Year.ToString(), Time.Month.ToString());
            Directory.CreateDirectory(new_path);
            File.Move(path, Path.Combine(new_path, Path.GetFileName(path)), overwrite: true);
            Console.WriteLine("Zdjecie ${path} , zostalo przeniesione do ${new_path}");
        }

        void Archives(string archpath)
        {
            string name =  Path.GetDirectoryName(archpath);
            ZipFile.ExtractToDirectory(archpath, name,overwriteFiles:true);
            File.Delete(archpath);
        
            Console.WriteLine($"Extracted {archpath} to {name}");
        }
        void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("Error");
        }
        void OnCreated(object sender, FileSystemEventArgs e)
        {
            if(!File.Exists(e.FullPath))
                return;
            Console.WriteLine($"Created: {e.FullPath}");
            HashSet<string> extensions_for_photos = new HashSet<string>
            {
                ".jpg", ".jpeg", ".png", ".bmp", ".gif"
            };
            if (extensions_for_photos.Contains(Path.GetExtension(e.FullPath)))
            {
                Photos(e.FullPath);    
            }else if (Path.GetExtension(e.FullPath) == ".zip")
            {
                Archives(e.FullPath);
            }
        }
        Console.WriteLine("Press enter to exit...");
        Console.ReadLine();
    }
    static void Main(string[] args)
    {
        string watched_path = args[0];
        if(!Directory.Exists("./sorted"))
            Directory.CreateDirectory("./sorted");
        Watch(watched_path);
    }
}