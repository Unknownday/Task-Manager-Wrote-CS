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

        private readonly UserService _userService = new UserService(configuration);

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
                    string sql = "INSERT INTO Projects (project_name, project_description, project_creation_date, project_photo, project_creator_id, project_status) VALUES (@Name, @Description, @CreationDate, @Photo, @CreatorId, @Status)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", model.Name);
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.Add("@Photo", NpgsqlTypes.NpgsqlDbType.Bytea).Value = model.Photo;
                        command.Parameters.AddWithValue("@CreatorId", model.CreatorId);
                        command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                        command.Parameters.AddWithValue("@Status", (int)model.Status);
                        command.ExecuteNonQuery();
                    }

                    sql = "SELECT * FROM Projects WHERE project_id = currval('project_id_seq')";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            ProjectModel? project = null;
                            while (reader.Read())
                            {
                                project = new ProjectModel()
                                {
                                    Id = reader.GetFieldValue<int>(reader.GetOrdinal("project_id")),
                                    CreatorId = reader.GetFieldValue<int>(reader.GetOrdinal("project_creator_id")),
                                    Name = reader.GetFieldValue<string>(reader.GetOrdinal("project_name")),
                                    Description = reader.GetFieldValue<string>(reader.GetOrdinal("project_description")),
                                    CreationDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("project_creation_date")),
                                    Photo = reader.GetFieldValue<byte[]>(reader.GetOrdinal("project_photo")),
                                    Status = (ProjectStatus)Enum.ToObject(typeof(ProjectStatus), reader.GetFieldValue<int>(reader.GetOrdinal("project_status")))
                                };
                            }
                            return new ResultModel(ResultStatus.Success, project);
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
                    string sql = "DELETE FROM Projects WHERE project_id = @Id";
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
                    string sql = "SELECT project_id FROM ProjectMembers WHERE user_id = @Id";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                user_projects.Add(reader.GetFieldValue<int>(reader.GetOrdinal("project_id")));
                            }
                        }
                    }

                    sql = "SELECT * FROM Projects WHERE project_id = @PID";
                    for (int i = 0; i < user_projects.Count(); i++)
                    {
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@PID", user_projects[i]);

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    ProjectModel project = new ProjectModel()
                                    {
                                        Id = reader.GetFieldValue<int>(reader.GetOrdinal("project_id")),
                                        CreatorId = reader.GetFieldValue<int>(reader.GetOrdinal("project_creator_id")),
                                        Name = reader.GetFieldValue<string>(reader.GetOrdinal("project_name")),
                                        Description = reader.GetFieldValue<string>(reader.GetOrdinal("project_description")),
                                        CreationDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("project_creation_date")),
                                        Photo = reader.GetFieldValue<byte[]>(reader.GetOrdinal("project_photo")),
                                        Status = (ProjectStatus)Enum.ToObject(typeof(ProjectStatus), reader.GetFieldValue<int>(reader.GetOrdinal("project_status")))
                                    };
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

        public ResultModel Get(int id)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "SELECT * FROM projects WHERE project_id = @PID";
                    ProjectModel project = new ProjectModel();
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PID", id);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                project = new ProjectModel()
                                {
                                    Id = reader.GetFieldValue<int>(reader.GetOrdinal("project_id")),
                                    CreatorId = reader.GetFieldValue<int>(reader.GetOrdinal("project_creator_id")),
                                    Name = reader.GetFieldValue<string>(reader.GetOrdinal("project_name")),
                                    Description = reader.GetFieldValue<string>(reader.GetOrdinal("project_description")),
                                    CreationDate = reader.GetFieldValue<DateTime>(reader.GetOrdinal("project_creation_date")),
                                    Photo = reader.GetFieldValue<byte[]>(reader.GetOrdinal("project_photo")),
                                    Status = (ProjectStatus)Enum.ToObject(typeof(ProjectStatus), reader.GetFieldValue<int>(reader.GetOrdinal("project_status")))
                                };

                                var getDesksResult = GetProjectDesks(id);

                                if (getDesksResult.Status != ResultStatus.Error) project.Desks = (List<DeskModel>)getDesksResult.Result;

                                var getUsersResult = GetProjectMembers(id);

                                if (getUsersResult.Status != ResultStatus.Error) project.Users = (List<ShortUserModel>)getUsersResult.Result;

                                if (getUsersResult.Status == ResultStatus.Error) Console.WriteLine(getUsersResult.Message); 
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

        public ResultModel Patch(int id, ProjectModel model)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "UPDATE Projects " +
                             "SET project_name = @Name, " +
                             "project_description = @Description, " +
                             "project_photo = @Photo, " +
                             "project_status = @Status, " +
                             "project_creator_id = @AdministratorId, " +
                             "WHERE project_id = @Id;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", model.Name);
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.Add("@Photo", NpgsqlTypes.NpgsqlDbType.Bytea).Value = model.Photo;
                        command.Parameters.AddWithValue("@AdministratorId", model.CreatorId);
                        command.Parameters.AddWithValue("@Status", (int)model.Status);
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }
                var updatedProject = Get(id);

                if (updatedProject.Result != null) return new ResultModel(ResultStatus.Success, updatedProject.Result);  

                return new ResultModel(ResultStatus.Error, "Unable to find project");
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel AddUserToProject(int userId, int projectId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "INSERT INTO ProjectMembers(user_id, project_id) VALUES (@UID, @PID)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@UID", userId);
                        command.Parameters.AddWithValue("PID", projectId);
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

        public ResultModel DeleteUserFromProject(int userId, int projectId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "DELETE FROM ProjectMembers WHERE user_id = @UID AND project_id = @PID)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@UID", userId);
                        command.Parameters.AddWithValue("PID", projectId);
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

        public ResultModel AddDeskToProject(int deskId, int projectId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "INSERT INTO ProjectDesks(desk_id, project_id) VALUES (@DID, @PID)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DID", deskId);
                        command.Parameters.AddWithValue("@PID", projectId);
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

        public ResultModel DeletDeskFromProject(int deskId, int projectId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "DELETE FROM ProjectDesks WHERE desk_id = @DID AND project_id = @PID";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DID", deskId);
                        command.Parameters.AddWithValue("@PID", projectId);
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

        public bool IsOwner(int userId, int projectId)
        {
            var project = Get(projectId).Result;

            if (project == null) { return false; }

            return ((ProjectModel)project).CreatorId == userId ? true : false;

        }

        private ResultModel GetProjectMembers(int projectId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "SELECT user_id FROM ProjectMembers WHERE project_id = @PID";
                    List<int> users_ids = new List<int>();
                    using (var command = new NpgsqlCommand(sql,connection))
                    {
                        command.Parameters.AddWithValue("@PID", projectId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users_ids.Add(reader.GetFieldValue<int>(reader.GetOrdinal("user_id")));
                            }
                        }
                    }

                    List<ShortUserModel> users = new List<ShortUserModel>();
                    foreach (int user in users_ids)
                    {
                        var getUserResult = _userService.Get(user);

                        Console.WriteLine(user);

                        if (getUserResult == null) { continue; }

                        if (getUserResult.Status == ResultStatus.Error) { continue; }

                        users.Add(new ShortUserModel((UserModel)getUserResult.Result));
                    }

                    return new ResultModel(ResultStatus.Success, users);
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }

        public ResultModel GetProjectDesks(int projectId)
        {
            try
            {
                using (var connection = GetOpenConnection())
                {
                    string sql = "SELECT desk_id FROM ProjectDesks WHERE project_id = @PID";
                    List<int> desk_ids = new List<int>();
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PID", projectId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                desk_ids.Add(reader.GetFieldValue<int>(reader.GetOrdinal("user_id")));
                            }
                        }
                    }

                    List<DeskModel> desks = new List<DeskModel>();
                    foreach (int desk in desk_ids)
                    {
                        var getUserResult = _deskService.Get(desk);

                        if (getUserResult == null) { continue; }

                        if (getUserResult.Status == ResultStatus.Error) { continue; }

                        desks.Add((DeskModel)getUserResult.Result);
                    }

                    return new ResultModel(ResultStatus.Success, desks);
                }
            }
            catch (Exception ex)
            {
                return new ResultModel(ResultStatus.Error, ex.Message);
            }
        }
    }
}
