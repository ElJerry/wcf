// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace Microsoft.SyndicationFeed.Tests
{
    public class RSS20Writer
    {
        [Fact]
        public async Task RSS_StartWriter()
        {
            StringBuilder sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings() { Async = true}))
            {
                var writer = new Rss20FeedWriter(xmlWriter);
                await writer.WriteStartDocument(); // Write initial tags

                var title = new SyndicationContent("<title> My title </title>");

                await writer.WriteContent(title);

                var link = new SyndicationLink(new Uri("http://hello.com"))
                {
                    RelationshipType = "alternate"
                };

                await writer.WriteLink(link);

                var category = new SyndicationCategory()
                {
                    Name = "sports"
                };

                await writer.WriteCategory(category);

                var person = new SyndicationPerson()
                {
                    Email = "hero@test.com"
                };

                await writer.WritePerson(person);

                var item = new Rss20FeedFormatter().ParseItem("<item>\n         <title><![CDATA[Lorem ipsum 2017-07-06T20:25:00+00:00]]></title>\n         <description><![CDATA[Exercitation sit dolore mollit et est eiusmod veniam aute officia veniam ipsum.]]></description>\n         <link>http://example.com/test/1499372700</link>\n         <guid isPermaLink=\"true\">http://example.com/test/1499372700</guid>\n         <dc:creator xmlns:dc=\"http://purl.org/dc/elements/1.1/\"><![CDATA[John Smith]]></dc:creator>\n         <pubDate>Thu, 06 Jul 2017 20:25:00 GMT</pubDate>\n      </item>");
                await writer.WriteItem(item);


                await writer.WriteEndDocument(); //write closing tags channel and rss.
                xmlWriter.Flush();
            }
            
            string res = sb.ToString();
        }

     
    }
}
