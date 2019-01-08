using System;
using System.Threading;
using System.Threading.Tasks;
using Codekinson.Microservices.Extensions;
using FluentAssertions;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using RabbitMQ.Client.Exceptions;
using Xunit;

namespace Codekinson.Microservices.UnitTests.Extensions
{
    public class WebHostBuilderExtensionsTests
    {
        private readonly IWebHostBuilder _webHostBuilder;
        private readonly IBusControl _busControl;

        public WebHostBuilderExtensionsTests()
        {
            _webHostBuilder = Substitute.For<IWebHostBuilder>();
            _busControl = Substitute.For<IBusControl>();
        }

        [Fact]
        public void EnsureRabbitMqServerAvailable_WithRetry_DoesNotThrow()
        {
            _busControl
                .StartAsync(Arg.Any<CancellationToken>())
                .Returns(
                    x => Task.FromException<BusHandle>(new RabbitMqConnectionException("meh", new BrokerUnreachableException(new Exception()))), 
                    x => Task.FromResult(Substitute.For<BusHandle>()));
            
            Action actual = () => _webHostBuilder.EnsureRabbitMqServerAvailable(factory => _busControl, 1);
            
            actual.Should().NotThrow();
        }
        
        [Fact]
        public void EnsureRabbitMqServerAvailable_WithTooManyRetries_ThrowsInvalidOperationException()
        {
            _busControl
                .StartAsync(Arg.Any<CancellationToken>())
                .Returns(
                    x => Task.FromException<BusHandle>(new RabbitMqConnectionException("meh", new BrokerUnreachableException(new Exception()))),
                    x => Task.FromException<BusHandle>(new RabbitMqConnectionException("meh", new BrokerUnreachableException(new Exception()))),
                    x => Task.FromResult(Substitute.For<BusHandle>()));
            
            Action actual = () => _webHostBuilder.EnsureRabbitMqServerAvailable(factory => _busControl, 1);
            
            actual.Should().Throw<InvalidOperationException>();
        }
        
        [Fact]
        public void EnsureRabbitMqServerAvailable_WithSuccessfulAttempt_DoesNotThrow()
        {
            Action actual = () => _webHostBuilder.EnsureRabbitMqServerAvailable(factory => _busControl, 1);
            
            actual.Should().NotThrow();
        }
    }
}