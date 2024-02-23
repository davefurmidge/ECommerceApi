using Pulumi;

namespace EcommerceApi.InfraTests
{
    public static class OutputExtensions
    {
        /// <summary>
        /// Extract the value from an output.
        /// </summary>
        public static Task<T> GetValueAsync<T>(this Output<T> output)
        {
            var tcs = new TaskCompletionSource<T>();
            output.Apply(v =>
            {
                tcs.SetResult(v);
                return v;
            });
            return tcs.Task;
        }
    }
}
