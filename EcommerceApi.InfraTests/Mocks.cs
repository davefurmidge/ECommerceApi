using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Pulumi.Testing;

namespace EcommerceApi.InfraTests
{
    class Mocks : IMocks
    { 
        public Task<(string? id, object state)> NewResourceAsync(MockResourceArgs args)
        {
            var outputs = ImmutableDictionary.CreateBuilder<string, object>();

            // Forward all input parameters as resource outputs, so that we could test them.
            outputs.AddRange(args.Inputs);

            // <-- We'll customize the mocks here
            // Set the name to resource name if it's not set explicitly in inputs.
            if (!args.Inputs.ContainsKey("name"))
            {
                outputs.Add("name", args.Name!);
            }
            if (args.Type == "azure-native:web:WebApp")
                    
            {
                // ... set its web endpoint property.
                // Normally this would be calculated by Azure, so we have to mock it.
                outputs.Add("defaultHostName", "https://ecommerce-api-furmidge.azurewebsites.net");
          
            }
            if (args.Type == "azure-native:storage:StorageAccount")
            {
                // ... set its web endpoint property.
                // Normally this would be calculated by Azure, so we have to mock it.
                outputs.Add("primaryEndpoints", new Dictionary<string, object?>
                {
                    {"blob", "BlobEndpoint"},
                    {"dfs", "DfsEndpoint"},
                    {"file", "FileEndpoint"},
                    {"internetEndpoints", null},
                    {"microsoftEndpoints", null},
                    {"queue", "QueueEndpoint"},
                    {"table", "TableEndpoint"},
                    {"web", $"https://{args.Name}.web.core.windows.net"},
                });
            }

            // Default the resource ID to `{name}_id`.
            args.Id ??= $"{args.Name}_id";
            return Task.FromResult((args.Id, (object)outputs));
        }

        public Task<object> CallAsync(MockCallArgs args)
        {
            // We don't use this method in this particular test suite.
            // Default to returning whatever we got as input.
            return Task.FromResult((object)args.Args);
        }
    }
}
