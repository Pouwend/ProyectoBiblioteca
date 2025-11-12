using Biblioteca;
using BibliotecaDAE.Formularios;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient;
using SistemaControlPersonal.Core.Lib;
using SistemaControlPersonal.Core.Lib;
using System;
using System;
using System.Data;
using System.Data;
using System.Drawing;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms;

namespace BibliotecaDAE.Formularios
{
    public partial class frmPrestamos : Form
    {
        // Declaración de todos los controles
        private DataGridView dgvPrestamo;
        private TextBox txtIdPrestamo, txtIdUsuario, txtIdLector, txtIdEjemplar;
        private TextBox txtNUsuario, txtNLector, txtNEjemplar;
        private DateTimePicker dtPrestamo, dtDevolucion, dtReal;
        private ComboBox cbEstado;
        private Button btnNuevoPrestamo, btnLector, btnLibros, btnRenovar, btnGuardar, btnLimpiar;
        private Label lblInfoRenovacion, lblDiasRestantes, lblEstadoRenovacion;
        private GroupBox gbListadoPrestamos, gbRenovaciones;
        private TableLayoutPanel mainLayout;

        public frmPrestamos()
        {
            InitializeComponent();

            // Cargar los prestamos cuando se abre el formulario
            this.Load += async (_, __) => await LoadPrestamosAsync();

            // Configurar eventos
            if (dgvPrestamo != null)
            {
                dgvPrestamo.SelectionChanged += dgvPrestamo_SelectionChanged;
            }

            // Limpiar campos al iniciar el formulario
            LimpiarCampos();

            // Cargar datos del usuario logueado
            CargarDatosUsuarioLogueado();
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

            // Verificar si puede renovar
            VerificarRenovacion(GetString("IdPrestamo"), GetString("EstadoPrestamo"), fechaDevolucion);
        }

