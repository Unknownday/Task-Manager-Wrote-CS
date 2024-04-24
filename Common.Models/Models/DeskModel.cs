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
        public List<DeskColumnModel> Columns { get; set; }
        public int AdministratorId { get; set; }
        public List<TaskModel> Tasks { get; set; }

        public DeskModel() { }

        public DeskModel(string name, int administratorId, string description, bool isPrivate, byte[] photo = null, List<DeskColumnModel> columns = null)
        {
            Name = name;
            AdministratorId = administratorId;
            Description = description;
            IsPublic = isPrivate;
            Photo = photo;
            Columns = columns;
        }
    }
}
