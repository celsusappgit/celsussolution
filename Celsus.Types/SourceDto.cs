using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Types
{
    [Table("Source", Schema = "Celsus")]
    public class SourceDto : ModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (Equals(value, name)) return;
                name = value;
                NotifyPropertyChanged(() => Name);
            }
        }
        string path;
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                if (Equals(value, path)) return;
                path = value;
                NotifyPropertyChanged(() => Path);
            }
        }

        bool isActive;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                if (Equals(value, isActive)) return;
                isActive = value;
                NotifyPropertyChanged(() => IsActive);
            }
        }
    }

    public class ModelBase : INotifyPropertyChanged
    {
        public object GetPropValue(string propName)
        {
            return this.GetType().GetProperty(propName).GetValue(this, null);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> exp)
        {
            var memberExpression = (MemberExpression)exp.Body;
            string propertyName = memberExpression.Member.Name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
