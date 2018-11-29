using Celsus.DataLayer;
using Celsus.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Celsus.Client.Shared.Types.Workflow
{
    [Workson("XML")]
    [LocKey("KyoceraXmlWorkflow")]
    public class KyoceraXmlWorkflow : CodeWorkflowBase, ICodeWorkflow
    {
        public List<MyArgument> GetArgumentsList()
        {
            return new List<MyArgument>()
            {
                new MyArgument() { Name="ArgFileSystemId", ArgumentType = typeof(int), IsOptional = false },
            };
        }

        public List<MyOption> GetOptionsList()
        {
            return new List<MyOption>()
            {

            };
        }

        public void SetArguments(List<MyArgument> arguments)
        {
            this.arguments = arguments;
        }

        public void SetOptions(List<MyOption> options)
        {
            this.options = options;
        }

        public async Task<bool> Run()
        {
            var connectionString = SettingsHelper.Instance.ConnectionString;

            var argFileIdValue = GetArgument<int>("ArgFileSystemId");
            var varSourceIdValue = GetVariable<int>("VarSourceId");

            FileSystemItemDto fileSystemItem = null;

            try
            {
                using (var sqlDbContext = new SqlDbContext(connectionString))
                {
                    fileSystemItem = await sqlDbContext.FileSystemItems.FirstOrDefaultAsync(x => x.SourceId == varSourceIdValue && x.Id == argFileIdValue);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            XmlSerializer ser = new XmlSerializer(typeof(XmlRoot));
            XmlRoot myData;
            try
            {
                using (XmlReader reader = XmlReader.Create(fileSystemItem.FullPath))
                {
                    myData = (XmlRoot)ser.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                return false;
            }


            List<FileSystemItemMetadataDto> metaDatas = new List<FileSystemItemMetadataDto>();
            if (myData.Files != null && myData.Files.File != null)
            {
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "File", StringValue = myData.Files.File, ValueType = "String" });
            }

            if (myData.Job != null)
            {
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Job Color", BoolValue = myData.Job.Color, ValueType = "Bool" });
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Job Color Count", IntValue = myData.Job.ColorCount, ValueType = "Int" });
                if (myData.Job.Created != null)
                {
                    metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Job Created", DateTimeValue = myData.Job.Created, ValueType = "DateTime" });
                }
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Job Duplex", BoolValue = myData.Job.Duplex, ValueType = "Bool" });
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Job Id", IntValue = myData.Job.Id, ValueType = "Int" });
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Job Mono Count", IntValue = myData.Job.MonoCount, ValueType = "Int" });
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Job Paper", IntValue = myData.Job.Paper, ValueType = "Int" });
            }

            if (myData.JobType != null)
            {
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Job Type", StringValue = myData.JobType, ValueType = "String" });
            }
            if (myData.Printer != null)
            {
                if (myData.Printer.MacAddresses != null && myData.Printer.MacAddresses.Item != null)
                {
                    metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Printer MAC Addresses ", StringValue = myData.Printer.MacAddresses.Item, ValueType = "String" });
                }
                if (myData.Printer.SerialNumber != null)
                {
                    metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Printer Serial Number ", StringValue = myData.Printer.SerialNumber, ValueType = "String" });
                }
            }

            if (myData.PrinterAddr != null)
            {
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Printer Addr", StringValue = myData.PrinterAddr, ValueType = "String" });
            }
            if (myData.ServerName != null)
            {
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Server Name", StringValue = myData.ServerName, ValueType = "String" });
            }
            if (myData.ServerVersion != null)
            {
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Server Version", StringValue = myData.ServerVersion, ValueType = "String" });
            }
            if (myData.Timestamp != null)
            {
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Timestamp", DateTimeValue = myData.Timestamp, ValueType = "DateTime" });
            }
            if (myData.Username != null)
            {
                metaDatas.Add(new FileSystemItemMetadataDto() { Key = "Username", StringValue = myData.Username, ValueType = "String" });
            }

            if (myData.Files != null && myData.Files.File != null)
            {
                var fullPath = Path.Combine(Path.GetDirectoryName(fileSystemItem.FullPath), myData.Files.File);
                FileSystemItemDto realtedFileSystemItem = null;

                try
                {
                    using (var sqlDbContext = new SqlDbContext(connectionString))
                    {
                        realtedFileSystemItem = await sqlDbContext.FileSystemItems.FirstOrDefaultAsync(x => x.SourceId == varSourceIdValue && x.ParentId == fileSystemItem.ParentId && x.FullPath == fullPath);
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }

                foreach (var metaData in metaDatas)
                {
                    metaData.FileSystemItemId = realtedFileSystemItem.Id;
                }

                try
                {
                    using (var sqlDbContext = new SqlDbContext(connectionString))
                    {
                        foreach (var metaData in metaDatas)
                        {
                            sqlDbContext.FileSystemItemMetadatas.Add(metaData);
                        }

                        await sqlDbContext.SaveChangesAsync();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    //LogHelper.AddSessionLog(SessionLogTypeEnum.ActivityError, sessionId, $"Exception has been thrown when getting fileSystemItem. FileSystemItemId: {fileSystemItemId}", ex);
                    return false;
                }
            }
            

            return true;
        }
    }



    //// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    ///// <remarks/>
    //[System.SerializableAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    //[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    //public partial class data
    //{

    //    /// <remarks/>
    //    public DateTime timestamp { get; set; }

    //    /// <remarks/>
    //    public string username { get; set; }

    //    /// <remarks/>
    //    public string jobType { get; set; }

    //    /// <remarks/>
    //    public string serverName { get; set; }

    //    /// <remarks/>
    //    public string serverVersion { get; set; }

    //    /// <remarks/>
    //    public string printerAddr { get; set; }
    //    /// <remarks/>
    //    public dataFiles files { get; set; }

    //    /// <remarks/>
    //    public dataJob job { get; set; }

    //    /// <remarks/>
    //    public dataPrinter printer { get; set; }
    //}

    ///// <remarks/>
    //[System.SerializableAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    //public partial class dataFiles
    //{


    //    public string file { get; set; }
    //}

    ///// <remarks/>
    //[System.SerializableAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    //public partial class dataJob
    //{

    //    /// <remarks/>
    //    public int monoCount { get; set; }

    //    /// <remarks/>
    //    public int colorCount { get; set; }

    //    /// <remarks/>
    //    public bool duplex { get; set; }

    //    /// <remarks/>
    //    public bool color { get; set; }

    //    /// <remarks/>
    //    public int paper { get; set; }

    //    /// <remarks/>
    //    public DateTime created { get; set; }

    //    /// <remarks/>
    //    public long id { get; set; }
    //}

    ///// <remarks/>
    //[System.SerializableAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    //public partial class dataPrinter
    //{
    //    /// <remarks/>
    //    public dataPrinterMacAddresses macAddresses { get; set; }

    //    public string serialNumber { get; set; }
    //}

    ///// <remarks/>
    //[System.SerializableAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    //public partial class dataPrinterMacAddresses
    //{

    //    /// <remarks/>
    //    public string item { get; set; }
    //}


    /////////////////////
    /////
    //[XmlRoot(ElementName = "files")]
    //public class Files
    //{
    //    [XmlElement(ElementName = "file")]
    //    public string File { get; set; }
    //}

    //[XmlRoot(ElementName = "job")]
    //public class Job
    //{
    //    [XmlElement(ElementName = "monoCount")]
    //    public string MonoCount { get; set; }
    //    [XmlElement(ElementName = "colorCount")]
    //    public string ColorCount { get; set; }
    //    [XmlElement(ElementName = "duplex")]
    //    public string Duplex { get; set; }
    //    [XmlElement(ElementName = "color")]
    //    public string Color { get; set; }
    //    [XmlElement(ElementName = "paper")]
    //    public string Paper { get; set; }
    //    [XmlElement(ElementName = "created")]
    //    public string Created { get; set; }
    //    [XmlElement(ElementName = "id")]
    //    public string Id { get; set; }
    //}

    //[XmlRoot(ElementName = "macAddresses")]
    //public class MacAddresses
    //{
    //    [XmlElement(ElementName = "item")]
    //    public string Item { get; set; }
    //}

    //[XmlRoot(ElementName = "printer")]
    //public class Printer
    //{
    //    [XmlElement(ElementName = "serialNumber")]
    //    public string SerialNumber { get; set; }
    //    [XmlElement(ElementName = "macAddresses")]
    //    public MacAddresses MacAddresses { get; set; }
    //}

    //[XmlRoot(ElementName = "data")]
    //public class Data
    //{
    //    internal object job;

    //    [XmlElement(ElementName = "timestamp")]
    //    public string Timestamp { get; set; }
    //    [XmlElement(ElementName = "username")]
    //    public string Username { get; set; }
    //    [XmlElement(ElementName = "jobType")]
    //    public string JobType { get; set; }
    //    [XmlElement(ElementName = "serverName")]
    //    public string ServerName { get; set; }
    //    [XmlElement(ElementName = "serverVersion")]
    //    public string ServerVersion { get; set; }
    //    [XmlElement(ElementName = "printerAddr")]
    //    public string PrinterAddr { get; set; }
    //    [XmlElement(ElementName = "files")]
    //    public Files Files { get; set; }
    //    [XmlElement(ElementName = "job")]
    //    public Job Job { get; set; }
    //    [XmlElement(ElementName = "printer")]
    //    public Printer Printer { get; set; }
    //}



}
