🚀 Jak działa cały ten program — najprościej jak się da

1️⃣ Mamy sobie „paczke danych” PriceChangedEventArgs

To jest po prostu mały obiekt, który trzyma:
•	starą cenę
•	nową cenę

Nic więcej. Paczka, którą wysyłamy do ludzi, którzy chcą wiedzieć, że coś się zmieniło.

⸻

2️⃣ Product — to jest gość, który „nadaje sygnał” że cena się zmieniła

Masz obiekt Product.

Ustawiasz mu cenę.

On patrzy:

„ej, nowa cena ≠ stara cena? Jeśli tak, to muszę wszystkich powiadomić.”

Więc robi:
•	tworzy paczkę PriceChangedEventArgs (old → new)
•	odpala metodę OnPriceChanged
•	a ta metoda wywołuje event PriceChanged, który odpala wszystkie dopięte metody subskrybentów

Czyli Product wysyła sygnał:

„Halo, cena się zmieniła! Oto dane co i jak!”

⸻

3️⃣ Event PriceChanged to lista metod, które mają się wykonać

Wyobraź sobie listę:

[ HandlePriceChanged z Notifiera, UpdateDisplay z Display, ZapiszDoLogów ... ]

Każdy kto zrobi +=, dopina swoją metodę do tej listy.

Gdy event odpala:

➡️ wszystkie te metody lecą jedna po drugiej.

⸻

4️⃣ Subskrybent (Notifier) to po prostu ktoś, kto chce być powiadomiony

Notifier pisze sobie zwykłą metodę:

HandlePriceChanged(object sender, PriceChangedEventArgs e)

I mówi:

product.PriceChanged += HandlePriceChanged;

Czyli:

„Jak product powie że cena się zmieniła — zawołaj mnie.”

To wszystko.

⸻

5️⃣ Zmiana ceny = wywołanie wszystkich podpiętych metod

Przykład:

product.Price = 999;

Dzieje się:
1.	Product widzi zmianę → tworzy paczkę z danymi
2.	Product odpala OnPriceChanged
3.	OnPriceChanged odpala event
4.	Event wywołuje po kolei każdą metodę:
•	HandlePriceChanged z Notifiera
•	itd.

Notifier wtedy wypisuje:

„Cena Laptop zmieniła się z 1199.90 na 999.90”

⸻

6️⃣ Odpięcie (-=) usuwa metodę z listy

Czyli po tym:

product.PriceChanged -= notifier.HandlePriceChanged;

Notifier już NIC nie będzie wiedział o zmianach cen.

⸻

🔥 Najkrótsze podsumowanie w jednym zdaniu

Product wykrywa zmianę → robi paczkę danych → odpala event → event odpala wszystkie metody subskrybentów → przez to każdy kto się zapisał dostaje info o zmianie.

To wszystko.

