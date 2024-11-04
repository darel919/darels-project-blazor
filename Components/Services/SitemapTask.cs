using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YourNamespace.Services
{
    public class SitemapTask : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IWebHostEnvironment _env;
        private Timer _timer;

        public SitemapTask(IServiceScopeFactory scopeFactory, IWebHostEnvironment env)
        {
            _scopeFactory = scopeFactory;
            _env = env;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ExecuteTask, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void ExecuteTask(object state)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var sitemapService = scope.ServiceProvider.GetRequiredService<ISitemapService>();
                var sitemap = await sitemapService.GenerateSitemapAsync();
                var sitemapPath = Path.Combine(_env.WebRootPath, "sitemap.xml");
                await File.WriteAllTextAsync(sitemapPath, sitemap);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
