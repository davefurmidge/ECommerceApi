using System.Threading.Tasks;
using Pulumi;
using EcommerceApi.Infra;

class Program
{
    static Task<int> Main() => Deployment.RunAsync<WebServerStack>();
}