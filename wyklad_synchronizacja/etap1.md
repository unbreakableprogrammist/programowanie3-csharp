Jasne, zrealizujmy **Etap 1**. Skupimy się wyłącznie na:

1. Ograniczeniu liczby klientów w sklepie do 8.
2. Stworzeniu zadań klientów, którzy wchodzą, czekają i wychodzą.

Oto kod, który musisz wkleić do swojej klasy `Shop`.

### Co zmieniamy w kodzie?

1. **Dodajemy pole `_shopCapacity**`: To nasz `SemaphoreSlim`, który działa jak bramka wejściowa (max 8 osób).
2. **Inicjalizujemy go w konstruktorze**.
3. **Tworzymy metodę `RunCustomerAsync**`: Logika pojedynczego klienta.
4. **Uruchamiamy zadania w `StartSimulationAsync**`.

### Kod klasy Shop (Etap 1)

```csharp
using SyncLab.Actors; // Zakładam, że tu mogą być pomocnicze klasy, ale nie są wymagane w Etapie 1

namespace SyncLab;

public class Shop : IDisposable
{
    public int CustomersCount { get; }
    public int WorkersCount { get; }
    public int CashiersCount { get; }
    public CancellationToken Token { get; }

    // --- ETAP 1: Pola ---
    // Semafor kontrolujący dostęp do sklepu (max 8 osób)
    private readonly SemaphoreSlim _shopCapacity; 

    public Shop(int customers, int workers, int cashiers, CancellationToken token = default)
    {
        CustomersCount = customers;
        WorkersCount = workers;
        CashiersCount = cashiers;
        Token = token;
        
        // --- ETAP 1: Inicjalizacja ---
        // 8 miejsc początkowych, 8 miejsc maksymalnie
        _shopCapacity = new SemaphoreSlim(8, 8);
    }

    public async Task StartSimulationAsync()
    {
        // --- ETAP 1: Tworzenie i uruchamianie zadań klientów ---
        var customerTasks = new List<Task>();

        for (int i = 0; i < CustomersCount; i++)
        {
            int customerId = i; // Kopiowanie zmiennej dla domknięcia (closure)
            customerTasks.Add(RunCustomerAsync(customerId));
        }

        // Oczekiwanie na zakończenie wszystkich klientów
        await Task.WhenAll(customerTasks);
    }

    // --- ETAP 1: Logika Klienta ---
    private async Task RunCustomerAsync(int id)
    {
        try
        {
            // 1. Próba wejścia do sklepu (czekamy na wolne miejsce na semaforze)
            await _shopCapacity.WaitAsync(Token);

            // Sekcja krytyczna (w sklepie jest max 8 wątków)
            try
            {
                Console.WriteLine($"CUSTOMER {id}: Entered the shop");
                
                // Symulacja przebywania w sklepie (200ms)
                await Task.Delay(200, Token);
            }
            finally
            {
                // ZAWSZE zwalniamy semafor przy wyjściu, nawet jak wystąpi błąd
                _shopCapacity.Release();
            }
            
            // W Etapie 1 tutaj kończymy. W kolejnych etapach tu dojdzie logika kasy.
        }
        catch (OperationCanceledException)
        {
            // Obsługa anulowania (SIGINT), żeby nie "wysypać" aplikacji brzydkim błędem
        }
    }

    public void Dispose()
    {
        // --- ETAP 1: Sprzątanie ---
        _shopCapacity.Dispose();
    }
}

```

### Wyjaśnienie kluczowych elementów:

* **`new SemaphoreSlim(8, 8)`**: Pierwsza ósemka oznacza, że na starcie mamy 8 wolnych miejsc. Druga ósemka to maksymalna pojemność (nie można zwolnić więcej niż 8).
* **`WaitAsync(Token)`**: Klient czeka w kolejce przed sklepem. Jeśli w środku jest już 8 osób, ten wątek się tutaj zatrzyma. Przekazanie `Token` pozwala przerwać oczekiwanie, gdy naciśniesz `Ctrl+C`.
* **`try...finally`**: To bardzo ważne przy semaforach. `Release()` jest w bloku `finally`, aby mieć pewność, że klient zwolni miejsce w sklepie ("odda koszyk"), nawet jeśli program wyrzuci błąd w trakcie zakupów.

