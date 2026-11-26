using Admission.Application.Classes;
using Admission.Application.Holds;
using Microsoft.Extensions.DependencyInjection;

namespace Admission.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, HoldOptions holdOptions)
    {
        services.AddSingleton(holdOptions);
        services.AddScoped<ClassService>();
        services.AddScoped<HoldService>();
        return services;
    }
}
