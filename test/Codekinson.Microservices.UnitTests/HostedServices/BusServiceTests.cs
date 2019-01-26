using System.Threading;
using System.Threading.Tasks;
using Codekinson.Microservices.HostedServices;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Codekinson.Microservices.UnitTests.HostedServices
{
    public class BusServiceTests
    {
        private readonly BusService _systemUnderTest;
        private readonly IBusControl _busControl;

        public BusServiceTests()
        {
            _busControl = Substitute.For<IBusControl>();
            
            _systemUnderTest = new BusService(_busControl, Substitute.For<ILogger<BusService>>());
        }

        [Fact]
        public void StartAsync_StartsTheBus()
        {
            _busControl.StartAsync().Returns(Task.FromResult(Substitute.For<BusHandle>()));
            
            var actual = _systemUnderTest.StartAsync(default(CancellationToken));

            actual.IsCompleted.Should().BeTrue();
            _busControl.Received(1).StartAsync();
        }
        
        [Fact]
        public void StopAsync_StopsTheBus()
        {
            _busControl.StopAsync().Returns(Task.CompletedTask);
            
            var actual = _systemUnderTest.StopAsync(default(CancellationToken));

            actual.IsCompleted.Should().BeTrue();
            _busControl.Received(1).StopAsync();
        }
    }
}