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
                    Email = "hero@test.com"
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
        }

     
    }
}
