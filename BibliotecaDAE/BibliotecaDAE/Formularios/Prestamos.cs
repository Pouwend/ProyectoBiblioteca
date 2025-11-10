using Biblioteca;
using Microsoft.Data.SqlClient;
using SistemaControlPersonal.Core.Lib;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BibliotecaDAE.Formularios
{
    public partial class frmPrestamos : Form
    {
        public frmPrestamos()
        {
            InitializeComponent();
            this.Load += async (_, __) => await LoadPrestamosAsync();

            if (dgvPrestamo != null)
            {
                dgvPrestamo.ReadOnly = true;
                dgvPrestamo.AllowUserToAddRows = false;
                dgvPrestamo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvPrestamo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvPrestamo.MultiSelect = false;
            }
        }

        private async Task LoadPrestamosAsync()
        {
            var dt = new DataTable();
            Cnn cnn = null;

            try
            {
                // Crear instancia de la conexión
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        p.*,
                        l.Nombre + ' ' + l.Apellido AS NombreLector
                    FROM dbo.Prestamo p
                    INNER JOIN dbo.Lector l ON p.IdLector = l.IdLector
                    ORDER BY p.IdPrestamo DESC";

                using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);

                dgvPrestamo.DataSource = dt;

                // Configurar apariencia de columnas
                if (dgvPrestamo.Columns.Contains("IdPrestamo"))
                {
                    dgvPrestamo.Columns["IdPrestamo"].HeaderText = "ID Préstamo";
                    dgvPrestamo.Columns["IdPrestamo"].Width = 90;
                }
                if (dgvPrestamo.Columns.Contains("IdLector"))
                {
                    dgvPrestamo.Columns["IdLector"].HeaderText = "ID Lector";
                    dgvPrestamo.Columns["IdLector"].Width = 80;
                }
                if (dgvPrestamo.Columns.Contains("NombreLector"))
                {
                    dgvPrestamo.Columns["NombreLector"].HeaderText = "Nombre del Lector";
                    dgvPrestamo.Columns["NombreLector"].Width = 200;
                }
                if (dgvPrestamo.Columns.Contains("IdEjemplar"))
                {
                    dgvPrestamo.Columns["IdEjemplar"].HeaderText = "ID Ejemplar";
                    dgvPrestamo.Columns["IdEjemplar"].Width = 90;
                }
                if (dgvPrestamo.Columns.Contains("FechaPrestamo"))
                {
                    dgvPrestamo.Columns["FechaPrestamo"].HeaderText = "Fecha Préstamo";
                    dgvPrestamo.Columns["FechaPrestamo"].Width = 120;
                }
                if (dgvPrestamo.Columns.Contains("FechaDevolucionEsperada"))
                {
                    dgvPrestamo.Columns["FechaDevolucionEsperada"].HeaderText = "Devolución Esperada";
                    dgvPrestamo.Columns["FechaDevolucionEsperada"].Width = 130;
                }
                if (dgvPrestamo.Columns.Contains("FechaDevolucionReal"))
                {
                    dgvPrestamo.Columns["FechaDevolucionReal"].HeaderText = "Devolución Real";
                    dgvPrestamo.Columns["FechaDevolucionReal"].Width = 120;
                }
                if (dgvPrestamo.Columns.Contains("EstadoPrestamo"))
                {
                    dgvPrestamo.Columns["EstadoPrestamo"].HeaderText = "Estado";
                    dgvPrestamo.Columns["EstadoPrestamo"].Width = 100;
                }
                if (dgvPrestamo.Columns.Contains("Observaciones"))
                {
                    dgvPrestamo.Columns["Observaciones"].HeaderText = "Observaciones";
                    dgvPrestamo.Columns["Observaciones"].Width = 200;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando Préstamos: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Asegurar que la conexión se cierre
                if (cnn != null)
                {
                    cnn.CloseDB();
                }
            }
        }

        private void dgvPrestamo_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Implementar si necesitas acciones al hacer clic en una celda
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            // Evento del GroupBox si lo necesitas
        }

        private void frmPrestamos_Load(object sender, EventArgs e)
        {
            // Ya se carga en el constructor, pero puedes agregar más lógica aquí
        }

        private async void btnPrestamo_Click_1(object sender, EventArgs e)
        {
            using var detalle = new PrestamoDetalle();
            detalle.ShowDialog(this);
            await LoadPrestamosAsync();
        }

        private async void btnLector_Click_1(object sender, EventArgs e)
        {
            using var lectorForm = new FrmLectores();

            lectorForm.ShowDialog(this);

            await LoadPrestamosAsync();
        }
    }
}