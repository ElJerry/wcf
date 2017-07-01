// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


/*
 * Some diagnostic lines have been commented
 * 
 * */

namespace Microsoft.ServiceModel.Syndication
{
    using Microsoft.ServiceModel.Syndication.Resources;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    [XmlRoot(ElementName = Atom10Constants.EntryTag, Namespace = Atom10Constants.Atom10Namespace)]
    public class Atom10ItemFormatter : SyndicationItemFormatter
    {
        private Atom10FeedFormatter _feedSerializer;
        private Type _itemType;
        private bool _preserveAttributeExtensions;
        private bool _preserveElementExtensions;

        public Atom10ItemFormatter()
            : this(typeof(SyndicationItem))
        {
        }

        public Atom10ItemFormatter(Type itemTypeToCreate)
            : base()
        {
            if (itemTypeToCreate == null)
            {
                throw new ArgumentNullException("itemTypeToCreate");
            }
            if (!typeof(SyndicationItem).IsAssignableFrom(itemTypeToCreate))
            {
                throw new ArgumentException(String.Format(SR.InvalidObjectTypePassed, "itemTypeToCreate", "SyndicationItem"));
            }
            _feedSerializer = new Atom10FeedFormatter();
            _feedSerializer.PreserveAttributeExtensions = _preserveAttributeExtensions = true;
            _feedSerializer.PreserveElementExtensions = _preserveElementExtensions = true;
            _itemType = itemTypeToCreate;
        }

        public Atom10ItemFormatter(SyndicationItem itemToWrite)
            : base(itemToWrite)
        {
            // No need to check that the parameter passed is valid - it is checked by the c'tor of the base class
            _feedSerializer = new Atom10FeedFormatter();
            _feedSerializer.PreserveAttributeExtensions = _preserveAttributeExtensions = true;
            _feedSerializer.PreserveElementExtensions = _preserveElementExtensions = true;
            _itemType = itemToWrite.GetType();
        }

        public bool PreserveAttributeExtensions
        {
            get { return _preserveAttributeExtensions; }
            set
            {
                _preserveAttributeExtensions = value;
                _feedSerializer.PreserveAttributeExtensions = value;
            }
        }

        public bool PreserveElementExtensions
        {
            get { return _preserveElementExtensions; }
            set
            {
                _preserveElementExtensions = value;
                _feedSerializer.PreserveElementExtensions = value;
            }
        }

        public override string Version
        {
            get { return SyndicationVersions.Atom10; }
        }

        protected Type ItemType
        {
            get
            {
                return _itemType;
            }
        }

        public override bool CanRead(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            return reader.IsStartElement(Atom10Constants.EntryTag, Atom10Constants.Atom10Namespace);
        }

        public override async Task ReadFromAsync(XmlReader reader)
        {
            if (!CanRead(reader))
            {
                throw new XmlException(String.Format(SR.UnknownItemXml, reader.LocalName, reader.NamespaceURI));
            }

            await ReadItem(reader);
        }

        public override async Task WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            writer.WriteStartElement(Atom10Constants.EntryTag, Atom10Constants.Atom10Namespace);
            WriteItem(writer);
            writer.WriteEndElement();
        }

        protected override SyndicationItem CreateItemInstance()
        {
            return SyndicationItemFormatter.CreateItemInstance(_itemType);
        }
        
        private async Task ReadItem(XmlReader reader)
        {
            SetItem(CreateItemInstance());
            await _feedSerializer.ReadItemFrom(XmlReaderWrapper.CreateFromReader(XmlDictionaryReader.CreateDictionaryReader(reader)), this.Item);
        }

        private void WriteItem(XmlWriter writer)
        {
            if (this.Item == null)
            {
                throw new InvalidOperationException(SR.ItemFormatterDoesNotHaveItem);
            }
            XmlDictionaryWriter w = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            _feedSerializer.WriteItemContentsAsync(w, this.Item);
        }
    }

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    [XmlRoot(ElementName = Atom10Constants.EntryTag, Namespace = Atom10Constants.Atom10Namespace)]
    public class Atom10ItemFormatter<TSyndicationItem> : Atom10ItemFormatter
        where TSyndicationItem : SyndicationItem, new()
    {
        // constructors
        public Atom10ItemFormatter()
            : base(typeof(TSyndicationItem))
        {
        }
        public Atom10ItemFormatter(TSyndicationItem itemToWrite)
            : base(itemToWrite)
        {
        }

        protected override SyndicationItem CreateItemInstance()
        {
            return new TSyndicationItem();
        }
    }
}