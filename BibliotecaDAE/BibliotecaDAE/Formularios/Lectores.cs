// IMPORTACIONES
using Microsoft.Data.SqlClient;
using SistemaControlPersonal.Core.Lib;
using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Biblioteca
{
    public class FrmLectores : Form
    {
        // DECLARACIÓN DE CONTROLES
        private DataGridView dgvLectores;
        private TextBox txtNombre, txtApellido, txtDireccion, txtTelefono, txtEmail, txtEdad, txtCarnet, txtDUI;
        private ComboBox cbxTipo;
        private Button btnGuardar, btnLimpiar, btnCerrar;
        private TableLayoutPanel mainLayout;
        private GroupBox gbListadoLectores, gbDetalleLector;
        private TableLayoutPanel formLayout;
        private FlowLayoutPanel btnPanel;
        private TextBox txtIdLector;
        private ComboBox cbxEstado;
        private NumericUpDown numLimitePrestamos;
        private Button btnEliminar;

        // CONSTRUCTOR E INICIALIZACIÓN
        public FrmLectores()
        {
            InitializeComponent();
            RegisterEvents();
            LimpiarCampos();
        }

        private void RegisterEvents()
        {
            this.Load += async (_, __) => await LoadLectoresAsync();
            dgvLectores.SelectionChanged += DgvLectores_SelectionChanged;

            btnGuardar.Click += async (_, __) => await GuardarLectorAsync();
            btnEliminar.Click += async (_, __) => await EliminarLectorAsync();
            btnLimpiar.Click += (_, __) => LimpiarCampos();
            btnCerrar.Click += (_, __) => this.Close();
        }

        // LÓGICA DE DATOS (ACCESO A BD)
        private async Task LoadLectoresAsync()
        {
            var dt = new DataTable();
            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM dbo.Lector ORDER BY Apellido, Nombre";

                using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);

                dgvLectores.DataSource = dt;
                // Ocultar columnas que no son necesarias en la vista principal
                if (dgvLectores.Columns.Contains("IdLector")) dgvLectores.Columns["IdLector"].Visible = false;
                if (dgvLectores.Columns.Contains("Direccion")) dgvLectores.Columns["Direccion"].Visible = false;
                if (dgvLectores.Columns.Contains("Telefono")) dgvLectores.Columns["Telefono"].Visible = false;
                if (dgvLectores.Columns.Contains("Email")) dgvLectores.Columns["Email"].Visible = false;
                if (dgvLectores.Columns.Contains("Edad")) dgvLectores.Columns["Edad"].Visible = false;
                if (dgvLectores.Columns.Contains("LimitePrestamos")) dgvLectores.Columns["LimitePrestamos"].Visible = false;
                if (dgvLectores.Columns.Contains("FechaRegistro")) dgvLectores.Columns["FechaRegistro"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando Lectores: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null)
                {
                    cnn.CloseDB();
                }
            }
        }

        private async Task GuardarLectorAsync()
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("Nombre y Apellido son obligatorios.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cbxTipo.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un Tipo de Usuario.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cbxEstado.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un Estado.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var carnet = string.IsNullOrWhiteSpace(txtCarnet.Text) ? null : txtCarnet.Text.Trim();
            var dui = string.IsNullOrWhiteSpace(txtDUI.Text) ? null : txtDUI.Text.Trim();
            if (carnet == null && dui == null)
            {
                MessageBox.Show("Debe ingresar al menos el Carnet o el DUI del lector.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Recoleccion de datos
            var nombre = txtNombre.Text.Trim();
            var apellido = txtApellido.Text.Trim();
            var direccion = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text.Trim();
            var telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim();
            var email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
            int? edad = int.TryParse(txtEdad.Text, out var eVal) ? eVal : (int?)null;
            var tipo = cbxTipo.SelectedItem?.ToString();
            var estado = cbxEstado.SelectedItem?.ToString();
            var limite = (int)numLimitePrestamos.Value;

            int? idLector = int.TryParse(txtIdLector.Text, out var id) ? id : (int?)null;

            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();

                if (idLector.HasValue)
                {
                    // Lógica de Actualización (UPDATE)
                    cmd.CommandText = @"
                        UPDATE dbo.Lector SET
                            Nombre = @nombre, Apellido = @apellido, Direccion = @direccion,
                            Telefono = @telefono, Email = @email, Edad = @edad,
                            Carnet = @carnet, DUI = @dui, LimitePrestamos = @limite,
                            Estado = @estado, TipoUsuario = @tipo
                        WHERE IdLector = @id";

                    cmd.Parameters.AddWithValue("@id", idLector.Value);
                }
                else
                {
                    // Lógica de Creación (INSERT)
                    cmd.CommandText = @"
                        INSERT INTO dbo.Lector
                            (Nombre, Apellido, Direccion, Telefono, Email, Edad, Carnet, DUI, TipoUsuario, Estado, LimitePrestamos)
                        VALUES
                            (@nombre, @apellido, @direccion, @telefono, @email, @edad, @carnet, @dui, @tipo, @estado, @limite)";
                }

                // Asignación de parámetros (común para INSERT y UPDATE)
                cmd.Parameters.Add(new SqlParameter("@nombre", SqlDbType.NVarChar, 50) { Value = nombre });
                cmd.Parameters.Add(new SqlParameter("@apellido", SqlDbType.NVarChar, 50) { Value = apellido });
                cmd.Parameters.Add(new SqlParameter("@direccion", SqlDbType.NVarChar, 200) { Value = (object?)direccion ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@telefono", SqlDbType.NVarChar, 20) { Value = (object?)telefono ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@email", SqlDbType.NVarChar, 100) { Value = (object?)email ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@edad", SqlDbType.Int) { Value = (object?)edad ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@carnet", SqlDbType.NVarChar, 20) { Value = (object?)carnet ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@dui", SqlDbType.NVarChar, 10) { Value = (object?)dui ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@tipo", SqlDbType.NVarChar, 20) { Value = tipo });
                cmd.Parameters.Add(new SqlParameter("@estado", SqlDbType.NVarChar, 20) { Value = estado });
                cmd.Parameters.Add(new SqlParameter("@limite", SqlDbType.Int) { Value = limite });

                var rows = await cmd.ExecuteNonQueryAsync();
                MessageBox.Show("Lector guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LimpiarCampos();
                await LoadLectoresAsync();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627) // Violación de índice único (Carnet/DUI)
                    MessageBox.Show("Ya existe un lector registrado con ese Carnet o DUI.", "Error de duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    MessageBox.Show("Error SQL: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private async Task EliminarLectorAsync()
        {
            if (string.IsNullOrWhiteSpace(txtIdLector.Text))
            {
                MessageBox.Show("Debe seleccionar un lector de la lista para eliminar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"¿Está seguro de eliminar a '{txtNombre.Text} {txtApellido.Text}'?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.No) return;

            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = "DELETE FROM dbo.Lector WHERE IdLector = @id";
                cmd.Parameters.AddWithValue("@id", Convert.ToInt32(txtIdLector.Text));

                await cmd.ExecuteNonQueryAsync();
                MessageBox.Show("Lector eliminado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LimpiarCampos();
                await LoadLectoresAsync();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Error de Foreign Key (tiene préstamos)
                    MessageBox.Show("No se puede eliminar este lector, tiene préstamos asociados.", "Error de integridad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Error SQL: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // MANEJADORES DE EVENTOS (UI)
        private void DgvLectores_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvLectores.SelectedRows.Count == 0) return;

            var row = dgvLectores.SelectedRows[0];

            // Función local para obtener valores de forma segura
            string GetString(string col) => row.Cells[col].Value != DBNull.Value ? row.Cells[col].Value.ToString() : string.Empty;

            txtIdLector.Text = GetString("IdLector");
            txtNombre.Text = GetString("Nombre");
            txtApellido.Text = GetString("Apellido");
            txtDireccion.Text = GetString("Direccion");
            txtTelefono.Text = GetString("Telefono");
            txtEmail.Text = GetString("Email");
            txtEdad.Text = GetString("Edad");
            txtCarnet.Text = GetString("Carnet");
            txtDUI.Text = GetString("DUI");

            cbxTipo.SelectedItem = GetString("TipoUsuario");
            cbxEstado.SelectedItem = GetString("Estado");

            if (int.TryParse(GetString("LimitePrestamos"), out int limite))
                numLimitePrestamos.Value = limite;
            else
                numLimitePrestamos.Value = 3; // Valor por defecto si falla

            btnEliminar.Enabled = true;
        }

        // MÉTODOS AUXILIARES (HELPERS)
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
            txtIdLector.Clear();

            cbxTipo.SelectedIndex = -1;
            cbxEstado.SelectedIndex = -1;
            numLimitePrestamos.Value = 3; // Valor por defecto

            btnEliminar.Enabled = false; // Deshabilitar hasta que se seleccione uno
            txtNombre.Focus();
        }

        // INICIALIZACIÓN DE COMPONENTES (UI)
        private void InitializeComponent()
        {
            this.Text = "Gestión de Lectores";
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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));

            // GroupBox para el Listado
            gbListadoLectores = new GroupBox
            {
                Text = "Listado de Lectores",
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

            gbListadoLectores.Controls.Add(dgvLectores);

            // GroupBox para el Formulario de Detalles
            gbDetalleLector = new GroupBox
            {
                Text = "Información del Lector",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 6,
                AutoSize = true
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            for (int i = 0; i < 6; i++)
                formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));


            // Row 0: Nombre / Apellido
            formLayout.Controls.Add(new Label { Text = "Nombre:*", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 0);
            txtNombre = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 50 };
            formLayout.Controls.Add(txtNombre, 1, 0);

            formLayout.Controls.Add(new Label { Text = "Apellido:*", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 0);
            txtApellido = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 50 };
            formLayout.Controls.Add(txtApellido, 3, 0);

            // Row 1: Dirección / Teléfono
            formLayout.Controls.Add(new Label { Text = "Dirección:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 1);
            txtDireccion = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 200 };
            formLayout.Controls.Add(txtDireccion, 1, 1);

            formLayout.Controls.Add(new Label { Text = "Teléfono:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 1);
            txtTelefono = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 20 };
            formLayout.Controls.Add(txtTelefono, 3, 1);

            // Row 2: Email / Edad
            formLayout.Controls.Add(new Label { Text = "Email:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 2);
            txtEmail = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 100 };
            formLayout.Controls.Add(txtEmail, 1, 2);

            formLayout.Controls.Add(new Label { Text = "Edad:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 2);
            txtEdad = new TextBox { Anchor = AnchorStyles.Left, MaxLength = 3, Width = 140 };
            formLayout.Controls.Add(txtEdad, 3, 2);

            // Row 3: Carnet / DUI
            formLayout.Controls.Add(new Label { Text = "N° Carnet:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 3);
            txtCarnet = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 20 };
            formLayout.Controls.Add(txtCarnet, 1, 3);

            formLayout.Controls.Add(new Label { Text = "DUI:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 3);
            txtDUI = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 10 };
            formLayout.Controls.Add(txtDUI, 3, 3);

            // Row 4: ID Lector / Tipo
            formLayout.Controls.Add(new Label { Text = "ID Lector:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 4);
            txtIdLector = new TextBox { Anchor = AnchorStyles.Left, Width = 140, ReadOnly = true, BackColor = SystemColors.Control };
            formLayout.Controls.Add(txtIdLector, 1, 4);

            formLayout.Controls.Add(new Label { Text = "Tipo:*", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 4);
            cbxTipo = new ComboBox { Anchor = AnchorStyles.Left, DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            cbxTipo.Items.AddRange(new object[] { "Docente", "Estudiante" });
            formLayout.Controls.Add(cbxTipo, 3, 4);

            // Row 5: Estado / Límite
            formLayout.Controls.Add(new Label { Text = "Estado:*", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 5);
            cbxEstado = new ComboBox { Anchor = AnchorStyles.Left, DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            cbxEstado.Items.AddRange(new object[] { "Activo", "Inactivo" });
            formLayout.Controls.Add(cbxEstado, 1, 5);

            formLayout.Controls.Add(new Label { Text = "Límite Prést.:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 5);
            numLimitePrestamos = new NumericUpDown { Anchor = AnchorStyles.Left, Width = 140, Minimum = 1, Maximum = 10 };
            formLayout.Controls.Add(numLimitePrestamos, 3, 5);

            // Panel de Botones
            btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(0, 6, 0, 0)
            };
            btnGuardar = new Button { Text = "Guardar", Width = 120, Height = 28 };
            btnEliminar = new Button { Text = "Eliminar", Width = 100, Height = 28 };
            btnLimpiar = new Button { Text = "Limpiar", Width = 100, Height = 28 };
            btnCerrar = new Button { Text = "Cerrar", Width = 100, Height = 28 };

            btnPanel.Controls.AddRange(new Control[] { btnCerrar, btnLimpiar, btnEliminar, btnGuardar });

            gbDetalleLector.Controls.Add(formLayout);
            gbDetalleLector.Controls.Add(btnPanel);

            // Ensamblaje final
            mainLayout.Controls.Add(gbListadoLectores, 0, 0);
            mainLayout.Controls.Add(gbDetalleLector, 0, 1);

            this.Controls.Add(mainLayout);
        }
    }
}