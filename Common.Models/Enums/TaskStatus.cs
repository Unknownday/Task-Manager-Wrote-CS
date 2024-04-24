using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public enum TaskStatus
    {
        Completed = 0,
        InProgress = 1,
        Expired = 2,
        Failed = 3,
        Suspended = 4
    }
}
