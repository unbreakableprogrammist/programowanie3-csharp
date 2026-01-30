// semaforSlim (8,8)
Oto szczegółowy plan działania krok po kroku, który musisz zaimplementować w klasie `Shop`, oparty na dostarczonym pliku `README.md`.

### 1. Zdefiniowanie pól (Prymitywy synchronizacyjne)

Musisz dodać odpowiednie obiekty do klasy `Shop`, aby obsłużyć wymagania z etapów 1, 2 i 3.

* **Pojemność sklepu (Etap 1):** Potrzebujesz semafora, który wpuszcza max 8 osób.
* **Magazyn / Towary (Etap 2):** Potrzebujesz semafora, który śledzi liczbę dostępnych "Lidlomixów". Klienci czekają na nim (Wait), pracownicy go zwalniają (Release).
* **Synchronizacja Zespołów (Etap 2):** Potrzebujesz Bariery (`Barrier`) dla każdego zespołu, aby zsynchronizować 3 pracowników. Ponieważ pracowników jest , a każdy zespół ma 3 osoby, będziesz potrzebować tablicy lub listy barier (liczba zespołów = `WorkersCount / 3`).
* **Kolejka do kasy (Etap 3):** Potrzebujesz `BlockingCollection<T>` o pojemności 5.
* *Wskazówka:* Jako `T` warto przesłać obiekt, który pozwoli kasjerowi powiadomić konkretnego klienta o zakończeniu obsługi (np. klasę pomocniczą z własnym `TaskCompletionSource` lub `SemaphoreSlim`).


* **Licznik dostaw (Etap 2):** Zmienna `int` (np. `_deliveredCount`) obsługiwana przez `Interlocked`, aby wiedzieć, kiedy zakończyć pracę pracowników (cel ).

### 2. Implementacja Konstruktora

W konstruktorze musisz zainicjalizować powyższe pola:

* **_shopCapacity:** `new SemaphoreSlim(8, 8)`.
* **_products:** `new SemaphoreSlim(0, int.MaxValue)` (zaczynamy od 0 towarów).
* **_teamsBarriers:** Stwórz `WorkersCount / 3` barier. Każda bariera powinna mieć `participantCount: 3`.
* **_checkoutQueue:** `new BlockingCollection<CustomerContext>(5)` (gdzie `CustomerContext` to twoja klasa pomocnicza do komunikacji klient-kasjer).

### 3. Implementacja Logiki Klienta (`CustomerTask`)

To zadanie łączy wszystkie etapy. Stwórz metodę prywatną (np. `RunCustomerAsync(int id)`), która wykonuje:

1. **Wejście (Etap 1):** Czekaj na semaforze pojemności sklepu (`WaitAsync(Token)`).
* Wypisz: `Entered the shop`.


2. **Symulacja zakupów:** `Task.Delay(200, Token)`.
3. **Pobranie towaru (Etap 2):** Czekaj na semaforze produktów (`WaitAsync(Token)`).
* Gdy się uda: Wypisz `Picked up Lidlomix`.


4. **Kolejka do kasy (Etap 3):**
* Stwórz mechanizm powiadamiania (np. lokalny `SemaphoreSlim(0,1)` lub `TaskCompletionSource`).
* Dodaj siebie do `_checkoutQueue.Add(...)`.


5. **Płatność:** Czekaj na swój lokalny mechanizm powiadamiania (to oznacza, że kasjer skończył).
6. **Wyjście:**
* Wypisz `Paid and leaving`.
* Zwolnij semafor pojemności sklepu (`Release()`).



### 4. Implementacja Logiki Pracownika (`WorkerTask`)

Stwórz metodę (np. `RunWorkerAsync(int id, int teamId, Barrier teamBarrier)`):

1. **Pętla pracy:** Pętla `while`, która sprawdza, czy globalny licznik dostaw < `M` (CustomersCount) ORAZ czy nie ma anulowania (`!Token.IsCancellationRequested`).
2. **Synchronizacja (Etap 2):** Wywołaj `teamBarrier.SignalAndWait(Token)`.
* *Ważne:* Bariera zrzuci wyjątek przy anulowaniu, obsłuż go (`OperationCanceledException`).


