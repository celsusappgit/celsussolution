using System;

namespace Celsus.Client.Controls.Management.Sources
{
    public class MyWorkflowModel
    {
        public string Name { get; internal set; }
        public Type InternalType { get; internal set; }

        public string InternalTypeFullName
        {
            get
            {
                return InternalType.FullName;
            }
        }
    }
}