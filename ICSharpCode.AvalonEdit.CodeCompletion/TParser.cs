using System.Collections.Generic;
using System.IO;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Parser;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;
using TagComment = ICSharpCode.NRefactory.Parser.TagComment;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public class TParser
    {
        private static readonly string[] LexerTags = new string[0];


        private static void RetrieveRegions(ICompilationUnit cu, SpecialTracker tracker)
        {
            for (int i = 0; i < tracker.CurrentSpecials.Count; ++i)
            {
                var directive = tracker.CurrentSpecials[i] as PreprocessingDirective;
                if (directive != null)
                {
                    if (directive.Cmd == "#region")
                    {
                        int deep = 1;
                        for (int j = i + 1; j < tracker.CurrentSpecials.Count; ++j)
                        {
                            var nextDirective = tracker.CurrentSpecials[j] as PreprocessingDirective;
                            if (nextDirective != null)
                            {
                                switch (nextDirective.Cmd)
                                {
                                    case "#region":
                                        ++deep;
                                        break;
                                    case "#endregion":
                                        --deep;
                                        if (deep == 0)
                                        {
                                            cu.FoldingRegions.Add(new FoldingRegion(directive.Arg.Trim(),
                                                                                    DomRegion.FromLocation(
                                                                                        directive.StartPosition,
                                                                                        nextDirective.EndPosition)));
                                            goto end;
                                        }
                                        break;
                                }
                            }
                        }
                        end:
                        ;
                    }
                }
            }
        }

        public static ICompilationUnit Parse(IProjectContent projectContent, string text)
        {
            using (IParser p = ParserFactory.CreateParser(SupportedLanguage.CSharp, new StringReader(text)))
            {
                return Parse(p, string.Empty, projectContent);
            }
        }

        private static ICompilationUnit Parse(IParser p, string fileName, IProjectContent projectContent)
        {
            p.Lexer.SpecialCommentTags = LexerTags;
            p.ParseMethodBodies = false;
            p.Parse();

            var visitor = new NRefactoryASTConvertVisitor(projectContent,
                                                          SupportedLanguage.CSharp)
                              {
                                  Specials = p.Lexer.SpecialTracker.CurrentSpecials
                              };
            visitor.VisitCompilationUnit(p.CompilationUnit, null);
            visitor.Cu.FileName = fileName;
            visitor.Cu.ErrorsDuringCompile = p.Errors.Count > 0;
            RetrieveRegions(visitor.Cu, p.Lexer.SpecialTracker);
            AddCommentTags(visitor.Cu, p.Lexer.TagComments);
            return visitor.Cu;
        }

        private static void AddCommentTags(ICompilationUnit cu, IEnumerable<TagComment> tagComments)
        {
            foreach (TagComment tagComment in tagComments)
            {
                var tagRegion = new DomRegion(tagComment.StartPosition.Y, tagComment.StartPosition.X);
                var tag = new SharpDevelop.Dom.TagComment(tagComment.Tag, tagRegion, tagComment.CommentText);
                cu.TagComments.Add(tag);
            }
        }
    }
}