using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using MassTransit.MongoDbIntegration.MessageData;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Common.Infrastructure;
using Sample.Components.Consumers;
using Sample.Components.CourierActivities;
using Sample.Components.StateMachines;
using Sample.Components.StateMachines.OrderStateMachineActivities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Warehouse.Contracts;

namespace Sample.Service
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            var builder = new HostBuilder()
                                .ConfigureAppConfiguration((hostingContext, config) =>
                                {
                                    config.AddJsonFile("appsetings.json", true);
                                    config.AddEnvironmentVariables();
                                    if (args != null)
                                    {
                                        config.AddCommandLine(args);
                                    }
                                })
                                .ConfigureServices((hostContext, services) =>
                                {
                                    services.AddScoped<AcceptOrderActivity>();
                                    services.AddSingleton(KebabCaseEndpointNameFormatter.Instance);
                                    services.AddMassTransit(configure =>
                                    {
                                        configure.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                                        configure.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();
                                        configure.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                                        //.RedisRepository(s => s.DatabaseConfiguration("127.0.0.1"))
                                        .MongoDbRepository(r => {
                                            r.Connection = "mongodb://127.0.0.1";
                                            r.DatabaseName = "orderdb";
                                        }
                                        )
                                        ;
                                        // configure.AddConsumers(Assembly.GetEntryAssembly());
                                        //configure.UsingRabbitMq(

                                        //    (context, configurator) => {
                                        //    configurator.ConfigureEndpoints(context
                                        //       , new KebabCaseEndpointNameFormatter("order-service", false)
                                        //        );
                                        //    configurator.Host("localhost");
                                        //    //configurator.UseMessageRetry(retryConfigurator =>
                                        //    //{
                                        //    //    retryConfigurator.Interval(3, TimeSpan.FromSeconds(60));
                                        //    //});
                                        //    }

                                        //    );
                                        configure.UsingRabbitMq(ConfigureBus2);
                                        configure.AddRequestClient<AllocateInventory>();
                                        //configure.AddBus(ConfigureBus);
                                    });
                                    services.AddHostedService<MassTransitConsoleHostedService>();
                                })
                                .ConfigureLogging((hostingContext, logging) =>
                                {
                                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                                    logging.AddConsole()
                                   // .AddJsonConsole(options => { })
                                    ;
                                });
            if (isService)
            {
                await builder.UseWindowsService().Build().RunAsync();
            } else
            {
                Console.Title = ConsoleUtils.GetVersion();
                await builder.RunConsoleAsync();
            }
        }

       

        static IBusControl ConfigureBus(IServiceProvider serviceProvider)
        {
            return Bus.Factory.CreateUsingRabbitMq(
                //configure =>
                //    {
                //        configure.ReceiveEndpoint("order-service", e => {
                //            e.Consumer<SubmitOrderConsumer>();
                //        });
                //    }
            
            );
        }

        static void ConfigureBus2(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator configurator)
        {
            configurator.UseMessageData(new MongoDbMessageDataRepository("mongodb://127.0.0.1", "attachments"));
            configurator.UseMessageScheduler(new Uri("queue:quartz"));

            //configurator.ReceiveEndpoint(KebabCaseEndpointNameFormatter.Instance.Consumer<RoutingSlipBatchEventConsumer>(), e =>
            //{
            //    e.PrefetchCount = 20;

            //    e.Batch<RoutingSlipCompleted>(b =>
            //    {
            //        b.MessageLimit = 10;
            //        b.TimeLimit = TimeSpan.FromSeconds(5);

            //        b.Consumer<RoutingSlipBatchEventConsumer, RoutingSlipCompleted>(context);
            //    });
            //});

            configurator.ConfigureEndpoints(context);
        }
    }
}
