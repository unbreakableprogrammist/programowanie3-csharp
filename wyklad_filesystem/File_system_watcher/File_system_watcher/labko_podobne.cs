using System.IO.Compression;

namespace Files;

class Program3
{
    static void Main(string[] args)
    {
        // Create dirs
        CreateDirs([
            "./watched",
            "./sorted"    
        ]);
        
        // Watch for new files in ./watched dir
        Watch("./watched");
    }

    public static void Watch(string path)
    {
        // FileSystemWatcher object for getting dir events
        var watcher = new FileSystemWatcher(path);

        watcher.NotifyFilter = NotifyFilters.FileName; // notify only if filename changed e.g. new file
        
        // We only want to get notification when new file is created
        watcher.Created += OnCreated;
        watcher.Error += OnError;
        
        // Only one extension can be filtered so we filter in delegate
        watcher.Filter = "*";
        // Watch subdirs - we create subdir for extracting archive
        watcher.IncludeSubdirectories = true;
        // Start watching:
        watcher.EnableRaisingEvents = true;
        
        Console.WriteLine("Press enter to exit...");
        Console.ReadLine();
    }

    private static void OnCreated(object source, FileSystemEventArgs e)
    {
        if (!File.Exists(Path.Combine(e.FullPath)))
        {
            return;
        }
        Console.WriteLine($"Created: {e.FullPath}");
        
        // Handle only photos and archives
        // Delete other files
        switch (Path.GetExtension(e.FullPath).ToLower())
        {
            case ".jpg":
            case ".jpeg":
            case ".png":
            case ".heic":
                HandlePhotos(e.FullPath);
                break;
            case ".zip":
                HandleArchives(e.FullPath);
                break;
            default:
                DeleteFile(e.FullPath);
                break;
        }
    }

    private static void OnError(object source, ErrorEventArgs e)
    {
        Console.WriteLine($"ERROR: {e.GetException().Message}");
    }

    private static void HandlePhotos(string photoPath)
    {
        // get photo creation time
        var creationTime = File.GetCreationTime(photoPath);
        
        // create folder for photo if doesnt exist
        string newPath = Path.Combine("sorted", $"{creationTime.Year}", $"{creationTime.Month}", $"{creationTime.Day}",
            $"{creationTime.Hour}");
        
        CreateDirs([newPath]);
        
        // move photo
        File.Move(photoPath, Path.Combine(newPath, Path.GetFileName(photoPath)), overwrite: true);
        
        Console.WriteLine($"Moved photo {photoPath} to {newPath}");
    }

    private static void HandleArchives(string archivePath)
    {
        // get extract path
        string? extractPath = Path.GetDirectoryName(archivePath);
        if (extractPath == null) extractPath = ".";
        
        // extract archive (it should be extracted to watched folder and watcher will see extracted photos)
        ZipFile.ExtractToDirectory(archivePath, extractPath, overwriteFiles: true);
        // delete archive file
        File.Delete(archivePath);
        
        Console.WriteLine($"Extracted {archivePath} to {extractPath}");
    }

    private static void DeleteFile(string filePath)
    {
        File.Delete(filePath);
        Console.WriteLine($"Deleted: {filePath}");
    }
    
    private static void CreateDirs(string[] dirs)
    {
        // check if dir exist, if not -> create
        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}