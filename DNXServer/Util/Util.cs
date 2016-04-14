using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Npgsql;
using System.Timers;
using System.Threading.Tasks;
using System.Dynamic;
using System.Runtime.InteropServices;
using Mono.Xml;

namespace DNXServer
{
	public static class Util 
	{
		private static Random rand = new System.Random ();
		private static byte[] b;
		public static string access_token { get; set;}
		public static string refresh_token { get; set;}
		public static DateTime next_time_to_refresh { get; set;}
		public static Timer timer = new Timer ();

		public static int Clamp(int val, int min, int max)
		{
			return Math.Max (Math.Min (val, max), min);
		}

		public static int MaxCap(int val, int curr, int max)
		{
			int total = curr + val;
			return total > max ? Math.Min (total, max) : total;
		}


		/// <summary>
		/// Returns a random number range from min to max.
		/// </summary>
		/// <param name="min">Minimum possible number returned.</param>
		/// <param name="max">Maximum possible number returned.</param>
		/// <returns>Returns a random number range from min to max.</returns>

		public static int RandomInt(int min, int max)
		{
			// RandomInt(0,2) will have possible result : 0, 1 or 2
			return rand.Next (min, max+1);
		}

		public static int RandomCardPackInt(int min, int max)
		{
			return rand.Next (min, max);
		}

		/// <summary>
		/// Returns a random number between 0.0 and 1.0 .
		/// </summary>
		/// <param name="min">Minimum possible number returned.</param>
		/// <param name="max">Maximum possible number returned.</param>
		/// <param name="cons">Maximum possible number returned.</param>
		/// <returns>Returns a random number between 0.0 and 1.0.</returns>

		public static double RandomDouble()
		{
			if (b == null)
			{
				b = new byte[1];
			}
			rand.NextBytes(b);
			return (double)b[0] / (double)byte.MaxValue;
		}

		public static double RandomDouble(double min, double max)
		{		
			double doub = rand.NextDouble ();
			return doub * (max - min) + min;
		}

//		public static void ActionTimer()
//		{
//			//System.Timers.Timer aTimer = new System.Timers.Timer();
//			Console.WriteLine ("starting timer");
//			timer.Elapsed+=new ElapsedEventHandler(OnTimedEvent);
//			timer.Interval=10000;
//			timer.Start();
//		}
//
//		// Specify what you want to happen when the Elapsed event is raised.
//		private static void OnTimedEvent(object source, ElapsedEventArgs e)
//		{
//			Console.WriteLine("TimerStopped");
//			timer.Stop();
//		}

//		public static void ActionTimer()
//		{
//			Timer t = new Timer(TimerCallback, null, 0, 3000);
//			Console.ReadLine ();
//		}
//
//		public static void TimerCallback(Object o) {
//			// Display the date/time when this method got called.
//			Console.WriteLine("In TimerCallback: " + DateTime.Now);
//			// Force a garbage collection to occur for this demo.
//			GC.Collect();
//		}
	}
}

