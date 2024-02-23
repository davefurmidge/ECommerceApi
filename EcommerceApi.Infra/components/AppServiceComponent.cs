using Pulumi;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;

namespace EcommerceApi.Infra.components
{
    public class ecAppServiceArgs : ResourceArgs
    {
        public Input<string> ResourceGroup { get; set; }
        public Input<string> ApplicationName { get; set; }
        public Input<string> Environment { get; set; } = "dev";
        public Input<string> Location { get; set; }
        public Input<string> DatabaseUrl { get; set; }
        public Input<string> DatabaseUsername { get; set; }
        public Input<string> DatabasePassword { get; set; }
        public Input<string> DatabaseConnstr { get; set; } = null;
    }
    public class ecAppService : ComponentResource
    {
        [Output("defaultHostName")]
        public Output<string> DefaultHostName { get; private set; }

        public ecAppService(string name, ecAppServiceArgs args,
         ComponentResourceOptions options = null)
         : base("dfurmidge:azure:webapp", name, options)
        {

            var appServicePlan = new AppServicePlan($"{name}-asp", new AppServicePlanArgs
            {
                Name = args.ApplicationName.Apply(applicationName => $"{applicationName}-asp"),
                ResourceGroupName = args.ResourceGroup,
                Location = args.Location,
                Sku = new SkuDescriptionArgs
                {
                    Name = "B3",
                    Tier = "Basic",
                    Capacity = 1
                },
                Kind = "Linux",
                Tags =
            {
                {"environment", args.Environment},
                {"application-name", args.ApplicationName}
            }
            });

            var webApp = new WebApp($"{name}-app", new WebAppArgs
            {
                Name = args.ApplicationName.Apply(applicationName => $"{applicationName}-app"),
                ResourceGroupName = args.ResourceGroup,
                Location = args.Location,
                ServerFarmId = appServicePlan.Id,
                HttpsOnly = true,
                Tags =
                {
                    {"environment", args.Environment},
                    {"application-name", args.ApplicationName}
                },
                SiteConfig = new SiteConfigArgs
                {

                    AppSettings = new[]
                    {
                    // Settings for the Azure Web App, for example, here we set ASP.NET Core environment to Development
                    new NameValuePairArgs
                       {
                           Name = "ASPNETCORE_ENVIRONMENT",
                           Value = "Production"
                       },
                    new NameValuePairArgs
                       {
                           Name = "WEBSITES_ENABLE_APP_SERVICE_STORAGE",
                           Value = "false"
                       },
                    new NameValuePairArgs
                       {
                           Name = "DATABASE_URL",
                           Value = args.DatabaseUrl
                       },
                    new NameValuePairArgs
                       {
                           Name = "DATABASE_USERNAME",
                           Value = args.DatabaseUsername
                       },
                     new NameValuePairArgs
                       {
                           Name = "DATABASE_PASSWORD",
                           Value = args.DatabasePassword
                       }
                    },
                    ConnectionStrings =
                    {
                        new ConnStringInfoArgs
                        {
                             Name = "AZURE_SQL_CONNECTIONSTRING",
                             Type = ConnectionStringType.SQLAzure,
                             ConnectionString = args.DatabaseConnstr,
                        }
                    }
                }
            });

          
            //DefaultHostName = Output.Format($"https://{webApp.DefaultHostName}");

            DefaultHostName = webApp.DefaultHostName;
        }

    }


}
