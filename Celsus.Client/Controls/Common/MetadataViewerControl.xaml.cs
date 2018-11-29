using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

namespace Celsus.Client.Controls.Common
{
    public class MetadataViewerControlModel : BaseModel<MetadataViewerControlModel>
    {
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
                GetMetadatas();
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

        List<FileSystemItemMetadataDto> metadatas;
        public List<FileSystemItemMetadataDto> Metadatas
        {
            get
            {
                return metadatas;
            }
            set
            {
                if (Equals(value, metadatas)) return;
                metadatas = value;
                NotifyPropertyChanged(() => Metadatas);
            }
        }

        private async void GetMetadatas()
        {
            IsBusy = true;
            try
            {
                using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                {
                    var metadatas = await context.FileSystemItemMetadatas.Where(x => x.FileSystemItemId == FileSystemId).ToListAsync();
                    //Status = $"Found {metadatas.Count} items";
                    if (metadatas == null || metadatas.Count == 0)
                    {
                        Status = "There is no items found for search term".ConvertToBindableText();
                    }
                    Metadatas = metadatas;
                }
            }
            catch (Exception ex)
            {
                Status = $"Error in search.".ConvertToBindableText();
            }
            IsBusy = false;
        }
    }
    public partial class MetadataViewerControl : UserControl
    {
        public MetadataViewerControl()
        {
            InitializeComponent();

        }

        internal void Prepare(int id)
        {
            MetadataViewerControlModel.Instance.FileSystemId = id;
            DataContext = MetadataViewerControlModel.Instance;
        }
    }
}
