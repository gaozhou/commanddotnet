using System;
using System.Collections.Generic;

namespace CommandDotNet.Tests.BddTests.Models
{
    /// <summary>To capture outputs for args from params tests</summary>
    public class ParametersSampleTypesResults : ISampleTypesArgumentsModel
    {
        public string StringArg { get; set; }
        public int StructArg { get; set; }
        public int? StructNArg { get; set; }
        public DayOfWeek EnumArg { get; set; }
        public Uri ObjectArg { get; set; }
        public List<string> StringListArg { get; set; }
        public List<int> StructListArg { get; set; }
        public List<DayOfWeek> EnumListArg { get; set; }
        public List<Uri> ObjectListArg { get; set; }

        public ParametersSampleTypesResults()
        {
        }

        public ParametersSampleTypesResults(
            string stringArg, int structArg, int? structNArg, 
            DayOfWeek enumArg, Uri objectArg, List<string> stringListArg)
        {
            StringArg = stringArg;
            StructArg = structArg;
            StructNArg = structNArg;
            EnumArg = enumArg;
            ObjectArg = objectArg;
            StringListArg = stringListArg;
        }

        public ParametersSampleTypesResults(List<int> structListArg)
        {
            StructListArg = structListArg;
        }

        public ParametersSampleTypesResults(List<DayOfWeek> enumListArg)
        {
            EnumListArg = enumListArg;
        }

        public ParametersSampleTypesResults(List<Uri> objectListArg)
        {
            ObjectListArg = objectListArg;
        }
    }
}