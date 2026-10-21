namespace ClaimsModule.Application.ClaimDocuments.Contracts;

public interface ILocalDocumentDownloads
{
    LocalDownload Open(string token);
}
