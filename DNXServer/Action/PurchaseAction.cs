using System;
using DNXServer.Action;
using System.Collections.Specialized;

namespace DNXServer
{
	public class PurchaseAction : BaseAction
	{
		public PurchaseAction ()
		{
		}

		public string GooglePurchase(SessionData so, string productId, string token)
		{
			// RECEIVE: VerifyPurchase`productID~token`
			// cek purchase id dari table purchase history, klo ga ada lanjut, klo udh ada return -1

			// try to refresh token if obsolete
			GooglePlay.oAuthRefreshToken ();

			StringDictionary result;
			string response = "-1";
			int userCoin = 0;

			using (Command("server_get_purchase_history")) {
				AddParameter ("token", token);
				result = ExecuteSpRow ();
			}

			if(result == null)
			{
				string googleResponse = new GooglePlay ().GooglePurchase (productId, token);

//				var split = googleResponse.Split ('~');
//				string kind = split [0];
//				long purchaseTime = Convert.ToInt64 (split [1]);
//				int purchaseState = Convert.ToInt32 (split [2]);
//				int consumptionState = Convert.ToInt32 (split [3]);
//				string developerPayload = split [4];

				if(googleResponse != "-1")
				{
					using (Command(@"server_add_purchase_history")) {
						AddParameter ("userid", so.UserId);
						AddParameter ("product", productId);
						AddParameter ("tok", token);
						result = ExecuteSpRow ();
					}

					var topup = productId.Split ('_');
					int qty = Convert.ToInt32 (topup [1]);

					result.Clear ();

					using (Command(@"server_add_user_coin")) {
						AddParameter ("userid", so.UserId);
						AddParameter ("qty", qty);
						userCoin = Convert.ToInt32(ExecuteSpRow ()["_mobile_coins"]);
						so.UserCoin = userCoin;
					}

				}

				response = googleResponse;

				// klo udh update table user, tambahin coinnya & update table purchase history
				//RETURN: PurchaseResult`consumptionState~userCoin`
				// consumptionState may be -1, 0, or 1
				// 0 and 1 are reply from google developer API
				// -1 means server could not connect to google or encounter any error which has not been specified

			}

			return (int)CommandResponseEnum.PurchaseResult + "`" + response + "~" + so.UserCoin + "`";
		}
	}
}

