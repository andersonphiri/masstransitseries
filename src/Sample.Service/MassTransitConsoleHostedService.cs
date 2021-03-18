using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Service
{
    public class MassTransitConsoleHostedService : IHostedService
    {
        readonly IBusControl _bus;
        readonly ILogger<MassTransitConsoleHostedService> _Logger;

        public MassTransitConsoleHostedService(IBusControl bus, ILogger<MassTransitConsoleHostedService> logger)
        {
            _bus = bus;
            _Logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _Logger.LogInformation("Statrting Console COnsumer");
            await _bus.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _bus.StopAsync(cancellationToken);
        }
    }
}
