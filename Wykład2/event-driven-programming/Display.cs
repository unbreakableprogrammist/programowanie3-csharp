// Przestrzeń nazw — grupuje klasy projektu.
namespace ShopEvents;

// Komentarz XML — opis klasy Display.
/// <summary>
/// Simulates a UI display that renders the state of a shopping cart.
/// This class is an event subscriber to the ShoppingCart's events.
/// </summary>
// Klasa Display — subskrybent eventów koszyka; odpowiada za wypisywanie jego zawartości.
public class Display
{
    // Metoda Subscribe — podpinamy Display pod event CartUpdated.
    public void Subscribe(ShoppingCart cart)
    {
        // Podłączenie handlera — Display będzie reagował na zmiany koszyka.
        cart.CartUpdated += OnCartUpdated;
    }

    // Handler eventu CartUpdated — wywoływany przy każdej zmianie koszyka.
    protected virtual void OnCartUpdated(object? sender, EventArgs e)
    {
        // Sprawdzamy, czy event pochodzi z obiektu typu ShoppingCart, i rzutujemy sender.
        if (sender is ShoppingCart cart)
        {
            // Nagłówek wizualny wyświetlający sekcję koszyka.
            Console.WriteLine("\n---[ Shopping Cart Display ]---");
            // Jeśli koszyk jest pusty — wypisujemy odpowiedni komunikat.
            if (cart.Items.Count == 0)
            {
                Console.WriteLine("  Cart is empty.");
            }
            else
            {
                // Iterujemy po wszystkich pozycjach koszyka i wypisujemy ich stan.
                foreach (var item in cart.Items)
                {
                    Console.WriteLine($"  - {item.Key.Name,-10} | {item.Value} unit(s) | {item.Key.Price:C} each");
                }
            }
            Console.WriteLine("-------------------------------");
            // Wypisujemy aktualną łączną cenę koszyka.
            Console.WriteLine($"  Total Price: {cart.TotalPrice:C}");
            Console.WriteLine("-------------------------------\n");
        }
    }
}
