namespace Pccg.Models
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Net.Http;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

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

        public async Task<string> Render()
        {
            // TODO this ain't working my dude
            string filepath = Path.Join("data", "pccg", $"{this.Id}.png");
            if (File.Exists(filepath))
            {
                return filepath;
            }

            using (var stream = await httpClient.GetStreamAsync(this.ImageUri).ConfigureAwait(false))
            using (var image = Image.FromStream(stream))
            using (var bitmap = new Bitmap(image))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var textFont = new Font("DejaVu Sans", 30);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.DrawString(
                    $"Name: {this.Name}",
                    textFont,
                    Brushes.Black,
                    new Rectangle(30, 270, 240, 240),
                    new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    });
                graphics.Flush();

                image.Save(filepath);
                return filepath;
            }
        }

        private static HttpClient httpClient = new HttpClient();
    }
}