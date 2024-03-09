using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Serilog;
using System.Security.Claims;
using System.Text;
using TaskManager.API.Controllers;
using TaskManager.Common.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TaskManager.API.Models.Services
{
    public class UserService
    {
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Создание соединения с базой данных
        /// </summary>
        /// <returns>Открытое соединение с базой данных</returns>
        private NpgsqlConnection GetOpenConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Возвращает кортеж с логин и паролем пользователя.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Кортеж с логин и паролем пользователя</returns>
        public async Task<Tuple<string, string>> GetUserLoginPassFromBasicAuth(HttpRequest request)
        {
            string userLogin = "";
            string userPassword = "";
            string authHeader = request.Headers["Authorization"].ToString();
            string[] namePassArray = DecodeBase64(authHeader).Split(':');

            userLogin = namePassArray[0];
            userPassword = namePassArray[1];
            
            Console.WriteLine($"{userLogin} {userPassword}");
            return Tuple.Create(userLogin, userPassword);
        }

        static string DecodeBase64(string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString.Replace("Basic ", ""));

            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Получение информации о пользователе с указаным логином и паролем
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>Безопасная модель пользователя</returns>
        public async Task<UserModel> GetUser(string email, string password)
        {
            using (var connection = GetOpenConnection())
            {
                string sqlCommandBase = "SELECT * FROM users WHERE email = @Email AND password = @Password";
                using (var command = new NpgsqlCommand(sqlCommandBase, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        UserModel user = new UserModel("notFound", "notFound", "notFound", "notFound", "notFound", 0, 0);
                        while (reader.Read())
                        {
                            user = new UserModel(
                                reader["FirstName"].ToString(),
                                reader["LastName"].ToString(),
                                reader["Email"].ToString(),
                                reader["Password"].ToString(),
                                reader["Phone"].ToString(),
                                (UserStatus)Enum.ToObject(typeof(UserStatus), reader["Status"]),
                                Convert.ToInt32(reader["Id"])
                                
                            );
                            Console.WriteLine($"{user.FirstName}, {user.LastName}, {user.Email}, {user.Status}, {user.Id}, {user.Password}, {user.Phone}");
                        }
                        return user;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<ClaimsIdentity?> GetIdentity(string email, string password)
        {
            UserModel currentUser = await GetUser(email, password);

            if (currentUser != null)
            {
                Console.WriteLine($"{currentUser.FirstName}, {currentUser.LastName}, {currentUser.Email}, {currentUser.Status}, {currentUser.Id}, {currentUser.Password}, {currentUser.Phone}");

                currentUser.LastLoginDate = DateTime.Now;

                await UpdateUser(currentUser);

                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, currentUser.Email),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, currentUser.Status.ToString())
                };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                    claims,
                    "Token",
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType
                );

                return claimsIdentity;
            }

            return null;
        }

        public async Task UpdateUser(UserModel userModel)
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
                        "email = @Email " +
                        "WHERE email = @Email;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", userModel.FirstName);
                        command.Parameters.AddWithValue("@LastName", userModel.LastName);
                        command.Parameters.AddWithValue("@Password", userModel.Password);
                        command.Parameters.AddWithValue("@Phone", userModel.Phone);
                        command.Parameters.AddWithValue("@Email", userModel.Email);
                        command.Parameters.AddWithValue("@Status", (int)userModel.Status);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование исключения
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
