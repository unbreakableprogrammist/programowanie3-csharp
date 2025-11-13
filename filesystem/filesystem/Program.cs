using FileStream file = File.Open("test.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
file.Close(); 
// czytanie calego tekstu 
string content = File.ReadAllText("test.txt");
Console.WriteLine(content);

// tworzy/nadpsuje caly tekst , ktory jest w contencie
File.WriteAllText("test2.txt", content);

// dodawanie do pliku 
const string logFile = "log.txt";
for (int i = 0; i < 100; i++)
{
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


