using System;
using BibliotecaDAE.Formularios;
using SistemaControlPersonal.Core.Lib;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using BibliotecaDAE;

namespace BibliotecaDAE
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.AcceptButton = btnEntrar;
            txtUsuario.Focus();
        }

        private void SetTextBoxIfExists(string controlName, string? value)
        {
            var matches = this.Controls.Find(controlName, true);
            if (matches.Length > 0 && matches[0] is TextBox tb)
            {
                tb.Text = value ?? string.Empty;
            }
        }

        private async void btnEntrar_Click_1(object sender, EventArgs e)
        {
            string nombreUsuario = txtUsuario.Text.Trim();
            string contraseña = txtContraseña.Text;

            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrEmpty(contraseña))
            {
                MessageBox.Show("Introduce nombre de usuario y contraseña.", "Datos incompletos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnEntrar.Enabled = false;
            Cnn cnn = null;

            try
            {
                cnn = new Cnn();
                var cn = cnn.OpenDb();

                using var cmd = cn.CreateCommand();

                cmd.CommandText = "SELECT IdUsuario, Password, Nombre, Rol, DUI FROM Usuario WHERE CAST(NombreUsuario AS VARBINARY(50)) = CAST(@user AS VARBINARY(50))";
                cmd.Parameters.AddWithValue("@user", nombreUsuario);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string passwordInDb = reader["Password"] as string ?? string.Empty;

                    if (passwordInDb == contraseña)
                    {

                        int idUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario"));
                        string nombre = reader["Nombre"] as string ?? string.Empty;
                        string rol = reader["Rol"] as string ?? string.Empty;
                        string dui = reader["DUI"] as string ?? string.Empty;

                        SesionUsuario.IdUsuario = idUsuario;
                        SesionUsuario.Nombre = nombre;
                        SesionUsuario.Rol = rol;
                        SesionUsuario.NombreUsuario = nombreUsuario;

                        SetTextBoxIfExists("txtNombre", nombre);
                        SetTextBoxIfExists("txtRol", rol);
                        SetTextBoxIfExists("txtDUI", dui);

                        reader.Close();
                        cnn.CloseDB();

                        this.Hide();
                        using var f = new frmPrestamos();
                        f.ShowDialog(this);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Contraseña incorrecta.", "Acceso denegado",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtContraseña.Clear();
                        txtContraseña.Focus();
                    }
                }
                else
                {
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
    }
}