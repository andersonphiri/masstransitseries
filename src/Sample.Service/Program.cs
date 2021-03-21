using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Common.Infrastructure;
using Sample.Components.Consumers;
using Sample.Components.StateMachines;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
                                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
                                    services.AddMassTransit(configure =>
                                    {
                                        configure.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                                        configure.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                                        .RedisRepository(
                                            s => s.DatabaseConfiguration("127.0.0.1")
                                            );
                                       // configure.AddConsumers(Assembly.GetEntryAssembly());
                                        configure.UsingRabbitMq((context, configurator) => {
                                            configurator.ConfigureEndpoints(context
                                               // , new KebabCaseEndpointNameFormatter("order-service", false)
                                                );
                                            configurator.Host("localhost");
                                            //configurator.UseMessageRetry(retryConfigurator =>
                                            //{
                                            //    retryConfigurator.Interval(3, TimeSpan.FromSeconds(60));
                                            //});

                                        });
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
    }
}
