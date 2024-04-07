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
                    string sql = "INSERT INTO desk(administratorid, description, deskname, ispublic, avatar, creationdate, projectid) VALUES (@AdministratorID, @Description, @DeskName, @IsPublic, @Photo, @CreationDate, @ProjectId);";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@AdministratorID", model.AdminId);
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.AddWithValue("@DeskName", model.Name);
                        command.Parameters.AddWithValue("@IsPublic", model.IsPublic);
                        command.Parameters.AddWithValue("@Photo", model.Photo);
                        command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                        command.Parameters.AddWithValue("@ProjectId", model.ProjectId);
                        command.ExecuteNonQuery();
                    }

                    sql = "SELECT * FROM desk WHERE id = MAX(id)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DeskModel desk = new DeskModel()
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    AdminId = Convert.ToInt32(reader["administratorid"]),
                                    ProjectId = Convert.ToInt32(reader["projectid"]),
                                    Description = reader["description"].ToString(),
                                    Name = reader["deskname"].ToString(),
                                    IsPublic = bool.Parse(reader["ispublic"].ToString()),
                                    Photo = (byte[])reader["avatar"]
                                };

                                var columns = GetDeskColumns(desk.Id);
                                if (columns == null || columns.Status == ResultStatus.Error) { return columns; }
                                desk.Columns = (List<string>)columns.Result;

                                var tasks = GetDeskTasksIds(desk.Id);
                                if (tasks == null || tasks.Status == ResultStatus.Error) { return tasks; }
                                desk.TasksIds = (List<int>)tasks.Result;

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
                    var sql = "DELETE FROM desk WHERE id = @ID";
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
                    var sql = "SELECT * FROM desk WHERE id = @Id";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DeskModel desk = new DeskModel()
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    AdminId = Convert.ToInt32(reader["administratorid"]),
                                    ProjectId = id,
                                    Description = reader["description"].ToString(),
                                    Name = reader["deskname"].ToString(),
                                    IsPublic = bool.Parse(reader["ispublic"].ToString()),
                                    Photo = (byte[])reader["avatar"]
                                };

                                var columns = GetDeskColumns(desk.Id);
                                if (columns == null || columns.Status == ResultStatus.Error) { return columns; }
                                desk.Columns = (List<string>)columns.Result;

                                var tasks = GetDeskTasksIds(desk.Id);
                                if (tasks == null || tasks.Status == ResultStatus.Error) { return tasks; }
                                desk.TasksIds = (List<int>)tasks.Result;

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

        public ResultModel Update(int id, DeskModel model)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "UPDATE desk " +
                             "SET deskname = @DeskName, " +
                             "description = @Description, " +
                             "avatar = @Photo, " +
                             "ispublic = @IsPublic, " +
                             "administratorid = @AdministratorId, " +
                             "WHERE id = @Id;";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ProjectName", model.Name);
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.AddWithValue("@Photo", model.Photo);
                        command.Parameters.AddWithValue("@AdministratorId", model.AdminId);
                        command.Parameters.AddWithValue("@IsPublic", model.IsPublic);
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }
                var updatedDesk = Get(id);

                if (updatedDesk.Result != null) return new ResultModel(ResultStatus.Success, updatedDesk.Result);

                return new ResultModel(ResultStatus.Error, "Unable to find desk");
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        private ResultModel GetDeskColumns(int desk_id)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var columns = new List<string>();

                    string sql = "SELECT desk_column FROM deskcolumns WHERE desk_id = @DeskId";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DeskId", desk_id);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(reader["desk_column"].ToString());
                            }
                        }
                    }
                    return columns.IsNullOrEmpty() ? new ResultModel(ResultStatus.Error, "Unable to get columns data") : new ResultModel(ResultStatus.Success, columns);
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        private ResultModel GetDeskTasksIds(int desk_id)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var tasks = new List<int>();

                    string sql = "SELECT id FROM tasks INNER JOIN desktasks ON tasks.id = desktasks.task_id WHERE desktasks.desk_id = @DeskId";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DeskId", desk_id);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasks.Add(int.Parse(reader["id"].ToString()));
                            }
                        }
                    }
                    return tasks.IsNullOrEmpty() ? new ResultModel(ResultStatus.Error, "Unable to get tasks data") : new ResultModel(ResultStatus.Success, tasks);
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }
    }
}
