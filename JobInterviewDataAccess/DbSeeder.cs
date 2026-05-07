using JobInterviewCore.Entities;
using JobInterviewCore.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobInterviewDataAccess;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Topics.AnyAsync())
            return;

        // Temakorok
        var topics = new List<Topic>
        {
            new() { Name = "C# Alapok" },
            new() { Name = "OOP" },
            new() { Name = ".NET Keretrendszer" },
            new() { Name = "SQL es Adatbazisok" },
            new() { Name = "Design Patterns" },
            new() { Name = "Web Fejlesztes" },
            new() { Name = "Algoritmusok es Adatszerkezetek" }
        };

        context.Topics.AddRange(topics);
        await context.SaveChangesAsync();

        // Kerdesek valaszokkal
        var now = DateTime.UtcNow;
        var questions = new List<Question>
        {
            // C# Alapok
            new()
            {
                Text = "Mi a kulonbseg a value type es reference type kozott C#-ban?",
                Answer = "A value type (int, struct, enum) kozvetlenul tartalmazza az erteket es a stack-en tarolodik. A reference type (class, interface, delegate) egy referenciat (mutatot) tarol a heap-en levo objektumra. Value type masolasnal az ertek masolodik, reference type-nal a referencia.",
                TopicId = topics[0].Id, Difficulty = Difficulty.Easy, CreatedAt = now
            },
            new()
            {
                Text = "Magyarazd el a boxing es unboxing fogalmat!",
                Answer = "A boxing a value type objektumma (object) konvertalasa - ilyenkor az ertek a heap-re kerul egy object wrapperbe. Az unboxing ennek az ellentete: az object-bol visszakapjuk a value type-ot. Mindketto teljesitmenyigenyes, kerulendo gyakori hasznalat eseten.",
                TopicId = topics[0].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },
            new()
            {
                Text = "Mi a kulonbseg a string es StringBuilder kozott? Mikor melyiket erdemes hasznalni?",
                Answer = "A string immutable (megvaltoztathatatlan): minden modositas uj string objektumot hoz letre. A StringBuilder mutable: helyben modositja a szoveget. Sok string-muvelet (pl. ciklusban osszefuzes) eseten a StringBuilder sokkal hatekonyabb.",
                TopicId = topics[0].Id, Difficulty = Difficulty.Easy, CreatedAt = now
            },
            new()
            {
                Text = "Mik azok a nullable tipusok es mikor hasznaljuk oket?",
                Answer = "A nullable tipusok (int?, bool? stb.) lehetove teszik, hogy egy value type null erteket is felvehessen. Jelolese: Nullable<T> vagy T?. Adatbazisbol szarmazo adatoknal gyakori, ahol egy mezo lehet NULL. A HasValue es Value property-kkel, vagy a ?? (null-coalescing) operatorral hasznaljuk.",
                TopicId = topics[0].Id, Difficulty = Difficulty.Easy, CreatedAt = now
            },
            new()
            {
                Text = "Mi a kulonbseg a == operator es az Equals() metodus kozott?",
                Answer = "A == operator alapertelmezetten referencia-egyenloseget vizsgal reference type-oknal (kivetel string, ahol ertek-osszehasonlitast vegez). Az Equals() virtualis metodus, felulirhato. Value type-oknal mindketto erteket hasonlit. Az == feluldefinialhat operator overloading-gal, az Equals() override-dal.",
                TopicId = topics[0].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },

            // OOP
            new()
            {
                Text = "Mik az objektorientalt programozas negy alappillere?",
                Answer = "1) Egysegbezaras (Encapsulation): adatok es metodusok osszezarasa, hozzaferes-szabalyozas. 2) Oroklodes (Inheritance): osztalyok hierarchiaja, kod ujrafelhasznalas. 3) Polimorfizmus (Polymorphism): ugyanaz az interfesz, kulonbozo implementacio. 4) Absztrakcio (Abstraction): lenyeges jellemzok kiemelese, reszletek elrejtese.",
                TopicId = topics[1].Id, Difficulty = Difficulty.Easy, CreatedAt = now
            },
            new()
            {
                Text = "Mi a kulonbseg az abstract class es az interface kozott?",
                Answer = "Az abstract class tartalmazhat implementaciot es allapotot (mezoket), egy osztaly csak egybol orokolhet. Az interface csak metodus-alairasokat defnial (C# 8-tol default implementacioval is), egy osztaly tobb interface-t is implementalhat. Az abstract class 'is-a' kapcsolatot, az interface 'can-do' kepesseget fejez ki.",
                TopicId = topics[1].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },
            new()
            {
                Text = "Magyarazd el a polimorfizmus fogalmat egy C# peldan keresztul!",
                Answer = "A polimorfizmus lehetove teszi, hogy egy ososztaly referencian keresztul a leszarmazott osztaly felulirt metodusat hivjuk. Pelda: Animal base class virtual Speak() metodussal, Dog es Cat override-dal. Animal a = new Dog(); a.Speak() a Dog implementaciojat futtatja. Compile-time (overloading) es runtime (override) polimorfizmus letezik.",
                TopicId = topics[1].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },
            new()
            {
                Text = "Mik a SOLID elvek? Magyarazd el mindegyiket roviden!",
                Answer = "S - Single Responsibility: egy osztaly egy felelosseg. O - Open/Closed: nyitott bovitesre, zart modositasra. L - Liskov Substitution: leszarmazott behelyettesitheto az os helyere. I - Interface Segregation: sok kicsi interfesz jobb, mint egy nagy. D - Dependency Inversion: magas szintu modul ne fuggjon alacsony szintutol, mindketto absztraciotol fuggjon.",
                TopicId = topics[1].Id, Difficulty = Difficulty.Hard, CreatedAt = now
            },
            new()
            {
                Text = "Mi a kulonbseg a shallow copy es deep copy kozott?",
                Answer = "A shallow copy masolja a mezok ertekeit: value type-oknal az erteket, reference type-oknal a referenciat (tehat ugyanarra az objektumra mutat). A deep copy rekurzivan masolja az osszes hivatkozott objektumot is, teljesen fuggetlen masolatot letrehozva. C#-ban az ICloneable es MemberwiseClone() shallow copy-t vegez.",
                TopicId = topics[1].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },

            // .NET Keretrendszer
            new()
            {
                Text = "Mi a CLR (Common Language Runtime) es mi a feladata?",
                Answer = "A CLR a .NET futtatokornyezete, amely a leforditott IL (Intermediate Language) kodot JIT (Just-In-Time) forditassal nativ gepi kodda alakitja. Feladatai: memoriakezelesy (GC), tipusbiztonsag, kivetelkezeles, szalkezeles, biztonsagi ellenorzesek. Nyelv-fuggetlen: C#, VB.NET, F# mind IL-re fordul.",
                TopicId = topics[2].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },
            new()
            {
                Text = "Hogyan mukodik a garbage collection a .NET-ben?",
                Answer = "A GC automatikusan felszabaditja a nem hivatkozott objektumokat a heap-rol. Generacios modellt hasznal: Gen0 (rovid eletu), Gen1 (koztes), Gen2 (hosszu eletu). Az objektumok Gen0-ban jonnek letre, ha tulelnek egy GC ciklust, elolepnek. A GC mark-and-sweep algoritmust alkalmaz: megjeloli az elerheto objektumokat, a tobbit felszabaditja.",
                TopicId = topics[2].Id, Difficulty = Difficulty.Hard, CreatedAt = now
            },
            new()
            {
                Text = "Mi a kulonbseg a .NET Framework es a .NET (Core) kozott?",
                Answer = "A .NET Framework csak Windows-on fut, a .NET (Core/5+) cross-platform (Windows, Linux, macOS). A .NET Core nyilt forraskodu, modularis, jobb teljesitmenyu es side-by-side telepitheto. A .NET Framework System.Web-re epul (ASP.NET), a .NET Core Kestrel webszervert hasznal. A .NET 5+ a ketto egyesitese.",
                TopicId = topics[2].Id, Difficulty = Difficulty.Easy, CreatedAt = now
            },
            new()
            {
                Text = "Mik azok a delegate-ek es event-ek? Miben kulonboznek?",
                Answer = "A delegate egy tipusbiztos fuggveny-mutato: metodus-referenciat tarol es hivhato. Az event egy specialis delegate, amely korlatozza a hozzaferest: kivulrol csak += es -= (feliratkozas/leiratkozas) engedelyezett, meghivni csak a deklaralo osztaly tudja. Beepitett delegate-ek: Action, Func, Predicate.",
                TopicId = topics[2].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },
            new()
            {
                Text = "Magyarazd el az async/await mukodeset .NET-ben!",
                Answer = "Az async/await aszinkron programozasi minta, amely nem blokkolja a szalat I/O muveletekkor. Az async metodus Task-ot ad vissza. Az await felfuggeszti a metodus vegrehajtasat amig a Task befejezodik, de kozben a szal felszabadul mas munkara. Nem hoz letre uj szalat, hanem hatekonyan kezeli az I/O-bound muveleteket.",
                TopicId = topics[2].Id, Difficulty = Difficulty.Hard, CreatedAt = now
            },

            // SQL es Adatbazisok
            new()
            {
                Text = "Mi a kulonbseg az INNER JOIN es LEFT JOIN kozott?",
                Answer = "Az INNER JOIN csak azokat a sorokat adja vissza, amelyekhez mindket tablaban van megfelelo par. A LEFT JOIN az osszes sort visszaadja a bal oldali tablabol, es ha nincs par a jobb oldalon, NULL ertekekkel tolti ki. Ha van 10 rendeles es 8-nak van vezoje, INNER JOIN 8 sort ad, LEFT JOIN 10-et.",
                TopicId = topics[3].Id, Difficulty = Difficulty.Easy, CreatedAt = now
            },
            new()
            {
                Text = "Mi az index es miert fontos az adatbazis teljesitmeny szempontjabol?",
                Answer = "Az index egy adatszerkezet (altalaban B-tree), amely gyorsitja a keresest az adatbazisban, hasonloan egy konyv targymutatojahoz. Indexeles nelkul full table scan szukseges. Clustered index a fizikai sorrendet hatarozza meg (tablankent egy), non-clustered index kulon strukturat hoz letre mutatokkal. Hatrany: lassitja az INSERT/UPDATE muveleteket.",
                TopicId = topics[3].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },
            new()
            {
                Text = "Magyarazd el a normalizalas fogalmat! Mik a normalformak?",
                Answer = "A normalizalas az adatbazis-tervezesi folyamat, amely minimalizalja az adatredundanciat es a modositasi anomaliakat. 1NF: atomi ertekek, nincs ismetlodo csoport. 2NF: 1NF + nincs reszleges fugges az osszetett kulcstol. 3NF: 2NF + nincs tranzitiv fuggoseg. BCNF: minden determinans szuperkulcs.",
                TopicId = topics[3].Id, Difficulty = Difficulty.Hard, CreatedAt = now
            },
            new()
            {
                Text = "Mi a kulonbseg a WHERE es HAVING zaradek kozott?",
                Answer = "A WHERE az egyes sorokra szur a csoportositas ELOTT (GROUP BY elott fut). A HAVING a csoportositott eredmenyre szur a GROUP BY UTAN. Pelda: WHERE salary > 50000 minden sort szur, HAVING COUNT(*) > 5 csak azokat a csoportokat tartja meg, ahol 5-nel tobb sor van.",
                TopicId = topics[3].Id, Difficulty = Difficulty.Easy, CreatedAt = now
            },
            new()
            {
                Text = "Mi a tranzakcio es mik az ACID tulajdonsagok?",
                Answer = "A tranzakcio logikailag osszetartozo muveletek egysege, amely vagy teljesen vegrehajodik, vagy teljesen visszagorgetodik. ACID: Atomicity (atomisag - mindent vagy semmit), Consistency (konzisztencia - ervenyes allapotbol ervenyes allapotba), Isolation (izolacio - tranzakciok nem zavarjak egymast), Durability (tartossag - commit utan vegleges).",
                TopicId = topics[3].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },

            // Design Patterns
            new()
            {
                Text = "Magyarazd el a Singleton mintat! Mikor hasznaljuk es mik a veszelyei?",
                Answer = "A Singleton biztositja, hogy egy osztalybol pontosan egy peldany letezzen es globalis hozzaferesi pontot biztosit. Megvalositas: privat konstruktor + statikus property. Veszelyek: rejtett fuggoseg (nehez tesztelni), szalbiztonsagi problemak. C#-ban Lazy<T> vagy static readonly mezo hasznalata javasolt. DI kontenerben AddSingleton() a modern megoldas.",
                TopicId = topics[4].Id, Difficulty = Difficulty.Easy, CreatedAt = now
            },
            new()
            {
                Text = "Mi a Repository pattern es mi az elonye?",
                Answer = "A Repository pattern egy absztrakcios reteg az adathozzaferes es az uzleti logika kozott. Az adatforrast (DB, API, fajl) elrejti egy interfesz moge (pl. IRepository<T>). Elonyok: tesztelheto (mockolhato), cserelheto adatforras, kozpontositott lekerdezes-logika, tiszta architektura.",
                TopicId = topics[4].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },
            new()
            {
                Text = "Mi a kulonbseg a Factory Method es Abstract Factory minta kozott?",
                Answer = "A Factory Method egy metodust defnial az objektum letrehozasara, a leszarmazott osztaly donti el, melyik konkret tipust hozza letre (egy terkmeket gyart). Az Abstract Factory egy interfeszt ad kapcsolodo objektumok csaladjanak letrehozasara (tobb osszetartozo terkmeket gyart). Pelda: Factory Method -> CreateButton(), Abstract Factory -> CreateButton() + CreateTextBox() egy temahoz.",
                TopicId = topics[4].Id, Difficulty = Difficulty.Hard, CreatedAt = now
            },
            new()
            {
                Text = "Magyarazd el az Observer mintat! Hol talalkozunk vele a .NET-ben?",
                Answer = "Az Observer minta egy-a-tobbhoz fuggoseget valosit meg: amikor egy objektum (Subject) allapota valtozik, az osszes figyelo (Observer) automatikusan ertesul. .NET-ben az event/delegate mechanizmus pontosan ezt valositja meg. Pelda: INotifyPropertyChanged WPF data binding-nal, vagy az EventHandler<T> pattern.",
                TopicId = topics[4].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },
            new()
            {
                Text = "Mi a Dependency Injection es miert fontos a modern szoftverfejlesztesben?",
                Answer = "A DI egy tervezesi minta, ahol az osztaly fuggosegeit kivulrol kapja (konstruktor, property vagy metodus injektalas) ahelyett, hogy maga hozna letre. Elonyok: laza csatolas, tesztelhetoseg (mock objektumok), konfiguralhatosag, Single Responsibility betartasa. ASP.NET Core beepitett DI kontenert kinal: AddTransient, AddScoped, AddSingleton.",
                TopicId = topics[4].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },

            // Web Fejlesztes
            new()
            {
                Text = "Mi a REST API es mik az alapelvei?",
                Answer = "A REST (Representational State Transfer) egy architekturalis stilus webes API-khoz. Alapelvei: 1) Kliens-szerver szetvalasztas, 2) Allapotmentes kommunikacio (stateless), 3) Gyorsitotarazhatosag (cacheable), 4) Egyseges interfesz (URI-k eroforrasokhoz, HTTP igek: GET, POST, PUT, DELETE), 5) Retegzett rendszer.",
                TopicId = topics[5].Id, Difficulty = Difficulty.Easy, CreatedAt = now
            },
            new()
            {
                Text = "Mi a kulonbseg a GET, POST, PUT es DELETE HTTP metodusok kozott?",
                Answer = "GET: eroforras lekerese (idempotens, biztonsagos). POST: uj eroforras letrehozasa (nem idempotens). PUT: meglevo eroforras teljes csereje (idempotens). DELETE: eroforras torlese (idempotens). PATCH: reszleges modositas. Az idempotens azt jelenti, hogy tobbszori vegrehajtas ugyanazt az eredmenyt adja.",
                TopicId = topics[5].Id, Difficulty = Difficulty.Easy, CreatedAt = now
            },
            new()
            {
                Text = "Mik azok a middleware-ek az ASP.NET Core-ban?",
                Answer = "A middleware-ek a HTTP request/response pipeline-t alkoto komponensek. Minden middleware megkapja a kerest, feldolgozza es opcionalisn tovabbadja a kovetkezonek (next()). Sorrend fontos! Tipusok: Authentication, Authorization, CORS, Static Files, Exception Handling, Routing. A Use(), Map(), Run() metodusokkal konfiguralhatoak.",
                TopicId = topics[5].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },
            new()
            {
                Text = "Magyarazd el a JWT token alapu authentikacio mukodeset!",
                Answer = "1) A kliens elkuldi a hitelesito adatokat (user/pass). 2) A szerver ellenorzi es alairt JWT tokent general (Header.Payload.Signature). 3) A kliens minden kereshez csatolja: Authorization: Bearer <token>. 4) A szerver az alairassal ellenorzi a token ervenyesseget. A payload claim-eket tartalmaz (userId, role, exp). A Refresh Token a lejarat kezelesere szolgal.",
                TopicId = topics[5].Id, Difficulty = Difficulty.Hard, CreatedAt = now
            },
            new()
            {
                Text = "Mi a CORS es miert van ra szukseg a web fejlesztesben?",
                Answer = "A CORS (Cross-Origin Resource Sharing) egy bongeszo biztonsagi mechanizmus, amely szabalyozza, mely domainek erhetik el mas domain eroforrasait. Alapertelmezetten a same-origin policy tiltja a cross-origin kereseket. A szerver CORS fejlecekkel (Access-Control-Allow-Origin stb.) engedelyezi a hozzaferest. ASP.NET Core-ban: builder.Services.AddCors() + app.UseCors().",
                TopicId = topics[5].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },

            // Algoritmusok es Adatszerkezetek
            new()
            {
                Text = "Mi a Big O jeloles es mire hasznaljuk?",
                Answer = "A Big O jeloles az algoritmusok idsobeli vagy terbeli komplexitasanak felso korlatjat adja meg az input meretenek fuggvenyeben. Gyakori komplexitasok: O(1) konstans, O(log n) logaritmikus, O(n) linearis, O(n log n) linearitmikus, O(n^2) negyzetes, O(2^n) exponencialis. Segit osszehasonlitani algoritmusokat.",
                TopicId = topics[6].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },
            new()
            {
                Text = "Mi a kulonbseg a Stack es Queue adatszerkezetek kozott?",
                Answer = "A Stack LIFO (Last In, First Out): az utoljara berakott elemet vesszuk ki eloszor. Muveletek: Push, Pop, Peek. A Queue FIFO (First In, First Out): az eloszor berakott elemet vesszuk ki eloszor. Muveletek: Enqueue, Dequeue, Peek. Stack: undo/redo, metodushivas-verem. Queue: nyomtatasi sor, BFS.",
                TopicId = topics[6].Id, Difficulty = Difficulty.Easy, CreatedAt = now
            },
            new()
            {
                Text = "Magyarazd el a binary search algoritmus mukodeset! Mi az idokomplexitasa?",
                Answer = "A binaris kereses rendezett tombben keres O(log n) idoben. 1) Kozepso elemet vizsgalja. 2) Ha egyezik, megtalalta. 3) Ha a keresett elem kisebb, a bal feleben folytatja. 4) Ha nagyobb, a jobb feleben. 5) Felezi a keresesi tartomanyt minden lepesben. 1000 elemu tombben maximum 10 osszehasonlitas kell.",
                TopicId = topics[6].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },
            new()
            {
                Text = "Mi a hash table es hogyan mukodik? Mi tortenik utkozes eseten?",
                Answer = "A hash table kulcs-ertek parokat tarol gyors O(1) atlagos hozzaferessel. A kulcsot egy hash fuggveny indexsze alakitja. Utkozes (collision) kezelese: chaining (lancolt lista az adott indexen) vagy open addressing (kovetkezo szabad hely). C#-ban: Dictionary<TKey, TValue>. A jo hash fuggveny egyenletes eloszlast biztosit.",
                TopicId = topics[6].Id, Difficulty = Difficulty.Hard, CreatedAt = now
            },
            new()
            {
                Text = "Mi a rekurzio es mikor erdemes hasznalni iteracio helyett?",
                Answer = "A rekurzio egy onmagat hivo fuggveny, amely egy bazis esettel (base case) es egy rekurziv esettel rendelkezik. Akkor erdemes hasznalni, ha a problema termeszetesen rekurziv (fa bejaras, Fibonacci, faktorialis, rendezesek). Hatrany: stack overflow veszely, memoriahasznalat. Iteracio gyakran hatekonyabb.",
                TopicId = topics[6].Id, Difficulty = Difficulty.Medium, CreatedAt = now
            },
        };

        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();
    }
}
