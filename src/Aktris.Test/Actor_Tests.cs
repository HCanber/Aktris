using System;
using Aktris.Exceptions;
using Xunit;

namespace Aktris.Test
{
	public class Actor_Tests
	{
		[Fact]
		public void Creating_an_actor_directly_should_fail()
		{
			Assert.Throws<InvalidOperationException>(() => new TestActor());
		}

		private class TestActor : Actor { }
	}
}