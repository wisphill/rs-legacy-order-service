using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace LegacyOrderService.Common;

public sealed class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
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
        var provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });

        return new TypeResolver(provider);
    }
}

public sealed class TypeResolver : ITypeResolver, IDisposable
{
    private readonly ServiceProvider _provider;

    public TypeResolver(ServiceProvider provider)
    {
        _provider = provider;
    }

    public object? Resolve(Type? type)
    {
        return type == null ? null : _provider.GetService(type);
    }

    public void Dispose()
    {
        _provider.Dispose();
    }
}