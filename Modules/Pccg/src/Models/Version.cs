namespace Pccg.Models
{
    using System.Text.Json.Serialization;

    internal class Version
    {
        [JsonPropertyName("commit_hash")]
        public string CommitHash { get; set; } = null!;
    }
}