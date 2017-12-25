﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommandDotNet.Models;

namespace CommandDotNet
{
    public static class AppInstanceCreator
    {                
        public static object CreateInstance(Type type, IEnumerable<ArgumentInfo> construcitonParams)
        {
            construcitonParams = construcitonParams ?? new List<ArgumentInfo>();
            
            object[] values = construcitonParams.Select(ValueMachine.GetValue).ToArray();   
            return Activator.CreateInstance(type, values);
        }
    }
}