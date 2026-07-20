namespace TruckParkingManager
{
    partial class FormMain
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtNrInmatriculare;
        private System.Windows.Forms.ComboBox cmbTara;
        private System.Windows.Forms.Button btnIntrare;
        private System.Windows.Forms.Button btnIesire;
        private System.Windows.Forms.DataGridView dgvCamioane;
        private System.Windows.Forms.Label lblTitlu;
        private System.Windows.Forms.Panel pnlHarta;
        private System.Windows.Forms.Label lblHarta;

        private System.Windows.Forms.Label lblNrInmatriculare;
        private System.Windows.Forms.Label lblTara;

        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblKpiLibere;
        private System.Windows.Forms.Label lblKpiOcupate;
        private System.Windows.Forms.Label lblKpiTotal;
        private System.Windows.Forms.Label lblKpiCumulat;
        private System.Windows.Forms.Button btnExportSVG;
        private System.Windows.Forms.Button btnExportCSV;

        private System.Windows.Forms.Button btnLoginAdmin;
        private System.Windows.Forms.Button btnClearLogs;
        private System.Windows.Forms.Button btnMutaCamion;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtNrInmatriculare = new System.Windows.Forms.TextBox();
            this.cmbTara = new System.Windows.Forms.ComboBox();
            this.btnIntrare = new System.Windows.Forms.Button();
            this.btnIesire = new System.Windows.Forms.Button();
            this.dgvCamioane = new System.Windows.Forms.DataGridView();
            this.lblTitlu = new System.Windows.Forms.Label();
            this.pnlHarta = new System.Windows.Forms.Panel();
            this.lblHarta = new System.Windows.Forms.Label();
            this.lblNrInmatriculare = new System.Windows.Forms.Label();
            this.lblTara = new System.Windows.Forms.Label();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.btnExportSVG = new System.Windows.Forms.Button();
            this.btnExportCSV = new System.Windows.Forms.Button();
            this.btnLoginAdmin = new System.Windows.Forms.Button();
            this.btnClearLogs = new System.Windows.Forms.Button();
            this.btnMutaCamion = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.dgvCamioane)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // picLogo
            // 
            this.picLogo.Location = new System.Drawing.Point(24, 12);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(120, 45);
            this.picLogo.TabIndex = 10;
            this.picLogo.TabStop = false;
            // 
            // lblTitlu
            // 
            this.lblTitlu.AutoSize = true;
            this.lblTitlu.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitlu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(43)))), ((int)(((byte)(73)))));
            this.lblTitlu.Location = new System.Drawing.Point(150, 20);
            this.lblTitlu.Name = "lblTitlu";
            this.lblTitlu.Size = new System.Drawing.Size(225, 28);
            this.lblTitlu.TabIndex = 4;
            this.lblTitlu.Text = "Truck Parking Manager";
            // 
            // btnLoginAdmin
            // 
            this.btnLoginAdmin.BackColor = System.Drawing.Color.Orange;
            this.btnLoginAdmin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoginAdmin.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.btnLoginAdmin.ForeColor = System.Drawing.Color.White;
            this.btnLoginAdmin.Location = new System.Drawing.Point(465, 20);
            this.btnLoginAdmin.Name = "btnLoginAdmin";
            this.btnLoginAdmin.Size = new System.Drawing.Size(110, 28);
            this.btnLoginAdmin.TabIndex = 13;
            this.btnLoginAdmin.Text = "ðŸ”‘ Login Admin";
            this.btnLoginAdmin.UseVisualStyleBackColor = false;
            this.btnLoginAdmin.Click += new System.EventHandler(this.btnLoginAdmin_Click);
            // 
            // lblNrInmatriculare
            // 
            this.lblNrInmatriculare.AutoSize = true;
            this.lblNrInmatriculare.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblNrInmatriculare.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.lblNrInmatriculare.Location = new System.Drawing.Point(24, 68);
            this.lblNrInmatriculare.Name = "lblNrInmatriculare";
            this.lblNrInmatriculare.Size = new System.Drawing.Size(155, 15);
            this.lblNrInmatriculare.TabIndex = 11;
            this.lblNrInmatriculare.Text = "Introdu nr. de Ã®nmatriculare";
            // 
            // txtNrInmatriculare
            // 
            this.txtNrInmatriculare.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtNrInmatriculare.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtNrInmatriculare.Location = new System.Drawing.Point(24, 88);
            this.txtNrInmatriculare.Name = "txtNrInmatriculare";
            this.txtNrInmatriculare.Size = new System.Drawing.Size(180, 29);
            this.txtNrInmatriculare.TabIndex = 0;
            // 
            // lblTara
            // 
            this.lblTara.AutoSize = true;
            this.lblTara.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTara.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.lblTara.Location = new System.Drawing.Point(215, 68);
            this.lblTara.Name = "lblTara";
            this.lblTara.Size = new System.Drawing.Size(31, 15);
            this.lblTara.TabIndex = 12;
            this.lblTara.Text = "ÈšarÄƒ";
            // 
            // cmbTara
            // 
            this.cmbTara.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTara.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbTara.FormattingEnabled = true;
            this.cmbTara.Items.AddRange(new object[] {
            "RomÃ¢nia",
            "Spania",
            "Germania",
            "Italia",
            "Bulgaria",
            "Turcia",
            "Republica Moldova"});
            this.cmbTara.Location = new System.Drawing.Point(215, 90);
            this.cmbTara.Name = "cmbTara";
            this.cmbTara.Size = new System.Drawing.Size(130, 25);
            this.cmbTara.TabIndex = 7;
            // 
            // btnIntrare
            // 
            this.btnIntrare.BackColor = System.Drawing.Color.ForestGreen;
            this.btnIntrare.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIntrare.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnIntrare.ForeColor = System.Drawing.Color.White;
            this.btnIntrare.Location = new System.Drawing.Point(355, 87);
            this.btnIntrare.Name = "btnIntrare";
            this.btnIntrare.Size = new System.Drawing.Size(110, 31);
            this.btnIntrare.TabIndex = 1;
            this.btnIntrare.Text = "INTRARE";
            this.btnIntrare.UseVisualStyleBackColor = false;
            this.btnIntrare.Click += new System.EventHandler(this.btnIntrare_Click);
            // 
            // btnIesire
            // 
            this.btnIesire.BackColor = System.Drawing.Color.Crimson;
            this.btnIesire.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIesire.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnIesire.ForeColor = System.Drawing.Color.White;
            this.btnIesire.Location = new System.Drawing.Point(473, 87);
            this.btnIesire.Name = "btnIesire";
            this.btnIesire.Size = new System.Drawing.Size(110, 31);
            this.btnIesire.TabIndex = 2;
            this.btnIesire.Text = "IEÈ˜IRE";
            this.btnIesire.UseVisualStyleBackColor = false;
            this.btnIesire.Click += new System.EventHandler(this.btnIesire_Click);
            // 
            // DASHBOARD KPI CARDS CU ETICHETE
            // 
            this.Controls.Add(CreazaKpiCard("LIBERE", "0", System.Drawing.Color.ForestGreen, new System.Drawing.Point(580, 12), out this.lblKpiLibere));
            this.Controls.Add(CreazaKpiCard("OCUPATE", "0", System.Drawing.Color.Crimson, new System.Drawing.Point(682, 12), out this.lblKpiOcupate));
            this.Controls.Add(CreazaKpiCard("TOTAL", "50", System.Drawing.Color.SteelBlue, new System.Drawing.Point(784, 12), out this.lblKpiTotal));
            this.Controls.Add(CreazaKpiCard("CUMULAT", "0", System.Drawing.Color.DarkOrange, new System.Drawing.Point(886, 12), out this.lblKpiCumulat));
            // 
            // dgvCamioane
            // 
            this.dgvCamioane.BackgroundColor = System.Drawing.Color.White;
            this.dgvCamioane.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCamioane.Location = new System.Drawing.Point(24, 130);
            this.dgvCamioane.Name = "dgvCamioane";
            this.dgvCamioane.Size = new System.Drawing.Size(660, 360);
            this.dgvCamioane.TabIndex = 3;
            // 
            // lblHarta
            // 
            this.lblHarta.AutoSize = true;
            this.lblHarta.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblHarta.Location = new System.Drawing.Point(700, 105);
            this.lblHarta.Name = "lblHarta";
            this.lblHarta.Size = new System.Drawing.Size(148, 19);
            this.lblHarta.TabIndex = 5;
            this.lblHarta.Text = "HartÄƒ Locuri Parcare";
            // 
            // pnlHarta
            // 
            this.pnlHarta.AutoScroll = true;
            this.pnlHarta.BackColor = System.Drawing.Color.White;
            this.pnlHarta.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlHarta.Location = new System.Drawing.Point(700, 130);
            this.pnlHarta.Name = "pnlHarta";
            this.pnlHarta.Size = new System.Drawing.Size(280, 360);
            this.pnlHarta.TabIndex = 6;
            // 
            // btnMutaCamion
            // 
            this.btnMutaCamion.BackColor = System.Drawing.Color.Orange;
            this.btnMutaCamion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMutaCamion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnMutaCamion.ForeColor = System.Drawing.Color.White;
            this.btnMutaCamion.Location = new System.Drawing.Point(420, 500);
            this.btnMutaCamion.Name = "btnMutaCamion";
            this.btnMutaCamion.Size = new System.Drawing.Size(125, 32);
            this.btnMutaCamion.TabIndex = 15;
            this.btnMutaCamion.Text = "ðŸ”„ MutÄƒ Camion";
            this.btnMutaCamion.UseVisualStyleBackColor = false;
            this.btnMutaCamion.Visible = false;
            this.btnMutaCamion.Click += new System.EventHandler(this.btnMutaCamion_Click);
            // 
            // btnClearLogs
            // 
            this.btnClearLogs.BackColor = System.Drawing.Color.Crimson;
            this.btnClearLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearLogs.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnClearLogs.ForeColor = System.Drawing.Color.White;
            this.btnClearLogs.Location = new System.Drawing.Point(555, 500);
            this.btnClearLogs.Name = "btnClearLogs";
            this.btnClearLogs.Size = new System.Drawing.Size(130, 32);
            this.btnClearLogs.TabIndex = 14;
            this.btnClearLogs.Text = "ðŸ§¹ Clear Logs";
            this.btnClearLogs.UseVisualStyleBackColor = false;
            this.btnClearLogs.Visible = false;
            this.btnClearLogs.Click += new System.EventHandler(this.btnClearLogs_Click);
            // 
            // btnExportSVG
            // 
            this.btnExportSVG.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(68)))), ((int)(((byte)(173)))));
            this.btnExportSVG.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportSVG.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExportSVG.ForeColor = System.Drawing.Color.White;
            this.btnExportSVG.Location = new System.Drawing.Point(700, 500);
            this.btnExportSVG.Name = "btnExportSVG";
            this.btnExportSVG.Size = new System.Drawing.Size(135, 32);
            this.btnExportSVG.TabIndex = 8;
            this.btnExportSVG.Text = "ðŸ“¥ Export SVG";
            this.btnExportSVG.UseVisualStyleBackColor = false;
            this.btnExportSVG.Click += new System.EventHandler(this.btnExportSVG_Click);
            // 
            // btnExportCSV
            // 
            this.btnExportCSV.BackColor = System.Drawing.Color.ForestGreen;
            this.btnExportCSV.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportCSV.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExportCSV.ForeColor = System.Drawing.Color.White;
            this.btnExportCSV.Location = new System.Drawing.Point(845, 500);
            this.btnExportCSV.Name = "btnExportCSV";
            this.btnExportCSV.Size = new System.Drawing.Size(135, 32);
            this.btnExportCSV.TabIndex = 9;
            this.btnExportCSV.Text = "ðŸ“Š Export CSV";
            this.btnExportCSV.UseVisualStyleBackColor = false;
            this.btnExportCSV.Click += new System.EventHandler(this.btnExportCSV_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1005, 545);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Controls.Add(this.btnMutaCamion);
            this.Controls.Add(this.btnClearLogs);
            this.Controls.Add(this.btnLoginAdmin);
            this.Controls.Add(this.lblNrInmatriculare);
            this.Controls.Add(this.lblTara);
            this.Controls.Add(this.btnExportCSV);
            this.Controls.Add(this.btnExportSVG);
            this.Controls.Add(this.picLogo);
            this.Controls.Add(this.lblTitlu);
            this.Controls.Add(this.lblHarta);
            this.Controls.Add(this.pnlHarta);
            this.Controls.Add(this.dgvCamioane);
            this.Controls.Add(this.btnIesire);
            this.Controls.Add(this.btnIntrare);
            this.Controls.Add(this.cmbTara);
            this.Controls.Add(this.txtNrInmatriculare);
            this.Name = "FormMain";
            this.Text = "TruckParkingManager - De'Longhi";
            ((System.ComponentModel.ISupportInitialize)(this.dgvCamioane)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Panel CreazaKpiCard(string titlu, string valoareInitiala, System.Drawing.Color culoare, System.Drawing.Point loc, out System.Windows.Forms.Label lblValoare)
        {
            var pnl = new System.Windows.Forms.Panel
            {
                BackColor = culoare,
                Location = loc,
                Size = new System.Drawing.Size(95, 45)
            };

            var lblTitlu = new System.Windows.Forms.Label
            {
                Text = titlu,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 7F, System.Drawing.FontStyle.Bold),
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 16,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            lblValoare = new System.Windows.Forms.Label
            {
                Text = valoareInitiala,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold),
                Dock = System.Windows.Forms.DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            pnl.Controls.Add(lblValoare);
            pnl.Controls.Add(lblTitlu);
            return pnl;
        }
    }
}