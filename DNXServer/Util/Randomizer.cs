using System;

namespace DNXServer
{
	public class Randomizer
	{
		protected float seed;
	
	
		public Randomizer()
		{
			this.seed = 0;
		}
		
		public Randomizer(int seed)
		{
			Log.Info ("Seed: " + seed);
			//Console.WriteLine("Seed: " + seed);
			this.seed = seed;
		}	
	
		/**
		 *Returns a pseudo-random number n, where 0 <= n < 1
		 */
		public double GetRandom()
		{
			this.seed = (this.seed*9301+49297) % 233280;
			//double apagitu = this.seed/233280.0;
			//return (((double)(int)(apagitu * 1000)) / (double) 1000);

			return this.seed / 233280.0;
		}
		
		public double GetRandom3()
		{
			double rand = GetRandom();
			double dRand = (((double)(int)(rand * 1000)) / (double)1000);
			Log.Info ("Random: " + dRand + " " + this.seed);
			//Console.WriteLine("Random: " + dRand + " " + this.seed);
			return dRand;
		}
	
	
		/**
		 *Utility method for getting real numbers in the provided range
		 *The range is inclusive 
		 */
		public double GetNumInRange(double bottom, double top)
		{
			double dif = top - bottom + 1;
			double num = GetRandom();
			return bottom + (dif * num);
		}
	
	
		/**
		 *Utility method for getting integers numbers in the provided range
		 *The range is inclusive
		 */
		public int GetIntInRange(int bottom, int top)
		{
			int dif = top - bottom + 1;
			double num = GetRandom();
			return (int)Math.Floor(bottom + (dif * num));
		}
	
	
		public bool GetBoolean()
		{
			return GetRandom() < .5;
		}
	}
}

