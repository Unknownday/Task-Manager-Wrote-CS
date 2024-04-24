using Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using TaskManager.API.Models.Global;

namespace TaskManager.API.Models.Services
{
    public class DeskService(IConfiguration configuration) : ICommonService<DeskModel>
    {
        private readonly IConfiguration _configuration = configuration;

        private readonly TaskService _taskService = new TaskService(configuration);

        private NpgsqlConnection GetOpenConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public ResultModel Create(DeskModel model)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "INSERT INTO Desks (desk_name, desk_description, desk_administrator_id, desk_is_public, desk_photo, desk_creation_date) VALUES (@Name, @Description, @AdministratorId, @IsPublic, @Photo, @CreationDate);";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", model.Name);
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.AddWithValue("@AdministratorId", model.AdministratorId);
                        command.Parameters.AddWithValue("@IsPublic", model.IsPublic);
                        command.Parameters.Add("@Photo", NpgsqlTypes.NpgsqlDbType.Bytea).Value = model.Photo;
                        command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                        command.ExecuteNonQuery();
                    }

                    sql = "SELECT * FROM Desks WHERE desk_id = currval('desk_id_seq')";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DeskModel desk = new DeskModel()
                                {
                                    Id = reader.GetFieldValue<int>(reader.GetOrdinal("desk_id")),
                                    CreationDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("desk_creation_date")),
                                    AdministratorId = reader.GetFieldValue<int>(reader.GetOrdinal("desk_administrator_id")),
                                    Description = reader.GetFieldValue<string>(reader.GetOrdinal("desk_description")),
                                    Name = reader.GetFieldValue<string>(reader.GetOrdinal("desk_name")),
                                    IsPublic = reader.GetFieldValue<bool>(reader.GetOrdinal("desk_is_public")),
                                    Photo = reader.GetFieldValue<byte[]>(reader.GetOrdinal("desk_photo"))
                                };

                                return new ResultModel(ResultStatus.Success, desk);
                            }
                        }

                    }

                    return new ResultModel(ResultStatus.Error);
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
                    var sql = "DELETE FROM Desks WHERE desk_id = @ID";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ID", id);
                        command.ExecuteNonQuery();
                    }
                    return new ResultModel(ResultStatus.Success);
                }
            }
            catch (Exception ex)
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
                    var sql = "SELECT * FROM Desks WHERE desk_id = @Id";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DeskModel desk = new DeskModel()
                                {
                                    Id = reader.GetFieldValue<int>(reader.GetOrdinal("desk_id")),
                                    CreationDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("desk_creation_date")),
                                    AdministratorId = reader.GetFieldValue<int>(reader.GetOrdinal("desk_administrator_id")),
                                    Description = reader.GetFieldValue<string>(reader.GetOrdinal("desk_description")),
                                    Name = reader.GetFieldValue<string>(reader.GetOrdinal("desk_name")),
                                    IsPublic = reader.GetFieldValue<bool>(reader.GetOrdinal("desk_is_public")),
                                    Photo = reader.GetFieldValue<byte[]>(reader.GetOrdinal("desk_photo"))
                                };

                                var columns = GetDeskColumns(desk.Id);
                                desk.Columns = (List<DeskColumnModel>)columns.Result;

                                var tasks = GetDeskTasks(desk.Id);
                                desk.Tasks = (List<TaskModel>)tasks.Result;

                                return new ResultModel(ResultStatus.Success, desk);
                            }
                        }
                        return new ResultModel(ResultStatus.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel Patch(int id, DeskModel model)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "UPDATE Desks " +
                             "SET desk_name = @Name, " +
                             "desk_description = @Description, " +
                             "desk_photo = @Photo, " +
                             "desk_is_public = @IsPublic, " +
                             "desk_administrator_id = @AdministratorId, " +
                             "WHERE desk_id = @Id;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", model.Name);
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.AddWithValue("@Photo", model.Photo);
                        command.Parameters.AddWithValue("@AdministratorId", model.AdministratorId);
                        command.Parameters.AddWithValue("@IsPublic", model.IsPublic);
                        command.Parameters.AddWithValue("@Id", model.Id);
                        command.ExecuteNonQuery();
                    }
                }
                var updatedDesk = Get(model.Id);

                if (updatedDesk.Result != null) return new ResultModel(ResultStatus.Success, updatedDesk.Result);

                return new ResultModel(ResultStatus.Error, "Unable to find desk");
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        private ResultModel GetDeskColumns(int deskId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var columns = new List<DeskColumnModel>();

                    string sql = "SELECT desk_column FROM DeskColumns WHERE desk_id = @DeskId";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DeskId", deskId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var column = new DeskColumnModel()
                                {
                                    DeskId = reader.GetFieldValue<int>(reader.GetOrdinal("desk_id")),
                                    Id = reader.GetFieldValue<int>(reader.GetOrdinal("desk_column_id")),
                                    Value = reader.GetFieldValue<string>(reader.GetOrdinal("desk_column"))
                                };
                                columns.Add(column);
                            }
                        }
                    }
                    return new ResultModel(ResultStatus.Success, columns);
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        private ResultModel GetDeskTasks(int deskId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var tasksIds = new List<int>();

                    string sql = "SELECT task_id FROM DeskTasks WHERE desk_id = @Id";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", deskId);    

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasksIds.Add(reader.GetFieldValue<int>(reader.GetOrdinal("task_id")));
                            }
                        }
                    }
                    var tasks = new List<TaskModel>();

                    for (int i = 0; i < tasksIds.Count; i++)
                    {
                        var getResult = _taskService.Get(tasksIds[i]);

                        if (getResult.Status == ResultStatus.Error) continue;

                        tasks.Add((TaskModel)getResult.Result);
                    }

                    return new ResultModel(ResultStatus.Success, tasksIds);
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel AddColumnToDesk(string value, int deskId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var sql = "INSERT INTO DeskColumns(desk_id, desk_column) VALUES (@DID, @Column)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DID", deskId);
                        command.Parameters.AddWithValue("@Column", value);
                        command.ExecuteNonQuery();
                        sql = "SELECT * FROM DeskColumns WHERE desk_column_id = curval(desk_column_id_seq)";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var column = new DeskColumnModel()
                                {
                                    DeskId = reader.GetFieldValue<int>(reader.GetOrdinal("desk_id")),
                                    Id = reader.GetFieldValue<int>(reader.GetOrdinal("desk_column_id")),
                                    Value = reader.GetFieldValue<string>(reader.GetOrdinal("desk_column"))
                                };

                                return new ResultModel(ResultStatus.Success, column);
                            }
                        }
                        return new ResultModel(ResultStatus.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel DeleteColumnFromDesk(int columnId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var sql = "DELETE FROM DeskColumns WHERE desk_column_id = @CID";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CID", columnId);
                        command.ExecuteNonQuery();

                        return new ResultModel(ResultStatus.Success);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel PatchDeskColumn(DeskColumnModel model)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var sql = "UPDATE DeskColumns SET desk_column = @Column WHERE desk_column_id = @CID";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Column", model.Value);
                        command.Parameters.AddWithValue("@CID", model.Id);
                        command.ExecuteNonQuery();
                        return new ResultModel(ResultStatus.Success, GetDeskColumn(model.Id).Result);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel GetDeskColumn(int columnId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var sql = "SELECT * FROM DeskColumns WHERE desk_column_id = @CID";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CID", columnId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var column = new DeskColumnModel()
                                {
                                    DeskId = reader.GetFieldValue<int>(reader.GetOrdinal("desk_id")),
                                    Id = reader.GetFieldValue<int>(reader.GetOrdinal("desk_column_id")),
                                    Value = reader.GetFieldValue<string>(reader.GetOrdinal("desk_column"))
                                };

                                return new ResultModel(ResultStatus.Success, column);
                            }
                        }
                        return new ResultModel(ResultStatus.Error);
                    }
                }               
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel AddTaskToDesk(int deskId,  int taskId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var sql = "INSERT INTO DeskTasks(desk_id, task_id) VALUES (@DID, @TID)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DID", deskId);
                        command.Parameters.AddWithValue("@TID", taskId);
                        command.ExecuteNonQuery();
                        return new ResultModel(ResultStatus.Success);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel DeleteTaskFromDesk(int taskId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "DELETE FROM DeskTasks WHERE task_id = @TID";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@TID", taskId);
                        command.ExecuteNonQuery();
                        return new ResultModel(ResultStatus.Success);
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
