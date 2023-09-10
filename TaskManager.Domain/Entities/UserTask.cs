using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Domain.Entities
{
    public class UserTask
    {
        public User User { get; set; }
        public string UserId { get; set; }
        public Task Task { get; set; }
        public Guid TaskId { get; set; }
    }
}
