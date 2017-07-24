using System;
using System.Threading.Tasks;
using System.Xml;

namespace Microsoft.SyndicationFeed
{
    public class Rss20FeedWriter : ISyndicationFeedWriter
    {
        private XmlWriter _writer;

        public Rss20FeedWriter(XmlWriter writer)
        {
            _writer = writer;
        }

        public async Task WriteCategory(ISyndicationCategory category)
        {
            string name = category.Name;            
            await _writer.WriteElementStringAsync(null,Rss20Constants.CategoryTag,null,name);            
        }

        public Task WriteContent(ISyndicationContent content)
        {
            throw new NotImplementedException();
        }

        public async Task WriteEndDocument()
        {
            await _writer.WriteEndElementAsync(); //channel
            await _writer.WriteEndElementAsync(); //Rss
            await _writer.WriteEndDocumentAsync();//xml 
        }

        public Task WriteItem(ISyndicationItem item)
        {
            throw new NotImplementedException();
        }

        public async Task WriteLink(ISyndicationLink link)
        {
            switch (link.RelationshipType)
            {
                case Rss20Constants.AlternateLink:
                    await _writer.WriteElementStringAsync(null,Rss20Constants.LinkTag,null,link.Uri.OriginalString);
                    break;

                case Rss20Constants.CommentsTag:
                    await _writer.WriteElementStringAsync(null, Rss20Constants.CommentsTag, null, link.Uri.OriginalString);
                    break;

                case Rss20Constants.EnclosureTag:
                    await _writer.WriteStartElementAsync(null,Rss20Constants.EnclosureTag,null);
                    //Attributes
                    await _writer.WriteAttributeStringAsync(null, Rss20Constants.UrlTag, null, link.Uri.OriginalString);
                    
                    if(link.Length != 0)
                    {
                        await _writer.WriteAttributeStringAsync(null, Rss20Constants.LengthTag, null, link.Length.ToString());
                    }

                    if(link.MediaType != null)
                    {
                        await _writer.WriteAttributeStringAsync(null, Rss20Constants.TypeTag, null, link.MediaType);
                    }

                    await _writer.WriteEndElementAsync();
                    break;

                case Rss20Constants.SourceTag:
                    await _writer.WriteStartElementAsync(null,Rss20Constants.SourceTag,null);
                    await _writer.WriteAttributeStringAsync(null, Rss20Constants.UrlTag, null, link.Uri.OriginalString);
                    await _writer.WriteEndElementAsync();
                    break;
            }
        }

        public Task WritePerson(ISyndicationPerson person)
        {
            throw new NotImplementedException();
        }

        public async Task WriteStartDocument()
        {
            await _writer.WriteStartDocumentAsync();
            await _writer.WriteStartElementAsync(null, Rss20Constants.RssTag, null);
            await _writer.WriteAttributeStringAsync(null,Rss20Constants.VersionTag, null, Rss20Constants.Version);
            await _writer.WriteStartElementAsync(null, Rss20Constants.ChannelTag, null);
        }

        private void WriteStartTags()
        {
            _writer.WriteStartDocument();
            _writer.WriteStartElement(Rss20Constants.RssTag);
            _writer.WriteAttributeString(Rss20Constants.VersionTag,Rss20Constants.Version);
            _writer.WriteStartElement(Rss20Constants.ChannelTag);

            _writer.WriteElementString("title", "some item values");

            //TEST CLOSING TAGS
            _writer.WriteEndElement();
            _writer.WriteEndElement();
            _writer.WriteEndDocument();

        }

    }
}