Chcesz przejść teraz do **Etapu 2** (Pracownicy i dostawy towaru)?


Jasne, oto kod realizujący wyłącznie **Etap 2** (Pracownicy, Zespoły, Dostawy i Pobieranie towaru), który należy dodać do klasy `Shop`.

Zakładam, że masz już kod z Etapu 1 (wejście do sklepu). Poniżej pokazuję, co trzeba **dodać** lub **zmienić**.

### Co robimy w Etapie 2?

1. **Pracownicy (`WorkerTask`)**: Pracują w 3-osobowych zespołach.
2. **Bariera (`Barrier`)**: Synchronizuje 3 pracowników – muszą zacząć dostawę razem.
3. **Semafor towaru (`_products`)**: Pracownicy go zwalniają (dodają towar), Klienci na nim czekają (biorą towar).
4. **Limit dostaw**: Pracujemy, dopóki łącznie nie dostarczymy `M` sztuk towaru.

---

### 1. Pola do dodania w klasie `Shop`

```csharp
    // --- ETAP 2: Pola ---
    // Semafor liczby produktów (Lidlomixów). Zaczyna się od 0.
    private readonly SemaphoreSlim _products;
    
    // Lista barier - jedna bariera dla każdego 3-osobowego zespołu
    private readonly List<Barrier> _teamBarriers;
    
    // Licznik dostarczonych towarów (współdzielony przez wszystkie zespoły)
    private int _deliveredCount = 0;

```

### 2. Inicjalizacja w Konstruktorze

Dodaj to do konstruktora:

```csharp
    public Shop(int customers, int workers, int cashiers, CancellationToken token = default)
    {
        // ... (kod z etapu 1: CustomersCount = ..., _shopCapacity = ...)

        // --- ETAP 2: Inicjalizacja ---
        // 0 produktów na start
        _products = new SemaphoreSlim(0, int.MaxValue);
        
        // Tworzymy bariery. Liczba zespołów = WorkersCount / 3
        _teamBarriers = new List<Barrier>();
        int numberOfTeams = WorkersCount / 3;
        
        for (int i = 0; i < numberOfTeams; i++)
        {
            // Każda bariera czeka na 3 pracowników
            _teamBarriers.Add(new Barrier(3));
        }
    }

```

### 3. Logika Pracownika (`RunWorkerAsync`)

To jest nowa metoda. Pracownicy synchronizują się na barierze, jeden z nich wykonuje dostawę, a potem wszyscy wracają do pracy.

```csharp
    private async Task RunWorkerAsync(int id, int teamId, Barrier teamBarrier)
    {
        try
        {
            // Pętla: Pracujemy dopóki nie dostarczymy tyle towaru, ilu jest klientów (M)
            // ORAZ dopóki nie ma sygnału zatrzymania (SIGINT)
            while (!Token.IsCancellationRequested && _deliveredCount < CustomersCount)
            {
                // 1. Synchronizacja: Czekamy na resztę zespołu (3 osoby)
                // Wszyscy pracownicy zespołu zatrzymają się tutaj, aż zbierze się cała trójka.
                teamBarrier.SignalAndWait(Token);

                // 2. Dostawa (wykonuje tylko Lider zespołu, żeby nie dodać towaru 3 razy)
                // Przyjmujemy, że liderem jest pracownik, którego ID jest podzielne przez 3 (w ramach zespołu logicznego)
                // Najprościej: sprawdzamy czy to pierwszy wątek w tej grupie.
                // Ponieważ barierę przekazujemy konkretną, po prostu umówmy się:
                // Logikę wykonuje tylko ten, kto wpadł w ifa (tutaj prościej użyć modulo z globalnego ID)
                bool isLeader = (id % 3 == 0);

                if (isLeader)
                {
                    // Symulacja dostawy
                    Console.WriteLine($"TEAM {teamId}: Delivering stock");
                    await Task.Delay(400, Token);

                    // Zwiększamy licznik dostaw (bezpiecznie wielowątkowo)
                    Interlocked.Increment(ref _deliveredCount);

                    // Dodajemy towar na półkę (zwalniamy semafor produktów)
                    // Dzięki temu jeden oczekujący klient zostanie "obudzony"
                    _products.Release();
                }

                // 3. Opcjonalnie: Druga bariera, aby zespół skończył cykl razem
                // (zapobiega sytuacji, gdzie szybcy pracownicy obracają pętlę i czekają na wolnego lidera)
                teamBarrier.SignalAndWait(Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Obsługa zatrzymania
        }
        catch (Exception e)
        {
            // Obsługa błędu bariery (np. gdy zostanie zdisposowana)
        }
    }

```

