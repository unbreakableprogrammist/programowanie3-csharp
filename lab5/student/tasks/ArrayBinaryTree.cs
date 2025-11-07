using System.Collections;

namespace tasks;

/// <summary>
/// Interface for an array-based binary tree.
/// </summary>
public interface IArrayBinaryTree<T> : IEnumerable<T>
{
    /// <summary>
    /// Gets the number of nodes in the tree.
    /// </summary>
    int Count { get; } // done

    /// <summary>
    /// Gets the index of the root.
    /// </summary>
    int RootIndex { get; }  // done

    /// <summary>
    /// Sets the value of the root node.
    /// </summary>
    void SetRoot(T value);  // done

    /// <summary>
    /// Returns the indices of the left and right children for a given parent.
    /// </summary>
    (int leftIndex, int rightIndex) GetChildrenIndices(int parentIndex);

    /// <summary>
    /// Sets the value of the left child of a given parent.
    /// </summary>
    void SetLeftChild(int parentIndex, T value);  

    /// <summary>
    /// Sets the value of the right child of a given parent.
    /// </summary>
    void SetRightChild(int parentIndex, T value);

    /// <summary>
    /// Gets the node at the specified index.
    /// </summary>
    T this[int index] { get; }

    /// <summary>
    /// Checks if a node at the specified index exists.
    /// </summary>
    bool Exists(int index);

    /// <summary>
    /// Clears all nodes from the tree.
    /// </summary>
    void Clear();
}

/// <summary>
/// An implementation of a binary tree based on an array.
/// Indices are calculated as:
/// - Left Child: 2 * i + 1
/// - Right Child: 2 * i + 2
/// - Parent: (i - 1) / 2
/// </summary>
public class ArrayBinaryTree<T> : IArrayBinaryTree<T>
{
    private T[] nodes;  // deklaracja pola
    private bool[] _present;
    public int Count { get;private set;}
    // to samo co public int RootIndex => 0 (  jedna strzaleczka odpowiada za get) 
    public int RootIndex
    {
        get
        {
            return 0;
        }
    }
    public ArrayBinaryTree(int initialCapacity = 8)   // konstruktor , 8 randomowa liczba na poczatek
    {
        if (initialCapacity < 1) initialCapacity = 8;
        nodes = new T[initialCapacity];
        _present= new bool[initialCapacity];
        Count = 0;
    }
    private void Resize(int index)
    {
        if (index  >= nodes.Length)
        {
            int newLength = nodes.Length * 2;
            while (newLength < index)
            {
                newLength *= 2;
            }
            Array.Resize(ref nodes, newLength);
            Array.Resize(ref _present, newLength);
        }
    }
    public T this[int index]
    {
        get
        {
            if (index >= RootIndex && index < Count && _present[index])
                return nodes[index];
            throw new IndexOutOfRangeException();
        }
    }
    public void Clear()
    {
        Array.Clear(nodes, 0, nodes.Length);
        Array.Clear(_present, 0, _present.Length);
        Count = 0;
    }

    public bool Exists(int index)
    {
        if(index >= RootIndex && index < Count && _present[index])
            return true;
        return false;
    }

    public (int leftIndex, int rightIndex) GetChildrenIndices(int parentIndex)
    {
        if(parentIndex >= RootIndex && parentIndex < Count && _present[parentIndex])
            throw  new IndexOutOfRangeException();
        return (parentIndex*2+1, parentIndex*2+2);
    }

    
    /*
      Jak to dziala , IEnumerable to taki interfejs , 	2.	IEnumerable<T> — to tylko umowa: „potrafię zwrócić enumerator”. Nic nie „chodzi” i niczego „nie przekształca” samo z siebie.
        ale jesli zrobimy metode typu IEnumerable to dziala to tak , ta metoda za pomoca yield ustawia nasze dane do zwrocenia w jakas sekwencje , pozniej te dane sa typu IEnumerable , wiec maja 
        metode GetEnumerator ktora jest typu IEnumerator , czyli jak odaplamy for to on idzie po ustawionej przez nas sekwencji , dla kazdej kolejnego obiektu odpala GetEnumerator , wiec ma 
        dostep do current i Move next i tak krazy 
     */
    private IEnumerable<T> InOrder(int index)
    {
        if (!Exists(index)) 
            yield break;

        var (left, right) = GetChildrenIndices(index);

        foreach (var v in InOrder(left))   // idziemy po lewych synach najpierw, dzieki temu ze jest to lazy programming to to bedzie sie odpalalo tylko kiedy bedziemy chcieli ta rekurencja
            yield return v;

        yield return nodes[index]; // teraz przeszlismy wszystko w lewo wiec jestesmy u siebie i idziemy w prawo

        foreach (var v in InOrder(right))
            yield return v;
    }
    
