using System;
using System.Threading;
using System.Threading.Tasks;
using Aktris.Internals;
using FluentAssertions;
using Xunit;

namespace Aktris.Test.Internals
{
	public class InterlockedSpin_Tests
	{
		[Fact]
		public void When_a_shared_variable_is_updated_on_another_thread_Then_the_update_method_is_rerun()
		{
			var sharedVariable = 0;
			var hasEnteredUpdateMethod = new ManualResetEvent(false);
			var okToContinue = new ManualResetEvent(false);
			var numberOfCallsToUpdateWhenSignaled = 0;

			//This is what we want to test:
			//  sharedVariable = 0
			//  Fork to two threads:
			//  THREAD 1                               THREAD 2
			//                                         Call InterlockedSpin.Swap
			//                                             It calls updateWhenSignaled(0)
			//                                                Signal thread 1 it can do it's work, and wait for it
			//  set sharedVariable = 42
			//  signal thread 2 it can continue
			//  and wait for it.
			//                                                 return 4711 from updateWhenSignaled
			//                                             Interlocked.CompareExchange will update sharedVariable to 4711 if it still is 0
			//                                               which it isn't so it will fail. It will then loop.
			//                                             Call updateWhenSignaled(42)
			//                                                 return 4711 from updateWhenSignaled
			//                                             Interlocked.CompareExchange will update sharedVariable to 4711 if it still is 42
			//                                               which it is, and we return.
			//  Test that sharedVariable=42												 
			//  Test that updateWhenSignaled was called twice

			Func<int, int> updateWhenSignaled = i =>
			{
				numberOfCallsToUpdateWhenSignaled++;
				hasEnteredUpdateMethod.Set();	//Signal THREAD 1 to update sharedVariable
				okToContinue.WaitOne(TimeSpan.FromSeconds(2));	//Wait for THREAD 1
				return 4711;
			};
			var task = Task.Run(() => InterlockedSpin.Swap(ref sharedVariable, updateWhenSignaled));
			hasEnteredUpdateMethod.WaitOne(TimeSpan.FromSeconds(2)); //Wait for THREAD 2 to enter updateWhenSignaled
			sharedVariable = 42;
			okToContinue.Set();	//Signal THREAD 1 it can continue in updateWhenSignaled
			task.Wait(TimeSpan.FromSeconds(2));	//Wait for THREAD 1

			sharedVariable.Should().Be(4711);
			numberOfCallsToUpdateWhenSignaled.Should().Be(2);
		}

		[Fact]
		public void When_a_shared_variable_is_updated_on_another_thread_Then_the_update_method_is_rerun_but_as_the_break_condition_is_fulfilled_it_do_not_update()
		{
			var sharedVariable = 0;
			var hasEnteredUpdateMethod = new ManualResetEvent(false);
			var okToContinue = new ManualResetEvent(false);
			var numberOfCallsToUpdateWhenSignaled = 0;

			//This is what we want to test:
			//  sharedVariable = 0
			//  Fork to two threads:
			//  THREAD 1                               THREAD 2
			//                                         Call InterlockedSpin.Swap
			//                                             It calls updateWhenSignaled(0)
			//                                                Signal thread 1 it can do it's work, and wait for it
			//  set sharedVariable = 42
			//  signal thread 2 it can continue
			//  and wait for it.
			//                                                 Since value==0 we do not want to break
			//                                                 return <false,4711,"update"> from updateWhenSignaled
			//                                             Interlocked.CompareExchange will update sharedVariable to 4711 if it still is 0
			//                                               which it isn't so it will fail. It will then loop.
			//                                             Call updateWhenSignaled(42)
			//                                                 Since value!=0 we want to break
			//                                                 return <true,4711,"break"> from updateWhenSignaled
			//                                             Since first item in tuple==true, we break and return item 3: "break"
			//  Test that sharedVariable=42												 
			//  Test that updateWhenSignaled was called twice
			//  Test that return from updateWhenSignaled is "break"

			Func<int, Tuple<bool,int,string>> updateWhenSignaled = i =>
			{
				numberOfCallsToUpdateWhenSignaled++;
				hasEnteredUpdateMethod.Set();	//Signal to start-thread that we have entered the update method (it will chang
				okToContinue.WaitOne(TimeSpan.FromSeconds(2));	//Wait to be signalled
				var shouldUpdate = i==0;
				return Tuple.Create(shouldUpdate,4711,shouldUpdate ? "update":"break");
			};
			string result="";
			var task = Task.Run(() => { result= InterlockedSpin.ConditionallySwap(ref sharedVariable, updateWhenSignaled); });
			hasEnteredUpdateMethod.WaitOne(TimeSpan.FromSeconds(2));
			sharedVariable = 42;
			okToContinue.Set();
			task.Wait(TimeSpan.FromSeconds(2));

			sharedVariable.Should().Be(42);
			numberOfCallsToUpdateWhenSignaled.Should().Be(2);
			result.Should().Be("break");
		}

	}


}