using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using TaskManager.Common.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TaskManager.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Открытие соединения с базой данных
        /// </summary>
        /// <returns>Открытое соединения с базой данных</returns>
        private NpgsqlConnection GetOpenConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Создание нового пользователя.
        /// </summary>
        /// <param name="userModel">Модель пользователя для создания.</param>
        /// <returns>Созданный пользователь.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserModel userModel)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "INSERT INTO Users(firstname, lastname, email, password, phone, registrationdate, lastlogindate, status) " +
                        "VALUES(@FirstName, @LastName, @Email, @Password, @Phone, @RegistrationDate, @LastLoginDate, @Status)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", userModel.FirstName);
                        command.Parameters.AddWithValue("@LastName", userModel.LastName);
                        command.Parameters.AddWithValue("@Email", userModel.Email);
                        command.Parameters.AddWithValue("@Password", userModel.Password);
                        command.Parameters.AddWithValue("@Phone", userModel.Phone);
                        command.Parameters.AddWithValue("@RegistrationDate", userModel.RegistrationDate);
                        command.Parameters.AddWithValue("@LastLoginDate", userModel.LastLoginDate);
                        command.Parameters.AddWithValue("@Status", (int)userModel.Status);
                        await command.ExecuteNonQueryAsync();
                    }

                    sql = "SELECT * FROM Users WHERE email=@Email";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", userModel.Email);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            SafeUserModel? user = null;
                            while (reader.Read())
                            {
                                user = new SafeUserModel(
                                    reader["FirstName"].ToString(),
                                    reader["LastName"].ToString(),
                                    reader["Email"].ToString(),
                                    reader["Phone"].ToString(),
                                    (UserStatus)Enum.ToObject(typeof(UserStatus), reader["Status"]),
                                    Convert.ToInt32(reader["Id"])
                                );
                            }
                            return Created("localhost/api/user", user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование исключения
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }
        }

        /// <summary>
        /// Обновление информации о пользователе.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <param name="userModel">Модель пользователя для обновления.</param>
        /// <returns>Обновленная информация о пользователе.</returns>
        [Authorize]
        [HttpPatch]
        public async Task<IActionResult> UpdateUser([FromBody] UserModel userModel)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "UPDATE Users " +
                        "SET firstname = @FirstName, " +
                        "lastname = @LastName, " +
                        "password = @Password, " +
                        "phone = @Phone, " +
                        "status = @Status, " +
                        "email = @Email, " +
                        "WHERE email = @Email;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", userModel.FirstName);
                        command.Parameters.AddWithValue("@LastName", userModel.LastName);
                        command.Parameters.AddWithValue("@Password", userModel.Password);
                        command.Parameters.AddWithValue("@Phone", userModel.Phone);
                        command.Parameters.AddWithValue("@Email", GetUserEmailByToken(Request));
                        command.Parameters.AddWithValue("@Status", (int)userModel.Status);
                        await command.ExecuteNonQueryAsync();
                    }

                    sql = "SELECT * FROM Users WHERE email=@Email";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", userModel.Email);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            SafeUserModel user = null;
                            while (reader.Read())
                            {
                                user = new SafeUserModel(
                                    reader["FirstName"].ToString(),
                                    reader["LastName"].ToString(),
                                    reader["Email"].ToString(),
                                    reader["Phone"].ToString(),
                                    (UserStatus)Enum.ToObject(typeof(UserStatus), reader["Status"]),
                                    Convert.ToInt32(reader["Id"])
                                );
                            }
                            return Ok(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование исключения
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }
        }

        /// <summary>
        /// Удаление пользователя по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор пользователя для удаления.</param>
        /// <returns>Результат операции удаления.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "DELETE FROM Users WHERE Id = @Id;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                // Логирование исключения
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }
        }

        /// <summary>
        /// Получение информации о пользователе по его идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <returns>Информация о пользователе.</returns>
        [HttpGet("GetUserById/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "SELECT * FROM Users WHERE id = @Id";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            List<SafeUserModel> users = new List<SafeUserModel>();
                            while (reader.Read())
                            {
                                SafeUserModel user = new SafeUserModel(
                                    reader["FirstName"].ToString(),
                                    reader["LastName"].ToString(),
                                    reader["Email"].ToString(),
                                    reader["Phone"].ToString(),
                                    (UserStatus)Enum.ToObject(typeof(UserStatus), reader["Status"]),
                                    Convert.ToInt32(reader["Id"])
                                );

                                users.Add(user);
                            }
                            return !users.IsNullOrEmpty() ? Ok(users) : NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование исключения
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }
        }

        /// <summary>
        /// Получение информации о пользователе по его адресу электронной почты.
        /// </summary>
        /// <param name="email">Адрес электронной почты пользователя.</param>
        /// <returns>Информация о пользователе.</returns>
        [HttpGet("GetUserByEmail/{email}")]
        public async Task<IActionResult> GetUser(string email)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "SELECT * FROM Users WHERE email=@Email";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            List<SafeUserModel> users = new List<SafeUserModel>();
                            while (reader.Read())
                            {
                                SafeUserModel user = new SafeUserModel(
                                    reader["FirstName"].ToString(),
                                    reader["LastName"].ToString(),
                                    reader["Email"].ToString(),
                                    reader["Phone"].ToString(),
                                    (UserStatus)Enum.ToObject(typeof(UserStatus), reader["Status"]),
                                    Convert.ToInt32(reader["Id"])
                                );

                                users.Add(user);
                            }
                            return !users.IsNullOrEmpty() ? Ok(users) : NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование исключения
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }
        }

        private async Task<string> GetUserEmailByToken(HttpRequest request)
        {
            string token = request.Headers["Authorization"].ToString();
            using (var connection = GetOpenConnection())
            {
                var sql = "SELECT * FROM tokens WHERE userToken=@Token";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Token", token.Replace("Bearer ", ""));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        string? email = null;
                        while (reader.Read())
                        {
                            email = reader["Email"].ToString();
                        }
                        return email;
                    }
                    
                }
            }
        }
    }
}
