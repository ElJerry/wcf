// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace Microsoft.SyndicationFeed.Tests
{
    public class RSS20Writer
    {
        [Fact]
        public void RSS_StartWriter()
        {
            StringBuilder sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings { }))
            {
                var writer = new Rss20FeedWriter(xmlWriter);
                xmlWriter.Flush();
            }
            
                string res = sb.ToString();
        }

     
    }
}
