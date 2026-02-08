using System;
using System.Threading.Tasks;
using Azure.AI.Projects;
using Azure.Identity;
using Azure.Core;
using System.ClientModel.Primitives;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        // 1. Azure AI endpoint
        var endpoint = new Uri(
            "https://resource.services.ai.azure.com/api/projects/yourproject"
        );

        // 2. Use DefaultAzureCredential (picks up your MFA CLI login automatically)
        var cred = new DefaultAzureCredential();

        // 3. Create Foundry client
        var client = new AIProjectClient(endpoint, cred);
        Console.WriteLine("Client created");

        // 4. List deployments
        await foreach (var deployment in client.Deployments.GetDeploymentsAsync())
        {
            Console.WriteLine($"Deployment: {deployment.Name}");
        }
    }
}
