using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace LegacyOrderService.Common;

public sealed class CliTypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    public void Register(Type service, Type implementation)
    {
        services.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        services.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        services.AddSingleton(service, _ => factory());
    }

    public ITypeResolver Build()
    {
        // make sure we do not inject a Scoped into a Singleton
        var provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });

        return new CliTypeResolver(provider);
    }
}

public sealed class CliTypeResolver(ServiceProvider provider) : ITypeResolver, IDisposable
{
    public object? Resolve(Type? type)
    {
        return type == null ? null : provider.GetService(type);
    }

    public void Dispose()
    {
        provider.Dispose();
    }
}