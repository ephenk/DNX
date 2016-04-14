using System;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using LitJson;
using System.Xml;

namespace DNXServer
{
	public class GooglePlay
	{
		public GooglePlay ()
		{
		}

//		public static string getCode()
//		{
//			WebRequest request = WebRequest.Create ("https://accounts.google.com/o/oauth2/auth");
//			request.Method = "POST";
//
//			string s1 = "response_type=code";
//			string s2 = "&scope=https://www.googleapis.com/auth/androidpublisher";
//			string s3 = "&redirect_uri=http://localhost";
//			string s4 = "&access_type=offline";
//			string s5 = "&client_id=623059900601-q36dct4tcjt4k2dssl65kh069e2ec7v0.apps.googleusercontent.com";
//			string s6 = "&hl=en";
//			string s7 = "&from_login=1&as=648d636147049305";
//			string s8 = "&pli=1";
//			string s9 = "&authuser=0";
//
//			string postData = s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8 + s9;
//			byte[] byteArray = Encoding.UTF8.GetBytes (postData);
//			request.ContentType = "application/x-www-form-urlencoded";
//			request.ContentLength = byteArray.Length;
//			Stream dataStream = request.GetRequestStream ();
//			dataStream.Write (byteArray, 0, byteArray.Length);
//			dataStream.Close();
//
//			WebResponse response = request.GetResponse();
//			// Display the status.
//			Console.WriteLine (((HttpWebResponse)response).StatusDescription);
//			// Get the stream containing content returned by the server.
//			dataStream = response.GetResponseStream ();
//			// Open the stream using a StreamReader for easy access.
//			StreamReader reader = new StreamReader(dataStream);
//			// Read the content.
//			string responseFromServer = reader.ReadToEnd();
//			// Display the content.
//			Log.Info(responseFromServer);
//			// Clean up the streams.
//			reader.Close();
//			dataStream.Close();
//			response.Close();
//			return responseFromServer;
//		}

		/// <summary>
		/// Manually retreive the access token and refresh token using POST method to google website, then write them on file
		/// </summary>

		public static void oAuthGetAccessToken()
		{
			// use this function manually to retreive the access token and refresh token
			// use this URL to get the code
			// https://accounts.google.com/o/oauth2/auth?response_type=code&scope=https://www.googleapis.com/auth/androidpublisher&redirect_uri=http://localhost&access_type=offline&client_id=623059900601-q36dct4tcjt4k2dssl65kh069e2ec7v0.apps.googleusercontent.com&hl=en&from_login=1&as=-2edbbac18a7de052&pli=1&authuser=0
			// it will return something like this: code=4/teRE2fCxBoVr33D9yyOIpt-b4dVK.AphcwUnOGKYWdJfo-QBMszuO6_64jQI
			// this code is one time use only
			// then input the code to https://accounts.google.com/o/oauth2/token using POST method

			WebRequest request = WebRequest.Create ("https://accounts.google.com/o/oauth2/token");		
			request.Method = "POST";
			string str1 = "grant_type=authorization_code";
			string str2 = "&code=4/IntNojCrWNCV7eiRkaycVn7GYU09.opzhpLN8fT4edJfo-QBMszuI0HHrjQI";
			string str3 = "&client_id=623059900601-q36dct4tcjt4k2dssl65kh069e2ec7v0.apps.googleusercontent.com";
			string str4 = "&client_secret=al747oVNeCUiwJWktTyrdeJ8";
			string str5 = "&redirect_uri=http://localhost";

			string postData = str1 + str2 + str3 + str4 + str5;
			byte[] byteArray = Encoding.UTF8.GetBytes (postData);
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = byteArray.Length;
			Stream dataStream = request.GetRequestStream ();
			dataStream.Write (byteArray, 0, byteArray.Length);
			dataStream.Close();
			string responseFromServer = string.Empty;

			try{
				WebResponse response = request.GetResponse();
				// Display the status.
				Console.WriteLine (((HttpWebResponse)response).StatusDescription);
				// Get the stream containing content returned by the server.
				dataStream = response.GetResponseStream ();
				// Open the stream using a StreamReader for easy access.
				StreamReader reader = new StreamReader(dataStream);
				// Read the content.
				responseFromServer = reader.ReadToEnd();
				// Display the content.
				Log.Info(responseFromServer);
				// Clean up the streams.
				reader.Close();
				dataStream.Close();
				response.Close();

				JsonData jsondata = JsonMapper.ToObject(responseFromServer);
				string access_token = (string) jsondata["access_token"];
				string token_type = (string) jsondata["token_type"];
				int expires_in = (int) jsondata["expires_in"];
				string refresh_token  = (string) jsondata["refresh_token"];

				DateTime dt =DateTime.Now.AddSeconds(expires_in);

				if (File.Exists ("Google")) {
					string[] newLines = {"access_token;"+access_token,"next_time_to_refresh;"+dt.ToString(),"refresh_token;"+refresh_token};
					File.WriteAllLines("Google",newLines);
				}
			}
			catch(Exception e) {
				Console.WriteLine (e.Message);
			}
		}

		/// <summary>
		/// Check the lifetime of current access token, refresh the token if obsolete.
		/// </summary>

