namespace Pccg.Commands
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using BlendoBotLib;
    using DSharpPlus.EventArgs;

    internal class DrawCommand
    {
        public async Task Run(
            IPccgClient client,
            MessageCreateEventArgs e,
            Func<SendMessageEventArgs, Task> sendMessageCallback,
            Func<SendFileEventArgs, Task> sendFileCallback)
        {
            var result = await client.GetRandomCardFromCompendium().ConfigureAwait(false);
            if (result.IsFailed)
            {
                var errorMessage = string.Join(' ', result.Reasons);
                await sendMessageCallback(new SendMessageEventArgs
                    {
                        Message = $"Error processing {Command.Draw}: {errorMessage}",
                        Channel = e.Channel,
                        LogMessage = $"PccgDrawFailure"
                    }).ConfigureAwait(false);
            }
            else
            {
                var imageFilepath = await result.Value.Render().ConfigureAwait(false);
                await sendMessageCallback(new SendMessageEventArgs
                    {
                        Message = $"{JsonSerializer.Serialize(result.Value).Code()}",
                        Channel = e.Channel,
                        LogMessage = "PccgDrawReplyText"
                    });
                await sendFileCallback(new SendFileEventArgs
                    {
                        FilePath = imageFilepath,
                        Channel = e.Channel,
                        LogMessage = "PccgDrawReplyFile"
                    });
            }
        }
    }
}