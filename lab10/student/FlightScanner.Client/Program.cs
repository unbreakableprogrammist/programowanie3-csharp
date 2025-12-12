using System.Diagnostics;
using System.Net.Http.Json;
using FlightScanner.Common.Dtos;

namespace FlightScanner.Client;

using System;
using System.Collections.Generic;
using System.Net.Http;

public record AggregatedFlightOffer(
    string ProviderName,
    string FlightId,
    string Origin,
    string Destination,
    decimal Price
);

public class Program
{
    public const int TimeoutMs = 3000;
    // to jest wywolanie obiektu typu HttpClient i wywolanie na nim ustawienie pola
    private static readonly HttpClient httpClient = new() // new to jest od razu konstruktor bezparametrowy
    {
        BaseAddress = new Uri("http://localhost:5222")
    };

    public static async Task RunFlightScranner()
    {
        var koniec = new CancellationTokenSource(TimeoutMs); //tworzymy sobie taki alarm ze za 3 sekundy koniec programu 
        IProgress<string> progress = new Progress<string>(Console.WriteLine); // to takie cudo do przekazywania jak nam idzie ( moze byc cos innego niz console.writeline , np przekazywanie cos do innego watku
        /*
         PartnerAirlineDto - skad ten typ , otoz rekor zapisany gdzies w flightscanner common:
         public record PartnerAirlineDto(
               string Id,
               string Name,
               string Endpoint - endpoint do juz konkretnej lini 
           );
         */
        List<PartnerAirlineDto>? providers;  // lista lini lotniczych ktore cos oferuja 
        try
        {
            var pytaj = "/api/providers"; // to jest po porstu miejsce do ktorego bedziemy wysylac zapytania
            progress.Report($"[Phase 1] Fetching providers from {pytaj}..."); // raport na console
            /*
             teraz sobie wywolujemy na obiekcie httpClient metode GetFromJsonAsync bierze Jsona ze strony i 
             dosdososwuje go pod klucz czyli pod liste obiektow typu PartnerAirlineDto, no i ten pytaj tam gdzie chcemy pytac i timer
             */
            providers = await httpClient.GetFromJsonAsync<List<PartnerAirlineDto>>(pytaj,koniec.Token);
            // teraz mamy juz liste providerow
            // skopiowane z rozwiazania , ale chb nie potrzeba wyjasniac ( nic nie pobralismy)
            if (providers == null || providers.Count == 0)
            {
                progress.Report("[ERROR] No providers found, exiting...");
                return;
            }
            progress.Report($"[Phase 1] Found {providers.Count} providers.");
        }catch (OperationCanceledException)
        {
            // jesli skonczyl sie czas
            progress.Report($"[TIMEOUT] Failed to fetch providers within {TimeoutMs}ms.");
            return;
        }
        catch (Exception ex)
        {
            // jakis inny wyjatek
            progress.Report($"[ERROR] Failed to fetch providers: {ex.Message}");
            return;
        }
        // teraz mamy liste providerow i ich dane
        progress.Report("\n[Phase 2] Fetching all flights concurrently...");
        // teraz plan jest taki zeby odpalic sobie taski na zczytywanie danych 
        var taski = new List<Task<ProviderResponseDto?>>(); // lista taskow ktore zwracaja jaka strukture:
        /*
         public record ProviderResponseDto(
               string ProviderName - nazwa dostawcy, który zwrócił dane (np. "Reliable Air")
               List<FlightOfferDto> Flights - lista ofert lotów udostępnionych przez tę linię.
                                             Każda oferta jest obiektem typu FlightOfferDto.
           );
         */
        foreach (var provider in providers)
        {
            // nie musimy robic task run bo 
            taski.Add(GetFlightsFromProviderAsync(provider, koniec.Token, progress));
        }

    }

    public static async Task<ProviderResponseDto?> GetFlightsFromProviderAsync(
        PartnerAirlineDto? provider,
        CancellationToken koniec,
        IProgress<string> progress)
    {
        try
        {
            progress.Report($"\t[START] Querying {provider.Name} ({provider.Endpoint})");
            // odpalamy asynchronicznie ( czyli odpalamy i czekamy na wynik ) funckje GetAsync 
            // get async - to funckcja ktora jest asynchroniczna i zwraca odpowiedz http ok/json/404
            // wazne ze tutaj mozemy zauwazyc ze nasza funkcja getFlightFromProvider sie zatrzymuje ale ten for ktory nas wywolal dziala
            var response = await httpClient.GetAsync(provider.Endpoint, koniec);
            if (!response.IsSuccessStatusCode)
            {
                progress.Report($"\t[FAIL] {provider.Name} returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase})");
                return null;
            }
            // tu dopisz 

        }
    }
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting...");
        var stopwatch = Stopwatch.StartNew(); // pomiar czasu dla nas 
        try
        {
            await RunFlightScranner();  // zatrzymujemy maina az nam cos zwroci RunFlightScanner
        }
        catch(Exception ex) // jesli RunFlightScanner rzuci jakikoliwek wyjatek -> zapisz do zmiennej ex i chodzi tutaj 
        {
            Console.ForegroundColor = ConsoleColor.Red; // zmieniamy kolor konsoli na czerowny 
            Console.WriteLine(ex.Message);
            Console.ResetColor();

        }
        // skopiowane z wykladu , jakies nudy z wypisywaniem
        stopwatch.Stop();
        Console.WriteLine("\n--- Aggregation Complete ---");
        Console.WriteLine($"Total operation time: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static void DisplayTop10Flights(List<AggregatedFlightOffer> offers)
    {
        if (offers.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No flight offers could be aggregated.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("\n--- Top 10 Cheapest Flights Found ---");
        Console.ForegroundColor = ConsoleColor.Green;

        var header = $"{"Price",-12} {"Provider",-25} {"Flight",-10} {"Route",-10}";
        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));

        foreach (var offer in offers)
        {
            var price = $"{offer.Price:C}";
            var route = $"{offer.Origin} -> {offer.Destination}";
            Console.WriteLine(
                $"{price,-12} {offer.ProviderName,-25} {offer.FlightId,-10} {route,-10}"
            );
        }

        Console.ResetColor();
    }
}