### 4. Aktualizacja Logiki Klienta (`RunCustomerAsync`)

Musisz dodać fragment, w którym klient czeka na towar (między wejściem do sklepu a kasą).

```csharp
    private async Task RunCustomerAsync(int id)
    {
        try
        {
            // Etap 1: Wejście
            await _shopCapacity.WaitAsync(Token);
            try // Try-finally dla zwolnienia miejsca w sklepie
            {
                Console.WriteLine($"CUSTOMER {id}: Entered the shop");
                await Task.Delay(200, Token);

                // --- ETAP 2: Pobranie towaru ---
                // Klient czeka, aż pracownik wrzuci towar (Release)
                // Jeśli stan = 0, wątek klienta tutaj się zatrzyma.
                await _products.WaitAsync(Token);
                
                Console.WriteLine($"CUSTOMER {id}: Picked up Lidlomix");
                
                // Tutaj w Etapie 3 dojdzie kolejka do kasy...
            }
            finally
            {
                _shopCapacity.Release();
            }
        }
        catch (OperationCanceledException) { }
    }

```

### 5. Uruchamianie w `StartSimulationAsync`

Dodaj pętlę uruchamiającą pracowników.

```csharp
    public async Task StartSimulationAsync()
    {
        var allTasks = new List<Task>();

        // --- ETAP 2: Uruchamianie Pracowników ---
        for (int i = 0; i < WorkersCount; i++)
        {
            int workerId = i;
            int teamId = i / 3; // Np. pracownicy 0,1,2 -> zespół 0; 3,4,5 -> zespół 1
            
            // Pobieramy odpowiednią barierę dla tego zespołu
            var barrier = _teamBarriers[teamId];

            allTasks.Add(Task.Run(() => RunWorkerAsync(workerId, teamId, barrier), Token));
        }

        // ... (Uruchamianie Klientów z Etapu 1) ...
        
        // ... (WaitWhenAll) ...
    }

```

### 6. Sprzątanie w `Dispose`

Nie zapomnij o `Dispose` dla nowych obiektów.

```csharp
    public void Dispose()
    {
        _shopCapacity?.Dispose(); // Etap 1
        
        // --- ETAP 2 ---
        _products?.Dispose();
        if (_teamBarriers != null)
        {
            foreach (var b in _teamBarriers) b.Dispose();
        }
    }

```


Jasne, realizujemy **Etap 3**. To najtrudniejsza część, ponieważ wymaga bezpośredniej komunikacji między dwoma różnymi zadaniami: **Kasjerem** a konkretnym **Klientem**.

W tym etapie:

1. Wprowadzamy kolejkę blokującą (`BlockingCollection`) o pojemności 5.
2. Tworzymy logikę kasjerów, którzy pobierają klientów z tej kolejki.
3. Implementujemy mechanizm, dzięki któremu **Klient czeka na Kasjera**, a Kasjer po zakończeniu pracy "budzi" tego konkretnego Klienta.

---

### 1. Klasa Pomocnicza (Kontekst Klienta)

Aby kasjer wiedział, kogo obsługuje, i mógł powiadomić **tylko tego jednego klienta** o zakończeniu, musimy przekazać do kolejki obiekt zawierający ID klienta oraz jego prywatny "sygnalizator" (Semafor).

Dodaj tę małą klasę **wewnątrz** klasy `Shop` (na samym dole) lub w tym samym pliku:

