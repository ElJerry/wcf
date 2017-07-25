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
                await writer.WriteStartDocument(); // Write initial tags <rss><channel>

                // Write title in raw format
                var title = new SyndicationContent("<title> My title </title>");
                await writer.WriteContent(title);

                // Write any tag and value
                await writer.WriteElementString("Example-Name", "Value information");
                
                //Write a link.
                var link = new SyndicationLink(new Uri("http://hello.com"))
                {
                    RelationshipType = "alternate"
                };

                await writer.WriteLink(link);

                //Write a category
                var category = new SyndicationCategory()
                {
                    Name = "sports"
                };

                await writer.WriteCategory(category);


                //Write a Person
                var person = new SyndicationPerson()
                {
                    Email = "hero@test.com",
                    RelationshipType = "managingEditor"
                };

                await writer.WritePerson(person);

                //Write an Item
                var itemReader = XmlReader.Create(@"..\..\..\TestFeeds\rssitem.xml");
                itemReader.MoveToContent();
                var item = new Rss20FeedFormatter().ParseItem(itemReader.ReadOuterXml());
                await writer.WriteItem(item);

                //write closing tags channel and rss.
                await writer.WriteEndDocument(); 
                xmlWriter.Flush();
            }
            
            string res = sb.ToString();
            Assert.True(res == "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss version=\"2.0\"><channel><title> My title </title><Example-Name>Value information</Example-Name><link>http://hello.com</link><category>sports</category><managingEditor>hero@test.com</managingEditor><item><title>Lorem ipsum 2017-07-06T20:25:00+00:00</title><link>http://example.com/test/1499372700</link><source url=\"http://www.quotationspage.com/data/qotd.rss\">Quotes of the Day</source><enclosure url=\"http://testpage.com\" length=\"123\" type=\"audio/mp3\" /><description>Lorem ipsum 2017-07-06T20:25:00+00:00</description><author>Author@email.com</author><category>Enterntainment</category><category>Sports</category><guid>http://example.com/test/1499372700</guid><pubDate>Thu, 06 Jul 2017 20:25:00 GMT</pubDate></item></channel></rss>");
        }     
    }
}
