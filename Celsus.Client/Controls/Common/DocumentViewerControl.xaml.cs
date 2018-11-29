using Celsus.Client.Controls.Setup.Database;
using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Navigation;
using Telerik.Windows.Documents.Flow.FormatProviders.Txt;
using Telerik.Windows.Documents.Flow.Model;

namespace Celsus.Client.Controls.Common
{
    public class DocumentViewerControlModel : BaseModel<DocumentViewerControlModel>
    {

        public DocumentViewerControlModel()
        {
            FirstWindowModel.Instance.AnalitycsMonitor.TrackScreenView("DocumentViewer");
        }


        string textContent;
        public string TextContent
        {
            get
            {
                return textContent;
            }
            set
            {
                if (Equals(value, textContent)) return;
                textContent = value;
                NotifyPropertyChanged(() => TextContent);
            }
        }

        int fileSystemId;
        public int FileSystemId
        {
            get
            {
                return fileSystemId;
            }
            set
            {
                if (Equals(value, fileSystemId)) return;
                fileSystemId = value;
                NotifyPropertyChanged(() => FileSystemId);
                GetText();
            }
        }

        string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                if (Equals(value, title)) return;
                title = value;
                NotifyPropertyChanged(() => Title);
            }
        }

        bool isBusy;
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                if (Equals(value, isBusy)) return;
                isBusy = value;
                NotifyPropertyChanged(() => IsBusy);
            }
        }

        object status;
        public object Status
        {
            get
            {
                return status;
            }
            set
            {
                if (Equals(value, status)) return;
                status = value;
                NotifyPropertyChanged(() => Status);
                NotifyPropertyChanged(() => StatusVisibility);
            }
        }

        public Visibility StatusVisibility
        {
            get
            {
                return Status == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private async void GetText()
        {
            TextContent = "";
            IsBusy = true;
            try
            {
                using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                {
                    var fileSystemItem = await context.FileSystemItems.FirstOrDefaultAsync(x => x.Id == FileSystemId);
                    Title = fileSystemItem.Name;
                    var clearText = await context.ClearTexts.FirstOrDefaultAsync(x => x.FileSystemItemId == FileSystemId);
                    //Status = $"Found {metadatas.Count} items";
                    if (clearText == null)
                    {
                        Status = "There is no items found for search term".ConvertToBindableText();
                    }
                    IsBusy = false;
                    TextContent = clearText.TextInFile;
                }
            }
            catch (Exception ex)
            {
                Status = $"Error in search.".ConvertToBindableText();
            }

        }


    }
    public partial class DocumentViewerControl : UserControl
    {
        public DocumentViewerControl()
        {
            InitializeComponent();
        }

        internal void Prepare(int fileSystemItemId)
        {
            DocumentViewerControlModel.Instance.FileSystemId = fileSystemItemId;
            DataContext = DocumentViewerControlModel.Instance;
        }
    }
}
