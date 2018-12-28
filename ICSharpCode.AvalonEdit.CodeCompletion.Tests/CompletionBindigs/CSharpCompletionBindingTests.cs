using System.Collections.Generic;
using ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems;
using ICSharpCode.SharpDevelop.Dom;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace ICSharpCode.AvalonEdit.CodeCompletion.Tests.CompletionBindigs
{
    [TestFixture]
    public class CSharpCompletionBindingTests
    {
        private DefaultProjectContent _projectContent;
        private CSharpCompletionBinding _cSharpCompletionBinding;
        private Mock<CtrlSpaceCompletionItemProvider> _completionItemProviderMock;
        private Mock<ICompletionItemProviderFactory> _completionItemProviderFactoryMock;

        [SetUp]
        public void TestInitialize()
        {
            _projectContent = new DefaultProjectContent();
            _completionItemProviderFactoryMock = new Mock<ICompletionItemProviderFactory>();
            _completionItemProviderMock = new Mock<CtrlSpaceCompletionItemProvider>(_projectContent);
            _completionItemProviderMock.Setup(
                provider => provider.GenerateCompletionList(It.IsAny<ITextEditor>(), It.IsAny<IProjectContent>()))
                .Returns(new DefaultCompletionItemList());

            _completionItemProviderFactoryMock.Setup(
                factory => factory.Create(It.IsAny<LanguageProperties>(), It.IsAny<IProjectContent>()))
                .Returns(_completionItemProviderMock.Object);
            
            _cSharpCompletionBinding = new CSharpCompletionBinding(_projectContent,
                                                                  _completionItemProviderFactoryMock.Object);
        }

        [Test]
        public void FiltersCompletionList()
        {
            var filterStrategyMock = new Mock<IFilterStrategy>();
            var completionItemMock = new Mock<ICompletionItem>();
            var filteredItems = new[] { completionItemMock.Object };
            filterStrategyMock.Setup(strategy => strategy.Filter(It.IsAny<IEnumerable<ICompletionItem>>()))
                .Returns(filteredItems);

            _cSharpCompletionBinding.FilterStrategy = filterStrategyMock.Object;

            var textEditorMock = new Mock<ITextEditor>();
            var textEditor = textEditorMock.Object;

            _cSharpCompletionBinding.CtrlSpace(textEditor);

            textEditorMock.Verify(editor => editor.ShowCompletionWindow(It.Is<ICompletionItemList>(list => list.Items.Single() == completionItemMock.Object)),
                Times.Once());
        }
        
        [Test]
        public void ShouldNotFilterIfFilterStrategyIsNull()
        {
            var notFilteredItems = new DefaultCompletionItemList();
            notFilteredItems.Items.Add(new DefaultCompletionItem("some item"));

            _completionItemProviderMock.Setup(
                provider => provider.GenerateCompletionList(It.IsAny<ITextEditor>(), It.IsAny<IProjectContent>()))
                .Returns(notFilteredItems);

            _cSharpCompletionBinding.FilterStrategy = null;
            var textEditorMock = new Mock<ITextEditor>();

            _cSharpCompletionBinding.CtrlSpace(textEditorMock.Object);

            textEditorMock.Verify(editor => editor.ShowCompletionWindow(It.Is<ICompletionItemList>(list => list.Items.Single().Text == "some item")));
        }
        
        [Test]
        public void ProvidesUsingCompletion()
        {
            var textEditorMock = CreateTextEditorMock(@"using", @"using".Length);

            var defaultCompletionItemList = new DefaultCompletionItemList();
            defaultCompletionItemList.Items.Add(new NamespaceCompletionItem(new NamespaceEntry("System")));
            _completionItemProviderMock.Setup(
                provider => provider.GenerateCompletionList(It.IsAny<ITextEditor>(), It.IsAny<IProjectContent>()))
                .Returns(defaultCompletionItemList);

            _completionItemProviderFactoryMock.Setup(
                factory => factory.Create(It.IsAny<LanguageProperties>(), ExpressionContext.Namespace, It.IsAny<IProjectContent>()))
                .Returns(_completionItemProviderMock.Object);

            _cSharpCompletionBinding.HandleKeyPress(textEditorMock.Object, ' ', _projectContent);
            textEditorMock.Verify(editor => editor.ShowCompletionWindow(It.Is<ICompletionItemList>(list => list.Items.Single().Text == "System")));
        }
        
        private static Mock<ITextEditor> CreateTextEditorMock(string codeToComplete, int codeToCompleteCarretOffset)
        {
            var documentMock = new Mock<IDocument>();
            documentMock.Setup(document => document.Text).Returns(codeToComplete);
            documentMock.Setup(document => document.TextLength).Returns(codeToComplete.Length);
            documentMock.Setup(document => document.GetText(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>(codeToComplete.Substring);
            documentMock.As<ITextBuffer>().Setup(document => document.Text).Returns(codeToComplete);
            var caretMock = new Mock<ITextEditorCaret>();
            caretMock.Setup(caret => caret.Offset).Returns(codeToCompleteCarretOffset);
            var textEditorMock = new Mock<ITextEditor>();
            textEditorMock.Setup(editor => editor.Document).Returns(documentMock.Object);
            textEditorMock.Setup(editor => editor.Caret).Returns(caretMock.Object);

            return textEditorMock;
        }
    }
}