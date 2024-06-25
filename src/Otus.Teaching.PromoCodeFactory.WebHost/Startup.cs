using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.DataAccess;
using Otus.Teaching.PromoCodeFactory.DataAccess.Data;
using Otus.Teaching.PromoCodeFactory.DataAccess.Repositories;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Otus.Teaching.PromoCodeFactory.WebHost;

public class Startup {
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration) => Configuration = configuration;

    public void ConfigureServices(IServiceCollection services) {
        services.AddControllers().AddMvcOptions(x =>
            x.SuppressAsyncSuffixInActionNames = false);
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IDbInitializer, EfDbInitializer>();
        services.AddDbContext<DataContext>(x => {
            x.UseSqlite("Filename=PromoCodeFactoryDb.sqlite");
            //x.UseNpgsql(Configuration.GetConnectionString("PromoCodeFactoryDb"));
            x.UseSnakeCaseNamingConvention();
            x.UseLazyLoadingProxies();
        });

        services.AddOpenApiDocument(options => {
            options.Title = "PromoCode Factory API Doc";
            options.Version = "1.0";
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer) {
        if (env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }
        else {
            app.UseHsts();
        }

        app.UseOpenApi();
        app.UseSwaggerUi(x => {
            x.DocExpansion = "list";
        });

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
        });

        dbInitializer.InitializeDb();
    }
}