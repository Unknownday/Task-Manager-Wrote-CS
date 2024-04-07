using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class ProjectItems
    {
        public List<UserModel> Users { get; set; }
        public List<DeskModel> Desks { get; set; }

        public ProjectItems(List<UserModel> users, List<DeskModel> desks)
        {
            Users = users;
            Desks = desks;
        }
    }
}
