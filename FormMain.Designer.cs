namespace ParcareCamioane
{
    partial class FormMain
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtNrInmatriculare;
        private System.Windows.Forms.Button btnIntrare;
        private System.Windows.Forms.Button btnIesire;
        private System.Windows.Forms.DataGridView dgvCamioane;
        private System.Windows.Forms.Label lblTitlu;

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
            this.btnIntrare = new System.Windows.Forms.Button();
            this.btnIesire = new System.Windows.Forms.Button();
            this.dgvCamioane = new System.Windows.Forms.DataGridView();
            this.lblTitlu = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCamioane)).BeginInit();
            this.SuspendLayout();
            // 
            // txtNrInmatriculare
            // 
            this.txtNrInmatriculare.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtNrInmatriculare.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.txtNrInmatriculare.Location = new System.Drawing.Point(30, 80);
            this.txtNrInmatriculare.Name = "txtNrInmatriculare";
            this.txtNrInmatriculare.Size = new System.Drawing.Size(260, 36);
            this.txtNrInmatriculare.TabIndex = 0;
            // 
            // btnIntrare
            // 
            this.btnIntrare.BackColor = System.Drawing.Color.ForestGreen;
            this.btnIntrare.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIntrare.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnIntrare.ForeColor = System.Drawing.Color.White;
            this.btnIntrare.Location = new System.Drawing.Point(310, 80);
            this.btnIntrare.Name = "btnIntrare";
            this.btnIntrare.Size = new System.Drawing.Size(140, 36);
            this.btnIntrare.TabIndex = 1;
            this.btnIntrare.Text = "INTRARE";
            this.btnIntrare.UseVisualStyleBackColor = false;
            this.btnIntrare.Click += new System.EventHandler(this.btnIntrare_Click);
            // 
            // btnIesire
            // 
            this.btnIesire.BackColor = System.Drawing.Color.Crimson;
            this.btnIesire.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIesire.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnIesire.ForeColor = System.Drawing.Color.White;
            this.btnIesire.Location = new System.Drawing.Point(460, 80);
            this.btnIesire.Name = "btnIesire";
            this.btnIesire.Size = new System.Drawing.Size(140, 36);
            this.btnIesire.TabIndex = 2;
            this.btnIesire.Text = "IEȘIRE";
            this.btnIesire.UseVisualStyleBackColor = false;
            this.btnIesire.Click += new System.EventHandler(this.btnIesire_Click);
            // 
            // dgvCamioane
            // 
            this.dgvCamioane.BackgroundColor = System.Drawing.Color.White;
            this.dgvCamioane.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCamioane.Location = new System.Drawing.Point(30, 140);
            this.dgvCamioane.Name = "dgvCamioane";
            this.dgvCamioane.Size = new System.Drawing.Size(720, 280);
            this.dgvCamioane.TabIndex = 3;
            // 
            // lblTitlu
            // 
            this.lblTitlu.AutoSize = true;
            this.lblTitlu.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitlu.Location = new System.Drawing.Point(24, 25);
            this.lblTitlu.Name = "lblTitlu";
            this.lblTitlu.Size = new System.Drawing.Size(434, 32);
            this.lblTitlu.TabIndex = 4;
            this.lblTitlu.Text = "Sistem Control Parcare Companie";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 450);
            this.Controls.Add(this.lblTitlu);
            this.Controls.Add(this.dgvCamioane);
            this.Controls.Add(this.btnIesire);
            this.Controls.Add(this.btnIntrare);
            this.Controls.Add(this.txtNrInmatriculare);
            this.Name = "FormMain";
            this.Text = "Gestiune Parcare v2.0";
            ((System.ComponentModel.ISupportInitialize)(this.dgvCamioane)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}