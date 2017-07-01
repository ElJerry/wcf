// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

    internal delegate InlineCategoriesDocument CreateInlineCategoriesDelegate();
    internal delegate ReferencedCategoriesDocument CreateReferencedCategoriesDelegate();

    [XmlRoot(ElementName = App10Constants.Service, Namespace = App10Constants.Namespace)]
    public class AtomPub10ServiceDocumentFormatter : ServiceDocumentFormatter
    {
        private Type _documentType;
        private int _maxExtensionSize;
        private bool _preserveAttributeExtensions;
        private bool _preserveElementExtensions;

        public AtomPub10ServiceDocumentFormatter()
            : this(typeof(ServiceDocument))
        {
        }

        public AtomPub10ServiceDocumentFormatter(Type documentTypeToCreate)
            : base()
        {
            if (documentTypeToCreate == null)
            {
                throw new ArgumentNullException("documentTypeToCreate");
            }
            if (!typeof(ServiceDocument).IsAssignableFrom(documentTypeToCreate))
            {
                throw new ArgumentException(String.Format(SR.InvalidObjectTypePassed, "documentTypeToCreate", "ServiceDocument"));
            }
            _maxExtensionSize = int.MaxValue;
            _preserveAttributeExtensions = true;
            _preserveElementExtensions = true;
            _documentType = documentTypeToCreate;
        }

        public AtomPub10ServiceDocumentFormatter(ServiceDocument documentToWrite)
            : base(documentToWrite)
        {
            // No need to check that the parameter passed is valid - it is checked by the c'tor of the base class
            _maxExtensionSize = int.MaxValue;
            _preserveAttributeExtensions = true;
            _preserveElementExtensions = true;
            _documentType = documentToWrite.GetType();
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

            XmlReaderWrapper readerWrapper = XmlReaderWrapper.CreateFromReader(reader);
            return await readerWrapper.IsStartElementAsync(App10Constants.Service, App10Constants.Namespace);
        }

        async Task ReadXml(XmlReaderWrapper reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            await ReadDocumentAsync(reader);
        }

        async Task WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (this.Document == null)
            {
                throw new InvalidOperationException(SR.DocumentFormatterDoesNotHaveDocument);
            }
            await WriteDocumentAsync(XmlWriterWrapper.CreateFromWriter(writer));
        }

        public override async Task ReadFromAsync(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            XmlReaderWrapper wrappedReader = XmlReaderWrapper.CreateFromReader(reader);
            await wrappedReader.MoveToContentAsync();

            if (!await CanReadAsync(reader))
            {
                throw new XmlException(String.Format(SR.UnknownDocumentXml, reader.LocalName, reader.NamespaceURI));
            }

            await ReadDocumentAsync(wrappedReader);
        }

        public override async Task WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (this.Document == null)
            {
                throw new InvalidOperationException(SR.DocumentFormatterDoesNotHaveDocument);
            }

            XmlWriterWrapper wrappedWriter = XmlWriterWrapper.CreateFromWriter(writer);

            await wrappedWriter.WriteStartElementAsync(App10Constants.Prefix, App10Constants.Service, App10Constants.Namespace);
            await WriteDocumentAsync(wrappedWriter);
            await wrappedWriter.WriteEndElementAsync();
        }

        internal static async Task<CategoriesDocument> ReadCategories(XmlReaderWrapper reader, Uri baseUri, CreateInlineCategoriesDelegate inlineCategoriesFactory, CreateReferencedCategoriesDelegate referencedCategoriesFactory, string version, bool preserveElementExtensions, bool preserveAttributeExtensions, int maxExtensionSize)
        {
            string link = reader.GetAttribute(App10Constants.Href, string.Empty);
            if (string.IsNullOrEmpty(link))
            {
                InlineCategoriesDocument inlineCategories = inlineCategoriesFactory();
                await ReadInlineCategoriesAsync(reader, inlineCategories, baseUri, version, preserveElementExtensions, preserveAttributeExtensions, maxExtensionSize);
                return inlineCategories;
            }
            else
            {
                ReferencedCategoriesDocument referencedCategories = referencedCategoriesFactory();
                await ReadReferencedCategoriesAsync(reader, referencedCategories, baseUri, new Uri(link, UriKind.RelativeOrAbsolute), version, preserveElementExtensions, preserveAttributeExtensions, maxExtensionSize);
                return referencedCategories;
            }
        }



        internal static async Task WriteCategoriesInnerXml(XmlWriter writer, CategoriesDocument categories, Uri baseUri, string version)
        {
            Uri baseUriToWrite = FeedUtils.GetBaseUriToWrite(baseUri, categories.BaseUri);
            if (baseUriToWrite != null)
            {
                WriteXmlBase(writer, baseUriToWrite);
            }
            if (!string.IsNullOrEmpty(categories.Language))
            {
                WriteXmlLang(writer, categories.Language);
            }
            if (categories.IsInline)
            {
                await WriteInlineCategoriesContentAsync(XmlWriterWrapper.CreateFromWriter(writer), (InlineCategoriesDocument)categories, version);
            }
            else
            {
                WriteReferencedCategoriesContent(writer, (ReferencedCategoriesDocument)categories, version);
            }
        }

        protected override ServiceDocument CreateDocumentInstance()
        {
            if (_documentType == typeof(ServiceDocument))
            {
                return new ServiceDocument();
            }
            else
            {
                return (ServiceDocument)Activator.CreateInstance(_documentType);
            }
        }
        
        private static async Task ReadInlineCategoriesAsync(XmlReaderWrapper reader, InlineCategoriesDocument inlineCategories, Uri baseUri, string version, bool preserveElementExtensions, bool preserveAttributeExtensions, int maxExtensionSize)
        {
            inlineCategories.BaseUri = baseUri;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if (reader.LocalName == "base" && reader.NamespaceURI == Atom10FeedFormatter.XmlNs)
                    {
                        inlineCategories.BaseUri = FeedUtils.CombineXmlBase(inlineCategories.BaseUri, reader.Value);
                    }
                    else if (reader.LocalName == "lang" && reader.NamespaceURI == Atom10FeedFormatter.XmlNs)
                    {
                        inlineCategories.Language = reader.Value;
                    }
                    else if (reader.LocalName == App10Constants.Fixed && reader.NamespaceURI == string.Empty)
                    {
                        inlineCategories.IsFixed = (reader.Value == "yes");
                    }
                    else if (reader.LocalName == Atom10Constants.SchemeTag && reader.NamespaceURI == string.Empty)
                    {
                        inlineCategories.Scheme = reader.Value;
                    }
                    else
                    {
                        string ns = reader.NamespaceURI;
                        string name = reader.LocalName;
                        if (FeedUtils.IsXmlns(name, ns) || FeedUtils.IsXmlSchemaType(name, ns))
                        {
                            continue;
                        }
                        string val = reader.Value;
                        if (!TryParseAttribute(name, ns, val, inlineCategories, version))
                        {
                            if (preserveAttributeExtensions)
                            {
                                inlineCategories.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                            }
                        }
                    }
                }
            }
            await SyndicationFeedFormatter.MoveToStartElementAsync(reader);
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmptyElement)
            {
                XmlBuffer buffer = null;
                XmlDictionaryWriter extWriter = null;
                try
                {
                    while (reader.IsStartElement())
                    {
                        if (reader.IsStartElement(Atom10Constants.CategoryTag, Atom10Constants.Atom10Namespace))
                        {
                            SyndicationCategory category = CreateCategory(inlineCategories);
                            await Atom10FeedFormatter.ReadCategoryAsync(reader, category, version, preserveAttributeExtensions, preserveElementExtensions, maxExtensionSize);
                            if (category.Scheme == null)
                            {
                                category.Scheme = inlineCategories.Scheme;
                            }
                            inlineCategories.Categories.Add(category);
                        }
                        else if (!TryParseElement(reader, inlineCategories, version))
                        {
                            if (preserveElementExtensions)
                            {
                                if (buffer == null)
                                {
                                    buffer = new XmlBuffer(maxExtensionSize);
                                    extWriter = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
                                    extWriter.WriteStartElement(Rss20Constants.ExtensionWrapperTag);
                                }

                                await XmlReaderWrapper.WriteNodeAsync(extWriter, reader, false);
                            }
                            else
                            {
                                await reader.SkipAsync();
                            }
                        }
                    }
                    LoadElementExtensions(buffer, extWriter, inlineCategories);
                }
                finally
                {
                    if (extWriter != null)
                    {
                        extWriter.Close();
                    }
                }
                reader.ReadEndElement();
            }
        }
        
        private static async Task ReadReferencedCategoriesAsync(XmlReaderWrapper reader, ReferencedCategoriesDocument referencedCategories, Uri baseUri, Uri link, string version, bool preserveElementExtensions, bool preserveAttributeExtensions, int maxExtensionSize)
        {
            referencedCategories.BaseUri = baseUri;
            referencedCategories.Link = link;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if (reader.LocalName == "base" && reader.NamespaceURI == Atom10FeedFormatter.XmlNs)
                    {
                        referencedCategories.BaseUri = FeedUtils.CombineXmlBase(referencedCategories.BaseUri, reader.Value);
                    }
                    else if (reader.LocalName == "lang" && reader.NamespaceURI == Atom10FeedFormatter.XmlNs)
                    {
                        referencedCategories.Language = reader.Value;
                    }
                    else if (reader.LocalName == App10Constants.Href && reader.NamespaceURI == string.Empty)
                    {
                        continue;
                    }
                    else
                    {
                        string ns = reader.NamespaceURI;
                        string name = reader.LocalName;
                        if (FeedUtils.IsXmlns(name, ns) || FeedUtils.IsXmlSchemaType(name, ns))
                        {
                            continue;
                        }
                        string val = reader.Value;
                        if (!TryParseAttribute(name, ns, val, referencedCategories, version))
                        {
                            if (preserveAttributeExtensions)
                            {
                                referencedCategories.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                            }
                        }
                    }
                }
            }
            reader.MoveToElement();
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmptyElement)
            {
                XmlBuffer buffer = null;
                XmlDictionaryWriter extWriter = null;
                try
                {
                    while (reader.IsStartElement())
                    {
                        if (!TryParseElement(reader, referencedCategories, version))
                        {
                            if (preserveElementExtensions)
                            {
                                if (buffer == null)
                                {
                                    buffer = new XmlBuffer(maxExtensionSize);
                                    extWriter = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
                                    extWriter.WriteStartElement(Rss20Constants.ExtensionWrapperTag);
                                }

                                await XmlReaderWrapper.WriteNodeAsync(extWriter, reader, false);
                            }
                        }
                    }
                    LoadElementExtensions(buffer, extWriter, referencedCategories);
                }
                finally
                {
                    if (extWriter != null)
                    {
                        extWriter.Close();
                    }
                }
                reader.ReadEndElement();
            }
        }
        
        private static async Task WriteCategories(XmlWriterWrapper writer, CategoriesDocument categories, Uri baseUri, string version)
        {
            await writer.WriteStartElementAsync(App10Constants.Prefix, App10Constants.Categories, App10Constants.Namespace);
            WriteCategoriesInnerXml(writer, categories, baseUri, version);
            await writer.WriteEndElementAsync();
        }
        
        private static async Task WriteInlineCategoriesContentAsync(XmlWriterWrapper writer, InlineCategoriesDocument categories, string version)
        {
            XmlWriterWrapper wrappedWriter = XmlWriterWrapper.CreateFromWriter(writer);
            if (!string.IsNullOrEmpty(categories.Scheme))
            {
                await writer.WriteAttributeStringAsync(Atom10Constants.SchemeTag, categories.Scheme);
            }
            // by default, categories are not fixed
            if (categories.IsFixed)
            {
                await writer.WriteAttributeStringAsync(App10Constants.Fixed, "yes");
            }
            WriteAttributeExtensions(writer, categories, version);

            for (int i = 0; i < categories.Categories.Count; ++i)
            {
                await Atom10FeedFormatter.WriteCategoryAsync(wrappedWriter, categories.Categories[i], version);
            }
            WriteElementExtensions(writer, categories, version);
        }

        private static void WriteReferencedCategoriesContent(XmlWriter writer, ReferencedCategoriesDocument categories, string version)
        {
            if (categories.Link != null)
            {
                writer.WriteAttributeString(App10Constants.Href, FeedUtils.GetUriString(categories.Link));
            }
            WriteAttributeExtensions(writer, categories, version);
            WriteElementExtensions(writer, categories, version);
        }

        private static void WriteXmlBase(XmlWriter writer, Uri baseUri)
        {
            writer.WriteAttributeString("xml", "base", Atom10FeedFormatter.XmlNs, FeedUtils.GetUriString(baseUri));
        }

        private static void WriteXmlLang(XmlWriter writer, string lang)
        {
            writer.WriteAttributeString("xml", "lang", Atom10FeedFormatter.XmlNs, lang);
        }
        
        private async Task<ResourceCollectionInfo> ReadCollection(XmlReaderWrapper reader, Workspace workspace)
        {
            ResourceCollectionInfo result = CreateCollection(workspace);
            result.BaseUri = workspace.BaseUri;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if (reader.LocalName == "base" && reader.NamespaceURI == Atom10FeedFormatter.XmlNs)
                    {
                        result.BaseUri = FeedUtils.CombineXmlBase(result.BaseUri, reader.Value);
                    }
                    else if (reader.LocalName == App10Constants.Href && reader.NamespaceURI == string.Empty)
                    {
                        result.Link = new Uri(reader.Value, UriKind.RelativeOrAbsolute);
                    }
                    else
                    {
                        string ns = reader.NamespaceURI;
                        string name = reader.LocalName;
                        if (FeedUtils.IsXmlns(name, ns) || FeedUtils.IsXmlSchemaType(name, ns))
                        {
                            continue;
                        }
                        string val = reader.Value;
                        if (!TryParseAttribute(name, ns, val, result, this.Version))
                        {
                            if (_preserveAttributeExtensions)
                            {
                                result.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                            }
                        }
                    }
                }
            }

            XmlBuffer buffer = null;
            XmlDictionaryWriter extWriter = null;

            reader.ReadStartElement();
            try
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(Atom10Constants.TitleTag, Atom10Constants.Atom10Namespace))
                    {
                        result.Title = await Atom10FeedFormatter.ReadTextContentFromAsync(reader, "//app:service/app:workspace/app:collection/atom:title[@type]", _preserveAttributeExtensions);
                    }
                    else if (reader.IsStartElement(App10Constants.Categories, App10Constants.Namespace))
                    {
                        result.Categories.Add(ReadCategories(reader,
                            result.BaseUri,
                            delegate ()
                            {
                                return CreateInlineCategories(result);
                            },

                            delegate ()
                            {
                                return CreateReferencedCategories(result);
                            },
                            this.Version,
                            _preserveElementExtensions,
                            _preserveAttributeExtensions,
                            _maxExtensionSize).Result);
                    }
                    else if (reader.IsStartElement(App10Constants.Accept, App10Constants.Namespace))
                    {
                        result.Accepts.Add(reader.ReadElementString());
                    }
                    else if (!TryParseElement(reader, result, this.Version))
                    {
                        if (_preserveElementExtensions)
                        {
                            if (buffer == null)
                            {
                                buffer = new XmlBuffer(_maxExtensionSize);
                                extWriter = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
                                extWriter.WriteStartElement(Rss20Constants.ExtensionWrapperTag);
                            }

                            await XmlReaderWrapper.WriteNodeAsync(extWriter, reader, false);
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                }
                LoadElementExtensions(buffer, extWriter, result);
            }
            finally
            {
                if (extWriter != null)
                {
                    extWriter.Close();
                }
            }
            reader.ReadEndElement();
            return result;
        }
        
        private async Task ReadDocumentAsync(XmlReaderWrapper reader)
        {
            ServiceDocument result = CreateDocumentInstance();
            try
            {
                await SyndicationFeedFormatter.MoveToStartElementAsync(reader);
                bool elementIsEmpty = reader.IsEmptyElement;
                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.LocalName == "lang" && reader.NamespaceURI == Atom10FeedFormatter.XmlNs)
                        {
                            result.Language = reader.Value;
                        }
                        else if (reader.LocalName == "base" && reader.NamespaceURI == Atom10FeedFormatter.XmlNs)
                        {
                            result.BaseUri = new Uri(reader.Value, UriKind.RelativeOrAbsolute);
                        }
                        else
                        {
                            string ns = reader.NamespaceURI;
                            string name = reader.LocalName;
                            if (FeedUtils.IsXmlns(name, ns) || FeedUtils.IsXmlSchemaType(name, ns))
                            {
                                continue;
                            }
                            string val = reader.Value;
                            if (!TryParseAttribute(name, ns, val, result, this.Version))
                            {
                                if (_preserveAttributeExtensions)
                                {
                                    result.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                                }
                            }
                        }
                    }
                }
                XmlBuffer buffer = null;
                XmlDictionaryWriter extWriter = null;

                reader.ReadStartElement();
                if (!elementIsEmpty)
                {
                    try
                    {
                        while (reader.IsStartElement())
                        {
                            if (reader.IsStartElement(App10Constants.Workspace, App10Constants.Namespace))
                            {
                                result.Workspaces.Add(ReadWorkspace(reader, result).Result);
                            }
                            else if (!TryParseElement(reader, result, this.Version))
                            {
                                if (_preserveElementExtensions)
                                {
                                    if (buffer == null)
                                    {
                                        buffer = new XmlBuffer(_maxExtensionSize);
                                        extWriter = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
                                        extWriter.WriteStartElement(Rss20Constants.ExtensionWrapperTag);
                                    }

                                    await XmlReaderWrapper.WriteNodeAsync(extWriter, reader, false);
                                }
                                else
                                {
                                    await reader.SkipAsync();
                                }
                            }
                        }
                        LoadElementExtensions(buffer, extWriter, result);
                    }
                    finally
                    {
                        if (extWriter != null)
                        {
                            extWriter.Close();
                        }
                    }
                }
                reader.ReadEndElement();
            }
            catch (FormatException e)
            {
                throw new XmlException(FeedUtils.AddLineInfo(reader, SR.ErrorParsingDocument), e);
            }
            catch (ArgumentException e)
            {
                new XmlException(FeedUtils.AddLineInfo(reader, SR.ErrorParsingDocument), e);
            }
            SetDocument(result);
        }
        
        private async Task<Workspace> ReadWorkspace(XmlReaderWrapper reader, ServiceDocument document)
        {
            Workspace result = CreateWorkspace(document);
            result.BaseUri = document.BaseUri;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if (reader.LocalName == "base" && reader.NamespaceURI == Atom10FeedFormatter.XmlNs)
                    {
                        result.BaseUri = FeedUtils.CombineXmlBase(result.BaseUri, reader.Value);
                    }
                    else
                    {
                        string ns = reader.NamespaceURI;
                        string name = reader.LocalName;
                        if (FeedUtils.IsXmlns(name, ns) || FeedUtils.IsXmlSchemaType(name, ns))
                        {
                            continue;
                        }
                        string val = reader.Value;
                        if (!TryParseAttribute(name, ns, val, result, this.Version))
                        {
                            if (_preserveAttributeExtensions)
                            {
                                result.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                            }
                        }
                    }
                }
            }

            XmlBuffer buffer = null;
            XmlDictionaryWriter extWriter = null;

            reader.ReadStartElement();
            try
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(Atom10Constants.TitleTag, Atom10Constants.Atom10Namespace))
                    {
                        result.Title = await Atom10FeedFormatter.ReadTextContentFromAsync(reader, "//app:service/app:workspace/atom:title[@type]", _preserveAttributeExtensions);
                    }
                    else if (reader.IsStartElement(App10Constants.Collection, App10Constants.Namespace))
                    {
                        result.Collections.Add(ReadCollection(reader, result).Result);
                    }
                    else if (!TryParseElement(reader, result, this.Version))
                    {
                        if (_preserveElementExtensions)
                        {
                            if (buffer == null)
                            {
                                buffer = new XmlBuffer(_maxExtensionSize);
                                extWriter = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
                                extWriter.WriteStartElement(Rss20Constants.ExtensionWrapperTag);
                            }

                            await XmlReaderWrapper.WriteNodeAsync(extWriter, reader, false);
                        }
                        else
                        {
                            await reader.SkipAsync();
                        }
                    }
                }
                LoadElementExtensions(buffer, extWriter, result);
            }
            finally
            {
                if (extWriter != null)
                {
                    extWriter.Close();
                }
            }
            reader.ReadEndElement();
            return result;
        }
        
        private async Task WriteCollectionAsync(XmlWriterWrapper writer, ResourceCollectionInfo collection, Uri baseUri)
        {
            await writer.WriteStartElementAsync(App10Constants.Prefix, App10Constants.Collection, App10Constants.Namespace);
            Uri baseUriToWrite = FeedUtils.GetBaseUriToWrite(baseUri, collection.BaseUri);
            if (baseUriToWrite != null)
            {
                baseUri = collection.BaseUri;
                WriteXmlBase(writer, baseUriToWrite);
            }
            if (collection.Link != null)
            {
                await writer.WriteAttributeStringAsync(App10Constants.Href, FeedUtils.GetUriString(collection.Link));
            }
            WriteAttributeExtensions(writer, collection, this.Version);
            if (collection.Title != null)
            {
                collection.Title.WriteTo(writer, Atom10Constants.TitleTag, Atom10Constants.Atom10Namespace);
            }
            for (int i = 0; i < collection.Accepts.Count; ++i)
            {
                await writer.WriteElementStringAsync(App10Constants.Prefix, App10Constants.Accept, App10Constants.Namespace, collection.Accepts[i]);
            }
            for (int i = 0; i < collection.Categories.Count; ++i)
            {
                await WriteCategories(writer, collection.Categories[i], baseUri, this.Version);
            }
            WriteElementExtensions(writer, collection, this.Version);
            await writer.WriteEndElementAsync();
        }
        
        private async Task WriteDocumentAsync(XmlWriterWrapper writer)
        {
            // declare the atom10 namespace upfront for compactness
            await writer.WriteAttributeStringAsync(Atom10Constants.Atom10Prefix, Atom10FeedFormatter.XmlNsNs, Atom10Constants.Atom10Namespace);
            if (!string.IsNullOrEmpty(this.Document.Language))
            {
                WriteXmlLang(writer, this.Document.Language);
            }
            Uri baseUri = this.Document.BaseUri;
            if (baseUri != null)
            {
                WriteXmlBase(writer, baseUri);
            }
            WriteAttributeExtensions(writer, this.Document, this.Version);

            for (int i = 0; i < this.Document.Workspaces.Count; ++i)
            {
                await WriteWorkspaceAsync(writer, this.Document.Workspaces[i], baseUri);
            }
            WriteElementExtensions(writer, this.Document, this.Version);
        }
        
        private async Task WriteWorkspaceAsync(XmlWriterWrapper writer, Workspace workspace, Uri baseUri)
        {
            await writer.WriteStartElementAsync(App10Constants.Prefix, App10Constants.Workspace, App10Constants.Namespace);
            Uri baseUriToWrite = FeedUtils.GetBaseUriToWrite(baseUri, workspace.BaseUri);
            if (baseUriToWrite != null)
            {
                baseUri = workspace.BaseUri;
                WriteXmlBase(writer, baseUriToWrite);
            }
            WriteAttributeExtensions(writer, workspace, this.Version);
            if (workspace.Title != null)
            {
                workspace.Title.WriteTo(writer, Atom10Constants.TitleTag, Atom10Constants.Atom10Namespace);
            }
            for (int i = 0; i < workspace.Collections.Count; ++i)
            {
                await WriteCollectionAsync(writer, workspace.Collections[i], baseUri);
            }
            WriteElementExtensions(writer, workspace, this.Version);
            await writer.WriteEndElementAsync();
        }
    }

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    [XmlRoot(ElementName = App10Constants.Service, Namespace = App10Constants.Namespace)]
    public class AtomPub10ServiceDocumentFormatter<TServiceDocument> : AtomPub10ServiceDocumentFormatter
        where TServiceDocument : ServiceDocument, new()
    {
        public AtomPub10ServiceDocumentFormatter() :
            base(typeof(TServiceDocument))
        {
        }

        public AtomPub10ServiceDocumentFormatter(TServiceDocument documentToWrite)
            : base(documentToWrite)
        {
        }

        protected override ServiceDocument CreateDocumentInstance()
        {
            return new TServiceDocument();
        }
    }
}