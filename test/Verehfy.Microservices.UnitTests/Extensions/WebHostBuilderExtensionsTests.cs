using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RabbitMQ.Client.Exceptions;
using Verehfy.Microservices.Extensions;
using Xunit;

namespace Verehfy.Microservices.UnitTests.Extensions
{
    public class WebHostBuilderExtensionsTests
    {
        private readonly IWebHost _webHost;
        private readonly IBusControl _busControl;

        public WebHostBuilderExtensionsTests()
        {
            _webHost = Substitute.For<IWebHost>();
            
            var serviceScope = Substitute.For<IServiceScope>();
            var serviceProvider = Substitute.For<IServiceProvider>();
            var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();

            serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(serviceScopeFactory);
            
            _webHost.Services.Returns(serviceProvider);
            
            serviceScope.ServiceProvider.Returns(serviceProvider);
            
            serviceScopeFactory.CreateScope().Returns(serviceScope);
            
            _busControl = Substitute.For<IBusControl>();
        }

        [Fact]
        public void EnsureRabbitMqServerAvailable_WithRetry_DoesNotThrow()
        {
            _busControl
                .StartAsync(Arg.Any<CancellationToken>())
                .Returns(
                    x => Task.FromException<BusHandle>(new RabbitMqConnectionException("meh", new BrokerUnreachableException(new InvalidOperationException()))), 
                    x => Task.FromResult(Substitute.For<BusHandle>()));
            
            Action actual = () => _webHost.EnsureRabbitMqServerAvailable((services, factory) => _busControl, 1);
            
            actual.Should().NotThrow();
        }
        
        [Fact]
        public void EnsureRabbitMqServerAvailable_WithTooManyRetries_ThrowsInvalidOperationException()
        {
            _busControl
                .StartAsync(Arg.Any<CancellationToken>())
                .Returns(
                    x => Task.FromException<BusHandle>(new RabbitMqConnectionException("meh", new BrokerUnreachableException(new InvalidOperationException()))),
                    x => Task.FromException<BusHandle>(new RabbitMqConnectionException("meh", new BrokerUnreachableException(new InvalidOperationException()))),
                    x => Task.FromResult(Substitute.For<BusHandle>()));
            
            Action actual = () => _webHost.EnsureRabbitMqServerAvailable((services, factory) => _busControl, 1);
            
            actual.Should().Throw<InvalidOperationException>();
        }
        
        [Fact]
        public void EnsureRabbitMqServerAvailable_WithSuccessfulAttempt_DoesNotThrow()
        {
            Action actual = () => _webHost.EnsureRabbitMqServerAvailable((services, factory) => _busControl, 1);
            
            actual.Should().NotThrow();
        }
    }
}