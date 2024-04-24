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
                string sqlCommandBase = "SELECT * FROM Users WHERE user_email = @Email AND user_password = @Password";
                using (var command = new NpgsqlCommand(sqlCommandBase, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        UserModel user = new UserModel();
                        while (reader.Read())
                        {
                            user = new UserModel()
                            {
                                Id = reader.GetFieldValue<int>(reader.GetOrdinal("user_id")),
                                FirstName = reader.GetFieldValue<string>(reader.GetOrdinal("user_firstname")),
                                Surname = reader.GetFieldValue<string>(reader.GetOrdinal("user_surname")),
                                Nickname = reader.GetFieldValue<string>(reader.GetOrdinal("user_nickname")),
                                Email = reader.GetFieldValue<string>(reader.GetOrdinal("user_email")),
                                Password = reader.GetFieldValue<string>(reader.GetOrdinal("user_password")),
                                Phone = reader.GetFieldValue<string>(reader.GetOrdinal("user_phone")),
                                Description = reader.GetFieldValue<string>(reader.GetOrdinal("user_profile_description")),
                                Status = (UserStatus)Enum.ToObject(typeof(UserStatus), reader.GetFieldValue<int>(reader.GetOrdinal("user_status"))),
                                RegistrationDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("user_registration_date")),
                                LastLoginDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("user_last_login_date")),
                                Photo = reader.GetFieldValue<byte[]>(reader.GetOrdinal("user_profile_photo")),
                            };
                            Console.WriteLine($"{user.FirstName}, {user.Surname}, {user.Email}, {user.Status}, {user.Id}, {user.Password}, {user.Phone}");
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
                string sqlCommandBase = "SELECT user_refresh_token FROM Users WHERE user_id = @Id";
                using (var command = new NpgsqlCommand(sqlCommandBase, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        string? refreshToken = "";
                        while (reader.Read())
                        {
                            refreshToken = reader["user_refresh_token"].ToString();
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
                        "SET user_firstname = @Firstname, " +
                        "user_surname = @Surname, " +
                        "user_nickname = @Nickname, " +
                        "user_password = @Password, " +
                        "user_phone = @Phone, " +
                        "user_status = @Status, " +
                        "user_email = @Email, " +
                        "user_profile_photo = @Photo, " +
                        "user_profile_description = @Description, " +
                        "user_last_login_date = @LoginDate " +
                        "WHERE user_email = @Email;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Firstname", userModel.FirstName);
                        command.Parameters.AddWithValue("@Surname", userModel.Surname);
                        command.Parameters.AddWithValue("@Nickname", userModel.Nickname);
                        command.Parameters.AddWithValue("@Phone", userModel.Phone);
                        command.Parameters.AddWithValue("@Password", userModel.Password);
                        command.Parameters.AddWithValue("@Email", userModel.Email);
                        command.Parameters.Add("@Photo", NpgsqlTypes.NpgsqlDbType.Bytea).Value = userModel.Photo;
                        command.Parameters.AddWithValue("@Description", userModel.Description);
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

                    SaveTokens(userId, newRefreshToken);

                    return (true, userId);
                }

                return (true, userId);
            }
            catch (Exception)
            {
                return (false, -1);
            }
        }

        public async void SaveTokens(int userId, string refreshtoken)
        {
            using (var connection = GetOpenConnection())
            {
                string sqlCommandBase = "SELECT user_id FROM Users WHERE user_id = @Id";
                using (var command = new NpgsqlCommand(sqlCommandBase, connection))
                {
                    int id = -1;
                    command.Parameters.AddWithValue("@Id", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {

                        while (reader.Read())
                        {
                            if (int.TryParse(reader["user_id"].ToString(), out userId))
                            {
                                id = userId; 
                                break;
                            }
                        }
                    }
                    sqlCommandBase = "UPDATE Users SET user_refresh_token = @Token WHERE user_id = @Id";
                    using (var update = new NpgsqlCommand(sqlCommandBase, connection))
                    {
                        update.Parameters.AddWithValue("@Token", refreshtoken);
                        update.Parameters.AddWithValue("@Id", userId);
                        await update.ExecuteNonQueryAsync();
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
                    string sql = "INSERT INTO Users (user_firstname, user_surname, user_nickname, user_profile_description, user_email, user_password, user_phone, user_registration_date, user_last_login_date, user_status, user_profile_photo) " +
                        "VALUES (@Firstname, @Surname, @Nickname, @Description, @Email, @Password, @Phone, @RegistrationDate, @LastLoginDate, @Status, @Photo)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Firstname", model.FirstName);
                        command.Parameters.AddWithValue("@Surname", model.Surname);
                        command.Parameters.AddWithValue("@Nickname", model.Nickname);
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.AddWithValue("@Email", model.Email);
                        command.Parameters.AddWithValue("@Password", model.Password);
                        command.Parameters.AddWithValue("@Phone", model.Phone);
                        command.Parameters.AddWithValue("@RegistrationDate", DateTime.Now);
                        command.Parameters.AddWithValue("@LastLoginDate", DateTime.Now);
                        command.Parameters.AddWithValue("@Status", (int)model.Status);
                        command.Parameters.Add("@Photo", NpgsqlTypes.NpgsqlDbType.Bytea).Value = model.Photo;
                        command.ExecuteNonQuery();
                    }

                    sql = "SELECT * FROM Users WHERE user_email=@Email";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", model.Email);
                        using (var reader = command.ExecuteReader())
                        {
                            UserModel? user = null;
                            while (reader.Read())
                            {
                                user = new UserModel()
                                {
                                    Id = reader.GetFieldValue<int>(reader.GetOrdinal("user_id")),
                                    FirstName = reader.GetFieldValue<string>(reader.GetOrdinal("user_firstname")),
                                    Surname = reader.GetFieldValue<string>(reader.GetOrdinal("user_surname")),
                                    Nickname = reader.GetFieldValue<string>(reader.GetOrdinal("user_nickname")),
                                    Email = reader.GetFieldValue<string>(reader.GetOrdinal("user_email")),
                                    Password = reader.GetFieldValue<string>(reader.GetOrdinal("user_password")),
                                    Phone = reader.GetFieldValue<string>(reader.GetOrdinal("user_phone")),
                                    Description = reader.GetFieldValue<string>(reader.GetOrdinal("user_profile_description")),
                                    Status = (UserStatus)Enum.ToObject(typeof(UserStatus), reader.GetFieldValue<int>(reader.GetOrdinal("user_status"))),
                                    RegistrationDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("user_registration_date")),
                                    LastLoginDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("user_last_login_date")),
                                    Photo = reader.GetFieldValue<byte[]>(reader.GetOrdinal("user_profile_photo")),
                                };
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

        public ResultModel Patch(int id, UserModel model)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "UPDATE Users " +
                     "SET user_firstname = @Firstname, " +
                     "user_surname = @Surname, " +
                     "user_nickname = @Nickname, " +
                     "user_password = @Password, " +
                     "user_phone = @Phone, " +
                     "user_status = @Status, " +
                     "user_email = @Email, " +
                     "user_profile_photo = @Photo, " +
                     "user_profile_description = @Description, " +
                     "user_last_login_date = @LoginDate " +
                     "WHERE user_email = @Email;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Firstname", model.FirstName);
                        command.Parameters.AddWithValue("@Surname", model.Surname);
                        command.Parameters.AddWithValue("@Nickname", model.Nickname);
                        command.Parameters.AddWithValue("@Phone", model.Phone);
                        command.Parameters.AddWithValue("@Password", model.Password);
                        command.Parameters.AddWithValue("@Email", model.Email);
                        command.Parameters.Add("@Photo", NpgsqlTypes.NpgsqlDbType.Bytea).Value = model.Photo;
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.AddWithValue("@Email", model.Email);
                        command.Parameters.AddWithValue("@LoginDate", model.LastLoginDate);
                        command.Parameters.AddWithValue("@Status", (int)model.Status);
                        command.ExecuteNonQuery();
                    }

                    sql = "SELECT * FROM Users WHERE user_email = @Email";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", model.Email);
                        using (var reader = command.ExecuteReader())
                        {
                            UserModel? user = null;
                            while (reader.Read())
                            {
                                user = new UserModel()
                                {
                                    Id = reader.GetFieldValue<int>(reader.GetOrdinal("user_id")),
                                    FirstName = reader.GetFieldValue<string>(reader.GetOrdinal("user_firstname")),
                                    Surname = reader.GetFieldValue<string>(reader.GetOrdinal("user_surname")),
                                    Nickname = reader.GetFieldValue<string>(reader.GetOrdinal("user_nickname")),
                                    Email = reader.GetFieldValue<string>(reader.GetOrdinal("user_email")),
                                    Password = reader.GetFieldValue<string>(reader.GetOrdinal("user_password")),
                                    Phone = reader.GetFieldValue<string>(reader.GetOrdinal("user_phone")),
                                    Description = reader.GetFieldValue<string>(reader.GetOrdinal("user_profile_description")),
                                    Status = (UserStatus)Enum.ToObject(typeof(UserStatus), reader.GetFieldValue<int>(reader.GetOrdinal("user_status"))),
                                    RegistrationDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("user_registration_date")),
                                    LastLoginDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("user_last_login_date")),
                                    Photo = reader.GetFieldValue<byte[]>(reader.GetOrdinal("user_profile_photo")),
                                };
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
                    string sql = "DELETE FROM Users WHERE user_id = @Id;";
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
                    string sql = "SELECT * FROM Users WHERE user_id = @Id";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            UserModel user = null;
                            while (reader.Read())
                            {
                                if (reader != null)
                                {
                                    user = new UserModel()
                                    {
                                        Id = reader.GetFieldValue<int>(reader.GetOrdinal("user_id")),
                                        FirstName = reader.GetFieldValue<string>(reader.GetOrdinal("user_firstname")),
                                        Surname = reader.GetFieldValue<string>(reader.GetOrdinal("user_surname")),
                                        Nickname = reader.GetFieldValue<string>(reader.GetOrdinal("user_nickname")),
                                        Email = reader.GetFieldValue<string>(reader.GetOrdinal("user_email")),
                                        Password = reader.GetFieldValue<string>(reader.GetOrdinal("user_password")),
                                        Phone = reader.GetFieldValue<string>(reader.GetOrdinal("user_phone")),
                                        Description = reader.GetFieldValue<string>(reader.GetOrdinal("user_profile_description")),
                                        Status = (UserStatus)Enum.ToObject(typeof(UserStatus), reader.GetFieldValue<int>(reader.GetOrdinal("user_status"))),
                                        RegistrationDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("user_registration_date")),
                                        LastLoginDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("user_last_login_date")),
                                        Photo = reader.GetFieldValue<byte[]>(reader.GetOrdinal("user_profile_photo")),
                                    };
                                    
                                }
                                if (user == null)
                                {
                                    return new ResultModel(ResultStatus.Error, "Can not found user with that token. Check out the token and try again!");
                                }
                                return new ResultModel(ResultStatus.Success, user);
                            }
                            return new ResultModel(ResultStatus.Error);
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
