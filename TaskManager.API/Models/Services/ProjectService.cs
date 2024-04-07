using Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using TaskManager.API.Models.Global;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TaskManager.API.Models.Services
{
    public class ProjectService(IConfiguration configuration) : ICommonService<ProjectModel>
    {
        private readonly IConfiguration _configuration = configuration;

        private readonly DeskService _deskService = new DeskService(configuration);

        private NpgsqlConnection GetOpenConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public ResultModel Create(ProjectModel model)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "INSERT INTO Project(ProjectName, Decription, Creationdate, Avatar, AdminId, Status) VALUES (@ProjectName, @Description, @CreationDate, @Photo, @AdminId, @Status)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ProjectName", model.Name);
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.AddWithValue("@Photo", model.Photo);
                        command.Parameters.AddWithValue("@AdminId", model.AdministratorId);
                        command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                        command.Parameters.AddWithValue("@Status", (int)model.Status);
                        command.ExecuteNonQuery();
                    }

                    sql = "SELECT * FROM Project WHERE Id=MAX(ID)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            ProjectModel? project = null;
                            while (reader.Read())
                            {
                                project = new ProjectModel(
                                    Convert.ToInt32(reader["AdminId"]),
                                    reader["ProjectName"].ToString(),
                                    reader["Desctiption"].ToString(),
                                    DateTime.Parse(reader["Creationdate"].ToString()),
                                    (ProjectStatus)Enum.ToObject(typeof(ProjectStatus), reader["Status"])
                                );
                            }
                            return new ResultModel(ResultStatus.Success, project);
                        }
                    }
                }
            }
            catch
            {
                return new ResultModel(ResultStatus.Error, "An error");
            }
        }

        public ResultModel Delete(int id)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "DELETE FROM Project WHERE Id = @Id";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
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

        public ResultModel GetUserProjects(int id) 
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    List<int> user_projects = new List<int>();
                    List<ProjectModel> projects = new List<ProjectModel>();
                    string sql = "SELECT user_id FROM ProjectMember WHERE project_id = @Id";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                user_projects.Add(Convert.ToInt32(reader["user_id"].ToString()));
                            }
                        }
                    }

                    sql = "SELECT * FROM project WHERE id = @PID";
                    for (int i = 0; i < user_projects.Count(); i++)
                    {
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@PID", user_projects[i]);

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    ProjectModel project = new ProjectModel(
                                        Convert.ToInt32(reader["adminid"].ToString()),
                                        reader["projectname"].ToString(),
                                        reader["description"].ToString(),
                                        DateTime.Parse(reader["creationdate"].ToString()),
                                        (ProjectStatus)Enum.ToObject(typeof(UserStatus), Convert.ToUInt32(reader["Status"].ToString())),
                                        (byte[])reader["avatar"]
                                    );
                                    projects.Add(project);
                                }
                            }
                            return new ResultModel(ResultStatus.Success, projects);
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

        public ResultModel GetProjectDesks(int id)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    List<int> project_desks = new List<int>();
                    List<DeskModel> desks = new List<DeskModel>();
                    string sql = "SELECT desk_id FROM ProjectDesk WHERE project_id = @Id";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                project_desks.Add(Convert.ToInt32(reader["desk_id"].ToString()));
                            }
                        }
                    }

                    sql = "SELECT * FROM desk WHERE id = @PID";
                    for (int i = 0; i < project_desks.Count(); i++)
                    {
                        var deskGetResult = _deskService.Get(project_desks[i]);
                        if (deskGetResult.Status == ResultStatus.Success) desks.Add((DeskModel)deskGetResult.Result);
                    }
                    return new ResultModel(ResultStatus.Error);
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
                    string sql = "SELECT * FROM project WHERE id = @PID";
                    ProjectModel project = new ProjectModel();
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PID", id);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var itemsGetResult = GetProjectItemsIds(id);
                                if (itemsGetResult == null || itemsGetResult.Status == ResultStatus.Error) { return new ResultModel(ResultStatus.Error); }
                                project = new ProjectModel(
                                    Convert.ToInt32(reader["administratorid"].ToString()),
                                    reader["projectname"].ToString(),
                                    reader["description"].ToString(),
                                    DateTime.Parse(reader["creationdate"].ToString()),
                                    (ProjectStatus)Enum.ToObject(typeof(UserStatus), Convert.ToUInt32(reader["Status"].ToString())),
                                    (byte[])reader["avatar"]
                                ); 
                                var items = (Tuple<List<int>, List<int>>)itemsGetResult.Result;
                                project.DeskIds = items.Item1;
                                project.UserIds = items.Item2;
                                
                            }
                        }
                    }
                    return new ResultModel(ResultStatus.Success, project);
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel Update(int id, ProjectModel model)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "UPDATE project " +
                             "SET projectname = @ProjectName, " +
                             "description = @Description, " +
                             "avatar = @Photo, " +
                             "status = @Status, " +
                             "administratorid = @AdministratorId, " +
                             "WHERE id = @Id;";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ProjectName", model.Name);
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.AddWithValue("@Photo", model.Photo);
                        command.Parameters.AddWithValue("@AdministratorId", model.AdministratorId);
                        command.Parameters.AddWithValue("@Status", (int)model.Status);
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }
                var updatedProject = Get(id);
                if (updatedProject.Result != null)
                {
                    return new ResultModel(ResultStatus.Success, updatedProject.Result);
                }
                return new ResultModel(ResultStatus.Error, "Unable to find project");
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        private ResultModel GetProjectItemsIds(int projectId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    var desksIds = new List<int>();
                    var usersIds = new List<int>();

                    string sql = "SELECT desk_id, user_id FROM projectdesks JOIN projectmembers ON projectdesks.project_id = projectmembers.project_id WHERE projectdesks.project_id = @ProjectID;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PProjectIDrojectID", projectId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                desksIds.Add(int.Parse(reader["desk_id"].ToString()));
                                usersIds.Add(int.Parse(reader["user_id"].ToString()));
                            }
                        }
                    }
                    return new ResultModel(ResultStatus.Success, new Tuple<List<int>, List<int>>(desksIds, usersIds));
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }
    }
}
