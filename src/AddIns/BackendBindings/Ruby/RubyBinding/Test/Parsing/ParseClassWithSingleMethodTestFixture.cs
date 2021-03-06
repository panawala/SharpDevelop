﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.AddIn;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.RubyBinding;
using ICSharpCode.SharpDevelop.Dom;
using NUnit.Framework;

namespace RubyBinding.Tests.Parsing
{
	[TestFixture]
	public class ParseClassWithMethodTestFixture
	{
		ICompilationUnit compilationUnit;
		IClass c;
		IMethod method;
		FoldingSection methodFold;
		FoldingSection classFold;
		TextDocument document; 
		
		[TestFixtureSetUp]
		public void SetUpFixture()
		{
			string ruby = "class Test\r\n" +
							"\tdef foo\r\n" +
							"\tend\r\n" +
							"end";
			
			DefaultProjectContent projectContent = new DefaultProjectContent();
			RubyParser parser = new RubyParser();
			compilationUnit = parser.Parse(projectContent, @"C:\test.rb", ruby);			
			if (compilationUnit.Classes.Count > 0) {
				c = compilationUnit.Classes[0];
				if (c.Methods.Count > 0) {
					method = c.Methods[0];
				}
				
				// Get folds.
				TextArea textArea = new TextArea();
				document = new TextDocument();
				textArea.Document = document;
				textArea.Document.Text = ruby;
				
				ParserFoldingStrategy foldingStrategy = new ParserFoldingStrategy(textArea);
				
				ParseInformation parseInfo = new ParseInformation(compilationUnit);
				foldingStrategy.UpdateFoldings(parseInfo);
				List<FoldingSection> folds = new List<FoldingSection>(foldingStrategy.FoldingManager.AllFoldings);
			
				if (folds.Count > 0) {
					classFold = folds[0];
				}
				if (folds.Count > 1) {
					methodFold = folds[1];
				}
			}
		}
		
		[Test]
		public void OneClass()
		{
			Assert.AreEqual(1, compilationUnit.Classes.Count);
		}
		
		[Test]
		public void ClassName()
		{
			Assert.AreEqual("Test", c.Name);
		}
		
		[Test]
		public void ClassBodyRegion()
		{
			int startLine = 1;
			int startColumn = 11;
			int endLine = 4;
			int endColumn = 4;
			DomRegion region = new DomRegion(startLine, startColumn, endLine, endColumn);
			Assert.AreEqual(region.ToString(), c.BodyRegion.ToString());
		}
		
		/// <summary>
		/// The class declaration region needs to extend up to and
		/// including the colon.
		/// </summary>
		[Test]
		public void ClassRegion()
		{
			int startLine = 1;
			int startColumn = 1;
			int endLine = 4;
			int endColumn = 4;
			DomRegion region = new DomRegion(startLine, startColumn, endLine, endColumn);
			Assert.AreEqual(region.ToString(), c.Region.ToString());
		}		
		
		[Test]
		public void MethodName()
		{
			Assert.AreEqual("foo", method.Name);
		}
		
		[Test]
		public void MethodBodyRegion()
		{
			int startLine = 2;
			int startColumn = 11; // IronRuby parser includes the "()" part of the method parameters even if it does not exist.
			int endLine = 3;
			int endColumn = 5;
			DomRegion region = new DomRegion(startLine, startColumn, endLine, endColumn);
			Assert.AreEqual(region.ToString(), method.BodyRegion.ToString());
		}
		
		/// <summary>
		/// The method region does not include the body.
		/// </summary>
		[Test]
		public void MethodRegion()
		{
			int startLine = 2;
			int startColumn = 2;
			int endLine = 2;
			int endColumn = 11; // IronRuby parser includes the "()" part of the method parameters even if it does not exist.
			DomRegion region = new DomRegion(startLine, startColumn, endLine, endColumn);
			Assert.AreEqual(region.ToString(), method.Region.ToString());
		}
		
		[Test]
		public void MethodFoldMarkerInnerText()
		{
			string textInsideFold = document.GetText(methodFold.StartOffset, methodFold.Length);
			Assert.AreEqual("\r\n\tend", textInsideFold);
		}
		
		[Test]
		public void MethodIsNotConstructor()
		{
			Assert.IsFalse(method.IsConstructor);
		}
		
		[Test]
		public void MethodIsPublic()
		{
			ModifierEnum modifiers = ModifierEnum.Public;
			Assert.AreEqual(modifiers, method.Modifiers);
		}
		
		[Test]
		public void ClassFoldMarkerInnerText()
		{
			string textInsideFold = document.GetText(classFold.StartOffset, classFold.Length);
			Assert.AreEqual("\r\n\tdef foo\r\n\tend\r\nend", textInsideFold);
		}
	}
}
