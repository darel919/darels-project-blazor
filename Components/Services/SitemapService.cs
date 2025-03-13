using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;
using System;
using System.Collections.Generic;
using dpOnDotnet.Components.Models;

namespace dpOnDotnet.Components.Services
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
            _baseUri = configuration["BaseURL"] ?? "https://darelisme.my.id";
            _httpClient = httpClient;
        }

        public async Task<string> GenerateSitemapAsync()
        {
            var sitemap = new StringBuilder();
            sitemap.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sitemap.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            // Add static URLs
            sitemap.AppendLine(CreateSitemapUrl(_baseUri, "Home", DateTime.UtcNow));
            
            // Fetch and add video URLs
            var videos = await FetchVideosAsync();
            foreach (var video in videos)
            {
                DateTime videoDate;
                if (!string.IsNullOrEmpty(video.created) && DateTime.TryParse(video.created, out videoDate))
                {
                    sitemap.AppendLine(CreateSitemapUrl($"{_baseUri}/watch?v={video.id}", video.title ?? "", videoDate));
                }
                else
                {
                    sitemap.AppendLine(CreateSitemapUrl($"{_baseUri}/watch?v={video.id}", video.title ?? "", DateTime.UtcNow));
                }
            }

            sitemap.AppendLine("</urlset>");
            return sitemap.ToString();
        }

        private async Task<List<FetchedData>> FetchVideosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://api.darelisme.my.id/dp?sortBy=desc");
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // First try to deserialize directly to a list of FetchedData
                try
                {
                    var directList = JsonSerializer.Deserialize<List<FetchedData>>(responseContent, options);
                    if (directList != null && directList.Count > 0)
                    {
                        return directList;
                    }
                }
                catch
                {
                    // If direct deserialization fails, try with the Response wrapper
                    try
                    {
                        var apiResponse = JsonSerializer.Deserialize<Response>(responseContent, options);
                        if (apiResponse?.data != null && apiResponse.data.Count > 0)
                        {
                            return apiResponse.data;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deserializing Response: {ex.Message}");
                        
                        // Try with temp model and map
                        try
                        {
                            var tempData = JsonSerializer.Deserialize<List<TempFetchedData>>(responseContent, options);
                            if (tempData != null)
                            {
                                var result = new List<FetchedData>();
                                foreach (var item in tempData)
                                {
                                    result.Add(new FetchedData
                                    {
                                        created = item.created,
                                        id = item.id,
                                        category = item.category,
                                        title = item.title,
                                        total_views = item.total_views,
                                        yt_data = item.yt_data != null ? new YtData
                                        {
                                            title = item.yt_data.title,
                                            description = item.yt_data.description,
                                            videoThumbnails = item.yt_data.videoThumbnails?.Select(t => 
                                                new Thumbnail { url = t.url }).ToList()
                                        } : null
                                    });
                                }
                                return result;
                            }
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"Error with temp model: {innerEx.Message}");
                        }
                    }
                }
                
                return new List<FetchedData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching videos: {ex.Message}");
                return new List<FetchedData>();
            }
        }

        private string CreateSitemapUrl(string loc, string title, DateTime lastMod)
        {
            var titleElement = !string.IsNullOrEmpty(title) ? $"<title>{title}</title>" : string.Empty;
            return $"<url><loc>{loc}</loc>{titleElement}<lastmod>{lastMod:yyyy-MM-dd}</lastmod><changefreq>daily</changefreq><priority>0.8</priority></url>";
        }
        
        // Temporary model for direct parsing
        private class TempFetchedData
        {
            public string created { get; set; } = "";
            public string id { get; set; } = "";
            public string[]? category { get; set; }
            public TempYtData? yt_data { get; set; }
            public string title { get; set; } = "";
            public int total_views { get; set; }
        }
        
        private class TempYtData
        {
            public string? title { get; set; }
            public List<TempThumbnail>? videoThumbnails { get; set; }
            public string? description { get; set; }
        }
        
        private class TempThumbnail
        {
            public string url { get; set; } = "";
        }
    }
}