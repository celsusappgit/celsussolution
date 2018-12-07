using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Types
{
    public class HelpAttribute : Attribute
    {
        public string HelpKeywords { get; set; }

        public string Url { get; set; }

        public HelpAttribute(string helpKeywords, string url)
        {
            HelpKeywords = helpKeywords;
            Url = url;
        }
    }
}
