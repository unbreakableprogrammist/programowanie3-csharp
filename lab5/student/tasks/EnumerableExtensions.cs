namespace tasks;

// Zbiór metod rozszerzających LINQ dla IEnumerable<T>.
// Każda metoda działa leniwie (yield) - elementy są przetwarzane dopiero gdy ktoś je pobiera.
public static class EnumerableExtensions
{
    // -----------------------------------------------------------
    // FOLD (ogólna redukcja sekwencji)
    // -----------------------------------------------------------
    // Cel:
    //  - przejść po całej sekwencji
    //  - utrzymywać "akumulator" (stan, który pamięta poprzednie kroki)
    //  - na końcu przekształcić stan w końcowy wynik
    //
    // To odpowiednik Aggregate, ale bardziej ogólny.
    public static TResult Fold<TSource, TAccumulate, TResult>(
        this IEnumerable<TSource> source,
        TAccumulate seed,                              // wartość startowa akumulatora
        Func<TAccumulate, TSource, TAccumulate> func,  // jak aktualizujemy akumulator
        Func<TAccumulate, TResult> resultSelector)     // jak z ostatniego stanu zrobić wynik końcowy
    {
        var acc = seed; // stan początkowy

        using var enumerator = source.GetEnumerator(); // jawny enumerator
        while (enumerator.MoveNext())                  // iteracja krok po kroku
        {
            acc = func(acc, enumerator.Current);       // aktualizujemy stan na podstawie elementu
        }

        return resultSelector(acc); // przekształcamy stan na wynik
    }

    // -----------------------------------------------------------
    // BATCH
    // -----------------------------------------------------------
    // Dzielimy sekwencję na porcje o zadanym rozmiarze.
    // Przykład: batch size = 3
    // [1,2,3,4,5,6,7] => [1,2,3], [4,5,6], [7]
    public static IEnumerable<IEnumerable<T>> Batch<T>(
        this IEnumerable<T> collection,
        int size)
    {
        if (size < 1)
            throw new ArgumentOutOfRangeException(nameof(size));

        using var enumerator = collection.GetEnumerator();

        while (enumerator.MoveNext()) // każdy batch zaczyna się od aktualnego elementu
        {
            var batch = new List<T>(capacity: size)
            {
                enumerator.Current
            };

            // próbujemy dobrać kolejne elementy aż do limitu batcha
            for (var i = 1; i < size && enumerator.MoveNext(); i++)
            {
                batch.Add(enumerator.Current);
            }

            yield return batch; // zwracamy gotową grupę (leniwe)
        }
    }

    // -----------------------------------------------------------
    // SLIDING WINDOW (okno przesuwne)
    // -----------------------------------------------------------
    // Zamiast brać elementy porcjami bez powrotu, okno przesuwa się po jednym elemencie:
    // Example, size=3:
    // [1,2,3,4,5] => [1,2,3], [2,3,4], [3,4,5]
    public static IEnumerable<T[]> SlidingWindow<T>(
        this IEnumerable<T> collection,
        int size)
    {
        if (size < 1)
            throw new ArgumentException(nameof(size));

        var window = new Queue<T>(); // przechowujemy aktualne okno

        using var enumerator = collection.GetEnumerator();

        while (enumerator.MoveNext())
        {
            window.Enqueue(enumerator.Current);
            if (window.Count > size)
                window.Dequeue(); // przesuwamy okno

            if (window.Count == size)
                yield return window.ToArray(); // emitujemy snapshot
        }
    }

    // -----------------------------------------------------------
    // Wykrywanie rosnących sum okien (5→2→porównanie sum)
    // -----------------------------------------------------------
    public static IEnumerable<IEnumerable<int>> FindSlidingWindowsWithRisingSum(this IEnumerable<int> sequence)
    {
        return sequence
            .SlidingWindow(5)                         // najpierw okna po 5 elem.
            .Select(w => (Window: w, Sum: w.Sum()))   // obliczamy sumę każdego okna
            .SlidingWindow(2)                         // teraz przesuwamy okno na "pary" okien
            .Where(w => w[0].Sum < w[1].Sum)          // interesuje nas wzrost sumy
            .Select(w => w[1].Window);                // zwracamy to drugie, "większe"
    }

    // -----------------------------------------------------------
    // Szukanie okien 4-elementowych, które mają duplikaty
    // -----------------------------------------------------------
    public static IEnumerable<IEnumerable<int>> FindSlidingWindowsWithDuplicates(this IEnumerable<int> sequence)
    {
        return sequence
            .SlidingWindow(4)
            .Where(window => window.Distinct().Count() < window.Length); // jeśli mniej unikalnych niż elementów → są duplikaty
    }

    // -----------------------------------------------------------
    // Najczęstsze trigramy (ciągi 3 liter)
    // -----------------------------------------------------------
    public static IEnumerable<string> FindMostCommonTrigrams(this string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length < 3)
            return [];

        var trigrams = text
            .Where(char.IsLetter)
            .Select(char.ToLowerInvariant)
            .SlidingWindow(3)
            .Select(chars => new string([.. chars]))   // budujemy string
            .GroupBy(trigram => trigram)
            .Select(group => new { Trigram = group.Key, Count = group.Count() });

        var maxCount = trigrams.Max(group => group.Count); // znajdujemy najwyższą częstość

        return trigrams
            .Where(group => group.Count == maxCount)  // filtrujemy tylko te najczęstsze
            .Select(group => group.Trigram);
    }

    // -----------------------------------------------------------
    // Najdłuższy ciąg powtarzających się wartości
    // -----------------------------------------------------------
    public static (int start, int end, int value) LongestSequence(this IEnumerable<int> sequence)
    {
        return sequence.Fold(
            seed: (
                Start: 0,
                End: 0,
                Value: sequence.First(),
                CurrentStart: 0,
                CurrentEnd: 0,
                CurrentValue: sequence.First()
            ),
            func: (acc, elem) =>
            {
                if (elem == acc.CurrentValue)
                {
                    var length = acc.End - acc.Start + 1;
                    var currentLength = acc.CurrentEnd - acc.CurrentStart + 1;

                    if (currentLength > length)
                    {
                        acc.Start = acc.CurrentStart;
                        acc.End = acc.CurrentEnd;
                        acc.Value = acc.CurrentValue;
                    }
                }
                else
                {
                    acc.CurrentStart = acc.CurrentEnd;
                    acc.CurrentValue = elem;
                }

                acc.CurrentEnd++;
                return acc;
            },
            resultSelector: acc => (acc.Start, acc.End, acc.Value)
        );
    }

    // -----------------------------------------------------------
    // Statystyki: min, max, średnia, odchylenie std.
    // -----------------------------------------------------------
    public static (int min, int max, double average, double standardDeviation) ComputeStatistics(this IEnumerable<int> source)
    {
        if (source == null || !source.Any())
            throw new ArgumentException(nameof(source));

        var result = source.Fold(
            seed: (
                Min: int.MaxValue,
                Max: int.MinValue,
                Sum: 0L,
                SumOfSquares: 0L,
                Count: 0
            ),
            func: (acc, x) => (
                Min: Math.Min(acc.Min, x),
                Max: Math.Max(acc.Max, x),
                Sum: acc.Sum + x,
                SumOfSquares: acc.SumOfSquares + (long)x * x,
                Count: acc.Count + 1
            ),
            resultSelector: acc =>
            {
                var avg = (double)acc.Sum / acc.Count;
                var variance = (double)acc.SumOfSquares / acc.Count - avg * avg;
                return (acc.Min, acc.Max, avg, Math.Sqrt(Math.Max(0, variance)));
            }
        );

        return result;
    }
}