3. **Dostawa (wykonuje tylko jeden pracownik z trójki lub wszyscy po barierze, ale logikę dostawy lepiej ograniczyć):**
* Aby uniknąć potrójnego dodawania, można użyć mechanizmu `Barrier` "post-phase action" (definiowanego w konstruktorze bariery) LUB umówić się, że tylko "Lider" zespołu (np. `id % 3 == 0`) wykonuje logikę dostawy.
* Wypisz: `TEAM <TEAM_ID>: Delivering stock`.
* Czekaj: `Task.Delay(400, Token)`.
* Zwiększ stan magazynu: `_products.Release()`.
* Zwiększ licznik dostaw: `Interlocked.Increment(ref _deliveredCount)`.



### 5. Implementacja Logiki Kasjera (`CashierTask`)

Stwórz metodę (np. `RunCashierAsync(int id)`):

1. **Pętla obsługi:** Pętla `while(!Token.IsCancellationRequested)`.
2. **Pobranie klienta (Etap 3):**
* Pobierz z `_checkoutQueue.Take(Token)`. Użyj bloku `try-catch` na `OperationCanceledException`, aby wyjść z pętli przy zamykaniu.


3. **Obsługa:**
* Wypisz: `CASHIER <CashID>: Serving Customer <CustID>`.
* Czekaj: `Task.Delay(300, Token)`.


4. **Koniec obsługi:**
* Zwolnij blokadę klienta (jego prywatny semafor/TCS), aby mógł wyjść ze sklepu.



### 6. Metoda `StartSimulationAsync`

Tutaj spinasz wszystko razem:

1. Stwórz listę `List<Task> allTasks`.
2. **Uruchom Kasjerów:** W pętli `for (0..K)` uruchom `RunCashierAsync`.
3. **Uruchom Pracowników:** W pętli `for (0..N)`.
* Oblicz `teamId = i / 3`.
* Przekaż odpowiednią barierę z listy `_teamsBarriers`.
* Uruchom `RunWorkerAsync`.


4. **Uruchom Klientów:** W pętli `for (0..M)` uruchom `RunCustomerAsync`.
5. **Oczekiwanie:**
* Zasadniczo czekasz na zakończenie wszystkich zadań (`Task.WhenAll`).
* *Uwaga:* Kasjerzy i Pracownicy pracują w pętlach nieskończonych (lub do warunku). Klienci są zadaniami skończonymi.
* Gdy wszyscy klienci skończą (`Task.WhenAll(customerTasks)`), możesz anulować resztę przez `CancellationTokenSource` (jeśli masz kontrolę nad źródłem tokena) lub po prostu zakończyć metodę, wiedząc, że `Dispose` posprząta. W tym zadaniu `Token` przychodzi z zewnątrz (obsługa SIGINT), więc głównym oczekiwaniem jest `await Task.WhenAll(customerTasks)` lub oczekiwanie na anulowanie tokena.



### 7. Metoda `Dispose`

To jest kluczowe dla punktu [2p] (sprzątanie zasobów).

* `_shopCapacity.Dispose()`
* `_products.Dispose()`
* `_checkoutQueue.Dispose()`
* Pętla po barierach: `barrier.Dispose()`

---

### Przykład struktury pomocniczej (dla kolejki)

Aby zrealizować Etap 3 (kasjer budzi klienta), sugeruję taką klasę wewnątrz `Shop` lub obok:

```csharp
private class CustomerInQueue
{
    public int CustomerId { get; }
    // Semafor, na którym klient czeka, a kasjer go zwalnia
    public SemaphoreSlim PaymentSignal { get; } = new SemaphoreSlim(0, 1);

    public CustomerInQueue(int id)
    {
        CustomerId = id;
    }
}

```

Dzięki temu w `_checkoutQueue` przechowujesz obiekty `CustomerInQueue`.


using System.Collections.Concurrent;

namespace SyncLab;

public class Shop : IDisposable
{
    public int CustomersCount { get; }
    public int WorkersCount { get; }
    public int CashiersCount { get; }
    public CancellationToken Token { get; }

    // --- PRYMITYWY SYNCHRONIZACYJNE ---

    // Etap 1: Limit miejsc w sklepie (max 8)
    private readonly SemaphoreSlim _shopCapacity;

