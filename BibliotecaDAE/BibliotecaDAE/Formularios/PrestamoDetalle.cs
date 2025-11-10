using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace BibliotecaDAE
{
    public class PrestamoDetalle : Form
    {
        private DataGridView dgvEjemplares;
        private TextBox txtNombre, txtApellido, txtDireccion, txtTelefono, txtEmail, txtEdad, txtCarnet, txtDUI, txtIdEjemplar;
        private ComboBox cbxTipo;
        private Button btnGuardar, btnLimpiar, btnCerrar;
        private TableLayoutPanel mainLayout;
        private GroupBox gbLector, gbEjemplares;

        public PrestamoDetalle()
        {
            InitializeComponent();
            RegisterEvents();
        }

        private void RegisterEvents()   
        {
            this.Load += async (_, __) => await LoadEjemplaresAsync();
            dgvEjemplares.SelectionChanged += DgvEjemplares_SelectionChanged;
            btnGuardar.Click += async (_, __) => await GuardarLectorAsync();
            btnLimpiar.Click += (_, __) => LimpiarCampos();
            btnCerrar.Click += (_, __) => this.Close();
        }

        private string GetConnectionString()
        {
            try
            {
                var cs = ConfigurationManager.ConnectionStrings["Biblioteca"]?.ConnectionString;
                if (!string.IsNullOrWhiteSpace(cs)) return cs;
            }
            catch { }
            return "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Biblioteca;Integrated Security=True";
        }

        private async Task LoadEjemplaresAsync()
        {
            var cs = GetConnectionString();
            var dt = new DataTable();
            try
            {
                using var cn = new SqlConnection(cs);
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

                await cn.OpenAsync();
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

            var cs = GetConnectionString();

            try
            {
                using var cn = new SqlConnection(cs);
                await cn.OpenAsync();

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