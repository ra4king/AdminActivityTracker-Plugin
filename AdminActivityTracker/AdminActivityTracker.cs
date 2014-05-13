using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;

namespace PRoConEvents
{
	class AdminActivityTracker : PRoConPluginAPI, IPRoConPluginInterface
	{
		private bool pluginEnabled;
		private bool debug = false;

		public AdminActivityTracker()
		{

		}

		#region Console Methods

		public enum ConsoleMessageType { Warning, Error, Exception, Normal, Debug };

		private string FormatMessage(string msg, ConsoleMessageType type)
		{
			string prefix = "[^b" + GetPluginName() + "^n] ";

			switch (type)
			{
				case ConsoleMessageType.Warning:
					prefix += "^1^bWARNING^0^n: ";
					break;
				case ConsoleMessageType.Error:
					prefix += "^1^bERROR^0^n: ";
					break;
				case ConsoleMessageType.Exception:
					prefix += "^1^bEXCEPTION^0^n: ";
					break;
				case ConsoleMessageType.Debug:
					prefix += "^1^bDEBUG^0^n: ";
					break;
			}

			return prefix + msg;
		}

		public void LogWrite(string msg)
		{
			this.ExecuteCommand("procon.protected.pluginconsole.write", msg);
		}

		public void ConsoleWrite(string msg, ConsoleMessageType type)
		{
			LogWrite(FormatMessage(msg, type));
		}

		public void ConsoleWrite(string msg)
		{
			ConsoleWrite(msg, ConsoleMessageType.Normal);
		}

		public void ConsoleDebug(string msg)
		{
			if (debug)
				ConsoleWrite(msg, ConsoleMessageType.Debug);
		}

		public void ConsoleWarn(string msg)
		{
			ConsoleWrite(msg, ConsoleMessageType.Warning);
		}

		public void ConsoleError(string msg)
		{
			ConsoleWrite(msg, ConsoleMessageType.Error);
		}

		public void ConsoleException(string msg)
		{
			ConsoleWrite(msg, ConsoleMessageType.Exception);
		}

		public void AdminSayAll(string msg)
		{
			if (debug)
				ConsoleDebug("Saying to all: " + msg);

			foreach (string s in splitMessage(msg, 128))
				this.ExecuteCommand("procon.protected.send", "admin.say", s, "all");
		}

		public void AdminSayTeam(string msg, int teamID)
		{
			if (debug)
				ConsoleDebug("Saying to Team " + teamID + ": " + msg);

			foreach (string s in splitMessage(msg, 128))
				this.ExecuteCommand("procon.protected.send", "admin.say", s, "team", string.Concat(teamID));
		}

		public void AdminSaySquad(string msg, int teamID, int squadID)
		{
			if (debug)
				ConsoleDebug("Saying to Squad " + squadID + " in Team " + teamID + ": " + msg);

			foreach (string s in splitMessage(msg, 128))
				this.ExecuteCommand("procon.protected.send", "admin.say", s, "squad", string.Concat(teamID), string.Concat(squadID));
		}

		public void AdminSayPlayer(string msg, string player)
		{
			if (debug)
				ConsoleDebug("Saying to player '" + player + "': " + msg);

			foreach (string s in splitMessage(msg, 128))
				this.ExecuteCommand("procon.protected.send", "admin.say", s, "player", player);
		}

		public void AdminYellAll(string msg)
		{
			AdminYellAll(msg, 10);
		}

		public void AdminYellAll(string msg, int duration)
		{
			if (msg.Length > 256)
				ConsoleError("AdminYell msg > 256. msg: " + msg);

			if (debug)
				ConsoleDebug("Yelling to all: " + msg);

			this.ExecuteCommand("procon.protected.send", "admin.yell", msg, string.Concat(duration), "all");
		}

		public void AdminYellTeam(string msg, int teamID)
		{
			AdminYellTeam(msg, teamID, 10);
		}

		public void AdminYellTeam(string msg, int teamID, int duration)
		{
			if (msg.Length > 256)
				ConsoleError("AdminYell msg > 256. msg: " + msg);

			if (debug)
				ConsoleDebug("Yelling to Team " + teamID + ": " + msg);

			this.ExecuteCommand("procon.protected.send", "admin.yell", msg, string.Concat(duration), "team", string.Concat(teamID));
		}

		public void AdminYellSquad(string msg, int teamID, int squadID)
		{
			AdminYellSquad(msg, teamID, squadID, 10);
		}

