using System.Collections.Concurrent;

namespace PizzeriaSimulation;

/// <summary>
/// Rekord reprezentujący zamówienie pizzy.
/// </summary>
public sealed record PizzaOrder(string Name, int Size, string Toppings, decimal Price)
{
    public override string ToString()
    {
       return $"{Name} (rozmiar {Size}) z {Toppings} [{Price:C}]";
    }
}

public sealed class Pizzeria : IDisposable
{
    // --- POLA KONFIGURACYJNE ---
    public int ChefsCount { get; }
    public int DeliverersCount { get; }
    public int PizzaQueueCapacity { get; }

    // --- KLUCZOWE STRUKTURY DANYCH ---
    
    // Nasza własna kolejka (Etap 1 i 2)
    private ParallelQueue<PizzaOrder> _queue;
    
    // Słownik do zliczania utargu (Etap 4) - bezpieczny wątkowo
    private static ConcurrentDictionary<string, decimal> _dailyIncomeDictionary = new();
    
    // Bariera do synchronizacji zamknięcia lokalu (Etap 4)
    private Barrier _barrier;

    // Licznik dostarczonych pizz w dniu dzisiejszym (do limitu dziennego)
    private int _deliveredCount = 0; 
    private const int _dailyPizzaOrders = 10; // Przykładowy limit dzienny

    // Dane do generowania losowych pizz
    private static readonly string[] _pizzaNames = new[] { "Margherita", "Pepperoni", "Hawajska", "Wegetariańska", "Kurczak BBQ" };
    private static readonly string[] _pizzaToppings = new[] { "Ser", "Oliwki", "Pieczarki", "Cebula", "Bekon", "Szpinak" };

    public Pizzeria(int chefsCount, int deliverersCount, int pizzaQueueCapacity)
    {
       ChefsCount = chefsCount;
       DeliverersCount = deliverersCount;
       PizzaQueueCapacity = pizzaQueueCapacity;

       // Inicjalizacja naszej kolejki
       _queue = new ParallelQueue<PizzaOrder>(capacity: pizzaQueueCapacity);

       // ETAP 4: Inicjalizacja Bariery
       // Bariera musi poczekać na: Wszystkich Kucharzy + Wszystkich Dostawców + 1 Główny Wątek (Symulację)
       // Kiedy naciśniemy 'Q', wszyscy pracownicy skończą pętle i spotkają się na tej barierce.
       _barrier = new Barrier(participantCount: chefsCount + deliverersCount + 1);
    }

    public void DisplayControls()
    {
       Console.Title = "Symulacja Pizzerii";
       Console.WriteLine($"=> Liczba kucharzy: {ChefsCount}");
       Console.WriteLine($"=> Liczba dostawców: {DeliverersCount}");
       Console.WriteLine($"=> Pojemność półki: {PizzaQueueCapacity}");
       Console.WriteLine($"=> Cel dzienny: {_dailyPizzaOrders} pizz");
       Console.WriteLine();
       Console.WriteLine("Sterowanie:");
       Console.WriteLine("  p - Pauzuj kuchnię (Kucharzy)");
       Console.WriteLine("  r - Wznów kuchnię");
       Console.WriteLine("  k - Pauzuj dostawy (Dostawców)");
       Console.WriteLine("  l - Wznów dostawy");
       Console.WriteLine("  b - Zakończ dzień (czekaj na barierce)");
       Console.WriteLine("  q - Wyjście (Anuluj i zamknij)");
       Console.Out.Flush();
    }

    public void Dispose()
    {
       _queue?.Dispose();
       _barrier?.Dispose();
    }

    public async Task StartSimulationAsync()
    {
       var cancellationTokenSource = new CancellationTokenSource();
       var cancellationToken = cancellationTokenSource.Token;

       // Uruchamiamy pracowników (Etap 3)
       var chefs = StartChefs(cancellationToken);
       var drivers = StartDeliverers(cancellationToken);

       // Wątek sterujący (obsługa klawiatury)
       var controlTask = Task.Run(() =>
       {
          while (!cancellationToken.IsCancellationRequested)
          {
             // Console.ReadKey blokuje wątek, ale w Task.Run to nie problem
             if (!Console.KeyAvailable) 
             {
                 Thread.Sleep(100); 
                 continue; 
             }
             
             var input = Console.ReadKey(intercept: true).Key;

             switch (input)
             {
                case ConsoleKey.P:
                   _queue.PauseEnqueue();
                   Console.WriteLine("=> [PAUZA] Kucharze przestają piec.");
                   break;
                case ConsoleKey.R:
                   _queue.ResumeEnqueue();
                   Console.WriteLine("=> [START] Kucharze wracają do pracy.");
                   break;
                case ConsoleKey.K:
                   _queue.PauseDequeue();
                   Console.WriteLine("=> [PAUZA] Dostawcy mają przerwę.");
                   break;
                case ConsoleKey.L:
                   _queue.ResumeDequeue();
                   Console.WriteLine("=> [START] Dostawcy wracają na skutery.");
                   break;
                
                case ConsoleKey.B: // Opcja: Czekaj aż pracownicy skończą (jeśli anulowano token)
                   // Sprawdzamy czy został tylko 1 uczestnik (my sami), co oznaczałoby, że reszta już czeka
                   if (_barrier.ParticipantsRemaining == 1)
                   {
                      Console.WriteLine("Czekamy aż wszyscy skończą...");
                      _barrier.SignalAndWait(cancellationToken); 
                   }
                   else
                   {
                       Console.WriteLine("Pracownicy jeszcze pracują. Użyj 'Q' aby zakończyć.");
                   }
                   break;

                case ConsoleKey.Q:
                   Console.WriteLine("Zamykamy pizzerię! Anulowanie tokena...");
                   cancellationTokenSource.Cancel(); // To wywoła wyjątek OperationCanceledException u pracowników
                   
                   // ETAP 4: Czekamy na barierce jako "Szef"
                   // Pracownicy w blokach 'finally' też dojdą do barierki.
                   Console.WriteLine("Szef czeka przy wyjściu na pracowników...");
                   try 
                   {
                       _barrier.SignalAndWait(); // Czekamy aż wszyscy się zameldują
                   }
                   catch(Exception ex) { Console.WriteLine("Błąd bariery: " + ex.Message); }
                   
                   break;
             }
          }
       });

       // Czekamy na zakończenie wątku sterującego
       await controlTask;
       
       // Czekamy aż wszystkie Taski pracowników bezpiecznie wygasną
       await Task.WhenAll(chefs);
       await Task.WhenAll(drivers);

       // Podsumowanie finansowe (Etap 4)
       Console.WriteLine("\n=== PODSUMOWANIE DNIA ===");
       decimal totalIncome = 0;
       foreach (var entry in _dailyIncomeDictionary)
       {
           Console.WriteLine($"Pizza: {entry.Key} | Zarobek: {entry.Value:C}");
           totalIncome += entry.Value;
       }
       Console.WriteLine($"RAZEM UTARG: {totalIncome:C}");
       Console.WriteLine("Pizzeria zamknięta. Do jutra!");
    }

