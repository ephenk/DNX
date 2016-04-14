using System;
using DNXServer.Action;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using Mono.Math;
using System.IO;
using System.Linq.Expressions;
using Mono.Security.Protocol.Ntlm;

namespace DNXServer
{
	public class ShopAction : BaseAction
	{
		public ShopAction ()
		{
		}

		public string RetrieveShopItems(SessionData so)
		{
			string shopData;
			using(Command("server_get_shop_items"))
			{
				List<string> shoplist  = new List<string>();
				foreach(StringDictionary result in ExecuteSpRead())
				{
					List<string> shop = new List<string>();
					shop.Add (result ["_item_id"]);
					shop.Add (result ["_item_name"]);
					shop.Add (result ["_item_price"]);
					shop.Add (result ["_item_desc"]);
					shop.Add (result ["_img_url"]);
					shop.Add (result ["_category"]);

					string shopString = String.Join ("~", shop);
					shoplist.Add (shopString);
				}
				shopData = String.Join ("`", shoplist);
			}
			return (int)CommandResponseEnum.LoadShopItems + "`" + shopData + "`";
		}

		public string GenerateCards(int pack_id, int user_id)
		{
			string monster_generated = string.Empty;
			string item_generated = string.Empty;

			string card_pack_result = string.Empty;
			StringDictionary sd = new StringDictionary ();

			using(Command("server_get_card_pack"))
			{
				AddParameter ("id", pack_id);
				sd = ExecuteSpRow ();
			}

			var card_qty = sd ["_card_quantity"].Split (',');
			var card_type = sd ["_contains"].Split('+');
			var card_category1 = card_type [0].Split (';'); 
			var card_category2 = card_type [1].Split (';');
			string[] monster_type_id = new string[card_category1.Length];
			int[] monster_type_id_chance = new int[card_category1.Length];
			string[] monster_version = new string[card_category1.Length];
			string[] item_type_id = new string[card_category2.Length];
			int[] item_type_id_chance = new int[card_category2.Length];

			string[] mons_id_result = new string[Convert.ToInt32(card_qty[0])];
			string[] item_id_result = new string[Convert.ToInt32(card_qty[1])];
			string[] mons_ver_result = new string[Convert.ToInt32(card_qty[0])];

			if(card_category1.Length > 1)
			{
				int count = 0;
				for(int i=0;i<card_category1.Length - 1;i++)
				{
					var sp = Convert.ToString(card_category1 [i]).Split (',');

					monster_type_id.SetValue (sp[0], i);
					monster_version.SetValue (Convert.ToString(sp[1]), i);

					if(monster_type_id[i].Contains("/"))
					{
						var sp2 = Convert.ToString (monster_type_id [i]).Split ('/');
						monster_type_id.SetValue (sp2 [0], i);
						monster_type_id_chance.SetValue (Convert.ToInt32(sp2 [1]), i);
					}
				}
					
				while(count < Convert.ToInt32(card_qty[0]))
				{
					if(monster_type_id_chance[0] != 0)
					{
						int[] possible = new int[monster_type_id_chance.Length];
						int[] possible2 = new int[monster_type_id_chance.Length];
						int[] possible3 = new int[monster_type_id_chance.Length];
						int[] same_chance = new int[monster_type_id_chance.Length];
						int[] same_chance2 = new int[monster_type_id_chance.Length];
						int[] same_chance3 = new int[monster_type_id_chance.Length];

						int roll = Util.RandomCardPackInt (1,100);
//						Console.WriteLine ("Curr ROLL: " + roll);

						for(int j=0;j<monster_type_id.Length-1;j++)
						{
							if(roll <= Convert.ToInt32(monster_type_id_chance[j]) )
							{
//								Console.WriteLine ("Pos value: " + monster_type_id [j] + " Pos Roll needed: " + monster_type_id_chance [j]);
								possible.SetValue (Convert.ToInt32 (monster_type_id [j]), j);
								possible2.SetValue (Convert.ToInt32 (monster_version [j]), j);
								possible3.SetValue (Convert.ToInt32 (monster_type_id_chance [j]), j);
							}
						}

						possible =  possible.Where(x => x != 0).ToArray();
						possible2 =  possible2.Where(x => x != 0).ToArray();
						possible3 =  possible3.Where(x => x != 0).ToArray();

//						Console.WriteLine ("Possible: ");
//
//						foreach(var l in possible)
//						{
//							Console.WriteLine (l);
//						}
//
//						Console.WriteLine ("Possible2: ");
//
//						foreach(var k in possible2)
//						{
//							Console.WriteLine (k);
//						}
//
//						Console.WriteLine ("Possible3: ");
//
//						foreach(var j in possible3)
//						{
//							Console.WriteLine (j);
//						}

						if(possible3.Length > 1)
						{
							if(possible3[0] != 0 && possible3 [1] != 0)
							{
								// check klo yg paling kecil ada yg sama
								for(int k=0; k<possible3.Length;k++)
								{
									if(possible3[k] == possible3.Min() && possible3[k] != 0)
									{
//										Console.WriteLine ("Writing to same chance");
										same_chance.SetValue (possible [k], k);
										same_chance2.SetValue (possible2 [k], k);
										same_chance3.SetValue (possible3 [k], k);
									}
								}

								same_chance =  same_chance.Where(x => x != 0).ToArray();
								same_chance2 =  same_chance2.Where(x => x != 0).ToArray();
								same_chance3 =  same_chance3.Where(x => x != 0).ToArray();

//								Console.WriteLine ("same_chance");
//								foreach (var k in same_chance)
//								{
//									Console.WriteLine (k);
//								}
//
//								Console.WriteLine ("same_chance2");
//								foreach (var k in same_chance2)
//								{
//									Console.WriteLine (k);
//								}
//
//								Console.WriteLine ("same_chance3");
//								foreach (var k in same_chance3)
//								{
//									Console.WriteLine (k);
//								}

								if(same_chance3.Length > 1)
								{
//									Console.WriteLine ("Same chance value Exist! Proceed with roll 3");									 
//									Console.WriteLine("Same chance 3 length: " + (same_chance3.Length - 1));
									int roll3 = Util.RandomCardPackInt (0, same_chance3.Length-1);
//									Console.WriteLine("roll3: " + roll3 );
//									Console.WriteLine ("Rool3 result: " + roll3 + " Mons ID: " + same_chance [roll3] + " Ver: " + same_chance2 [roll3] + " Chance: " + same_chance3[roll3]);
									mons_id_result.SetValue (Convert.ToString(same_chance [roll3]),count);
									mons_ver_result.SetValue (Convert.ToString(same_chance2 [roll3]), count);
								}
								else
								{
//									Console.WriteLine ("ONE");
									int minValue = possible3.Min();
									int minIndex = possible3.ToList().IndexOf(minValue);
									mons_id_result.SetValue (Convert.ToString(possible [minIndex]),count);
									mons_ver_result.SetValue (Convert.ToString(possible2 [minIndex]), count);
								}
							}							 
						}
						else
						{
							// cari chance yg paling besar, blom di handel klo ada 2 yg paling besar
							int maxValue = monster_type_id_chance.Max () ;
							int maxIndex = monster_type_id_chance.ToList().IndexOf(maxValue);
//							Console.WriteLine ("Roll too high! Monsert Card ID result: " + monster_type_id [maxIndex] + " Ver: " + monster_version [maxIndex]);
							mons_id_result.SetValue (monster_type_id [maxIndex],count);
							mons_ver_result.SetValue (monster_version [maxIndex], count);

						}
					}
					else
					{
						// semua card punya chance yg sama untuk ada di dlm sebuah card pack					    
						int roll = Util.RandomCardPackInt (0, Convert.ToInt32(monster_type_id.Length-1));
//						Console.WriteLine ("Monster Spawned ID: " + monster_type_id[roll] + " Ver: " + monster_version [roll]);
						mons_id_result.SetValue (monster_type_id [roll],count);
						mons_ver_result.SetValue (monster_version [roll], count);
					}

//					Console.WriteLine ("Next Card \n");
					count++;
				}

//				Console.WriteLine ("isi Monter type ID result");
				List<string> mons = new List<string> ();

				for(int i = 0; i<mons_id_result.Length ;i++)
				{
					Console.WriteLine("Monster Id: " + mons_id_result[i] + " Version: " + mons_ver_result[i]);
					using(Command("server_add_monster_card_to_user"))
					{
						AddParameter ("id", mons_id_result [i]);
						AddParameter ("version", mons_ver_result [i].Substring (0, 1));
						AddParameter ("subversion", mons_ver_result [i].Substring (1, 1));
						AddParameter ("userid", user_id);
						StringDictionary mon = ExecuteSpRow ();
						mons.Add (mon["_qrid"] + "~0~" + mon["_tmplt_id"] + "~" + mon["_version"] + "~" + mon["_subversion"]);
					}
				}
				//monster_qrid~0~monster_tmplt_id~version~subversion
				monster_generated = string.Join ("`", mons);
			}

			if (card_category2.Length > 1)
			{
				int count = 0;
				for (int i = 0; i < card_category2.Length; i++) {
					var sp = Convert.ToString (card_category2 [i]).Split (',');
					item_type_id.SetValue (sp [0], i);
					if (item_type_id [i].Contains ("/")) {
						var sp2 = Convert.ToString (item_type_id [i]).Split ('/');
						item_type_id.SetValue (sp2 [0], i);
						item_type_id_chance.SetValue (Convert.ToInt32(sp2[1]),i);
					}
				}

//				Console.Write ("Jumlah type item card: ");
//				Console.WriteLine (item_type_id.Length-1);
//				Console.WriteLine ("Jumlah item card dlm pack: " + card_qty [1] + "\n");
				//misal isi pack ada 5 berarti 5x loop
				while(count < Convert.ToInt32(card_qty[1]))
				{
					int[] possible = new int[item_type_id_chance.Length];
					int[] possible2 = new int[item_type_id_chance.Length];
					int[] same_chance = new int[item_type_id_chance.Length];
					int[] same_chance2 = new int[item_type_id_chance.Length];

					if(item_type_id_chance[0] != 0)
					{
						string[] temp = item_type_id.Clone() as string[];

						int roll = Util.RandomCardPackInt (1, 100);
//						Console.WriteLine ("Curr ROLL: " + roll);

						for(int j=0;j<item_type_id.Length-1;j++)
						{
							if(roll <= Convert.ToInt32(item_type_id_chance[j]) )
							{
//								Console.WriteLine ("Pos value: " + item_type_id [j] + " Pos Roll needed: " +item_type_id_chance [j]);
								possible.SetValue (Convert.ToInt32 (item_type_id [j]), j);
								possible2.SetValue (Convert.ToInt32 (item_type_id_chance [j]), j);
							}
						}

						possible =  possible.Where(x => x != 0).ToArray();
						possible2 =  possible2.Where(x => x != 0).ToArray();

//						Console.WriteLine ("Possible: ");
//						foreach(var l in possible)
//						{
//							Console.WriteLine (l);
//						}
//
//						Console.WriteLine ("Possible2: ");
//						foreach(var k in possible2)
//						{
//							Console.WriteLine (k);
//						}

						if(possible2.Length > 1)
						{
							if(possible2[0] != 0 && possible2 [1] != 0)
							{
								// check klo ada chance yg sama
								for(int k=0; k<possible2.Length;k++)
								{
									if(possible2[k] == possible2.Min() && possible2[k] != 0)
									{
//										Console.WriteLine ("Writing to same chance");
										same_chance.SetValue (possible [k], k);
										same_chance2.SetValue (possible2 [k], k);
									}
								}

								same_chance =  same_chance.Where(x => x != 0).ToArray();
								same_chance2 =  same_chance2.Where(x => x != 0).ToArray();

//								Console.WriteLine ("same_chance");
//								foreach (var k in same_chance)
//								{
//									Console.WriteLine (k);
//								}
//
//								Console.WriteLine ("same_chance2");
//								foreach (var k in same_chance2)
//								{
//									Console.WriteLine (k);
//								}

								if(same_chance2[1] != 0)
								{
//									Console.WriteLine ("Same chance value Exist! Proceed with roll 3");
//									Console.WriteLine("Same chance 3 length: " + (same_chance2.Length - 1));
									int roll3 = Util.RandomCardPackInt (0, same_chance2.Length-1);
//									Console.WriteLine("roll3: " + roll3 );
//									Console.WriteLine ("Rool3 result: " + roll3 + " Item ID: " + same_chance [roll3] + " Chance: " + same_chance2[roll3]);
									item_id_result.SetValue (Convert.ToString(same_chance [roll3]),count);
								}
								else
								{
//									Console.WriteLine ("ONE");
									int minValue = possible2.Min () ;
									int minIndex = possible2.ToList().IndexOf(minValue);
									item_id_result.SetValue (Convert.ToString(possible [minIndex]),count);
								}
							}

						}
						else
						{
							int maxValue = item_type_id_chance.Max () ;
							int maxIndex = item_type_id_chance.ToList().IndexOf(maxValue);
//							Console.WriteLine ("Roll too high! Card result: " + temp [maxIndex]);
							item_id_result.SetValue (item_type_id[maxIndex],count);
						}
					}
					else
					{
						// semua card punya chance yg sama untuk ada di dlm sebuah card pack					    
						int roll = Util.RandomCardPackInt (0, Convert.ToInt32(item_type_id.Length-1));
						Console.WriteLine ("Item Spawned ID: " + item_type_id [roll]);
						item_id_result.SetValue (item_type_id [roll],count);

					}
//					Console.WriteLine ("Next Card \n");
					count++;
				}

//				Console.WriteLine ("Item Type ID result");
				List<string> item = new List<string> ();

				for(int i = 0; i<item_id_result.Length ;i++)
				{
					Console.WriteLine ("Item ID: " + item_id_result[i]);
					using(Command("server_add_item_card_to_user"))
					{
						AddParameter ("id", item_id_result [i]);
						AddParameter ("userid", user_id);
						StringDictionary itm = ExecuteSpRow ();
						item.Add (itm["_qrid"] + "~1~" + itm["_tmplt_id"] + "~" + itm["_qty"]);
					}
				}

				item_generated = string.Join ("`", item);
			}

//			if(monster_generated != string.Empty && item_generated != string.Empty)
//			{
//				card_pack_result = monster_generated + "`" + item_generated;
//			}
//			else if(monster_generated == string.Empty )
//			{
//				card_pack_result = item_generated;
//			}
//			else if(item_generated == string.Empty)
//			{
//				card_pack_result == string.Empty;
//			}

			return monster_generated + "`" + item_generated;
		}

