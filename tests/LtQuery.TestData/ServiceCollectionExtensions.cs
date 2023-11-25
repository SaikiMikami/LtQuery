using LtQuery.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.TestData;

public static class ServiceCollectionExtensions
{
    public static void AddTest(this IServiceCollection _this)
    {
        _this.AddSingleton<IModelConfiguration, ModelConfiguration>();
    }
}
