using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DNXServer
{
	public static class DNXConfig
	{
		public static string DbHost { get; set; }
		public static string DbUser { get; set; }
		public static string DbPassword { get; set; }
		public static string DbName { get; set; }
		public static string DbPort { get; set; }
		public static bool Compression { get; set; }
		public static bool ShowInput { get; set; }
		public static bool ShowOutput { get; set; }
		public static string ConsoleHost { get; set; }
		public static int ConsolePort { get; set; }
		public static void ReadConfigFile()
		{
			if(!File.Exists("Config/config.ini"))
			if(!File.Exists("config.ini"))
			{
				Log.Error(@"File config.ini in ""Config"" is not found");
				throw new Exception(@"File config.ini in ""Config"" is not found");
			}

//			DbHost = "58.65.245.154";
//			DbPort = "5432";
//			DbName = "dnx";
//			DbPassword = "ananta88.";
//			DbUser = "postgres";
//			Compression = false;
//			ShowInput = true;
//			ShowOutput = false;
//			ConsoleHost = "127.0.0.1";
//			ConsolePort = 11111;
			
			using(StreamReader file = new StreamReader("Config/config.ini", Encoding.Default))
			{
				while(!file.EndOfStream)
				{
					string line = file.ReadLine();
				
					//Regex categories = new Regex(@"\s*\[*.?\]\s");
					Regex reg = new Regex(@"\s*(\S+)\s*=\s*(\S+)\s*");
					
					Match match = reg.Match(line);
					
					if(match.Success)
					{
						string key = match.Groups[1].Value.ToUpper();
						string value = match.Groups[2].Value;
						
						Log.Info(key + " = " + value);
						
						switch(key)
						{
						case "DBHOST":
							DbHost = value;
							break;
							
						case "DBPORT":
							DbPort = value;
							break;
							
						case "DBUSER":
							DbUser = value;
							break;
							
						case "DBPASS":
							DbPassword = value;
							break;
							
						case "DBNAME":
							DbName = value;
							break;
							
						case "COMPRESSION":
							Compression = (value == "0" ? false : true);
							break;
							
						case "SHOW_INPUT":
							ShowInput = (value == "0" ? false : true);
							break;
							
						case "SHOW_OUTPUT":
							ShowOutput = (value == "0" ? false : true);
							break;

						case "CONSOLEHOST":
							ConsoleHost = value;
							break;

						case "CONSOLEPORT":
							ConsolePort = Int32.Parse(value);
							break;
						}
					}
					
				}
			}
			
		}
	}
}

