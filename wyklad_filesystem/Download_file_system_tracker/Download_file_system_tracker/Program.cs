// bierzemy nowe pliki z downloads o je grupujemy , pliki .txt odczytujemy zawartocs i costam sobie porobimy 
class Program
{
    public static string destinated_path = "/Users/krzys/Desktop/Grouped/";
    static void Handle_pdfs(string path)
    {
        string pdfs = Path.Combine(destinated_path, "pdfs");
        File.Move(path,pdfs);
    }
    static void Handle_moving(string path)
    {
        string moving = Path.Combine(destinated_path, "moving");
        File.Move(path,moving);
    }
    static void Handle_code(string path)
    {
        string code = Path.Combine(destinated_path, "code");
        File.Move(path,code);
    }
    static void Handle_other(string path)
    {
        string other = Path.Combine(destinated_path, "other");
        File.Move(path,other);
    }

    static void Handle_txt(string path)
    {
        string txt = Path.Combine(destinated_path, "txt");
        FileStream fs = new FileStream(path,FileMode.OpenOrCreate,FileAccess.ReadWrite);
        
        
    }
    static void OnCreate(object sender, FileSystemEventArgs e)
    {
        switch (Path.GetExtension(e.FullPath).ToLower())
        {
            case ".pdf" : 
                Handle_pdfs(e.FullPath);
                break;
            case ".txt" :
                Handle_txts(e.FullPath);
                break;
            case ".mp3":
            case ".mp4":
                Handle_moving(e.FullPath);
                break;
            case ".c" :
            case ".cpp" :
            case ".cs" :
            case ".py" :
            case ".js" :
                Handle_code(e.FullPath);
                break;
            default:
                Handle_other(e.FullPath);
                break;
        }
    }
    public static void Watch(string from_path)
    {
        using var watcher = new FileSystemWatcher(from_path,"*");
        watcher.NotifyFilter = NotifyFilters.FileName;  // sledzimy created , deleted , renamed
        watcher.Created += OnCreate;
        
    }
    static void Main(string[] args)
    {
        string download_path = "/Users/krzys/Downloads/";
        Watch(download_path);
    }
}