// IMPORTACIONES
using SistemaControlPersonal.Core.Lib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BibliotecaDAE.Formularios
{
    /// Formulario para configurar y enviar reportes de préstamos por correo electrónico
    /// Permite filtrar datos, generar HTML y enviar mediante FormSubmit
    public partial class frmEnviarReporte : Form
    {
        #region Campos y Propiedades

        // Referencia al DataGridView con los datos de préstamos
        private readonly DataGridView dgvSource;

        // Controles del formulario
        private TextBox txtCorreoDestino;
        private TextBox txtAsunto;
        private TextBox txtMensaje;
        private CheckBox chkIncluirEstadisticas;
        private CheckBox chkSoloActivos;
        private CheckBox chkSoloVencidos;
        private Button btnEnviar;
        private Button btnCancelar;
        private Label lblUsuario;
        private Label lblFecha;
        private ProgressBar progressBar;

        // Constantes
        private const string CORREO_DESTINO_DEFAULT = "davidrodlfo.2005@gmail.com";
        private const string FORMSUBMIT_ENDPOINT = "https://formsubmit.co/davidrodlfo.2005@gmail.com";

        #endregion

        #region Constructor

        /// Constructor del formulario de envío de reportes
        /// <param name="dataGridView">DataGridView con los datos a reportar</param>
        public frmEnviarReporte(DataGridView dataGridView)
        {
            dgvSource = dataGridView ?? throw new ArgumentNullException(nameof(dataGridView));
            InitializeComponent();
            CargarInformacionInicial();
        }

        /// Carga la información inicial del formulario
        /// Incluye datos del usuario logueado y valores por defecto
        private void CargarInformacionInicial()
        {
            // Información del usuario actual
            lblUsuario.Text = $"Generado por: {SesionUsuario.Nombre} ({SesionUsuario.Rol})";
            lblFecha.Text = $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}";

            // Valores predeterminados
            txtCorreoDestino.Text = CORREO_DESTINO_DEFAULT;
            txtAsunto.Text = $"Reporte de Préstamos - {DateTime.Now:dd/MM/yyyy}";
            txtMensaje.Text = "Adjunto el reporte de préstamos de la biblioteca.";
        }

        #endregion

        #region Módulo de Envío de Correo

        /// Maneja el evento de clic en el botón Enviar
        /// Valida datos, genera reporte y envía por correo
        private async void btnEnviar_Click(object sender, EventArgs e)
        {
            // Validar correo de destino
            if (!ValidarCorreoDestino()) return;

            // Deshabilitar controles durante el proceso
            ConfigurarEstadoEnvio(true);

            try
            {
                // Generar reporte HTML con los datos filtrados
                string reporteHtml = GenerarReporteHTML();

                // Enviar correo mediante FormSubmit
                bool enviado = await EnviarCorreoAsync(reporteHtml);

                if (enviado)
                {
                    MessageBox.Show($"Reporte enviado correctamente al correo:\n{txtCorreoDestino.Text}",
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo enviar el reporte. Intente nuevamente.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar reporte:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ConfigurarEstadoEnvio(false);
            }
        }

        /// Valida el formato del correo electrónico de destino
        /// <returns>True si el correo es válido</returns>
        private bool ValidarCorreoDestino()
        {
            if (string.IsNullOrWhiteSpace(txtCorreoDestino.Text))
            {
                MessageBox.Show("Debe ingresar un correo de destino.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!EsCorreoValido(txtCorreoDestino.Text))
            {
                MessageBox.Show("El formato del correo no es válido.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        /// Configura el estado de los controles durante el envío
        /// <param name="enviando">True si está en proceso de envío</param>
        private void ConfigurarEstadoEnvio(bool enviando)
        {
            btnEnviar.Enabled = !enviando;
            btnCancelar.Enabled = !enviando;
            progressBar.Visible = enviando;
            progressBar.Style = enviando ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
        }

        /// Envía el reporte por correo usando FormSubmit
        /// <param name="htmlContent">Contenido HTML del reporte</param>
        /// <returns>True si se envió correctamente</returns>
        private async Task<bool> EnviarCorreoAsync(string htmlContent)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // --- INICIO DE LA CORRECCIÓN ---
                    // Agregamos un encabezado 'Referer' para simular que venimos de un sitio web.
                    // FormSubmit puede requerir esto para aceptar la solicitud desde una app de escritorio.
                    client.DefaultRequestHeaders.Referrer = new Uri("http://localhost");
                    // --- FIN DE LA CORRECCIÓN ---

                    var formContent = new MultipartFormDataContent
                    {
                        { new StringContent(txtCorreoDestino.Text), "email" },
                        { new StringContent(txtAsunto.Text), "asunto" },
                        { new StringContent(txtMensaje.Text), "mensaje" },
                        { new StringContent(htmlContent), "reporte_html" },
                        { new StringContent(SesionUsuario.Nombre), "usuario" },
                        { new StringContent(SesionUsuario.Rol), "rol" },
                        { new StringContent(DateTime.Now.ToString()), "fecha_generacion" }
                    };

                    var response = await client.PostAsync(FORMSUBMIT_ENDPOINT, formContent);
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        /// Valida el formato de una dirección de correo electrónico
        /// <param name="correo">Correo a validar</param>
        /// <returns>True si el formato es válido</returns>
        private bool EsCorreoValido(string correo)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(correo);
                return addr.Address == correo;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Módulo de Generación de Reporte

        /// Genera el contenido HTML completo del reporte
        /// Incluye estilos CSS, estadísticas y tabla de datos
        /// <returns>HTML del reporte como string</returns>
        private string GenerarReporteHTML()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHTML());
            sb.AppendLine(GenerarTituloReporte());
            var filas = FiltrarDatos();
            if (chkIncluirEstadisticas.Checked)
            {
                sb.AppendLine(GenerarEstadisticas(filas));
            }
            sb.AppendLine(GenerarTablaHTML(filas));
            sb.AppendLine(GenerarPiePagina());
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        /// Genera el encabezado HTML con estilos CSS
        private string GenerarEncabezadoHTML()
        {
            return @"<!DOCTYPE html>
            <html><head><meta charset='utf-8'>
                <style>
                    body { font-family: Arial, sans-serif; margin: 20px; background-color: #f8f9fa; }
                    h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }
                    .metadata { color: #7f8c8d; margin-bottom: 20px; }
                    table { border-collapse: collapse; width: 100%; margin-top: 20px; background-color: white; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
                    th { background-color: #3498db; color: white; padding: 12px; text-align: left; font-weight: bold; }
                    td { padding: 10px; border-bottom: 1px solid #ddd; }
                    tr:hover { background-color: #f5f5f5; }
                    .stats { background-color: #ecf0f1; padding: 20px; border-radius: 8px; margin: 20px 0; display: flex; flex-wrap: wrap; gap: 20px; }
                    .stat-item { flex: 1; min-width: 150px; text-align: center; }
                    .stat-number { font-size: 28px; font-weight: bold; color: #3498db; margin-bottom: 5px; }
                    .stat-label { color: #7f8c8d; font-size: 14px; }
                    .footer { margin-top: 30px; padding-top: 20px; border-top: 2px solid #ddd; color: #7f8c8d; font-size: 12px; }
                    .vencido { color: #e74c3c; font-weight: bold; }
                    .activo { color: #27ae60; font-weight: bold; }
                    .devuelto { color: #95a5a6; }
                    .perdido { color: #c0392b; font-weight: bold; }
                </style></head><body>";
        }

        /// Genera el título y metadatos del reporte
        private string GenerarTituloReporte()
        {
            return $@"
<h1>Reporte de Préstamos - Biblioteca DAE</h1>
<div class='metadata'>
    <p><strong>Generado por:</strong> {SesionUsuario.Nombre} ({SesionUsuario.Rol})</p>
    <p><strong>Fecha de generación:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
    <p><strong>Usuario ID:</strong> {SesionUsuario.IdUsuario}</p>
</div>";
        }

        /// Filtra los datos del DataGridView según las opciones seleccionadas
        /// <returns>Lista de filas filtradas</returns>
        private List<DataGridViewRow> FiltrarDatos()
        {
            var filas = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in dgvSource.Rows)
            {
                if (row.IsNewRow) continue;
                var estado = row.Cells["EstadoPrestamo"].Value?.ToString() ?? "";
                if (chkSoloActivos.Checked && estado != "Activo") continue;
                if (chkSoloVencidos.Checked && estado != "Vencido") continue;
                filas.Add(row);
            }
            return filas;
        }

        /// Genera la sección de estadísticas del reporte
        /// <param name="filas">Lista de filas a analizar</param>
        /// <returns>HTML con las estadísticas</returns>
        private string GenerarEstadisticas(List<DataGridViewRow> filas)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div class='stats'>");
            sb.AppendLine("<h3 style='width: 100%; margin-bottom: 15px;'>Estadísticas del Reporte</h3>");

            int total = filas.Count;
            int activos = filas.Count(r => r.Cells["EstadoPrestamo"].Value?.ToString() == "Activo");
            int vencidos = filas.Count(r => r.Cells["EstadoPrestamo"].Value?.ToString() == "Vencido");
            int devueltos = filas.Count(r => r.Cells["EstadoPrestamo"].Value?.ToString() == "Devuelto");
            int perdidos = filas.Count(r => r.Cells["EstadoPrestamo"].Value?.ToString() == "Perdido");

            sb.AppendLine(GenerarStatCard(total, "Total"));
            sb.AppendLine(GenerarStatCard(activos, "Activos"));
            sb.AppendLine(GenerarStatCard(vencidos, "Vencidos"));
            sb.AppendLine(GenerarStatCard(devueltos, "Devueltos"));
            sb.AppendLine(GenerarStatCard(perdidos, "Perdidos"));

            sb.AppendLine("</div>");
            return sb.ToString();
        }

        /// Genera un card individual de estadística
        private string GenerarStatCard(int valor, string etiqueta)
        {
            return $@"
            <div class='stat-item'>
                <div class='stat-number'>{valor}</div>
                <div class='stat-label'>{etiqueta}</div>
            </div>";
        }

        /// Genera la tabla HTML con los datos de préstamos
        /// <param name="filas">Lista de filas a incluir</param>
        /// <returns>HTML de la tabla</returns>
        private string GenerarTablaHTML(List<DataGridViewRow> filas)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr>");
            foreach (DataGridViewColumn col in dgvSource.Columns)
            {
                if (col.Visible)
                {
                    sb.AppendLine($"<th>{col.HeaderText}</th>");
                }
            }
            sb.AppendLine("</tr></thead><tbody>");
            foreach (DataGridViewRow row in filas)
            {
                if (row.IsNewRow) continue;
                sb.AppendLine("<tr>");
                foreach (DataGridViewColumn col in dgvSource.Columns)
                {
                    if (col.Visible)
                    {
                        var valor = row.Cells[col.Index].Value?.ToString() ?? "";
                        string claseCSS = col.Name == "EstadoPrestamo" ? ObtenerClaseEstado(valor) : "";
                        sb.AppendLine($"<td class='{claseCSS}'>{valor}</td>");
                    }
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tbody></table>");
            return sb.ToString();
        }

        /// Obtiene la clase CSS según el estado del préstamo
        private string ObtenerClaseEstado(string estado)
        {
            return estado switch
            {
                "Vencido" => "vencido",
                "Activo" => "activo",
                "Devuelto" => "devuelto",
                "Perdido" => "perdido",
                _ => ""
            };
        }

        /// Genera el pie de página del reporte
        private string GenerarPiePagina()
        {
            return $@"
            <div class='footer'>
                <p><strong>Sistema de Gestión de Biblioteca DAE</strong></p>
                <p>Este reporte fue generado automáticamente y contiene información actualizada al momento de su creación.</p>
                <p>© {DateTime.Now.Year} - Todos los derechos reservados</p>
            </div>";
        }

        #endregion

        #region Eventos de Controles

        /// Maneja el evento de clic en el botón Cancelar
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// Maneja cambios en los checkboxes de filtro
        private void ConfigurarCheckboxesMutuosExcluyentes()
        {
            chkSoloActivos.CheckedChanged += (s, e) => {
                if (chkSoloActivos.Checked) chkSoloVencidos.Checked = false;
            };
            chkSoloVencidos.CheckedChanged += (s, e) => {
                if (chkSoloVencidos.Checked) chkSoloActivos.Checked = false;
            };
        }

        #endregion

        #region InitializeComponent - Diseño de la Interfaz

        /// Inicializa todos los componentes visuales del formulario
        private void InitializeComponent()
        {
            this.Text = "Enviar Reporte por Correo Electrónico";
            this.ClientSize = new Size(650, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 9f);

            var mainPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8, Padding = new Padding(15) };
            var panelInfo = new GroupBox { Dock = DockStyle.Fill, Height = 70, Text = "Información del Reporte", Padding = new Padding(10) };
            lblUsuario = new Label { Text = "Usuario:", Location = new Point(10, 25), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblFecha = new Label { Text = "Fecha:", Location = new Point(10, 45), AutoSize = true };
            panelInfo.Controls.AddRange(new Control[] { lblUsuario, lblFecha });

            var panelCorreo = new Panel { Dock = DockStyle.Fill, Height = 60 };
            panelCorreo.Controls.Add(new Label { Text = "Correo destino:*", Location = new Point(0, 5), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
            txtCorreoDestino = new TextBox { Location = new Point(0, 25), Width = 600, MaxLength = 100, Font = new Font("Segoe UI", 9.5f) };
            panelCorreo.Controls.Add(txtCorreoDestino);

            var panelAsunto = new Panel { Dock = DockStyle.Fill, Height = 60 };
            panelAsunto.Controls.Add(new Label { Text = "Asunto:*", Location = new Point(0, 5), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
            txtAsunto = new TextBox { Location = new Point(0, 25), Width = 600, MaxLength = 200, Font = new Font("Segoe UI", 9.5f) };
            panelAsunto.Controls.Add(txtAsunto);

            var panelMensaje = new Panel { Dock = DockStyle.Fill, Height = 90 };
            panelMensaje.Controls.Add(new Label { Text = "Mensaje:", Location = new Point(0, 5), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
            txtMensaje = new TextBox { Location = new Point(0, 25), Width = 600, Height = 60, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", 9.5f) };
            panelMensaje.Controls.Add(txtMensaje);

            var panelOpciones = new GroupBox { Text = "Opciones del Reporte", Dock = DockStyle.Fill, Height = 110, Padding = new Padding(10) };
            chkIncluirEstadisticas = new CheckBox { Text = "Incluir estadísticas resumidas", Location = new Point(10, 25), Checked = true, AutoSize = true, Font = new Font("Segoe UI", 9f) };
            chkSoloActivos = new CheckBox { Text = "Solo préstamos activos", Location = new Point(10, 50), AutoSize = true, Font = new Font("Segoe UI", 9f) };
            chkSoloVencidos = new CheckBox { Text = "Solo préstamos vencidos", Location = new Point(10, 75), AutoSize = true, Font = new Font("Segoe UI", 9f) };
            panelOpciones.Controls.AddRange(new Control[] { chkIncluirEstadisticas, chkSoloActivos, chkSoloVencidos });
            ConfigurarCheckboxesMutuosExcluyentes();

            progressBar = new ProgressBar { Dock = DockStyle.Fill, Visible = false, MarqueeAnimationSpeed = 30, Height = 25 };

            var panelBotones = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, Height = 45, Padding = new Padding(0, 5, 0, 0) };
            btnEnviar = new Button { Text = "Enviar Reporte", Width = 150, Height = 35, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnEnviar.FlatAppearance.BorderSize = 0;
            btnEnviar.Click += btnEnviar_Click;
            btnCancelar = new Button { Text = "Cancelar", Width = 100, Height = 35, Font = new Font("Segoe UI", 9.5f), Cursor = Cursors.Hand };
            btnCancelar.Click += btnCancelar_Click;
            panelBotones.Controls.AddRange(new Control[] { btnEnviar, btnCancelar });

            mainPanel.Controls.Add(panelInfo, 0, 0);
            mainPanel.Controls.Add(panelCorreo, 0, 1);
            mainPanel.Controls.Add(panelAsunto, 0, 2);
            mainPanel.Controls.Add(panelMensaje, 0, 3);
            mainPanel.Controls.Add(panelOpciones, 0, 4);
            mainPanel.Controls.Add(progressBar, 0, 5);
            mainPanel.Controls.Add(panelBotones, 0, 6);

            this.Controls.Add(mainPanel);
        }

        #endregion
    }
}