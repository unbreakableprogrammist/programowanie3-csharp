namespace ShopEvents;                      // Deklaracja przestrzeni nazw. Wszystkie klasy z projektu,
                                           // które są w "ShopEvents", są logicznie pogrupowane razem.

/// <summary>                               // Komentarz XML – opisuje klasę poniżej (do IntelliSense, dokumentacji).
/// Simulates a store notifier that listens to product stock changes.
/// This class is an event subscriber.
/// </summary>
public class Notifier                       // Deklaracja klasy Notifier – obiekt, który będzie "słuchaczem" (subskrybentem) eventów.
{
    public void Subscribe(Product product)  // Publiczna metoda, która zapisuje (subskrybuje) Notifiera na event danego Producta.
    {
        product.StockChanged += OnProductStockChanged;
                                           // Do eventu StockChanged danego obiektu product
                                           // dopinamy metodę OnProductStockChanged.
                                           // Od teraz przy każdej zmianie stanu magazynowego (StockChanged)
                                           // ta metoda będzie wywoływana.
    }

    public void Unsubscribe(Product product)// Publiczna metoda, która wypisuje (unsubskrybuje) Notifiera z eventu Producta.
    {
        product.StockChanged -= OnProductStockChanged;
                                           // Z eventu StockChanged usuwamy (odejmujemy) metodę OnProductStockChanged.
                                           // Po tym wywołaniu Notifier przestaje reagować na zmiany stocku tego konkretnego Product.
    }

    protected virtual void OnProductStockChanged(object? sender, StockChangedEventArgs e)
                                           // Metoda obsługująca event – będzie wywoływana, gdy Product zgłosi StockChanged.
                                           // 'protected' – widoczna w tej klasie i klasach pochodnych.
                                           // 'virtual' – można ją nadpisać w klasie dziedziczącej, np. żeby zmienić sposób logowania.
                                           // 'sender' – obiekt, który wywołał event (najczęściej konkretny Product).
                                           // 'StockChangedEventArgs e' – paczka z danymi o zmianie ilości (stara/nowa ilość).
    {
        if (sender is not Product product) return;
                                           // Sprawdzamy, czy sender to rzeczywiście Product.
                                           // Jeśli event przyszedł z czegokolwiek innego (albo null),
                                           // wychodzimy z metody i nic nie robimy.
                                           // Dodatkowo pattern matching 'is not Product product' robi od razu rzutowanie
                                           // do zmiennej 'product'.

        switch (e.NewQuantity)             // Instrukcja switch na nowej ilości towaru (po zmianie).
        {
            case 0 when e.OldQuantity > 0: // Przypadek 1: nowa ilość == 0, a stara była > 0.
                                           // Czyli produkt właśnie wyzerował stan – wysprzedany.
                Console.WriteLine($"[NOTIFIER] ALERT: '{product.Name}' is now OUT OF STOCK.");
                                           // Wypisujemy komunikat: produkt wyszedł z magazynu (brak na stanie).
                break;                     // Kończymy ten przypadek switcha.

            case > 0 when e.OldQuantity == 0:
                                           // Przypadek 2: nowa ilość > 0, a stara była równa 0.
                                           // Czyli wcześniej produkt był niedostępny, a teraz wrócił na magazyn.
                Console.WriteLine($"[NOTIFIER] GOOD NEWS: '{product.Name}' is BACK IN STOCK ({e.NewQuantity} units).");
                                           // Wypisujemy komunikat: produkt wrócił na stan, z podaniem nowej ilości.
                break;                     // Kończymy ten przypadek switcha.
        }                                  // Koniec switch (inne przypadki – np. zmiana 5 → 10 – są ignorowane).
    }                                      // Koniec metody OnProductStockChanged.
}                                          // Koniec klasy Notifier.