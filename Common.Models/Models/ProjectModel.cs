using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Common.Models
{
    public class ProjectModel : CommonModel
    {
        public int CreatorId { get; set; }
        public ProjectStatus Status { get; set; }

        public List<DeskModel> Desks { get; set; }

        public List<ShortUserModel> Users { get; set; }

        public ProjectModel() {
            
        }
    }
}
