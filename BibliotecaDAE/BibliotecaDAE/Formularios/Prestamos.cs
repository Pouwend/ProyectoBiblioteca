using Microsoft.Data.SqlClient;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Net.NetworkInformation;
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

        private string GetConnectionString()
        {
            try
            {
                var cs = ConfigurationManager.ConnectionStrings["Biblioteca"]?.ConnectionString;
                if (!string.IsNullOrWhiteSpace(cs)) return cs;
            }
            catch
            {
            }
            return "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Biblioteca;Integrated Security=True";
        }

        private async Task LoadPrestamosAsync()
        {
            var cs = GetConnectionString();
            var dt = new DataTable();
            try
            {
                using var cn = new SqlConnection(cs);
                using var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM dbo.Prestamo";
                await cn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);
                dgvPrestamo.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando Préstamos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvPrestamo_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
        }

        private void frmPrestamos_Load(object sender, EventArgs e)
        {

        }

        private async void btnPrestamo_Click_1(object sender, EventArgs e)
        {
            using var detalle = new PrestamoDetalle();
            detalle.ShowDialog(this);
            await LoadPrestamosAsync();
        }

        private void btnLector_Click_1(object sender, EventArgs e)
        {

        }
    }
}