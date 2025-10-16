class Osoba
{
    public string imie;
    public string nazwisko;    
}

class Wyklad
{
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
        goat.imie = "Josue";
        goat.nazwisko = "Pesqueira";
        Osoba podroba = goat;
        podroba.imie = "Pesqueira";
        Console.WriteLine(goat.imie);
        Console.WriteLine(goat.nazwisko);
        
        
    }
}
