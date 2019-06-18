using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sportsstop.Models
{
    public class ResponseObject
    {
        public bool Status { get; set; }
        public String Message { get; set; }
        public ICollection<object> Data { get; set; }

        public ResponseObject(bool status, String message, ICollection<object> data = null)
        {
            this.Status = status;
            this.Message = message;
            this.Data = data;
        }

        public ResponseObject() { }

        public void SetContent(bool status, String message, List<object> data = null)
        {
            this.Status = status;
            this.Message = message;
            this.Data = data;
        }
    }
}
