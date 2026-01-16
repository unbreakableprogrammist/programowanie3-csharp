using System.ComponentModel;
using System.Diagnostics;

[assembly: Description("Applied to an assembly")]
[module: Description("Applied to a module")]

[Description("Applied to a class")]
public class Stack<[Description("Applied to a generic parameter")] T>
{
    [Description("Applied to a field")]
    private T[] _items = new T[8];
    [field: Description("Applied to a backing field")]
    [Description("Applied to a property")]
    public int Count { get; private set; }

    [Description("Applied to a method")]
    public void Push([Description("Applied to a parameter")] T item)
    {
        if (_items.Length == Count)
        {
            Array.Resize(ref _items, _items.Length * 2);
        }
        _items[Count++] = item;
    }

    [return: Description("Applied to a return value")]
    [method: Description("Implicitly applied to a method")]
    public T Pop()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
        return _items[--Count];
    }
}
public class Program
{
    static void Main()
    {
        Log("Hello");
        Console.WriteLine("Done");
    }

    [Conditional("DEBUG")]
    static void Log(string msg)
    {
        Console.WriteLine(msg);
    }
}