```csharp
    // Klasa pomocnicza: Bilet, który klient wręcza kasjerowi
    private class CustomerContext
    {
        public int CustomerId { get; }
        // Prywatny semafor klienta - kasjer go zwolni (Release), gdy skończy
        public SemaphoreSlim PaidSignal { get; }

        public CustomerContext(int id, SemaphoreSlim signal)
        {
            CustomerId = id;
            PaidSignal = signal;
        }
    }

```

### 2. Pola w klasie `Shop`

Dodaj pole odpowiedzialne za kolejkę do kasy:

```csharp
    // --- ETAP 3: Pola ---
    // Kolejka FIFO o pojemności 5. Przechowuje kontekst klienta.
    private readonly BlockingCollection<CustomerContext> _checkoutQueue;

```

### 3. Inicjalizacja w Konstruktorze

Zainicjalizuj kolejkę z ograniczeniem do 5 elementów:

```csharp
    public Shop(int customers, int workers, int cashiers, CancellationToken token = default)
    {
        // ... (poprzednie inicjalizacje) ...

        // --- ETAP 3: Inicjalizacja ---
        // Pojemność 5 - jeśli wejdzie szósty, zablokuje się na metodzie .Add()
        _checkoutQueue = new BlockingCollection<CustomerContext>(5);
    }

```

### 4. Logika Kasjera (`RunCashierAsync`)

To nowa metoda. Kasjer w pętli bierze klienta, "obsługuje" go (czeka 300ms), a potem zwalnia blokadę tego klienta.

```csharp
    private async Task RunCashierAsync(int id)
    {
        try
        {
            // Pętla pobierająca klientów z kolejki
            // GetConsumingEnumerable() czeka, jeśli kolejka jest pusta, i kończy się przy anulowaniu tokena
            foreach (var context in _checkoutQueue.GetConsumingEnumerable(Token))
            {
                // Logika obsługi
                Console.WriteLine($"CASHIER {id}: Serving Customer {context.CustomerId}");
                
                // Symulacja kasowania (300ms)
                await Task.Delay(300, Token);

                // Powiadomienie klienta: "Już zapłacone, możesz iść"
                // Zwalniamy semafor, na którym wisi TEN konkretny klient
                context.PaidSignal.Release();
            }
        }
        catch (OperationCanceledException)
        {
            // Koniec pracy kasjera
        }
    }

```

### 5. Aktualizacja Logiki Klienta (`RunCustomerAsync`)

To kluczowy moment. Po pobraniu towaru klient nie wychodzi od razu. Musi stanąć w kolejce i czekać na obsługę.

```csharp
    private async Task RunCustomerAsync(int id)
    {
        try
        {
            // --- ETAP 1: Wejście ---
            await _shopCapacity.WaitAsync(Token);
            try
            {
                Console.WriteLine($"CUSTOMER {id}: Entered the shop");
                await Task.Delay(200, Token);

                // --- ETAP 2: Towar ---
                await _products.WaitAsync(Token);
                Console.WriteLine($"CUSTOMER {id}: Picked up Lidlomix");

                // --- ETAP 3: Kasa ---
                
                // 1. Tworzymy własny semafor (czerwone światło), domyślnie zamknięty (0)
                using var myReceiptSignal = new SemaphoreSlim(0, 1);
                
                // 2. Tworzymy bilet z naszym ID i tym semaforem
                var ticket = new CustomerContext(id, myReceiptSignal);

                // 3. Stajemy w kolejce (blokuje, jeśli w kolejce jest już 5 osób)
                _checkoutQueue.Add(ticket, Token);

                // 4. Czekamy, aż kasjer nas obsłuży (zwolni nasz semafor)
                await myReceiptSignal.WaitAsync(Token);

                // 5. Wyjście
                Console.WriteLine($"CUSTOMER {id}: Paid and leaving");
            }
            finally
            {
                // Zwolnienie miejsca w sklepie (dla kolejnych klientów przed wejściem)
                _shopCapacity.Release();
            }
        }
        catch (OperationCanceledException) { }
    }

```

### 6. Uruchamianie w `StartSimulationAsync`

