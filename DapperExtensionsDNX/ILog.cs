﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperExtensions
{
    public interface ILog
    {
		void Log(string logEntry, Exception ex = null);
    }
}
