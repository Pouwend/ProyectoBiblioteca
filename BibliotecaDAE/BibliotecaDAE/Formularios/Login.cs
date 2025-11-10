using BibliotecaDAE.Formularios;
using System.Configuration;
using System.Data.SqlClient;


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
                MessageBox.Show("Introduce nombre de usuario y contraseña.", "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnEntrar.Enabled = false;
            try
            {
                string? connectionString = null;
                try
                {
                    connectionString = ConfigurationManager.ConnectionStrings["Biblioteca"]?.ConnectionString;
                }
                catch
                {
                }

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Biblioteca;Integrated Security=True";
                }

                using var cn = new SqlConnection(connectionString);
                using var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT Password, Nombre, Rol, DUI FROM Usuario WHERE NombreUsuario = @user";
                cmd.Parameters.AddWithValue("@user", nombreUsuario);

                await cn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string passwordInDb = reader["Password"] as string ?? string.Empty;

                    if (passwordInDb == contraseña)
                    {
                        SetTextBoxIfExists("txtNombre", reader["Nombre"]?.ToString());
                        SetTextBoxIfExists("txtRol", reader["Rol"]?.ToString());
                        SetTextBoxIfExists("txtDUI", reader["DUI"]?.ToString());

                        this.Hide();

                        using var f = new frmPrestamos();

                        f.ShowDialog(this);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Contraseña incorrecta.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtContraseña.Clear();
                        txtContraseña.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("Usuario no encontrado.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtUsuario.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar/leer la base de datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
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