using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using SistemaControlPersonal.Core.Lib;

namespace BibliotecaDAE
{
    public class PrestamoDetalle : Form
    {
        private DataGridView dgvEjemplares, dgvLectores;
        private TextBox txtIdLector, txtNombreLector, txtIdEjemplar, txtTituloLibro;
        private DateTimePicker dtpFechaPrestamo, dtpFechaDevolucion;
        private Button btnRegistrarPrestamo, btnLimpiar, btnCerrar;
        private TableLayoutPanel mainLayout;
        private GroupBox gbEjemplares, gbLectores, gbDetallePrestamo;
        private Label lblPrestamosActivos, lblLimitePrestamos, lblEstadoLector;

        public PrestamoDetalle()
        {
            InitializeComponent();
            RegisterEvents();
            LimpiarCampos();
        }

        private void RegisterEvents()
        {
            this.Load += async (_, __) =>
            {
                await LoadEjemplaresAsync();
                await LoadLectoresAsync();
            };

            dgvEjemplares.SelectionChanged += DgvEjemplares_SelectionChanged;
            dgvLectores.SelectionChanged += DgvLectores_SelectionChanged;
            btnRegistrarPrestamo.Click += async (_, __) => await RegistrarPrestamoAsync();
            btnLimpiar.Click += (_, __) => LimpiarCampos();
            btnCerrar.Click += (_, __) => this.Close();
            dtpFechaPrestamo.ValueChanged += (_, __) => ActualizarFechaDevolucion();
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
                cmd.CommandText = @"
                    SELECT 
                        e.IdEjemplar,
                        e.CodigoEjemplar,
                        lib.Titulo,
                        lib.Editorial,
                        lib.AnioPublicacion,
                        e.EstadoEjemplar,
                        STRING_AGG(g.Nombre, ', ') AS Generos
                    FROM dbo.Ejemplares e
                    INNER JOIN dbo.Libros lib ON e.IdLibro = lib.IdLibros
                    LEFT JOIN dbo.GeneroLibro gl ON lib.IdLibros = gl.IdLibro
                    LEFT JOIN dbo.Genero g ON gl.IdGenero = g.IdGenero
                    WHERE e.EstadoEjemplar = 'Disponible' AND lib.Estado = 1
                    GROUP BY e.IdEjemplar, e.CodigoEjemplar, lib.Titulo, lib.Editorial, 
                             lib.AnioPublicacion, e.EstadoEjemplar
                    ORDER BY lib.Titulo ASC";

                using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);

