using Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManager.API.Models.Global;

namespace TaskManager.API.Models.Services
{
    public class UserService : ICommonService<UserModel>
    {
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private NpgsqlConnection GetOpenConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public async Task<UserModel?> GetUser(string email, string password)
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
                        UserModel user = new UserModel();
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
                        if(user.Email.IsNullOrEmpty()) return null;
                        return user;
                    }
                }
            }
        }

        public async Task<string?> GetRefreshTokenFromDatabase(int userId)
        {
            using (var connection = GetOpenConnection())
            {
                string sqlCommandBase = "SELECT refreshToken FROM tokens WHERE userId = @Id";
                using (var command = new NpgsqlCommand(sqlCommandBase, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        string? refreshToken = "";
                        while (reader.Read())
                        {
                            refreshToken = reader["refreshToken"].ToString();
                        }
                        return refreshToken;
                    }
                }
            }
        }

        public string GetAcessToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var identity = new List<Claim> { new Claim("token_type", "access_token"), new Claim("user_id", userId.ToString()) };

            // Создайте описание токена
            var accessToken = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(identity),
                Expires = DateTime.Now.Add(TimeSpan.FromHours(2)),
                IssuedAt = DateTime.Now,
                Issuer = AuthOptions.ISSUER,
                Audience = AuthOptions.AUDIENCE,
                SigningCredentials = new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256Signature)
            };

            var encodedAccessToken = tokenHandler.CreateToken(accessToken);

            return tokenHandler.WriteToken(encodedAccessToken);
        }

        public string GetRefreshToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var identity = new[] { new Claim("token_type", "refresh_token"), new Claim("user_id", userId.ToString())};

            var refreshToken = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(identity),
                Expires = DateTime.Now + TimeSpan.FromDays(3),
                IssuedAt = DateTime.Now,
                Issuer = AuthOptions.ISSUER,
                Audience = AuthOptions.AUDIENCE,
                SigningCredentials = new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256Signature)
            };

            var encodedRefreshToken = tokenHandler.CreateToken(refreshToken);

            return tokenHandler.WriteToken(encodedRefreshToken);
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
                        "email = @Email, " +
                        "lastlogindate = @LoginDate " +
                        "WHERE email = @Email;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", userModel.FirstName);
                        command.Parameters.AddWithValue("@LastName", userModel.LastName);
                        command.Parameters.AddWithValue("@Password", userModel.Password);
                        command.Parameters.AddWithValue("@Phone", userModel.Phone);
                        command.Parameters.AddWithValue("@Email", userModel.Email);
                        command.Parameters.AddWithValue("@LoginDate", userModel.LastLoginDate);
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

        public async Task<(bool isValid, int userId)> TryGetId(HttpRequest request)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = AuthOptions.ISSUER,

                ValidateAudience = true,
                ValidAudience = AuthOptions.AUDIENCE,

                ValidateLifetime = true,

                IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                ValidateIssuerSigningKey = true
            };

            SecurityToken validatedToken;
            try
            {
                string accessToken = request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var principal = tokenHandler.ValidateToken(accessToken, validationParameters, out validatedToken);

                var userIdClaim = principal.FindFirst("user_id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return (false, -1);
                }

                if (validatedToken.ValidTo < DateTime.Now)
                {
                    var existingToken = ConvertStringToToken(await GetRefreshTokenFromDatabase(userId));
                    if (existingToken == null || existingToken.ValidTo < DateTime.Now)
                    {
                        return (false, -1);
                    }

                    var newAccessToken = GetAcessToken(userId);
                    var newRefreshToken = GetRefreshToken(userId);

                    SaveTokens(userId, newAccessToken, newRefreshToken);

                    return (true, userId);
                }

                return (true, userId);
            }
            catch (Exception)
            {
                return (false, -1);
            }
        }

        public async void SaveTokens(int userId, string accessToken, string refreshToken)
        {
            using (var connection = GetOpenConnection())
            {
                string sqlCommandBase = "SELECT userId FROM tokens WHERE userId = @Id";
                using (var command = new NpgsqlCommand(sqlCommandBase, connection))
                {
                    int id = -1;
                    command.Parameters.AddWithValue("@Id", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {

                        while (reader.Read())
                        {
                            if (int.TryParse(reader["userId"].ToString(), out userId))
                            {
                                id = userId; 
                                break;
                            }
                        }
                    }
                    if (id != -1)
                    {
                        sqlCommandBase = "UPDATE tokens SET accessToken=@Token, refreshToken=@RefreshToken WHERE userId = @Id";
                        using (var update = new NpgsqlCommand(sqlCommandBase, connection))
                        {
                            update.Parameters.AddWithValue("@Token", accessToken);
                            update.Parameters.AddWithValue("@RefreshToken", refreshToken);
                            update.Parameters.AddWithValue("@Id", userId);
                            await update.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        sqlCommandBase = "INSERT INTO tokens(accessToken, refreshToken, userId) VALUES(@Token, @RefreshToken, @Id)";
                        using (var insert = new NpgsqlCommand(sqlCommandBase, connection))
                        {
                            insert.Parameters.AddWithValue("@Token", accessToken);
                            insert.Parameters.AddWithValue("@RefreshToken", refreshToken);
                            insert.Parameters.AddWithValue("@Id", userId);
                            await insert.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }

        private static SecurityToken ConvertStringToToken(string? tokenString)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token;

            tokenHandler.ValidateToken(tokenString, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                ValidateIssuer = true,
                ValidIssuer = AuthOptions.ISSUER,
                ValidateAudience = true,
                ValidAudience = AuthOptions.AUDIENCE,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out token);

            return token;
        }

        public ResultModel Create(UserModel model)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "INSERT INTO Users(firstname, lastname, email, password, phone, registrationdate, lastlogindate, status) " +
                        "VALUES(@FirstName, @LastName, @Email, @Password, @Phone, @RegistrationDate, @LastLoginDate, @Status)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", model.FirstName);
                        command.Parameters.AddWithValue("@LastName", model.LastName);
                        command.Parameters.AddWithValue("@Email", model.Email);
                        command.Parameters.AddWithValue("@Password", model.Password);
                        command.Parameters.AddWithValue("@Phone", model.Phone);
                        command.Parameters.AddWithValue("@RegistrationDate", DateTime.Now);
                        command.Parameters.AddWithValue("@LastLoginDate", DateTime.Now);
                        command.Parameters.AddWithValue("@Status", (int)model.Status);
                        command.ExecuteNonQuery();
                    }

                    sql = "SELECT * FROM Users WHERE email=@Email";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", model.Email);
                        using (var reader = command.ExecuteReader())
                        {
                            UserModel? user = null;
                            while (reader.Read())
                            {
                                user = new UserModel(
                                    reader["FirstName"].ToString(),
                                    reader["LastName"].ToString(),
                                    reader["Email"].ToString(),
                                    reader["Phone"].ToString(),
                                    reader["Password"].ToString(),
                                    (UserStatus)Enum.ToObject(typeof(UserStatus), reader["Status"]),
                                    Convert.ToInt32(reader["Id"])
                                );
                            }
                            return new ResultModel(ResultStatus.Success, user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel Update(int id, UserModel model)
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
                        "email = @NewEmail, " +
                        "WHERE id = @Id;";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", model.FirstName);
                        command.Parameters.AddWithValue("@LastName", model.LastName);
                        command.Parameters.AddWithValue("@Password", model.Password);
                        command.Parameters.AddWithValue("@Phone", model.Phone);
                        command.Parameters.AddWithValue("@NewEmail", model.Email);
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@Status", (int)model.Status);
                        command.ExecuteNonQuery();
                    }

                    sql = "SELECT * FROM Users WHERE email=@Email";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", model.Email);
                        using (var reader = command.ExecuteReader())
                        {
                            UserModel? user = null;
                            while (reader.Read())
                            {
                                user = new UserModel(
                                    reader["FirstName"].ToString(),
                                    reader["LastName"].ToString(),
                                    reader["Email"].ToString(),
                                    reader["Phone"].ToString(),
                                    reader["Password"].ToString(),
                                    (UserStatus)Enum.ToObject(typeof(UserStatus), reader["Status"]),
                                    Convert.ToInt32(reader["Id"])
                                );
                            }
                            return new ResultModel(ResultStatus.Success, user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel Delete(int id)
        {
            try
            {

                using (var connection = GetOpenConnection())
                {
                    string sql = "DELETE FROM Users WHERE id = @Id;";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        command.ExecuteNonQuery();
                    }
                }
                return new ResultModel(ResultStatus.Success);
            }
            catch(Exception ex) 
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel Get(int id)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "SELECT * FROM Users WHERE id=@Id";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader =command.ExecuteReader())
                        {
                            UserModel user = null;
                            while (reader.Read())
                            {
                                if (reader != null) {
                                    user = new UserModel(
                                        reader["FirstName"].ToString(),
                                        reader["LastName"].ToString(),
                                        reader["Email"].ToString(),
                                        reader["Phone"].ToString(),
                                        reader["Password"].ToString(),
                                        (UserStatus)Enum.ToObject(typeof(UserStatus), reader["Status"]),
                                        id
                                    );
                                }
                            }
                            if (user == null)
                            {
                                return new ResultModel(ResultStatus.Error, "Can not found user with that token. Check out the token and try again!");
                            }
                            return new ResultModel(ResultStatus.Success, user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }
    }
}
