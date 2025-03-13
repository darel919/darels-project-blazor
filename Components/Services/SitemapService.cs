using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;
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
            // sitemap.AppendLine(CreateSitemapUrl($"{_baseUri}/about", "About", DateTime.UtcNow));

            // Fetch and add video URLs
            var videos = await FetchVideosAsync();
            foreach (var video in videos)
            {
                sitemap.AppendLine(CreateSitemapUrl($"{_baseUri}/watch?v={video.id}", video.title, video.created));
            }

            sitemap.AppendLine("</urlset>");
            return sitemap.ToString();
        }

        private async Task<List<FetchedData>> FetchVideosAsync()
{
    var response = await _httpClient.GetAsync("https://api.darelisme.my.id/dp?sortBy=desc");
    response.EnsureSuccessStatusCode();
    var responseContent = await response.Content.ReadAsStringAsync();
    
    // Use TempFetchedData to handle string dates directly
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    // Skip trying to deserialize directly to FetchedData and go straight to the fallback approach
    var tempData = JsonSerializer.Deserialize<List<TempFetchedData>>(responseContent, options);
    var result = new List<FetchedData>();
    
    foreach (var item in tempData)
    {
        try
        {
            // Parse with explicit format for better reliability
            DateTime parsedDate = DateTime.ParseExact(
                item.created.Substring(0, 19), // Take only "2025-02-17 00:00:00" part
                "yyyy-MM-dd HH:mm:ss", 
                System.Globalization.CultureInfo.InvariantCulture
            );
                
            result.Add(new FetchedData
            {
                created = parsedDate,
                id = item.id,
                category = item.category,
                yt_data = item.yt_data,
                title = item.title
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing date '{item.created}': {ex.Message}");
            // Add with minimum date if parsing fails
            result.Add(new FetchedData
            {
                created = DateTime.MinValue,
                id = item.id,
                category = item.category,
                yt_data = item.yt_data,
                title = item.title
            });
        }
    }
    
    return result;
}

// Add this temporary class for fallback parsing
public class TempFetchedData
{
    public string created { get; set; } // As string instead of DateTime
    public string id { get; set; }
    public string[] category { get; set; }
    public yt_data yt_data { get; set; }
    public string title { get; set; }
}

        private string CreateSitemapUrl(string loc, string title, DateTime lastMod)
        {
            var titleElement = title != null ? $"<title>{title}</title>" : string.Empty;
            return $"<url><loc>{loc}</loc>{titleElement}<lastmod>{lastMod:yyyy-MM-dd}</lastmod><changefreq>daily</changefreq><priority>0.8</priority></url>";
        }
    }

    public class FetchedData
{
    public DateTime created { get; set; }
    public string id { get; set; }
    public string[] category { get; set; }  // Changed from string to string[]
    public yt_data yt_data { get; set; }
    public string title { get; set; }
}

    public class yt_data
    {
        public string title { get; set; }
        public List<Thumbnail> videoThumbnails { get; set; }
    }

    public class Thumbnail
    {
        public string url { get; set; }
    }
}
