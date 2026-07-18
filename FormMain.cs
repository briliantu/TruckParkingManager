using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TruckParkingManager
{
    public partial class FormMain : Form
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly ConfigurareApp Config = ConfigurareApp.Incarca();

        // Citite acum din appsettings.json (implementare #5) - nu mai necesită recompilare
        private static readonly string ApiUrl = Config.ApiUrl;
        private readonly int CAPACITATE_MAXIMA = Config.CapacitateMaxima;

        private const string CaleFisierOffline = "offline_buffer.txt";

        private List<Camion> listaCamioaneActive = new List<Camion>();

        // Regex permisiv pentru formatul românesc de înmatriculare
        // (ex: CJ99ABC, B123ABC). Ajustează dacă ai și numere provizorii/altă țară.
        private static readonly Regex FormatNumarInmatriculare =
            new Regex(@"^[A-Z]{1,2}\d{2,3}[A-Z]{3}$", RegexOptions.Compiled);

        // Hartă vizuală a locurilor: index 1..CAPACITATE_MAXIMA, 0 neutilizat
        private Panel[] locuriCelule = Array.Empty<Panel>();
        private readonly ToolTip tipLocuri = new ToolTip();

        public FormMain()
        {
            InitializeComponent();
            client.Timeout = TimeSpan.FromSeconds(8);
            ConfigurareTabel();
            ConfigureazaHartaLocuri();
            _ = InitializeazaAsync();
        }

        private async Task InitializeazaAsync()
        {
            await IncarcaCamioaneActive();
            await SincronizeazaBufferOfflineAsync();
            ActualizeazaAfisajLocuriDisponibile();
        }

        private void ConfigurareTabel()
        {
            dgvCamioane.ColumnCount = 5;
            dgvCamioane.Columns[0].Name = "Număr Înmatriculare";
            dgvCamioane.Columns[1].Name = "Loc Alocat";
            dgvCamioane.Columns[2].Name = "Dată Intrare";
            dgvCamioane.Columns[3].Name = "Dată Ieșire";
            dgvCamioane.Columns[4].Name = "Durată Totală";
            dgvCamioane.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCamioane.AllowUserToAddRows = false;
            dgvCamioane.ReadOnly = true;
        }

        // --- Sursă unică de adevăr pentru locurile ocupate (implementare #2) ---
        // Înainte existau două structuri paralele (bool[] locuriParcare + listaCamioaneActive)
        // care se puteau desincroniza dacă IncarcaCamioaneActive eșua parțial.
        // Acum totul se calculează direct din listaCamioaneActive.

        private HashSet<int> GetLocuriOcupate()
        {
            var ocupate = new HashSet<int>();
            foreach (var camion in listaCamioaneActive)
            {
                if (camion.DataIesire == null)
                {
                    ocupate.Add(camion.NumarLoc);
                }
            }
            return ocupate;
        }

        private int GetLocuriLibere()
        {
            return CAPACITATE_MAXIMA - GetLocuriOcupate().Count;
        }

        private int GasestePrimulLocLiber()
        {
            var ocupate = GetLocuriOcupate();
            for (int i = 1; i <= CAPACITATE_MAXIMA; i++)
            {
                if (!ocupate.Contains(i)) return i;
            }
            return -1;
        }

        private void ActualizeazaAfisajLocuriDisponibile()
        {
            int libere = GetLocuriLibere();
            this.Text = $"TruckParkingManager - Locuri Libere: {libere} / {CAPACITATE_MAXIMA}";

            if (libere <= 0)
            {
                btnIntrare.Enabled = false;
                btnIntrare.Text = "PARCARE PLINĂ";
            }
            else
            {
                btnIntrare.Enabled = true;
                btnIntrare.Text = "INTRARE";
            }

            ActualizeazaHartaLocuriVizual();
        }

        // --- Hartă vizuală a locurilor (2 coloane x N rânduri, verde/roșu) ---
        // Construită o singură dată la pornire; culorile se actualizează
        // din aceeași sursă de adevăr ca restul aplicației (GetLocuriOcupate).

        private void ConfigureazaHartaLocuri()
        {
            const int coloane = 5;
            const int dimensiuneCelula = 24;
            const int spatiu = 3;

            locuriCelule = new Panel[CAPACITATE_MAXIMA + 1];
            pnlHarta.Controls.Clear();

            for (int i = 1; i <= CAPACITATE_MAXIMA; i++)
            {
                int rand = (i - 1) / coloane;
                int coloana = (i - 1) % coloane;

                var celula = new Panel
                {
                    Width = dimensiuneCelula,
                    Height = dimensiuneCelula,
                    Location = new Point(coloana * (dimensiuneCelula + spatiu), rand * (dimensiuneCelula + spatiu)),
                    BackColor = Color.ForestGreen,
                    BorderStyle = BorderStyle.FixedSingle
                };

                tipLocuri.SetToolTip(celula, $"Loc {i}");

                pnlHarta.Controls.Add(celula);
                locuriCelule[i] = celula;
            }
        }

        private void ActualizeazaHartaLocuriVizual()
        {
            var ocupate = GetLocuriOcupate();
            for (int i = 1; i <= CAPACITATE_MAXIMA; i++)
            {
                if (locuriCelule[i] == null) continue;
                locuriCelule[i].BackColor = ocupate.Contains(i) ? Color.Crimson : Color.ForestGreen;
            }
        }

        // SIMULARE DESCHIDERE BARIERĂ
        private void ActioneazaBariera()
        {
            MessageBox.Show("Comandă trimisă! Bariera a fost deschisă în siguranță.", "Sistem Control Barieră", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // BUTONUL INTRARE
        private async void btnIntrare_Click(object sender, EventArgs e)
        {
            string nrInmatriculare = txtNrInmatriculare.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(nrInmatriculare))
            {
                MessageBox.Show("Introduceți un număr de înmatriculare valid!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Implementare #7: validare format. Dacă formatul nu se potrivește,
            // avertizăm dar lăsăm operatorul să confirme (poate fi un caz special:
            // număr provizoriu, remorcă, sau vehicul înmatriculat în altă țară).
            if (!FormatNumarInmatriculare.IsMatch(nrInmatriculare))
            {
                var confirmare = MessageBox.Show(
                    $"Numărul \"{nrInmatriculare}\" nu are un format românesc standard. Continui oricum?",
                    "Format neobișnuit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirmare != DialogResult.Yes) return;
            }

            // Implementare #1: verificare duplicate - un camion nu poate intra de două ori
            if (listaCamioaneActive.Exists(c => c.NumarInmatriculare == nrInmatriculare && c.DataIesire == null))
            {
                MessageBox.Show("Acest camion este deja înregistrat ca fiind în parcare!", "Duplicat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (GetLocuriLibere() <= 0)
            {
                MessageBox.Show("Parcarea este plină!", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            int locAlocat = GasestePrimulLocLiber();
            if (locAlocat == -1) return;

            var nouCamion = new Camion(nrInmatriculare, DateTime.Now, locAlocat);

            // Implementare #8: dezactivăm butoanele cât timp așteptăm răspunsul API,
            // ca să nu se poată crea intrări duplicate din click-uri repetate.
            SeteazaButoaneActive(false);
            try
            {
                ActioneazaBariera();

                string json = JsonSerializer.Serialize(nouCamion);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync($"{ApiUrl}/intrare", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        await SalvareLocalaFallbackAsync("INTRARE", nouCamion);
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    await SalvareLocalaFallbackAsync("INTRARE", nouCamion);
                }

                listaCamioaneActive.Add(nouCamion);
                ActualizeazaGridVizual();
                ActualizeazaAfisajLocuriDisponibile();
                txtNrInmatriculare.Clear();
            }
            finally
            {
                SeteazaButoaneActive(true);
            }
        }

        // BUTONUL IEȘIRE
        private async void btnIesire_Click(object sender, EventArgs e)
        {
            string nrInmatriculare = txtNrInmatriculare.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(nrInmatriculare))
            {
                MessageBox.Show("Introduceți numărul camionului care pleacă!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Camion camionGasit = listaCamioaneActive.FindLast(c => c.NumarInmatriculare == nrInmatriculare && c.DataIesire == null);

            if (camionGasit == null)
            {
                MessageBox.Show("Camionul nu a fost găsit în parcare.", "Informație", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SeteazaButoaneActive(false);
            try
            {
                camionGasit.FinalizeazaStationare(DateTime.Now);

                ActioneazaBariera();

                string json = JsonSerializer.Serialize(camionGasit);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PutAsync($"{ApiUrl}/iesire", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        await SalvareLocalaFallbackAsync("IESIRE", camionGasit);
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    await SalvareLocalaFallbackAsync("IESIRE", camionGasit);
                }

                ActualizeazaGridVizual();
                ActualizeazaAfisajLocuriDisponibile();
                txtNrInmatriculare.Clear();
            }
            finally
            {
                SeteazaButoaneActive(true);
            }
        }

        private void SeteazaButoaneActive(bool activ)
        {
            btnIesire.Enabled = activ;
            // btnIntrare respectă în continuare regula "parcare plină" din ActualizeazaAfisajLocuriDisponibile;
            // aici doar prevenim dublul-click în timpul unui apel în desfășurare.
            btnIntrare.Enabled = activ && GetLocuriLibere() > 0;
        }

        private void ActualizeazaGridVizual()
        {
            dgvCamioane.Rows.Clear();
            foreach (var c in listaCamioaneActive)
            {
                dgvCamioane.Rows.Add(c.NumarInmatriculare, $"Loc {c.NumarLoc}", c.DataIntrare.ToString("g"), c.DataIesire?.ToString("g") ?? "-", c.DurataTotala);
            }
        }

        private async Task IncarcaCamioaneActive()
        {
            try
            {
                string responseBody = await client.GetStringAsync($"{ApiUrl}/active");
                var dateSrv = JsonSerializer.Deserialize<List<Camion>>(responseBody);
                if (dateSrv != null)
                {
                    listaCamioaneActive = dateSrv;
                    ActualizeazaGridVizual();
                    ActualizeazaAfisajLocuriDisponibile();
                }
            }
            catch
            {
                // În caz de eroare la conexiunea inițială, aplicația va porni goală
                // dar va funcționa local în regim offline
            }
        }

        // --- Implementare #4: buffer offline cu retry real ---
        // Fiecare linie salvată conține tipul evenimentului + JSON-ul complet al camionului,
        // nu doar text descriptiv, ca să poată fi retrimisă exact la reconectare.

        private class BufferEntry
        {
            public string Tip { get; set; } = "";
            public Camion? Date { get; set; }
        }

        private async Task SalvareLocalaFallbackAsync(string tipEveniment, Camion camion)
        {
            var entry = new BufferEntry { Tip = tipEveniment, Date = camion };
            string linie = JsonSerializer.Serialize(entry) + Environment.NewLine;
            try
            {
                await File.AppendAllTextAsync(CaleFisierOffline, linie);
            }
            catch
            {
                // dacă nici scrierea locală nu reușește (disc plin, permisiuni etc.),
                // nu mai putem face nimic - operatorul a fost deja informat implicit
                // prin faptul că bariera s-a acționat oricum (offline-first).
            }
        }

        // Încearcă să retrimită tot ce e în offline_buffer.txt către API.
        // Se apelează la pornirea aplicației; poate fi legată și de un Timer
        // periodic dacă ai nevoie de sincronizare automată în timpul rulării.
        private async Task SincronizeazaBufferOfflineAsync()
        {
            if (!File.Exists(CaleFisierOffline)) return;

            string[] linii;
            try
            {
                linii = await File.ReadAllLinesAsync(CaleFisierOffline);
            }
            catch
            {
                return;
            }

            if (linii.Length == 0) return;

            var neretrimise = new List<string>();

            foreach (var linie in linii)
            {
                if (string.IsNullOrWhiteSpace(linie)) continue;

                bool trimisCuSucces = false;
                try
                {
                    var entry = JsonSerializer.Deserialize<BufferEntry>(linie);
                    if (entry?.Date != null)
                    {
                        string json = JsonSerializer.Serialize(entry.Date);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = entry.Tip == "INTRARE"
                            ? await client.PostAsync($"{ApiUrl}/intrare", content)
                            : await client.PutAsync($"{ApiUrl}/iesire", content);

                        trimisCuSucces = response.IsSuccessStatusCode;
                    }
                }
                catch
                {
                    trimisCuSucces = false;
                }

                if (!trimisCuSucces)
                {
                    neretrimise.Add(linie);
                }
            }

            // Rescriem fișierul doar cu ce încă n-a plecat - astfel buffer-ul
            // nu mai crește la infinit și nu mai pierdem intrări nereușite.
            if (neretrimise.Count != linii.Length)
            {
                try
                {
                    await File.WriteAllLinesAsync(CaleFisierOffline, neretrimise);
                }
                catch
                {
                    // dacă rescrierea eșuează, la următoarea pornire se va încerca din nou
                }
            }
        }
    }
}