    public IEnumerator<T> GetEnumerator() 
    {
        return InOrder(RootIndex).GetEnumerator();  // bo my tak na prawde potrzebujemy po prostu zaczac od korzenia i dostawac kolejne enumeratory tak (bo dzieki lzay programing i tak dostaniemy pozniej)
    }

    /*
     Dokladny opis kroków na gorze 
     mamy petle :
     foreach (var v in tree)
       {
           Console.WriteLine(v);
       }
       kompilator robi to 
       var enumerator = tree.GetEnumerator(); , czyli mowi do drzewa daj mi swoj enumerator , drzewo mowi spoko i patrzy co ejst w metodzie GetEnumerator a tam : 
       GetEnumerator()  // na ArrayBinaryTree<T>
       → InOrder(RootIndex).GetEnumerator()
        czyli mowimy mu wszystko jest zapisane w sekwencji generowanej przez InOrder i przkazujemy mu roota 
        teraz foreach zaczyna dzialac 
        while (enumerator.MoveNext()) czyli tak dziala foreach idziemy po calej sekwencji az mozemy
       {
           var v = enumerator.Current;  bierzemy obecna wartosc
           Console.WriteLine(v);
       }
       InOrder za to to funkcja ktora nam generuje sekwencjie , ale kazde nastepne wywolanie rekurencujne jest tylko kiedy jest MoveNext();
       tu jeszcze chat fajnie opisal :
       
       Jak MoveNext() uruchamia InOrder
       •	Pierwsze MoveNext():
       •	Wywołuje się InOrder(0) (bo RootIndex=0).
       •	Jeśli Exists(0) jest false → yield break → pętla jest pusta.
       •	Jeśli Exists(0) jest true:
       1.	wylicza (left, right).
       2.	rekurencyjnie wchodzi w InOrder(left) „aż do skrajnie lewego istniejącego liścia”.
       3.	gdy dotrze do liścia, wykona pierwszy yield return → MoveNext() zwraca true.
       4.	Current teraz ma pierwszą wartość w porządku in-order.
       •	Każde kolejne MoveNext():
       •	„Wznawia” InOrder od miejsca przerwania
       •	Jeśli po lewej już koniec, zwraca bieżący węzeł (yield return nodes[index])
       •	Potem wchodzi w prawe poddrzewo (InOrder(right)), znów aż do pierwszego yield itd.
       •	Gdy wszystko zwrócone, kolejne MoveNext() da false.
       4.	Zakończenie
       •	Po wyjściu z pętli enumerator jest utylizowany (jeśli implementuje IDisposable, co ma miejsce dla iteratorów z yield).

     */
    public void SetLeftChild(int parentIndex, T value)
    {
        if(!Exists(parentIndex))
            throw new IndexOutOfRangeException();
        int index = parentIndex * 2 + 1;
        if (index >= nodes.Length)
            Resize(index * 2 + 1);
        nodes[index] = value;
        Count++;
    }

    public void SetRightChild(int parentIndex, T value)
    {
        if(!Exists(parentIndex))
            throw new IndexOutOfRangeException();
        int index = parentIndex * 2 + 2;
        if (index >= nodes.Length)
            Resize(index * 2 + 1);
        nodes[index] = value;
        Count++;
    }

    public void SetRoot(T value)
    {
        nodes[0]= value; // bo zawsze mamy zapas na miejscu zerowym 
        Count++;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
