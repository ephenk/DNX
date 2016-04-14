using System;
using System.IO;
using System.Collections.Generic;

namespace DNXServer
{
	public class DNXMessage
	{
		protected string originalString;
		protected DNXMessage[] children;
		protected string[] stringObjects;

		protected int level = 0;

		public DNXMessage(string stringData): this(stringData, '`')
		{

		}

		public DNXMessage(string stringData, char delimiter)
		{
			this.originalString = stringData;
			this.Deserialize(stringData, delimiter);

			if(this.stringObjects.Length <= 1)
			{
				return;
			}

			int objLength = this.stringObjects.Length;
			this.children = new DNXMessage[objLength];
			for(int i = 0; i < this.stringObjects.Length; i++)
			{
				int v = this.level + 1;

				char deli;
				if(v == 1)
				{
					deli = '~';
				}
				else
				{
					deli = '!';
				}

				this.children[i] = new DNXMessage(this.stringObjects[i], deli);
			}
		}

		public object[] Deserialize(string stringData, char delimiter)
		{
			this.stringObjects = stringData.Split(delimiter);
			return this.stringObjects;
		}

		public string GetValue(int idx)
		{
			return this.stringObjects[idx];
		}

		public override string ToString()
		{
			return string.Format("[DNXMessage: Item={0}]", this.originalString);
		}

		public DNXMessage this [int i]
		{
			get
			{
				return this.children[i];
			}

			set
			{
				this.children[i] = value;
			}
		}
	}
}

