namespace Pccg
{
    using System;
    using System.Threading.Tasks;
    using BlendoBotLib;
    using DSharpPlus.EventArgs;
    using Pccg.Commands;

    public class PccgCommand : CommandBase, IDisposable
    {
        public PccgCommand(ulong guildId, IBotMethods botMethods) : base(guildId, botMethods) { }

        public override string Term => "?pccg";
        public override string Name => "pccg";
        public override string Description => "Predatory Collectable Character Game";
        public override string Usage => $"Usage: {"?pccg <command> <arguments>".Code()}";
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
            if (!Enum.TryParse<Command>(subcommandStr, true, out var subcommand))
            {
                await BotMethods.SendMessage(this, new SendMessageEventArgs
                    {
                        Message = $"Invalid command for {this.Term.Code()}",
                        Channel = e.Channel,
                        LogMessage = "PccgBadCommand"
                    }).ConfigureAwait(false);
                return;
            }

            switch (subcommand)
            {
                case Command.Compendium:
                    await new CompendiumCommand().Run(
                        this.client,
                        e,
                        msg => BotMethods.SendMessage(this, msg));
                    break;
                case Command.Draw:
                    await new DrawCommand().Run(
                        this.client,
                        e,
                        msg => BotMethods.SendMessage(this, msg),
                        msg => BotMethods.SendFile(this, msg));
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
