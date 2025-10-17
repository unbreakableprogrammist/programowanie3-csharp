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

    static void tablice()
    {
        // tablice sa nierozszerzalne 
        int[] primes = new int[] {2, 3, 5, 7, 11};
        char[] vowels = {'a', 'e', 'i', 'o', 'u'};
        uint[] even = [0, 2, 4, 6, 8]; // C# 12
        float[] data = new float[10]; 
        Array array = primes;  // trzyma referencje , dodatkowo jak damy array to taki jakby typ nijaki , tzn mozemy przyporzadkowac go do jakiejkolwiek tablicy ale wtedy to jest tylko do odczytu 
        primes[1] = 200;
        Console.WriteLine($"Primes array length: {array.Length}");
        for (int i = 0; i < array.Length; i++)
        {
            Console.WriteLine(primes[i]);
        }
        int firstElem = primes[0], secondElem = primes[1];
        int lastElem = primes[^1], secondToLastElem = primes[^2];
        Index first = 0;
        Index last = ^1;
        firstElem = primes[first]; lastElem = primes[last];
        // tablica [i..j] , od i wlacznie do j wylacznie, poczatek = 0 koniec = ^0 , bo ostatni element to ^1
        int[] firstTwo = primes[..2]; //exclusive end
        int[] withoutFirst = primes[1..]; // inclusive start
        int[] withoutLast = primes[..^1];
        int[] withoutFirstAndLast = primes[1..^1];
        int[] all = primes[..];
        Range lastTwoRange = ^2..;
        int[] lastTwo = primes[lastTwoRange];
        // tablice prostokatne 
        float[,] matrix = new float[3, 4]; // matrix 3 wiersze , cztery kolumny 
    }
    //olajestsuper:3

    static void instrukcje()
    {
        int x = 2;

        switch (x)
        {
            case 1:
                Console.WriteLine("Jeden");
                break;
            case 2:
                Console.WriteLine("Dwa");
                break;
            default:
                Console.WriteLine("Coś innego");
                break;
        }

        int cardNumber = 13;
        string cardName = cardNumber switch
        {
            13 => "King", // constant pattern
            12 => "Queen",
            11 => "Jack",
            > 1 and < 11 => "Pip card", // relational pattern
            1 => "Ace",
            // discard pattern, equivalent of default:
            _ => throw new ArgumentOutOfRangeException()
        };
        Console.WriteLine(cardName);
        
        // foreach = takie for auto w cpp 
        int[] array = new int[] {0, 1, 2, 3, 4};
        int[] array2 = new int[5]; // pusta tablica na 5 miejsc
        foreach (var i in array)
            Console.WriteLine(i);
        foreach (char c in "foreach")
            Console.WriteLine(c);
        int pp = default;  // inicjalizujemy pp wartoscia domyslna
        Console.WriteLine(pp); // odpowiedz 0
        var ii = 0; // var  =  auto w cpp 
    }
    static void Foo(ref int p)
    {
        p = p + 1;
        Console.WriteLine(p);
    }
    static void referencje()
    {
        // ref -> jesli przekazujemy ref zmienna -> przekazujemy referencje do niego 
        int x = 8;
        Foo(ref x);
        Console.WriteLine(x);
        // out dziala troche ja return , te zmienne ktore maja out int nazwa zostaja "zwracane"
        void GetData(out int a, out int b)
        {
            a = 10;
            b = 20;
        }

        int X, Y;
        GetData(out X, out Y);
        Console.WriteLine($"{X}, {Y}"); // 10, 20
        // dodatkowo jak nie chcemy przyjmowac czasem jakiegos argumentu robimy out int _ np Console.WriteLine(out int a, out _ )
        
        // in dziala tak ze rzecz jest tylko do odczytu 
        
        // params dziala tak ze mozemy przekazac do funckji dowolna ilosc argumentow tego samego typu np :
        void PrintNumbers(params int[] numbers)
        {
            foreach (int n in numbers)
                Console.WriteLine(n);
        }
        PrintNumbers(1, 2, 3, 4, 5);   // ✅ dowolna liczba argumentów

    }
    /*
     * przestrzenie nazw : 
    
    -namespace -> tak samo jak w c++ 
    -using nazwa_namespace -> nie trzeba pisac ze cos jest z namespace
    -using -> takie jak #include
    -using static -> nie trzeba pisac np Math.Sqrt tylko Sqrt
    -using global -> to samo co using static tylko dziala we wszyskich plikach projektu
    -alias : using Vec3 = System.Numerics.Vector3; ( po prostu skracamy)
     */
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
        else if (args[0] == "8")
            tablice();
        else if (args[0] == "9")
            instrukcje();
        else if (args[0] == "10")
            referencje();
    }
}


