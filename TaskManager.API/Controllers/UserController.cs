using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using TaskManager.API.Models;
using TaskManager.API.Models.Services;
using TaskManager.Common.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TaskManager.API.Controllers
{
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
                            ShortUserModel? user = null;
                            while (reader.Read())
                            {
                                user = new ShortUserModel(
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
                var request = Request;
                var userEmail = GetUserEmailByToken(request).Result;
                switch (userEmail)
                {
                    case "TOKENSEXPIRED":
                        return BadRequest("User tokens expired! Please re-authorize!");
                    case "UNABLETOREADTOKEN":
                        return NotFound("User with that token hasn't found!");
                }
                using (var connection = GetOpenConnection())
                {
                    string sql = "UPDATE Users " +
                        "SET firstname = @FirstName, " +
                        "lastname = @LastName, " +
                        "password = @Password, " +
                        "phone = @Phone, " +
                        "status = @Status, " +
                        "email = @NewEmail, " +
                        "WHERE email = @Email;";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", userModel.FirstName);
                        command.Parameters.AddWithValue("@LastName", userModel.LastName);
                        command.Parameters.AddWithValue("@Password", userModel.Password);
                        command.Parameters.AddWithValue("@Phone", userModel.Phone);
                        command.Parameters.AddWithValue("@NewEmail", userModel.Email);
                        command.Parameters.AddWithValue("@Email", userEmail);
                        command.Parameters.AddWithValue("@Status", (int)userModel.Status);
                        await command.ExecuteNonQueryAsync();
                    }

                    sql = "SELECT * FROM Users WHERE email=@Email";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", userModel.Email);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            ShortUserModel user = null;
                            while (reader.Read())
                            {
                                user = new ShortUserModel(
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
        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                var request = Request;
                var userEmail = GetUserEmailByToken(request).Result;
                switch (userEmail)
                {
                    case "TOKENSEXPIRED":
                        return BadRequest("User tokens expired! Please re-authorize!");
                    case "UNABLETOREADTOKEN":
                        return NotFound("User with that token hasn't found!");
                }

                using (var connection = GetOpenConnection())
                {
                    string sql = "DELETE FROM Users WHERE Email = @Email;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", userEmail);

                        await command.ExecuteNonQueryAsync();
                    }
                    sql = "DELETE FROM tokens WHERE Email = @Email;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", userEmail);

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

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var request = Request;
                var userEmail = GetUserEmailByToken(request).Result;
                switch (userEmail)
                {
                    case "TOKENSEXPIRED":
                        return BadRequest("User tokens expired! Please re-authorize!");
                    case "UNABLETOREADTOKEN":
                        return NotFound("User with that token hasn't found!");
                }
                using (var connection = GetOpenConnection())
                {
                    string sql = "SELECT * FROM Users WHERE email=@Email";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", userEmail);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            List<ShortUserModel> users = new List<ShortUserModel>();
                            while (reader.Read())
                            {
                                ShortUserModel user = new ShortUserModel(
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
            UserService service = new UserService(_configuration);
            TokenResults result =  await service.ValidateTokens(request);
            if(result == TokenResults.UnableToRead) 
            {
                return "UNABLETOREADTOKEN";
            }
            if(result == TokenResults.Expired)
            {
                return "TOKENSEXPIRED";
            }

            using (var connection = GetOpenConnection())
            {
                var sql = "SELECT * FROM tokens WHERE accessToken=@Token";
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
