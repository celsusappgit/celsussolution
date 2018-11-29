using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//------------------------------------------------------------------------------

using System.Xml.Serialization;

namespace Celsus.Client.Shared.Types.Workflow
{
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "data",  Namespace = "", IsNullable = false)]
    public partial class XmlRoot
    {
        [System.Xml.Serialization.XmlElementAttribute(DataType = "string", ElementName = "timestamp")]
        public string _timestamp { get; set; }

        [XmlIgnore]
        public DateTime? Timestamp
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_timestamp))
                {
                    return DateTime.ParseExact(_timestamp, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                }
                return null;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "string", ElementName = "username")]
        public string Username { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "string", ElementName = "jobType")]
        public string JobType { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "string", ElementName = "serverName")]
        public string ServerName { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "string", ElementName = "serverVersion")]
        public string ServerVersion { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "string", ElementName = "printerAddr")]
        public string PrinterAddr { get; set; }

        [System.Xml.Serialization.XmlElementAttribute( ElementName = "files")]
        public Files Files { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(ElementName = "job")]
        public Job Job { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(ElementName = "printer")]
        public Printer Printer { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Files
    {
        [System.Xml.Serialization.XmlElementAttribute(DataType = "string", ElementName = "file")]
        public string File { get; set; }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Job
    {

        [System.Xml.Serialization.XmlElementAttribute(DataType = "int", ElementName = "id")]
        public int Id { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "int", ElementName = "monoCount")]
        public int MonoCount { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "int", ElementName = "colorCount")]
        public int ColorCount { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "boolean", ElementName = "duplex")]
        public bool Duplex { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "boolean", ElementName = "color")]
        public bool Color { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "int", ElementName = "paper")]
        public int Paper { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "string", ElementName = "created")]
        public string _created { get; set; }

        [XmlIgnore]
        public DateTime? Created
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_created))
                {
                    return DateTime.ParseExact(_created, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                }
                return null;
            }
        }

    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Printer
    {
        [System.Xml.Serialization.XmlElementAttribute(DataType = "string", ElementName = "serialNumber")]
        public string SerialNumber { get; set; }

        [System.Xml.Serialization.XmlElementAttribute( ElementName = "macAddresses")]
        public PrinterMacAddresses MacAddresses { get; set; }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class PrinterMacAddresses
    {
        [System.Xml.Serialization.XmlElementAttribute(DataType = "string", ElementName = "item")]
        public string Item { get; set; }
    }

}
