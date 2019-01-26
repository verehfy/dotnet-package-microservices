using System;
using System.Diagnostics;
using System.Threading;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Hosting;
using RabbitMQ.Client.Exceptions;
using Serilog;

namespace Codekinson.Microservices.Extensions
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder EnsureRabbitMqServerAvailable(this IWebHostBuilder builder, string busHost)
        {
            return EnsureRabbitMqServerAvailable(builder, busHost, "/");
        }
        
        public static IWebHostBuilder EnsureRabbitMqServerAvailable(this IWebHostBuilder builder, string busHost, string virtualHost)
        {
            if (string.IsNullOrWhiteSpace(busHost))
            {
                throw new ArgumentNullException(nameof(busHost));
            }

            if (string.IsNullOrWhiteSpace(virtualHost))
            {
                throw new ArgumentNullException(nameof(virtualHost));
            }
            
            return EnsureRabbitMqServerAvailable(
                builder, 
                factory => factory.CreateUsingRabbitMq(cfg => { cfg.Host(busHost, virtualHost, h => { }); }),
                3);
        }
        
        public static IWebHostBuilder EnsureRabbitMqServerAvailable(this IWebHostBuilder builder, Func<IBusFactorySelector, IBusControl> createBusControl, int numberOfRetries)
        {
            if (createBusControl == null)
            {
                throw new ArgumentNullException(nameof(createBusControl));
            }
            
            var attempts = 0;
            var available = false;
            while (!available)
            {
                if (attempts > numberOfRetries)
                {
                    throw new InvalidOperationException();
                }

                try
                {
                    var factory = Bus.Factory;
                    var control = createBusControl(factory);
                    control.Start();
                    
                    available = true;
                    Log.Information("Event bus available, continuing");
                }
                catch (RabbitMqConnectionException exception)
                    when (exception.InnerException is BrokerUnreachableException)
                {
                    available = false;
                    Log.Information("Unable to connect to the rabbit queue, retrying in 5 seconds");
                    Thread.Sleep(5000);
                }
                finally
                {
                    attempts += 1;
                }
            }
            
            return builder;
        }
    }
}