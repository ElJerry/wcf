// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


namespace Microsoft.ServiceModel.Syndication
{

    using Microsoft.ServiceModel.Syndication.Resources;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    [XmlRoot(ElementName = App10Constants.Categories, Namespace = App10Constants.Namespace)]
    public class AtomPub10CategoriesDocumentFormatter : CategoriesDocumentFormatter
    {
        private Type _inlineDocumentType;
        private int _maxExtensionSize;
        private bool _preserveAttributeExtensions;
        private bool _preserveElementExtensions;
        private Type _referencedDocumentType;

        public AtomPub10CategoriesDocumentFormatter()
            : this(typeof(InlineCategoriesDocument), typeof(ReferencedCategoriesDocument))
        {
        }

        public AtomPub10CategoriesDocumentFormatter(Type inlineDocumentType, Type referencedDocumentType)
            : base()
        {
            if (inlineDocumentType == null)
            {
                throw new ArgumentNullException("inlineDocumentType");
            }
            if (!typeof(InlineCategoriesDocument).IsAssignableFrom(inlineDocumentType))
            {
                throw new ArgumentException(String.Format(SR.InvalidObjectTypePassed, "inlineDocumentType", "InlineCategoriesDocument"));
            }
            if (referencedDocumentType == null)
            {
                throw new ArgumentNullException("referencedDocumentType");
            }
            if (!typeof(ReferencedCategoriesDocument).IsAssignableFrom(referencedDocumentType))
            {
                throw new ArgumentException(String.Format(SR.InvalidObjectTypePassed, "referencedDocumentType", "ReferencedCategoriesDocument"));
            }
            _maxExtensionSize = int.MaxValue;
            _preserveAttributeExtensions = true;
            _preserveElementExtensions = true;
            _inlineDocumentType = inlineDocumentType;
            _referencedDocumentType = referencedDocumentType;
        }

        public AtomPub10CategoriesDocumentFormatter(CategoriesDocument documentToWrite)
            : base(documentToWrite)
        {
            // No need to check that the parameter passed is valid - it is checked by the c'tor of the base class
            _maxExtensionSize = int.MaxValue;
            _preserveAttributeExtensions = true;
            _preserveElementExtensions = true;
            if (documentToWrite.IsInline)
            {
                _inlineDocumentType = documentToWrite.GetType();
                _referencedDocumentType = typeof(ReferencedCategoriesDocument);
            }
            else
            {
                _referencedDocumentType = documentToWrite.GetType();
                _inlineDocumentType = typeof(InlineCategoriesDocument);
            }
        }

        public override string Version
        {
            get { return App10Constants.Namespace; }
        }

        public override async Task<bool> CanReadAsync(XmlReader reader)
        {            
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            XmlReaderWrapper wrappedReader = XmlReaderWrapper.CreateFromReader(reader);
            return await wrappedReader.IsStartElementAsync(App10Constants.Categories, App10Constants.Namespace);
        }

        

        async Task ReadXmlAsync(XmlReaderWrapper reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            await ReadDocumentAsync(reader);
        }

        void WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (this.Document == null)
            {
                throw new InvalidOperationException(SR.DocumentFormatterDoesNotHaveDocument);
            }
            WriteDocument(writer);
        }

        public override async Task ReadFrom(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (!await CanReadAsync(reader))
            {
                throw new XmlException(String.Format(SR.UnknownDocumentXml, reader.LocalName, reader.NamespaceURI));
            }

            await ReadDocumentAsync(XmlReaderWrapper.CreateFromReader(reader));
        }

        public override void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (this.Document == null)
            {
                throw new InvalidOperationException(SR.DocumentFormatterDoesNotHaveDocument);
            }
            writer.WriteStartElement(App10Constants.Prefix, App10Constants.Categories, App10Constants.Namespace);
            WriteDocument(writer);
            writer.WriteEndElement();
        }

        protected override InlineCategoriesDocument CreateInlineCategoriesDocument()
        {
            if (_inlineDocumentType == typeof(InlineCategoriesDocument))
            {
                return new InlineCategoriesDocument();
            }
            else
            {
                return (InlineCategoriesDocument)Activator.CreateInstance(_inlineDocumentType);
            }
        }

        protected override ReferencedCategoriesDocument CreateReferencedCategoriesDocument()
        {
            if (_referencedDocumentType == typeof(ReferencedCategoriesDocument))
            {
                return new ReferencedCategoriesDocument();
            }
            else
            {
                return (ReferencedCategoriesDocument)Activator.CreateInstance(_referencedDocumentType);
            }
        }
        
        private async Task ReadDocumentAsync(XmlReaderWrapper reader)
        {
            try
            {
                await SyndicationFeedFormatter.MoveToStartElementAsync(reader);
                SetDocument(AtomPub10ServiceDocumentFormatter.ReadCategories(reader, null,
                    delegate ()
                    {
                        return this.CreateInlineCategoriesDocument();
                    },

                    delegate ()
                    {
                        return this.CreateReferencedCategoriesDocument();
                    },
                    this.Version,
                    _preserveElementExtensions,
                    _preserveAttributeExtensions,
                    _maxExtensionSize).Result);
            }
            catch (FormatException e)
            {
                throw new XmlException(FeedUtils.AddLineInfo(reader, SR.ErrorParsingDocument), e);
            }
            catch (ArgumentException e)
            {
                throw new XmlException(FeedUtils.AddLineInfo(reader, SR.ErrorParsingDocument), e);
            }
        }

        private void WriteDocument(XmlWriter writer)
        {
            // declare the atom10 namespace upfront for compactness
            writer.WriteAttributeString(Atom10Constants.Atom10Prefix, Atom10FeedFormatter.XmlNsNs, Atom10Constants.Atom10Namespace);
            AtomPub10ServiceDocumentFormatter.WriteCategoriesInnerXml(writer, this.Document, null, this.Version);
        }
    }
}