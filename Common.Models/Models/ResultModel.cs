using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class ResultModel
    {
        public string Message { get; set; } = null;
        public List<object> ResultArray { get; set; } = new List<object>();

        public object Result { get; set; }
        public ResultStatus Status { get; set; }

        public ResultModel(ResultStatus status, string message) 
        { 
            Message = message;
            Status = status;
        }

        public ResultModel(ResultStatus status, string message, object result) 
        { 
            Message = message;
            Result = result;
            Status = status;
        }

        public ResultModel(ResultStatus status, object result) 
        { 
            Result = result;
            Status = status;
        }

        public ResultModel(ResultStatus status)
        {
            Status = status;
        }
    }
}
