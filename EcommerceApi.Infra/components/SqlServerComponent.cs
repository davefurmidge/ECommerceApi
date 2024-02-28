using Pulumi;
using Random = Pulumi.Random;
using Pulumi.AzureNative.Sql;
using Pulumi.AzureNative.Sql.Inputs;
using System.Collections.Generic;

namespace EcommerceApi.Infra.components
{
    public class ecSqlServerArgs : ResourceArgs
    {
        public Input<string> ServerName { get; set; }
        public Input<string> ResourceGroup { get; set; }
        public Input<string> ApplicationName { get; set; }
        public Input<string> Environment { get; set; }
        public Input<string> Location { get; set; }
        public Input<string> AdministratorLogin { get; set; }
    }

    public class ecSqlServer : ComponentResource
    {
        [Output("databaseUrl")]
        public Output<string>? DatabaseUrl { get; private set; }

        [Output("databaseUsername")]
        public Output<string>? DatabaseUsername { get; private set; }

        [Output("databasePassword")]
        public Output<string>? DatabasePassword { get; private set; }

        [Output("connectionString")]
        public Output<string>? ConnectionString { get; private set; }

        [Output("fullyQualifiedDomainName")]
        public Output<string>? FullyQualifiedDomainName { get; private set; }

        public ecSqlServer(string name, ecSqlServerArgs args, ComponentResourceOptions? options = null)
            : base("dfurmidge:azure:sqlserver", name, options)
        {
            var myIpRanges = new[]
            {
            "0.0.0.0",
            "86.22.232.57",
        };

            var password = new Random.RandomPassword($"{name}-password", new Random.RandomPasswordArgs
            {
                Length = 32,
                Special = true,
                OverrideSpecial = "_%@",
            }, new CustomResourceOptions
            {
                Parent = this,
            });

            var database = new Server($"{name}-database", new ServerArgs
            {
                ServerName = args.ServerName,
                ResourceGroupName = args.ResourceGroup,
                Location = args.Location,
                Version = "12.0",
                AdministratorLogin = args.AdministratorLogin,
                AdministratorLoginPassword = password.Result,
           
                Tags =
            {
                { "environment", args.Environment },
                { "application-name", args.ApplicationName },
            },
            }, new CustomResourceOptions
            {
                Parent = this,
            });

            var databaseResource = new Database($"{name}-database", new DatabaseArgs
            {
                DatabaseName = args.ServerName.Apply(serverName => $"{serverName}-asp"),
                ServerName = database.Name,
                Collation = "SQL_Latin1_General_CP1_CI_AS",
                ResourceGroupName = args.ResourceGroup,
                Location = args.Location,
                Sku = new SkuArgs
                {
                    // Capacity = 2,
                    // Family = "Gen5",
                    // Name = "BC",
                    Name = "S3",
                    Tier = "Standard"
                },
            }, new CustomResourceOptions
            {
                Parent = this,
            });

            var rules = new List<FirewallRule>();
            foreach (var range in myIpRanges)
            {
                rules.Add(new FirewallRule($"{name}-rules-{range}", new FirewallRuleArgs
                {
                    ResourceGroupName = args.ResourceGroup,
                    ServerName = database.Name,
                    StartIpAddress = range,
                    EndIpAddress = range
                }));
            }

            DatabaseUrl = Output.Tuple(database.Name, database.FullyQualifiedDomainName).Apply(values =>
            {
                var databaseName = values.Item1;
                var fullyQualifiedDomainName = values.Item2;
                return $"{fullyQualifiedDomainName}:1433;database={databaseName};encrypt=true;trustServerCertificate=false;hostNameInCertificate=*.database.windows.net;loginTimeout=30;";
            });

            DatabaseUsername = database.AdministratorLogin;

            DatabasePassword = password.Result.Apply(p => p ?? "");

            ConnectionString = Output.Tuple(database.Name, databaseResource.Name, database.AdministratorLogin, password.Result).Apply(values =>
            {
                var databaseName = values.Item1;
                var databaseResourceName = values.Item2;
                var administratorLogin = values.Item3;
                var administratorLoginPassword = values.Item4;
                return $"Server=tcp:{databaseName}.database.windows.net,1433;Initial Catalog={databaseResourceName};Persist Security Info=False;User ID={administratorLogin};Password={administratorLoginPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            });

            FullyQualifiedDomainName = database.FullyQualifiedDomainName;

            RegisterOutputs(new Dictionary<string, object?>
        {
            { "databaseUrl", DatabaseUrl },
            { "databaseUsername", DatabaseUsername },
            { "databasePassword", DatabasePassword },
            { "connectionString", ConnectionString },
            { "fullyQualifiedDomainName", FullyQualifiedDomainName },
        });
        }
    }

}
