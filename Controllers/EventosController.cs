using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using GestionEventosAPI.Models;
using System.Data;

namespace GestionEventosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Genera la ruta: api/eventos
    public class EventosController : ControllerBase
    {
        private readonly string _connectionString;

        public EventosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TiDBConnection")!;
        }

        // POST: /api/eventos
        [HttpPost]
        public async Task<IActionResult> CrearEvento([FromBody] Evento nuevoEvento)
        {
            if (string.IsNullOrEmpty(nuevoEvento.Correo) || string.IsNullOrEmpty(nuevoEvento.Titulo) ||
                string.IsNullOrEmpty(nuevoEvento.Lugar) || string.IsNullOrEmpty(nuevoEvento.Fecha) || string.IsNullOrEmpty(nuevoEvento.Hora))
            {
                return BadRequest(new { error = "Los campos correo, título, lugar, fecha y hora son obligatorios." });
            }

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = @"INSERT INTO eventos 
                    (correo, titulo, lugar, fecha, hora, evento_1, evento_2, evento_3, evento_4, evento_5, evento_6, evento_7) 
                    VALUES (@correo, @titulo, @lugar, @fecha, @hora, @e1, @e2, @e3, @e4, @e5, @e6, @e7);
                    SELECT LAST_INSERT_ID();";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@correo", nuevoEvento.Correo);
                command.Parameters.AddWithValue("@titulo", nuevoEvento.Titulo);
                command.Parameters.AddWithValue("@lugar", nuevoEvento.Lugar);
                command.Parameters.AddWithValue("@fecha", nuevoEvento.Fecha);
                command.Parameters.AddWithValue("@hora", nuevoEvento.Hora);

                // Si vienen nulos o vacíos en C#, se envían como DBNull.Value a la base de datos
                command.Parameters.AddWithValue("@e1", (object?)nuevoEvento.Evento_1 ?? DBNull.Value);
                command.Parameters.AddWithValue("@e2", (object?)nuevoEvento.Evento_2 ?? DBNull.Value);
                command.Parameters.AddWithValue("@e3", (object?)nuevoEvento.Evento_3 ?? DBNull.Value);
                command.Parameters.AddWithValue("@e4", (object?)nuevoEvento.Evento_4 ?? DBNull.Value);
                command.Parameters.AddWithValue("@e5", (object?)nuevoEvento.Evento_5 ?? DBNull.Value);
                command.Parameters.AddWithValue("@e6", (object?)nuevoEvento.Evento_6 ?? DBNull.Value);
                command.Parameters.AddWithValue("@e7", (object?)nuevoEvento.Evento_7 ?? DBNull.Value);

                var insertId = Convert.ToInt32(await command.ExecuteScalarAsync());

                return StatusCode(201, new { id = insertId, message = "Evento registrado con éxito." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { error = "Error interno al registrar el evento." });
            }
        }

        // GET: /api/eventos
        [HttpGet]
        public async Task<IActionResult> ObtenerTodosLosEventos()
        {
            try
            {
                var eventos = new List<Evento>();
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("SELECT * FROM eventos ORDER BY id DESC", connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    eventos.Add(MapearEvento(reader));
                }

                return Ok(eventos);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { error = "Error interno al obtener los eventos." });
            }
        }

        // GET: /api/eventos/{correo}
        [HttpGet("{correo}")]
        public async Task<IActionResult> ObtenerEventosPorCorreo(string correo)
        {
            try
            {
                var eventos = new List<Evento>();
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("SELECT * FROM eventos WHERE correo = @correo ORDER BY id DESC", connection);
                command.Parameters.AddWithValue("@correo", correo);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    eventos.Add(MapearEvento(reader));
                }

                return Ok(eventos);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { error = "Error interno del servidor al filtrar los eventos." });
            }
        }

        // DELETE: /api/eventos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarEvento(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("DELETE FROM eventos WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await command.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return Ok(new { message = "Evento eliminado con éxito." });
                }

                return NotFound(new { error = "El evento no existe o ya fue eliminado." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { error = "Error interno del servidor al eliminar el evento." });
            }
        }

        // Función auxiliar para no repetir código al leer las columnas opcionales de la BD
        private static Evento MapearEvento(MySqlDataReader reader)
        {
            return new Evento
            {
                Id = reader.GetInt32("id"),
                Correo = reader.GetString("correo"),
                Titulo = reader.GetString("titulo"),
                Lugar = reader.GetString("lugar"),
                Fecha = reader.GetString("fecha"),
                Hora = reader.GetString("hora"),
                Evento_1 = reader.IsDBNull(reader.GetOrdinal("evento_1")) ? null : reader.GetString("evento_1"),
                Evento_2 = reader.IsDBNull(reader.GetOrdinal("evento_2")) ? null : reader.GetString("evento_2"),
                Evento_3 = reader.IsDBNull(reader.GetOrdinal("evento_3")) ? null : reader.GetString("evento_3"),
                Evento_4 = reader.IsDBNull(reader.GetOrdinal("evento_4")) ? null : reader.GetString("evento_4"),
                Evento_5 = reader.IsDBNull(reader.GetOrdinal("evento_5")) ? null : reader.GetString("evento_5"),
                Evento_6 = reader.IsDBNull(reader.GetOrdinal("evento_6")) ? null : reader.GetString("evento_6"),
                Evento_7 = reader.IsDBNull(reader.GetOrdinal("evento_7")) ? null : reader.GetString("evento_7"),
            };
        }
    }
}
