using System;

namespace Celsus.Client.Shared.Types.Workflow
{
    public class WorksonAttribute : Attribute
    {
        public string FileType { get; set; }
        public WorksonAttribute(string fileType)
        {
            FileType = fileType;
        }
    }
}