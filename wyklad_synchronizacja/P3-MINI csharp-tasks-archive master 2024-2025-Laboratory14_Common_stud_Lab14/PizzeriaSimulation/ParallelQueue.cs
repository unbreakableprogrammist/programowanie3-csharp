using System;
using System.Threading;
using System.Threading.Tasks;

namespace PizzeriaSimulation;

public sealed class ParallelQueue<T>
    : IParallelQueue<T>, IPausableQueue<T>, IDisposable
    where T : class
{
    private readonly T[] _items;
    private int _head;
    private int _tail;
    private int _count;
    private readonly int _capacity;

    private readonly SemaphoreSlim _semEmpty;
    private readonly SemaphoreSlim _semFull;
    private readonly ManualResetEventSlim _pauseEnqueueEvent;
    private readonly ManualResetEventSlim _pauseDequeueEvent;
    
    // POPRAWKA 1: _lock musi być polem klasy, a nie zmienną w konstruktorze!
    private readonly object _lock = new object();

    // POPRAWKA 2: Zmieniono nazwę argumentu z 'maxSize' na 'capacity' dla spójności
    public ParallelQueue(int capacity)
    {
        if (capacity <= 0) throw new ArgumentException("Capacity must be greater than 0");

        _capacity = capacity;
        _items = new T[capacity];
        _head = 0;
        _tail = 0;
        _count = 0;

        _semEmpty = new SemaphoreSlim(capacity, capacity);
        _semFull = new SemaphoreSlim(0, capacity);

        _pauseEnqueueEvent = new ManualResetEventSlim(true);
        _pauseDequeueEvent = new ManualResetEventSlim(true);
    }

    public void Dispose()
    {
        _semEmpty.Dispose();
        _semFull.Dispose();
        _pauseEnqueueEvent.Dispose();
        _pauseDequeueEvent.Dispose();
    }

    public T Dequeue()
    {
        _pauseDequeueEvent.Wait();
        _semFull.Wait();

        T item;
        lock (_lock)
        {
            item = _items[_head];
            _items[_head] = default!; // Dodano wykrzyknik (null-forgiving) dla czystości
            _head = (_head + 1) % _capacity;
            _count--;
        }

        _semEmpty.Release();
        return item;
    }

    public void Enqueue(T item)
    {
        _pauseEnqueueEvent.Wait();
        _semEmpty.Wait();

        lock (_lock)
        {
            _items[_tail] = item;
            _tail = (_tail + 1) % _capacity;
            _count++;
        }

        _semFull.Release();
    }

    public async Task<T?> TryDequeueAsync(int timeoutMilliseconds, CancellationToken cancellationToken)
    {
        try
        {
            _pauseDequeueEvent.Wait(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null;
        }

        bool can_enter = await _semFull.WaitAsync(timeoutMilliseconds, cancellationToken);
        if (can_enter)
        {
            T item;
            lock (_lock)
            {
                item = _items[_head];
                _items[_head] = default!;
                _head = (_head + 1) % _capacity;
                _count--;
            }

            _semEmpty.Release();
            return item;
        }
        return null; // POPRAWKA 4: Brakowało średnika
    }

    public async Task<bool> TryEnqueueAsync(T item, int timeoutMilliseconds, CancellationToken cancellationToken)
    {
        try
        {
            _pauseEnqueueEvent.Wait(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }

        bool can_enter = await _semEmpty.WaitAsync(timeoutMilliseconds, cancellationToken);
        if (can_enter)
        {
            lock (_lock)
            {
                _items[_tail] = item;
                _tail = (_tail + 1) % _capacity;
                _count++; 
            }

            _semFull.Release();
            return true;
        }

        return false;
    }

    
    
    public void PauseDequeue()
    {
        _pauseDequeueEvent.Reset(); // RESET = Zamknij bramę (Pauza)
    }

    public void PauseEnqueue()
    {
        _pauseEnqueueEvent.Reset(); // RESET = Zamknij bramę (Pauza)
    }

    public void ResumeDequeue()
    {
        _pauseDequeueEvent.Set();   // SET = Otwórz bramę (Wznowienie) - nie ma metody Resume()!
    }

    public void ResumeEnqueue()
    {
        _pauseEnqueueEvent.Set();   // SET = Otwórz bramę (Wznowienie)
    }
}