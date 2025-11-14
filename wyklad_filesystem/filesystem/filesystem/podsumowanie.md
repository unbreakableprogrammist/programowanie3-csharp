
📘 Strumienie w C# — pełne kompendium praktyczne i techniczne

🧩 Spis treści
1.	Wprowadzenie — czym jest strumień?
2.	Architektura strumieni (warstwowy model)
3.	Stream — klasa bazowa
4.	FileStream — bezpośredni dostęp do plików
5.	BufferedStream — buforowanie operacji I/O
6.	MemoryStream — strumień w RAM
7.	NetworkStream — komunikacja sieciowa
8.	GZipStream — kompresja i dekompresja
9.	CryptoStream — szyfrowanie
10.	Adaptery tekstowe: StreamReader/StreamWriter
11.	Adaptery binarne: BinaryReader/BinaryWriter
12.	Fasada File i Directory
13.	Jak łączyć strumienie? (model kanapki)
14.	Read vs ReadByte vs CopyTo
15.	Zasady doboru strumieni — co kiedy stosować
16.	Najczęstsze błędy i pułapki
17.	Przykłady zaawansowane
18.	Podsumowanie master-level

⸻

1️⃣ Wprowadzenie — czym jest strumień?

Strumień (Stream) jest abstrakcją jednowymiarowego kanału danych,
z którego bajty:
•	wypływają (Write)
•	wpływają (Read)

Strumień NIE wie:
•	skąd pochodzą dane,
•	dokąd płyną dane.

To tylko interfejs.

Strumień może reprezentować:
•	plik,
•	kompresję,
•	sieć,
•	szyfrowanie,
•	bufor RAM,
•	konsolę,
•	pamięć wirtualną.

To fundament całego .NET I/O.

⸻

2️⃣ Architektura strumieni (warstwowy model)

Najważniejsza idea:

Strumienie budujemy warstwowo, „jak kanapkę”.
Każda warstwa dodaje funkcję: kompresję, buforowanie, szyfrowanie, login, itd.

Na przykład:

[ StreamWriter ]
↓ (tekst)
[ BufferedStream ]
↓ (bufferowanie)
[ FileStream ]
↓ (dysk)

Inny przykład:

[ GZipStream ]
↓ (kompresja)
[ BufferedStream ]
↓ (buforowanie)
[ NetworkStream ]
↓ (socket / sieć)

Warstwy wykonują swoje operacje liniowo w dół.

⸻

3️⃣ Stream — klasa bazowa

Stream zapewnia ogólny zestaw metod:

❗ Wszystkie strumienie używają tych samych metod:
•	int Read(byte[] buffer, int offset, int count)
•	int ReadByte()
•	void Write(byte[] buffer, int offset, int count)
•	void WriteByte(byte b)
•	long Seek(long offset, SeekOrigin origin)
•	void Flush()
•	bool CanRead, CanWrite, CanSeek
•	void Close() / Dispose()

Stream nie robi nic konkretnego.
Dopiero klasy pochodne nadają mu znaczenie.

⸻

4️⃣ FileStream — strumień do plików

To prawdziwy strumień, który:
•	współpracuje z systemem operacyjnym,
•	otwiera pliki,
•	czyta i zapisuje na dysk,
•	utrzymuje uchwyt systemowy.

Kiedy używać FileStream?
•	praca z dużymi plikami,
•	kopiowanie blokami,
•	dopisywanie,
•	praca binarna,
•	streaming danych.

Przykład:

using FileStream fs = new FileStream(
"dane.bin",
FileMode.OpenOrCreate,
FileAccess.ReadWrite);

Nie używaj FileStream do tekstu → użyj StreamWriter.

⸻

5️⃣ BufferedStream — ekstremalne przyspieszenie I/O

BufferedStream ma jedno zadanie:

Minimalizować ilość operacji I/O (systemowych), łącząc wiele Read/Write w jeden duży blok.

Jak działa:
•	Odczyt: przy pierwszym Read() → pobiera np. 20 KB do RAM
•	Kolejne Read*() → wydają bajty z bufora
•	Zapis: mały Write → trafia do bufora i dopiero potem na dysk

Gdzie używać:
•	przy ReadByte()
•	przy kompresji
•	przy szyfrowaniu
•	przy plikach tekstowych (Writer)
•	przy dużych plikach

Przykład:

using BufferedStream bs = new BufferedStream(fs, 20000);


⸻

6️⃣ MemoryStream — strumień w RAM

MemoryStream umożliwia pracę na danych w pamięci:
•	superszybki,
•	idealny dla testów,
•	dobry dla konwersji (np. PDF → byte[]),
•	używany przy serializacji.

Przykład:

using MemoryStream ms = new MemoryStream();
ms.Write(data);
byte[] result = ms.ToArray();


⸻

7️⃣ NetworkStream — strumień TCP

Reprezentuje połączenie sieciowe.
Pracujesz na nim jak na pliku:

NetworkStream ns = client.GetStream();
ns.Read(buffer);
ns.Write(buffer);

Ale:
•	NIE można seekować,
•	operacje mogą być opóźnione,
•	warto dodać BufferedStream.

⸻

8️⃣ GZipStream — kompresja i dekompresja

GZipStream:
•	kompresuje lub dekompresuje wszystko, co przez niego przepływa,
•	nie zna plików — to tylko filtr strumieniowy.

