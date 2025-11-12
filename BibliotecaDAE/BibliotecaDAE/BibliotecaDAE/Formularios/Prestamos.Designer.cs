
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
            groupBox2 = new GroupBox();
            cbEstado = new ComboBox();
            btnGuardar = new Button();
            dtReal = new DateTimePicker();
            dtDevolucion = new DateTimePicker();
            dtPrestamo = new DateTimePicker();
            label11 = new Label();
            txtNEjemplar = new TextBox();
            txtIdEjemplar = new TextBox();
            label10 = new Label();
            label9 = new Label();
            label8 = new Label();
            label7 = new Label();
            label6 = new Label();
            txtNLector = new TextBox();
            txtIdLector = new TextBox();
            txtNUsuario = new TextBox();
            txtIdUsuario = new TextBox();
            txtIdPrestamo = new TextBox();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            btnLimpiar = new Button();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPrestamo).BeginInit();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnRenovacion);
            groupBox1.Controls.Add(btnPrestamo);
            groupBox1.Controls.Add(btnLector);
            groupBox1.Location = new Point(20, 246);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(445, 100);
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
            btnPrestamo.Text = "Prestamos";
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
            dgvPrestamo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dgvPrestamo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPrestamo.Location = new Point(0, -1);
            dgvPrestamo.Name = "dgvPrestamo";
            dgvPrestamo.Size = new Size(1198, 225);
            dgvPrestamo.TabIndex = 5;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(btnLimpiar);
            groupBox2.Controls.Add(cbEstado);
            groupBox2.Controls.Add(btnGuardar);
            groupBox2.Controls.Add(dtReal);
            groupBox2.Controls.Add(dtDevolucion);
            groupBox2.Controls.Add(dtPrestamo);
            groupBox2.Controls.Add(label11);
            groupBox2.Controls.Add(txtNEjemplar);
            groupBox2.Controls.Add(txtIdEjemplar);
            groupBox2.Controls.Add(label10);
            groupBox2.Controls.Add(label9);
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(txtNLector);
            groupBox2.Controls.Add(txtIdLector);
            groupBox2.Controls.Add(txtNUsuario);
            groupBox2.Controls.Add(txtIdUsuario);
            groupBox2.Controls.Add(txtIdPrestamo);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(label1);
            groupBox2.Location = new Point(471, 246);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(714, 429);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            groupBox2.Text = "Crear / Actualizar Prestamos";
            groupBox2.Enter += groupBox2_Enter;
            // 
            // cbEstado
            // 
            cbEstado.FormattingEnabled = true;
            cbEstado.Items.AddRange(new object[] { "Devuelto", "Activo", "Renovado" });
            cbEstado.Location = new Point(524, 146);
            cbEstado.Name = "cbEstado";
            cbEstado.Size = new Size(121, 23);
            cbEstado.TabIndex = 26;
            cbEstado.SelectedIndexChanged += cbEstado_SelectedIndexChanged;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(472, 378);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(75, 23);
            btnGuardar.TabIndex = 24;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // dtReal
            // 
            dtReal.Location = new Point(146, 376);
            dtReal.Name = "dtReal";
            dtReal.Size = new Size(233, 23);
            dtReal.TabIndex = 23;
            // 
            // dtDevolucion
            // 
            dtDevolucion.Location = new Point(146, 327);
            dtDevolucion.Name = "dtDevolucion";
            dtDevolucion.Size = new Size(233, 23);
            dtDevolucion.TabIndex = 22;
            // 
            // dtPrestamo
            // 
            dtPrestamo.Location = new Point(146, 269);
            dtPrestamo.Name = "dtPrestamo";
            dtPrestamo.Size = new Size(233, 23);
            dtPrestamo.TabIndex = 21;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(472, 151);
            label11.Name = "label11";
            label11.Size = new Size(45, 15);
            label11.TabIndex = 18;
            label11.Text = "Estado:";
            // 
            // txtNEjemplar
            // 
            txtNEjemplar.Location = new Point(352, 205);
            txtNEjemplar.Name = "txtNEjemplar";
            txtNEjemplar.Size = new Size(100, 23);
            txtNEjemplar.TabIndex = 17;
            // 
            // txtIdEjemplar
            // 
            txtIdEjemplar.Location = new Point(102, 210);
            txtIdEjemplar.Name = "txtIdEjemplar";
            txtIdEjemplar.Size = new Size(100, 23);
            txtIdEjemplar.TabIndex = 15;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(25, 382);
            label10.Name = "label10";
            label10.Size = new Size(95, 15);
            label10.TabIndex = 14;
            label10.Text = "Devolucion Real:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(25, 333);
            label9.Name = "label9";
            label9.Size = new Size(101, 15);
            label9.TabIndex = 13;
            label9.Text = "Fecha Devolucion";
            label9.Click += label9_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(25, 275);
            label8.Name = "label8";
            label8.Size = new Size(113, 15);
            label8.TabIndex = 12;
            label8.Text = "Fecha del Prestamo:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(227, 213);
            label7.Name = "label7";
            label7.Size = new Size(119, 15);
            label7.TabIndex = 11;
            label7.Text = "Nombre del Ejemplar";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(27, 213);
            label6.Name = "label6";
            label6.Size = new Size(69, 15);
            label6.TabIndex = 10;
            label6.Text = "Id Ejemplar:";
            // 
            // txtNLector
            // 
            txtNLector.Location = new Point(330, 143);
            txtNLector.Name = "txtNLector";
            txtNLector.Size = new Size(122, 23);
            txtNLector.TabIndex = 9;
            // 
            // txtIdLector
            // 
            txtIdLector.Location = new Point(102, 138);
            txtIdLector.Name = "txtIdLector";
            txtIdLector.Size = new Size(100, 23);
            txtIdLector.TabIndex = 8;
            // 
            // txtNUsuario
            // 
            txtNUsuario.Location = new Point(330, 82);
            txtNUsuario.Name = "txtNUsuario";
            txtNUsuario.Size = new Size(122, 23);
            txtNUsuario.TabIndex = 7;
            // 
            // txtIdUsuario
            // 
            txtIdUsuario.Location = new Point(102, 82);
            txtIdUsuario.Name = "txtIdUsuario";
            txtIdUsuario.Size = new Size(100, 23);
            txtIdUsuario.TabIndex = 6;
            // 
            // txtIdPrestamo
            // 
            txtIdPrestamo.Location = new Point(102, 26);
            txtIdPrestamo.Name = "txtIdPrestamo";
            txtIdPrestamo.Size = new Size(100, 23);
            txtIdPrestamo.TabIndex = 5;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(227, 151);
            label5.Name = "label5";
            label5.Size = new Size(90, 15);
            label5.TabIndex = 4;
            label5.Text = "Nombre Lector:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(29, 146);
            label4.Name = "label4";
            label4.Size = new Size(56, 15);
            label4.TabIndex = 3;
            label4.Text = "Id Lector:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(227, 90);
            label3.Name = "label3";
            label3.Size = new Size(97, 15);
            label3.TabIndex = 2;
            label3.Text = "Nombre Usuario:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(29, 90);
            label2.Name = "label2";
            label2.Size = new Size(63, 15);
            label2.TabIndex = 1;
            label2.Text = "Id Usuario:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(25, 34);
            label1.Name = "label1";
            label1.Size = new Size(67, 15);
            label1.TabIndex = 0;
            label1.Text = "IdPrestamo";
            // 
            // btnLimpiar
            // 
            btnLimpiar.Location = new Point(570, 378);
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.Size = new Size(75, 23);
            btnLimpiar.TabIndex = 27;
            btnLimpiar.Text = "Limpiar";
            btnLimpiar.UseVisualStyleBackColor = true;
            btnLimpiar.Click += btnLimpiar_Click;
            // 
            // frmPrestamos
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1197, 687);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(dgvPrestamo);
            Name = "frmPrestamos";
            Text = "Prestamos";
            Load += frmPrestamos_Load;
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPrestamo).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        private void frmPrestamos_Load(object sender, EventArgs e)
        {  
        }

        #endregion

        private GroupBox groupBox1;
        private Button btnRenovacion;
        private Button btnPrestamo;
        private Button btnLector;
        private DataGridView dgvPrestamo;
        private GroupBox groupBox2;
        private TextBox txtNLector;
        private TextBox txtIdLector;
        private TextBox txtNUsuario;
        private TextBox txtIdUsuario;
        private TextBox txtIdPrestamo;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private TextBox txtNEjemplar;
        private TextBox txtIdEjemplar;
        private Label label10;
        private Label label9;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label11;
        private DateTimePicker dtDevolucion;
        private DateTimePicker dtPrestamo;
        private DateTimePicker dtReal;
        private Button btnGuardar;
        private ComboBox cbEstado;
        private Button btnLimpiar;
    }
}