    // --- METODY PRACOWNIKÓW (ETAP 3 + 4) ---

    private Task[] StartDeliverers(CancellationToken cancellationToken)
    {
       Task[] tasks = new Task[DeliverersCount];

       for (int i = 0; i < DeliverersCount; i++)
       {
          int delivererId = i;

          // Używamy CancellationToken.None w Task.Run, żeby Task wystartował 
          // i mógł wejść do bloku try/catch/finally, gdzie obsłużymy anulowanie.
          tasks[i] = Task.Run(async () =>
          {
             try
             {
                 while (!cancellationToken.IsCancellationRequested)
                 {
                    // A. Próba pobrania pizzy (timeout 1s)
                    var pizza = await _queue.TryDequeueAsync(1000, cancellationToken);

                    if (pizza != null)
                    {
                       // B. Symulacja dostawy (0.5 - 1s)
                       await Task.Delay(Random.Shared.Next(500, 1000), cancellationToken);
                       
                       // C. Logika bezpiecznego licznika (Interlocked)
                       int currentCount = Interlocked.Increment(ref _deliveredCount);

                       // D. Zliczanie kasy (ConcurrentDictionary - Etap 4)
                       _dailyIncomeDictionary.AddOrUpdate(
                          key: pizza.Name,
                          addValue: pizza.Price, // Pierwsza taka pizza
                          updateValueFactory: (key, currentTotal) => currentTotal + pizza.Price // Kolejna taka pizza
                       );

                       Console.WriteLine($"[Dostawca {delivererId}] Dostarczył: {pizza.Name}. (Cel: {currentCount}/{_dailyPizzaOrders})");

                       // Sprawdzenie celu dziennego
                       if (currentCount >= _dailyPizzaOrders)
                       {
                           Console.WriteLine("\n!!! PLAN WYKONANY! MOŻESZ ZAMKNĄĆ KLASĘ ('Q') !!!\n");
                       }
                    }
                    // Jeśli null (timeout), pętla leci dalej
                 }
             }
             catch (OperationCanceledException)
             {
                 // Normalne wyjście z pętli przy zamykaniu
             }
             finally
             {
                 // ETAP 4: SYGNALIZACJA NA BARIERZE
                 // Niezależnie czy był błąd, czy cancel - meldujemy, że kończymy.
                 _barrier.SignalAndWait();
                 Console.WriteLine($"[Dostawca {delivererId}] Fajrant.");
             }
          }, CancellationToken.None);
       }

       return tasks;
    }

    private Task[] StartChefs(CancellationToken cancellationToken)
    {
       Task[] tasks = new Task[ChefsCount];

       for (int i = 0; i < ChefsCount; i++)
       {
          int chefId = i;

          tasks[i] = Task.Run(async () =>
          {
             try
             {
                 while (!cancellationToken.IsCancellationRequested)
                 {
                    // A. Zrób pizzę
                    var pizza = GeneratePizzaOrder();

                    // B. Symulacja pieczenia (0.5 - 1s)
                    await Task.Delay(Random.Shared.Next(500, 1000), cancellationToken);

                    // C. Próba włożenia na półkę (timeout 1s)
                    bool added = await _queue.TryEnqueueAsync(pizza, 1000, cancellationToken);

                    if (added)
                    {
                       Console.WriteLine($"[Kucharz {chefId}] Upiekł: {pizza.Name}");
                    }
                    else
                    {
                       Console.WriteLine($"[Kucharz {chefId}] Półka pełna! Pizza {pizza.Name} zmarnowana.");
                    }
                 }
             }
             catch (OperationCanceledException)
             {
                 // Normalne wyjście
             }
             finally
             {
                 // ETAP 4: SYGNALIZACJA NA BARIERZE
                 _barrier.SignalAndWait();
                 Console.WriteLine($"[Kucharz {chefId}] Fajrant.");
             }
          }, CancellationToken.None);
       }

       return tasks;
    }

    private static PizzaOrder GeneratePizzaOrder()
    {
       var pizza = new PizzaOrder(
          _pizzaNames[Random.Shared.Next(_pizzaNames.Length)],
          Random.Shared.Next(8, 16), // Rozmiar
          _pizzaToppings[Random.Shared.Next(_pizzaToppings.Length)],
          (decimal)(Random.Shared.NextDouble() * 10) + 20 // Cena 20-30 zł
       );

       return pizza;
    }
}