using System.Linq;
using System.Reflection;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.CSharp;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Stores the compilation units for files.
    /// </summary>
    public static class ParserService
    {
        #region GetParser / ExpressionFinder / Resolve / etc.

        private static readonly string[] DefaultTaskListTokens = {"HACK", "TODO", "UNDONE", "FIXME"};
        private static ProjectContentRegistry _projectContentRegistry;

        /// <summary>
        /// Creates a new IParser instance that can parse the specified file.
        /// This method is thread-safe.
        /// </summary>
        public static ICompilationUnit CreateParser(string text, IProjectContent projectContent)
        {
            return TParser.Parse(projectContent, text);
        }

        /// <summary>
        /// Creates an IExpressionFinder instance for the specified file.
        /// This method is thread-safe.
        /// </summary>
        public static IExpressionFinder GetExpressionFinder(string fileName, IProjectContent projectContent)
        {
            ICompilationUnit parser = CreateParser(fileName, projectContent);
            if (parser != null)
            {
                return new CSharpExpressionFinder(new ParseInformation(parser));
            }
            return null;
        }

        public static IResolver CreateResolver()
        {
            return new NRefactoryResolver(LanguageProperties.CSharp);
        }

        /// <summary>
        /// Resolves given ExpressionResult.
        /// </summary>
        public static ResolveResult Resolve(ExpressionResult expressionResult, int caretLineNumber, int caretColumn,
                                            string fileName, string fileContent, IProjectContent projectContent)
        {
            if (expressionResult.Region.IsEmpty)
            {
                expressionResult.Region = new DomRegion(caretLineNumber, caretColumn);
            }
            IResolver resolver = CreateResolver();
            if (resolver != null)
            {
                ParseInformation parseInfo = GetParseInformation(fileName, projectContent);
                return resolver.Resolve(expressionResult, parseInfo, fileContent);
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Gets parse information for the specified file.
        /// Blocks if the file wasn't parsed yet, but may return an old parsed version.
        /// This method is thread-safe. This method involves waiting for the main thread, so using it while
        /// holding a lock can lead to deadlocks. You might want to use <see cref="GetExistingParseInformation"/> instead.
        /// </summary>
        /// <returns>Returns the ParseInformation for the specified file, or null if the file cannot be parsed.
        /// The returned ParseInformation might be stale (re-parse is not forced).</returns>
//        public static ParseInformation GetParseInformation(string text, IProjectContent)
//        {
//            if (string.IsNullOrEmpty(text))
//                return null;
//            return new ParseInformation(CreateParser(text, TODO));
//        }

        public static ParseInformation GetParseInformation(string text, IProjectContent projectContent)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            return new ParseInformation(TParser.Parse(projectContent, text));
        }
    }
}