		/// <summary>
		/// Generate cards based on card packs using random number twice.
		/// </summary>
		/// <returns>The cards generated.</returns>
		/// <param name="pack_id">Pack identifier.</param>
		/// <param name="user_id">User identifier.</param>

		public string GenerateCards2(int pack_id, int user_id)
		{
			string monster_generated = string.Empty;
			string item_generated = string.Empty;

			StringDictionary sd = new StringDictionary ();

			using(Command("server_get_card_pack"))
			{
				AddParameter ("id", pack_id);
				sd = ExecuteSpRow ();
			}

			var card_qty = sd ["_card_quantity"].Split (',');
			var card_type = sd ["_contains"].Split('+');
			var card_category1 = card_type [0].Split (';');
			var card_category2 = card_type [1].Split (';');
			string[] monster_type_id = new string[card_category1.Length];
			int[] monster_type_id_chance = new int[card_category1.Length];
			string[] monster_version = new string[card_category1.Length];
			string[] item_type_id = new string[card_category2.Length];
			int[] item_type_id_chance = new int[card_category2.Length];

			string[] mons_id_result = new string[Convert.ToInt32(card_qty[0])];
			string[] item_id_result = new string[Convert.ToInt32(card_qty[1])];
			string[] mons_ver_result = new string[Convert.ToInt32(card_qty[0])];

			if(card_category1.Length > 1)
			{
				int count = 0;
				for(int i=0;i<card_category1.Length - 1;i++)
				{
					var sp = Convert.ToString(card_category1 [i]).Split (',');

					monster_type_id.SetValue (sp[0], i);
					monster_version.SetValue (Convert.ToString(sp[1]), i);

					if(monster_type_id[i].Contains("/"))
					{
						var sp2 = Convert.ToString (monster_type_id [i]).Split ('/');
						monster_type_id.SetValue (sp2 [0], i);
						monster_type_id_chance.SetValue (Convert.ToInt32(sp2 [1]), i);
					}
				}

				Console.Write ("Jumlah type mons card: ");
				Console.WriteLine (monster_type_id.Length-1);
				Console.WriteLine ("Jumlah monster card dlm pack: " + card_qty [0] + "\n");
				//misal isi pack ada 5 berarti 5x loop
				while(count < Convert.ToInt32(card_qty[0]))
				{
					//bool flag = false;
					int[] possible = new int[monster_type_id_chance.Length];
					int[] possible2 = new int[monster_type_id_chance.Length];
					//check chance nya di set atau ngga
					if(monster_type_id_chance[0] != 0)
					{
						string[] temp = monster_type_id.Clone() as string[];
						string[] temp2 = monster_version.Clone () as string[];

						//						foreach(var u in temp2)
						//						{
						//							Console.WriteLine ("TEMP2" + u);
						//						}

						for(int i=0; i<monster_type_id.Length-1 ;i++)
						{
							Console.WriteLine ("TEMP LENGTH: " + (temp.Length-1));
							int roll = Util.RandomCardPackInt (0, Convert.ToInt32(temp.Length-1));
							Console.WriteLine ("Array index roll: " + roll);

							if(temp.Length == 2)
							{
								Console.WriteLine ("Last! auto->Monster Spawned ID: " + temp[roll] + " Ver: " + temp2[roll]);
								mons_id_result.SetValue (temp [roll],count);
								mons_ver_result.SetValue (temp2 [roll], count);
							}
							else
							{
								int roll2 = Util.RandomCardPackInt (1, 100);
								Console.WriteLine ("Current Roll: " + roll2 + " Requirement: " + monster_type_id_chance[roll] );
								if(roll2 <= Convert.ToInt32(monster_type_id_chance[roll]))
								{
									Console.WriteLine ("Monster Spawned ID: " + temp[roll] + " Ver: " + temp2[roll]);
									mons_id_result.SetValue (temp [roll],count);
									mons_ver_result.SetValue (temp2 [roll], count);
									break;
								}
								else
								{
									string toRemove = Convert.ToString(roll);
									var foos = new List<string>(temp);
									foos.RemoveAt(roll);
									temp = foos.ToArray();

									var foos2 = new List<string>(temp2);
									foos2.RemoveAt(roll);
									temp2 = foos2.ToArray();
								}
							}								
						}


					}
					else
					{
						// semua card punya chance yg sama untuk ada di dlm sebuah card pack

						int roll = Util.RandomCardPackInt (0, Convert.ToInt32(monster_type_id.Length-1));
						Console.WriteLine ("Monster Spawned ID: " + monster_type_id[roll] + " Ver: " + monster_version [roll]);
						mons_id_result.SetValue (monster_type_id [roll],count);
						mons_ver_result.SetValue (monster_version [roll], count);
					}

					Console.WriteLine ("Next Card \n");
					//flag = false;
					count++;
				}

				Console.WriteLine ("isi MOnter type ID result");
				List<string> mons = new List<string> ();

				for(int i = 0; i<mons_id_result.Length ;i++)
				{
					//					Console.WriteLine("Monster Id: " + mons_id_result[i] + " Version: " + mons_ver_result[i]);
					using(Command("server_add_monster_card_to_user"))
					{
						AddParameter ("id", mons_id_result [i]);
						AddParameter ("version", mons_ver_result [i].Substring (0, 1));
						AddParameter ("subversion", mons_ver_result [i].Substring (1, 1));
						AddParameter ("userid", user_id);
						StringDictionary mon = ExecuteSpRow ();
						mons.Add (mon["_qrid"] + "~0~" + mon["_tmplt_id"] + "~" + mon["_version"] + mon["_subversion"]);
					}
				}
				//monster_qrid~0~monster_tmplt_id~version~subversion
				monster_generated = string.Join ("`", mons);
			}

			if (card_category2.Length > 1)
			{
				int count = 0;
				for (int i = 0; i < card_category2.Length; i++) {
					var sp = Convert.ToString (card_category2 [i]).Split (',');
					item_type_id.SetValue (sp [0], i);
					if (item_type_id [i].Contains ("/")) {
						var sp2 = Convert.ToString (item_type_id [i]).Split ('/');
						item_type_id.SetValue (sp2 [0], i);
						item_type_id_chance.SetValue (Convert.ToInt32(sp2[1]),i);
					}
				}

				Console.Write ("Jumlah type item card: ");
				Console.WriteLine (item_type_id.Length-1);
				Console.WriteLine ("Jumlah item card dlm pack: " + card_qty [1] + "\n");
				//misal isi pack ada 5 berarti 5x loop
				while(count < Convert.ToInt32(card_qty[1]))
				{
					//bool flag = false;
					int[] possible = new int[item_type_id_chance.Length];

					if (item_type_id_chance [0] != 0) {
						// loop dari yg chance kecil ke besar ** VERY NOT RELEVANT!!
						//						for (int i = item_type_id.Length - 1; i > -1; i--) {
						//							// kalo ada chance nya, klo ga ada??/
						//							int roll = Util.RandomInt (1, 100);
						//							Console.WriteLine ("Roll to get: " + item_type_id_chance [i] + " Curr roll: " + roll);
						//
						//							if (i == 0 && flag == false) {
						//								Console.WriteLine ("Item Spawned ID: " + item_type_id [i]);
						//								break;
						//							} else {
						//								if (roll <= Convert.ToInt32 (item_type_id_chance [i])) {
						//									flag = true;
						//									Console.WriteLine ("Item Spawned ID: " + item_type_id [i]);
						//									break;
						//								}
						//							}
						//						}

						string[] temp = item_type_id.Clone() as string[];
						for(int i=0; i<item_type_id.Length-1 ;i++)
						{
							Console.WriteLine ("TEMP LENGTH: " + (temp.Length-1));
							int roll = Util.RandomCardPackInt (0, Convert.ToInt32(temp.Length-1));
							Console.WriteLine ("Array index roll: " + roll);

							if(temp.Length == 2)
							{
								Console.WriteLine ("Last! auto->Item ID: " + temp[roll]);
								item_id_result.SetValue (temp [roll],count);
							}
							else
							{
								int roll2 = Util.RandomCardPackInt (1, 100);
								Console.WriteLine ("Current Roll: " + roll2 + " Requirement: " + item_type_id_chance[roll]);
								if(roll2 <= Convert.ToInt32(item_type_id_chance[roll]))
								{
									Console.WriteLine ("Item ID: " + temp[roll]);
									item_id_result.SetValue (temp [roll],count);
									break;
								}
								else
								{
									string toRemove = Convert.ToString(roll);
									//temp = temp.Where(val => val != toRemove).ToArray();
									var foos = new List<string>(temp);
									foos.RemoveAt(roll);
									temp = foos.ToArray();
								}
							}								
						}


					}
					else
					{
						// semua card punya chance yg sama untuk ada di dlm sebuah card pack
						int roll = Util.RandomCardPackInt (0, Convert.ToInt32(item_type_id.Length));
						item_id_result.SetValue (item_type_id [roll],count);
					}

					count++;
				}
				Console.WriteLine ("Item Type ID result");
				List <string> item = new List<string>();
				for(int i = 0; i<item_id_result.Length ;i++)
				{
					//					Console.WriteLine ("Item ID: " + item_id_result[i]);
					using(Command("server_add_item_card_to_user"))
					{
						AddParameter ("id", item_id_result [i]);
						AddParameter ("userid", user_id);
						StringDictionary itm = ExecuteSpRow ();
						item.Add (itm["_qrid"] + "~1~" + itm["_tmplt_id"] + "~" + itm["_qty"]);
					}
				}

				item_generated = string.Join ("`", item);
			}

			return monster_generated + "`" + item_generated;
		}

