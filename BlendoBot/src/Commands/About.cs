﻿using BlendoBotLib;
using BlendoBotLib.Commands;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlendoBot.Commands {
	public class About : ICommand {
		CommandProps ICommand.Properties => properties;

		private static readonly CommandProps properties = new CommandProps {
			Term = "?about",
			Name = "About",
			Description = "Posts information about this version of the bot, or of any loaded module. You probably already know how to use this command by now.",
			Usage = $"Use {"?about".Code()} to see the information about the bot.\nUse {"?about [command]".Code()} to see information about another command.",
			Author = "Biendeo",
			Version = "1.0.0",
			Func = AboutCommand
		};

		public static async Task AboutCommand(MessageCreateEventArgs e) {
			var sb = new StringBuilder();

			if (e.Message.Content.Length == properties.Term.Length) {
				sb.AppendLine($"`{Program.Props.Name} {Program.Props.Version} ({Program.Props.Description}) by {Program.Props.Author}`");
				await Methods.SendMessage(null, new SendMessageEventArgs {
					Message = sb.ToString(),
					Channel = e.Channel,
					LogMessage = "About"
				});
			} else {
				string specifiedCommand = e.Message.Content.Substring(properties.Term.Length + 1);
				if (!specifiedCommand.StartsWith('?')) {
					specifiedCommand = $"?{specifiedCommand}";
				}
				var command = Command.AvailableCommands.FirstOrDefault(x => x.Value.Term == specifiedCommand);
				if (command.Key == null) {
					await Methods.SendMessage(null, new SendMessageEventArgs {
						Message = $"No command called {specifiedCommand}",
						Channel = e.Channel,
						LogMessage = "AboutErrorInvalidCommand"
					});
				} else {
					sb.AppendLine($"{command.Value.Name.Bold()} ({command.Value.Version?.Italics()}) by {command.Value.Author?.Italics()}");
					sb.AppendLine(command.Value.Description);
					await Methods.SendMessage(null, new SendMessageEventArgs {
						Message = sb.ToString(),
						Channel = e.Channel,
						LogMessage = "AboutSpecific"
					});
				}
			}
		}
	}
}
