using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Celsus.Client.Types
{
    public static class Extensions
    {
        
        public static TextBlock ConvertToBindableText(this string locKey)
        {
            var textBlock = new TextBlock();
            textBlock.SetBinding(TextBlock.TextProperty, new LocExtension(locKey));
            return textBlock;
        }

        public static TextBlock ConvertToBindableText(this string locKey, params object[] converterParameters)
        {
            var textBlock = new TextBlock();
            textBlock.SetBinding(TextBlock.TextProperty, new LocExtension(locKey) { Converter = new StringFormatConverter(), ConverterParameter = converterParameters });
            return textBlock;
        }
    }

    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameters = parameter as object[];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].GetType() == typeof(DateTime))
                {
                    parameters[i] = ((DateTime)parameters[i]).ToString(TranslationSource.Instance.CurrentCulture);
                }
            }
            return string.Format(value.ToString(), (object[])parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
