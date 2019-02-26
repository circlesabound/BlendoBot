﻿using BlendoBotLib;
using BlendoBotLib.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RemindMe {
	public class RemindMe : ICommand {
		CommandProps ICommand.Properties => properties;

		public static readonly CommandProps properties = new CommandProps {
			Term = "?remind",
			Name = "Remind Me",
			Description = "Reminds you about something later on! Please note that I currently do not remember messages if I am restarted.",
			Usage = $"Usage:\n{$"?remind {"at".Bold()} [date/time] {"to".Bold()} [message]".Code()} {"(this reminds you at a certain point in time)".Italics()}\n{$"?remind {"in".Bold()} [date/time] {"to".Bold()} [message]".Code()} {"(this reminds you after a certain interval)".Italics()}",
			Author = "Biendeo",
			Version = "0.1.0",
			Startup = Startup,
			OnMessage = RemindCommand
		};

		private static readonly string DatabasePath = "blendobot-remindme-database.json";

		private static List<Reminder> OutstandingReminders;

		private static async Task<bool> Startup() {
			OutstandingReminders = new List<Reminder>();
			/*
			if (File.Exists(DatabasePath)) {
				dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(DatabasePath));
				foreach (var item in json.reminders) {
					var reminder = new Reminder(DateTime.FromFileTimeUtc(item.Time), item.Message, item.Channel, item.User, new Action<Reminder>((Reminder r) => { CleanupReminder(r); }));

					if (reminder.Time < DateTime.UtcNow) {
						await Methods.SendMessage(null, new SendMessageEventArgs {
							Message = $"I just woke up and forgot to send you this alert on time!\n{reminder.Message}",
							Channel = reminder.Channel,
							LogMessage = "ReminderLateAlert"
						});
					} else {
						reminder.Activate();
						OutstandingReminders.Add(reminder);
					}
				}

				OutstandingReminders.Sort();
				SaveReminders();
			}
			*/
			return true;
		}

		private static void SaveReminders() {
			JsonConvert.SerializeObject(OutstandingReminders);
		}

		private static void CleanupReminder(Reminder reminder) {
			OutstandingReminders.Remove(reminder);
			SaveReminders();
		}

		public static async Task RemindCommand(MessageCreateEventArgs e) {
			// Try and decipher the output.
			var splitMessage = e.Message.Content.Split(' ');

			// Try and look for the "to" index.
			int toIndex = 0;
			while (toIndex < splitMessage.Length && splitMessage[toIndex] != "to") {
				++toIndex;
			}

			if (toIndex == splitMessage.Length) {
				await Methods.SendMessage(null, new SendMessageEventArgs {
					Message = "Incorrect syntax, make sure you use the word \"to\" after you indicate the time you want the reminder!",
					Channel = e.Channel,
					LogMessage = "ReminderErrorNoTo"
				});
				return;
			} else if (toIndex == splitMessage.Length - 1) {
				await Methods.SendMessage(null, new SendMessageEventArgs {
					Message = "Incorrect syntax, make sure you type a message after that \"to\"!",
					Channel = e.Channel,
					LogMessage = "ReminderErrorNoMessage"
				});
				return;
			}

			// Now decipher the time.
			DateTime foundTime = DateTime.Now;
			if (splitMessage[1] == "at") {
				foundTime = DateTime.Parse(string.Join(' ', splitMessage.Skip(2).Take(toIndex - 2)));
				if (foundTime < DateTime.Now) {
					await Methods.SendMessage(null, new SendMessageEventArgs {
						Message = $"The time you input was parsed as {foundTime}, which is in the past! Make your time a little more specific!",
						Channel = e.Channel,
						LogMessage = "ReminderErrorPastTime"
					});
					return;
				}
			} else  if (splitMessage[1] == "in") {
				var span = TimeSpan.Parse(string.Join(' ', splitMessage.Skip(2).Take(toIndex - 2)));
				foundTime = DateTime.Now + span;
			} else {
				await Methods.SendMessage(null, new SendMessageEventArgs {
					Message = "Incorrect syntax, make sure you use the word \"in\" or \"at\" to specify a time for the reminder!",
					Channel = e.Channel,
					LogMessage = "ReminderErrorNoAt"
				});
				return;
			}

			// Finally extract the message.
			string message = string.Join(' ', splitMessage.Skip(toIndex + 1));

			// Make the reminder.
			var reminder = new Reminder(foundTime, message, e.Channel, e.Author, new Action<Reminder>((Reminder r) => { CleanupReminder(r); }));
			reminder.Activate();
			OutstandingReminders.Add(reminder);
			//SaveReminders();

			await Methods.SendMessage(null, new SendMessageEventArgs {
				Message = $"Okay, I'll tell you this message at {foundTime}",
				Channel = e.Channel,
				LogMessage = "ReminderConfirm"
			});
		}
	}
}
