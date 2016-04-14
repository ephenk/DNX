using System;
using System.Text;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace DNXServer.Action
{
	public class InventoryAction:BaseAction
	{
		public InventoryAction ()
		{
		}

		public string GetInventory(SessionData so, string qrid)
		{
			//RetrieveMonsterInventory`QRID`
			List<string> monster = new List<string>();
			StringBuilder sb = new StringBuilder ();

			//RETURN: ReceiveMonsterInventory`flegs~rlegs~tails~wings~weapons~`
			//		  ReceiveMonsterInventory`flegs0!flegs1!flegs2!~rlegs0!rlegs1!rlegs2!~tails0!tails1!tails2!~wings0!wings1!wings2!~weapons0!weapons1!weapons2!`
		
			using (Command("server_get_monster_f_legs"))
			{
				AddParameter ("qrid",qrid);

				foreach(StringDictionary item in ExecuteSpRead())
				{
					List<string> itemList = new List<string>();
					itemList.Add(item["_monster_type_id"]);

					string join = String.Join ("!", itemList);
					monster.Add(join);
				}

			}
			string f_legs = String.Join ("!", monster);
			sb.Append (f_legs).Append ("~");

			using (Command("server_get_monster_r_legs"))
			{
				AddParameter ("qrid",qrid);
				monster.Clear ();
				foreach(StringDictionary item in ExecuteSpRead())
				{
					List<string> itemList = new List<string>();
					itemList.Add(item["_monster_type_id"]);

					string join = String.Join ("!", itemList);
					monster.Add(join);
				}

			}
			string r_legs = String.Join ("!", monster);
			sb.Append (r_legs).Append ("~");

			using (Command("server_get_monster_tails"))
			{
				AddParameter ("qrid",qrid);
				monster.Clear ();
				foreach(StringDictionary item in ExecuteSpRead())
				{
					List<string> itemList = new List<string>();
					itemList.Add(item["_monster_type_id"]);

					string join = String.Join ("!", itemList);
					monster.Add(join);
				}

			}
			string tail = String.Join ("!", monster);
			sb.Append (tail).Append ("~");

			using (Command("server_get_monster_wings_int"))
			{
				AddParameter ("qrid",qrid);
				monster.Clear ();
				foreach(StringDictionary item in ExecuteSpRead())
				{
					List<string> itemList = new List<string>();
					itemList.Add(item["_wings_int"]);

					string join = String.Join ("!", itemList);
					monster.Add(join);
				}
			}
			string wing = String.Join ("!", monster);
			sb.Append (wing).Append ("~");

			using (Command("server_get_monster_weapons"))
			{
				AddParameter ("qrid",qrid);
				monster.Clear ();
				foreach(StringDictionary item in ExecuteSpRead())
				{
					List<string> itemList = new List<string>();
					itemList.Add(item["_weapon"]);

					string join = String.Join ("!", itemList);
					monster.Add(join);
				}
			}

			string weapon = String.Join ("!", monster);
			sb.Append (weapon);

			return (int)CommandResponseEnum.MonsterInventory + "`" + sb.ToString () + "`";

		}
	}
}

