namespace Aktris
{
// ReSharper disable once InconsistentNaming
	public interface ActorSystemFactory
	{
		ActorSystem Create(string name);
	}
}