using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LR.Standard;

public static class ServiceCollectionHostedServiceExtensions
{
    public static void AddHostedServiceByWrapper<TService>(this IServiceCollection services)
        where TService : class, IHostedService
    {
        if (typeof(BackgroundService).IsAssignableFrom(typeof(TService)))
        {
            services.AddHostedService(sp => new BackgroundServiceWrapper<TService>((BackgroundService)(object)sp.GetRequiredService<TService>()));
        }
        else
        {
            services.AddHostedService(sp => new HostedServiceWrapper<TService>(sp.GetRequiredService<TService>()));
        }
    }

    private class BackgroundServiceWrapper<TService> : BackgroundService
    {
        private readonly BackgroundService _service;

        public BackgroundServiceWrapper(BackgroundService service)
        {
            this._service = service;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return this._service.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return this._service.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new InvalidOperationException();
        }

        public override Task ExecuteTask => this._service.ExecuteTask;
    }

    private class HostedServiceWrapper<TService> : IHostedService
    {
        private readonly IHostedService _service;

        public HostedServiceWrapper(IHostedService service)
        {
            this._service = service;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return this._service.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return this._service.StopAsync(cancellationToken);
        }
    }
}
