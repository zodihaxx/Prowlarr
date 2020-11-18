using System.Collections.Generic;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexer : IProvider
    {
        bool SupportsRss { get; }
        bool SupportsSearch { get; }
        IndexerCapabilities Capabilities { get; }

        DownloadProtocol Protocol { get; }
        IndexerPrivacy Privacy { get; }

        IList<ReleaseInfo> Fetch(MovieSearchCriteria searchCriteria);
    }
}
