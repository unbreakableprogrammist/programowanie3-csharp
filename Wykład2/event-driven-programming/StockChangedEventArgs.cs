// Przestrzeń nazw — grupuje klasy projektu w logiczny moduł.
namespace ShopEvents;

/// <summary>
// Komentarz XML — opis klasy reprezentującej dane zdarzenia zmiany stanu magazynowego.
///
/// Custom event arguments for the StockChanged event.
/// Contains the old and new stock quantity.
/// </summary>
// Klasa StockChangedEventArgs — paczka danych dla eventu zmiany ilości produktu; dziedziczy po EventArgs zgodnie ze standardem .NET.
public class StockChangedEventArgs : EventArgs
{
    // Właściwości tylko do odczytu — przechowują starą oraz nową ilość produktu na magazynie.
    public int OldQuantity { get; }
    public int NewQuantity { get; }

    // Konstruktor — przyjmuje starą i nową ilość, zapisując je do właściwości.
    public StockChangedEventArgs(int oldQuantity, int newQuantity)
    {
        OldQuantity = oldQuantity;
        NewQuantity = newQuantity;
    }
}
