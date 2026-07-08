using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using GestionEventosAPI.Models;
using System.Data;

namespace GestionEventosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Esto generará la ruta básica: api/usuarios
    public class UsuariosController : ControllerBase
    {
        private readonly string _connectionString;

        // Inyectamos la configuración de la base de datos a través del constructor
        public UsuariosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TiDBConnection")!;
        }

        // GET: /api/usuarios
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = new List<Usuario>();
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("SELECT id, username, correo FROM usuarios", connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    usuarios.Add(new Usuario
                    {
                        Id = reader.GetInt32("id"),
                        Username = reader.GetString("username"),
                        Correo = reader.GetString("correo")
                    });
                }

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { error = "Error interno al obtener los usuarios." });
            }
        }

        // GET: /api/usuarios/{correo}
        [HttpGet("{correo}")]
        public async Task<IActionResult> GetUsuarioPorCorreo(string correo)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("SELECT id, username, correo, contrasena FROM usuarios WHERE correo = @correo", connection);
                command.Parameters.AddWithValue("@correo", correo);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var usuario = new Usuario
                    {
                        Id = reader.GetInt32("id"),
                        Username = reader.GetString("username"),
                        Correo = reader.GetString("correo"),
                        Contrasena = reader.GetString("contrasena")
                    };
                    return Ok(usuario);
                }

                return NotFound(new { error = "Correo no encontrado." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }

        // POST: /api/usuarios (Registrar)
        [HttpPost]
        public async Task<IActionResult> RegistrarUsuario([FromBody] Usuario nuevoUsuario)
        {
            if (string.IsNullOrEmpty(nuevoUsuario.Username) || string.IsNullOrEmpty(nuevoUsuario.Correo) || string.IsNullOrEmpty(nuevoUsuario.Contrasena))
            {
                return BadRequest(new { error = "El username, correo y contraseña son obligatorios." });
            }

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("INSERT INTO usuarios (username, correo, contrasena) VALUES (@username, @correo, @contrasena); SELECT LAST_INSERT_ID();", connection);
                command.Parameters.AddWithValue("@username", nuevoUsuario.Username);
                command.Parameters.AddWithValue("@correo", nuevoUsuario.Correo);
                command.Parameters.AddWithValue("@contrasena", nuevoUsuario.Contrasena);

                // Ejecutamos la consulta y recuperamos el ID generado
                var insertId = Convert.ToInt32(await command.ExecuteScalarAsync());

                return StatusCode(201, new { id = insertId, username = nuevoUsuario.Username, correo = nuevoUsuario.Correo });
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Código de duplicado en MySQL
            {
                return BadRequest(new { error = "El username o el correo ya están registrados." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { error = "Error interno al registrar el usuario." });
            }
        }

        // POST: /api/usuarios/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Correo) || string.IsNullOrEmpty(request.Contrasena))
            {
                return BadRequest(new { error = "Campos obligatorios faltantes." });
            }

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("SELECT id, username, correo FROM usuarios WHERE correo = @correo AND contrasena = @contrasena", connection);
                command.Parameters.AddWithValue("@correo", request.Correo);
                command.Parameters.AddWithValue("@contrasena", request.Contrasena);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return Ok(new
                    {
                        id = reader.GetInt32("id"),
                        username = reader.GetString("username"),
                        correo = reader.GetString("correo")
                    });
                }

                return Unauthorized(new { error = "El correo o la contraseña son incorrectos." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }
    }
}
