namespace Pccg.Models
{
    using System;
    using System.Text.Json.Serialization;

    internal class Card
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.Empty!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("image_uri")]
        public Uri ImageUri { get; set; } = null!;
    }
}