using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;
using Snowflake.EntityFrameworkCore.Scaffolding.Internal;

[assembly: DesignTimeProviderServices("Snowflake.EntityFrameworkCore.Design.Internal.SnowflakeDesignTimeServices")]

namespace Snowflake.EntityFrameworkCore.Design.Internal;

public class SnowflakeDesignTimeServices : IDesignTimeServices
{
    
    public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddEntityFrameworkSnowflake();

#pragma warning disable EF1001 // Internal EF Core API usage.
        new EntityFrameworkRelationalDesignServicesBuilder(serviceCollection)
            .TryAdd<IAnnotationCodeGenerator, SnowflakeAnnotationCodeGenerator>()
            .TryAdd<ICSharpRuntimeAnnotationCodeGenerator, SnowflakeCSharpRuntimeAnnotationCodeGenerator>()
#pragma warning restore EF1001 // Internal EF Core API usage.
            .TryAdd<IDatabaseModelFactory, SnowflakeDatabaseModelFactory>()
            .TryAdd<IProviderConfigurationCodeGenerator, SnowflakeCodeGenerator>()
            .TryAddCoreServices();
    }
}
