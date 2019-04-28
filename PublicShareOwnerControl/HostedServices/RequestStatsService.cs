using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using PublicShareOwnerControl.DB;

namespace PublicShareOwnerControl.HostedServices
{
    public class RequestStatsService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly PublicShareOwnerContext _context;
        private Timer _timer;
        private static readonly Gauge TotalValueOfAllShares = Metrics.CreateGauge("TotalValueOfAllShares", "Total value of all shares multiplied with the last traded value");

        public RequestStatsService(ILogger<RequestStatsService> logger, PublicShareOwnerContext context)
        {
            _logger = logger;
            _context = context;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Request Stats services is starting.");

            _timer = new Timer(UpdateRequestStats, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private async void UpdateRequestStats(object state)
        {
            var totalValueOfAllShares = await _context.Stocks.SumAsync(stock => stock.ShareHolders.Sum(shareholder => shareholder.Amount) * stock.LastTradedValue);
            _logger.LogInformation("Total value of all shares {totalValue}", totalValueOfAllShares);
            TotalValueOfAllShares.Set(totalValueOfAllShares);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Request Stats services is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}