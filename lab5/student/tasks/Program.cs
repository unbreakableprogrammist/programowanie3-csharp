using tasks.Databases;

namespace tasks;

public sealed class Program
{
    static void Main(string[] args)
    {
        var database = new SampleMovieDatabase();
        database.RunQueries();
        PressEnterToContinue();
        /*
        var snake = "email_adress";
        var pascal = snake.SnakeToPascalCase();
        */

        /*Console.WriteLine(pascal); // "html_element_id"*/
        /*foreach (var prime in PrimeFinder.SieveOfEratosthenes(11))
        {
            if (prime > 850) break;
            Console.WriteLine(prime);
        }*/
        
        
    }
    

    private  static void PressEnterToContinue()
    {
        Console.WriteLine("Press ENTER key to continue...");
        Console.ReadLine();
    }
}