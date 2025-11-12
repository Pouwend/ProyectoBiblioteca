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
        // DECLARACIÓN DE CONTROLES
        private DataGridView dgvLibros;
        private TextBox txtIdLibro, txtISBN, txtTitulo, txtEditorial, txtAnio;
        private CheckBox chkEstado;
        private ComboBox cbAutor;
        private CheckedListBox clbGeneros;
        private Button btnGuardarLibro, btnEliminarLibro, btnLimpiarLibro;
        private GroupBox gbListadoLibros, gbDetalleLibro;
        private Label lblTotalLibros, lblLibrosActivos;

        private DataGridView dgvEjemplares;
        private TextBox txtIdEjemplar, txtCodigoEjemplar, txtTituloLibro;
        private ComboBox cbLibroEjemplar, cbEstadoEjemplar;
        private DateTimePicker dtpFechaAdquisicion;
        private Button btnGuardarEjemplar, btnEliminarEjemplar, btnLimpiarEjemplar;
        private GroupBox gbListadoEjemplares, gbDetalleEjemplar;
        private Label lblTotalEjemplares, lblDisponibles, lblPrestados, lblDanados;

        private TabControl tabControl;
        private Button btnCerrar;

        // CONSTRUCTOR E INICIALIZACIÓN
        public frmLibros()
        {
            InitializeComponent();
            RegisterEvents();
            LimpiarCamposLibros();
            LimpiarCamposEjemplares();
        }

        private void RegisterEvents()
        {
            // Evento de carga inicial
            this.Load += async (_, __) =>
            {
                await LoadLibrosAsync();
                await CargarAutoresAsync();
                await CargarGenerosAsync();
                await LoadEjemplaresAsync();
                await CargarLibrosParaEjemplaresAsync();
            };

            // Eventos de Libros
            dgvLibros.SelectionChanged += DgvLibros_SelectionChanged;
            btnGuardarLibro.Click += async (_, __) => await GuardarLibroAsync();
            btnEliminarLibro.Click += async (_, __) => await EliminarLibroAsync();
            btnLimpiarLibro.Click += (_, __) => LimpiarCamposLibros();

            // Eventos de Ejemplares
            dgvEjemplares.SelectionChanged += DgvEjemplares_SelectionChanged;
            cbLibroEjemplar.SelectedIndexChanged += CbLibroEjemplar_SelectedIndexChanged;
            btnGuardarEjemplar.Click += async (_, __) => await GuardarEjemplarAsync();
            btnEliminarEjemplar.Click += async (_, __) => await EliminarEjemplarAsync();
            btnLimpiarEjemplar.Click += (_, __) => LimpiarCamposEjemplares();

            // Evento general
            btnCerrar.Click += (_, __) => this.Close();

            // Evento al cambiar de pestaña
            tabControl.SelectedIndexChanged += async (_, __) =>
            {
                if (tabControl.SelectedIndex == 0) // Tab de Libros
                    await LoadLibrosAsync();
                else if (tabControl.SelectedIndex == 1) // Tab de Ejemplares
                    await LoadEjemplaresAsync();
            };
        }

        // LÓGICA DE LIBROS
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

                // Añadir objetos anónimos al CheckedListBox
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
                        l.IdLibros, l.ISBN, l.Titulo, l.Editorial, l.AnioPublicacion, l.Estado,
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
                ConfigurarColumnasLibros();
                ActualizarEstadisticasLibros(dt);
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

        private void ConfigurarColumnasLibros()
        {
            if (dgvLibros.Columns.Contains("IdLibros")) dgvLibros.Columns["IdLibros"].Visible = false;
            if (dgvLibros.Columns.Contains("Estado")) dgvLibros.Columns["Estado"].Visible = false;
            if (dgvLibros.Columns.Contains("ISBN")) dgvLibros.Columns["ISBN"].HeaderText = "ISBN";
            if (dgvLibros.Columns.Contains("Titulo")) dgvLibros.Columns["Titulo"].HeaderText = "Título";
            if (dgvLibros.Columns.Contains("Editorial")) dgvLibros.Columns["Editorial"].HeaderText = "Editorial";
            if (dgvLibros.Columns.Contains("AnioPublicacion")) dgvLibros.Columns["AnioPublicacion"].HeaderText = "Año";
            if (dgvLibros.Columns.Contains("EstadoTexto")) dgvLibros.Columns["EstadoTexto"].HeaderText = "Estado";
            if (dgvLibros.Columns.Contains("Autor")) dgvLibros.Columns["Autor"].HeaderText = "Autor";
            if (dgvLibros.Columns.Contains("Generos")) dgvLibros.Columns["Generos"].HeaderText = "Género(s)";
            if (dgvLibros.Columns.Contains("TotalEjemplares")) dgvLibros.Columns["TotalEjemplares"].HeaderText = "Total Ej.";
            if (dgvLibros.Columns.Contains("EjemplaresDisponibles")) dgvLibros.Columns["EjemplaresDisponibles"].HeaderText = "Disponibles";
        }

        private void ActualizarEstadisticasLibros(DataTable dt)
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
                LimpiarCamposLibros();
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

            btnEliminarLibro.Enabled = true;

            await CargarAutorYGenerosSeleccionados(Convert.ToInt32(txtIdLibro.Text));
        }

        private async Task CargarAutorYGenerosSeleccionados(int idLibro)
        {
            Cnn cnn = null;
            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                // Cargar Autor
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

                // Cargar Géneros
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

                    // Marcar los géneros en el CheckedListBox
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
                MessageBox.Show("El ISBN es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtISBN.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtTitulo.Text))
            {
                MessageBox.Show("El título es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitulo.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtEditorial.Text))
            {
                MessageBox.Show("La editorial es obligatoria.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEditorial.Focus();
                return;
            }
            if (!int.TryParse(txtAnio.Text, out int anio) || anio < 1000 || anio > DateTime.Now.Year)
            {
                MessageBox.Show("El año de publicación es inválido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAnio.Focus();
                return;
            }
            if (cbAutor.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un autor.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbAutor.Focus();
                return;
            }
            if (clbGeneros.CheckedItems.Count == 0)
            {
                MessageBox.Show("Debe seleccionar al menos un género.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Recolección de datos
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

                    // 1. Guardar o Actualizar el Libro
                    if (idLibro.HasValue)
                    {
                        // UPDATE
                        using (var cmd = cn.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandText = @"
                                UPDATE dbo.Libros SET
                                    ISBN = @isbn, Titulo = @titulo, Editorial = @editorial,
                                    AnioPublicacion = @anio, Estado = @estado
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

                        // Limpiar relaciones antiguas
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
                        // INSERT
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

                    // Insertar nueva relación de Autor
                    using (var cmdAutor = cn.CreateCommand())
                    {
                        cmdAutor.Transaction = transaction;
                        cmdAutor.CommandText = "INSERT INTO dbo.AutorLibro (IdAutor, IdLibro) VALUES (@idAutor, @idLibro)";
                        cmdAutor.Parameters.AddWithValue("@idAutor", idAutor);
                        cmdAutor.Parameters.AddWithValue("@idLibro", libroId);
                        await cmdAutor.ExecuteNonQueryAsync();
                    }

                    // Insertar nuevas relaciones de Género
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
                    await CargarLibrosParaEjemplaresAsync(); 
                    LimpiarCamposLibros();
                }
                catch
                {
                    transaction.Rollback(); // Revertir si algo falla
                    throw;
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627) // Clave única duplicada
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

                // se encarga de Ejemplares, AutorLibro y GeneroLibro.
                using var cmd = cn.CreateCommand();
                cmd.CommandText = "DELETE FROM dbo.Libros WHERE IdLibros = @id";
                cmd.Parameters.AddWithValue("@id", Convert.ToInt32(txtIdLibro.Text));

                await cmd.ExecuteNonQueryAsync();

                MessageBox.Show("Libro eliminado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadLibrosAsync();
                await CargarLibrosParaEjemplaresAsync();
                LimpiarCamposLibros();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Conflicto de Foreign Key (si no hay borrado en cascada)
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

        private void LimpiarCamposLibros()
        {
            txtIdLibro.Clear();
            txtISBN.Clear();
            txtTitulo.Clear();
            txtEditorial.Clear();
            txtAnio.Clear();
            chkEstado.Checked = true;
            cbAutor.SelectedIndex = -1;

            // Desmarcar todos los géneros
            for (int i = 0; i < clbGeneros.Items.Count; i++)
            {
                clbGeneros.SetItemChecked(i, false);
            }

            btnEliminarLibro.Enabled = false;
            txtISBN.Focus();
        }

        // LÓGICA DE EJEMPLARES
        private async Task CargarLibrosParaEjemplaresAsync()
        {
            Cnn cnn = null;
            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = @"
                    SELECT IdLibros, Titulo + ' (' + ISBN + ')' AS TituloISBN 
                    FROM dbo.Libros 
                    WHERE Estado = 1
                    ORDER BY Titulo";

                var dt = new DataTable();
                using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);

                cbLibroEjemplar.DataSource = dt;
                cbLibroEjemplar.DisplayMember = "TituloISBN";
                cbLibroEjemplar.ValueMember = "IdLibros";
                cbLibroEjemplar.SelectedIndex = -1;
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

        private void CbLibroEjemplar_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Actualiza el TextBox de título
            if (cbLibroEjemplar.SelectedIndex != -1)
            {
                var tituloCompleto = cbLibroEjemplar.Text;
                var inicio = tituloCompleto.IndexOf(" ("); // Encontrar el paréntesis del ISBN
                txtTituloLibro.Text = inicio > 0 ? tituloCompleto.Substring(0, inicio) : tituloCompleto;
            }
            else
            {
                txtTituloLibro.Clear();
            }
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
                        e.IdEjemplar, e.IdLibro, l.Titulo AS TituloLibro, l.ISBN,
                        e.CodigoEjemplar, e.EstadoEjemplar, e.FechaAdquisicion,
                        CASE 
                            WHEN e.EstadoEjemplar = 'Prestado' THEN 
                                (SELECT TOP 1 'Préstamo #' + CAST(p.IdPrestamo AS VARCHAR) + ' - ' + 
                                 lec.Nombre + ' ' + lec.Apellido
                                 FROM dbo.Prestamo p
                                 INNER JOIN dbo.Lector lec ON p.IdLector = lec.IdLector
                                 WHERE p.IdEjemplar = e.IdEjemplar 
                                   AND p.EstadoPrestamo IN ('Activo', 'Renovado')
                                 ORDER BY p.IdPrestamo DESC)
                            ELSE NULL
                        END AS InfoPrestamo
                    FROM dbo.Ejemplares e
                    INNER JOIN dbo.Libros l ON e.IdLibro = l.IdLibros
                    ORDER BY l.Titulo, e.CodigoEjemplar";

                using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);

                dgvEjemplares.DataSource = dt;
                ConfigurarColumnasEjemplares();
                ActualizarEstadisticasEjemplares(dt);
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

        private void ConfigurarColumnasEjemplares()
        {
            if (dgvEjemplares.Columns.Contains("IdEjemplar")) dgvEjemplares.Columns["IdEjemplar"].HeaderText = "ID";
            if (dgvEjemplares.Columns.Contains("IdLibro")) dgvEjemplares.Columns["IdLibro"].Visible = false;
            if (dgvEjemplares.Columns.Contains("TituloLibro")) dgvEjemplares.Columns["TituloLibro"].HeaderText = "Libro";
            if (dgvEjemplares.Columns.Contains("ISBN")) dgvEjemplares.Columns["ISBN"].HeaderText = "ISBN";
            if (dgvEjemplares.Columns.Contains("CodigoEjemplar")) dgvEjemplares.Columns["CodigoEjemplar"].HeaderText = "Código Ejemplar";
            if (dgvEjemplares.Columns.Contains("EstadoEjemplar")) dgvEjemplares.Columns["EstadoEjemplar"].HeaderText = "Estado";
            if (dgvEjemplares.Columns.Contains("FechaAdquisicion"))
            {
                dgvEjemplares.Columns["FechaAdquisicion"].HeaderText = "Fecha Adquisición";
                dgvEjemplares.Columns["FechaAdquisicion"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            if (dgvEjemplares.Columns.Contains("InfoPrestamo")) dgvEjemplares.Columns["InfoPrestamo"].HeaderText = "Info Préstamo";
        }

        private void ActualizarEstadisticasEjemplares(DataTable dt)
        {
            int total = dt.Rows.Count;
            int disponibles = 0, prestados = 0, danados = 0;

            foreach (DataRow row in dt.Rows)
            {
                string estado = row["EstadoEjemplar"].ToString();
                switch (estado)
                {
                    case "Disponible": disponibles++; break;
                    case "Prestado": prestados++; break;
                    case "Dañado":
                    case "Perdido": danados++; break;
                }
            }

            lblTotalEjemplares.Text = $"Total: {total}";
            lblDisponibles.Text = $"Disponibles: {disponibles}";
            lblDisponibles.ForeColor = Color.Green;
            lblPrestados.Text = $"Prestados: {prestados}";
            lblPrestados.ForeColor = Color.Orange;
            lblDanados.Text = $"Dañados/Perdidos: {danados}";
            lblDanados.ForeColor = Color.Red;
        }

        private void DgvEjemplares_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvEjemplares.SelectedRows.Count == 0)
            {
                LimpiarCamposEjemplares();
                return;
            }

            var row = dgvEjemplares.SelectedRows[0];
            string GetString(string col) => row.Cells[col].Value?.ToString() ?? "";

            txtIdEjemplar.Text = GetString("IdEjemplar");
            txtCodigoEjemplar.Text = GetString("CodigoEjemplar");
            txtTituloLibro.Text = GetString("TituloLibro");
            cbLibroEjemplar.SelectedValue = row.Cells["IdLibro"].Value;
            cbEstadoEjemplar.SelectedItem = GetString("EstadoEjemplar");

            if (DateTime.TryParse(GetString("FechaAdquisicion"), out DateTime fecha))
                dtpFechaAdquisicion.Value = fecha;

            // No permitir eliminar ejemplares prestados
            string estadoActual = GetString("EstadoEjemplar");
            btnEliminarEjemplar.Enabled = estadoActual != "Prestado";
            if (estadoActual == "Prestado")
            {
                btnEliminarEjemplar.Text = "No eliminable";
                btnEliminarEjemplar.BackColor = Color.LightGray;
            }
            else
            {
                btnEliminarEjemplar.Text = "Eliminar";
                btnEliminarEjemplar.BackColor = SystemColors.Control;
            }
        }

        private async Task GuardarEjemplarAsync()
        {
            // Validaciones
            if (cbLibroEjemplar.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un libro.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbLibroEjemplar.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtCodigoEjemplar.Text))
            {
                MessageBox.Show("El código del ejemplar es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCodigoEjemplar.Focus();
                return;
            }
            if (cbEstadoEjemplar.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un estado.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbEstadoEjemplar.Focus();
                return;
            }

            // Recolección de datos
            int idLibro = Convert.ToInt32(cbLibroEjemplar.SelectedValue);
            string codigo = txtCodigoEjemplar.Text.Trim();
            string estado = cbEstadoEjemplar.SelectedItem.ToString();
            DateTime fechaAdquisicion = dtpFechaAdquisicion.Value;
            int? idEjemplar = int.TryParse(txtIdEjemplar.Text, out var id) ? id : (int?)null;

            Cnn cnn = null;
            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();
                using var cmd = cn.CreateCommand();

                if (idEjemplar.HasValue)
                {
                    // UPDATE
                    cmd.CommandText = @"
                        UPDATE dbo.Ejemplares SET
                            IdLibro = @idLibro, CodigoEjemplar = @codigo,
                            EstadoEjemplar = @estado, FechaAdquisicion = @fecha
                        WHERE IdEjemplar = @id";
                    cmd.Parameters.AddWithValue("@id", idEjemplar.Value);
                }
                else
                {
                    // INSERT
                    cmd.CommandText = @"
                        INSERT INTO dbo.Ejemplares (IdLibro, CodigoEjemplar, EstadoEjemplar, FechaAdquisicion)
                        VALUES (@idLibro, @codigo, @estado, @fecha)";
                }

                cmd.Parameters.AddWithValue("@idLibro", idLibro);
                cmd.Parameters.AddWithValue("@codigo", codigo);
                cmd.Parameters.AddWithValue("@estado", estado);
                cmd.Parameters.AddWithValue("@fecha", fechaAdquisicion);

                await cmd.ExecuteNonQueryAsync();

                MessageBox.Show("Ejemplar guardado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadEjemplaresAsync();
                await LoadLibrosAsync(); // Actualizar contadores en la otra pestaña
                LimpiarCamposEjemplares();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627) // Clave única duplicada
                    MessageBox.Show("Ya existe un ejemplar con ese código.", "Error de duplicado",
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

        private async Task EliminarEjemplarAsync()
        {
            if (string.IsNullOrWhiteSpace(txtIdEjemplar.Text))
            {
                MessageBox.Show("Debe seleccionar un ejemplar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Está seguro de eliminar el ejemplar '{txtCodigoEjemplar.Text}'?\n\n" +
                $"Libro: {txtTituloLibro.Text}",
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
                cmd.CommandText = "DELETE FROM dbo.Ejemplares WHERE IdEjemplar = @id";
                cmd.Parameters.AddWithValue("@id", Convert.ToInt32(txtIdEjemplar.Text));

                await cmd.ExecuteNonQueryAsync();

                MessageBox.Show("Ejemplar eliminado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadEjemplaresAsync();
                await LoadLibrosAsync(); // Actualizar contadores
                LimpiarCamposEjemplares();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Conflicto de Foreign Key (Préstamos)
                    MessageBox.Show("No se puede eliminar este ejemplar porque tiene préstamos asociados.",
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

        private void LimpiarCamposEjemplares()
        {
            txtIdEjemplar.Clear();
            txtCodigoEjemplar.Clear();
            txtTituloLibro.Clear();
            cbLibroEjemplar.SelectedIndex = -1;
            cbEstadoEjemplar.SelectedIndex = 0;
            dtpFechaAdquisicion.Value = DateTime.Now;

            btnEliminarEjemplar.Enabled = false;
            btnEliminarEjemplar.Text = "Eliminar";
            btnEliminarEjemplar.BackColor = SystemColors.Control;
            cbLibroEjemplar.Focus();
        }


        // INICIALIZACIÓN DE COMPONENTES 
        private void InitializeComponent()
        {
            this.Text = "Gestión de Libros y Ejemplares";
            this.ClientSize = new Size(1300, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9f);

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f)
            };
            var tabLibros = new TabPage("Catálogo de Libros");

            var mainLayoutLibros = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainLayoutLibros.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            mainLayoutLibros.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            gbListadoLibros = new GroupBox
            {
                Text = "Listado de Libros",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            var panelListadoLibros = new Panel { Dock = DockStyle.Fill };

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

            var panelEstadisticasLibros = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 30,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            lblTotalLibros = new Label { Text = "Total de Libros: 0", AutoSize = true, Margin = new Padding(0, 5, 20, 0), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblLibrosActivos = new Label { Text = "Libros Activos: 0", AutoSize = true, Margin = new Padding(0, 5, 0, 0), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.Green };

            panelEstadisticasLibros.Controls.AddRange(new Control[] { lblTotalLibros, lblLibrosActivos });
            panelListadoLibros.Controls.Add(dgvLibros);
            panelListadoLibros.Controls.Add(panelEstadisticasLibros);
            gbListadoLibros.Controls.Add(panelListadoLibros);

            gbDetalleLibro = new GroupBox
            {
                Text = "Información del Libro",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            var formLayoutLibros = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 5,
                Padding = new Padding(5)
            };

            formLayoutLibros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            formLayoutLibros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formLayoutLibros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            formLayoutLibros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            for (int i = 0; i < 5; i++)
                formLayoutLibros.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

            formLayoutLibros.Controls.Add(new Label { Text = "ISBN:*", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 0);
            txtISBN = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 20 };
            formLayoutLibros.Controls.Add(txtISBN, 1, 0);

            formLayoutLibros.Controls.Add(new Label { Text = "Título:*", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 0);
            txtTitulo = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 200 };
            formLayoutLibros.Controls.Add(txtTitulo, 3, 0);

            formLayoutLibros.Controls.Add(new Label { Text = "Editorial:*", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 1);
            txtEditorial = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 100 };
            formLayoutLibros.Controls.Add(txtEditorial, 1, 1);

            formLayoutLibros.Controls.Add(new Label { Text = "Año Publ.:*", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 1);
            txtAnio = new TextBox { Anchor = AnchorStyles.Left, Width = 100, MaxLength = 4 };
            formLayoutLibros.Controls.Add(txtAnio, 3, 1);

            formLayoutLibros.Controls.Add(new Label { Text = "Autor:*", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 2);
            cbAutor = new ComboBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, DropDownStyle = ComboBoxStyle.DropDownList };
            formLayoutLibros.Controls.Add(cbAutor, 1, 2);

            formLayoutLibros.Controls.Add(new Label { Text = "Estado:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 2);
            chkEstado = new CheckBox { Text = "Activo", Checked = true, Anchor = AnchorStyles.Left };
            formLayoutLibros.Controls.Add(chkEstado, 3, 2);

            formLayoutLibros.Controls.Add(new Label { Text = "Géneros:*", Anchor = AnchorStyles.Right | AnchorStyles.Top, AutoSize = true, Margin = new Padding(0, 5, 0, 0) }, 0, 3);
            clbGeneros = new CheckedListBox { Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom, CheckOnClick = true };
            formLayoutLibros.Controls.Add(clbGeneros, 1, 3);
            formLayoutLibros.SetColumnSpan(clbGeneros, 3);

            var btnPanelLibros = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 5, 0, 0)
            };

            btnGuardarLibro = new Button { Text = "Guardar", Width = 120, Height = 28 };
            btnEliminarLibro = new Button { Text = "Eliminar", Width = 100, Height = 28, Enabled = false };
            btnLimpiarLibro = new Button { Text = "Limpiar", Width = 100, Height = 28 };

            btnPanelLibros.Controls.AddRange(new Control[] { btnLimpiarLibro, btnEliminarLibro, btnGuardarLibro });
            formLayoutLibros.Controls.Add(btnPanelLibros, 0, 4);
            formLayoutLibros.SetColumnSpan(btnPanelLibros, 4);

            txtIdLibro = new TextBox { Visible = false };

            gbDetalleLibro.Controls.Add(formLayoutLibros);

            mainLayoutLibros.Controls.Add(gbListadoLibros, 0, 0);
            mainLayoutLibros.Controls.Add(gbDetalleLibro, 0, 1);

            tabLibros.Controls.Add(mainLayoutLibros);

            var tabEjemplares = new TabPage("Inventario de Ejemplares");

            var mainLayoutEjemplares = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainLayoutEjemplares.RowStyles.Add(new RowStyle(SizeType.Percent, 60f));
            mainLayoutEjemplares.RowStyles.Add(new RowStyle(SizeType.Percent, 40f));

            gbListadoEjemplares = new GroupBox
            {
                Text = "Listado de Ejemplares",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            var panelListadoEjemplares = new Panel { Dock = DockStyle.Fill };

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

            var panelEstadisticasEjemplares = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 30,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            lblTotalEjemplares = new Label { Text = "Total: 0", AutoSize = true, Margin = new Padding(0, 5, 15, 0), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblDisponibles = new Label { Text = "Disponibles: 0", AutoSize = true, Margin = new Padding(0, 5, 15, 0), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblPrestados = new Label { Text = "Prestados: 0", AutoSize = true, Margin = new Padding(0, 5, 15, 0), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblDanados = new Label { Text = "Dañados/Perdidos: 0", AutoSize = true, Margin = new Padding(0, 5, 0, 0), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };

            panelEstadisticasEjemplares.Controls.AddRange(new Control[] { lblTotalEjemplares, lblDisponibles, lblPrestados, lblDanados });
            panelListadoEjemplares.Controls.Add(dgvEjemplares);
            panelListadoEjemplares.Controls.Add(panelEstadisticasEjemplares);
            gbListadoEjemplares.Controls.Add(panelListadoEjemplares);

            gbDetalleEjemplar = new GroupBox
            {
                Text = "Información del Ejemplar",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            var formLayoutEjemplares = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(5)
            };

            formLayoutEjemplares.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            formLayoutEjemplares.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formLayoutEjemplares.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            formLayoutEjemplares.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            for (int i = 0; i < 4; i++)
                formLayoutEjemplares.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

            formLayoutEjemplares.Controls.Add(new Label { Text = "Libro:*", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 0);
            cbLibroEjemplar = new ComboBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, DropDownStyle = ComboBoxStyle.DropDownList };
            formLayoutEjemplares.Controls.Add(cbLibroEjemplar, 1, 0);
            formLayoutEjemplares.SetColumnSpan(cbLibroEjemplar, 3);

            formLayoutEjemplares.Controls.Add(new Label { Text = "Código:*", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 1);
            txtCodigoEjemplar = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, MaxLength = 20 };
            formLayoutEjemplares.Controls.Add(txtCodigoEjemplar, 1, 1);

            formLayoutEjemplares.Controls.Add(new Label { Text = "Estado:*", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 1);
            cbEstadoEjemplar = new ComboBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, DropDownStyle = ComboBoxStyle.DropDownList };
            cbEstadoEjemplar.Items.AddRange(new object[] { "Disponible", "Prestado", "Dañado", "Perdido" });
            formLayoutEjemplares.Controls.Add(cbEstadoEjemplar, 3, 1);

            formLayoutEjemplares.Controls.Add(new Label { Text = "Fecha Adq.:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 2);
            dtpFechaAdquisicion = new DateTimePicker { Anchor = AnchorStyles.Left | AnchorStyles.Right, Format = DateTimePickerFormat.Short };
            formLayoutEjemplares.Controls.Add(dtpFechaAdquisicion, 1, 2);

            formLayoutEjemplares.Controls.Add(new Label { Text = "Título Libro:", Anchor = AnchorStyles.Right, AutoSize = true }, 2, 2);
            txtTituloLibro = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true, BackColor = SystemColors.Control };
            formLayoutEjemplares.Controls.Add(txtTituloLibro, 3, 2);

            var btnPanelEjemplares = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 5, 0, 0)
            };

            btnGuardarEjemplar = new Button { Text = "Guardar", Width = 120, Height = 28 };
            btnEliminarEjemplar = new Button { Text = "Eliminar", Width = 100, Height = 28, Enabled = false };
            btnLimpiarEjemplar = new Button { Text = "Limpiar", Width = 100, Height = 28 };

            btnPanelEjemplares.Controls.AddRange(new Control[] { btnLimpiarEjemplar, btnEliminarEjemplar, btnGuardarEjemplar });
            formLayoutEjemplares.Controls.Add(btnPanelEjemplares, 0, 3);
            formLayoutEjemplares.SetColumnSpan(btnPanelEjemplares, 4);

            txtIdEjemplar = new TextBox { Visible = false };

            gbDetalleEjemplar.Controls.Add(formLayoutEjemplares);

            mainLayoutEjemplares.Controls.Add(gbListadoEjemplares, 0, 0);
            mainLayoutEjemplares.Controls.Add(gbDetalleEjemplar, 0, 1);

            tabEjemplares.Controls.Add(mainLayoutEjemplares);

            tabControl.TabPages.Add(tabLibros);
            tabControl.TabPages.Add(tabEjemplares);

            var panelInferior = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10, 8, 10, 8)
            };

            btnCerrar = new Button { Text = "Cerrar", Width = 100, Height = 28 };
            panelInferior.Controls.Add(btnCerrar);

            this.Controls.Add(tabControl);
            this.Controls.Add(panelInferior);
        }
    }
}