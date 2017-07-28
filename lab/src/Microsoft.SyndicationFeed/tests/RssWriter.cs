﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace Microsoft.SyndicationFeed.Tests
{
    public class RssWriter
    {
        [Fact]
        public async Task Rss20Writer_WriteCategory()
        {

            StringBuilder sb = new StringBuilder();

            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {
                Rss20FeedWriter writer = new Rss20FeedWriter(xmlWriter);
                SyndicationCategory category = new SyndicationCategory();
                category.Name = "Test Category";
                await writer.Write(category);
                xmlWriter.Flush();
            }

            string res = sb.ToString();
            Assert.True(res == "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss version=\"2.0\"><channel><category>Test Category</category></channel></rss>");
        }

        [Fact]
        public async Task Rss20Writer_WritePerson()
        {

            StringBuilder sb = new StringBuilder();

            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {
                xmlWriter.WriteStartElement("document");
                Rss20FeedWriter writer = new Rss20FeedWriter(xmlWriter);
                SyndicationPerson author = new SyndicationPerson()
                {
                    Email = "author@email.com",
                    RelationshipType = Rss20Constants.AuthorTag
                };

                SyndicationPerson managingEditor = new SyndicationPerson()
                {
                    Email = "mEditor@email.com",
                    RelationshipType = Rss20Constants.ManagingEditorTag
                };

                await writer.Write(author);
                await writer.Write(managingEditor);

                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
            }

            string res = sb.ToString();
            Assert.True(res == "<?xml version=\"1.0\" encoding=\"utf-16\"?><document><rss version=\"2.0\"><channel><author>author@email.com</author><managingEditor>mEditor@email.com</managingEditor></channel></rss></document>");
        }

        [Fact]
        public async Task Rss20Writer_WriteImage()
        {

            StringBuilder sb = new StringBuilder();

            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {

                Rss20FeedWriter writer = new Rss20FeedWriter(xmlWriter);

                Uri url = new Uri("http://testuriforimage.com");
                Uri urlForLink = new Uri("http://testuriforlink.com");
                SyndicationLink link = new SyndicationLink(urlForLink)
                {
                    RelationshipType = Rss20Constants.LinkTag
                };

                SyndicationImage image = new SyndicationImage(url)
                {
                    Title = "Testing image title",
                    Description = "testing image description",
                    Link = link
                };

                await writer.Write(image);
                
                xmlWriter.Flush();
            }

            string res = sb.ToString();
            Assert.True(res == "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss version=\"2.0\"><channel><image><url>http://testuriforimage.com/</url><title>Testing image title</title><link>http://testuriforlink.com/</link><description>testing image description</description></image></channel></rss>");
        }

        [Fact]
        public async Task Rss20Writer_WriteLink_onlyUrl()
        {

            StringBuilder sb = new StringBuilder();

            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {
                
                Rss20FeedWriter writer = new Rss20FeedWriter(xmlWriter);
                
                Uri urlForLink = new Uri("http://testuriforlink.com");
                SyndicationLink link = new SyndicationLink(urlForLink)
                {
                    RelationshipType = Rss20Constants.LinkTag
                };
                
                await writer.Write(link);
                
                xmlWriter.Flush();
            }

            string res = sb.ToString();
            Assert.True(res == "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss version=\"2.0\"><channel><link>http://testuriforlink.com/</link></channel></rss>");
        }

        [Fact]
        public async Task Rss20Writer_WriteLink_allElements()
        {

            StringBuilder sb = new StringBuilder();

            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {

                Rss20FeedWriter writer = new Rss20FeedWriter(xmlWriter);

                Uri urlForLink = new Uri("http://testuriforlink.com");
                SyndicationLink link = new SyndicationLink(urlForLink)
                {
                    Title = "Test title",
                    Length = 123,
                    MediaType = "mp3/video",
                    RelationshipType = Rss20Constants.LinkTag
                };

                await writer.Write(link);

                xmlWriter.Flush();
            }

            string res = sb.ToString();
            Assert.True(res == "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss version=\"2.0\"><channel><link length=\"123\" type=\"mp3/video\" url=\"http://testuriforlink.com/\">Test title</link></channel></rss>");
        }

        [Fact]
        public async Task Rss20Writer_WriteLink_uriEqualsTitle()
        {

            StringBuilder sb = new StringBuilder();

            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {

                Rss20FeedWriter writer = new Rss20FeedWriter(xmlWriter);

                Uri urlForLink = new Uri("http://testuriforlink.com");
                SyndicationLink link = new SyndicationLink(urlForLink)
                {
                    Title = "http://testuriforlink.com",
                    RelationshipType = Rss20Constants.LinkTag
                };

                await writer.Write(link);

                xmlWriter.Flush();
            }

            string res = sb.ToString();
            Assert.True(res == "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss version=\"2.0\"><channel><link>http://testuriforlink.com</link></channel></rss>");
        }

        [Fact]
        public async Task Rss20Writer_WriteItem()
        {

            StringBuilder sb = new StringBuilder();

            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {

                Rss20FeedWriter writer = new Rss20FeedWriter(xmlWriter);

                Uri url = new Uri("http://testuriforlinks.com");
                SyndicationLink link = new SyndicationLink(url)
                {
                    RelationshipType = Rss20Constants.LinkTag
                };

                SyndicationLink enclosureLink = new SyndicationLink(url)
                {
                    Title = "http://enclosurelink.com",
                    RelationshipType = Rss20Constants.EnclosureTag,
                    Length = 4123,
                    MediaType = "audio/mpeg"
                };

                SyndicationLink commentsLink = new SyndicationLink(url)
                {
                    RelationshipType = Rss20Constants.CommentsTag
                };

                SyndicationLink sourceLink = new SyndicationLink(url)
                {
                    RelationshipType = Rss20Constants.SourceTag,
                    Title = "Anonymous Blog"
                };

                SyndicationItem item = new SyndicationItem();

                item.Title = "First item on ItemWriter";
                var links = new List<SyndicationLink>() { link, enclosureLink, commentsLink, sourceLink };
                item.Links = links;

                item.Description = "Brief description of an item";

                var contributors = new List<SyndicationPerson>() {
                    new SyndicationPerson() {
                        Email = "person@email.com", RelationshipType = Rss20Constants.AuthorTag
                    }
                };
                
                item.Contributors = contributors;

                item.Id = "Unique ID for this item";

                DateTimeOffset time;
                DateTimeOffset.TryParse("Fri, 28 Jul 2017 19:07:32 GMT",out time);

                item.Published = time;
                
                await writer.Write(item);

                xmlWriter.Flush();
            }

            string res = sb.ToString();
            Assert.True(res == "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss version=\"2.0\"><channel><item><title>First item on ItemWriter</title><link>http://testuriforlinks.com/</link><enclosure url=\"http://testuriforlinks.com/\" length=\"4123\" type=\"audio/mpeg\" /><comments>http://testuriforlinks.com/</comments><source url=\"http://testuriforlinks.com/\">Anonymous Blog</source><description>Brief description of an item</description><author>person@email.com</author><guid>Unique ID for this item</guid><pubDate>Fri, 28 Jul 2017 19:07:32 GMT</pubDate></item></channel></rss>");
        }

        [Fact]
        public async Task Rss20Writer_WriteContent()
        {
            SyndicationContent content = null;

            using (XmlReader xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\CustomXml.xml"))
            {
                Rss20FeedReader reader = new Rss20FeedReader(xmlReader);
                content = (SyndicationContent)await reader.ReadContent();
            }

            StringBuilder sb = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {
                Rss20FeedWriter writer = new Rss20FeedWriter(xmlWriter);

                await writer.Write(content);
                xmlWriter.Flush();
            }

            string res = sb.ToString();
            Assert.True(res == "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss version=\"2.0\"><channel><NewItem><enclosure url=\"http://www.scripting.com/mp3s/weatherReportSuite.mp3\" length=\"12216320\" type=\"audio/mpeg\" /><title>Lorem ipsum 2017-07-06T20:25:00+00:00</title><description>Exercitation sit dolore mollit et est eiusmod veniam aute officia veniam ipsum.</description><link>http://example.com/test/1499372700</link><guid isPermaLink=\"true\">http://example.com/test/1499372700</guid><pubDate>Thu, 06 Jul 2017 20:25:00 GMT</pubDate></NewItem></channel></rss>");
        }

        [Fact]
        public async Task Rss20Writer_WriteValue()
        {
            var sb = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {
                var writer = new Rss20FeedWriter(xmlWriter);
                await writer.WriteValue("CustomTag", "Custom Content");
                xmlWriter.Flush();
            }

            var res = sb.ToString();
            Assert.True(res == "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss version=\"2.0\"><channel><CustomTag>Custom Content</CustomTag></channel></rss>");
        }

        [Fact]
        public async Task Rss20Writer_UseNamespaces()
        {
            var sb = new StringBuilder();

            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {
                var n1 = new XmlNamespace("test1", new Uri("http://namespace1.com"));
                var n2 = new XmlNamespace("test2", new Uri("http://namespace2.com"));
                var namespaces = new List<XmlNamespace>() { n1 , n2 };

                

                var writer = new Rss20FeedWriter(xmlWriter, namespaces);

                await writer.WriteValue("Hello", "World");
                xmlWriter.Flush();
            }

            var res = sb.ToString();
            Assert.True(res == "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss xmlns:test1=\"http://namespace1.com/\" xmlns:test2=\"http://namespace2.com/\" version=\"2.0\"><channel><Hello>World</Hello></channel></rss>"    );
        }
    }
}
