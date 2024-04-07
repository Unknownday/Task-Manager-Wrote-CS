using Common.Models;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using TaskManager.API.Models.Global;

namespace TaskManager.API.Models.Services
{
    public class TaskService(IConfiguration configuration) : ICommonService<TaskModel>
    {
        private readonly IConfiguration _configuration = configuration;

        private NpgsqlConnection GetOpenConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public ResultModel Create(TaskModel model)
        {
            throw new NotImplementedException();
        }

        public ResultModel Delete(int id)
        {
            throw new NotImplementedException();
        }

        public ResultModel Get(int id)
        {
            throw new NotImplementedException();
        }

        public ResultModel Update(int id, TaskModel model)
        {
            throw new NotImplementedException();
        }

        private ResultModel GetDeskTasks(int desk_id)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var tasks = new List<TaskModel>();

                    string sql = "SELECT * FROM tasks INNER JOIN desktasks ON tasks.id = desktasks.task_id WHERE desktasks.desk_id = @DeskId";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DeskId", desk_id);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TaskModel task = new TaskModel()
                                {
                                    Id = int.Parse(reader["id"].ToString()),
                                    Name = reader["name"].ToString(),
                                    Description = reader["description"].ToString(),
                                    Column = reader["taskcolumn"].ToString(),
                                    StartDate = DateTime.Parse(reader["startdate"].ToString()),
                                    EndDate = DateTime.Parse(reader["enddate"].ToString()),
                                    CreationDate = DateTime.Parse(reader["creationdate"].ToString()),
                                    DeskId = desk_id,
                                    CreatorId = int.Parse(reader["creatorid"].ToString()),
                                    ExecutorId = int.Parse(reader["executorid"].ToString()),
                                    Photo = (byte[])reader["avatar"],
                                    File = (byte[])reader["taskfiles"]
                                };

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

        private ResultModel GetTaskColumns(int task_id)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var columns = new List<string>();

                    string sql = "SELECT task_column FROM taskcolumns WHERE task_id = @TaskId";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@TaskId", task_id);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(reader["task_column"].ToString());
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
    }
}
