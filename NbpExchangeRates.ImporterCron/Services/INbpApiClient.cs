using NbpImporter.Nbp;

namespace Nbp.Rates.Importer.Services;

public interface INbpApiClient
{
    Task<List<NbpTableResponse>> GetTableBAsync();
}