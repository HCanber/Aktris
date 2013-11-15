using System.Linq;
using Aktris.Internals;
using FluentAssertions;
using Xunit;

namespace Aktris.Test.Internals
{
// ReSharper disable InconsistentNaming
	public class UniqueNameCreator_Tests
	{
		[Fact]
		public void When_getting_many_names_they_are_different()
		{
			var randomNameCreator = new UniqueNameCreator();

			var names = Enumerable.Range(1,10).Select(_=>randomNameCreator.GetNextRandomName()).ToList();

			names.Should().OnlyHaveUniqueItems();
		}
	}
	// ReSharper restore InconsistentNaming
}