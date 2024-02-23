using Pulumi;
using Pulumi.AzureNative.Logic.V20160601;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using System.Collections.Generic;

namespace EcommerceApi.Infra.components
{
    public class ecResourceGroupArgs : Pulumi.ResourceArgs
    {
        public Input<string> ResourceGroupName { get; set; }
        public Input<string> Location { get; set; }
        public InputMap<string>? Tags { get; set; }
    }


    public class ecResourceGroup : ComponentResource
    {
        public Output<string> ResourceGroupName { get; private set; }
        public ecResourceGroup(string name, ecResourceGroupArgs args,
         ComponentResourceOptions options = null)
         : base("dfurmidge:azure:resourcegroup", name, options)
        {


            var resourceGroup = new ResourceGroup($"{name}-rg", new ResourceGroupArgs
            {
                ResourceGroupName = args.ResourceGroupName,
                Location = args.Location,
                Tags = args.Tags,
            });

            this.ResourceGroupName = resourceGroup.Name;
          
      
        }
    }
}