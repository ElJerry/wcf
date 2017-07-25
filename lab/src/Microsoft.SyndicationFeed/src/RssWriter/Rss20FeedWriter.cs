// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

        public virtual Task WriteCategory(ISyndicationCategory category)
        {

            if (string.IsNullOrEmpty(category.Name))
            {
                throw new FormatException("Category must have a name");
            }

            return _writer.WriteElementStringAsync(null,Rss20Constants.CategoryTag,null, category.Name);            
        }

        public virtual Task WriteContent(ISyndicationContent content)
        {
            if (string.IsNullOrEmpty(content.RawContent))
            {
                throw new FormatException("Content does not contain any data");
            }

            return _writer.WriteRawAsync(content.RawContent);
        }
        
        public virtual async Task WriteItem(ISyndicationItem item)
        {
            await _writer.WriteStartElementAsync(null,Rss20Constants.ItemTag,null); //Write <item> tag

            //Write title
            if (!string.IsNullOrEmpty(item.Title))
            {
                await _writer.WriteElementStringAsync(null, Rss20Constants.TitleTag, null, item.Title);
            }

            //Write links
            foreach(var link in item.Links)
            {
                await WriteLink(link);
            }

            //Write description
            if (!string.IsNullOrEmpty(item.Description))
            {
                await _writer.WriteElementStringAsync(null, Rss20Constants.DescriptionTag, null, item.Title);
            }
                       
            //Write persons
            foreach(var person in item.Contributors)
            {
                if(person.RelationshipType == Rss20Constants.AuthorTag)
                {
                    //check if email exists
                    if (!string.IsNullOrEmpty(person.Email))
                    {
                        await _writer.WriteElementStringAsync(null, Rss20Constants.AuthorTag, null, person.Email);
                    }
                    else if (!string.IsNullOrEmpty(person.Name))
                    {
                        await _writer.WriteElementStringAsync(null, Rss20Constants.AuthorTag, null, person.Name);
                    }
                }
            }

            //Write Categories
            foreach (var category in item.Categories)
            {
                _writer.WriteElementString(Rss20Constants.CategoryTag, category.Name);
            }

            //Write Guid
            if (!string.IsNullOrEmpty(item.Id))
            {
                await _writer.WriteElementStringAsync(null,Rss20Constants.GuidTag,null,item.Id);
            }

            //Write pubdate
            if(!item.Published.Equals(new DateTimeOffset()))
            {
                _writer.WriteElementString(Rss20Constants.PubDateTag,item.Published.ToString("r"));
            }
        }

        public virtual async Task WriteLink(ISyndicationLink link)
        {

            if(link.Uri == null)
            {
                throw new FormatException("Link's Uri can not be null");
            }

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

                    if (!string.IsNullOrEmpty(link.Title))
                    {
                        await _writer.WriteStringAsync(link.Title);
                    }

                    await _writer.WriteEndElementAsync();
                    break;

                default:
                    break;
            }
        }

        public virtual Task WritePerson(ISyndicationPerson person)
        {
            if (string.IsNullOrEmpty(person.Email))
            {
                throw new FormatException("Person does not contain an email");
            }

            if(person.RelationshipType == Rss20Constants.AuthorTag)
            {
                return _writer.WriteElementStringAsync(null,Rss20Constants.AuthorTag,null,person.Email);
            }
            else if(person.RelationshipType == Rss20Constants.ManagingEditorTag)
            {
                return _writer.WriteElementStringAsync(null, Rss20Constants.ManagingEditorTag, null, person.Email);
            }

            throw new FormatException("The relationship type is not recognized");
        }

        public Task WriteElementString(string name, string value)
        {
            return _writer.WriteElementStringAsync(null,name,null,value);
        }

        public async Task WriteStartDocument()
        {
            await _writer.WriteStartDocumentAsync();
            await _writer.WriteStartElementAsync(null, Rss20Constants.RssTag, null); // <Rss>
            await _writer.WriteAttributeStringAsync(null,Rss20Constants.VersionTag, null, Rss20Constants.Version); // <Rss version="2">
            await _writer.WriteStartElementAsync(null, Rss20Constants.ChannelTag, null); // <channel>
        }

        public async Task WriteEndDocument()
        {
            await _writer.WriteEndElementAsync(); //channel
            await _writer.WriteEndElementAsync(); //Rss
            await _writer.WriteEndDocumentAsync();//xml 
        }
    }
}
