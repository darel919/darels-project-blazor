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
        public string? description { get; set; }
        public string? content_id { get; set; }
        public string? desc { get; set; }
        public DateTime? createdDate { get; set; } // Add this for components that need DateTime
    }

    public class Thumbnail 
    {
        public string? url { get; set; }
    }

    public class YtData 
    {
        public string? title { get; set; }
        public List<Thumbnail>? videoThumbnails { get; set; }
        public string? description { get; set; }
    }

    public class Response
    {
        public List<FetchedData>? data { get; set; }
    }
}