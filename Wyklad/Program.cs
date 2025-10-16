using System.Runtime.CompilerServices;
using System;
using System.Text;

class Osoba
{
    public string name;
    public string nazwisko;    
}

class Wyklad
{
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
        List<int> numbers = new List<int> { 3,5,12,5,123,512,1,2,12 };
        numbers.Sort(); // sortowanie listy 
        numbers.Reverse();  // odwracanie listy
        numbers.Add(12);  // dodawnaie elementu
        foreach (var x in numbers)
            Console.WriteLine(x);
    }


    static void typy()
    {
        /* roznica miedzy typami bezposrednimi ( int char , struct) a referncyjnymi ( string , object , tablice) 
        jest taka ze objekty bezposrednie sa na stosie , wiec powiedzmy ze w ramie ,a typy refencyjne na stercie
        i teraz dla kodu :  
        int x = 10;
        Osoba osoba = new Osoba();
        osoba.Imie = "Ala";
        wyglada to tak :   
        STOS (Stack)
        -------------
        x = 10
        osoba → (adres #1234)

        STERTA (Heap)
        -------------
        #1234 : Osoba
                Imie = "Ala"
                
        wiec w czym roznica , otoz w kopiowaniu 
        
        dla bezposrednich typow : 
        int a = 5;
        int b = a;
        b = 10;
        Console.WriteLine(a); // 5
        
        a dlatypow referencyjnych : 
        Osoba a = new Osoba();
        Osoba b = a;
        b.Imie = "Jan";
        Console.WriteLine(a.Imie); // Jan
        BO -> b to jest tak naprawde skopiowany adres od osoby a, wiec kopiujemy adres na ktory wskazuje a i jak robimy b.Imie to idziemy do tej samej osoby
        */
        int a = 5;
        int b = a;
        b = 10;
        Console.WriteLine(b);
        Console.WriteLine(a);
        
        Osoba goat = new Osoba();
        goat.name = "Josue";
        goat.nazwisko = "Pesqueira";
        Osoba podroba = goat;
        podroba.name = "Pesqueira";
        Console.WriteLine(goat.name);
        Console.WriteLine(goat.nazwisko);
        /*
         przekazywanie wartosci do funkcji : 
         1. przekazwyanie typow bezposrednich ( int, char , struct) -> przekazuje sie kopie obiektu
         2. przekazywanie typow referencyjnych( string , obiekt , tablica) -> przekazuje sie kopia referencji , ale obie wskazuja na ten sam obiekt
         */
        
    }
    
    public struct Point{
        public float X;
        public float Y;
    }   
    static void ModifyValueType(Point point) // 'point' jest kopią 'p'
    {
        // Modyfikujemy tylko lokalną kopię
        point.X = 100;
        point.Y = 100;
    }

    static void typy_podstawowe()
    {
        /*
         konwersje niejawne ( nic nie trzeba dodawac : 
         calkowita -> calkowita ( int -> long itp_
         calkowita -> zmiennoprzecinkowa ( int -> float , int ->double) 
         long - > float / double , ale tu moze byc ryzyko utraty prezycji
         
         konwersje jawne dodajemy w nawiasach na co chcemy zmienic
         double/float -> int 
         double d = 99.9 ;
         int i = (int)d;
         
         dla typow referencyjnych sprawda czy referencja pokazuje na ten sam obiekt
         */
        char a = 'A';
        char newLine = '\n';
        int x = a;
        Console.WriteLine(x);
        int number = Convert.ToInt32("42");
        int rounded = Convert.ToInt32(3.5);
        bool isTrue = Convert.ToBoolean(1);
        int binaryInt = Convert.ToInt32("101010", 2);
        string hex = Convert.ToString(255, 16);
        
    }

    static void Stringi()
    {
        // stringi 
        char[] chars = new char[]{'w', 'o', 'r', 'l', 'd'};  // modyfikowalne
        string fromLiteral = "Hello";  // niemodyfikowalne
        string fromArray = new string(chars);
        string fromSubArray = new string(chars, 1, 2);
        string repeatedChar = new string(' ', 4);
        string concatenated = fromLiteral + ' ' + fromArray;
        Console.WriteLine(chars);
        Console.WriteLine("string Length: " + concatenated.Length);
        char space = concatenated[5];
        
        // stringbuilder - 
        StringBuilder stringBuilder = new StringBuilder("Hello, ");
        stringBuilder.Append("this is ");
        stringBuilder.Append("a simple ");
        stringBuilder.Append("StringBuilder demo.");
        Console.WriteLine(stringBuilder.ToString());
        
        string example = "Showcasing C# strings";
        string sub = example.Substring(11, 2);
        Console.WriteLine($"Substring: {sub}");
        bool contains = example.Contains("C#");
        Console.WriteLine($"Contains 'C#': {contains}");
        string replaced = example.Replace("Showcasing", "Demo of");
        Console.WriteLine($"Replace: {replaced}");
        string upper = example.ToUpper();
        Console.WriteLine($"Uppercase: {upper}");
        string[] words = example.Split(' ');
        Console.WriteLine("Split:");
        foreach (string word in words)
        {
            Console.WriteLine(word);
        }
        string joined = string.Join(", ", words);
        Console.WriteLine($"Join: {joined}");
        
        // jak damy przed stringiem @ to wtedy slashe nie maja znaczenia ( poza "" , wtdy musimy dodac ""_""
        // """ ___ """ - po prostu lepsze literaly 
        string path = @"C:\Users\Krzysztof\Desktop";
        string tekst = @"Linia 1
    Linia 2
        Linia 3";
        Console.WriteLine(tekst);
        Console.WriteLine(path);
        //interpolacja stringa
        string author = "George Orwell";
        string book = "Nineteen Eighty-Four";
        int year = 1949;
        decimal price = 19.50m;
        string description = $"{author} is the author of {book}. \n" +
                             $"The book price is {price:C}, it was published in {year}.";
        Console.WriteLine(description);
        // innymi slowy w {} mozna wstawic cokoliwiek co nie jest stringiem , nawet warunek : Console.WriteLine($"Coin flip: {(random.NextDouble() < 0.5 ? "heads" : "tails")}");
    }
    static void Main(string[] args)
    {
        if (args[0] == "1")
            poczatek();
        else if( args[0] == "2")
            stringi();
        else if( args[0] == "3")
            listy();
        else if( args[0] == "4")
            typy();
        else if (args[0] == "5")
        {
            Point p = new Point { X = 1, Y = 1 };
            ModifyValueType(p);
            Console.WriteLine(p.X);
            Console.WriteLine(p.Y);
        }
        else if (args[0] == "6")
            typy_podstawowe();
        else if (args[0] == "7")
            Stringi();
    }
}
