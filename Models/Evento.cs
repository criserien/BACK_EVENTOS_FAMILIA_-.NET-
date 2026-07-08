namespace GestionEventosAPI.Models
{
    public class Evento
    {
        public int Id { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Lugar { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty; // Puedes usar string o DateTime
        public string Hora { get; set; } = string.Empty;

        // Eventos opcionales (mapeados a null si vienen vacíos)
        public string? Evento_1 { get; set; }
        public string? Evento_2 { get; set; }
        public string? Evento_3 { get; set; }
        public string? Evento_4 { get; set; }
        public string? Evento_5 { get; set; }
        public string? Evento_6 { get; set; }
        public string? Evento_7 { get; set; }
    }
}