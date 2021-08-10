using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RequestResponseModification.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RequestResponseModification
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton<Microsoft.Azure.Cosmos.Database>(InitializeCosmosDatabaseClientInstanceAsync().GetAwaiter().GetResult());
            services.AddSingleton<MyMiddleware>();
        }

        private static async Task<Microsoft.Azure.Cosmos.Database> InitializeCosmosDatabaseClientInstanceAsync()
        {
            string databaseName = "MyDatabase";
            string connString = "MyConnectionString";

            Microsoft.Azure.Cosmos.CosmosClient cosmosClient = new Microsoft.Azure.Cosmos.CosmosClient(connString);

            Microsoft.Azure.Cosmos.DatabaseResponse database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
            Microsoft.Azure.Cosmos.Database dbClient = cosmosClient.GetDatabase(databaseName);

            return dbClient;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
