using Pulumi;
using Pulumi.AzureNative.Resources;

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