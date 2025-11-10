namespace BibliotecaDAE.Formularios
{
    partial class frmPrestamos
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            btnRenovacion = new Button();
            btnPrestamo = new Button();
            btnLector = new Button();
            dgvPrestamo = new DataGridView();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPrestamo).BeginInit();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnRenovacion);
            groupBox1.Controls.Add(btnPrestamo);
            groupBox1.Controls.Add(btnLector);
            groupBox1.Location = new Point(20, 246);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(531, 100);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Opciones";
            // 
            // btnRenovacion
            // 
            btnRenovacion.Location = new Point(201, 60);
            btnRenovacion.Name = "btnRenovacion";
            btnRenovacion.Size = new Size(106, 23);
            btnRenovacion.TabIndex = 3;
            btnRenovacion.Text = "Renovaciones";
            btnRenovacion.UseVisualStyleBackColor = true;
            // 
            // btnPrestamo
            // 
            btnPrestamo.Location = new Point(17, 60);
            btnPrestamo.Name = "btnPrestamo";
            btnPrestamo.Size = new Size(75, 23);
            btnPrestamo.TabIndex = 1;
            btnPrestamo.Text = "Prestamo";
            btnPrestamo.UseVisualStyleBackColor = true;
            btnPrestamo.Click += btnPrestamo_Click_1;
            // 
            // btnLector
            // 
            btnLector.Location = new Point(111, 60);
            btnLector.Name = "btnLector";
            btnLector.Size = new Size(75, 23);
            btnLector.TabIndex = 2;
            btnLector.Text = "Lectores";
            btnLector.UseVisualStyleBackColor = true;
            btnLector.Click += btnLector_Click_1;
            // 
            // dgvPrestamo
            // 
            dgvPrestamo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvPrestamo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPrestamo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPrestamo.Location = new Point(0, -1);
            dgvPrestamo.Name = "dgvPrestamo";
            dgvPrestamo.Size = new Size(1047, 225);
            dgvPrestamo.TabIndex = 5;
            // 
            // frmPrestamos
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1046, 395);
            Controls.Add(groupBox1);
            Controls.Add(dgvPrestamo);
            Name = "frmPrestamos";
            Text = "Prestamos";
            Load += frmPrestamos_Load;
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPrestamo).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private Button btnRenovacion;
        private Button btnPrestamo;
        private Button btnLector;
        private DataGridView dgvPrestamo;
    }
}