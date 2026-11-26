using Admission.Application.Classes;
using Microsoft.Extensions.DependencyInjection;

namespace Admission.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ClassService>();
        return services;
    }
}
