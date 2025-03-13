using System;

namespace dpOnDotnet.Components.Modules
{
    public static class DateTimeExtensions
    {
        public static string GetTimeAgo(this DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalDays > 365)
            {
                int years = (int)(timeSpan.TotalDays / 365);
                return years == 1 ? "1 year ago" : $"{years} years ago";
            }
            else if (timeSpan.TotalDays > 30)
            {
                int months = (int)(timeSpan.TotalDays / 30);
                return months == 1 ? "1 month ago" : $"{months} months ago";
            }
            else if (timeSpan.TotalDays > 1)
            {
                return $"{(int)timeSpan.TotalDays} days ago";
            }
            else if (timeSpan.TotalHours > 1)
            {
                return $"{(int)timeSpan.TotalHours} hours ago";
            }
            else if (timeSpan.TotalMinutes > 1)
            {
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            }
            else
            {
                return "just now";
            }
        }

        // Add a string extension method for GetTimeAgo
        public static string GetTimeAgo(this string? dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
                return "unknown time";
                
            if (DateTime.TryParse(dateTimeString, out DateTime dateTime))
                return GetTimeAgo(dateTime);
                
            return "unknown time";
        }
    }
}