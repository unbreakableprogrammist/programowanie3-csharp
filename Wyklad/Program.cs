using System.Runtime.CompilerServices;
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
    
    /*
     Klasy : 
     public - inne pliki kody ja widza
     internal / nic sie nie dadaje - widoczna w pliku 
     static -  nie mozna robic z niej obiektow (dziala jak funckcja) 
     sealed -  nie da sie dziedziczyc
     abstract-> po niej tylko sie dziedziczy
     
     class Book
       {
           private string _title;
           private int _year;
           Book(string title) => _title = title;  // jesli dostaniemy sam tytul to przypisze ttile do zmiennej _title
           Book(string title, int year) : this(title)  // this(title) oznacza ze najpierw przeniesie nas do przypisania Book(string title) => _title=title, a dopiero pozniej zrobi rok
           {
               _year = year;
           }
       }
       
       W C# słowo this ma dwa różne znaczenia, zależnie od tego, gdzie je użyjesz:
       1️⃣ w środku metody lub konstruktora → oznacza „ten obiekt”,
       2️⃣ po dwukropku w konstruktorze (: this(...)) → oznacza „inny konstruktor tej samej klasy”.
     
        Dekonstruktor : 
        class Point
       {
           public float x, y;
       
           public Point(float x, float y)  // konstruktor ( przypsianie zmiennych)
           {
               this.x = x;
               this.y = y;
           }
       
           public void Deconstruct(out float x, out float y) // dekonstruktor -> zwracanie zmiennych z klasy
           {
               x = this.x;
               y = this.y;
           }
       }
     
     
     Inicjalizatory :
     class Hamster
       {
           public string Name;
           public bool LikesViolence;
       
           public Hamster() { }
           public Hamster(string name) => Name = name;
       }
       
       // --- tworzenie obiektów ---
       Hamster h1 = new Hamster { Name = "Boo", LikesViolence = true };
       Hamster h2 = new Hamster("Boo") { LikesViolence = true };
       
       ostatnie dwie linijkit to inicjalizatory ( dzialaja ostantie po konstruktorach ) 
       pierwszy inicjalizator inicjalizuje pusty konstruktor a nastepnie sam inicjalizuje 
       drugi podobnie tylko inny konstruktor
       
       get i set -> pozwalaja dostac sie do zmiennych prywatnych
       public string Name { get; set; } 
       dziala tak : 
       private string _name;
       public string Name
       {
           get { return _name; }
           set { _name = value; }
       }
       
       
     */
    //indeksator : 
    class Sentence
    {
        private string[] Words { get; }  // prywatna właściwość przechowująca tablicę słów
        // (zwraca całą tablicę, ale tylko wewnątrz klasy)

        public string this[int i]        // indeksator — pozwala odczytywać i zmieniać
            // pojedyncze słowa przez indeks (np. s[0], s[1])
        {
            get => Words[i];
            set { Words[i] = value; }
        }

        public Sentence(string sentence) // konstruktor — dostaje całe zdanie
            => Words = sentence.Split(' '); // rozbija je na tablicę słów i zapisuje do Words
    }
    
    // dziedziczeni dziala tak jak w cpp : 
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    public class Student : Person
    {
        public string StudentID { get; set; }
    }
    
    /*
      Operator as próbuje rzutować (czyli „przekształcić”) obiekt jednego typu na inny —
       np. z klasy bazowej (Person) na klasę pochodną (Teacher).
       
       Jeśli się uda → zwraca obiekt nowego typu,
       jeśli się nie uda → zwraca null (zamiast rzucać wyjątek).
        
        is :
        Generalnie operator is sprawdza, czy zmienna pasuje do wzorca i zwraca wynik w postaci boola. 
        Jednym ze wzorców który nas interesuje jest wzorzec typu.
     
        
     */
    
    // ------------------------------
    // KLASA BAZOWA: Vehicle (pojazd)
    // ------------------------------

    public class Vehicle
    {
        // Właściwość Position — aktualna pozycja pojazdu (np. na drodze).
        // get -> każdy może odczytać pozycję.
        // protected set -> tylko ta klasa i klasy dziedziczące mogą ją zmieniać.
        // = 0 -> początkowa wartość (pojazd startuje z pozycji 0).
        public float Position { get; protected set; } = 0;

        // Właściwość Speed — aktualna prędkość pojazdu.
        // virtual -> można ją nadpisać w klasie pochodnej.
        // protected set -> zmieniać mogą tylko Vehicle i klasy dziedziczące.
        // = 1.0 -> domyślna prędkość 1 jednostka/s.
        public virtual float Speed { get; protected set; } = 1.0f;

        // Właściwość Name — nazwa pojazdu.
        // get bez set -> wartość ustawiana tylko w konstruktorze.
        public string Name { get; }

        // Konstruktor Vehicle — przyjmuje nazwę pojazdu.
        // Strzałka => to tzw. expression-bodied constructor (krótki zapis).
        public Vehicle(string name) => Name = name;

        // Wirtualna metoda Run — symuluje ruch pojazdu przez czas dt (delta time).
        // virtual -> może być nadpisana w klasach pochodnych (np. Car, Bike).
        public virtual float Run(float dt)
        {
            // Wypisuje komunikat, żeby było widać, że wywołano Vehicle.Run().
            Console.WriteLine($"Vehicle.Run({dt})");

            // Aktualizuje pozycję: nowa pozycja = stara pozycja + czas * prędkość.
            // Zwraca zaktualizowaną pozycję.
            return (Position = Position + dt * Speed);
        }
    }
    // ------------------------------
    // KLASA POCHODNA: Car (samochód)
    // ------------------------------

    public class Car : Vehicle
    {
        // Nadpisujemy właściwość Speed (prędkość) z klasy bazowej.
        // override -> oznacza, że to wersja "samochodowa".
        // protected set -> tylko Car (lub klasy dziedziczące po Car) mogą ją zmieniać.
        // = 0.0f -> samochód startuje od zera (stoi).
        public override float Speed { get; protected set; } = 0.0f;

        // Właściwość Acceleration (przyspieszenie) — jak szybko samochód przyspiesza.
        // virtual -> można ją nadpisać w klasie pochodnej (np. ElectricCar).
        // get tylko do odczytu — ustalana w konstruktorze.
        public virtual float Acceleration { get; }

        // Konstruktor Car — przekazuje nazwę do klasy bazowej Vehicle
        // i ustawia wartość przyspieszenia.
        // base(name) -> wywołanie konstruktora Vehicle(name)
        public Car(string name, float acceleration) : base(name) => Acceleration = acceleration;

        // Nadpisujemy metodę Run z klasy Vehicle.
        // Ta wersja dodaje przyspieszenie (czyli zmienia prędkość w czasie).
        public override float Run(float dt)
        {
            // Dla testów wypisuje, że wywołano Car.Run().
            Console.WriteLine($"Car.Run({dt})");

            // Aktualizuje pozycję na podstawie prędkości.
            Position += dt * Speed;

            // Następnie zwiększa prędkość zgodnie z przyspieszeniem.
            Speed += dt * Acceleration;

            // Zwraca nową pozycję po aktualizacji.
            return Position;
        }
    }
    // ------------------------------
    // KLASA POCHODNA: Bike (rower)
    // ------------------------------

    public class Bike : Vehicle
    {
        // Konstruktor Bike — wywołuje konstruktor Vehicle.
        public Bike(string name) : base(name) {}

        // Nadpisanie metody Run.
        // W tej wersji rower nie ma własnej logiki ruchu,
        // ale wypisuje komunikat i korzysta z implementacji klasy bazowej.
        public override float Run(float dt)
        {
            // Wypisanie informacji o wywołaniu Bike.Run().
            Console.WriteLine($"Bike.Run({dt})");

            // Wywołanie metody bazowej (czyli Vehicle.Run()).
            // Dzięki temu rower zachowuje się jak "domyślny pojazd".
            return base.Run(dt);
        }
    }
    /*
      klasa abstrakcyjna to klasa po ktorej mozna tylko dziedziczyc , jak jakas zmienna jest virutalna to oznacza ze mozna ja napisac
    
     jak dziala strzalka => :
     int Square(int x)
       {
           return x * x;
       } 
       to rowna sie temu 
       int Square(int x) => x * x; 
       czyli po lewej co to za funkcja i jakie argumenty a po prawej co robi
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
        else if (args[0] == "11")
        {
            Student alice = new Student() { FirstName = "Alice", 
                LastName = "Brown", 
                Age = 25, 
                StudentID = "X-84355" };
                Console.WriteLine($"{alice.FirstName} {alice.LastName}, {alice.Age}, {alice.StudentID}");
        }
    }
}


