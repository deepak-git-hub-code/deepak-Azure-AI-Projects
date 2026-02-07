using Azure.Identity;

internal class FoundryClient
{
    private DefaultAzureCredential cred;

    public FoundryClient(DefaultAzureCredential cred)
    {
        this.cred = cred;
    }
}