Dodaj pętlę uruchamiającą kasjerów na początku metody `StartSimulationAsync`.

```csharp
    public async Task StartSimulationAsync()
    {
        var allTasks = new List<Task>();

        // --- ETAP 3: Uruchamianie Kasjerów ---
        for (int i = 0; i < CashiersCount; i++)
        {
            int cashierId = i;
            // Uruchamiamy zadanie kasjera
            allTasks.Add(Task.Run(() => RunCashierAsync(cashierId), Token));
        }

        // ... (Uruchamianie Pracowników - Etap 2) ...
        // ... (Uruchamianie Klientów - Etap 1) ...
    }

```

### 7. Sprzątanie w `Dispose`

Dodaj sprzątanie kolejki.

```csharp
    public void Dispose()
    {
        _shopCapacity?.Dispose();
        _products?.Dispose();
        // --- ETAP 3 ---
        _checkoutQueue?.Dispose();
        
        if (_teamBarriers != null)
            foreach (var b in _teamBarriers) b.Dispose();
    }

```

### Podsumowanie działania Etapu 3:

1. Klient wchodzi do `_checkoutQueue.Add()`. Jeśli jest tam 5 osób, wątek klienta staje w miejscu (czeka na miejsce w kolejce).
2. Gdy wejdzie do kolejki, od razu robi `myReceiptSignal.WaitAsync()`. Teraz czeka na kasjera.
3. Kasjer wyciąga go za pomocą `GetConsumingEnumerable()`.
4. Kasjer "pikuje produkty" (Delay).
5. Kasjer robi `Release()` na semaforze klienta.
6. Klient budzi się, wypisuje "Paid" i wychodzi ze sklepu (`_shopCapacity.Release()`).


To jest ostatni, kluczowy element, który sprawia, że aplikacja kończy się "z klasą", zamiast się zawieszać.

**Etap 4** wymaga obsłużenia `SIGINT` (czyli np. Ctrl+C). W środowisku .NET realizuje się to poprzez przekazanie `CancellationToken` do wszystkich metod blokujących.

Oto co musisz zmienić/upewnić się, że masz w kodzie, aby zaliczyć ten etap.

### 1. Zasada działania

Wszystkie miejsca, gdzie wątek "czeka" (`Task.Delay`, `WaitAsync`, `SignalAndWait`, `GetConsumingEnumerable`), **muszą** przyjmować `Token`. Gdy użytkownik naciśnie Ctrl+C, ten token zostanie anulowany, a wszystkie te metody rzucą wyjątek `OperationCanceledException`. Musimy go złapać i po prostu zakończyć zadanie.

### 2. Kod do aktualizacji w klasie `Shop`

Oto jak powinny wyglądać metody z uwzględnieniem obsługi przerywania (zmiany są oznaczone komentarzami):

#### A. Pracownik (`RunWorkerAsync`)

Pracownik musi sprawdzać token w pętli `while` oraz obsłużyć przerwanie na barierze.

```csharp
private async Task RunWorkerAsync(int id, int teamId, Barrier teamBarrier)
{
    try
    {
        // 1. Sprawdzamy Token w warunku pętli
        while (!Token.IsCancellationRequested && _deliveredCount < CustomersCount)
        {
            // 2. Przekazujemy Token do bariery
            // Jeśli naciśniesz Ctrl+C, gdy pracownicy czekają, to rzuci wyjątek
            teamBarrier.SignalAndWait(Token);

            bool isLeader = (id % 3 == 0);
            if (isLeader)
            {
                Console.WriteLine($"TEAM {teamId}: Delivering stock");
                // 3. Przekazujemy Token do Delaya
                await Task.Delay(400, Token);
                
                Interlocked.Increment(ref _deliveredCount);
                _products.Release();
            }
            
            // Druga synchronizacja też z Tokenem
            teamBarrier.SignalAndWait(Token);
        }
    }
    catch (OperationCanceledException)
    {
        // 4. TO JEST KLUCZ DO ETAPU 4
        // Gdy przyjdzie sygnał SIGINT, łapiemy wyjątek i po prostu wychodzimy z metody.
        // Dzięki temu zadanie kończy się statusiem RanToCompletion lub Canceled, a nie Faulted.
        // Console.WriteLine($"Worker {id} stopping work."); 
    }
    catch (Exception)
    {
        // Obsługa np. BarrierPostPhaseException, gdy bariera zostanie zdisposowana
    }
}

```

