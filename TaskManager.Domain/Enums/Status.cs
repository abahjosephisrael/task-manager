using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Domain.Enums
{
    public enum Status
    {
        Pending = 1,
        [Description("In-Progess")]
        InProgress,
        Completed
    }
}
