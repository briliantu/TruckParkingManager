using System.Net;
using System.Net.Sockets;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Ascultă pe toate interfețele de rețea, port 5000
builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

const string CaleDate = "parcare-date.json";
var lockDate = new object();
List<Camion> camioane = IncarcaDeLaDisc();

// --- ENDPOINT-URI PENTRU CLIENT ȘI DASHBOARD WEB ---

// 1. Camioanele active în prezent
app.MapGet("/parcare/active", () =>
{
    lock (lockDate)
    {
        return Results.Ok(camioane.Where(c => c.DataIesire == null).ToList());
    }
});

// 2. Toate camioanele (pentru istoric / CSV export)
app.MapGet("/parcare/toate", () =>
{
    lock (lockDate)
    {
        return Results.Ok(camioane);
    }
});

// 3. Statistici Dashboard Admin
app.MapGet("/parcare/stats", () =>
{
    lock (lockDate)
    {
        int totalCumulat = camioane.Count; // Total intrări înregistrate vreodată
        int ocupate = camioane.Count(c => c.DataIesire == null);

        return Results.Ok(new
        {
            TotalCumulat = totalCumulat,
            Ocupate = ocupate,
            Capacitate = 50
        });
    }
});

// 4. Intrare Camion
app.MapPost("/parcare/intrare", (Camion camion) =>
{
    lock (lockDate)
    {
        camioane.Add(camion);
        SalveazaPeDisc();
    }
    return Results.Ok();
});

// 5. Ieșire Camion
app.MapPut("/parcare/iesire", (Camion camion) =>
{
    lock (lockDate)
    {
        var existent = camioane.FirstOrDefault(c =>
            c.NumarLoc == camion.NumarLoc &&
            c.DataIntrare == camion.DataIntrare &&
            c.DataIesire == null);

        if (existent == null)
        {
            return Results.NotFound();
        }

        existent.DataIesire = camion.DataIesire;
        existent.DurataTotala = camion.DurataTotala;
        SalveazaPeDisc();
    }
    return Results.Ok();
});

// 6. Mută un camion de pe un loc pe altul
app.MapPut("/parcare/muta", (RelocareRequest req) =>
{
    lock (lockDate)
    {
        var camion = camioane.FirstOrDefault(c => c.NumarLoc == req.LocSursa && c.DataIesire == null);
        var locDestinatieOcupat = camioane.Any(c => c.NumarLoc == req.LocDestinatie && c.DataIesire == null);

        if (camion == null)
            return Results.BadRequest("Nu există niciun camion activ pe locul sursă.");

        if (locDestinatieOcupat)
            return Results.BadRequest("Locul de destinație este deja ocupat!");

        camion.NumarLoc = req.LocDestinatie;
        SalveazaPeDisc();
        return Results.Ok();
    }
});

// 7. Curăță toate camioanele uitate în parcare (Clear Active Logs)
app.MapPost("/parcare/clear-active", () =>
{
    lock (lockDate)
    {
        DateTime acum = DateTime.Now;
        foreach (var c in camioane.Where(c => c.DataIesire == null))
        {
            c.DataIesire = acum;
            c.DurataTotala = "Forțat / Clear Logs";
        }
        SalveazaPeDisc();
        return Results.Ok();
    }
});

// --- PORNIRE SERVER ---

string ipLocal = ObtineIpLocal();
Console.Title = $"De'Longhi TruckParkingManager Server - IP: {ipLocal}:5000";
Console.WriteLine("========================================");
Console.WriteLine(" TruckParkingManager - Server De'Longhi pornit");
Console.WriteLine($" Adresa API Client: http://{ipLocal}:5000/parcare");
Console.WriteLine($" Dashboard Admin Web: http://{ipLocal}:5000/");
Console.WriteLine("========================================");

app.Run();

// --- FUNCȚII AJUTĂTOARE ---

List<Camion> IncarcaDeLaDisc()
{
    try
    {
        if (File.Exists(CaleDate))
        {
            string json = File.ReadAllText(CaleDate);
            return JsonSerializer.Deserialize<List<Camion>>(json) ?? new List<Camion>();
        }
    }
    catch { }
    return new List<Camion>();
}

void SalveazaPeDisc()
{
    string json = JsonSerializer.Serialize(camioane, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(CaleDate, json);
}

static string ObtineIpLocal()
{
    try
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect("8.8.8.8", 65530);
        if (socket.LocalEndPoint is IPEndPoint endPoint)
        {
            return endPoint.Address.ToString();
        }
    }
    catch { }

    return Dns.GetHostEntry(Dns.GetHostName())
        .AddressList
        .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1";
}

// --- CLASELE DE DATE ---

public class RelocareRequest
{
    public int LocSursa { get; set; }
    public int LocDestinatie { get; set; }
}

public class Camion
{
    public string NumarInmatriculare { get; set; } = "";
    public string Tara { get; set; } = "România";
    public DateTime DataIntrare { get; set; }
    public DateTime? DataIesire { get; set; }
    public string DurataTotala { get; set; } = "-";
    public int NumarLoc { get; set; }
}