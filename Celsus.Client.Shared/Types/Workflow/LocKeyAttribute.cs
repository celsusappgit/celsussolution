using System;

namespace Celsus.Client.Shared.Types.Workflow
{
    public class LocKeyAttribute : Attribute
    {
        public string LocKey { get; set; }
        public LocKeyAttribute(string locKey)
        {
            LocKey = locKey;
        }
    }
}