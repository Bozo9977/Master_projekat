using System;

namespace SCADASim
{
	class Program
	{
		static void Main(string[] args)
		{
			Simulator sim = new Simulator();
			sim.Start();

			Console.WriteLine("net.tcp://localhost:502/");
			Console.WriteLine("[Press Enter to stop]");
			Console.ReadLine();

			sim.Stop();
		}
	}
}
