using Biblioteca;
using Microsoft.Data.SqlClient;
using SistemaControlPersonal.Core.Lib;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using BibliotecaDAE.Formularios;

namespace BibliotecaDAE.Formularios
{
    public partial class frmPrestamos : Form
    {

        public frmPrestamos()
        {
            InitializeComponent();

            // Cargar los prestamos cuando se abre el formulario
            this.Load += async (_, __) => await LoadPrestamosAsync();

            // Configurar el DataGridView
            if (dgvPrestamo != null)
            {
                dgvPrestamo.ReadOnly = true;
                dgvPrestamo.AllowUserToAddRows = false;
                dgvPrestamo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvPrestamo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvPrestamo.MultiSelect = false;

                dgvPrestamo.SelectionChanged += dgvPrestamo_SelectionChanged;
            }

            // Limpiar campos al iniciar el formulario
            LimpiarCampos();

            // Cargar datos del usuario logueado
            CargarDatosUsuarioLogueado();

            // Configurar eventos para autocompletar
            ConfigurarEventosAutocompletar();
        }

        private void CargarDatosUsuarioLogueado()
        {
            // Cargar datos del usuario desde la sesión
            if (SesionUsuario.IdUsuario > 0)
            {
                txtIdUsuario.Text = SesionUsuario.IdUsuario.ToString();
                txtNUsuario.Text = SesionUsuario.Nombre;
            }
        }

        private void ConfigurarEventosAutocompletar()
        {
            // Evento cuando cambia el texto de IdEjemplar
            if (txtIdEjemplar != null)
            {
                txtIdEjemplar.Leave += async (_, __) => await CargarNombreEjemplarAsync();
            }

            // Evento cuando cambia el texto de IdLector
            if (txtIdLector != null)
            {
                txtIdLector.Leave += async (_, __) => await CargarNombreLectorAsync();
            }
        }

        private async Task CargarNombreEjemplarAsync()
        {
            if (string.IsNullOrWhiteSpace(txtIdEjemplar.Text))
            {
                txtNEjemplar.Clear();
                return;
            }

            if (!int.TryParse(txtIdEjemplar.Text, out int idEjemplar))
            {
                txtNEjemplar.Clear();
                return;
            }

            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = @"
                    SELECT l.Titulo 
                    FROM dbo.Ejemplares e
                    INNER JOIN dbo.Libros l ON e.IdLibro = l.IdLibros
                    WHERE e.IdEjemplar = @idEjemplar";

                cmd.Parameters.AddWithValue("@idEjemplar", idEjemplar);

                var result = await cmd.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    txtNEjemplar.Text = result.ToString();
                }
                else
                {
                    txtNEjemplar.Clear();
                    MessageBox.Show("No se encontró el ejemplar con ese ID.", "Advertencia",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error buscando ejemplar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null)
                {
                    cnn.CloseDB();
                }
            }
        }

