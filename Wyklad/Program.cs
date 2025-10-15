using System;

class Program
{
    static void Main(string[] args)
    {
        if (args[0] == "1")
            poczatek();
        else if( args[0] == "2")
            stringi();
        
    }

    static void poczatek()
    {
        Console.WriteLine("Hello, World!"); // wypisywanie
        string name = Console.ReadLine(); // wczytywanie
        int year = DateTime.Now.Year;
        Console.WriteLine($"Hello {name}! , mamy rok : {year}");
    }
    static void stringi()
    {
        string txt = "Hello ";
        string txt2 = txt + "World";
        Console.WriteLine(txt2);
    }

    static void listy()
    {
        
    }
}
