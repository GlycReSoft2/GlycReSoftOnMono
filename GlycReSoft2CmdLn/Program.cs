using System;
using System.Collections.Generic;
using System.IO;
using ManyConsole;
using NDesk.Options;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;

namespace GlycReSoft
{
	class MainClass
	{
		public static int Main (string[] args)
		{
			try {
				var commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs (typeof(MainClass));
				int exitCode = ConsoleCommandDispatcher.DispatchCommand (commands, args, Console.Out);
				return exitCode;
			} catch (Exception ex) {
				Utils.Warn (ex);
				Console.Read ();
				return -1;
			}
		}
	}


	static class Utils
	{
		public static void Warn (params object[] messages)
		{
			ConsoleColor prior = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			foreach (object msg in messages) {
				Console.WriteLine (msg);
			}
			Console.ForegroundColor = prior;
		}

		public static void Debug (params object[] messages)
		{
			ConsoleColor prior = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			foreach (object msg in messages) {
				Console.WriteLine (msg);
			}
			Console.ForegroundColor = prior;
		}
	}

}
