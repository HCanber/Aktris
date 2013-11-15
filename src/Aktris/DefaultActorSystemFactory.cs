using Aktris.Internals;

namespace Aktris
{
	public class DefaultActorSystemFactory : ActorSystemFactory
	{
		private static readonly DefaultActorSystemFactory _Instance = new DefaultActorSystemFactory();

		/// <summary>This constructor should be hidden from the outside</summary>
		private DefaultActorSystemFactory()
		{
			UniqueNameCreator=new UniqueNameCreator();
		}

		public static DefaultActorSystemFactory Instance { get { return _Instance; } }


		public ActorSystem Create(string name)
		{
			return new InternalActorSystem(name,UniqueNameCreator);
		}

		public IUniqueNameCreator UniqueNameCreator { get; set; }
	}
}