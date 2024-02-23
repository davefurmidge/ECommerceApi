using Pulumi;
using EcommerceApi.Infra.components;
using Github = Pulumi.Github;

namespace EcommerceApi.Infra
{
    public class WebServerStack : Stack
    {
        public WebServerStack()
        {
            var config = new Config();
            var applicationName = config.Get("applicationName") ?? "ecommerce-api-furmidge";
            var appenvironment = config.Get("appenvironment") ?? "dev";
            var applocation = config.Get("location") ?? "northeurope";

            var resourceGroupName = $"rg-{applicationName}-{appenvironment}"; // Dynamic resource group name
            var serverName = $"{applicationName}-{appenvironment}";

            // Create the resource group
            var resourceGroup = new ecResourceGroup("resourceGroup", new()
            {
                ResourceGroupName = resourceGroupName,
                Location = applocation,
                Tags = new InputMap<string>
                {
                    {"environment", "dev"},
                    {"application-name", "ecommerceapi"}
                }

            });
    

            var databaseComponent = new ecSqlServer("ecommerceapi", new ecSqlServerArgs
            {
                ServerName = serverName,
                ResourceGroup = resourceGroup.ResourceGroupName,
                ApplicationName = applicationName,
                Environment = appenvironment,
                Location = applocation,
                AdministratorLogin = "sqladmin",
            });

            var appService = new ecAppService("ecommerceapi", new ecAppServiceArgs
            {
                ResourceGroup = resourceGroup.ResourceGroupName,
                ApplicationName = applicationName,
                Environment = appenvironment,
                Location = applocation,
                DatabaseUrl = databaseComponent.DatabaseUrl,
                DatabaseUsername = databaseComponent.DatabaseUsername,
                DatabasePassword = databaseComponent.DatabasePassword,
                DatabaseConnstr = databaseComponent.ConnectionString,
            });

            var azureConnstrSecret = new Github.ActionsSecret("azure_connstr", new Github.ActionsSecretArgs
            {
                Repository = "ECommerceApi",
                SecretName = "AZURE_SQL_CONNECTIONSTRING",
                PlaintextValue = databaseComponent.ConnectionString,

            });
            // Set the output for the App Service DefaultHostName

            this.Endpoint = appService.DefaultHostName;

        }
        [Output] public Output<string> Endpoint { get; set; }

    }
}
