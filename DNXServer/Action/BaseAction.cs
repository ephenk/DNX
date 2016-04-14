using System;
using Npgsql;
using DNXServer;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;

namespace DNXServer.Action
{
	public abstract class BaseAction: IDisposable
	{
		public const string FAIL_STATUS = @"{""status"": ""FAIL""}";
		
		private string DbHost, DbUser, DbPass, DbPort, DbName;
		private NpgsqlConnection pgConnection;
		private NpgsqlCommand pgCommand;
		private string userId;
		private bool isCommandExecuted;
		private bool isSP;
		
		public BaseAction (string userId)
		{
			this.userId = userId;
			this.isSP = false;
			InitConfig();
		}
		
		public BaseAction ()
		{
			this.userId = "";
			this.isSP = false;
			InitConfig();
		}
		
		private void InitConfig()
		{
			this.DbHost = DNXConfig.DbHost;
			this.DbPort = DNXConfig.DbPort;
			this.DbName = DNXConfig.DbName;
			this.DbUser = DNXConfig.DbUser;
			this.DbPass = DNXConfig.DbPassword;
		}
		
		protected void LogInfo(string msg)
		{
			if(this.userId != "")
			{
				Log.Info(this.userId + ": " + msg);	
			}
			else
			{
				Log.Info(msg);	
			}
		}
		
		protected void LogDebug(string msg)
		{
			if(this.userId != "")
			{
				Log.Debug (this.userId + ": " + msg);	
			}
			else
			{
				Log.Debug(msg);
			}
		}
		
		protected void LogWarning(string msg)
		{
			if (this.userId != "")
			{
				Log.Warning(this.userId + ": " + msg);
			}
			else
			{
				Log.Warning(msg);
			}
		}
		
		protected BaseAction Command(string commandString)
		{
			string connString = "Server=" + DbHost + ";Port=" + DbPort + ";Database=" + DbName + ";User Id=" + DbUser + ";Password=" + DbPass + ";Pooling=false" ;
			pgConnection = new NpgsqlConnection(connString);
			
			pgConnection.Open();

			pgCommand = pgConnection.CreateCommand();
			pgCommand.CommandText = commandString;
			isCommandExecuted = false;
			return this;
		}
		
		protected void AddParameter(string paramName, object val)
		{
			LogInfo("Parameter: " + paramName + " - value: " + val.ToString());
			pgCommand.Parameters.Add(new NpgsqlParameter(paramName, val));
		}

		protected void SilentAddParameter(string paramName, object val)
		{
			//LogInfo("Parameter: " + paramName + " - value: " + val.ToString());
			pgCommand.Parameters.Add(new NpgsqlParameter(paramName, val));
		}

		protected List<object> ExecuteSpRead()
		{
			isSP = true;
			List<object> data = ExecuteRead ();
			isSP = false;
			return data;
		}
		
		protected List<object> ExecuteRead()
		{
			Log.Info ("ExecuteRead: " + pgCommand.CommandText);
			List<object> list = new List<object>();

			if(isSP)
			{
				pgCommand.CommandType = CommandType.StoredProcedure;
			}
			else
			{
				pgCommand.CommandType = CommandType.Text;
			}

			using(NpgsqlDataReader reader = pgCommand.ExecuteReader())
			{
				StringDictionary dict;
				while(reader.Read())
				{
					dict = new StringDictionary();
					for(int i=0; i<reader.FieldCount; i++)
					{
						dict.Add(reader.GetName(i), reader[i].ToString());
					}
					
					list.Add(dict);
				}
			}
			
			isCommandExecuted = true;
			
			return list;
		}

		protected List<object> SilentExecuteRead()
		{
			//Log.Info ("ExecuteRead: " + pgCommand.CommandText);
			List<object> list = new List<object>();

			if(isSP)
			{
				pgCommand.CommandType = CommandType.StoredProcedure;
			}
			else
			{
				pgCommand.CommandType = CommandType.Text;
			}

			using(NpgsqlDataReader reader = pgCommand.ExecuteReader())
			{
				StringDictionary dict;
				while(reader.Read())
				{
					dict = new StringDictionary();
					for(int i=0; i<reader.FieldCount; i++)
					{
						dict.Add(reader.GetName(i), reader[i].ToString());
					}

					list.Add(dict);
				}
			}

			isCommandExecuted = true;

			return list;
		}

		protected StringDictionary ExecuteSpRow()
		{
			isSP = true;
			StringDictionary data = ExecuteRow ();
			isSP = false;
			return data;
		}

		protected StringDictionary SilentExecuteRow()
		{
			//Log.Info ("ExecuteRow: " + pgCommand.CommandText);
			StringDictionary list = null;
			if(isSP)
			{
				pgCommand.CommandType = CommandType.StoredProcedure;
			}
			else
			{
				pgCommand.CommandType = CommandType.Text;
			}

			using(NpgsqlDataReader reader = pgCommand.ExecuteReader())
			{
				if(reader.Read())
				{
					list = new StringDictionary();
					for(int i=0; i<reader.FieldCount; i++)
					{
						//Console.WriteLine (reader.GetName(i));
						list.Add(reader.GetName(i), reader[i].ToString());
					}
				}
			}

			isCommandExecuted = true;

			return list;
		}

		protected StringDictionary ExecuteRow()
		{
			Log.Info ("ExecuteRow: " + pgCommand.CommandText);
			StringDictionary list = null;
			if(isSP)
			{
				pgCommand.CommandType = CommandType.StoredProcedure;
			}
			else
			{
				pgCommand.CommandType = CommandType.Text;
			}

			using(NpgsqlDataReader reader = pgCommand.ExecuteReader())
			{
				if(reader.Read())
				{
					list = new StringDictionary();
					for(int i=0; i<reader.FieldCount; i++)
					{
						//Console.WriteLine (reader.GetName(i));
						list.Add(reader.GetName(i), reader[i].ToString());
					}
				}
			}
			
			isCommandExecuted = true;
			
			return list;
		}

		protected int ExecuteSpWrite()
		{
			isSP = true;
			int result = ExecuteWrite ();
			isSP = false;
			return result;
		}
		
		protected int ExecuteWrite()
		{
			Log.Info ("ExecuteWrite: " + pgCommand.CommandText);
			isCommandExecuted = true;
			if(isSP)
			{
				pgCommand.CommandType = CommandType.StoredProcedure;
			}
			else
			{
				pgCommand.CommandType = CommandType.Text;
			}
			return pgCommand.ExecuteNonQuery();
		}
		
		public void Dispose ()
		{
			if (!isCommandExecuted)
			{
				
			}
			pgCommand.Dispose();
			pgConnection.Close();
		}

		protected string ExecuteSingleRow()
		{
			string result ="";
			Log.Info ("ExecuteSingleRow: " + pgCommand.CommandText);

			using(NpgsqlDataReader reader = pgCommand.ExecuteReader())
			{
				if(reader.Read())
				{
					result = reader.GetName (0);		
				}
			}
			return result;
		}
	}
}

