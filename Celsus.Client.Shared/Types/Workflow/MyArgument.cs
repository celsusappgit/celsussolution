using System;

namespace Celsus.Client.Shared.Types.Workflow
{
    public class MyArgument
    {
        public string Name { get; set; }
        public Type ArgumentType { get; set; }
        public bool IsOptional { get; set; }
        public string JSonValue { get; set; }
    }

    public class MyVariable
    {
        public string Name { get;  set; }
        public Type VariableType { get;  set; }
        public bool IsOptional { get;  set; }
        public string JSonValue { get;  set; }
    }

    public class MyOption
    {
        public string Name { get; set; }
        public Type OptionType { get; set; }
        public bool IsOptional { get; set; }
        public string JSonValue { get; set; }
    }
}