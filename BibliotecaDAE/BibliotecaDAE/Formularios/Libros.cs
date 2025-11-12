using Microsoft.Data.SqlClient;
using SistemaControlPersonal.Core.Lib;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BibliotecaDAE.Formularios
{
    public partial class frmLibros : Form
    {
        private DataGridView dgvLibros;
        private TextBox txtIdLibro, txtISBN, txtTitulo, txtEditorial, txtAnio;
        private CheckBox chkEstado;
        private ComboBox cbAutor;
        private CheckedListBox clbGeneros;
        private Button btnGuardar, btnEliminar, btnLimpiar, btnCerrar;
        private GroupBox gbListadoLibros, gbDetalleLibro;
        private TableLayoutPanel mainLayout;
        private Label lblTotalLibros, lblLibrosActivos;

        public frmLibros()
        {
            InitializeComponent();
            RegisterEvents();
            LimpiarCampos();
        }

        private void RegisterEvents()
        {
            this.Load += async (_, __) =>
            {
                await LoadLibrosAsync();
                await CargarAutoresAsync();
                await CargarGenerosAsync();
            };

            dgvLibros.SelectionChanged += DgvLibros_SelectionChanged;
            btnGuardar.Click += async (_, __) => await GuardarLibroAsync();
            btnEliminar.Click += async (_, __) => await EliminarLibroAsync();
            btnLimpiar.Click += (_, __) => LimpiarCampos();
            btnCerrar.Click += (_, __) => this.Close();
        }

        private async Task CargarAutoresAsync()
        {
            Cnn cnn = null;
            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT IdAutor, Nombre + ' ' + Apellido AS NombreCompleto FROM dbo.Autor ORDER BY Apellido, Nombre";

                var dt = new DataTable();
                using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);

                cbAutor.DataSource = dt;
                cbAutor.DisplayMember = "NombreCompleto";
                cbAutor.ValueMember = "IdAutor";
                cbAutor.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando autores: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null) cnn.CloseDB();
            }
        }

        private async Task CargarGenerosAsync()
        {
            Cnn cnn = null;
            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT IdGenero, Nombre FROM dbo.Genero ORDER BY Nombre";

                using var reader = await cmd.ExecuteReaderAsync();
                clbGeneros.Items.Clear();

                while (await reader.ReadAsync())
                {
                    var idGenero = reader.GetInt32(0);
                    var nombre = reader.GetString(1);
                    clbGeneros.Items.Add(new { IdGenero = idGenero, Nombre = nombre });
                }

                clbGeneros.DisplayMember = "Nombre";
                clbGeneros.ValueMember = "IdGenero";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando géneros: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null) cnn.CloseDB();
            }
        }

        private async Task LoadLibrosAsync()
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
                        l.IdLibros,
                        l.ISBN,
                        l.Titulo,
                        l.Editorial,
                        l.AnioPublicacion,
                        l.Estado,
                        CASE WHEN l.Estado = 1 THEN 'Activo' ELSE 'Inactivo' END AS EstadoTexto,
                        a.Nombre + ' ' + a.Apellido AS Autor,
                        STRING_AGG(g.Nombre, ', ') AS Generos,
                        (SELECT COUNT(*) FROM dbo.Ejemplares WHERE IdLibro = l.IdLibros) AS TotalEjemplares,
                        (SELECT COUNT(*) FROM dbo.Ejemplares WHERE IdLibro = l.IdLibros AND EstadoEjemplar = 'Disponible') AS EjemplaresDisponibles
                    FROM dbo.Libros l
                    LEFT JOIN dbo.AutorLibro al ON l.IdLibros = al.IdLibro
                    LEFT JOIN dbo.Autor a ON al.IdAutor = a.IdAutor
                    LEFT JOIN dbo.GeneroLibro gl ON l.IdLibros = gl.IdLibro
                    LEFT JOIN dbo.Genero g ON gl.IdGenero = g.IdGenero
                    GROUP BY l.IdLibros, l.ISBN, l.Titulo, l.Editorial, l.AnioPublicacion, l.Estado, a.Nombre, a.Apellido
                    ORDER BY l.Titulo";

                using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);

                dgvLibros.DataSource = dt;
                ConfigurarColumnasDataGridView();
                ActualizarEstadisticas(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando libros: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null) cnn.CloseDB();
            }
        }

        private void ConfigurarColumnasDataGridView()
        {
            if (dgvLibros.Columns.Contains("IdLibros"))
                dgvLibros.Columns["IdLibros"].Visible = false;
            if (dgvLibros.Columns.Contains("Estado"))
                dgvLibros.Columns["Estado"].Visible = false;
            if (dgvLibros.Columns.Contains("ISBN"))
                dgvLibros.Columns["ISBN"].HeaderText = "ISBN";
            if (dgvLibros.Columns.Contains("Titulo"))
                dgvLibros.Columns["Titulo"].HeaderText = "Título";
            if (dgvLibros.Columns.Contains("Editorial"))
                dgvLibros.Columns["Editorial"].HeaderText = "Editorial";
            if (dgvLibros.Columns.Contains("AnioPublicacion"))
                dgvLibros.Columns["AnioPublicacion"].HeaderText = "Año";
            if (dgvLibros.Columns.Contains("EstadoTexto"))
                dgvLibros.Columns["EstadoTexto"].HeaderText = "Estado";
            if (dgvLibros.Columns.Contains("Autor"))
                dgvLibros.Columns["Autor"].HeaderText = "Autor";
            if (dgvLibros.Columns.Contains("Generos"))
                dgvLibros.Columns["Generos"].HeaderText = "Género(s)";
            if (dgvLibros.Columns.Contains("TotalEjemplares"))
                dgvLibros.Columns["TotalEjemplares"].HeaderText = "Total Ej.";
            if (dgvLibros.Columns.Contains("EjemplaresDisponibles"))
                dgvLibros.Columns["EjemplaresDisponibles"].HeaderText = "Disponibles";
        }

        private void ActualizarEstadisticas(DataTable dt)
        {
            int total = dt.Rows.Count;
            int activos = dt.AsEnumerable().Count(row => Convert.ToBoolean(row["Estado"]));

            lblTotalLibros.Text = $"Total de Libros: {total}";
            lblLibrosActivos.Text = $"Libros Activos: {activos}";
        }

        private async void DgvLibros_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvLibros.SelectedRows.Count == 0)
            {
                LimpiarCampos();
                return;
            }

            var row = dgvLibros.SelectedRows[0];

            string GetString(string col) => row.Cells[col].Value?.ToString() ?? "";

            txtIdLibro.Text = GetString("IdLibros");
            txtISBN.Text = GetString("ISBN");
            txtTitulo.Text = GetString("Titulo");
            txtEditorial.Text = GetString("Editorial");
            txtAnio.Text = GetString("AnioPublicacion");
            chkEstado.Checked = Convert.ToBoolean(row.Cells["Estado"].Value);

            btnEliminar.Enabled = true;

            // Cargar autor y géneros seleccionados
            await CargarAutorYGenerosSeleccionados(Convert.ToInt32(txtIdLibro.Text));
        }

        private async Task CargarAutorYGenerosSeleccionados(int idLibro)
        {
            Cnn cnn = null;
            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                // Cargar autor
                using (var cmdAutor = cn.CreateCommand())
                {
                    cmdAutor.CommandText = "SELECT IdAutor FROM dbo.AutorLibro WHERE IdLibro = @idLibro";
                    cmdAutor.Parameters.AddWithValue("@idLibro", idLibro);

                    var idAutor = await cmdAutor.ExecuteScalarAsync();
                    if (idAutor != null && idAutor != DBNull.Value)
                    {
                        cbAutor.SelectedValue = idAutor;
                    }
                }

                // Cargar géneros
                using (var cmdGeneros = cn.CreateCommand())
                {
                    cmdGeneros.CommandText = "SELECT IdGenero FROM dbo.GeneroLibro WHERE IdLibro = @idLibro";
                    cmdGeneros.Parameters.AddWithValue("@idLibro", idLibro);

                    using var reader = await cmdGeneros.ExecuteReaderAsync();
                    var generosSeleccionados = new System.Collections.Generic.List<int>();

                    while (await reader.ReadAsync())
                    {
                        generosSeleccionados.Add(reader.GetInt32(0));
                    }

                    // Marcar géneros en CheckedListBox
                    for (int i = 0; i < clbGeneros.Items.Count; i++)
                    {
                        dynamic item = clbGeneros.Items[i];
                        clbGeneros.SetItemChecked(i, generosSeleccionados.Contains(item.IdGenero));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando relaciones: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null) cnn.CloseDB();
            }
        }

        private async Task GuardarLibroAsync()
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtISBN.Text))
            {
                MessageBox.Show("El ISBN es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtISBN.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitulo.Text))
            {
                MessageBox.Show("El título es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitulo.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEditorial.Text))
            {
                MessageBox.Show("La editorial es obligatoria.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEditorial.Focus();
                return;
            }

            if (!int.TryParse(txtAnio.Text, out int anio) || anio < 1000 || anio > DateTime.Now.Year)
            {
                MessageBox.Show("El año de publicación es inválido.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAnio.Focus();
                return;
            }

            if (cbAutor.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un autor.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbAutor.Focus();
                return;
            }

            if (clbGeneros.CheckedItems.Count == 0)
            {
                MessageBox.Show("Debe seleccionar al menos un género.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var isbn = txtISBN.Text.Trim();
            var titulo = txtTitulo.Text.Trim();
            var editorial = txtEditorial.Text.Trim();
            var estado = chkEstado.Checked;
            int idAutor = Convert.ToInt32(cbAutor.SelectedValue);

            int? idLibro = int.TryParse(txtIdLibro.Text, out var id) ? id : (int?)null;

            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var transaction = cn.BeginTransaction();

                try
                {
                    int libroId;

                    if (idLibro.HasValue)
                    {
                        // Actualizar
                        using (var cmd = cn.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandText = @"
                                UPDATE dbo.Libros SET
                                    ISBN = @isbn,
                                    Titulo = @titulo,
                                    Editorial = @editorial,
                                    AnioPublicacion = @anio,
                                    Estado = @estado
                                WHERE IdLibros = @id";

                            cmd.Parameters.AddWithValue("@id", idLibro.Value);
                            cmd.Parameters.AddWithValue("@isbn", isbn);
                            cmd.Parameters.AddWithValue("@titulo", titulo);
                            cmd.Parameters.AddWithValue("@editorial", editorial);
                            cmd.Parameters.AddWithValue("@anio", anio);
                            cmd.Parameters.AddWithValue("@estado", estado);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        libroId = idLibro.Value;

                        // Eliminar relaciones anteriores
                        using (var cmdDel = cn.CreateCommand())
                        {
                            cmdDel.Transaction = transaction;
                            cmdDel.CommandText = "DELETE FROM dbo.AutorLibro WHERE IdLibro = @id; DELETE FROM dbo.GeneroLibro WHERE IdLibro = @id";
                            cmdDel.Parameters.AddWithValue("@id", libroId);
                            await cmdDel.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        // Insertar
                        using (var cmd = cn.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandText = @"
                                INSERT INTO dbo.Libros (ISBN, Titulo, Editorial, AnioPublicacion, Estado)
                                VALUES (@isbn, @titulo, @editorial, @anio, @estado);
                                SELECT CAST(SCOPE_IDENTITY() AS INT)";

                            cmd.Parameters.AddWithValue("@isbn", isbn);
                            cmd.Parameters.AddWithValue("@titulo", titulo);
                            cmd.Parameters.AddWithValue("@editorial", editorial);
                            cmd.Parameters.AddWithValue("@anio", anio);
                            cmd.Parameters.AddWithValue("@estado", estado);

                            libroId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        }
                    }

                    // Insertar relación con autor
                    using (var cmdAutor = cn.CreateCommand())
                    {
                        cmdAutor.Transaction = transaction;
                        cmdAutor.CommandText = "INSERT INTO dbo.AutorLibro (IdAutor, IdLibro) VALUES (@idAutor, @idLibro)";
                        cmdAutor.Parameters.AddWithValue("@idAutor", idAutor);
                        cmdAutor.Parameters.AddWithValue("@idLibro", libroId);
                        await cmdAutor.ExecuteNonQueryAsync();
                    }

                    // Insertar relaciones con géneros
                    foreach (dynamic item in clbGeneros.CheckedItems)
                    {
                        using var cmdGenero = cn.CreateCommand();
                        cmdGenero.Transaction = transaction;
                        cmdGenero.CommandText = "INSERT INTO dbo.GeneroLibro (IdGenero, IdLibro) VALUES (@idGenero, @idLibro)";
                        cmdGenero.Parameters.AddWithValue("@idGenero", item.IdGenero);
                        cmdGenero.Parameters.AddWithValue("@idLibro", libroId);
                        await cmdGenero.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();

                    MessageBox.Show("Libro guardado correctamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await LoadLibrosAsync();
                    LimpiarCampos();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                    MessageBox.Show("Ya existe un libro con ese ISBN.", "Error de duplicado",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    MessageBox.Show($"Error SQL: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null) cnn.CloseDB();
            }
        }

        private async Task EliminarLibroAsync()
        {
            if (string.IsNullOrWhiteSpace(txtIdLibro.Text))
            {
                MessageBox.Show("Debe seleccionar un libro.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Está seguro de eliminar el libro '{txtTitulo.Text}'?\n\n" +
                "ADVERTENCIA: También se eliminarán todos sus ejemplares asociados.",
                "Confirmar Eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes) return;

            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = "DELETE FROM dbo.Libros WHERE IdLibros = @id";
                cmd.Parameters.AddWithValue("@id", Convert.ToInt32(txtIdLibro.Text));

                await cmd.ExecuteNonQueryAsync();

                MessageBox.Show("Libro eliminado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadLibrosAsync();
                LimpiarCampos();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                    MessageBox.Show("No se puede eliminar este libro porque tiene ejemplares asociados.",
                        "Error de integridad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show($"Error SQL: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cnn != null) cnn.CloseDB();
            }
        }

        private void LimpiarCampos()
        {
            txtIdLibro.Clear();
            txtISBN.Clear();
            txtTitulo.Clear();
            txtEditorial.Clear();
            txtAnio.Clear();
            chkEstado.Checked = true;
            cbAutor.SelectedIndex = -1;

            for (int i = 0; i < clbGeneros.Items.Count; i++)
            {
                clbGeneros.SetItemChecked(i, false);
            }

            btnEliminar.Enabled = false;
            txtISBN.Focus();
        }

        private void InitializeComponent()
        {
            this.Text = "Gestión de Libros";
            this.ClientSize = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9f);

            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            // ===== LISTADO DE LIBROS =====
            gbListadoLibros = new GroupBox
            {
                Text = "Catálogo de Libros",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            var panelListado = new Panel { Dock = DockStyle.Fill };

            dgvLibros = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                MultiSelect = false,
                BackgroundColor = Color.White
            };

            var panelEstadisticas = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 30,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            lblTotalLibros = new Label { Text = "Total de Libros: 0", AutoSize = true, Margin = new Padding(0, 5, 20, 0), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblLibrosActivos = new Label { Text = "Libros Activos: 0", AutoSize = true, Margin = new Padding(0, 5, 0, 0), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.Green };

            panelEstadisticas.Controls.AddRange(new Control[] { lblTotalLibros, lblLibrosActivos });

            panelListado.Controls.Add(dgvLibros);
            panelListado.Controls.Add(panelEstadisticas);
            gbListadoLibros.Controls.Add(panelListado);

            // ===== DETALLE DEL LIBRO =====
            gbDetalleLibro = new GroupBox
            {
                Text = "Información del Libro",
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

            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            for (int i = 0; i < 5; i++)
                formLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

            // Row 0: ISBN / Título
            formLayout.Controls.Add(new Label { Text = "ISBN:*", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 0);
            txtISBN = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 20 };
            formLayout.Controls.Add(txtISBN, 1, 0);

            formLayout.Controls.Add(new Label { Text = "Título:*", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 0);
            txtTitulo = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 200 };
            formLayout.Controls.Add(txtTitulo, 3, 0);

            // Row 1: Editorial / Año
            formLayout.Controls.Add(new Label { Text = "Editorial:*", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 1);
            txtEditorial = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 100 };
            formLayout.Controls.Add(txtEditorial, 1, 1);

            formLayout.Controls.Add(new Label { Text = "Año Publ.:*", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 1);
            txtAnio = new TextBox { Anchor = AnchorStyles.Left, Width = 100, MaxLength = 4 };
            formLayout.Controls.Add(txtAnio, 3, 1);

            // Row 2: Autor / Estado
            formLayout.Controls.Add(new Label { Text = "Autor:*", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 2);
            cbAutor = new ComboBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, DropDownStyle = ComboBoxStyle.DropDownList };
            formLayout.Controls.Add(cbAutor, 1, 2);

            formLayout.Controls.Add(new Label { Text = "Estado:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 2);
            chkEstado = new CheckBox { Text = "Activo", Checked = true, Anchor = AnchorStyles.Left };
            formLayout.Controls.Add(chkEstado, 3, 2);

            // Row 3: Géneros (span 2 columns)
            formLayout.Controls.Add(new Label { Text = "Géneros:*", Anchor = AnchorStyles.Right | AnchorStyles.Top, AutoSize = true, Margin = new Padding(0, 5, 0, 0) }, 0, 3);
            clbGeneros = new CheckedListBox { Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom, CheckOnClick = true };
            formLayout.Controls.Add(clbGeneros, 1, 3);
            formLayout.SetColumnSpan(clbGeneros, 3);

            // Row 4: Botones
            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 5, 0, 0)
            };

            btnGuardar = new Button { Text = "Guardar", Width = 120, Height = 28 };
            btnEliminar = new Button { Text = "Eliminar", Width = 100, Height = 28, Enabled = false };
            btnLimpiar = new Button { Text = "Limpiar", Width = 100, Height = 28 };
            btnCerrar = new Button { Text = "Cerrar", Width = 100, Height = 28 };

            btnPanel.Controls.AddRange(new Control[] { btnCerrar, btnLimpiar, btnEliminar, btnGuardar });
            formLayout.Controls.Add(btnPanel, 0, 4);
            formLayout.SetColumnSpan(btnPanel, 4);

            // ID oculto
            txtIdLibro = new TextBox { Visible = false };

            gbDetalleLibro.Controls.Add(formLayout);

            mainLayout.Controls.Add(gbListadoLibros, 0, 0);
            mainLayout.Controls.Add(gbDetalleLibro, 0, 1);

            this.Controls.Add(mainLayout);
        }
    }
}