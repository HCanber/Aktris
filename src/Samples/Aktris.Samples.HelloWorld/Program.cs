using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aktris.Samples.HelloWorld
{
	class Program
	{
		static void Main(string[] args)
		{
			var system = ActorSystem.Create();
			system.Start();
			var actor = system.CreateActor(ActorCreationProperties.Create<HelloWorldActor>());

			actor.Send("World", null);

			Console.WriteLine("Press any key");
			Console.ReadKey();
		}
	}

	public class HelloWorldActor : Actor
	{
		public HelloWorldActor()
		{
			Receive<string>(msg => Console.WriteLine("Hello " + msg + "!"));
		}
	}
}
