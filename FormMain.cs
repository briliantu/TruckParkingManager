using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TruckParkingManager
{
    public partial class FormMain : Form
    {
        private static readonly HttpClient client = new HttpClient();
        
        // Modifică aici cu URL-ul serverului companiei tale
        private const string ApiUrl = "https://api.compania-ta.ro/parcare";
        private const string CaleFisierOffline = "offline_buffer.txt";
        
        private List<Camion> listaCamioaneActive = new List<Camion>();

        // CONFIGURARE CAPACITATE PARCARE
        private const int CAPACITATE_MAXIMA = 50; 
        private bool[] locuriParcare = new bool[CAPACITATE_MAXIMA + 1]; // false = liber, true = ocupat

        public FormMain()
        {
            InitializeComponent();
            client.Timeout = TimeSpan.FromSeconds(8);
            ConfigurareTabel();
            _ = IncarcaCamioaneActive();
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

        private int GetLocuriLibere()
        {
            int ocupate = 0;
            foreach (var camion in listaCamioaneActive)
            {
                if (camion.DataIesire == null) ocupate++;
            }
            return CAPACITATE_MAXIMA - ocupate;
        }

        private int GasestePrimulLocLiber()
        {
            for (int i = 1; i <= CAPACITATE_MAXIMA; i++)
            {
                if (!locuriParcare[i]) return i;
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

            if (GetLocuriLibere() <= 0)
            {
                MessageBox.Show("Parcarea este plină!", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            int locAlocat = GasestePrimulLocLiber();
            if (locAlocat == -1) return;

            var nouCamion = new Camion(nrInmatriculare, DateTime.Now, locAlocat);
            locuriParcare[locAlocat] = true;

            ActioneazaBariera();

            string json = JsonSerializer.Serialize(nouCamion);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync($"{ApiUrl}/intrare", content);
                if (!response.IsSuccessStatusCode)
                {
                    SalvareLocalaFallback(nrInmatriculare, $"INTRARE|LOC:{locAlocat}");
                }
            }
            catch (HttpRequestException)
            {
                SalvareLocalaFallback(nrInmatriculare, $"INTRARE|LOC:{locAlocat}");
            }

            listaCamioaneActive.Add(nouCamion);
            ActualizeazaGridVizual();
            ActualizeazaAfisajLocuriDisponibile();
            txtNrInmatriculare.Clear();
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

            if (camionGasit != null)
            {
                camionGasit.FinalizeazaStationare(DateTime.Now);
                locuriParcare[camionGasit.NumarLoc] = false;

                ActioneazaBariera();

                string json = JsonSerializer.Serialize(camionGasit);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PutAsync($"{ApiUrl}/iesire", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        SalvareLocalaFallback(nrInmatriculare, $"IESIRE|LOC_ELIBERAT:{camionGasit.NumarLoc}");
                    }
                }
                catch (HttpRequestException)
                {
                    SalvareLocalaFallback(nrInmatriculare, $"IESIRE|LOC_ELIBERAT:{camionGasit.NumarLoc}");
                }

                ActualizeazaGridVizual();
                ActualizeazaAfisajLocuriDisponibile();
                txtNrInmatriculare.Clear();
            }
            else
            {
                MessageBox.Show("Camionul nu a fost găsit în parcare.", "Informație", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
                    Array.Clear(locuriParcare, 0, locuriParcare.Length);
                    foreach (var c in listaCamioaneActive)
                    {
                        if (c.DataIesire == null)
                        {
                            locuriParcare[c.NumarLoc] = true;
                        }
                    }
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

        private void SalvareLocalaFallback(string nrInmatriculare, string tipEveniment)
        {
            string linie = $"{nrInmatriculare},{DateTime.Now:yyyy-MM-dd HH:mm:ss},{tipEveniment}\n";
            File.AppendAllText(CaleFisierOffline, linie);
        }
    }
}