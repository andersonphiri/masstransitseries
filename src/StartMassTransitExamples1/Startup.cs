using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Definition;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Sample.Components.Consumers;
using Sample.Contracts;

namespace StartMassTransitExamples1
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
            services.AddSingleton(KebabCaseEndpointNameFormatter.Instance);
            services.AddMassTransit(configure =>
            {
                //configure.AddBus(provider => Bus.Factory.CreateUsingRabbitMq());
                configure.UsingRabbitMq((context, configurator) => {
                    configurator.ConfigureEndpoints(context
                        //, new KebabCaseEndpointNameFormatter("order-service", false)
                        );
                });
                // configure.AddConsumer<SubmitOrderConsumer>();
                // configure.AddMediator(services);
                configure.AddRequestClient<ISubmitOrder>(
                    //new Uri($"queue:{KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}")
                    new Uri($"exchange:{KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}")
                    ) ;
                configure.AddRequestClient<CheckOrder>();
            });
            services.AddMassTransitHostedService();
            //services.AddMediator(configure =>
            //{
            //    configure.AddConsumer<SubmitOrderConsumer>();
            //    configure.AddRequestClient<ISubmitOrder>();
            //});
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "StartMassTransitExamples1", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "StartMassTransitExamples1 v1"));
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
