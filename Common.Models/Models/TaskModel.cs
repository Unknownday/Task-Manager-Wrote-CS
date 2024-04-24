using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class TaskModel : CommonModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public byte[] File { get; set; }
        public string Column { get; set; }
        public int CreatorId { get; set; }
        public int ExecutorId { get; set; }
        public TaskStatus Status { get; set; }

        public TaskModel() { }
        public TaskModel(string name, string description, DateTime start, DateTime end, string column, int creatorId, int executorid = -1, byte[] file = null, byte[] photo = null)
        {
            Name = name;
            Description = description;
            StartDate = start;
            EndDate = end;
            Column = column;
            CreatorId = creatorId;
            ExecutorId = executorid;
            File = file;
            Photo = photo;
        }
    }
}