		public void AdminYellSquad(string msg, int teamID, int squadID, int duration)
		{
			if (msg.Length > 256)
				ConsoleError("AdminYell msg > 256. msg: " + msg);

			if (debug)
				ConsoleDebug("Yelling to Squad " + squadID + " in Team " + teamID + ": " + msg);

			this.ExecuteCommand("procon.protected.send", "admin.yell", msg, string.Concat(duration), "squad", string.Concat(teamID), string.Concat(squadID));
		}

		public void AdminYellPlayer(string msg, string player)
		{
			AdminYellPlayer(msg, player, 10);
		}

		public void AdminYellPlayer(string msg, string player, int duration)
		{
			if (msg.Length > 256)
				ConsoleError("AdminYell msg > 256. msg: " + msg);

			if (debug)
				ConsoleDebug("Yelling to player '" + player + "': " + msg);

			this.ExecuteCommand("procon.protected.send", "admin.yell", msg, string.Concat(duration), "player", player);
		}

		// This method splits the given string if it exceeds the maxSize by, in order of precedence: newlines, end-of-sentence punctuation marks, commas, spaces, or arbitrarily.</p>
		private static List<string> splitMessage(string message, int maxSize)
		{
			List<string> messages = new List<string>(message.Replace("\r", "").Trim().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

			for (int a = 0; a < messages.Count; a++)
			{
				messages[a] = messages[a].Trim();

				if (messages[a] == "")
				{
					messages.RemoveAt(a);
					a--;
					continue;
				}

				if (messages[a][0] == '/')
					messages[a] = ' ' + messages[a];

				string msg = messages[a];

				if (msg.Length > maxSize)
				{
					List<int> splitOptions = new List<int>();
					int split = -1;
					do
					{
						split = msg.IndexOfAny(new char[] { '.', '!', '?', ';' }, split + 1);
						if (split != -1 && split != msg.Length - 1)
							splitOptions.Add(split);
					} while (split != -1);

					if (splitOptions.Count > 2)
						split = splitOptions[(int)Math.Round(splitOptions.Count / 2.0)] + 1;
					else if (splitOptions.Count > 0)
						split = splitOptions[0] + 1;
					else
					{
						split = msg.IndexOf(',');

						if (split == -1)
						{
							split = msg.IndexOf(' ', msg.Length / 2);

							if (split == -1)
							{
								split = msg.IndexOf(' ');

								if (split == -1)
									split = maxSize / 2;
							}
						}
					}

					messages[a] = msg.Substring(0, split).Trim();
					messages.Insert(a + 1, msg.Substring(split).Trim());

					a--;
				}
			}

			return messages;
		}

		#endregion

		#region Plugin Config

		public string GetPluginName()
		{
			return "Admin Activity Tracker";
		}

		public string GetPluginVersion()
		{
			return "0.0.1";
		}

		public string GetPluginAuthor()
		{
			return "ra4king";
		}

		public string GetPluginWebsite()
		{
			return "purebattlefield.org";
		}

		public string GetPluginDescription()
		{
			return @"<h1>" + GetPluginName() + @"</h1>

<p>This is a sample description</p>
";
		}

		public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
		{
			this.RegisterEvents(this.GetType().Name, "OnGlobalChat", "OnTeamChat", "OnSquadChat");
		}

		public void OnPluginEnable()
		{
			this.pluginEnabled = true;

			ConsoleWrite("^2" + GetPluginName() + " Enabled");
		}

		public void OnPluginDisable()
		{
			this.pluginEnabled = false;

			ConsoleWrite("^8" + GetPluginName() + " Disabled");
		}

		#endregion

		public List<CPluginVariable> GetDisplayPluginVariables()
		{
			List<CPluginVariable> variables = new List<CPluginVariable>();

			variables.Add(new CPluginVariable("Debug", typeof(bool), debug));

			return variables;
		}

		public List<CPluginVariable> GetPluginVariables()
		{
			return GetDisplayPluginVariables();
		}

		public void SetPluginVariable(string variable, string value)
		{
			if (variable.Equals("Debug"))
				debug = bool.Parse(value);
		}

		public override void OnGlobalChat(string speaker, string message)
		{
			if (speaker.Equals("Server"))
			{

			}
			else
			{

			}
		}

		public override void OnTeamChat(string speaker, string message, int teamId)
		{
			OnGlobalChat(speaker, message);
		}

		public override void OnSquadChat(string speaker, string message, int teamId, int squadId)
		{
			OnGlobalChat(speaker, message);
		}
	}
}
