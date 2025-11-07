using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using tasks.Databases;

namespace tasks;

/// <summary>
/// Here you should implement various database query methods for the IMovieDatabase interface.
/// </summary>
///
/// Każde wyrażenie LINQ zwraca IEnumerable<T> (lub IQueryable<T> jeśli źródłem jest baza).
///Czyli rezultatem LINQ jest zawsze sekwencja, czyli obiekt, po którym można iterować.
public static class DatabaseQueries
{
    public static void RunQueries(this IMovieDatabase movieDatabase)
    {
        movieDatabase.ActorsFromFantasyMovies();
        movieDatabase.LongestMovieByGenre();
        movieDatabase.HighRatedMoviesWithCast();
        movieDatabase.DistinctRolesCountPerActor();
        movieDatabase.RecentMoviesWithAverageRating();
        movieDatabase.AverageRatingByGenre();
        movieDatabase.ActorsWhoNeverPlayedInThriller();
        movieDatabase.Top3MoviesByRatingCount();
        movieDatabase.MoviesWithoutRatings();
        movieDatabase.MostVersatileActors();
    }

    public static void ActorsFromFantasyMovies(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        /*
            Najpierw wybieramy tylko filmy z gatunku Fantasy:
                movies.Where(m => m.Genre == Genre.Fantasy)

            Następnie łączymy je z obsadą (casts) po MovieId = Id filmu:
                casts.Join(...)

            Z tego połączenia bierzemy ActorId,
            czyli dostajemy listę aktorów grających w filmach fantasy:
                (cast, mov) => cast.ActorId
            to nam zwroci taka pojedyncze rekordy , tak jak to wygladalo w accesie

            Distinct() usuwa duplikaty — bo jeden aktor mógł grać w kilku filmach fantasy.

            Następnie ponownie robimy Join, tym razem z tabelą aktorów (actors),
            po ActorId = actor.Id, żeby zamienić identyfikatory na imiona aktorów:
                .Join(actors, actorId => actorId, actor => actor.Id, ...)

            Ostatecznie zwracamy same nazwy aktorów:
                (actorId, actor) => actor.Name
        */
            
        var queryResult =  casts.Join(movies.Where(m => m.Genre == Genre.Fantasy),
            cast=>cast.MovieId,
            mov=>mov.Id,
            (cast,mov)=>cast.ActorId)
            .Distinct()
            .Join(actors,
                actorId=>actorId,
                actor=>actor.Id,
                (actorId,actor) =>actor.Name
            ).ToList();
        

        Console.WriteLine("Actors From Fantasy Movies");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    /*
         Grupujemy filmy względem gatunku:
             movies.GroupBy(m => m.Genre)
         czyli dostajemy grupy postaci:
             Genre -> lista filmów w tym gatunku

         Następnie dla każdej grupy wybieramy jeden rekord — ten,
         który ma największą długość (DurationMinutes):
             group.MaxBy(movie => movie.DurationMinutes)

         Select tworzy wynikowy obiekt z dwoma polami:
         - Genre = group.Key (czyli nazwa gatunku)
         - Movie = najdłuższy film w tym gatunku
     */
    public static void LongestMovieByGenre(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = movies
            .GroupBy(m => m.Genre)
            .Select(group => new 
                {
                    Genre = group.Key,
                    Movie = group.MaxBy(movie => movie.DurationMinutes)
                }).ToList();

        Console.WriteLine("Longest Movie By Genre");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void HighRatedMoviesWithCast(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = ratings
            .GroupBy(r => r.MovieId)                      // Grupujemy oceny według Id filmu:
                                                           //   group.Key = MovieId
                                                           //   group = sekwencja Rating dla tego MovieId
                                                           // Przykład (logicznie):
                                                           //   Key=1 → [(Id=1,MovieId=1,Score=8), (Id=2,MovieId=1,Score=9)]

            .Select(group => new                           // Dla każdej grupy budujemy obiekt (Id Filmu , srednia ocen) 
            {
                MovieId = group.Key,                       //   Id filmu (klucz grupy)
                Average = group.Average(r => r.Score)      //   średnia ocena tego filmu
            })

            .Where(x => x.Average > 8)                     // Zostawiamy tylko filmy o średniej ocenie > 8
            .Join(movies,                                  // Łączymy z tabelą filmów (INNER JOIN), bo may tabele obiektow select i laczymy po kluczu z movie Id
                  rat => rat.MovieId,                      //   klucz po stronie średnich ocen: MovieId
                  mov => mov.Id,                           //   klucz po stronie filmów: Id
                  (rat, mov) => new                        //   wynikowo chcemy dostac rekord rzeczy z tabeli ( jako klucz) , srednia jako wartosc
                  {
                      Movie = mov,                         //     pełny rekord filmu , czyli bierzemy calosc z rekord filmu , czyli tytul idt
                      rat.Average                          //     średnia ocena (przenosimy z poprzedniego etapu) , dodajemy
                  })

            .GroupJoin(casts,                              // LEFT JOIN: do każdego filmu dobieramy jego obsadę (może być pusta)
                       mov => mov.Movie.Id,                //   klucz po stronie filmów: Movie.Id
                       cast => cast.MovieId,               //   klucz po stronie obsady: Cast.MovieId
                       (mov, cast) => new             //   wynikowo dostajemy rekord ( wszystko z Movie , srednia ocen , i dodajemy nowy wiersz id aktorow ktorzy graja w tym filmie
                       {
                           mov.Movie,                      //     film
                           mov.Average,                    //     średnia ocena
                           CastIDS = cast             //     sekwencja dopasowanych wpisów obsady (Cast ...)
                               .Select(c => c.ActorId)     //     rzutujemy do samych ActorId (IEnumerable<int>)
                       })

            .Select(x => new                               // Dla każdego filmu budujemy wynik końcowy film , srednia , i wybieramy imiona aktorow, i wrzucamy ich do listy
            {
                x.Movie,                                   //   film
                x.Average,                                 //   średnia ocena
                Cast = x.CastIDS                           //   zamieniamy ActorId → pełne rekordy aktorów
                    .Join(actors,                          //   (INNER JOIN po ActorId)
                          actorId => actorId,              //     klucz po stronie ActorId (outer): int
                          actor   => actor.Id,             //     klucz po stronie aktorów: Actor.Id
                          (actorId, actor) => actor.Name)       //     projekcja: bierzemy pełny rekord aktora
                    .ToList()                              //   materializacja listy aktorów (żeby ładnie zserializować)
            })
            .ToList();                                     // Materializacja całej odpowiedzi (IEnumerable → List)
      

        Console.WriteLine("High Rated Movies With Cast");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void DistinctRolesCountPerActor(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = actors.GroupJoin(casts,
            actor => actor.Id,
            cast => cast.ActorId,
            (actor,cast) => new
            {
                Actor = actor.Name,
                Roles = cast.Select(c => c.Role).Distinct().Count(),
            }
            ).OrderBy(x => x.Roles)
            .ToList();

        Console.WriteLine("Distinct Roles Count Per Actor");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void RecentMoviesWithAverageRating(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = movies.Where(m => m.Year > DateTime.Now.Year - 5)
                .GroupJoin(ratings,
                    mov => mov.Id,
                    rat => rat.MovieId,
                    (mov,rat) => new
                    {
                        Movie = mov,
                        Avarage = rat.Any() ? rat.Average(r => r.Score) : 0.00
                    }
                    )
                .OrderByDescending(x => x.Avarage)
                .ToList();
            

        Console.WriteLine("Recent Movies With Average Rating");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void AverageRatingByGenre(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = movies.Join(ratings,   // dostajemy cos typu fantasy : 8 , fantasy : 9  , thriller : 8
            mov => mov.Id,
            rat => rat.MovieId,
            (mov, rat) => new  // tworzymy nowe pole , jedna rzecz z movie druga z rating
            {
                Genre = mov.Genre,
                Rating = rat.Score,
            }
            )
            .GroupBy(m => m.Genre)   // grupujemy by genre , czyli teraz mamy klucz , { ratings} , np fantastyka : { 8,9,10,1} 
            .Select(group => new  // wybieramy stamtad te wszytskie grupy , czyli fantastyka : {liczba ocen } 
            {
                Genre = group.Key,
                AvarageRating = group.Select(r => r.Rating).Average(),   // tworzymy zmienna z sredniej ocen
            }).ToList();
        

        Console.WriteLine("Average Rating By Genre");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void ActorsWhoNeverPlayedInThriller(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var thrillerMovieIds = movies.Where(m => m.Genre == Genre.Thriller).Select(m => m.Id).ToHashSet(); // w hashsecie trzymamy id tych movies ktore sa thrileramu
        var thrillerActorIds =
            casts.Where(c => thrillerMovieIds.Contains(c.MovieId)).Select(c => c.ActorId).ToHashSet();  // w tym hashsecie trzymamy te id aktorow ktorzy zagrali w thrillerze , po prostu id filmu musialo byc w hashsecie
        
        var queryResult = actors
            .Where(actor => !thrillerActorIds.Contains(actor.Id)) // tu bierzemy id tych aktorow , ze nie ma ich w hashsecie z aktorami ktorzy tam grali 
            .ToList();

        Console.WriteLine("Actors Who Never Played In Thriller");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void Top3MoviesByRatingCount(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        
        var queryResult = ratings.GroupBy(r => r.MovieId)
            .Select(r => new
            {
                movieId = r.Key,
                howMany = r.Count()
            })
            .OrderByDescending(x => x.howMany)
            .Join(movies,
                rat => rat.movieId,
                mov=> mov.Id,
                (rat,mov) => mov)
            .Take(3)
            .ToList();
        
        /*
        var queryResult = ratings
            .GroupBy(rating => rating.MovieId)
            .OrderByDescending(group => group.Count())
            .Take(3)
            .Select(group => movies.First(movie => movie.Id == group.Key))
            .ToList();
            */

        Console.WriteLine("Top 3 Movies By Rating Count");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    /*
        MoviesWithoutRatings — cel:
        znaleźć filmy, które nie mają żadnej oceny.

        Krok 1: budujemy zbiór (HashSet<int>) identyfikatorów filmów, które pojawiły się w tabeli Ratings:
            var moviesWithRatings = ratings.Select(r => r.MovieId).ToHashSet();
            // HashSet daje O(1) dla Contains – wydajnie przy dużych danych.

        Krok 2: filtrujemy listę filmów – zostawiamy te, których Id NIE MA w zbiorze:
            var queryResult = movies.Where(m => !moviesWithRatings.Contains(m.Id)).ToList();

        Intuicja tabelaryczna:
            MoviesWithRatings = { Id filmów występujących w Ratings }
            Wynik = Movies \ MoviesWithRatings

        Typowe potknięcia, na które uważamy:
        - GroupBy tutaj nie jest potrzebne; szybciej i prościej jest brać same klucze .Select(r => r.MovieId).
        - Nazewnictwo: lokalne zmienne zwykle camelCase → moviesWithRatings (nie: MoviesWithRatings).
    */
    public static void MoviesWithoutRatings(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var MoviesWithRatings = ratings.Select(r => r.MovieId).ToHashSet();
        var queryResult = movies.Where(r => !MoviesWithRatings.Contains(r.Id)).ToList();

        Console.WriteLine("Movies Without Ratings");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    
    /*
        MostVersatileActors — cel:
        policzyć, w ILU RÓŻNYCH GATUNKACH grał każdy aktor (liczymy DISTINCT po Genre),
        a potem posortować malejąco (najbardziej "wszechstronni" na górze).

        Krok 1: łączymy obsadę z filmami, żeby do każdej roli dołączyć gatunek filmu:
            casts.Join(movies,
                cast => cast.MovieId,
                mov  => mov.Id,
                (cast, mov) => new { cast.ActorId, mov.Genre })

            Wynik: sekwencja krotek (ActorId, Genre) – po jednej na KAŻDĄ rolę w filmie.

        Krok 2: grupujemy po aktorze:
            .GroupBy(x => x.ActorId)

            Każda grupa to: ActorId → lista (ActorId, Genre, ...) dla wszystkich jego ról.

        Krok 3: dla każdej grupy liczymy "liczbę unikalnych gatunków":
            .Select(g => new {
                ActorId    = g.Key,
                GenreCount = g.Select(x => x.Genre).Distinct().Count()
            })

            Dlaczego Distinct? Bo ten sam aktor mógł mieć wiele ról w filmach tego samego gatunku –
            interesuje nas liczba różnych gatunków, nie liczba ról.

        Krok 4: zamieniamy ActorId na pełny rekord aktora (Join z actors), żeby mieć nazwisko/imie/etc.:
            .Join(actors,
                agg => agg.ActorId,
                a   => a.Id,
                (agg, a) => new { Actor = a, agg.GenreCount })

        Krok 5: sortujemy malejąco po liczbie gatunków i materializujemy:
            .OrderByDescending(x => x.GenreCount)
            .ToList()

        Uwagi:
        - Jeśli chcesz wyeliminować ewentualne duplikaty ról PRZED liczeniem gatunków,
          możesz dodać Distinct po (ActorId, Genre): np. .Select(t => (t.ActorId, t.Genre)).Distinct()
          – ale obecny Distinct na Genre w ramach aktora zazwyczaj wystarcza.
        - Gdy chcesz także tie-break (np. po nazwisku), dodaj ThenBy(x => x.Actor.Name).

        Intuicja krokowa (metoda naukowa):
        1) cast ⨝ movies → (ActorId, Genre) dla każdej roli,
        2) GroupBy(ActorId) → zbiory ról per aktor,
        3) Distinct(Genre).Count() → liczba unikalnych gatunków,
        4) ⨝ actors → dane aktora,
        5) sortowanie malejąco po wszechstronności.

    */
    public static void MostVersatileActors(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = casts.Join(movies,
                cast => cast.MovieId,
                mov => mov.Id,
                (cast, mov) => new
                {
                    cast.ActorId,
                    mov.Genre
                })
            .GroupBy(m => m.ActorId)
            .Select(r => new
            {
                ActorId = r.Key,
                GenreCount = r.Select(r => r.Genre).Distinct().Count(),
            })
            .Join(actors,
                act => act.ActorId,
                actor => actor.Id,
                (act,actor)=>new
                {
                    Actor = actor,
                    act.GenreCount
                })
            .OrderByDescending(a => a.GenreCount)
            .ToList();
            
        Console.WriteLine("Most Versatile Actors");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void DisplayQueryResults<T>(T query)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter());

        var json = JsonSerializer.Serialize(query, options);

        Console.WriteLine(json);
    }
}