    // Etap 2: Dostępność towaru (Lidlomixów)
    // Zaczynamy od 0. Klienci czekają (Wait), Pracownicy dodają (Release).
    private readonly SemaphoreSlim _products;

    // Etap 2: Bariery dla zespołów pracowników
    // Lista barier, bo mamy kilka zespołów. Każda bariera dla 3 pracowników.
    private readonly List<Barrier> _teamBarriers;
    
    // Etap 2: Licznik dostarczonych palet (aby wiedzieć, kiedy przestać pracować)
    private int _deliveredCount = 0;

    // Etap 3: Kolejka do kasy
    // BlockingCollection idealnie pasuje do modelu Producent-Konsument z limitem bufora.
    private readonly BlockingCollection<CustomerContext> _checkoutQueue;

    public Shop(int customers, int workers, int cashiers, CancellationToken token = default)
    {
        CustomersCount = customers;
        WorkersCount = workers;
        CashiersCount = cashiers;
        Token = token;

        // Inicjalizacja Etap 1
        _shopCapacity = new SemaphoreSlim(8, 8);

        // Inicjalizacja Etap 2
        _products = new SemaphoreSlim(0, int.MaxValue);
        _teamBarriers = new List<Barrier>();
        
        int numberOfTeams = WorkersCount / 3;
        for (int i = 0; i < numberOfTeams; i++)
        {
            // Bariera dla 3 uczestników
            _teamBarriers.Add(new Barrier(3));
        }

        // Inicjalizacja Etap 3 (pojemność kolejki 5)
        _checkoutQueue = new BlockingCollection<CustomerContext>(5);
    }

    public async Task StartSimulationAsync()
    {
        var allTasks = new List<Task>();

        // 1. Uruchomienie Kasjerów
        for (int i = 0; i < CashiersCount; i++)
        {
            int cashierId = i;
            allTasks.Add(Task.Run(() => RunCashierAsync(cashierId), Token));
        }

        // 2. Uruchomienie Pracowników (w zespołach)
        for (int i = 0; i < WorkersCount; i++)
        {
            int workerId = i;
            int teamId = i / 3; // 0, 1, 2 idą do zespołu 0, itd.
            var barrier = _teamBarriers[teamId];
            
            allTasks.Add(Task.Run(() => RunWorkerAsync(workerId, teamId, barrier), Token));
        }

        // 3. Uruchomienie Klientów
        // Zbieramy zadania klientów osobno, aby wiedzieć, kiedy skończy się symulacja zakupów
        var customerTasks = new List<Task>();
        for (int i = 0; i < CustomersCount; i++)
        {
            int customerId = i;
            customerTasks.Add(Task.Run(() => RunCustomerAsync(customerId), Token));
        }

        // Dołączamy klientów do wszystkich zadań (dla porządku), ale czekamy głównie na nich
        allTasks.AddRange(customerTasks);

        try
        {
            // Czekamy aż wszyscy klienci zrobią zakupy i wyjdą
            await Task.WhenAll(customerTasks);
        }
        catch (OperationCanceledException)
        {
            // Ignorujemy anulowanie głównego oczekiwania
        }

        // Opcjonalnie: Skoro klienci wyszli, można by tu anulować resztę zadań,
        // ale w tym zadaniu anulowanie przychodzi z zewnątrz (SIGINT) lub program się kończy.
    }