        private async Task CargarNombreLectorAsync()
        {
            if (string.IsNullOrWhiteSpace(txtIdLector.Text))
            {
                txtNLector.Clear();
                return;
            }

            if (!int.TryParse(txtIdLector.Text, out int idLector))
            {
                txtNLector.Clear();
                return;
            }

            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = @"
                    SELECT Nombre + ' ' + Apellido AS NombreCompleto
                    FROM dbo.Lector
                    WHERE IdLector = @idLector";

                cmd.Parameters.AddWithValue("@idLector", idLector);

                var result = await cmd.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    txtNLector.Text = result.ToString();
                }
                else
                {
                    txtNLector.Clear();
                    MessageBox.Show("No se encontró el lector con ese ID.", "Advertencia",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error buscando lector: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null)
                {
                    cnn.CloseDB();
                }
            }
        }

        private void dgvPrestamo_SelectionChanged(object sender, EventArgs e)
        {
            // Si no hay ninguna fila seleccionada, limpiar todos los campos
            if (dgvPrestamo.SelectedRows.Count == 0)
            {
                LimpiarCampos();
                return;
            }

            var row = dgvPrestamo.SelectedRows[0];

            // Función auxiliar para obtener valores de las celdas de forma segura
            string GetString(string colName)
            {
                try
                {
                    return row.Cells[colName].Value?.ToString() ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }

            // Función auxiliar para obtener fechas de forma segura
            DateTime? GetDateTime(string colName)
            {
                try
                {
                    var value = row.Cells[colName].Value;
                    if (value != null && value != DBNull.Value && DateTime.TryParse(value.ToString(), out DateTime date))
                        return date;
                }
                catch { }
                return null;
            }

            // Rellenar los campos de texto con la información del préstamo
            txtIdPrestamo.Text = GetString("IdPrestamo");
            txtIdUsuario.Text = GetString("IdUsuario");
            txtIdLector.Text = GetString("IdLector");
            txtIdEjemplar.Text = GetString("IdEjemplar");
            txtNUsuario.Text = GetString("NombreUsuario");
            txtNLector.Text = GetString("NombreLector");
            txtNEjemplar.Text = GetString("TituloLibro");

            if (cbEstado != null)
            {
                var estado = GetString("EstadoPrestamo");
                // Buscar el índice del estado en el ComboBox
                int index = cbEstado.Items.IndexOf(estado);
                cbEstado.SelectedIndex = index >= 0 ? index : -1;
            }

            var fechaPrestamo = GetDateTime("FechaPrestamo");
            if (fechaPrestamo.HasValue && dtPrestamo != null)
                dtPrestamo.Value = fechaPrestamo.Value;

            var fechaDevolucion = GetDateTime("FechaDevolucionEstimada");
            if (fechaDevolucion.HasValue && dtDevolucion != null)
                dtDevolucion.Value = fechaDevolucion.Value;

            var fechaReal = GetDateTime("FechaDevolucionReal");
            if (fechaReal.HasValue && dtReal != null)
                dtReal.Value = fechaReal.Value;
            else if (dtReal != null)
                dtReal.Value = DateTime.Now;
        }
        private async Task LoadPrestamosAsync()
        {
            var dt = new DataTable();
            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();

                // Query para obtener información completa del prestamo
                cmd.CommandText = @"
                    SELECT 
                        p.IdPrestamo,
                        p.IdUsuario,
                        u.Nombre AS NombreUsuario,
                        p.IdLector,
                        l.Nombre + ' ' + l.Apellido AS NombreLector,
                        p.IdEjemplar,
                        li.Titulo AS TituloLibro,
                        p.FechaPrestamo,
                        p.FechaDevolucionEstimada,
                        p.FechaDevolucionReal,
                        p.EstadoPrestamo
                    FROM dbo.Prestamo p
                    INNER JOIN dbo.Lector l ON p.IdLector = l.IdLector
                    INNER JOIN dbo.Usuario u ON p.IdUsuario = u.IdUsuario
                    INNER JOIN dbo.Ejemplares e ON p.IdEjemplar = e.IdEjemplar
                    INNER JOIN dbo.Libros li ON e.IdLibro = li.IdLibros
                    ORDER BY p.IdPrestamo DESC";

                // Ejecutar la consulta de forma asíncrona
                using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);

                dgvPrestamo.DataSource = dt;

                ConfigurarColumnasDataGridView();
            }
            catch (Exception ex)
            {
                // Mostrar mensaje de error si algo falla
                MessageBox.Show($"Error cargando Préstamos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null)
                {
                    cnn.CloseDB();
                }
            }
        }
        private void ConfigurarColumnasDataGridView()
        {
            if (dgvPrestamo.Columns.Contains("IdPrestamo"))
            {
                dgvPrestamo.Columns["IdPrestamo"].HeaderText = "ID Préstamo";
            }
            if (dgvPrestamo.Columns.Contains("IdUsuario"))
            {
                dgvPrestamo.Columns["IdUsuario"].HeaderText = "ID Usuario";
            }
            if (dgvPrestamo.Columns.Contains("NombreUsuario"))
            {
                dgvPrestamo.Columns["NombreUsuario"].HeaderText = "Registrado por";
            }
            if (dgvPrestamo.Columns.Contains("IdLector"))
            {
                dgvPrestamo.Columns["IdLector"].HeaderText = "ID Lector";
            }
            if (dgvPrestamo.Columns.Contains("NombreLector"))
            {
                dgvPrestamo.Columns["NombreLector"].HeaderText = "Nombre del Lector";
            }
            if (dgvPrestamo.Columns.Contains("IdEjemplar"))
            {
                dgvPrestamo.Columns["IdEjemplar"].HeaderText = "ID Ejemplar";
            }
            if (dgvPrestamo.Columns.Contains("TituloLibro"))
            {
                dgvPrestamo.Columns["TituloLibro"].HeaderText = "Título del Libro";
            }
            if (dgvPrestamo.Columns.Contains("FechaPrestamo"))
            {
                dgvPrestamo.Columns["FechaPrestamo"].HeaderText = "Fecha Préstamo";
                dgvPrestamo.Columns["FechaPrestamo"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            if (dgvPrestamo.Columns.Contains("FechaDevolucionEstimada"))
            {
                dgvPrestamo.Columns["FechaDevolucionEstimada"].HeaderText = "Devolución Estimada";
                dgvPrestamo.Columns["FechaDevolucionEstimada"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            if (dgvPrestamo.Columns.Contains("FechaDevolucionReal"))
            {
                dgvPrestamo.Columns["FechaDevolucionReal"].HeaderText = "Devolución Real";
                dgvPrestamo.Columns["FechaDevolucionReal"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            if (dgvPrestamo.Columns.Contains("EstadoPrestamo"))
            {
                dgvPrestamo.Columns["EstadoPrestamo"].HeaderText = "Estado";
            }
        }

        private void LimpiarCampos()
        {
            txtIdPrestamo.Clear();
            txtIdLector.Clear();
            txtIdEjemplar.Clear();
            txtNLector.Clear();
            txtNEjemplar.Clear();

            if (cbEstado != null)
                cbEstado.SelectedIndex = -1;

            if (dtPrestamo != null)
                dtPrestamo.Value = DateTime.Now;

            if (dtDevolucion != null)
                dtDevolucion.Value = DateTime.Now;

            if (dtReal != null)
                dtReal.Value = DateTime.Now;

            // Mantener los datos del usuario logueado
            CargarDatosUsuarioLogueado();
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

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            // Validar que haya un préstamo seleccionado
            if (string.IsNullOrWhiteSpace(txtIdPrestamo.Text))
            {
                MessageBox.Show("Debe seleccionar un préstamo de la lista para modificar.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar que haya un estado seleccionado
            if (cbEstado == null || cbEstado.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un estado para el préstamo.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();

                // Actualizar el préstamo en la base de datos
                cmd.CommandText = @"
                    UPDATE dbo.Prestamo 
                    SET 
                        EstadoPrestamo = @estado,
                        FechaPrestamo = @fechaPrestamo,
                        FechaDevolucionEstimada = @fechaDevolucion,
                        FechaDevolucionReal = @fechaReal
                    WHERE IdPrestamo = @idPrestamo";

                cmd.Parameters.AddWithValue("@idPrestamo", Convert.ToInt32(txtIdPrestamo.Text));
                cmd.Parameters.AddWithValue("@estado", cbEstado.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@fechaPrestamo", dtPrestamo.Value);
                cmd.Parameters.AddWithValue("@fechaDevolucion", dtDevolucion.Value);
                cmd.Parameters.AddWithValue("@fechaReal", dtReal.Value);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Préstamo actualizado correctamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadPrestamosAsync();
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el préstamo.", "Advertencia",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null)
                {
                    cnn.CloseDB();
                }
            }
        }

        private void cbEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        private void groupBox2_Enter(object sender, EventArgs e)
        {
        }

        private void label9_Click(object sender, EventArgs e)
        {
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }
    }
}