		public string BuyCardPacks(SessionData so, string pack_id)
		{
			string result = string.Empty;
			string message = string.Empty;
			StringDictionary sd = new StringDictionary ();

			using(Command("server_purchase_card_pack"))
			{
				AddParameter ("id", pack_id);
				AddParameter ("userid", so.UserId);
				sd = ExecuteSpRow ();
				so.UserCoin = Convert.ToInt32(sd["_mobile_coins"]);
			}

			if(sd["_result"] == "1")
			{
				message = "Purchase Successfull";
				result = GenerateCards (Convert.ToInt32(pack_id), so.UserId);
			}
			else
			{
				message = "Insufficent coins";
			}

			//BuyShopItemResult`result~message~coins`
			return (int)CommandResponseEnum.BuyShopItemResult + "`" + result + "`" + message + "~" + so.UserCoin + "`";
		}

		public string BuyBattleItemSlot(SessionData so)
		{
			// TODO
			// harga di pukul rata 100
			// RETURN: BuyBattleSlotResult`result~message~totalcoins~slotcount`
			string result = string.Empty;
			string totalcoin = string.Empty;
			string slotcount = string.Empty;
			string message = string.Empty;
			StringDictionary sd = new StringDictionary ();

			using(Command(@"server_purchase_battle_item_slot"))
			{
				AddParameter ("userid", so.UserId);
				sd = ExecuteSpRow ();
			}

			if(sd["_result"] == "1")
			{
				message = " Purchase successful";
				totalcoin = sd ["_mobile_coins"];
				slotcount = sd["_battle_item_slot"];
			}
			else
			{
				message = "Insufficient coin";
			}
			return (int) CommandResponseEnum.BuyBattleSlotResult + "`" +result + "~" + message + "~" + totalcoin + "~" + slotcount + "`";
		}

	}
}

