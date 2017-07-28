using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SyndicationFeed
{
    public class XmlNamespace
    {
        public string Prefix { get; set; }

        public Uri Uri { get; set; }

        public XmlNamespace(string prefix, Uri uri)
        {

            if (string.IsNullOrEmpty(prefix))
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            Prefix = prefix;
        }
    }
}
