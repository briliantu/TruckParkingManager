using System.Net;
using System.Net.Sockets;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Ascultă pe toate interfețele de rețea (0.0.0.0), port 5000.
// HTTP simplu, nu HTTPS - suficient pentru rețea locală, evită bătăi de cap cu certificate.
builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

// Servește fișierele din wwwroot/ (index.html, css/, js/) - panoul vizual din browser
app.UseDefaultFiles();
app.UseStaticFiles();

const string CaleDate = "parcare-date.json";
var lockDate = new object();
List<Camion> camioane = IncarcaDeLaDisc();

// --- Endpoint-uri, exact cele apelate deja de aplicația desktop ---

app.MapGet("/parcare/active", () =>
{
    lock (lockDate)
    {
        return Results.Ok(camioane.Where(c => c.DataIesire == null).ToList());
    }
});

app.MapPost("/parcare/intrare", (Camion camion) =>
{
    lock (lockDate)
    {
        camioane.Add(camion);
        SalveazaPeDisc();
    }
    return Results.Ok();
});

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

// Panoul vizual din browser e servit acum static din wwwroot/ (vezi app.UseStaticFiles() mai sus)

// --- Pornire: afișăm IP-ul local direct în titlul ferestrei consolei ---

string ipLocal = ObtineIpLocal();
Console.Title = $"TruckParkingManager Server - IP: {ipLocal}:5000";
Console.WriteLine("========================================");
Console.WriteLine(" TruckParkingManager - Server pornit");
Console.WriteLine($" Adresa pentru aplicațiile client: http://{ipLocal}:5000/parcare");
Console.WriteLine(" (pune adresa asta în appsettings.json -> ApiUrl, pe fiecare PC client)");
Console.WriteLine($" Panou vizual (browser): http://{ipLocal}:5000/");
Console.WriteLine("========================================");

app.Run();

// --- Funcții ajutătoare ---

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
    catch
    {
        // fișier corupt/lipsă -> pornim cu listă goală, nu blocăm serverul
    }
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
        // Trucul clasic: deschidem un "socket" UDP către o adresă externă (nu se trimite
        // niciun pachet real) doar ca să aflăm ce IP local ar folosi sistemul de operare.
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect("8.8.8.8", 65530);
        if (socket.LocalEndPoint is IPEndPoint endPoint)
        {
            return endPoint.Address.ToString();
        }
    }
    catch
    {
        // fallback mai jos dacă nu există conexiune la internet
    }

    return Dns.GetHostEntry(Dns.GetHostName())
        .AddressList
        .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1";
}

// Model identic cu cel din aplicația desktop, ca serializarea JSON să corespundă exact.
public class Camion
{
    public string NumarInmatriculare { get; set; } = "";
    public string Tara { get; set; } = "România";
    public DateTime DataIntrare { get; set; }
    public DateTime? DataIesire { get; set; }
    public string DurataTotala { get; set; } = "-";
    public int NumarLoc { get; set; }
}