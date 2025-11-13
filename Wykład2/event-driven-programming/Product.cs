// Przestrzeń nazw — grupuje klasy projektu w logiczny moduł.
namespace ShopEvents;

/// <summary>
/// Represents a product in the shop.
/// This class is an event source for price and stock changes.
/// </summary>
// Komentarz XML — opis klasy Product jako źródła eventów.
// Klasa Product — reprezentuje produkt i jest wydawcą (publisher) eventów zmian ceny oraz stanu magazynowego.
public class Product
{
    public string Name { get; }
    // Pola prywatne przechowujące aktualną cenę i stan magazynowy.
    private decimal _price;
    private int _stockQuantity;

    // Deklaracje eventów — listy metod subskrybentów reagujących na zmiany.
    public event EventHandler<PriceChangedEventArgs>? PriceChanged;
    public event EventHandler<StockChangedEventArgs>? StockChanged;

    // Konstruktor — ustawia nazwę, cenę oraz ilość na magazynie.
    public Product(string name, decimal price, int stockQuantity)
    {
        Name = name;
        _price = price;
        _stockQuantity = stockQuantity;
    }

    // Właściwość Price — sprawdza zmianę ceny i wywołuje event jeśli wartość się zmieni.
    public decimal Price
    {
        get => _price;
        set
        {
            if (_price != value)
            {
                decimal oldPrice = _price;
                _price = value;
                OnPriceChanged(new PriceChangedEventArgs(oldPrice, _price));
            }
        }
    }

    // Właściwość StockQuantity — sprawdza zmianę ilości i wywołuje event jeśli wartość się zmieni.
    public int StockQuantity
    {
        get => _stockQuantity;
        set
        {
            if (_stockQuantity != value)
            {
                int oldStock = _stockQuantity;
                _stockQuantity = value;
                OnStockChanged(new StockChangedEventArgs(oldStock, _stockQuantity));
            }
        }
    }

    // Metoda wywołująca event PriceChanged — powiadamia wszystkich subskrybentów.
    protected virtual void OnPriceChanged(PriceChangedEventArgs e)
    {
        PriceChanged?.Invoke(this, e);
    }

    // Metoda wywołująca event StockChanged — powiadamia wszystkich subskrybentów.
    protected virtual void OnStockChanged(StockChangedEventArgs e)
    {
        StockChanged?.Invoke(this, e);
    }
}