    // --- LOGIKA KLIENTA ---
    private async Task RunCustomerAsync(int id)
    {
        try
        {
            // Etap 1: Wejście do sklepu
            await _shopCapacity.WaitAsync(Token);
            Console.WriteLine($"CUSTOMER {id}: Entered the shop");
            await Task.Delay(200, Token);

            // Etap 2: Pobranie towaru
            // Klient czeka na semaforze produktów. Jeśli stan = 0, zablokuje się tutaj.
            await _products.WaitAsync(Token);
            Console.WriteLine($"CUSTOMER {id}: Picked up Lidlomix");

            // Etap 3: Kolejka do kasy
            // Tworzymy kontekst (bilet), żeby kasjer wiedział kogo obsłużył
            using var myPaymentSignal = new SemaphoreSlim(0, 1);
            var context = new CustomerContext(id, myPaymentSignal);

            // Dodajemy się do kolejki (to zablokuje wątek, jeśli kolejka ma już 5 osób)
            _checkoutQueue.Add(context, Token);

            // Czekamy, aż kasjer da sygnał na naszym prywatnym semaforze
            await myPaymentSignal.WaitAsync(Token);

            Console.WriteLine($"CUSTOMER {id}: Paid and leaving");
        }
        catch (OperationCanceledException)
        {
            // Cicha obsługa przerwania
        }
        finally
        {
            // Zawsze zwalniamy miejsce w sklepie przy wyjściu (nawet przy błędzie/anulowaniu)
            // Sprawdzamy CurrentCount < InitialCount (właściwie Release rzuci wyjątek jak za dużo, 
            // ale tutaj po prostu zwalniamy to co zajęliśmy).
            try 
            {
               // Uwaga: Release rzuci SemaphoreFullException jeśli zwolnimy coś czego nie zajęliśmy,
               // w prostej logice try-finally tutaj jest ok, o ile WaitAsync przeszedł.
               // Dla uproszczenia w labach zakładamy, że jak wszedł do try, to Wait przeszedł.
               if (_shopCapacity.CurrentCount < 8) 
                   _shopCapacity.Release();
            }
            catch { }
        }
    }

    // --- LOGIKA PRACOWNIKA ---
    private async Task RunWorkerAsync(int id, int teamId, Barrier teamBarrier)
    {
        try
        {
            // Pętla: dopóki nie dostarczymy M towarów
            while (!Token.IsCancellationRequested && _deliveredCount < CustomersCount)
            {
                // Synchronizacja zespołu na barierze
                // Wszyscy 3 muszą tu dotrzeć, aby przejść dalej.
                teamBarrier.SignalAndWait(Token);

                // Tylko jeden pracownik z zespołu wykonuje logikę dostawy (np. ten podzielny przez 3 w zespole)
                // W tym zadaniu identyfikator id jest globalny.
                // Sprawdzamy czy to "lider" wewnątrz tej trójki.
                bool isLeader = (id % 3 == 0); 

                if (isLeader)
                {
                    Console.WriteLine($"TEAM {teamId}: Delivering stock");
                    await Task.Delay(400, Token);
                    
                    // Zwiększamy licznik dostaw
                    Interlocked.Increment(ref _deliveredCount);
                    
                    // Fizycznie dodajemy towar na półkę (zwalniamy semafor produktów)
                    _products.Release(); 
                }
                
                // Druga synchronizacja, żeby lider nie uciekł reszcie zespołu do następnej iteracji
                // (opcjonalne, ale zalecane dla spójności zespołu)
                teamBarrier.SignalAndWait(Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Koniec pracy
        }
        catch (Exception)
        {
            // Inne błędy (np. bariera po Dispose)
        }
    }

    // --- LOGIKA KASJERA ---
    private async Task RunCashierAsync(int id)
    {
        try
        {
            // Pobieramy klientów z kolejki dopóki nie anulowano tokena
            // GetConsumingEnumerable automatycznie obsługuje pętlę i oczekiwanie
            foreach (var customerCtx in _checkoutQueue.GetConsumingEnumerable(Token))
            {
                Console.WriteLine($"CASHIER {id}: Serving Customer {customerCtx.CustomerId}");
                await Task.Delay(300, Token);

                // Powiadamiamy konkretnego klienta, że jest obsłużony
                customerCtx.PaymentCompletedSignal.Release();
            }
        }
        catch (OperationCanceledException)
        {
            // Koniec zmiany
        }
    }

    // --- SPRZĄTANIE ---
    public void Dispose()
    {
        _shopCapacity?.Dispose();
        _products?.Dispose();
        _checkoutQueue?.Dispose();
        
        if (_teamBarriers != null)
        {
            foreach (var barrier in _teamBarriers)
            {
                barrier.Dispose();
            }
        }
    }

    // Klasa pomocnicza do przekazywania danych między Klientem a Kasjerem
    private class CustomerContext
    {
        public int CustomerId { get; }
        public SemaphoreSlim PaymentCompletedSignal { get; }

        public CustomerContext(int id, SemaphoreSlim signal)
        {
            CustomerId = id;
            PaymentCompletedSignal = signal;
        }
    }
}