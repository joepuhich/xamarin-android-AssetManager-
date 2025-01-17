using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;
using Xamarin.Android.Tasks;
using Xamarin.Android.Tools;
using Microsoft.Android.Build.Tasks;

namespace Xamarin.Android.Build.Tests
{
	[TestFixture]
	[Category ("Node-5")]
	public class RemoveDirTests : BaseTest
	{
		List<BuildMessageEventArgs> messages;
		string tempDirectory;

		[SetUp]
		public void Setup ()
		{
			messages = new List<BuildMessageEventArgs> ();
			tempDirectory = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			Directory.CreateDirectory (tempDirectory);
		}

		string NewFile (string fileName = "")
		{
			if (string.IsNullOrEmpty (fileName)) {
				fileName = Path.GetRandomFileName ();
			}
			var path = Path.Combine (tempDirectory, fileName);
			if (IsWindows && path.Length >= Files.MaxPath) {
				File.WriteAllText (Files.ToLongPath (path), contents: "");
			} else {
				File.WriteAllText (path, contents: "");
			}
			return path;
		}

		RemoveDirFixed CreateTask () => new RemoveDirFixed {
			BuildEngine = new MockBuildEngine (TestContext.Out, messages: messages),
			Directories = new [] { new TaskItem (tempDirectory) },
		};

		[Test]
		public void NormalDelete ()
		{
			NewFile ();
			var task = CreateTask ();
			Assert.IsTrue (task.Execute (), "task.Execute() should have succeeded.");
			Assert.AreEqual (1, task.RemovedDirectories.Length, "Changes should have been made.");
			DirectoryAssert.DoesNotExist (tempDirectory);
		}

		[Test]
		public void NoExist ()
		{
			Directory.Delete (tempDirectory);
			var task = CreateTask ();
			Assert.IsTrue (task.Execute (), "task.Execute() should have succeeded.");
			Assert.AreEqual (0, task.RemovedDirectories.Length, "No changes should have been made.");
			DirectoryAssert.DoesNotExist (tempDirectory);
		}

		[Test]
		public void ReadonlyFile ()
		{
			var file = NewFile ();
			File.SetAttributes (file, FileAttributes.ReadOnly);
			var task = CreateTask ();
			Assert.IsTrue (task.Execute (), "task.Execute() should have succeeded.");
			Assert.AreEqual (1, task.RemovedDirectories.Length, "Changes should have been made.");
			DirectoryAssert.DoesNotExist (tempDirectory);
		}

		[Test]
		public void LongPath ()
		{
			var file = NewFile (fileName: "foo".PadRight (MaxFileName, 'N'));
			var task = CreateTask ();
			Assert.IsTrue (task.Execute (), "task.Execute() should have succeeded.");
			Assert.AreEqual (1, task.RemovedDirectories.Length, "Changes should have been made.");
			DirectoryAssert.DoesNotExist (tempDirectory);
		}
	}
}
