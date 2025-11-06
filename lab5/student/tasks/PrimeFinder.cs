namespace tasks;

/// <summary>
/// Here you should implement the Sieve of Eratosthenes algorithm to find all prime numbers up to a given upper bound.
/// </summary>
public static class PrimeFinder
{
    public static IEnumerable<int> SieveOfEratosthenes(int upperBound)
    {
        if (upperBound <= 0)
        {
            yield break;
        }
        bool[] sito = new bool[upperBound + 7]; // false = pierwsza , true = nie pierwsza
        sito[0] = true;
        sito[1] = true;
        for (int i = 0; i <= upperBound; i++)
        {
            if (sito[i] == false)
            {
                for (int j = i * i; j <= upperBound; j += i)
                {
                    sito[j] = true;
                }
            }

            if (sito[i] == false)
            {
                yield return i;
            }
        }
    }
}