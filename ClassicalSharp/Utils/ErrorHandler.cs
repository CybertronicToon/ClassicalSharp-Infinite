﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using OpenTK;

namespace ClassicalSharp {
	
	/// <summary> Displays a message box when an unhandled exception occurs
	/// and also logs it to a specified log file. </summary>
	public static class ErrorHandler {

		static string logFile = "crash.log";
		static string fileName = "crash.log";
		
		/// <summary> Adds a handler for when a unhandled exception occurs, unless
		/// a debugger is attached to the process in which case this does nothing. </summary>
		public static void InstallHandler(string logFile) {
			ErrorHandler.logFile = logFile;
			fileName = Path.GetFileName(logFile);
			if (!Debugger.IsAttached)
				AppDomain.CurrentDomain.UnhandledException += UnhandledException;
		}
		
		/// <summary> Additional text that should be logged to the log file
		/// when an unhandled exception occurs. </summary>
		public static string[] AdditionalInfo;
		
		static string Format(Exception ex) {
			return ex.GetType().FullName + ": " + ex.Message
				+ Environment.NewLine + ex.StackTrace;
		}

		static void UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			// So we don't get the normal unhelpful crash dialog on Windows.
			Exception ex = (Exception)e.ExceptionObject;
			bool wroteToCrashLog = true;
			try {
				using (StreamWriter w = new StreamWriter(logFile, true)) {
					w.WriteLine("=== crash occurred ===");
					w.WriteLine("Time: " + DateTime.Now);
					
					string platform = Configuration.RunningOnMono ? "Mono " : ".NET ";
					platform += Environment.Version;
					
					if (Configuration.RunningOnWindows) platform += ", Windows - ";
					if (Configuration.RunningOnMacOS) platform += ", OSX - ";
					if (Configuration.RunningOnLinux) platform += ", Linux - ";
					platform += Environment.OSVersion.Version.ToString();
					w.WriteLine("Running on: " + platform);
					
					while (ex != null) {
						w.WriteLine(Format(ex));
						w.WriteLine();
						ex = ex.InnerException;
					}
					
					if (AdditionalInfo != null) {
						for (int i = 0; i < AdditionalInfo.Length; i++)
							w.WriteLine(AdditionalInfo[i]);
						w.WriteLine();
					}
				}
			} catch (Exception) {
				wroteToCrashLog = false;
			}
			
			string line1 = "ClassicalSharp crashed.";
			if (wroteToCrashLog) {
				line1 += " The cause has been logged to \"" + fileName + "\" in " + Program.AppDirectory;
			}
			string line2 = "Please report the crash to github.com/UnknownShadow200/ClassicalSharp/issues so we can fix it.";
			if (!wroteToCrashLog) {
				line2 += Environment.NewLine + Environment.NewLine + Format(ex);
			}

			MessageBox.Show(line1 + Environment.NewLine + Environment.NewLine + line2, "We're sorry");
			Environment.Exit(1);
		}
		
		/// <summary> Logs a handled exception that occured at the specified location to the log file. </summary>
		public static bool LogError(string location, Exception ex) {
			string error = DescribeException(ex);
			if (ex.InnerException != null) {
				error += Environment.NewLine + DescribeException(ex.InnerException);
			}
			return LogError(location, error);
		}
		
		static string DescribeException(Exception ex) {
			return ex.GetType().FullName + ": " + ex.Message + Environment.NewLine + ex.StackTrace;
		}
		
		/// <summary> Logs an error that occured at the specified location to the log file. </summary>
		public static bool LogError(string location, string text) {
			try {
				using (StreamWriter writer = new StreamWriter(logFile, true)) {
					writer.WriteLine("=== handled error ===");
					writer.WriteLine("Occured when: " + location);
					writer.WriteLine(text);
					writer.WriteLine();
				}
			} catch (Exception) {
				return false;
			}
			return true;
		}
	}
}