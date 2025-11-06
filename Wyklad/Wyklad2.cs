namespace Wyklad
{
    public class Wyklad2
    {
        /*
        Klasy :
        1. static -> takie pudleko ktore grupuje jakies rzeczy , np Match to jest klasa , i tylko mamy tam metody typu Math.Sqrt , nie da sie tworzyc obiektu , bo ta klasa jest po prostu juz takim obiektem od "zapytan"
        2. abstract -> klasa ktora nie da sie stworzyc obiektu, moze zawierac metody abstrakcyjne , sluzy do dziedziczenia
        3. sealed -> nie mozna po niej dziedziczyc

        Metody
        1. static - > nie korzysta z danych obiektu , moze korzystac tylko z danych przekazanych albo static variables w klasie , nie moze edytowac poprzez this ( inne metody moga)
        2. abstract - > nie ma implementacji , trzeba ja nadpisac w klasie dziedziczacej
        3. virtual -> ma cialo ale moze byc nadpisana przez klase pochodna
        4. ovverride -> nadpisuje metode abstract / virtual
        5. sealed override -> nadpisuje metode ale dalej juz nie da sie nadpisac
         */

        //teraz interfejsy
        public interface DajGlos
        {
            void glos();
            string imie { get; set; }
        }

        public class pies : DajGlos
        {
            public string imie { get; set; }

            public void glos()
            {
                Console.WriteLine($"{imie} : Daj rybkę");
            }
        }

        public interface IFoo1
        {
            void Bar();
        }

        public interface IFoo2
        {
            void Bar();
        }

        public class Foo : IFoo1, IFoo2
        {
            public void Bar()
            {
                Console.WriteLine("Implementation of IFoo1.Bar");
            }

            void IFoo2.Bar()
            {
                Console.WriteLine("Implementation of IFoo2.Bar");
            }
        }
        // typ wyliczeniowy
        enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        public static int x = 0;
        Direction dir = (Direction)Wyklad2.x;
        public void Test_Enuma()
        {
            if (dir == Direction.Up)
            {
                Console.WriteLine("Idziemy w gore!");
            }
        }
        
         /*
           public enum Topping
            {
                Pepperoni,   // 0
                Sausage,     // 1
                Mushrooms,   // 2
                Cheese,      // 3
                Onions       // 4
            }
            
            List<Topping> lista = new List<Topping>
            {
                Topping.Cheese,
                Topping.Onions
            };
            
            Console.WriteLine((Topping)lista[0]); // wypisze cheese  
          */
         
         //typy zagniezdzone 
         public class Pizza
         {
             public int SizeCm { get; } // wlasne pole, tylko do odczytu kiedy chcemy ( bo samo get ) pozwala nam zwracac to pole w dowolnym momencie
             public IReadOnlyList<Topping> Toppings { get; } // lista tylko do odczytu , dzieki get a nie set nie da sie podmienic tej listy na inna
    
             private Pizza(Builder builder) // konstruktor pizzy ale prywatny , czyli taki mozliwy do zbudowania tylko przez build
             {
                 SizeCm = builder.SizeCm; // kopiujemy wartosci z buildera
                 Toppings = new List<Topping>(builder.Toppings); // ----||----- 
             }
    
             public enum Topping { Pepperoni, Sausage, Mushrooms, Cheese, Onions }
    
             public class Builder // wewnetrzna klasa do budowania
             {
                 public int SizeCm { get; }   
                 public List<Topping> Toppings { get; } = new List<Topping>();  // tworzymy nowe pole 
        
                 public Builder(int sizeCm) => SizeCm = sizeCm;  // konstruktor do ustawiania pole SizeCm
        
                 public Builder AddTopping(Topping topping)  // metoda do dodawania skladnikow 
                 {
                     Toppings.Add(topping);
                     return this; // zwraca nam spowrotem siebie , czyli aktualizuje tego largePepperoniBuildera 
                 }
        
                 public Pizza Build() => new Pizza(this);  // konstruktor w klasie build , bo tamten w pizza jest prywatny czyli moze byc wywolywany tylko z tej klasy a ten konstrukor jest wywolywany tylko przez build ,i od razu przekazuje siebie samego 
             }
             
             /*
              wywolanie :
              Pizza.Builder largePepperoniBuilder = new Pizza.Builder(40); // na poczatku konstrukor z gotowym sizem
                
                largePepperoniBuilder.AddTopping(Pizza.Topping.Pepperoni);  // metoda do dodawania
                largePepperoniBuilder.AddTopping(Pizza.Topping.Cheese);  
                
                Pizza largePepperoni = largePepperoniBuilder.Build();  // wywolujemy konstruktor do zbudowania obiektu pizza 
              */
             
             
             // generyki 
             /*
                	•	Kowariancja (out) pozwala traktować typ bardziej szczegółowy jako bardziej ogólny.
                Czyli pozwala przypisać: T mniejszy  →  T większy , np : np. string → object, Dog → Animal.
               
                    Kontrawariancja (in) pozwala traktować typ bardziej ogólny jako bardziej szczegółowy, ale tylko przy wchodzeniu parametrów.
                Czyli: T większy → T mniejszy np. object → string, Animal → Dog.
              */
             
             /*
               enumeratory : 
               Co oznacza dla List, Array, Dictionary …
                
                Każda kolekcja, która ma być używana w foreach, musi implementować IEnumerable<T>.
                
                Przykład → List<int>:
                
                public class List<T> : IEnumerable<T>
                {
                    public IEnumerator<T> GetEnumerator()   // ← to jest wymagane
                    {
                        return new ListEnumerator<T>(this);
                    }
                }
                
                I właśnie dlatego możesz zrobić:
                foreach (var x in lista)
                Console.WriteLine(x);
                
                IEnumerable<T> jest implementowany przez kolekcję (np. List<T>), co pozwala wywołać GetEnumerator().
                GetEnumerator() zwraca obiekt innej klasy/struktury, która implementuje IEnumerator<T>.
                To właśnie ta wewnętrzna klasa enumeratora realizuje iterację (MoveNext, Current).
                
                enumerator w foreach dziala tak : 
                public static IEnumerable<int> Fibonacci(int count)
                {
                    for (int i = 0, prev = 1, curr = 1; i < count; i++)
                    {
                        yield return prev;
                        (prev, curr) = (curr, prev + curr);
                    }
                }
                na gorze jest ten enumerator , a w praktyce to po rpostu robi ruch tylko wtedy kiedy w foreach przejdziemy do nastepnej rzeczy w pentli 
                tzn najpiew wykona 
                
                
                
              */
             
             
         }
         
         public class Stack<T> : IEnumerable<T>
         {
             private T[] _items = new T[8];
             public int Count { get; private set; }

             public void Push(T item)
             {
                 if (_items.Length == Count)
                 {
                     Array.Resize(ref _items, _items.Length * 2);
                 }
                 _items[Count++] = item;
             }

             public T Pop()
             {
                 if (Count == 0)
                 {
                     throw new InvalidOperationException("Stack is empty");
                 }
                 return _items[--Count];
             }

             public IEnumerator<T> GetEnumerator() // metoda ktora zwraca nam iEnumerator , pod current podstawia to co robi yeld 
             {
                 int count = Count;
                 while (count-- > 0)
                 {
                     yield return _items[count];
                 }
             }

             IEnumerator IEnumerable.GetEnumerator() // dla starszej wersji enumeratorow 
             {
                 return GetEnumerator();
             }
         }
    }

}