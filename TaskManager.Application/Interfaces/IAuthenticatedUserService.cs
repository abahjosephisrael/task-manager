﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Application.Interfaces
{
    public interface IAuthenticatedUserService
    {
        string UserId { get; }
    }
}
