using System;
using Aktris.Internals.Concurrency;
using FluentAssertions;
using Xunit;
using Aktris.Test.TestHelpers;

namespace Aktris.Test.Internals.Concurrency
{
// ReSharper disable once InconsistentNaming
	public class Promise_Tests
	{
		[Fact]
		public void Given_a_new_Promise_Then_it_can_be_completed_and_result_is_correct()
		{
			var promise = new Promise<int>();
			var result = 0;
			Exception caughtException = null;
			var setResultTask = promise.Future.Then(t => result = t.Result, exception => caughtException = exception);

			promise.Success(42);

			promise.IsCompleted.Should().BeTrue();
			setResultTask.Wait();
			result.Should().Be(42);
			caughtException.Should().BeNull();
		}

		[Fact]
		public void Given_a_new_Promise_Then_it_can_be_faulted_and_result_is_correct()
		{
			var promise = new Promise<int>();
			var result = 0;
			Exception caughtException = null;
			var setResultTask = promise.Future.Then(t => result = t.Result, ex => caughtException = ex);

			var exception = new Exception();
			promise.Failure(exception);

			promise.IsCompleted.Should().BeTrue();
			try
			{
				setResultTask.Wait();
			}
			catch(AggregateException e)
			{
				e.Flatten().InnerExceptions[0].Should().BeSameAs(exception);
			}
			result.Should().Be(0);
			caughtException.Should().BeSameAs(exception);
		}

		[Fact]
		public void Given_a_completed_Promise_Then_it_cannot_be_completed()
		{
			var promise = new Promise<int>();
			promise.Success(42);
			Assert.Throws<InvalidOperationException>(() => promise.Success(4711));
		}

		[Fact]
		public void Given_a_failed_Promise_Then_it_cannot_be_completed()
		{
			var promise = new Promise<int>();
			promise.Failure(new Exception());
			Assert.Throws<InvalidOperationException>(() => promise.Success(4711));
		}


		[Fact]
		public void Given_a_completed_Promise_Then_it_cannot_be_failed()
		{
			var promise = new Promise<int>();
			promise.Success(42);
			Assert.Throws<InvalidOperationException>(() => promise.Failure(new Exception()));
		}

		[Fact]
		public void Given_a_failed_Promise_Then_it_cannot_be_failed()
		{
			var promise = new Promise<int>();
			promise.Failure(new Exception());
			Assert.Throws<InvalidOperationException>(() => promise.Failure(new Exception()));
		}

		[Fact]
		public void Given_a_disposed_Promise_Then_it_cannot_be_failed()
		{
			var promise = new Promise<int>();
			promise.Dispose();
			Assert.Throws<InvalidOperationException>(() => promise.Failure(new Exception()));
		}

		[Fact]
		public void Given_a_disposed_Promise_Then_it_cannot_be_completed()
		{
			var promise = new Promise<int>();
			promise.Dispose();
			Assert.Throws<InvalidOperationException>(() => promise.Success(42));
		}
	}
}