        private async void VerificarRenovacion(string idPrestamoStr, string estado, DateTime? fechaDevolucion)
        {
            if (string.IsNullOrEmpty(idPrestamoStr) || !int.TryParse(idPrestamoStr, out int idPrestamo))
            {
                lblInfoRenovacion.Text = "Información de Renovación";
                lblDiasRestantes.Text = "Días restantes: -";
                lblEstadoRenovacion.Text = "Estado: -";
                lblEstadoRenovacion.ForeColor = Color.Black;
                btnRenovar.Enabled = false;
                return;
            }

            Cnn cnn = null;
            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = @"
                    SELECT Renovado 
                    FROM dbo.Prestamo 
                    WHERE IdPrestamo = @idPrestamo";

                cmd.Parameters.AddWithValue("@idPrestamo", idPrestamo);
                var result = await cmd.ExecuteScalarAsync();

                bool yaRenovado = result != null && result != DBNull.Value && Convert.ToBoolean(result);

                // Calcular días restantes
                int diasRestantes = 0;
                if (fechaDevolucion.HasValue)
                {
                    diasRestantes = (fechaDevolucion.Value.Date - DateTime.Now.Date).Days;
                }

                lblDiasRestantes.Text = $"Días restantes: {diasRestantes}";

                if (diasRestantes < 0)
                {
                    lblDiasRestantes.ForeColor = Color.Red;
                }
                else if (diasRestantes <= 3)
                {
                    lblDiasRestantes.ForeColor = Color.Orange;
                }
                else
                {
                    lblDiasRestantes.ForeColor = Color.Green;
                }

                // Verificar si puede renovar
                bool puedeRenovar = estado == "Activo" && !yaRenovado && diasRestantes >= -7;

                if (yaRenovado)
                {
                    lblEstadoRenovacion.Text = "Ya fue renovado";
                    lblEstadoRenovacion.ForeColor = Color.Red;
                }
                else if (estado != "Activo")
                {
                    lblEstadoRenovacion.Text = $"Estado: {estado}";
                    lblEstadoRenovacion.ForeColor = Color.Gray;
                }
                else if (puedeRenovar)
                {
                    lblEstadoRenovacion.Text = "Puede renovar";
                    lblEstadoRenovacion.ForeColor = Color.Green;
                }
                else
                {
                    lblEstadoRenovacion.Text = "No puede renovar";
                    lblEstadoRenovacion.ForeColor = Color.Red;
                }

                btnRenovar.Enabled = puedeRenovar;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error verificando renovación: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null) cnn.CloseDB();
            }
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
                        p.EstadoPrestamo,
                        p.Renovado,
                        DATEDIFF(DAY, GETDATE(), p.FechaDevolucionEstimada) AS DiasRestantes
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
                dgvPrestamo.Columns["IdPrestamo"].HeaderText = "ID";
            if (dgvPrestamo.Columns.Contains("IdUsuario"))
                dgvPrestamo.Columns["IdUsuario"].Visible = false;
            if (dgvPrestamo.Columns.Contains("NombreUsuario"))
                dgvPrestamo.Columns["NombreUsuario"].HeaderText = "Registrado por";
            if (dgvPrestamo.Columns.Contains("IdLector"))
                dgvPrestamo.Columns["IdLector"].Visible = false;
            if (dgvPrestamo.Columns.Contains("NombreLector"))
                dgvPrestamo.Columns["NombreLector"].HeaderText = "Lector";
            if (dgvPrestamo.Columns.Contains("IdEjemplar"))
                dgvPrestamo.Columns["IdEjemplar"].Visible = false;
            if (dgvPrestamo.Columns.Contains("TituloLibro"))
                dgvPrestamo.Columns["TituloLibro"].HeaderText = "Libro";
            if (dgvPrestamo.Columns.Contains("FechaPrestamo"))
            {
                dgvPrestamo.Columns["FechaPrestamo"].HeaderText = "F. Préstamo";
                dgvPrestamo.Columns["FechaPrestamo"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            if (dgvPrestamo.Columns.Contains("FechaDevolucionEstimada"))
            {
                dgvPrestamo.Columns["FechaDevolucionEstimada"].HeaderText = "F. Devolución";
                dgvPrestamo.Columns["FechaDevolucionEstimada"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            if (dgvPrestamo.Columns.Contains("FechaDevolucionReal"))
                dgvPrestamo.Columns["FechaDevolucionReal"].Visible = false;
            if (dgvPrestamo.Columns.Contains("EstadoPrestamo"))
                dgvPrestamo.Columns["EstadoPrestamo"].HeaderText = "Estado";
            if (dgvPrestamo.Columns.Contains("Renovado"))
                dgvPrestamo.Columns["Renovado"].HeaderText = "Renovado";
            if (dgvPrestamo.Columns.Contains("DiasRestantes"))
                dgvPrestamo.Columns["DiasRestantes"].HeaderText = "Días Rest.";
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

            lblDiasRestantes.Text = "Días restantes: -";
            lblEstadoRenovacion.Text = "Estado: -";
            lblEstadoRenovacion.ForeColor = Color.Black;
            btnRenovar.Enabled = false;

            // Mantener los datos del usuario logueado
            CargarDatosUsuarioLogueado();
        }

        private async void btnPrestamo_Click_1(object sender, EventArgs e)
        {
            using var detalle = new frmPrestamoDetalle();
            detalle.ShowDialog(this);
            await LoadPrestamosAsync();
        }

        private async void btnLector_Click_1(object sender, EventArgs e)
        {
            using var lectorForm = new FrmLectores();
            lectorForm.ShowDialog(this);
            await LoadPrestamosAsync();
        }

        private async void btnLibros_Click(object sender, EventArgs e)
        {
            using var librosForm = new frmLibros();
            librosForm.ShowDialog(this);
            await LoadPrestamosAsync();
            await LoadPrestamosAsync();
        }

        private async void btnRenovar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdPrestamo.Text))
            {
                MessageBox.Show("Debe seleccionar un préstamo.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                "¿Está seguro de renovar este préstamo?\n\nSe extenderá por 7 días adicionales desde la fecha de devolución estimada.",
                "Confirmar Renovación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes) return;

            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE dbo.Prestamo 
                    SET 
                        FechaDevolucionEstimada = DATEADD(DAY, 7, FechaDevolucionEstimada),
                        Renovado = 1
                    WHERE IdPrestamo = @idPrestamo 
                      AND EstadoPrestamo = 'Activo' 
                      AND Renovado = 0";

                cmd.Parameters.AddWithValue("@idPrestamo", Convert.ToInt32(txtIdPrestamo.Text));

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Préstamo renovado exitosamente.\nNueva fecha de devolución: " +
                        dtDevolucion.Value.AddDays(7).ToString("dd/MM/yyyy"),
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await LoadPrestamosAsync();
                    LimpiarCampos();
                }
                else
                {
                    MessageBox.Show("No se pudo renovar el préstamo. Verifique que cumpla las condiciones.",
                        "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al renovar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null) cnn.CloseDB();
            }
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

            var estadoActual = cbEstado.SelectedItem.ToString();
            var idEjemplar = Convert.ToInt32(txtIdEjemplar.Text);

            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var transaction = cn.BeginTransaction();

                try
                {
                    // 1. Actualizar el préstamo
                    using (var cmdPrestamo = cn.CreateCommand())
                    {
                        cmdPrestamo.Transaction = transaction;
                        cmdPrestamo.CommandText = @"
                            UPDATE dbo.Prestamo 
                            SET 
                                EstadoPrestamo = @estado,
                                FechaDevolucionReal = @fechaReal
                            WHERE IdPrestamo = @idPrestamo";

                        cmdPrestamo.Parameters.AddWithValue("@idPrestamo", Convert.ToInt32(txtIdPrestamo.Text));
                        cmdPrestamo.Parameters.AddWithValue("@estado", estadoActual);
                        cmdPrestamo.Parameters.AddWithValue("@fechaReal", dtReal.Value);

                        await cmdPrestamo.ExecuteNonQueryAsync();
                    }

                    // 2. Actualizar estado del ejemplar según el estado del préstamo
                    string nuevoEstadoEjemplar = estadoActual switch
                    {
                        "Devuelto" => "Disponible",
                        "Activo" => "Prestado",
                        "Vencido" => "Prestado",
                        "Perdido" => "Perdido",
                        _ => "Prestado"
                    };

                    using (var cmdEjemplar = cn.CreateCommand())
                    {
                        cmdEjemplar.Transaction = transaction;
                        cmdEjemplar.CommandText = @"
                            UPDATE dbo.Ejemplares 
                            SET EstadoEjemplar = @estado 
                            WHERE IdEjemplar = @idEjemplar";

                        cmdEjemplar.Parameters.AddWithValue("@estado", nuevoEstadoEjemplar);
                        cmdEjemplar.Parameters.AddWithValue("@idEjemplar", idEjemplar);

                        await cmdEjemplar.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();

                    MessageBox.Show("Préstamo actualizado correctamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await LoadPrestamosAsync();
                    LimpiarCampos();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
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

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
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

        private void InitializeComponent()
        {
            this.Text = "Gestión de Préstamos y Renovaciones";
            this.ClientSize = new Size(1300, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9f);

            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40f));

            // ===== LISTADO DE PRÉSTAMOS =====
            gbListadoPrestamos = new GroupBox
            {
                Text = "Listado de Préstamos",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            var panelListado = new Panel { Dock = DockStyle.Fill };

            dgvPrestamo = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                MultiSelect = false,
                BackgroundColor = Color.White
            };

            var btnPanelTop = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 5, 0, 5)
            };

            btnNuevoPrestamo = new Button { Text = "Nuevo Préstamo", Width = 130, Height = 28 };
            btnNuevoPrestamo.Click += btnPrestamo_Click_1;

            btnLector = new Button { Text = "Gestionar Lectores", Width = 140, Height = 28 };
            btnLector.Click += btnLector_Click_1;

            btnLibros = new Button { Text = "Gestionar Libros", Width = 130, Height = 28 };
            btnLibros.Click += btnLibros_Click;

            btnPanelTop.Controls.AddRange(new Control[] { btnNuevoPrestamo, btnLector, btnLibros });

            panelListado.Controls.Add(dgvPrestamo);
            panelListado.Controls.Add(btnPanelTop);
            gbListadoPrestamos.Controls.Add(panelListado);

            // ===== RENOVACIONES =====
            gbRenovaciones = new GroupBox
            {
                Text = "Renovaciones y Actualización de Estado",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 6,
                Padding = new Padding(5)
            };

            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66.67f));

            for (int i = 0; i < 6; i++)
                formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

            // Row 0: ID Préstamo / Lector
            formLayout.Controls.Add(new Label { Text = "ID Préstamo:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 0);
            txtIdPrestamo = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true, BackColor = SystemColors.Control };
            formLayout.Controls.Add(txtIdPrestamo, 1, 0);

            formLayout.Controls.Add(new Label { Text = "Lector:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 0);
            txtNLector = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true, BackColor = SystemColors.Control };
            formLayout.Controls.Add(txtNLector, 3, 0);

            // Row 1: Usuario / Ejemplar
            formLayout.Controls.Add(new Label { Text = "Usuario:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 1);
            txtNUsuario = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true, BackColor = SystemColors.Control };
            formLayout.Controls.Add(txtNUsuario, 1, 1);

            formLayout.Controls.Add(new Label { Text = "Libro:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 1);
            txtNEjemplar = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true, BackColor = SystemColors.Control };
            formLayout.Controls.Add(txtNEjemplar, 3, 1);

            // Row 2: Fechas
            formLayout.Controls.Add(new Label { Text = "F. Préstamo:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 2);
            dtPrestamo = new DateTimePicker { Anchor = AnchorStyles.Left | AnchorStyles.Right, Format = DateTimePickerFormat.Short, Enabled = false };
            formLayout.Controls.Add(dtPrestamo, 1, 2);

            formLayout.Controls.Add(new Label { Text = "F. Devolución Est.:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 2);
            dtDevolucion = new DateTimePicker { Anchor = AnchorStyles.Left | AnchorStyles.Right, Format = DateTimePickerFormat.Short, Enabled = false };
            formLayout.Controls.Add(dtDevolucion, 3, 2);

            // Row 3: Estado / Fecha Real
            formLayout.Controls.Add(new Label { Text = "Estado:*", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 3);
            cbEstado = new ComboBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, DropDownStyle = ComboBoxStyle.DropDownList };
            cbEstado.Items.AddRange(new object[] { "Activo", "Devuelto", "Vencido", "Perdido" });
            cbEstado.SelectedIndexChanged += cbEstado_SelectedIndexChanged;
            formLayout.Controls.Add(cbEstado, 1, 3);

            formLayout.Controls.Add(new Label { Text = "F. Devolución Real:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 3);
            dtReal = new DateTimePicker { Anchor = AnchorStyles.Left | AnchorStyles.Right, Format = DateTimePickerFormat.Short };
            formLayout.Controls.Add(dtReal, 3, 3);

            // Row 4: Info de Renovación
            formLayout.Controls.Add(new Label { Text = "Renovación:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 4);

            var panelInfoRenov = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0)
            };

            lblDiasRestantes = new Label { Text = "Días restantes: -", AutoSize = true, Margin = new Padding(0, 5, 15, 0), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblEstadoRenovacion = new Label { Text = "Estado: -", AutoSize = true, Margin = new Padding(0, 5, 0, 0) };

            panelInfoRenov.Controls.AddRange(new Control[] { lblDiasRestantes, lblEstadoRenovacion });
            formLayout.Controls.Add(panelInfoRenov, 1, 4);
            formLayout.SetColumnSpan(panelInfoRenov, 3);

            // Row 5: Botones
            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 5, 0, 0)
            };

            btnRenovar = new Button { Text = "Renovar Préstamo", Width = 140, Height = 28, Enabled = false };
            btnRenovar.Click += btnRenovar_Click;

            btnGuardar = new Button { Text = "Actualizar Estado", Width = 140, Height = 28 };
            btnGuardar.Click += btnGuardar_Click;

            btnLimpiar = new Button { Text = "Limpiar", Width = 100, Height = 28 };
            btnLimpiar.Click += btnLimpiar_Click;

            btnPanel.Controls.AddRange(new Control[] { btnLimpiar, btnGuardar, btnRenovar });
            formLayout.Controls.Add(btnPanel, 0, 5);
            formLayout.SetColumnSpan(btnPanel, 4);

            // IDs ocultos
            txtIdUsuario = new TextBox { Visible = false };
            txtIdLector = new TextBox { Visible = false };
            txtIdEjemplar = new TextBox { Visible = false };
            lblInfoRenovacion = new Label { Visible = false };

            gbRenovaciones.Controls.Add(formLayout);

            mainLayout.Controls.Add(gbListadoPrestamos, 0, 0);
            mainLayout.Controls.Add(gbRenovaciones, 0, 1);

            this.Controls.Add(mainLayout);
        }
    }
}
namespace BibliotecaDAE
{
    public class frmPrestamos : Form
    {
        private DataGridView dgvEjemplares;
        private TextBox txtNombre, txtApellido, txtDireccion, txtTelefono, txtEmail, txtEdad, txtCarnet, txtDUI, txtIdEjemplar;
        private ComboBox cbxTipo;
        private Button btnGuardar, btnLimpiar, btnCerrar;
        private TableLayoutPanel mainLayout;
        private GroupBox gbLector, gbEjemplares;

        public frmPrestamos()
        {
            InitializeComponent();
            RegisterEvents();
            // Limpiar campos al iniciar el formulario
            LimpiarCampos();
        }

        private void RegisterEvents()
        {
            this.Load += async (_, __) => await LoadEjemplaresAsync();
            dgvEjemplares.SelectionChanged += DgvEjemplares_SelectionChanged;
            btnGuardar.Click += async (_, __) => await GuardarLectorAsync();
            btnLimpiar.Click += (_, __) => LimpiarCampos();
            btnCerrar.Click += (_, __) => this.Close();
        }

        private async Task LoadEjemplaresAsync()
        {
            var dt = new DataTable();
            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();

                // Query con JOINs para mostrar información completa del libro
                cmd.CommandText = @"
                    SELECT 
                        e.IdEjemplar,
                        e.IdLibro,
                        e.CodigoEjemplar,
                        e.EstadoEjemplar,
                        e.FechaAdquisicion,
                        lib.Titulo,
                        STRING_AGG(g.Nombre, ', ') AS Genero
                    FROM dbo.Ejemplares e
                    INNER JOIN dbo.Libros lib ON e.IdLibro = lib.IdLibros
                    LEFT JOIN dbo.GeneroLibro gl ON lib.IdLibros = gl.IdLibro
                    LEFT JOIN dbo.Genero g ON gl.IdGenero = g.IdGenero
                    WHERE e.EstadoEjemplar = 'Disponible'
                    GROUP BY e.IdEjemplar, e.IdLibro, e.CodigoEjemplar, e.EstadoEjemplar, e.FechaAdquisicion, lib.Titulo
                    ORDER BY e.IdEjemplar ASC";

                using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);

                dgvEjemplares.DataSource = dt;

                // Ajustar el orden y apariencia de las columnas
                if (dgvEjemplares.Columns.Contains("IdEjemplar"))
                {
                    dgvEjemplares.Columns["IdEjemplar"].HeaderText = "ID Ejemplar";
                    dgvEjemplares.Columns["IdEjemplar"].Width = 90;
                }
                if (dgvEjemplares.Columns.Contains("IdLibro"))
                {
                    dgvEjemplares.Columns["IdLibro"].HeaderText = "ID Libro";
                    dgvEjemplares.Columns["IdLibro"].Width = 80;
                }
                if (dgvEjemplares.Columns.Contains("CodigoEjemplar"))
                {
                    dgvEjemplares.Columns["CodigoEjemplar"].HeaderText = "Código";
                    dgvEjemplares.Columns["CodigoEjemplar"].Width = 100;
                }
                if (dgvEjemplares.Columns.Contains("EstadoEjemplar"))
                {
                    dgvEjemplares.Columns["EstadoEjemplar"].HeaderText = "Estado";
                    dgvEjemplares.Columns["EstadoEjemplar"].Width = 100;
                }
                if (dgvEjemplares.Columns.Contains("FechaAdquisicion"))
                {
                    dgvEjemplares.Columns["FechaAdquisicion"].HeaderText = "Fecha Adquisición";
                    dgvEjemplares.Columns["FechaAdquisicion"].Width = 130;
                }
                if (dgvEjemplares.Columns.Contains("Titulo"))
                {
                    dgvEjemplares.Columns["Titulo"].HeaderText = "Título del Libro";
                    dgvEjemplares.Columns["Titulo"].Width = 250;
                }
                if (dgvEjemplares.Columns.Contains("Genero"))
                {
                    dgvEjemplares.Columns["Genero"].HeaderText = "Género(s)";
                    dgvEjemplares.Columns["Genero"].Width = 150;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando Ejemplares: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null)
                {
                    cnn.CloseDB();
                }
            }
        }

        private void DgvEjemplares_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvEjemplares.SelectedRows.Count == 0) return;
            var row = dgvEjemplares.SelectedRows[0];

            // Obtener el IdEjemplar de la fila seleccionada
            if (row.Cells["IdEjemplar"].Value != null && row.Cells["IdEjemplar"].Value != DBNull.Value)
            {
                txtIdEjemplar.Text = row.Cells["IdEjemplar"].Value.ToString();
            }
            else
            {
                txtIdEjemplar.Text = string.Empty;
            }
        }

        private async Task GuardarLectorAsync()
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("Nombre y Apellido son obligatorios.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validación: TipoUsuario es obligatorio
            if (cbxTipo.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un Tipo de Usuario.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbxTipo.Focus();
                return;
            }

            // Validación: Al menos Carnet o DUI debe estar presente
            var carnet = string.IsNullOrWhiteSpace(txtCarnet.Text) ? null : txtCarnet.Text.Trim();
            var dui = string.IsNullOrWhiteSpace(txtDUI.Text) ? null : txtDUI.Text.Trim();

            if (carnet == null && dui == null)
            {
                MessageBox.Show("Debe ingresar al menos el Carnet o el DUI del lector.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var nombre = txtNombre.Text.Trim();
            var apellido = txtApellido.Text.Trim();
            var direccion = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text.Trim();
            var telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim();
            var email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
            int? edad = null;
            if (int.TryParse(txtEdad.Text, out var eVal)) edad = eVal;
            var tipo = cbxTipo.SelectedItem?.ToString();

            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                // Verificar si ya existe un lector con ese DUI o Carnet
                int? existingId = null;
                if (!string.IsNullOrWhiteSpace(dui))
                {
                    using var cmdCheck = cn.CreateCommand();
                    cmdCheck.CommandText = "SELECT TOP 1 IdLector FROM dbo.Lector WHERE DUI = @dui";
                    cmdCheck.Parameters.AddWithValue("@dui", dui);
                    var obj = await cmdCheck.ExecuteScalarAsync();
                    if (obj != null && obj != DBNull.Value) existingId = Convert.ToInt32(obj);
                }

                if (!existingId.HasValue && !string.IsNullOrWhiteSpace(carnet))
                {
                    using var cmdCheck = cn.CreateCommand();
                    cmdCheck.CommandText = "SELECT TOP 1 IdLector FROM dbo.Lector WHERE Carnet = @carnet";
                    cmdCheck.Parameters.AddWithValue("@carnet", carnet);
                    var obj = await cmdCheck.ExecuteScalarAsync();
                    if (obj != null && obj != DBNull.Value) existingId = Convert.ToInt32(obj);
                }

                if (existingId.HasValue)
                {
                    // Actualizar lector existente
                    using var cmdUpd = cn.CreateCommand();
                    cmdUpd.CommandText = @"
                        UPDATE dbo.Lector SET
                            Nombre = @nombre,
                            Apellido = @apellido,
                            Direccion = @direccion,
                            Telefono = @telefono,
                            Email = @email,
                            Edad = @edad,
                            Carnet = @carnet,
                            DUI = @dui,
                            TipoUsuario = @tipo
                        WHERE IdLector = @id";

                    cmdUpd.Parameters.Add(new SqlParameter("@nombre", SqlDbType.NVarChar, 50) { Value = nombre });
                    cmdUpd.Parameters.Add(new SqlParameter("@apellido", SqlDbType.NVarChar, 50) { Value = apellido });
                    cmdUpd.Parameters.Add(new SqlParameter("@direccion", SqlDbType.NVarChar, 200) { Value = (object?)direccion ?? DBNull.Value });
                    cmdUpd.Parameters.Add(new SqlParameter("@telefono", SqlDbType.NVarChar, 20) { Value = (object?)telefono ?? DBNull.Value });
                    cmdUpd.Parameters.Add(new SqlParameter("@email", SqlDbType.NVarChar, 100) { Value = (object?)email ?? DBNull.Value });
                    cmdUpd.Parameters.Add(new SqlParameter("@edad", SqlDbType.Int) { Value = (object?)edad ?? DBNull.Value });
                    cmdUpd.Parameters.Add(new SqlParameter("@carnet", SqlDbType.NVarChar, 20) { Value = (object?)carnet ?? DBNull.Value });
                    cmdUpd.Parameters.Add(new SqlParameter("@dui", SqlDbType.NVarChar, 10) { Value = (object?)dui ?? DBNull.Value });
                    cmdUpd.Parameters.Add(new SqlParameter("@tipo", SqlDbType.NVarChar, 20) { Value = tipo });
                    cmdUpd.Parameters.AddWithValue("@id", existingId.Value);

                    var rows = await cmdUpd.ExecuteNonQueryAsync();
                    MessageBox.Show(rows > 0 ? "Lector actualizado correctamente." : "No se actualizó el lector.", "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Insertar nuevo lector
                    using var cmdIns = cn.CreateCommand();
                    cmdIns.CommandText = @"
                        INSERT INTO dbo.Lector
                            (Nombre, Apellido, Direccion, Telefono, Email, Edad, Carnet, DUI, TipoUsuario)
                        VALUES
                            (@nombre, @apellido, @direccion, @telefono, @email, @edad, @carnet, @dui, @tipo)";

                    cmdIns.Parameters.Add(new SqlParameter("@nombre", SqlDbType.NVarChar, 50) { Value = nombre });
                    cmdIns.Parameters.Add(new SqlParameter("@apellido", SqlDbType.NVarChar, 50) { Value = apellido });
                    cmdIns.Parameters.Add(new SqlParameter("@direccion", SqlDbType.NVarChar, 200) { Value = (object?)direccion ?? DBNull.Value });
                    cmdIns.Parameters.Add(new SqlParameter("@telefono", SqlDbType.NVarChar, 20) { Value = (object?)telefono ?? DBNull.Value });
                    cmdIns.Parameters.Add(new SqlParameter("@email", SqlDbType.NVarChar, 100) { Value = (object?)email ?? DBNull.Value });
                    cmdIns.Parameters.Add(new SqlParameter("@edad", SqlDbType.Int) { Value = (object?)edad ?? DBNull.Value });
                    cmdIns.Parameters.Add(new SqlParameter("@carnet", SqlDbType.NVarChar, 20) { Value = (object?)carnet ?? DBNull.Value });
                    cmdIns.Parameters.Add(new SqlParameter("@dui", SqlDbType.NVarChar, 10) { Value = (object?)dui ?? DBNull.Value });
                    cmdIns.Parameters.Add(new SqlParameter("@tipo", SqlDbType.NVarChar, 20) { Value = tipo });

                    var rows = await cmdIns.ExecuteNonQueryAsync();
                    MessageBox.Show(rows > 0 ? "Lector creado correctamente." : "No se guardó el lector.", "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LimpiarCampos();
            }
            catch (SqlException ex)
            {
                // Manejo específico de errores de SQL
                if (ex.Number == 2601 || ex.Number == 2627) // Violación de índice único
                {
                    MessageBox.Show("Ya existe un lector registrado con ese Carnet o DUI.", "Error de duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (ex.Number == 547) // Violación de CHECK constraint
                {
                    MessageBox.Show("Los datos no cumplen con las restricciones de la base de datos. Verifique que al menos ingresó Carnet o DUI.", "Error de validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Error SQL: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null)
                {
                    cnn.CloseDB();
                }
            }
        }

        private void LimpiarCampos()
        {
            txtNombre.Clear();
            txtApellido.Clear();
            txtDireccion.Clear();
            txtTelefono.Clear();
            txtEmail.Clear();
            txtEdad.Clear();
            txtCarnet.Clear();
            txtDUI.Clear();
            txtIdEjemplar.Clear();
            cbxTipo.SelectedIndex = -1;
            txtNombre.Focus();
        }

        private void InitializeComponent()
        {
            this.Text = "Detalle de Préstamo / Lector";
            this.ClientSize = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9f);

            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10),
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            // GroupBox para Ejemplares
            gbEjemplares = new GroupBox
            {
                Text = "Ejemplares Disponibles",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            dgvEjemplares = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                MultiSelect = false,
                BackgroundColor = Color.White
            };

            gbEjemplares.Controls.Add(dgvEjemplares);

            // GroupBox para Lector
            gbLector = new GroupBox
            {
                Text = "Información del Lector",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 5,
                AutoSize = true
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            // Row 0
            formLayout.Controls.Add(new Label { Text = "Nombre:*", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 0);
            txtNombre = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 50 };
            formLayout.Controls.Add(txtNombre, 1, 0);

            formLayout.Controls.Add(new Label { Text = "Apellido:*", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 0);
            txtApellido = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 50 };
            formLayout.Controls.Add(txtApellido, 3, 0);

            // Row 1
            formLayout.Controls.Add(new Label { Text = "Dirección:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 1);
            txtDireccion = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 200 };
            formLayout.Controls.Add(txtDireccion, 1, 1);

            formLayout.Controls.Add(new Label { Text = "Teléfono:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 1);
            txtTelefono = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 20 };
            formLayout.Controls.Add(txtTelefono, 3, 1);

            // Row 2
            formLayout.Controls.Add(new Label { Text = "Email:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 2);
            txtEmail = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 100 };
            formLayout.Controls.Add(txtEmail, 1, 2);

            formLayout.Controls.Add(new Label { Text = "Edad:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 2);
            txtEdad = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 3 };
            formLayout.Controls.Add(txtEdad, 3, 2);

            // Row 3
            formLayout.Controls.Add(new Label { Text = "N° Carnet:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 3);
            txtCarnet = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 20 };
            formLayout.Controls.Add(txtCarnet, 1, 3);

            formLayout.Controls.Add(new Label { Text = "DUI:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 3);
            txtDUI = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 10 };
            formLayout.Controls.Add(txtDUI, 3, 3);

            // Row 4
            formLayout.Controls.Add(new Label { Text = "Id Ejemplar:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 4);
            txtIdEjemplar = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true, BackColor = SystemColors.Control };
            formLayout.Controls.Add(txtIdEjemplar, 1, 4);

            formLayout.Controls.Add(new Label { Text = "Tipo:*", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 4);
            cbxTipo = new ComboBox { Anchor = AnchorStyles.Left, DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            cbxTipo.Items.AddRange(new object[] { "Docente", "Estudiante" });
            formLayout.Controls.Add(cbxTipo, 3, 4);

            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(0, 6, 0, 0)
            };

            btnGuardar = new Button { Text = "Guardar Lector", Width = 120, Height = 28 };
            btnLimpiar = new Button { Text = "Limpiar", Width = 100, Height = 28 };
            btnCerrar = new Button { Text = "Cerrar", Width = 100, Height = 28 };
            btnPanel.Controls.AddRange(new Control[] { btnCerrar, btnLimpiar, btnGuardar });

            gbLector.Controls.Add(formLayout);
            gbLector.Controls.Add(btnPanel);

            mainLayout.Controls.Add(gbEjemplares, 0, 0);
            mainLayout.Controls.Add(gbLector, 0, 1);

            this.Controls.Add(mainLayout);
        }
    }
}