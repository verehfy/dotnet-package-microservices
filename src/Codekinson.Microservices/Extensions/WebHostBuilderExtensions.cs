using System;
using System.Threading;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Exceptions;
using Serilog;

namespace Codekinson.Microservices.Extensions
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHost EnsureRabbitMqServerAvailable(
            this IWebHost webHost,
            Func<IServiceProvider, IBusFactorySelector, IBusControl> createBusControl)
        {
            return EnsureRabbitMqServerAvailable(webHost, createBusControl, 3);
        }
        
        public static IWebHost EnsureRabbitMqServerAvailable(
            this IWebHost webHost,
            Func<IServiceProvider, IBusFactorySelector, IBusControl> createBusControl, 
            int numberOfRetries)
        {
            if (createBusControl == null)
            {
                throw new ArgumentNullException(nameof(createBusControl));
            }

            using (var scope = webHost.Services.CreateScope())
            {
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
                        var control = createBusControl(scope.ServiceProvider, factory);
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
            }

            return webHost;
        }
    }
}