                dgvEjemplares.DataSource = dt;
                ConfigurarColumnasEjemplares();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando ejemplares: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null) cnn.CloseDB();
            }
        }

        private async Task LoadLectoresAsync()
        {
            var dt = new DataTable();
            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        l.IdLector,
                        l.Nombre + ' ' + l.Apellido AS NombreCompleto,
                        l.Carnet,
                        l.DUI,
                        l.TipoUsuario,
                        l.Estado,
                        l.LimitePrestamos,
                        (SELECT COUNT(*) 
                         FROM dbo.Prestamo p 
                         WHERE p.IdLector = l.IdLector 
                           AND p.EstadoPrestamo IN ('Activo', 'Vencido')) AS PrestamosActivos
                    FROM dbo.Lector l
                    WHERE l.Estado = 'Activo'
                    ORDER BY l.Apellido, l.Nombre";

                using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);

                dgvLectores.DataSource = dt;
                ConfigurarColumnasLectores();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando lectores: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null) cnn.CloseDB();
            }
        }

        private void ConfigurarColumnasEjemplares()
        {
            if (dgvEjemplares.Columns.Contains("IdEjemplar"))
                dgvEjemplares.Columns["IdEjemplar"].HeaderText = "ID";
            if (dgvEjemplares.Columns.Contains("CodigoEjemplar"))
                dgvEjemplares.Columns["CodigoEjemplar"].HeaderText = "Código";
            if (dgvEjemplares.Columns.Contains("Titulo"))
                dgvEjemplares.Columns["Titulo"].HeaderText = "Título";
            if (dgvEjemplares.Columns.Contains("Editorial"))
                dgvEjemplares.Columns["Editorial"].HeaderText = "Editorial";
            if (dgvEjemplares.Columns.Contains("AnioPublicacion"))
                dgvEjemplares.Columns["AnioPublicacion"].HeaderText = "Año";
            if (dgvEjemplares.Columns.Contains("EstadoEjemplar"))
                dgvEjemplares.Columns["EstadoEjemplar"].HeaderText = "Estado";
            if (dgvEjemplares.Columns.Contains("Generos"))
                dgvEjemplares.Columns["Generos"].HeaderText = "Género(s)";
        }

        private void ConfigurarColumnasLectores()
        {
            if (dgvLectores.Columns.Contains("IdLector"))
                dgvLectores.Columns["IdLector"].HeaderText = "ID";
            if (dgvLectores.Columns.Contains("NombreCompleto"))
                dgvLectores.Columns["NombreCompleto"].HeaderText = "Nombre Completo";
            if (dgvLectores.Columns.Contains("Carnet"))
                dgvLectores.Columns["Carnet"].HeaderText = "Carnet";
            if (dgvLectores.Columns.Contains("DUI"))
                dgvLectores.Columns["DUI"].HeaderText = "DUI";
            if (dgvLectores.Columns.Contains("TipoUsuario"))
                dgvLectores.Columns["TipoUsuario"].HeaderText = "Tipo";
            if (dgvLectores.Columns.Contains("Estado"))
                dgvLectores.Columns["Estado"].HeaderText = "Estado";
            if (dgvLectores.Columns.Contains("LimitePrestamos"))
                dgvLectores.Columns["LimitePrestamos"].HeaderText = "Límite";
            if (dgvLectores.Columns.Contains("PrestamosActivos"))
                dgvLectores.Columns["PrestamosActivos"].HeaderText = "Activos";
        }

        private void DgvEjemplares_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvEjemplares.SelectedRows.Count == 0) return;
            var row = dgvEjemplares.SelectedRows[0];

            txtIdEjemplar.Text = row.Cells["IdEjemplar"].Value?.ToString() ?? "";
            txtTituloLibro.Text = row.Cells["Titulo"].Value?.ToString() ?? "";
        }

        private void DgvLectores_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvLectores.SelectedRows.Count == 0) return;
            var row = dgvLectores.SelectedRows[0];

            txtIdLector.Text = row.Cells["IdLector"].Value?.ToString() ?? "";
            txtNombreLector.Text = row.Cells["NombreCompleto"].Value?.ToString() ?? "";

            // Actualizar información del lector
            var prestamosActivos = row.Cells["PrestamosActivos"].Value?.ToString() ?? "0";
            var limitePrestamos = row.Cells["LimitePrestamos"].Value?.ToString() ?? "0";
            var estado = row.Cells["Estado"].Value?.ToString() ?? "";

            lblPrestamosActivos.Text = $"Préstamos Activos: {prestamosActivos}";
            lblLimitePrestamos.Text = $"Límite: {limitePrestamos}";
            lblEstadoLector.Text = $"Estado: {estado}";

            // Cambiar color según estado
            lblEstadoLector.ForeColor = estado == "Activo" ? Color.Green : Color.Red;
        }

        private void ActualizarFechaDevolucion()
        {
            // Por defecto, 7 días después de la fecha de préstamo
            dtpFechaDevolucion.Value = dtpFechaPrestamo.Value.AddDays(7);
        }

        private async Task RegistrarPrestamoAsync()
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtIdLector.Text))
            {
                MessageBox.Show("Debe seleccionar un lector.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtIdEjemplar.Text))
            {
                MessageBox.Show("Debe seleccionar un ejemplar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtIdLector.Text, out int idLector) ||
                !int.TryParse(txtIdEjemplar.Text, out int idEjemplar))
            {
                MessageBox.Show("IDs inválidos.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                // 1. Verificar estado del lector
                using (var cmdLector = cn.CreateCommand())
                {
                    cmdLector.CommandText = @"
                        SELECT Estado, LimitePrestamos,
                            (SELECT COUNT(*) FROM dbo.Prestamo 
                             WHERE IdLector = @idLector 
                               AND EstadoPrestamo IN ('Activo', 'Vencido')) AS PrestamosActivos
                        FROM dbo.Lector 
                        WHERE IdLector = @idLector";

                    cmdLector.Parameters.AddWithValue("@idLector", idLector);

                    using var reader = await cmdLector.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        var estado = reader["Estado"].ToString();
                        var limite = Convert.ToInt32(reader["LimitePrestamos"]);
                        var activos = Convert.ToInt32(reader["PrestamosActivos"]);

                        if (estado != "Activo")
                        {
                            MessageBox.Show("El lector está inactivo. No se puede realizar el préstamo.",
                                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (activos >= limite)
                        {
                            MessageBox.Show($"El lector ha alcanzado su límite de {limite} préstamos activos.",
                                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Lector no encontrado.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // 2. Verificar estado del ejemplar
                using (var cmdEjemplar = cn.CreateCommand())
                {
                    cmdEjemplar.CommandText = @"
                        SELECT EstadoEjemplar 
                        FROM dbo.Ejemplares 
                        WHERE IdEjemplar = @idEjemplar";

                    cmdEjemplar.Parameters.AddWithValue("@idEjemplar", idEjemplar);

                    var estadoEjemplar = await cmdEjemplar.ExecuteScalarAsync();

                    if (estadoEjemplar == null || estadoEjemplar.ToString() != "Disponible")
                    {
                        MessageBox.Show("El ejemplar no está disponible.", "Validación",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        await LoadEjemplaresAsync(); // Refrescar la lista
                        return;
                    }
                }

                // 3. Registrar el préstamo
                using (var cmdPrestamo = cn.CreateCommand())
                {
                    cmdPrestamo.CommandText = @"
                        INSERT INTO dbo.Prestamo 
                            (IdUsuario, IdLector, IdEjemplar, FechaPrestamo, 
                             FechaDevolucionEstimada, EstadoPrestamo)
                        VALUES 
                            (@idUsuario, @idLector, @idEjemplar, @fechaPrestamo, 
                             @fechaDevolucion, 'Activo')";

                    cmdPrestamo.Parameters.AddWithValue("@idUsuario", SesionUsuario.IdUsuario);
                    cmdPrestamo.Parameters.AddWithValue("@idLector", idLector);
                    cmdPrestamo.Parameters.AddWithValue("@idEjemplar", idEjemplar);
                    cmdPrestamo.Parameters.AddWithValue("@fechaPrestamo", dtpFechaPrestamo.Value);
                    cmdPrestamo.Parameters.AddWithValue("@fechaDevolucion", dtpFechaDevolucion.Value);

                    await cmdPrestamo.ExecuteNonQueryAsync();
                }

                // 4. Actualizar estado del ejemplar a "Prestado"
                using (var cmdUpdate = cn.CreateCommand())
                {
                    cmdUpdate.CommandText = @"
                        UPDATE dbo.Ejemplares 
                        SET EstadoEjemplar = 'Prestado' 
                        WHERE IdEjemplar = @idEjemplar";

                    cmdUpdate.Parameters.AddWithValue("@idEjemplar", idEjemplar);
                    await cmdUpdate.ExecuteNonQueryAsync();
                }

                MessageBox.Show("Préstamo registrado exitosamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refrescar listas
                await LoadEjemplaresAsync();
                await LoadLectoresAsync();
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar préstamo: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null) cnn.CloseDB();
            }
        }

        private void LimpiarCampos()
        {
            txtIdLector.Clear();
            txtNombreLector.Clear();
            txtIdEjemplar.Clear();
            txtTituloLibro.Clear();

            dtpFechaPrestamo.Value = DateTime.Now;
            dtpFechaDevolucion.Value = DateTime.Now.AddDays(7);

            lblPrestamosActivos.Text = "Préstamos Activos: -";
            lblLimitePrestamos.Text = "Límite: -";
            lblEstadoLector.Text = "Estado: -";
            lblEstadoLector.ForeColor = Color.Black;
        }

        private void InitializeComponent()
        {
            this.Text = "Registrar Nuevo Préstamo";
            this.ClientSize = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9f);

            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10),
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40f));

            // Panel de Ejemplares (arriba izquierda)
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

            // Panel de Lectores (arriba derecha)
            gbLectores = new GroupBox
            {
                Text = "Lectores Activos",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            dgvLectores = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                MultiSelect = false,
                BackgroundColor = Color.White
            };

            gbLectores.Controls.Add(dgvLectores);

            // Panel de Detalle del Préstamo (abajo, span 2 columnas)
            gbDetallePrestamo = new GroupBox
            {
                Text = "Detalle del Préstamo",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 5,
                Padding = new Padding(5)
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            for (int i = 0; i < 5; i++)
                formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

            // Row 0: ID Lector / Nombre Lector
            formLayout.Controls.Add(new Label { Text = "ID Lector:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 0);
            txtIdLector = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true, BackColor = SystemColors.Control };
            formLayout.Controls.Add(txtIdLector, 1, 0);

            formLayout.Controls.Add(new Label { Text = "Nombre:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 0);
            txtNombreLector = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true, BackColor = SystemColors.Control };
            formLayout.Controls.Add(txtNombreLector, 3, 0);

            // Row 1: ID Ejemplar / Título
            formLayout.Controls.Add(new Label { Text = "ID Ejemplar:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 1);
            txtIdEjemplar = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true, BackColor = SystemColors.Control };
            formLayout.Controls.Add(txtIdEjemplar, 1, 1);

            formLayout.Controls.Add(new Label { Text = "Título:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 1);
            txtTituloLibro = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true, BackColor = SystemColors.Control };
            formLayout.Controls.Add(txtTituloLibro, 3, 1);

            // Row 2: Fechas
            formLayout.Controls.Add(new Label { Text = "Fecha Préstamo:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 2);
            dtpFechaPrestamo = new DateTimePicker { Anchor = AnchorStyles.Left | AnchorStyles.Right, Format = DateTimePickerFormat.Short };
            formLayout.Controls.Add(dtpFechaPrestamo, 1, 2);

            formLayout.Controls.Add(new Label { Text = "Fecha Devolución:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 2);
            dtpFechaDevolucion = new DateTimePicker { Anchor = AnchorStyles.Left | AnchorStyles.Right, Format = DateTimePickerFormat.Short };
            formLayout.Controls.Add(dtpFechaDevolucion, 3, 2);

            // Row 3: Estado del Lector
            formLayout.Controls.Add(new Label { Text = "Info Lector:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 3);

            var panelInfoLector = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            lblEstadoLector = new Label { Text = "Estado: -", AutoSize = true, Margin = new Padding(0, 5, 10, 0) };
            lblPrestamosActivos = new Label { Text = "Préstamos Activos: -", AutoSize = true, Margin = new Padding(0, 5, 10, 0) };
            lblLimitePrestamos = new Label { Text = "Límite: -", AutoSize = true, Margin = new Padding(0, 5, 0, 0) };

            panelInfoLector.Controls.AddRange(new Control[] { lblEstadoLector, lblPrestamosActivos, lblLimitePrestamos });
            formLayout.Controls.Add(panelInfoLector, 1, 3);
            formLayout.SetColumnSpan(panelInfoLector, 3);

            // Row 4: Botones
            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 5, 0, 0)
            };

            btnRegistrarPrestamo = new Button { Text = "Registrar Préstamo", Width = 140, Height = 28 };
            btnLimpiar = new Button { Text = "Limpiar", Width = 100, Height = 28 };
            btnCerrar = new Button { Text = "Cerrar", Width = 100, Height = 28 };

            btnPanel.Controls.AddRange(new Control[] { btnCerrar, btnLimpiar, btnRegistrarPrestamo });

            formLayout.Controls.Add(btnPanel, 0, 4);
            formLayout.SetColumnSpan(btnPanel, 4);

            gbDetallePrestamo.Controls.Add(formLayout);

            // Agregar todo al layout principal
            mainLayout.Controls.Add(gbEjemplares, 0, 0);
            mainLayout.Controls.Add(gbLectores, 1, 0);
            mainLayout.Controls.Add(gbDetallePrestamo, 0, 1);
            mainLayout.SetColumnSpan(gbDetallePrestamo, 2);

            this.Controls.Add(mainLayout);
        }
    }
}