namespace BibliotecaDAE
{
    public static class SesionUsuario
    {
        public static int IdUsuario { get; set; }
        public static string Nombre { get; set; }
        public static string NombreUsuario { get; set; }
        public static string Rol { get; set; }

        public static void Limpiar()
        {
            IdUsuario = 0;
            Nombre = null;
            NombreUsuario = null;
            Rol = null;
        }
    }
}