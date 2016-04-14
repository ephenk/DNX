using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using LitJson;
using System.Security.Principal;

namespace DNXServer
{
	public class WebsiteRequest
	{
		public WebsiteRequest ()
		{
		}	

		public string CheckFBID(string access_token)
		{
			string url = string.Format("https://graph.facebook.com/v2.2/me?access_token={0}&fields=id,name,picture", access_token);
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
				string fbid = (string) jsondata["id"];
				string fbname = (string) jsondata["name"];
				string fbpicture = (string) jsondata["picture"]["data"]["url"];
				string result = fbid + "~" + fbname + "~" + fbpicture;
				return result;
			}
			catch(Exception ex) 
			{
				StringBuilder str = new StringBuilder();
				str.Append ("FB Access Token Checking Error!!");
				str.Append ("\nMessage: "+ex.Message);
				str.AppendLine("\nSource: " + ex.Source);
				str.AppendLine("\nStack Trace:");
				str.AppendLine(ex.StackTrace);
				Log.Error (str.ToString());
				return "-1~-1~-1";
			}
		}

		public string WebRequestQRID(string qrid)
		{
			// Create a request using a URL that can receive a post. 
			WebRequest request = WebRequest.Create ("http://dnx.anantarupa.com/qr/webRequest.php");
			// Set the Method property of the request to POST.
			request.Method = "POST";
			// Create POST data and convert it to a byte array.
			string postData = "qrid=" + qrid;
			byte[] byteArray = Encoding.UTF8.GetBytes (postData);
			// Set the ContentType property of the WebRequest.
			request.ContentType = "application/x-www-form-urlencoded";
			// Set the ContentLength property of the WebRequest.
			request.ContentLength = byteArray.Length;
			// Get the request stream.
			Stream dataStream = request.GetRequestStream ();
			// Write the data to the request stream.
			dataStream.Write (byteArray, 0, byteArray.Length);
			// Close the Stream object.
			dataStream.Close();
			// Get the response.
			WebResponse response = request.GetResponse();
			// Display the status.
			Console.WriteLine (((HttpWebResponse)response).StatusDescription);
			// Get the stream containing content returned by the server.
			dataStream = response.GetResponseStream ();
			// Open the stream using a StreamReader for easy access.
			StreamReader reader = new StreamReader(dataStream);
			// Read the content.
			string responseFromServer = reader.ReadToEnd();
			// Display the content.
			Log.Info(responseFromServer);
			// Clean up the streams.
			reader.Close();
			dataStream.Close();
			response.Close();

			return responseFromServer;
		}
	}
}

