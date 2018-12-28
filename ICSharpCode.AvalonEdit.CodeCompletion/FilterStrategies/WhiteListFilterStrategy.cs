using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion.FilterStrategies
{
    public class WhiteListFilterStrategy : IFilterStrategy
    {
        private readonly IEnumerable<Type> _typesWhiteList;

        public WhiteListFilterStrategy(IEnumerable<Type> typesWhiteList)
        {
            _typesWhiteList = typesWhiteList;
        }

        public IEnumerable<ICompletionItem> Filter(IEnumerable<ICompletionItem> completionItems)
        {
            var avalibleNamespaces = new HashSet<string>(_typesWhiteList.Select(type => type.Namespace).Distinct());
            var itemOfTypeCodeCompletionItemWithClassTests =
                completionItems.OfType<CodeCompletionItem>()
                .Where(item => item.Entity is IClass).ToArray();

            var namespaceCompletionItems = completionItems.OfType<NamespaceCompletionItem>().ToArray();
            var namespaces = namespaceCompletionItems
                .Where(item => avalibleNamespaces.Contains(item.Text)).ToArray();

            var filtredClasses =
                itemOfTypeCodeCompletionItemWithClassTests
                .Where(item => _typesWhiteList.Any(type => type.FullName == item.Entity.FullyQualifiedName));

            var otherItems = completionItems.Except(itemOfTypeCodeCompletionItemWithClassTests)
                .Except(namespaceCompletionItems);

            var orderedEnumerable = filtredClasses.Concat(otherItems).Concat(namespaces).OrderBy(item => item.Text);
            return orderedEnumerable;
        }
    }
}