using System.Collections.Immutable;
using EcommerceApi.Infra;
using Pulumi.AzureNative.Resources;
using Pulumi.Testing;
using Shouldly;
using Xunit;

namespace EcommerceApi.InfraTests
{
    public class WebServerTests
    {
        private static Task<ImmutableArray<Pulumi.Resource>> TestAsync()
        {
            return Pulumi.Deployment.TestAsync<WebServerStack>(new Mocks(), new TestOptions { IsPreview = false });
        }

        [Fact]
        public async Task ResourceGroup_ShouldExist()
        {
            var resources = await TestAsync();
            var resourceGroups = resources.OfType<ResourceGroup>().ToList();
            resourceGroups.Count.ShouldBe(1, "Website resource group expected");
        }

        [Fact]
        public async Task ResourceGroup_ShouldHasEnvironmentTag()
        {
            var resources = await TestAsync();
            var resourceGroup = resources.OfType<ResourceGroup>().First();

            var tags = await resourceGroup.Tags.GetValueAsync();
            tags.ShouldNotBeNull("Tags must be defined");
            tags.ShouldContainKey("environment");
        }

        [Fact]
        public async Task ShouldExportsWebsiteUrl()
        {
            var resources = await TestAsync();
            var stack = resources.OfType<WebServerStack>().First();

            var endpoint = await stack.Endpoint.GetValueAsync();
            endpoint.ShouldBe("https://ecommerce-api-furmidge.azurewebsites.net");
          
        }
    }
}
