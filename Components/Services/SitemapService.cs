using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YourNamespace.Services
{
    public interface ISitemapService
    {
        Task<string> GenerateSitemapAsync();
    }

    public class SitemapService : ISitemapService
    {
        private readonly string _baseUri;
        private readonly HttpClient _httpClient;

        public SitemapService(IConfiguration configuration, HttpClient httpClient)
        {
            _baseUri = configuration["BaseURL"];
            _httpClient = httpClient;
        }

        public async Task<string> GenerateSitemapAsync()
        {
            var sitemap = new StringBuilder();
            sitemap.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sitemap.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            // Add static URLs
            sitemap.AppendLine(CreateSitemapUrl(_baseUri, "Home", DateTime.UtcNow));
            sitemap.AppendLine(CreateSitemapUrl($"{_baseUri}about", "About", DateTime.UtcNow));

            // Fetch and add video URLs
            var videos = await FetchVideosAsync();
            foreach (var video in videos)
            {
                sitemap.AppendLine(CreateSitemapUrl($"{_baseUri}/watch?v={video.content_id}", video.ytData.title, video.created_at));
            }

            sitemap.AppendLine("</urlset>");
            return sitemap.ToString();
        }

        private async Task<List<FetchedData>> FetchVideosAsync()
        {
            var response = await _httpClient.GetAsync("https://darelisme.my.id/api/dp?sortBy=desc");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<FetchedData>>(responseContent);
        }

        private string CreateSitemapUrl(string loc, string title, DateTime lastMod)
        {
            var titleElement = title != null ? $"<title>{title}</title>" : string.Empty;
            return $"<url><loc>{loc}</loc>{titleElement}<lastmod>{lastMod:yyyy-MM-dd}</lastmod><changefreq>daily</changefreq><priority>0.8</priority></url>";
        }
    }

    public class FetchedData
    {
        public DateTime created_at { get; set; }
        public string content_id { get; set; }
        public string type_id { get; set; }
        public YTData ytData { get; set; }
        public int total_views { get; set; }
    }

    public class YTData
    {
        public string title { get; set; }
        public List<Thumbnail> videoThumbnails { get; set; }
    }

    public class Thumbnail
    {
        public string url { get; set; }
    }
}
