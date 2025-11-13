// Przestrzeń nazw — grupuje wszystkie klasy projektu w logiczny moduł.
namespace ShopEvents;

// Komentarz XML — opisuje, do czego służy ta klasa (pokazuje się w IDE).
/// <summary>
/// Custom event arguments for the PriceChanged event.
/// Contains the old and new price of the product.
/// </summary>
// Klasa paczki danych dla eventu zmiany ceny; dziedziczy po EventArgs zgodnie ze standardem .NET.
public class PriceChangedEventArgs : EventArgs
{
    // Właściwości tylko do odczytu — przechowują starą i nową cenę.
    public decimal OldPrice { get; }
    // Właściwości tylko do odczytu — przechowują starą i nową cenę.
    public decimal NewPrice { get; }

    // Konstruktor — przyjmuje starą i nową cenę i zapisuje je w właściwościach.
    public PriceChangedEventArgs(decimal oldPrice, decimal newPrice)
    {
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}
