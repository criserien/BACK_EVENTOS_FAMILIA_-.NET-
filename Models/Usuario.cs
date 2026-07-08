namespace GestionEventosAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Contrasena { get; set; } // Opcional por si no queremos retornarla en los GET
    }

    // Modelo específico para recibir los datos de Login
    public class LoginRequest
    {
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }
}