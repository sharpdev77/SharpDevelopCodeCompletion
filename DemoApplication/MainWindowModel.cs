using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.SharpDevelop.Dom;

namespace DemoApplication
{
    public class MainWindowModel
    {
        private readonly ProjectContentRegistry _registry = new ProjectContentRegistry();

        public TextDocument Document { get; private set; }

        public IFilterStrategy FilterStrategy { get; private set; }

        public IProjectContent ProjectContent { get; private set; }

        public MainWindowModel()
        {
            Document = new TextDocument()
                           {
                               Text = @"

using System;

namespace Test
{
    public class b2{
        int a2 {get; private set;}
        public event Action Call;
    }

    class TestClass
    {
        int field;

        [Obsolete(""Go to msdn"")]
        public int Age{get;set;}

        readonly int field2;
        
        event Action Run;

        int a{get;set;}
        int a2 {get; private set;}
        b2 b = new b2();
        b2 antoherB = new b2();

        AppDomain domain1;
        AppDomain domain2;
    
        public TestClass()
        {
            
        }

        public void Do()
        {
            
        }
    }
}"
                           };

            FilterStrategy = new NonFilterStrategy();
            ProjectContent = GetProjectContent(typeof(decimal).Assembly.Location, _registry);
        }

        private static IProjectContent GetProjectContent(string assemblyFileName, ProjectContentRegistry registry)
        {
            try
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assemblyFileName);
                return registry.GetProjectContentForReference(fileNameWithoutExtension, assemblyFileName);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class NonFilterStrategy : IFilterStrategy
    {
        public IEnumerable<ICompletionItem> Filter(IEnumerable<ICompletionItem> completionItems)
        {
            return completionItems;
        }
    }
}