Przykład kompresji:

using var fs = File.Create("a.gz");
using var bs = new BufferedStream(fs);
using var gz = new GZipStream(bs, CompressionMode.Compress);

gz.Write(data);

Przykład dekompresji:

using var fs = File.OpenRead("a.gz");
using var gz = new GZipStream(fs, CompressionMode.Decompress);
gz.Read(buffer);


⸻

9️⃣ CryptoStream — szyfrowanie danych

Zasada ta sama co GZipStream, ale dodaje szyfrowanie.

Pipeline:

plaintext → CryptoStream → FileStream


⸻

🔟 StreamReader / StreamWriter — adapter tekstowy

FileStream to bajty.
A my potrzebujemy stringów.

StreamReader

convertuje bajty → tekst

StreamWriter

konwertuje tekst → bajty

Obsługuje kodowanie:
•	UTF-8 (domyślne),
•	UTF-16,
•	ASCII.

Przykład:

using StreamWriter sw = new StreamWriter("tekst.txt");
sw.WriteLine("Ala ma kota");

Nigdy nie pisz tekstu bezpośrednio przez FileStream.

⸻

1️⃣1️⃣ BinaryReader / BinaryWriter — binaria wysokiego poziomu

Najlepsze do:
•	zapis liczb,
•	zapis struktur,
•	formaty binarne.

Przykład:

BinaryWriter bw = new BinaryWriter(fs);
bw.Write(123);     // int
bw.Write(3.14);    // double
bw.Write(true);    // bool


⸻

1️⃣2️⃣ Fasada File i Directory

Metody typu:
•	File.ReadAllBytes
•	File.ReadAllLines
•	File.WriteAllLines
•	File.AppendAllText

To proste wrappery:
•	otwierają strumień,
•	wykonują operację,
•	zamykają strumień.

Używaj tylko do małych plików.

⸻

1️⃣3️⃣ Łączenie strumieni — model kanapki

Najważniejsza zasada:

Strumień najwyżej → logika
Strumień środkowy → optymalizacja
Strumień najniżej → fizyczne I/O

Najlepszy pipeline:

Writer (lub GZip/Crypto)   ← najbliżej Twojego kodu
↓
BufferedStream             ← optymalizuje
↓
FileStream                 ← zapis fizyczny


⸻

1️⃣4️⃣ Read vs ReadByte vs CopyTo — kiedy stosować?

ReadByte()
•	proste, wolne
•	dobre gdy masz BufferedStream
•	czyta 1 bajt

Read()
•	najbardziej sensowne
•	czyta BLOKI bajtów (np. 4 KB, 64 KB)

CopyTo()
•	najwygodniejsze
•	kopiuje cały strumień za Ciebie
•	używa bufora 80 KB

Przykład:

input.CopyTo(output);


⸻

1️⃣5️⃣ Zasady doboru strumienia — co kiedy używać

Sytuacja	Strumień
mały plik	File.ReadAll*
duży plik, binary	FileStream + BufferedStream
tekst	StreamReader/Writer
kompresja	GZipStream + BufferedStream
szyfrowanie	CryptoStream + BufferedStream
RAM	MemoryStream
sieć	NetworkStream + BufferedStream


⸻

1️⃣6️⃣ Najczęstsze błędy studentów

❌ „BufferedStream czyta plik przy konstruktorze”

Nie. Bufer jest napełniany dopiero przy pierwszym odczycie.

❌ „GZipStream kompresuje plik”

Nie. Kompresuje dane, które przepływają przez stream.

❌ „mogę pisać tekst przez FileStream”

Nie. Musisz używać StreamWriter.

❌ „ReadByte jest szybkie”

Nie. Jest turbo wolne bez BufferedStream.

❌ „CopyTo kopiuje cały plik na raz”

Nie. Kopiuje blokami 80 KB.

⸻

1️⃣7️⃣ Przykłady zaawansowane

🔹 Szybkie kopiowanie pliku 1 GB

byte[] buf = new byte[64 * 1024];

using var fr = new BufferedStream(File.OpenRead("a.bin"));
using var fw = new BufferedStream(File.Create("b.bin"));

int n;
while ((n = fr.Read(buf, 0, buf.Length)) > 0)
fw.Write(buf, 0, n);

🔹 Kompresja dużego pliku do GZip

using var input = new BufferedStream(File.OpenRead("video.raw"));
using var output = new BufferedStream(File.Create("video.raw.gz"));
using var gz = new GZipStream(output, CompressionMode.Compress);

input.CopyTo(gz);

🔹 Czytanie linii z ogromnego pliku

using var sr = new StreamReader(new BufferedStream(File.OpenRead("log.txt")));
while (!sr.EndOfStream)
{
string line = sr.ReadLine();
// ...
}


⸻

1️⃣8️⃣ Podsumowanie master-level
1.	FileStream → prawdziwe I/O, wolne, precyzyjne
2.	BufferedStream → klucz do szybkości
3.	StreamReader/Writer → tekst
4.	BinaryReader/Writer → liczby
5.	GZipStream → kompresja
6.	MemoryStream → RAM
7.	CopyTo() → najwygodniejsza operacja kopiowania
8.	Read() (blokowe) → najlepsza metoda do dużych danych
9.	ReadByte() → tylko z BufferedStream
10.	Warstwowy model strumieni → fundament

