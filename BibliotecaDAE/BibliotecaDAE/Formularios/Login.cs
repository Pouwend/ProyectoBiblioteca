using BibliotecaDAE.Formularios;
using SistemaControlPersonal.Core.Lib;
using Microsoft.Data.SqlClient;
using System;
using System.Windows.Forms;

namespace BibliotecaDAE
{
    // DEFINICIÓN DE LA CLASE
    public partial class frmLogin : Form
    {
        // CONSTRUCTOR
        public frmLogin()
        {
            InitializeComponent();
        }

        // MANEJADORES DE EVENTOS
        private void Form1_Load(object sender, EventArgs e)
        {
            // Configura 'Enter' para que active el botón de entrar
            this.AcceptButton = btnEntrar;
            txtUsuario.Focus();
        }

        private async void btnEntrar_Click_1(object sender, EventArgs e)
        {
            string nombreUsuario = txtUsuario.Text.Trim();
            string contraseña = txtContraseña.Text;

            // Validación de entradas
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrEmpty(contraseña))
            {
                MessageBox.Show("Introduce nombre de usuario y contraseña.", "Datos incompletos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnEntrar.Enabled = false; // Deshabilitar botón durante el proceso
            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();

                // La conversión a VARBINARY es para asegurar un login 'case-sensitive' (sensible a mayúsculas)
                cmd.CommandText = "SELECT IdUsuario, Password, Nombre, Rol, DUI FROM Usuario WHERE CAST(NombreUsuario AS VARBINARY(50)) = CAST(@user AS VARBINARY(50))";
                cmd.Parameters.AddWithValue("@user", nombreUsuario);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    string passwordInDb = reader["Password"] as string ?? string.Empty;

                    // Validación de contraseña
                    if (passwordInDb == contraseña)
                    {
                        // Éxito: Cargar datos del usuario en la Sesión estática
                        int idUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario"));
                        string nombre = reader["Nombre"] as string ?? string.Empty;
                        string rol = reader["Rol"] as string ?? string.Empty;
                        string dui = reader["DUI"] as string ?? string.Empty;

                        SesionUsuario.IdUsuario = idUsuario;
                        SesionUsuario.Nombre = nombre;
                        SesionUsuario.Rol = rol;
                        SesionUsuario.NombreUsuario = nombreUsuario;

                        // (Helper) Actualiza campos si existen en este formulario
                        SetTextBoxIfExists("txtNombre", nombre);
                        SetTextBoxIfExists("txtRol", rol);
                        SetTextBoxIfExists("txtDUI", dui);

                        reader.Close();
                        cnn.CloseDB();

                        // Navegar al formulario principal
                        this.Hide();
                        using var f = new BibliotecaDAE.Formularios.frmPrestamos();
                        f.ShowDialog(this);
                        this.Close(); // Cerrar login al salir del formulario principal
                    }
                    else
                    {
                        // Contraseña incorrecta
                        MessageBox.Show("Contraseña incorrecta.", "Acceso denegado",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtContraseña.Clear();
                        txtContraseña.Focus();
                    }
                }
                else
                {
                    // Usuario no encontrado
                    MessageBox.Show("Usuario no encontrado.", "Acceso denegado",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtUsuario.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar/leer la base de datos: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Asegurarse de cerrar la conexión y reactivar el botón
                if (cnn != null)
                {
                    cnn.CloseDB();
                }
                btnEntrar.Enabled = true;
            }
        }

        private void btnLimpiar_Click_1(object sender, EventArgs e)
        {
            txtUsuario.Clear();
            txtContraseña.Clear();
            txtUsuario.Focus();
        }

        // MÉTODOS AUXILIARES (HELPERS)
        private void SetTextBoxIfExists(string controlName, string? value)
        {
            // Este método busca un control en *este* formulario (frmLogin) por su nombre.
            var matches = this.Controls.Find(controlName, true);
            if (matches.Length > 0 && matches[0] is TextBox tb)
            {
                tb.Text = value ?? string.Empty;
            }
        }
    }
}