using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Celsus.Client.Shared.Types
{
    public class TrialLicenseInfo
    {
        public string FirstName { get; internal set; }
        public string Organization { get; internal set; }
        public string EMail { get; internal set; }
        public string LastName { get; internal set; }
        public DateTime TrialDueDate { get; internal set; }
        public string TrialId { get; internal set; }
        public string Indexer01 { get; internal set; }

        public string GetAsString()
        {
            var builder = new StringBuilder();

            AddProperty(builder, () => TrialId);
            AddProperty(builder, () => FirstName);
            AddProperty(builder, () => LastName);
            AddProperty(builder, () => EMail);
            AddProperty(builder, () => Organization);
            AddProperty(builder, () => TrialDueDate);

            return builder.ToString();
        }

        private void AddProperty<T>(StringBuilder builder, Expression<Func<T>> propertyExpression)
        {
            var memberExpression = (MemberExpression)propertyExpression.Body;
            var propertyInfo = memberExpression.Member as PropertyInfo;
            string propertyName = memberExpression.Member.Name;
            builder.Append(propertyName);
            builder.Append(": ");
            var propValue = propertyInfo.GetValue(this);
            if (propValue == null || string.IsNullOrWhiteSpace(propertyInfo.GetValue(this).ToString()))
                builder.AppendLine("");
            else
            {
                if (propertyInfo.GetValue(this).GetType() == typeof(DateTime))
                {
                    builder.AppendLine(((DateTime)propertyInfo.GetValue(this)).ToString("dd MMMM yyyy HH:mm"));
                }
                else
                {
                    builder.AppendLine(propertyInfo.GetValue(this).ToString());
                }
            }

        }
    }
}