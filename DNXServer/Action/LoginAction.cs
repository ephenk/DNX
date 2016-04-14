using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Threading;

namespace DNXServer.Action
{
	public class LoginAction: BaseAction
	{
		public LoginAction()
		{

		}

		public static List <SessionData> ListUser = new List<SessionData>();

		public string Login(SessionData so, string accessToken, string id, string source, string version)
		{
			//used for reproduce error on disconnected client when the server is processing data
//			int millisecondsToWait = 5000;
//			Stopwatch stopwatch = Stopwatch.StartNew();
//			while (true)
//			{
//				//some other processing to do STILL POSSIBLE
//				if (stopwatch.ElapsedMilliseconds >= millisecondsToWait)
//				{
//					break;
//				}
//				Thread.Sleep(1); //so processor can rest for a while
//			}

			StringDictionary result;
			bool newUser = false;
			string trialQRID;
			//bool upToDate = false;
			System.Text.StringBuilder builder = new System.Text.StringBuilder ();
			List<string> userData = new List<string> ();
			List<string> newsUrlCompile = new List<string>();
			List<string> modelUrlCompile = new List<string>();
			List<string> larvaUrlCompile = new List<string>();
			List<string> weaponUrlCompile = new List<string>();
			List<string> wingUrlCompile = new List<string>();
			List<string> effectUrlCompile = new List<string> ();
			List<string> foodUrlCompile = new List<string> ();
	
			LoginSourceEnum sourceEnum;
			if(!Enum.TryParse(source, true, out sourceEnum))
			{
				return null;
			}

			// check the facebook access token

			if (source == "FB") 
			{
				string fbResult = new WebsiteRequest ().CheckFBID (accessToken);
				//string fbResult = id + "~" + "null" + "~" + "null";
				var split = fbResult.Split ('~');
				string fbid = split [0];
				string fbname = split [1];
				string fbpicture = split [2];

				if (fbid == "-1") {
					return((int)CommandResponseEnum.Error).ToString () + "`" + "-111~FB Access Token Checking Error" + "`";
				} else if (fbid != id) {
					return((int)CommandResponseEnum.Error).ToString () + "`" + "-222~Invalid FB login" + "`";
				} else {
					so.Username = fbname;
					so.FacebookPicture = fbpicture;
				}
			}

			using (Command("server_get_user_data")) {
				AddParameter ("external", id);
				AddParameter ("source", sourceEnum.ToString ());

				result = ExecuteSpRow ();
			}

			if (result == null) {
				using (Command("server_register_user")) {
					AddParameter ("external", id);
					AddParameter ("source", sourceEnum.ToString ());
					
					result = ExecuteSpRow ();
				}

				// get the login once again
				using (Command(@"server_get_user_data")) {
					AddParameter ("external", id);
					AddParameter ("source", sourceEnum.ToString ());
					
					result = ExecuteSpRow ();
				}
				newUser = true;
			}

			so.UserId = Convert.ToInt32 (result ["_user_id"]);
			so.UserCoin = Convert.ToInt32 (result ["_mobile_coins"]);
			so.ItemSlot = Convert.ToInt16 (result ["_mobile_battle_item_slot_count"]);
			so.Flee = Convert.ToInt32(result["_flee"]);
			string userdata = so.UserId + "`" + so.UserCoin + "`" + so.ItemSlot;

			//check is user already login on another device
			foreach (SessionData ses in ListUser)
			{
				//check each socket
				if (ses.UserId == so.UserId) 
				{
					string loginError = ((int)CommandResponseEnum.Error).ToString() + "`" +"-999~Duplicate login"+"`";
					List<byte> temp = new List<byte> ();
					temp.AddRange (Encoding.ASCII.GetBytes(loginError));
					temp.Add (0x03);
					byte[] data = temp.ToArray();

					// send to first device that user logged on
					try
					{
						ses.WriteLine(loginError);
						ses.Sock.Send(data);
					}
					catch(Exception e) 
					{
						StringBuilder str = new StringBuilder();

						str.AppendLine("\n----------------------------------\n\nDisconnecting client because error occured at " + DateTime.Now);
						str.AppendLine("Message: " + e.Message);
						str.AppendLine("Source: " + e.Source);
						str.AppendLine("Stack Trace:");
						str.AppendLine(e.StackTrace);

						Log.Error(str.ToString());
					}
				}
			}
			ListUser.Add (so);

			// give trial monster card for new user
			if (newUser) {
				// create new monster
				using (Command("server_add_trial_monster_card")) {
					AddParameter ("monster", 0);
					AddParameter ("version", 2);
					AddParameter ("subversion", 0);
					result = ExecuteSpRow ();
					trialQRID = result["server_add_trial_monster_card"];
				}

				//assign new monster to user
				using (Command("server_give_trial_monster_card")) {
					AddParameter ("qrid", trialQRID);
					AddParameter ("userid", so.UserId );
					ExecuteSpRow ();
				}

			}

			// get news
			using (Command(@"
				SELECT img_url,url
				 FROM server_news
			")) 
			{
				foreach(StringDictionary news in ExecuteRead())
				{
					List<string> newsList = new List<string>();
					newsList.Add(news["img_url"]);
					newsList.Add(news["url"]);

					string newsResult = String.Join("`", newsList);
					newsUrlCompile.Add(newsResult);
				}
			}
			string newsUrl = String.Join ("`",newsUrlCompile);

			using (Command(@"
				SELECT model_url, model_version
				 FROM monster_template
				 WHERE model_url IS NOT NULL
				 ORDER BY mon_tmplt_id 
			")) 
			{
				foreach(StringDictionary murl in ExecuteRead())
				{
					List<string> murlList = new List<string>();
					murlList.Add(murl["model_url"]);
					murlList.Add(murl["model_version"]);

					string murlResult = String.Join("!", murlList);
					modelUrlCompile.Add(murlResult);
				}
			}
			string modelUrl = String.Join ("~",modelUrlCompile);

			using (Command(@"
				SELECT larva_url, larva_version
				 FROM monster_template
				 WHERE larva_url IS NOT NULL
				 ORDER BY mon_tmplt_id 
			")) 
			{
				foreach(StringDictionary lurl in ExecuteRead())
				{
					List<string> lurlList = new List<string>();
					lurlList.Add(lurl["larva_url"]);
					lurlList.Add(lurl["larva_version"]);

					string lurlResult = String.Join("!", lurlList);
					larvaUrlCompile.Add(lurlResult);
				}
			}
			string larvaUrl = String.Join ("~",larvaUrlCompile);

			using (Command(@"
				SELECT weapon_url, weapon_version
				 FROM weapons
				 WHERE weapon_url IS NOT NULL
				 ORDER BY weapon_id 
			")) 
			{
				foreach(StringDictionary weapon in ExecuteRead())
				{
					List<string> weaponUrlList = new List<string>();
					weaponUrlList.Add(weapon["weapon_url"]);
					weaponUrlList.Add(weapon["weapon_version"]);

					string weaponurlResult = String.Join("!", weaponUrlList);
					weaponUrlCompile.Add(weaponurlResult);
				}
			}
			string weaponUrl = String.Join ("~",weaponUrlCompile);

			using (Command(@"
				SELECT wings_url, wings_version
				 FROM wings
				 WHERE wings_url IS NOT NULL
				 ORDER BY wings_id 
			")) 
			{
				foreach(StringDictionary wing in ExecuteRead())
				{
					List<string> wingUrlList = new List<string>();
					wingUrlList.Add(wing["wings_url"]);
					wingUrlList.Add(wing["wings_version"]);

					string wingurlResult = String.Join("!", wingUrlList);
					wingUrlCompile.Add(wingurlResult);
				}
			}
			string wingUrl = String.Join ("~",wingUrlCompile);

			using (Command(@"
				SELECT effect_url, effect_version
				 FROM effects
				 WHERE effect_url IS NOT NULL
				 ORDER BY effect_id 
			")) 
			{
				foreach(StringDictionary eUrl in ExecuteRead())
				{
					List<string> effectList = new List<string>();
					effectList.Add(eUrl["effect_url"]);
					effectList.Add(eUrl["effect_version"]);

					string effectResult = String.Join("!", effectList);
					effectUrlCompile.Add(effectResult);
				}
			}
			string effectUrl = String.Join ("~",effectUrlCompile);

			using (Command(@"
				SELECT food_url, food_version
				 FROM foods
				 WHERE food_url IS NOT NULL
				 ORDER BY food_id
			")) 
			{
				foreach(StringDictionary fUrl in ExecuteRead())
				{
					List<string> foodList = new List<string>();
					foodList.Add(fUrl["food_url"]);
					foodList.Add(fUrl["food_version"]);

					string foodResult = String.Join("!", foodList);
					foodUrlCompile.Add(foodResult);
				}
			}
			string foodUrl = String.Join ("~",foodUrlCompile);

			return ((int)CommandResponseEnum.LoginResult).ToString() + "`" + userdata + "`" + newsUrl + "`" + modelUrl + "`" + larvaUrl + "`" + weaponUrl + "`" + wingUrl + "`" + effectUrl + "`" + foodUrl + "`";
					
		}

		public void Logout(SessionData so)
		{
			so.UserId = 0;
			so.UserCoin = 0;
			so.Username = null;
			so.FacebookId = null;
			so.BattleId = 0;
			so.Version = null;
			so.IsRoomMaster = false;
			so.IsRoomGuest = false;
			so.RoomId = 0;
			so.InBattle = false;
			Log.Info("Flush session data");
		}
	}
}

