using System;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace web_client
{
	public struct Point{
		public int x;
		public int y;
	}
	[DataContract]
	public class RandomMover
	{
		const byte MAX_H = 99; //100 cols: 0 .. 99
		const byte MAX_V = 99; //100 rows: 0 .. 99

		[DataMember(Name="id")]
		public int id { get; set;}

		[DataMember(Name="point")]
		public Point pos { get; set;}
		private Random rand;

		public RandomMover(){
			rand = new Random ();
			pos = InitializePos ();
		}

		//move to another point
	   private void MoveOnePos(){
			Point tempPoint = pos;
			if (RandomizeTwo ()==0) { 		//moving vertically
				if (RandomizeTwo ()==0) { 	//moving up
					Console.Write ("Up \t");
					tempPoint.y = (pos.y + 1) % (MAX_V+1);
				} else { 				//moving down
					Console.Write ("Down\t");
					tempPoint.y = (pos.y==0)?MAX_V:(pos.y-1);
				}
			} else { 					//
				if (RandomizeTwo ()==0) { 	//moving left
					Console.Write ("Left\t");
					tempPoint.x = (pos.x == 0) ? MAX_H : (pos.x - 1);
				} else {				//moving right
					Console.Write ("Right\t");
					tempPoint.x = (pos.x + 1) % (MAX_H+1);
				}
			}
			pos = tempPoint;
		}

		private byte RandomizeTwo(){
			byte b = (byte)rand.Next (2);
			return b;
		}

		//obtain an initial random point
		private Point InitializePos(){
			Point rez;

			rez.x = rand.Next(MAX_H+1);
			rez.y = rand.Next (MAX_V+1);

			return rez;
		}

		public override string ToString(){
			return "id="+id+"; x="+pos.x+"; y="+pos.y;
		}

		private static bool IsInteger(string s)
		{
			ushort output;
			return ushort.TryParse(s,out output);
		}

		public static void Main (string[] args)
		{
			RandomMover rm = new RandomMover ();
			CommunicatorJSON cJson = new CommunicatorJSON ();

			string server = "localhost";
			int port = 8080;

			if (args.Length > 1) {
				if (IsInteger (args [1])) {//we have only the port specified
					port = System.Convert.ToUInt16 (args [1]);
				} else {
					server = args [1];
				}
				if (args.Length > 2) {
					if(!IsInteger(args[2])){
						Console.WriteLine ("Use me like this: %s [server] [port]",args[0]);
					}
				}
			}

			string httpLoc = "http://" + server + ":" + port;
			
			Console.WriteLine(cJson.SendRepresentation(ref rm,httpLoc));
			for (int i=0; i<50; i++) {
				System.Threading.Thread.Sleep (500);
				rm.MoveOnePos ();
				//Console.WriteLine (rm.ToString ());
				Console.WriteLine(cJson.SendRepresentation(ref rm,httpLoc));
			}
		}
	}


}