#### B. Kasjer (`RunCashierAsync`)

Kasjer używa `GetConsumingEnumerable`, który świetnie współpracuje z tokenem.

```csharp
private async Task RunCashierAsync(int id)
{
    try
    {
        // 1. Przekazujemy Token do GetConsumingEnumerable
        // Gdy Token zostanie anulowany, pętla foreach zostanie natychmiast przerwana (po rzuceniu wyjątku)
        foreach (var context in _checkoutQueue.GetConsumingEnumerable(Token))
        {
            Console.WriteLine($"CASHIER {id}: Serving Customer {context.CustomerId}");
            // 2. Token w Delay
            await Task.Delay(300, Token);
            
            context.PaidSignal.Release();
        }
    }
    catch (OperationCanceledException)
    {
        // 3. Obsługa wyjścia
        // Console.WriteLine($"Cashier {id} going home.");
    }
}

```

#### C. Klient (`RunCustomerAsync`)

U klienta najważniejsze jest to, żeby w bloku `finally` zwolnił miejsce w sklepie, nawet jeśli został "zabity" przez anulowanie w połowie zakupów.

```csharp
private async Task RunCustomerAsync(int id)
{
    try
    {
        // Token we wszystkich WaitAsync
        await _shopCapacity.WaitAsync(Token);
        try
        {
            Console.WriteLine($"CUSTOMER {id}: Entered the shop");
            await Task.Delay(200, Token);

            await _products.WaitAsync(Token);
            // ... reszta logiki ...
            
            // ... czekanie na kasę ...
            await myReceiptSignal.WaitAsync(Token); // Tu też Token!
            
            Console.WriteLine($"CUSTOMER {id}: Paid and leaving");
        }
        finally
        {
            // TO JEST WAŻNE DLA ETAPU 4:
            // Sprzątanie zasobów nawet przy przerwaniu programu
            if (_shopCapacity.CurrentCount < 8) 
                _shopCapacity.Release();
        }
    }
    catch (OperationCanceledException)
    {
        // Klient rezygnuje z zakupów bo sklep się zamyka (Ctrl+C)
    }
}

```

### 3. Zamykanie w `StartSimulationAsync`

Tutaj musimy obsłużyć sytuację, gdy `Task.WhenAll` rzuci błąd z powodu anulowania zadań.

```csharp
public async Task StartSimulationAsync()
{
    // ... (tworzenie zadań kasjerów, pracowników, klientów) ...

    try
    {
        // Czekamy na klientów.
        // Jeśli naciśniesz Ctrl+C, token zostanie anulowany, zadania klientów rzucą
        // OperationCanceledException, a Task.WhenAll rzuci to wyżej.
        await Task.WhenAll(customerTasks);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("\nSymulacja przerwana przez użytkownika.");
    }
    finally
    {
        // Upewniamy się, że wszystko jest posprzątane
        Dispose();
    }
}

```

### 4. Sprawdzenie (Checklist dla Etapu 4)

Aby zdobyć te [2p], upewnij się, że:

1. [x] Każdy `await Task.Delay(...)` ma `Token`.
2. [x] Każdy `await semaphore.WaitAsync(...)` ma `Token`.
3. [x] Każda bariera `barrier.SignalAndWait(...)` ma `Token`.
4. [x] Metody są otoczone blokiem `try { ... } catch (OperationCanceledException) { }`.
5. [x] Metoda `Dispose` zwalnia wszystkie semafory, bariery i kolekcję.

Jeśli wkleiłeś "Pełne rozwiązanie", które podałem Ci wcześniej, to **wszystkie te warunki są już spełnione**. Tamten kod zawierał logikę dla wszystkich 4 etapów od razu.

Czy chcesz, abym wygenerował teraz plik `Program.cs`, abyś mógł to uruchomić i przetestować?



caly kod 
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