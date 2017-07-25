// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

namespace Microsoft.SyndicationFeed
{
    public interface ISyndicationFeedWriter
    {
        Task WriteItem(ISyndicationItem item);

        Task WriteLink(ISyndicationLink link);

        Task WritePerson(ISyndicationPerson person);

        Task WriteCategory(ISyndicationCategory category);

        Task WriteContent(ISyndicationContent content);

        Task WriteStartDocument();

        Task WriteElementString(string name, string value);

        Task WriteEndDocument();
    }
}
