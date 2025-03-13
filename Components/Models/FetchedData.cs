using System;
using System.Collections.Generic;

namespace dpOnDotnet.Components.Models
{
    public class FetchedData 
    {
        public string? created { get; set; }
        public string? id { get; set; }
        public string[]? category { get; set; }
        public YtData? yt_data { get; set; }
        public int total_views { get; set; }
        public string? title { get; set; }
        public string? desc { get; set; }
        public string? creator { get; set; }
        public string? updated { get; set; }
        public string? collectionId { get; set; }
        public string? collectionName { get; set; }
        
        // Properties for Watch page
        public string? yt_vid_id { get; set; }
        
        // Add computed property for createdDate to fix errors in components
        public DateTime? createdDate => 
            !string.IsNullOrEmpty(created) && DateTime.TryParse(created, out var date) 
            ? date 
            : null;
    }

    public class Thumbnail 
    {
        public string? url { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
    }

    public class YtData 
    {
        public string? title { get; set; }
        public List<Thumbnail>? videoThumbnails { get; set; }
        public string? description { get; set; }
        public string? id { get; set; }
        public Thumbnail? thumbnail { get; set; }
    }

    public class Response
    {
        public List<FetchedData>? data { get; set; }
        public int total { get; set; }
        public int limit { get; set; }
        public int skip { get; set; }
    }

        public class CategoryResponse
    {
        public CategoryDetail Category { get; set; }
        public List<FetchedData> Videos { get; set; }
    }

    public class CategoryDetail
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}