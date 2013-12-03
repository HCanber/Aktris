using System;
using Aktris.Internals.Path;
using FluentAssertions;
using Xunit;

namespace Aktris.Test.Internals.Path
{
	public class ActorPath_Equal_Tests
	{
		[Fact]
		public void A_root_path_Is_equal_to_itself()
		{
			var root = new RootActorPath("root");
			root.Equals(root).Should().BeTrue();
		}

		[Fact]
		public void Two_root_paths_with_same_name_Are_equal()
		{
			var root1 = new RootActorPath("root");
			var root2 = new RootActorPath("root");
			root1.Equals(root2).Should().BeTrue();
		}

		[Fact]
		public void Two_root_paths_with_different_name_Are_not_equal()
		{
			var root1 = new RootActorPath("root1");
			var root2 = new RootActorPath("root2");
			root1.Equals(root2).Should().BeFalse();
		}

		[Fact]
		public void A_child_pathIs_equal_to_itself()
		{
			var root = new RootActorPath("/");
			var childActorPath = new ChildActorPath(root, "Path", 1);
			childActorPath.Equals(childActorPath).Should().BeTrue();
		}

		[Fact]
		public void Two_child_paths_with_different_name_Are_not_equal()
		{
			var root = new RootActorPath("/");
			var childActorPath1 = new ChildActorPath(root, "Path 1", 1);
			var childActorPath2 = new ChildActorPath(root, "Path 2", 1);
			childActorPath1.Equals(childActorPath2).Should().BeFalse();
		}

		[Fact]
		public void Two_child_paths_with_different_parents_Are_not_equal()
		{
			var root1 = new RootActorPath("/1");
			var root2 = new RootActorPath("/2");
			var childActorPath1 = new ChildActorPath(root1, "Path", 1);
			var childActorPath2 = new ChildActorPath(root2, "Path", 1);
			childActorPath1.Equals(childActorPath2).Should().BeFalse();
		}

		[Fact]
		public void Two_child_paths_with_same_name_but_different_ids_Are_equal()
		{
			var root = new RootActorPath("/");
			var childActorPath1 = new ChildActorPath(root, "Path", 1);
			var childActorPath2 = new ChildActorPath(root, "Path", 2);
			childActorPath1.Equals(childActorPath2).Should().BeTrue();
		} 
	}

	public class ActorPath_CompareTo_Tests
	{
		[Fact]
		public void A_root_path_Is_equal_to_itself()
		{
			var root = new RootActorPath("root");
			root.CompareTo(root).Should().Be(0);
		}

		[Fact]
		public void Two_root_paths_with_same_name_Are_equal()
		{
			var root1 = new RootActorPath("root");
			var root2 = new RootActorPath("root");
			root1.CompareTo(root2).Should().Be(0);
		}

		[Fact]
		public void Two_root_paths_with_different_name_Are_compared_correctly()
		{
			var root1 = new RootActorPath("A");
			var root2 = new RootActorPath("B");
			root1.CompareTo(root2).Should().BeLessOrEqualTo(-1);
			root2.CompareTo(root1).Should().BeLessOrEqualTo(1);
		}

		[Fact]
		public void A_child_pathIs_equal_to_itself()
		{
			var root = new RootActorPath("/");
			var childActorPath = new ChildActorPath(root, "Path", 1);
			childActorPath.CompareTo(childActorPath).Should().Be(0);
		}

		[Fact]
		public void Two_child_paths_with_different_name_Are_compared_correctly()
		{
			var root = new RootActorPath("/");
			var childActorPath1 = new ChildActorPath(root, "Path 1", 1);
			var childActorPath2 = new ChildActorPath(root, "Path 2", 1);
			childActorPath1.CompareTo(childActorPath2).Should().BeLessOrEqualTo(-1);
			childActorPath2.CompareTo(childActorPath1).Should().BeLessOrEqualTo(1);
		}

		[Fact]
		public void Two_child_paths_with_different_parents_Are_Compared_correctly()
		{
			var root1 = new RootActorPath("/1");
			var root2 = new RootActorPath("/2");
			var childActorPath1 = new ChildActorPath(root1, "Path", 1);
			var childActorPath2 = new ChildActorPath(root2, "Path", 1);
			childActorPath1.CompareTo(childActorPath2).Should().BeLessOrEqualTo(-1);
			childActorPath2.CompareTo(childActorPath1).Should().BeLessOrEqualTo(1);
		}

		[Fact]
		public void Two_child_paths_with_same_name_but_different_ids_Are_equal()
		{
			var root = new RootActorPath("/");
			var childActorPath1 = new ChildActorPath(root, "Path", 1);
			var childActorPath2 = new ChildActorPath(root, "Path", 2);
			childActorPath1.CompareTo(childActorPath2).Should().Be(0);
		}
	}

	public class ActorPath_HashCode_Tests
	{
	
		[Fact]
		public void Two_root_paths_with_same_name_Have_same_hashcode()
		{
			var root1 = new RootActorPath("root");
			var root2 = new RootActorPath("root");
			root1.GetHashCode().Should().Be(root2.GetHashCode());
		}

		[Fact]
		public void Two_root_paths_with_different_name_Have_different_hashcodes()
		{
			var root1 = new RootActorPath("root1");
			var root2 = new RootActorPath("root2");
			root1.GetHashCode().Should().NotBe(root2.GetHashCode());
		}


		[Fact]
		public void Two_child_paths_with_sane_name_Are_equal()
		{
			var root = new RootActorPath("/");
			var childActorPath1 = new ChildActorPath(root, "Path 1", 1);
			var childActorPath2 = new ChildActorPath(root, "Path 1", 1);
			childActorPath1.GetHashCode().Should().Be(childActorPath2.GetHashCode());
		}

		[Fact]
		public void Two_child_paths_with_different_name_Have_different_hashcodes()
		{
			var root = new RootActorPath("/");
			var childActorPath1 = new ChildActorPath(root, "Path 1", 1);
			var childActorPath2 = new ChildActorPath(root, "Path 2", 1);
			childActorPath1.GetHashCode().Should().NotBe(childActorPath2.GetHashCode());
		}

		[Fact]
		public void Two_child_paths_with_different_parents_Have_different_hashcodes()
		{
			var root1 = new RootActorPath("/1");
			var root2 = new RootActorPath("/2");
			var childActorPath1 = new ChildActorPath(root1, "Path", 1);
			var childActorPath2 = new ChildActorPath(root2, "Path", 1);
			childActorPath1.GetHashCode().Should().NotBe(childActorPath2.GetHashCode());
		}

		[Fact]
		public void Two_child_paths_with_equal_parents_Have_same_hashcodes()
		{
			var root1 = new RootActorPath("/1");
			var root2 = new RootActorPath("/1");
			var childActorPath1 = new ChildActorPath(root1, "Path", 1);
			var childActorPath2 = new ChildActorPath(root2, "Path", 1);
			childActorPath1.GetHashCode().Should().Be(childActorPath2.GetHashCode());
		}

		[Fact]
		public void Two_child_paths_with_same_name_but_different_ids_Have_same_hashcodes()
		{
			var root = new RootActorPath("/");
			var childActorPath1 = new ChildActorPath(root, "Path", 1);
			var childActorPath2 = new ChildActorPath(root, "Path", 2);
			childActorPath1.GetHashCode().Should().Be(childActorPath2.GetHashCode());
		} 
	}
}