using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class DeskModel : CommonModel
    {
        public bool IsPublic { get; set; }
        public List<string> Columns { get; set; }
        public int ProjectId { get; set; }
        public int AdminId { get; set; }
        public List<int> TasksIds { get; set; }

        public DeskModel() { }

        public DeskModel(string name, int projectId, int administratorId, string description, bool isPrivate, byte[] photo = null, List<string> columns = null)
        {
            Name = name;
            ProjectId = projectId;
            AdminId = administratorId;
            Description = description;
            IsPublic = isPrivate;
            Photo = photo;
            Columns = columns;
        }
    }
}
