using FileStream file = File.Open("lorem.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
file.Close(); 
// czytanie calego tekstu 
string content = File.ReadAllText("lorem.txt");
Console.WriteLine(content);

// tworzy/nadpsuje caly tekst , ktory jest w contencie
File.WriteAllText("test2.txt", content);

// dodawanie do pliku 
const string logFile = "log.txt";
for (int i = 0; i < 100; i++)
{
    // append dodaje na koncu pliku 
    File.AppendAllText(logFile, $"[Info] This is a log entry no. {i} \n");
}

// czytamy , po separatorze ( "\n") 
string[] lines =  File.ReadAllLines(logFile);
foreach (var line in lines)
{
    Console.WriteLine(line);
}

// tworzy/nadpisuje plik lorem_lines , kazdy element tablicy = jedna linia 
File.WriteAllLines("lorem_lines.txt", lines);

// to samo co ze strintagmi tylko tutaj po prostu bierzemy bity  
//byte[] bytes = File.ReadAllBytes("lorem.bin");
//File.WriteAllBytes("lorem.bin.bak", bytes);

//przyklad lazy porgrammingu , bedziemy czytac te linie dopoku gdy zaczniemy isc forem po lines2
IEnumerable<string> lines2 = File.ReadLines("lorem.txt");
File.AppendAllLines("lorem.txt.bak", lines2);

foreach (var line in lines2)
{
    Console.WriteLine(line);
}