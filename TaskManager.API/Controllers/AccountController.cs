using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using TaskManager.API.Models;
using TaskManager.API.Models.Services;
using TaskManager.Common.Models;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute; 

namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;
        

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
            _userService = new UserService(configuration);
            
        }

        /// <summary>
        /// Создает соединение с базой данных
        /// </summary>
        /// <returns>Соединение с базой данных</returns>
        private NpgsqlConnection GetOpenConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Получает информацию о пользователе по email
        /// </summary>
        /// <param name="UserEmail">Электронная почта пользователя</param>
        /// <returns>Данные о конкретном пользователе</returns>
        [Authorize]
        [HttpGet("info")]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            string? username = HttpContext.User.Identity.Name;
            using (var connection = GetOpenConnection())
            {
                string sqlCommandBase = "SELECT * FROM users WHERE email = @Email";
                using (var command = new NpgsqlCommand(sqlCommandBase, connection))
                {
                    command.Parameters.AddWithValue("@Email", username);
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
                        return Ok(user);
                    }
                }
            }
        }

        /// <summary>
        /// Создание нового токена пользователя
        /// </summary>
        /// <returns>Новый токен пользователя</returns>
        [HttpPost("token")]
        public async Task<IActionResult> GetToken()
        {
            var userData = await _userService.GetUserLoginPassFromBasicAuth(Request);
            var login = userData.Item1;
            var pass = userData.Item2;
            var identity = _userService.GetIdentity(login, pass).Result;

            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: DateTime.Now,
                    claims: identity.Claims,
                    expires: DateTime.Now.Add(TimeSpan.FromHours(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                refresh_token = GenerateRefreshToken(encodedJwt.Length)
            };

            using (var connection = GetOpenConnection())
            {
                string sqlCommandBase = "SELECT * FROM tokens WHERE email = @Email";
                string email = "";
                using (var command = new NpgsqlCommand(sqlCommandBase, connection))
                {
                    command.Parameters.AddWithValue("@Email", identity.Name.ToString());
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        
                        while (reader.Read())
                        {
                            email = reader["email"].ToString();
                        }
                    }
                    if (!email.IsNullOrEmpty())
                    {
                        Console.WriteLine("1");
                        sqlCommandBase = "UPDATE tokens SET accessToken=@Token, refreshToken=@RefreshToken WHERE email=@Email";
                        using (var update = new NpgsqlCommand(sqlCommandBase, connection))
                        {
                            update.Parameters.AddWithValue("@Token", response.access_token.ToString());
                            update.Parameters.AddWithValue("@RefreshToken", response.refresh_token.ToString());
                            update.Parameters.AddWithValue("@Email", identity.Name);
                            await update.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        Console.WriteLine("2");
                        sqlCommandBase = "INSERT INTO tokens VALUES(@Token, @RefreshToken, @RefreshExpires, @AccessExpires, @Email)";
                        using (var insert = new NpgsqlCommand(sqlCommandBase, connection))
                        {
                            insert.Parameters.AddWithValue("@Token", response.access_token.ToString());
                            insert.Parameters.AddWithValue("@RefreshToken", response.refresh_token.ToString());
                            insert.Parameters.AddWithValue("@Email", identity.Name);
                            insert.Parameters.AddWithValue("@AccessExpires", DateTime.Now + TimeSpan.FromHours(2));
                            insert.Parameters.AddWithValue("@RefreshExpires", DateTime.Now + TimeSpan.FromDays(3));
                            await insert.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            return Ok(response);
        }

        static string GenerateRefreshToken(int lenght)
        {
            byte[] randomNumber = new byte[lenght];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
