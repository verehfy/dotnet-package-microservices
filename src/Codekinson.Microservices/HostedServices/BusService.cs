using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Codekinson.Microservices.HostedServices
{
    public class BusService : IHostedService
    {
        private readonly IBusControl _busControl;
        private readonly ILogger<BusService> _logger;

        public BusService(IBusControl busControl, ILogger<BusService> logger)
        {
            _busControl = busControl;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Event Bus");
            return _busControl.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping event bus");
            return _busControl.StopAsync(cancellationToken);
        }
    }
}