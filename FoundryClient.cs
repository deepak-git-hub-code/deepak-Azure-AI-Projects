using Azure.Identity;

internal class FoundryClient
{
    private readonly DefaultAzureCredential _credential;

    public FoundryClient(DefaultAzureCredential credential)
    {
        _credential = credential;
    }

    public DefaultAzureCredential Credential => _credential;
}