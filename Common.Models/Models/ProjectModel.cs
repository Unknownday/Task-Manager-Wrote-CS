using System;
using System.Collections.Generic;

namespace Common.Models
{
    public class ProjectModel : CommonModel
    {
        public int AdministratorId { get; set; }
        public List<int> UserIds { get; set; }
        public List<int> DeskIds { get; set; }
        public ProjectStatus Status { get; set; }

        public ProjectModel() { }

        public ProjectModel(int adminId, string name, string description, DateTime creationDate, ProjectStatus status, byte[] photo = null) 
        { 
            AdministratorId = adminId;
            Name = name;
            Description = description;
            CreationDate = creationDate;
            Photo = photo;
            Status = status;
        }
    }
}
