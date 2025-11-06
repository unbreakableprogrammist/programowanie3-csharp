using tasks.Databases;

namespace tasks;

public sealed class Program
{
    static void Main(string[] args)
    {
        var database = new SampleMovieDatabase();
        database.RunQueries();
        PressEnterToContinue();
        var snake = "EmailAdress";
        var pascal = snake.SnakeToPascalCase();

        Console.WriteLine(pascal); // "html_element_id"
    }
    

    private  static void PressEnterToContinue()
    {
        Console.WriteLine("Press ENTER key to continue...");
        Console.ReadLine();
    }
}