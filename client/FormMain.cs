using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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

        private static readonly string ApiUrl = Config.ApiUrl;
        private readonly int CAPACITATE_MAXIMA = Config.CapacitateMaxima;
        private static readonly string ServerHostInfo = ObtineHostServer(ApiUrl);

        private const string CaleFisierOffline = "offline_buffer.txt";
        private const string CaleContorCumulat = "total_cumulat.txt";

        private List<Camion> listaCamioaneActive = new List<Camion>();
        private int totalCumulatIntrari = 0;

        private System.Windows.Forms.Timer timerLiveDuration = null!;
        private System.Windows.Forms.Timer timerSyncServer = null!;

        private bool esteAdmin = false;
        private const string PAROLA_ADMIN = "admin123";

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly Dictionary<string, Regex> FormatePeTara = new()
        {
            ["România"] = new Regex(@"^[A-Z]{1,2}\d{2,3}[A-Z]{3}$", RegexOptions.Compiled),
            ["Spania"] = new Regex(@"^\d{4}[A-Z]{3}$", RegexOptions.Compiled),
            ["Germania"] = new Regex(@"^[A-ZÄÖÜ]{1,3}[- ]?[A-Z]{1,2}[- ]?\d{1,4}$", RegexOptions.Compiled),
            ["Italia"] = new Regex(@"^[A-Z]{2}\d{3}[A-Z]{2}$", RegexOptions.Compiled),
            ["Bulgaria"] = new Regex(@"^[A-Z]{1,2}\d{4}[A-Z]{2}$", RegexOptions.Compiled),
            ["Turcia"] = new Regex(@"^\d{2}[ ]?[A-Z]{1,3}[ ]?\d{2,4}$", RegexOptions.Compiled),
            ["Republica Moldova"] = new Regex(@"^[A-Z]{1,2}[ ]?\d{3}[ ]?[A-Z]{2,3}$", RegexOptions.Compiled),
        };

        private Panel[] locuriCelule = Array.Empty<Panel>();
        private readonly ToolTip tipLocuri = new ToolTip();

        public FormMain()
        {
            InitializeComponent();
            IncarcaTotalCumulat();
            IncarcaLogoDeLonghi();

            client.Timeout = TimeSpan.FromSeconds(3);
            if (cmbTara.Items.Count > 0) cmbTara.SelectedIndex = 0;

            ConfigurareTabel();
            ConfigureazaHartaLocuri();
            InitTimers();

            _ = InitializeazaAsync();
        }

        private void IncarcaLogoDeLonghi()
        {
            try
            {
                string caleLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "delonghi.png");
                if (File.Exists(caleLogo))
                {
                    picLogo.Image = Image.FromFile(caleLogo);
                    picLogo.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            catch { }
        }

        private void IncarcaTotalCumulat()
        {
            try
            {
                if (File.Exists(CaleContorCumulat))
                {
                    string txt = File.ReadAllText(CaleContorCumulat);
                    int.TryParse(txt, out totalCumulatIntrari);
                }
            }
            catch { totalCumulatIntrari = 0; }
        }

        private void SalveazaTotalCumulat()
        {
            try
            {
                File.WriteAllText(CaleContorCumulat, totalCumulatIntrari.ToString());
            }
            catch { }
        }

        private void InitTimers()
        {
            timerLiveDuration = new System.Windows.Forms.Timer();
            timerLiveDuration.Interval = 1000;
            timerLiveDuration.Tick += (s, e) => ActualizeazaDurateLive();
            timerLiveDuration.Start();

            timerSyncServer = new System.Windows.Forms.Timer();
            timerSyncServer.Interval = 3000;
            timerSyncServer.Tick += async (s, e) => await SincronizeazaFundalAsync();
            timerSyncServer.Start();
        }

        private void ActualizeazaDurateLive()
        {
            DateTime acum = DateTime.Now;
            for (int i = 0; i < dgvCamioane.Rows.Count; i++)
            {
                if (i < listaCamioaneActive.Count && listaCamioaneActive[i].DataIesire == null)
                {
                    TimeSpan diff = acum - listaCamioaneActive[i].DataIntrare;
                    string durataLive = $"{Math.Floor(diff.TotalHours):00}h {diff.Minutes:00}m {diff.Seconds:00}s";
                    dgvCamioane.Rows[i].Cells[5].Value = durataLive;
                }
            }
        }

        private static string ObtineHostServer(string apiUrl)
        {
            try
            {
                var uri = new Uri(apiUrl);
                return uri.IsDefaultPort ? uri.Host : $"{uri.Host}:{uri.Port}";
            }
            catch { return "local"; }
        }

        private async Task InitializeazaAsync()
        {
            await SincronizeazaFundalAsync();
            ActualizeazaAfisajLocuriDisponibile();
            ActualizeazaGridVizual();
        }

        private void ConfigurareTabel()
        {
            dgvCamioane.ColumnCount = 6;
            dgvCamioane.Columns[0].Name = "Număr Înmatriculare";
            dgvCamioane.Columns[1].Name = "Țară";
            dgvCamioane.Columns[2].Name = "Loc Alocat";
            dgvCamioane.Columns[3].Name = "Dată Intrare";
            dgvCamioane.Columns[4].Name = "Dată Ieșire";
            dgvCamioane.Columns[5].Name = "Durată Staționare";
            dgvCamioane.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCamioane.AllowUserToAddRows = false;
            dgvCamioane.ReadOnly = true;
        }

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
            int ocupate = GetLocuriOcupate().Count;

            lblKpiLibere.Text = libere.ToString();
            lblKpiOcupate.Text = ocupate.ToString();
            lblKpiTotal.Text = CAPACITATE_MAXIMA.ToString();
            lblKpiCumulat.Text = totalCumulatIntrari.ToString();

            string rolStr = esteAdmin ? "ADMIN" : "OPERATOR";
            this.Text = $"De'Longhi TruckParkingManager [{ServerHostInfo}] - Rol: {rolStr} - Libere: {libere} / {CAPACITATE_MAXIMA}";

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

        private void ConfigureazaHartaLocuri()
        {
            const int coloane = 5;
            int latimeDisponibila = pnlHarta.ClientSize.Width - 12;
            int inaltimeDisponibila = pnlHarta.ClientSize.Height - 12;

            int latimeCelula = (latimeDisponibila / coloane) - 4;
            int inaltimeCelula = (inaltimeDisponibila / 10) - 4;

            const int spatiu = 4;
            const int margineStanga = 5;
            const int margineSus = 5;

            locuriCelule = new Panel[CAPACITATE_MAXIMA + 1];
            pnlHarta.Controls.Clear();

            for (int i = 1; i <= CAPACITATE_MAXIMA; i++)
            {
                int rand = (i - 1) / coloane;
                int coloana = (i - 1) % coloane;

                var celula = new Panel
                {
                    Width = latimeCelula,
                    Height = inaltimeCelula,
                    Location = new Point(margineStanga + coloana * (latimeCelula + spatiu), margineSus + rand * (inaltimeCelula + spatiu)),
                    BackColor = Color.ForestGreen,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = i
                };

                var lblNumar = new Label
                {
                    Text = $"#{i:D2}",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Enabled = false
                };

                celula.Controls.Add(lblNumar);
                tipLocuri.SetToolTip(celula, $"Loc #{i:D2} - LIBER");

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

                bool esteOcupat = ocupate.Contains(i);
                locuriCelule[i].BackColor = esteOcupat ? Color.Crimson : Color.ForestGreen;

                var camion = listaCamioaneActive.Find(c => c.NumarLoc == i && c.DataIesire == null);
                if (esteOcupat && camion != null)
                {
                    TimeSpan diff = DateTime.Now - camion.DataIntrare;
                    tipLocuri.SetToolTip(locuriCelule[i], $"Loc #{i:D2} - OCUPAT\nCamion: {camion.NumarInmatriculare}\nStaționat: {Math.Floor(diff.TotalHours)}h {diff.Minutes}m");
                }
                else
                {
                    tipLocuri.SetToolTip(locuriCelule[i], $"Loc #{i:D2} - LIBER");
                }
            }
        }

        private void ActioneazaBariera()
        {
            MessageBox.Show("Comandă trimisă! Bariera a fost deschisă în siguranță.", "Sistem Control Barieră", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnIntrare_Click(object sender, EventArgs e)
        {
            string nrInmatriculare = txtNrInmatriculare.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(nrInmatriculare))
            {
                MessageBox.Show("Introduceți un număr de înmatriculare valid!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tara = cmbTara.SelectedItem?.ToString() ?? "România";

            if (FormatePeTara.TryGetValue(tara, out var formatAsteptat) && !formatAsteptat.IsMatch(nrInmatriculare))
            {
                var confirmare = MessageBox.Show(
                    $"Numărul \"{nrInmatriculare}\" nu are formatul specific pentru {tara}. Continui oricum?",
                    "Format neobișnuit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirmare != DialogResult.Yes) return;
            }

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

            var nouCamion = new Camion(nrInmatriculare, DateTime.Now, locAlocat, tara);

            SeteazaButoaneActive(false);
            try
            {
                ActioneazaBariera();

                listaCamioaneActive.Add(nouCamion);
                totalCumulatIntrari++;
                SalveazaTotalCumulat();

                ActualizeazaGridVizual();
                ActualizeazaAfisajLocuriDisponibile();
                txtNrInmatriculare.Clear();

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
                catch
                {
                    await SalvareLocalaFallbackAsync("INTRARE", nouCamion);
                }
            }
            finally
            {
                SeteazaButoaneActive(true);
            }
        }

        private async void btnIesire_Click(object sender, EventArgs e)
        {
            string nrInmatriculare = txtNrInmatriculare.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(nrInmatriculare))
            {
                MessageBox.Show("Introduceți numărul camionului care pleacă!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Camion? camionGasit = listaCamioaneActive.FindLast(c => c.NumarInmatriculare == nrInmatriculare && c.DataIesire == null);

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

                ActualizeazaGridVizual();
                ActualizeazaAfisajLocuriDisponibile();
                txtNrInmatriculare.Clear();

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
                catch
                {
                    await SalvareLocalaFallbackAsync("IESIRE", camionGasit);
                }
            }
            finally
            {
                SeteazaButoaneActive(true);
            }
        }

        // LOGIN ADMIN -> CARACTERE ASCUNSE (isPassword = true)
        private void btnLoginAdmin_Click(object sender, EventArgs e)
        {
            if (!esteAdmin)
            {
                string parola = PromptDialog.ShowDialog("Introdu parola de Administrator:", "Login Admin", true);
                if (parola == PAROLA_ADMIN)
                {
                    esteAdmin = true;
                    btnLoginAdmin.Text = "🔓 Logout Admin";
                    btnLoginAdmin.BackColor = Color.Crimson;
                    btnClearLogs.Visible = true;
                    btnMutaCamion.Visible = true;
                    MessageBox.Show("Autentificat ca Administrator cu succes!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (!string.IsNullOrEmpty(parola))
                {
                    MessageBox.Show("Parolă incorectă!", "Eroare Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                esteAdmin = false;
                btnLoginAdmin.Text = "🔑 Login Admin";
                btnLoginAdmin.BackColor = Color.Orange;
                btnClearLogs.Visible = false;
                btnMutaCamion.Visible = false;
                MessageBox.Show("Te-ai delogat din contul de Admin.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            ActualizeazaAfisajLocuriDisponibile();
        }

        // MUTARE CAMION -> CARACTERE VIZIBILE (isPassword = false)
        private async void btnMutaCamion_Click(object sender, EventArgs e)
        {
            if (!esteAdmin) return;

            string inputSursa = PromptDialog.ShowDialog("Numărul locului SURSĂ (unde e parcat camionul):", "Mută Camion - Pasul 1", false);
            if (!int.TryParse(inputSursa, out int locSursa)) return;

            string inputDestinatie = PromptDialog.ShowDialog("Numărul locului DESTINAȚIE (unde îl muți):", "Mută Camion - Pasul 2", false);
            if (!int.TryParse(inputDestinatie, out int locDestinatie)) return;

            var camion = listaCamioaneActive.Find(c => c.NumarLoc == locSursa && c.DataIesire == null);
            if (camion == null)
            {
                MessageBox.Show($"Nu există niciun camion activ pe locul #{locSursa}!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (listaCamioaneActive.Exists(c => c.NumarLoc == locDestinatie && c.DataIesire == null))
            {
                MessageBox.Show($"Locul #{locDestinatie} este deja ocupat!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            camion.NumarLoc = locDestinatie;
            ActualizeazaGridVizual();
            ActualizeazaAfisajLocuriDisponibile();

            try
            {
                var reqObj = new { LocSursa = locSursa, LocDestinatie = locDestinatie };
                string json = JsonSerializer.Serialize(reqObj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PutAsync($"{ApiUrl}/muta", content);
            }
            catch { }
        }

        private async void btnClearLogs_Click(object sender, EventArgs e)
        {
            if (!esteAdmin) return;

            if (MessageBox.Show("Ești sigur că vrei să eliberezi TOATE camioanele active uitate în parcare?", "Confirmare Clear Logs", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DateTime acum = DateTime.Now;
                foreach (var c in listaCamioaneActive)
                {
                    if (c.DataIesire == null)
                    {
                        c.DataIesire = acum;
                        c.DurataTotala = "Forțat / Clear Logs";
                    }
                }

                ActualizeazaGridVizual();
                ActualizeazaAfisajLocuriDisponibile();

                try
                {
                    await client.PostAsync($"{ApiUrl}/clear-active", null);
                }
                catch { }
            }
        }

        private void SeteazaButoaneActive(bool activ)
        {
            btnIesire.Enabled = activ;
            btnIntrare.Enabled = activ && GetLocuriLibere() > 0;
        }

        private void ActualizeazaGridVizual()
        {
            dgvCamioane.Rows.Clear();
            foreach (var c in listaCamioaneActive)
            {
                if (c.DataIesire == null)
                {
                    dgvCamioane.Rows.Add(c.NumarInmatriculare, c.Tara, $"Loc #{c.NumarLoc:D2}", c.DataIntrare.ToString("g"), "-", c.DurataTotala);
                }
            }
        }

        private async Task SincronizeazaFundalAsync()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{ApiUrl}/active");
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var dateSrv = JsonSerializer.Deserialize<List<Camion>>(responseBody, jsonOptions);
                    if (dateSrv != null && dateSrv.Count > 0)
                    {
                        listaCamioaneActive = dateSrv;
                        if (this.IsHandleCreated)
                        {
                            this.BeginInvoke((MethodInvoker)delegate
                            {
                                ActualizeazaGridVizual();
                                ActualizeazaAfisajLocuriDisponibile();
                            });
                        }
                    }
                }
            }
            catch { }
        }

        private void btnExportSVG_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder svg = new StringBuilder();
                svg.AppendLine("<svg xmlns='http://www.w3.org/2000/svg' width='1000' height='700' style='background:#f4f7f6; font-family:Arial;'>");
                svg.AppendLine("<text x='20' y='40' font-size='22' font-weight='bold' fill='#002B49'>De'Longhi - Harta Parcare Camioane</text>");
                svg.AppendLine($"<text x='20' y='65' font-size='13' fill='#555'>Data Export: {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Total Cumulat Intrari: {totalCumulatIntrari}</text>");

                var ocupate = GetLocuriOcupate();

                for (int i = 1; i <= CAPACITATE_MAXIMA; i++)
                {
                    int col = (i - 1) % 5;
                    int row = (i - 1) / 5;
                    int x = 20 + col * 190;
                    int y = 90 + row * 95;

                    bool esteOcupat = ocupate.Contains(i);
                    string color = esteOcupat ? "#E74C3C" : "#2ECC71";

                    var c = listaCamioaneActive.Find(cam => cam.NumarLoc == i && cam.DataIesire == null);
                    string nrInm = (esteOcupat && c != null) ? c.NumarInmatriculare : "LIBER";

                    svg.AppendLine($"<g transform='translate({x}, {y})'>");
                    svg.AppendLine($"  <rect width='175' height='80' rx='6' fill='{color}' stroke='#333' stroke-width='1' />");
                    svg.AppendLine($"  <text x='12' y='28' fill='#fff' font-weight='bold' font-size='16'>LOC #{i:D2}</text>");
                    svg.AppendLine($"  <text x='12' y='55' fill='#fff' font-size='14'>{nrInm}</text>");
                    svg.AppendLine("</g>");
                }

                svg.AppendLine("</svg>");

                string caleFisier = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "parcare_map.svg");
                File.WriteAllText(caleFisier, svg.ToString());
                MessageBox.Show($"Harta SVG a fost exportată cu succes în:\n{caleFisier}", "Export SVG Reușit", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la export SVG: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportCSV_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder csv = new StringBuilder();
                csv.AppendLine("Numar Loc,Numar Inmatriculare,Tara,Data Intrare,Data Iesire,Durata Totala");

                foreach (var c in listaCamioaneActive)
                {
                    csv.AppendLine($"\"LOC #{c.NumarLoc:D2}\",\"{c.NumarInmatriculare}\",\"{c.Tara}\",\"{c.DataIntrare:yyyy-MM-dd HH:mm:ss}\",\"{(c.DataIesire.HasValue ? c.DataIesire.Value.ToString("yyyy-MM-dd HH:mm:ss") : "PARCAT")}\",\"{c.DurataTotala}\"");
                }

                string caleFisier = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "historique_parcare.csv");
                File.WriteAllText(caleFisier, csv.ToString());
                MessageBox.Show($"Raportul CSV a fost salvat în:\n{caleFisier}", "Export CSV Reușit", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la export CSV: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private const string FormatDataOra = "yyyy-MM-dd HH:mm:ss";

        private async Task SalvareLocalaFallbackAsync(string tipEveniment, Camion camion)
        {
            string textIesire = camion.DataIesire.HasValue
                ? camion.DataIesire.Value.ToString(FormatDataOra, CultureInfo.InvariantCulture)
                : "-";

            string linie = string.Join(" | ", new[]
            {
                DateTime.Now.ToString(FormatDataOra, CultureInfo.InvariantCulture),
                tipEveniment,
                $"Nr: {camion.NumarInmatriculare}",
                $"Tara: {camion.Tara}",
                $"Loc: {camion.NumarLoc}",
                $"Intrare: {camion.DataIntrare.ToString(FormatDataOra, CultureInfo.InvariantCulture)}",
                $"Iesire: {textIesire}",
                $"Durata: {camion.DurataTotala}"
            }) + Environment.NewLine;

            try
            {
                await File.AppendAllTextAsync(CaleFisierOffline, linie);
            }
            catch { }
        }

        private static Camion? ParseazaLinieBuffer(string linie, out string tip)
        {
            tip = "";
            var parti = linie.Split(" | ");
            if (parti.Length != 8) return null;

            try
            {
                tip = parti[1].Trim();
                string nr = parti[2].Replace("Nr:", "").Trim();
                string tara = parti[3].Replace("Tara:", "").Trim();
                int loc = int.Parse(parti[4].Replace("Loc:", "").Trim(), CultureInfo.InvariantCulture);
                DateTime intrare = DateTime.ParseExact(parti[5].Replace("Intrare:", "").Trim(), FormatDataOra, CultureInfo.InvariantCulture);
                string textIesire = parti[6].Replace("Iesire:", "").Trim();
                string durata = parti[7].Replace("Durata:", "").Trim();

                var camion = new Camion(nr, intrare, loc, tara);
                if (textIesire != "-")
                {
                    camion.DataIesire = DateTime.ParseExact(textIesire, FormatDataOra, CultureInfo.InvariantCulture);
                    camion.DurataTotala = durata;
                }
                return camion;
            }
            catch { return null; }
        }

        private async Task SincronizeazaBufferOfflineAsync()
        {
            if (!File.Exists(CaleFisierOffline)) return;

            string[] linii;
            try { linii = await File.ReadAllLinesAsync(CaleFisierOffline); }
            catch { return; }

            if (linii.Length == 0) return;

            var neretrimise = new List<string>();

            foreach (var linie in linii)
            {
                if (string.IsNullOrWhiteSpace(linie)) continue;

                bool trimisCuSucces = false;
                try
                {
                    Camion? camionReconstruit = ParseazaLinieBuffer(linie, out string tip);
                    if (camionReconstruit != null)
                    {
                        string json = JsonSerializer.Serialize(camionReconstruit);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = tip == "INTRARE"
                            ? await client.PostAsync($"{ApiUrl}/intrare", content)
                            : await client.PutAsync($"{ApiUrl}/iesire", content);

                        trimisCuSucces = response.IsSuccessStatusCode;
                    }
                }
                catch { trimisCuSucces = false; }

                if (!trimisCuSucces) neretrimise.Add(linie);
            }

            if (neretrimise.Count != linii.Length)
            {
                try { await File.WriteAllLinesAsync(CaleFisierOffline, neretrimise); }
                catch { }
            }
        }
    }

    // CLASA PROMPT DIALOG EXTRA-LARGĂ PENTRU TEXTE LUNGI
    public static class PromptDialog
    {
        public static string ShowDialog(string text, string caption, bool isPassword = false)
        {
            Form prompt = new Form()
            {
                Width = 560,
                Height = 240,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(245, 247, 248)
            };

            Label textLabel = new Label()
            {
                Left = 25,
                Top = 20,
                Width = 500,
                Height = 30,
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40)
            };

            TextBox textBox = new TextBox()
            {
                Left = 25,
                Top = 55,
                Width = 490,
                Height = 35,
                PasswordChar = isPassword ? '*' : '\0',
                Font = new Font("Segoe UI", 12F)
            };

            Button confirmation = new Button()
            {
                Text = "OK",
                Left = 285,
                Width = 110,
                Height = 38,
                Top = 120,
                DialogResult = DialogResult.OK,
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            Button cancel = new Button()
            {
                Text = "Anulează",
                Left = 405,
                Width = 110,
                Height = 38,
                Top = 120,
                DialogResult = DialogResult.Cancel,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.Controls.Add(textLabel);

            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }

}