using Aktris.Exceptions;
using Aktris.Internals.Children;
using FluentAssertions;
using Xunit;

namespace Aktris.Test.Internals.Children
{
// ReSharper disable once InconsistentNaming
	public class ChildrenCollection_Tests
	{
		[Fact]
		public void Reserving_succeeds()
		{
			var sut = EmptyChildrenCollection.Instance;
			sut.ReserveName("Name").Should().NotBeNull();
		}

		[Fact]
		public void Given_a_collection_where_name_has_been_reserved_and_released_Then_it_is_the_EmptyChildrenCollection()
		{
			var sut = EmptyChildrenCollection.Instance;
			var coll = sut.ReserveName("Name");
			var result = coll.ReleaseName("Name");
			((object)result).Should().BeSameAs(EmptyChildrenCollection.Instance);
		}
		[Fact]
		public void Given_a_collection_where_name_has_been_reserved_Then_another_name_can_be_reserved()
		{
			var sut = EmptyChildrenCollection.Instance;
			var coll = sut.ReserveName("Name");
			var result = coll.ReserveName("Name2");
		}
		[Fact]
		public void Given_a_collection_where_name_has_been_reserved_Then_reserving_name_again_fails()
		{
			var sut = EmptyChildrenCollection.Instance;
			var coll = sut.ReserveName("Name");
			Assert.Throws<InvalidActorNameException>(() => coll.ReserveName("Name"));
		}

		[Fact]
		public void Given_a_collection_where_two_names_have_been_reserved_Then_the_first_name_can_be_released_and_reserved_again()
		{
			ChildrenCollection sut = EmptyChildrenCollection.Instance;
			sut = sut.ReserveName("Name");
			sut = sut.ReserveName("Name2");
			sut = sut.ReleaseName("Name");
			sut = sut.ReserveName("Name");

		}
	}
}