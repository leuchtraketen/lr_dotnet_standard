using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LR.Standard;

public static class ServiceCollectionExtensions
{
    public static void AddLRStandard(this IServiceCollection services)
    {
        services.AddSingleton<FileTypeService>(sp => new FileTypeService(sp.GetRequiredService<ILogger<FileTypeService>>()));
    }
}
