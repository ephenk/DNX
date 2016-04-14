using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DNXServer.Action
{
	public class ItemAction: BaseAction
	{
		public ItemAction()
		{

		}

		public string GetItems(SessionData so)
		{
			List<string> listResult = new List<string>();

//			using(Command(@"
//				SELECT i.item_card_id, i.item_card_qrid, i.item_tmplt_id, i.user_id, i.qty, i.is_printed, t.itm_name AS item_name, i.expired_date
//				FROM item_cards i
//				LEFT JOIN item_template t
//					ON t.itm_tmplt_id = i.item_tmplt_id
//				WHERE user_id = :userid
//				ORDER BY t.itm_name ASC, i.qty DESC
//			"))
			using(Command(@"server_get_item_cards"))
			{
				AddParameter("owner", so.UserId);
				foreach(StringDictionary item in ExecuteSpRead())
				{
					List<string> itemList = new List<string>();
					itemList.Add(item["_item_card_qrid"]);
					itemList.Add("1"); // item4
					itemList.Add(item["_item_tmplt_id"]);
					itemList.Add(item["_qty"]);
					itemList.Add(item["_expired_date"]);
					itemList.Add(item["_is_printed"]);

					string itemResult = String.Join("~", itemList);
					listResult.Add(itemResult);
				}
			}

			string finalResult = String.Join("`", listResult);
			return finalResult;
		}
	}
}