		public static void oAuthRefreshToken()
		{
			string[] lines = File.ReadAllLines ("Google");
			var accessToken = lines [0].Split (';');
			var time = lines [1].Split (';');
			DateTime next_time_to_refresh = Convert.ToDateTime(time[1]);
			var refreshToken = lines [2].Split (';');

			DateTime current_time = DateTime.Now;

			if(DateTime.Compare(next_time_to_refresh,current_time)<0)
			{
				WebRequest request = WebRequest.Create ("https://accounts.google.com/o/oauth2/token");		
				request.Method = "POST";
				string str1 = "grant_type=refresh_token";
				string str2 = "&client_id=623059900601-q36dct4tcjt4k2dssl65kh069e2ec7v0.apps.googleusercontent.com";
				string str3 = "&client_secret=al747oVNeCUiwJWktTyrdeJ8";
				string str4 = "&refresh_token=" + refreshToken[1];

				string postData = str1 + str2 + str3 + str4;
				byte[] byteArray = Encoding.UTF8.GetBytes (postData);
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = byteArray.Length;
				Stream dataStream = request.GetRequestStream ();
				dataStream.Write (byteArray, 0, byteArray.Length);
				dataStream.Close();
				string responseFromServer = string.Empty;

				try{
					WebResponse response = request.GetResponse();
					Console.WriteLine (((HttpWebResponse)response).StatusDescription);
					dataStream = response.GetResponseStream ();
					StreamReader reader = new StreamReader(dataStream);
					responseFromServer = reader.ReadToEnd();
					Log.Info(responseFromServer);
					reader.Close();
					dataStream.Close();
					response.Close();

					JsonData jsondata = JsonMapper.ToObject(responseFromServer);
					string access_token = (string) jsondata["access_token"];
					string token_type = (string) jsondata["token_type"];
					int expires_in = (int) jsondata["expires_in"];

					DateTime dt =DateTime.Now.AddSeconds(expires_in);

					//write into file again
					if (File.Exists ("Google")) {
						string[] newLines = {"access_token;"+access_token,"next_time_to_refresh;"+dt.ToString(),"refresh_token;"+refreshToken[1]};
						File.WriteAllLines("Google",newLines);
					}

				}
				catch(Exception e) {
					Console.WriteLine (e.Message);
				}
			}
		}

		/// <summary>
		/// Check the validity of a client puchase
		/// </summary>
		/// <returns>Returns consumption state from google </returns>
		/// <param name="product_id">Product id from purchase.</param>
		/// <param name="token">Token from purchase</param>

		public string GooglePurchase(string product_id, string token)
		{
			// GET https://www.googleapis.com/androidpublisher/v1.1/applications/{packageName}/inapp/{productId}/purchases/{token}

			string[] lines = File.ReadAllLines ("Google");
			var accessToken = lines [0].Split (';');
			string access_token = accessToken [1];

			string url = string.Format("https://www.googleapis.com/androidpublisher/v1.1/applications/com.anantarupa.dnx/inapp/{0}/purchases/{1}?access_token={2}", product_id, token, access_token);
			//string url = string.Format("https://graph.facebook.com/me?access_token={0}&fields=id,name", access_token);
			//ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
			WebClient wc = new WebClient();
			try
			{
				Stream data = wc.OpenRead(url);
				StreamReader reader = new StreamReader(data);
				string json = reader.ReadToEnd();
				data.Close();
				reader.Close();
				JsonData jsondata = JsonMapper.ToObject(json);

				// kind	| string | This kind represents an inappPurchase object in the androidpublisher service.
				string kind = (string) jsondata["kind"];

				// purchaseTime	| long | The time the product was purchased, in milliseconds since the epoch (Jan 1, 1970).
				string purchaseTime = (string) jsondata["purchaseTime"];

				// purchaseState | integer | The purchase state of the order. Possible values are:
				//								0. Purchased
				//								1. Cancelled
				int purchaseState = (int) jsondata["purchaseState"];

				// consumptionState | integer | The consumption state of the inapp product. Possible values are:
				//								0. Yet to be consumed
				//								1. Consumed
				int consumptionState = (int) jsondata["consumptionState"];

				// developerPayload | string | A developer-specified string that contains supplemental information about an order.
				string developerPayload = (string) jsondata["developerPayload"];

				string result = Convert.ToString(consumptionState) ;
				return result;
			}
			catch(Exception ex) 
			{
				StringBuilder str = new StringBuilder();
				str.Append ("Google Purchase Error!!");
				str.Append ("\nMessage: "+ex.Message);
				str.AppendLine("\nSource: " + ex.Source);
				str.AppendLine("\nStack Trace:");
				str.AppendLine(ex.StackTrace);
				Log.Error (str.ToString());
				return "-1";
			}
		}

//		public bool VerifyPurchase(string message, string base64Signature, string publicKey)
//		{
//			// By default the result is false
//			bool result = false;
//			try
//			{
//				// Create the provider and load the KEY
//				RSACryptoServiceProvider provider = PEMKeyLoader.CryptoServiceProviderFromPublicKeyInfo(publicKey);
//				provider.FromXmlString(provider.ToXmlString(false));
//
//				// The signature is supposed to be encoded in base64 and the SHA1 checksum
//				// Of the message is computed against the UTF-8 representation of the 
//				// message
//				byte[] signature = Convert.FromBase64String(base64Signature);
//				SHA1Managed sha = new SHA1Managed();
//				byte[] data = Encoding.UTF8.GetBytes(message);
//				result = provider.VerifyData(data, sha, signature); 
//			
//			}
//			catch (Exception ex) 
//			{
//				StringBuilder str = new StringBuilder();
//				str.Append ("Google Purchase Error!!");
//				str.Append ("\nMessage: "+ex.Message);
//				str.AppendLine("\nSource: " + ex.Source);
//				str.AppendLine("\nStack Trace:");
//				str.AppendLine(ex.StackTrace);
//				Log.Error (str.ToString());
//			}
//			return result;
//		}


	}
}

