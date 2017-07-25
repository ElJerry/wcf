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

        public virtual async Task WriteCategory(ISyndicationCategory category)
        {
            string name = category.Name;            
            await _writer.WriteElementStringAsync(null,Rss20Constants.CategoryTag,null,name);            
        }

        public virtual async Task WriteContent(ISyndicationContent content)
        {
            await _writer.WriteRawAsync(content.RawContent);
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



        }

        public virtual async Task WriteLink(ISyndicationLink link)
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

                default:
                    break;
            }
        }

        public virtual async Task WritePerson(ISyndicationPerson person)
        {
            await _writer.WriteElementStringAsync(null,Rss20Constants.AuthorTag,null,person.Email);
        }

        public async Task WriteStartDocument()
        {
            await _writer.WriteStartDocumentAsync();
            await _writer.WriteStartElementAsync(null, Rss20Constants.RssTag, null);
            await _writer.WriteAttributeStringAsync(null,Rss20Constants.VersionTag, null, Rss20Constants.Version);
            await _writer.WriteStartElementAsync(null, Rss20Constants.ChannelTag, null);
        }

        public async Task WriteEndDocument()
        {
            await _writer.WriteEndElementAsync(); //channel
            await _writer.WriteEndElementAsync(); //Rss
            await _writer.WriteEndDocumentAsync();//xml 
        }
    }
}
