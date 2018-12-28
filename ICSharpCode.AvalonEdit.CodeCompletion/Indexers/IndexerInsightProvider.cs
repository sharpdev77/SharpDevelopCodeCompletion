using System.Linq;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion.Indexers
{
    /// <summary>
    /// Produces MethodInsightItem instances for showing the insight window on indexer calls.
    /// </summary>
    public class IndexerInsightProvider : MethodInsightProvider
    {
        public IndexerInsightProvider(IProjectContent projectContent) : base(projectContent)
        {
        }

        public override IInsightItem[] ProvideInsight(ExpressionResult expressionResult, ResolveResult result)
        {
            if (result == null)
                return null;
            var type = result.ResolvedType;
            if (type == null)
                return null;

            var indexers = type.GetProperties()
                           .Where(property => property.IsIndexer)
                           .ToArray();

            if (indexers.Any(property => property.Parameters.Any(parameter => parameter.ReturnType == null)))
            {
                return indexers
                    .Select(indexer => new MethodInsightItem(indexer))
                    .Cast<IInsightItem>()
                    .ToArray();
            }

            var groupedIndexers = indexers.GroupBy(property => property.Parameters.GetKey());
            var insightItems = groupedIndexers
                .Select(
                    grouping =>
                    grouping.OrderByDescending(property => property.DeclaringType, new ClassComparer()).First())
                .Select(property => new MethodInsightItem(property))
                .Cast<IInsightItem>()
                .ToArray();

            return insightItems;
        }
    }
} ;