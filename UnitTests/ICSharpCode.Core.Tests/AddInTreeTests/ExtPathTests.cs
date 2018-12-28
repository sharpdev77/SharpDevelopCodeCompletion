// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.IO;
using ICSharpCode.AvalonEdit.CodeCompletion;
using NUnit.Framework;

namespace ICSharpCode.Core.Tests.AddInTreeTests.Tests
{
	[TestFixture]
	public class FileUtilityTests
	{
		#region NormalizePath
		[Test]
		public void NormalizePath()
		{
			Assert.AreEqual(@"c:\temp\test.txt", FileUtility.NormalizePath(@"c:\temp\project\..\test.txt"));
			Assert.AreEqual(@"c:\temp\test.txt", FileUtility.NormalizePath(@"c:\temp\project\.\..\test.txt"));
			Assert.AreEqual(@"c:\temp\test.txt", FileUtility.NormalizePath(@"c:\temp\\test.txt")); // normalize double backslash
			Assert.AreEqual(@"c:\temp", FileUtility.NormalizePath(@"c:\temp\."));
			Assert.AreEqual(@"c:\temp", FileUtility.NormalizePath(@"c:\temp\subdir\.."));
		}
		
		[Test]
		public void NormalizePath_DriveRoot()
		{
			Assert.AreEqual(@"C:\", FileUtility.NormalizePath(@"C:\"));
			Assert.AreEqual(@"C:\", FileUtility.NormalizePath(@"C:/"));
			Assert.AreEqual(@"C:\", FileUtility.NormalizePath(@"C:"));
			Assert.AreEqual(@"C:\", FileUtility.NormalizePath(@"C:/."));
			Assert.AreEqual(@"C:\", FileUtility.NormalizePath(@"C:/.."));
			Assert.AreEqual(@"C:\", FileUtility.NormalizePath(@"C:/./"));
			Assert.AreEqual(@"C:\", FileUtility.NormalizePath(@"C:/..\"));
		}
		
		[Test]
		public void NormalizePath_UNC()
		{
			Assert.AreEqual(@"\\server\share", FileUtility.NormalizePath(@"\\server\share"));
			Assert.AreEqual(@"\\server\share", FileUtility.NormalizePath(@"\\server\share\"));
			Assert.AreEqual(@"\\server\share", FileUtility.NormalizePath(@"//server/share/"));
			Assert.AreEqual(@"\\server\share\otherdir", FileUtility.NormalizePath(@"//server/share/dir/..\otherdir"));
		}
		
		[Test]
		public void NormalizePath_Web()
		{
			Assert.AreEqual(@"http://danielgrunwald.de/path/", FileUtility.NormalizePath(@"http://danielgrunwald.de/path/"));
			Assert.AreEqual(@"browser://http://danielgrunwald.de/path/", FileUtility.NormalizePath(@"browser://http://danielgrunwald.de/wrongpath/../path/"));
		}
		#endregion
		
	}
}
