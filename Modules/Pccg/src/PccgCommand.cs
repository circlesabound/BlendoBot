using System;
using System.Text.Json;
using System.Threading.Tasks;
using BlendoBotLib;
using DSharpPlus.EventArgs;

namespace Pccg
{
    public class PccgCommand : CommandBase, IDisposable
    {
        public PccgCommand(ulong guildId, IBotMethods botMethods) : base(guildId, botMethods) { }

        public override string Term => "?pccg";
        public override string Name => "pccg";
        public override string Description => "Predatory Collectable Character Game";
        public override string Usage => "TODO";
        public override string Author => "mozzarella";
        public override string Version => $"Client=0.0.1; Server={this.client.GetServerVersion().Result.ValueOrDefault.CommitHash ?? "<error>"}";

        public override async Task<bool> Startup()
        {
            var config = this.LoadConfig();
            this.client = new PccgClient(
                config.ApiHost
            );
            return await Task.FromResult(true).ConfigureAwait(false);
        }

        public override async Task OnMessage(MessageCreateEventArgs e)
        {
            var splitInput = e.Message.Content.Trim().Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (splitInput.Length < 2)
            {
                await BotMethods.SendMessage(this, new SendMessageEventArgs
                    {
                        Message = $"Too few arguments specified to {this.Term.Code()}",
                        Channel = e.Channel,
                        LogMessage = "PccgTooFewArgs"
                    }).ConfigureAwait(false);
                return;
            }

            var subcommandStr = splitInput[1];
            if (!Enum.TryParse<Subcommand>(subcommandStr, true, out var subcommand))
            {
                await BotMethods.SendMessage(this, new SendMessageEventArgs
                    {
                        Message = $"Invalid subcommand for {this.Term.Code()}",
                        Channel = e.Channel,
                        LogMessage = "PccgBadSubcommand"
                    }).ConfigureAwait(false);
                return;
            }

            switch (subcommand)
            {
                case Subcommand.Compendium:
                    await CompendiumCommand(e);
                    break;
                case Subcommand.Draw:
                    await DrawCommand(e);
                    break;
                default:
                    break;
            }
        }

        private PccgConfig LoadConfig() =>
            new PccgConfig
            {
                ApiHost = new Uri(BotMethods.ReadConfig(this, this.Name, "ApiUri"))
            };
        
        private async Task DrawCommand(MessageCreateEventArgs e)
        {
            var result = await this.client.GetRandomCardFromCompendium().ConfigureAwait(false);
            if (result.IsFailed)
            {
                var errorMessage = string.Join(' ', result.Reasons);
                await BotMethods.SendMessage(this, new SendMessageEventArgs
                    {
                        Message = $"Error processing {Subcommand.Draw}: {errorMessage}",
                        Channel = e.Channel,
                        LogMessage = $"PccgDraw : Error{Environment.NewLine}{errorMessage}"
                    }).ConfigureAwait(false);
            }
            else
            {
                await BotMethods.SendMessage(this, new SendMessageEventArgs
                    {
                        Message = $"{JsonSerializer.Serialize(result.Value).Code()}",
                        Channel = e.Channel,
                        LogMessage = "PccgDraw : Success"
                    }).ConfigureAwait(false);
            }
        }

        private async Task CompendiumCommand(MessageCreateEventArgs e)
        {
            // Parse args
            var splitInput = e.Message.Content.Trim().Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            // TODO
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.client?.Dispose();
            }
        }

        private PccgClient client = null!;
    }
}
