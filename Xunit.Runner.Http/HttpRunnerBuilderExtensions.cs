using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Xunit.Runners.Http;

namespace Microsoft.Extensions.DependencyInjection;

public static class HttpRunnerBuilderExtensions
{
    public static void AddXunitHttpTestRunner(this IServiceCollection services, Assembly testAssembly)
    {
        services.AddSingleton(provider => new HttpTestRunner(provider.GetRequiredService<ILogger<HttpTestRunner>>(), testAssembly));
    }

    public static void UseXunitHttpTestRunner(this IEndpointRouteBuilder app)
    {
        var httpTestTrigger = app.ServiceProvider.GetRequiredService<HttpTestRunner>();

        var testCases = httpTestTrigger.Discover();

        foreach (var testCase in testCases)
        {
            app.MapGet(testCase.DisplayName, (HttpRequest request) =>
            {
                var nameOfTestToRun = request.Path.Value.TrimStart('/');

                return httpTestTrigger.RunTest(nameOfTestToRun);
            }).WithName(testCase.DisplayName)
            .WithOpenApi();
        }
    }
}
