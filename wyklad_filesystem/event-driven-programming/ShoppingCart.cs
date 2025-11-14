// Przestrzeń nazw — grupuje klasy projektu w logiczny moduł.
namespace ShopEvents;

// Komentarz XML — opis klasy koszyka oraz jego roli w systemie eventów.
/// <summary>
/// Reprezentuje koszyk zakupowy, który przechowuje produkty, subskrybuje eventy produktów
/// (np. zmiany ceny) i powiadamia o aktualizacjach własnym eventem.
/// </summary>
// Klasa ShoppingCart — reprezentuje koszyk, przechowuje produkty i reaguje na eventy produktów.
public class ShoppingCart
{
    // Prywatny słownik: klucz = produkt, wartość = ilość tego produktu w koszyku.
    private readonly Dictionary<Product, int> _items = new();

    // Publiczne tylko-do-odczytu API do podejrzenia zawartości koszyka (bez możliwości modyfikacji z zewnątrz).
    public IReadOnlyDictionary<Product, int> Items => _items;

    // Całkowita cena koszyka; aktualizowana wewnętrznie przy każdej zmianie.
    public decimal TotalPrice { get; private set; }

    // Event CartUpdated — informuje subskrybentów (np. UI), że koszyk się zmienił (zawartość lub cena).
    public event EventHandler? CartUpdated;

    // Dodaje podaną ilość produktu do koszyka; subskrybuje eventy cenowe produktu przy pierwszym dodaniu.
    public void AddItem(Product product, int quantity = 1)
    {
        // Sprawdzenie, czy na magazynie jest wystarczająca ilość danego produktu.
        if (product.StockQuantity < quantity)
        {
            Console.WriteLine($"(Cart) Cannot add {product.Name}. Not enough stock.");
            return;
        }

        // Jeśli produkt jest już w koszyku — zwiększamy ilość; w przeciwnym razie dodajemy nowy wpis.
        if (_items.ContainsKey(product))
        {
            _items[product] += quantity;
        }
        else
        {
            _items[product] = quantity;
            // Przy pierwszym dodaniu produktu — zaczynamy słuchać zmian jego ceny.
            product.PriceChanged += OnProductPriceChanged;
        }

        // Zmniejszamy stan magazynowy produktu o dodaną do koszyka ilość.
        product.StockQuantity -= quantity;

        // Przeliczamy całkowitą wartość koszyka po zmianie.
        RecalculateTotalPrice();
        // Wywołujemy event CartUpdated, aby powiadomić subskrybentów o zmianie koszyka.
        OnCartUpdated();
    }

    // Usuwa podaną ilość produktu z koszyka; wypisuje się z eventów produktu, gdy ilość spadnie do zera.
    public void RemoveItem(Product product, int quantity = 1)
    {
        // Jeśli produktu nie ma w koszyku — nie ma czego usuwać.
        if (!_items.ContainsKey(product)) return;

        // Zmniejszamy ilość produktu; jeśli spadnie do zera lub mniej — usuwamy z koszyka.
        _items[product] -= quantity;
        if (_items[product] <= 0)
        {
            _items.Remove(product);
            // Przy usunięciu ostatniej sztuki — przestajemy słuchać zmian ceny tego produktu.
            product.PriceChanged -= OnProductPriceChanged;
        }
        
        // Zwracamy usuniętą ilość produktu z koszyka z powrotem na magazyn.
        product.StockQuantity += quantity;

        // Przeliczamy całkowitą wartość koszyka po zmianie.
        RecalculateTotalPrice();
        // Wywołujemy event CartUpdated, aby powiadomić subskrybentów o zmianie koszyka.
        OnCartUpdated();
    }

    // Handler eventu PriceChanged z produktu — reaguje na zmianę ceny i aktualizuje wartość koszyka.
    protected virtual void OnProductPriceChanged(object? sender, PriceChangedEventArgs e)
    {
        // Sprawdzamy, czy event przyszedł faktycznie z obiektu typu Product i rzutujemy go.
        if (sender is Product p)
        {
            // Informacyjny log do konsoli — pokazuje zmianę ceny produktu.
            Console.WriteLine($"(Cart) Price for '{p.Name}' changed from {e.OldPrice:C} to {e.NewPrice:C}. Updating total.");
            // Przeliczamy całkowitą wartość koszyka po zmianie.
            RecalculateTotalPrice();
            // Wywołujemy event CartUpdated, aby powiadomić subskrybentów o zmianie koszyka.
            OnCartUpdated();
        }
    }

    // Pomocnicza metoda licząca sumę: Σ (cena produktu * ilość) dla wszystkich pozycji w koszyku.
    private void RecalculateTotalPrice()
    {
        // Przeliczamy całkowitą cenę na podstawie aktualnego słownika _items.
        TotalPrice = _items.Sum(item => item.Key.Price * item.Value);
    }

    // Metoda wywołująca event CartUpdated — powiadamia subskrybentów o zmianie koszyka.
    protected virtual void OnCartUpdated()
    {
        // Bezpieczne wywołanie eventu (jeśli ma subskrybentów) z pustymi danymi zdarzenia.
        CartUpdated?.Invoke(this, EventArgs.Empty);
    }
}
