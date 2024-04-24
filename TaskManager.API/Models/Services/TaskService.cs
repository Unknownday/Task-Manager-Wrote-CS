using Common.Models;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Threading.Tasks;
using TaskManager.API.Models.Global;
using TaskStatus = Common.Models.TaskStatus;

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
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var sql = "INSERT INTO Tasks (task_name, task_description, task_creation_date, task_photo, task_start_date, task_end_date, task_column, task_creator_id, task_executor_id, task_status) " +
                        "VALUES (@Name, @Description, @CreationDate, @Photo, @StartDate, @EndDate, @TaskColumn, @CreatorId, @ExecutorId, @Status)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", model.Name);
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                        command.Parameters.Add("@Photo", NpgsqlTypes.NpgsqlDbType.Bytea).Value = model.Photo;
                        command.Parameters.AddWithValue("@StartDate", model.StartDate);
                        command.Parameters.AddWithValue("@EndDate", model.EndDate);
                        command.Parameters.AddWithValue("@TaskColumn", model.Column);
                        command.Parameters.AddWithValue("@CreatorId", model.CreatorId);
                        command.Parameters.AddWithValue("@ExecutorId", model.ExecutorId);
                        command.Parameters.AddWithValue("@Status", 0);
                        command.ExecuteNonQuery();
                    }

                    sql = "SELECT * FROM Tasks WHERE task_id = currval('task_id_seq')";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TaskModel task = new TaskModel()
                                {
                                    Id = reader.GetFieldValue<int>(reader.GetOrdinal("task_id")),
                                    Name = reader.GetFieldValue<string>(reader.GetOrdinal("task_name")),
                                    Description = reader.GetFieldValue<string>(reader.GetOrdinal("task_description")),
                                    Column = reader.GetFieldValue<string>(reader.GetOrdinal("task_column")),
                                    StartDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("task_start_date")),
                                    EndDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("task_end_date")),
                                    CreationDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("task_creation_date")),
                                    CreatorId = reader.GetFieldValue<int>(reader.GetOrdinal("task_creator_id")),
                                    ExecutorId = reader.GetFieldValue<int>(reader.GetOrdinal("task_executor_id")),
                                    Photo = reader.GetFieldValue<byte[]>(reader.GetOrdinal("task_photo")),
                                    Status = (TaskStatus)Enum.ToObject(typeof(TaskStatus), reader.GetFieldValue<int>(reader.GetOrdinal("task_status"))),
                                };

                                return new ResultModel(ResultStatus.Success, task);
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
                    var sql = "DELETE FROM Tasks WHERE task_id = @TaskID";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@TaskID", id);
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

        /// <summary>
        /// Getting task from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Succes: new object of type 'ResultModel' with TaskModel as result. Failure: new object of type 'ResultModel' with ERROR status and error message</returns>
        public ResultModel Get(int id)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {

                    string sql = "SELECT * FROM Tasks WHERE task_id = @TaskID";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@TaskID", id);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TaskModel task = new TaskModel()
                                {
                                    Id = reader.GetFieldValue<int>(reader.GetOrdinal("task_id")),
                                    Name = reader.GetFieldValue<string>(reader.GetOrdinal("task_name")),
                                    Description = reader.GetFieldValue<string>(reader.GetOrdinal("task_description")),
                                    Column = reader.GetFieldValue<string>(reader.GetOrdinal("task_column")),
                                    StartDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("task_start_date")),
                                    EndDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("task_end_date")),
                                    CreationDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("task_creation_date")),
                                    CreatorId = reader.GetFieldValue<int>(reader.GetOrdinal("task_creator_id")),
                                    ExecutorId = reader.GetFieldValue<int>(reader.GetOrdinal("task_executor_id")),
                                    Photo = reader.GetFieldValue<byte[]>(reader.GetOrdinal("task_photo")),
                                    File = reader.GetFieldValue<byte[]>(reader.GetOrdinal("task_file")),
                                    Status = (TaskStatus)Enum.ToObject(typeof(TaskStatus), reader.GetFieldValue<int>(reader.GetOrdinal("task_status"))),
                                };

                                return task == null ? new ResultModel(ResultStatus.Error, "Unable to get task data") : new ResultModel(ResultStatus.Success, task);
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

        public ResultModel Patch(int id, TaskModel model)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "UPDATE Tasks " +
                             "SET task_name = @Name, " +
                             "task_description = @Description, " +
                             "task_phot = @Photo, " +
                             "task_end_date = @EndDate, " +
                             "task_column = @Column, " +
                             "task_file = @File, " +
                             "task_executor_id = @ExecutorId, " +
                             "task_status = @Status, " +
                             "WHERE task_id = @Id;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ProjectName", model.Name);
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.Add("@Photo", NpgsqlTypes.NpgsqlDbType.Bytea).Value = model.Photo;
                        command.Parameters.AddWithValue("@ExecutorId", model.ExecutorId);
                        command.Parameters.AddWithValue("@Column", model.Column);
                        command.Parameters.Add("@File", NpgsqlTypes.NpgsqlDbType.Bytea).Value = model.File;
                        command.Parameters.AddWithValue("@EndDate", model.EndDate);
                        command.Parameters.AddWithValue("@Status", model.Status);
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }
                var updatedTask = Get(id);

                if (updatedTask.Result != null)
                {
                    return new ResultModel(ResultStatus.Success, updatedTask.Result);
                }
                return new ResultModel(ResultStatus.Error, "Unable to find task");
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }
    }
}
