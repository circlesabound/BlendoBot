namespace Pccg
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentResults;
    using Pccg.Models;
    using Polly;

    internal class PccgClient : IPccgClient, IDisposable
    {
        public PccgClient(
            Uri apiHost
        )
        {
            this.httpClient = new HttpClient();
            this.apiHost = apiHost;
        }

        public void Dispose()
        {
            this.httpClient.Dispose();
        }

        public async Task<Result<Card>> GetRandomCardFromCompendium()
        {
            try
            {
                var ret = await Policy
                    .Handle<HttpRequestException>()
                    .RetryAsync(3)
                    .ExecuteAsync(async ct => await this.httpClient.GetAsync(
                        new Uri(this.apiHost, @"compendium/random"),
                        ct).ConfigureAwait(false),
                    CancellationToken.None).ConfigureAwait(false);
                var contentStr = await ret.Content.ReadAsStringAsync().ConfigureAwait(false);
                var card = JsonSerializer.Deserialize<Card>(contentStr);
                return Results.Ok(card);
            }
            catch (Exception ex)
            {
                return Results.Fail(
                    new Error("Exception occurred")
                        .CausedBy(ex));
            }
        }

        public async Task<Result> UpsertCardToCompendium(Card card)
        {
            try
            {
                var ret = await Policy
                    .Handle<HttpRequestException>()
                    .RetryAsync(3)
                    .ExecuteAsync(async ct => await this.httpClient.PutAsync(
                        new Uri(this.apiHost, $"compendium/{card.Id}"),
                        new StringContent(JsonSerializer.Serialize(card)),
                        ct).ConfigureAwait(false),
                    CancellationToken.None).ConfigureAwait(false);
                ret.EnsureSuccessStatusCode();
                switch (ret.StatusCode)
                {
                    case HttpStatusCode.Created:
                        // Inserted
                    case HttpStatusCode.OK:
                        // Updated
                        return Results.Ok();
                    default:
                        return Results.Fail(
                            new Error($"PUT returned {ret.StatusCode}")
                        );
                }
            }
            catch (Exception ex)
            {
                return Results.Fail(
                    new Error("Exception occurred")
                        .CausedBy(ex));
            }
        }

        public async Task<Result<Models.Version>> GetServerVersion()
        {
            try
            {
                var ret = await Policy
                    .Handle<HttpRequestException>()
                    .RetryAsync(3)
                    .ExecuteAsync(async ct => await this.httpClient.GetAsync(
                        new Uri(this.apiHost, @"version"),
                        ct).ConfigureAwait(false),
                    CancellationToken.None).ConfigureAwait(false);
                var contentStr = await ret.Content.ReadAsStringAsync().ConfigureAwait(false);
                var version = JsonSerializer.Deserialize<Models.Version>(contentStr);
                return Results.Ok(version);
            }
            catch (Exception ex)
            {
                return Results.Fail(
                    new Error("Exception occurred")
                        .CausedBy(ex));
            }
        }

        private HttpClient httpClient;

        private Uri apiHost;
    }
}