using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Domain.Entities
{
    public class UserTask
    {
        public Guid UserId { get; set; }
        public Guid TaskId { get; set; }
    }
}
