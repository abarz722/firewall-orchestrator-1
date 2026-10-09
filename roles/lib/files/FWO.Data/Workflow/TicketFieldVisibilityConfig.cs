using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FWO.Data.Workflow
{
    /// <summary>
    /// Configures which ticket fields are displayed in the ticket dialog.
    /// </summary>
    public class TicketFieldVisibilityConfig
    {
        [JsonProperty("requester"), JsonPropertyName("requester")]
        public bool ShowRequester { get; set; } = true;

        [JsonProperty("priority"), JsonPropertyName("priority")]
        public bool ShowPriority { get; set; } = true;

        [JsonProperty("deadline"), JsonPropertyName("deadline")]
        public bool ShowDeadline { get; set; } = true;

        [JsonProperty("reason"), JsonPropertyName("reason")]
        public bool ShowReason { get; set; } = true;

        [JsonProperty("comments"), JsonPropertyName("comments")]
        public bool ShowComments { get; set; } = false;

        public static TicketFieldVisibilityConfig Parse(string? serializedConfig)
        {
            if (string.IsNullOrWhiteSpace(serializedConfig))
            {
                return new();
            }

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<TicketFieldVisibilityConfig>(serializedConfig) ?? new();
            }
            catch (System.Text.Json.JsonException)
            {
                return new();
            }
        }

        public string ToConfigValue()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }
}
