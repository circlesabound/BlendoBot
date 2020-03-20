namespace Pccg.Commands
{
    using System;
    using System.Threading.Tasks;
    using BlendoBotLib;
    using DSharpPlus.EventArgs;

    internal class CompendiumCommand
    {
        public async Task Run(IPccgClient client, MessageCreateEventArgs e, Func<SendMessageEventArgs, Task> sendMessageCallback)
        {
            // Parse args
            var splitInput = e.Message.Content.Trim().Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            // TODO
            throw new NotImplementedException();
        }
    }
}