using System.Globalization;
using System.Text.Json;

namespace FWO.Data.Workflow
{
    public enum TimeObjectOrigin
    {
        Requester,
        Approver
    }

    public static class TimeObjectNamingHelper
    {
        public static Dictionary<string, string> ParseSuffixes(string? serializedSuffixes)
        {
            Dictionary<string, string> suffixes = new(StringComparer.OrdinalIgnoreCase)
            {
                [TimeObjectOrigin.Requester.ToString().ToLowerInvariant()] = "",
                [TimeObjectOrigin.Approver.ToString().ToLowerInvariant()] = ""
            };
            if (!string.IsNullOrWhiteSpace(serializedSuffixes))
            {
                try
                {
                    Dictionary<string, string>? configured = JsonSerializer.Deserialize<Dictionary<string, string>>(serializedSuffixes);
                    if (configured != null)
                    {
                        foreach ((string key, string value) in configured)
                        {
                            if (Enum.TryParse<TimeObjectOrigin>(key, true, out TimeObjectOrigin origin))
                            {
                                suffixes[origin.ToString().ToLowerInvariant()] = value ?? "";
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // Keep the empty defaults for malformed or legacy configuration.
                }
            }
            return suffixes;
        }

        public static string BuildDisplayName(string? baseName, TimeObjectOrigin? origin,
            IReadOnlyDictionary<string, string>? suffixes)
        {
            if (string.IsNullOrWhiteSpace(baseName) || !origin.HasValue || suffixes == null)
            {
                return baseName ?? "";
            }

            string key = origin.Value.ToString().ToLowerInvariant();
            if (!suffixes.TryGetValue(key, out string? suffix) || string.IsNullOrEmpty(suffix)
                || baseName.EndsWith(suffix, StringComparison.Ordinal))
            {
                return baseName;
            }
            return baseName + suffix;
        }

        public static string BuildDateBasedName(DateTime? start, DateTime? end, string precision)
        {
            string Format(DateTime value) => precision switch
            {
                FlowIntegrationTimePrecisionOptions.Date => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                FlowIntegrationTimePrecisionOptions.Hours => value.ToString("yyyy-MM-dd HH 'h'", CultureInfo.InvariantCulture),
                FlowIntegrationTimePrecisionOptions.Minutes => value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                _ => value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            };

            if (start.HasValue && end.HasValue)
            {
                return $"{Format(start.Value)} - {Format(end.Value)}";
            }
            if (start.HasValue)
            {
                return $">= {Format(start.Value)}";
            }
            return end.HasValue ? $"<= {Format(end.Value)}" : "";
        }
    }
}
