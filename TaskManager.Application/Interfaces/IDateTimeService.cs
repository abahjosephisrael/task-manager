using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Application.Interfaces
{
    public interface IDateTimeService
    {
        DateTime CurrentUtc { get; }
    }
}
