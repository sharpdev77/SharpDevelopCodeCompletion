using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description;
using ICSharpCode.NRefactory;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems
{
    public class CodeCompletionItem : IFancyCompletionItem
    {
        private readonly IProjectContent _projectContent;
        private readonly IEntity _entity;

        public CodeCompletionItem(IEntity entity, IProjectContent projectContent)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            _entity = entity;
            _projectContent = projectContent;

            IAmbience ambience = AmbienceService.GetCurrentAmbience();
            ambience.ConversionFlags = entity is IClass ? ConversionFlags.ShowTypeParameterList : ConversionFlags.None;
            Text = entity.Name;
            Content = ambience.Convert(entity);

            Overloads = 1;

            Priority = CodeCompletionDataUsageCache.GetPriority(entity.DotNetName, true);
        }

        public IEntity Entity
        {
            get { return _entity; }
        }

        public int Overloads { get; set; }

        #region IFancyCompletionItem Members

        public double Priority { get; set; }

        /// <summary>
        /// The text inserted into the code editor.
        /// </summary>
        public string Text { get; set; }

        public IImage Image { get; set; }

        /// <summary>
        /// The content displayed in the list.
        /// </summary>
        public object Content { get; set; }

        object IFancyCompletionItem.Description
        {
            get { return Description; }
        }

        #endregion

        protected void MarkAsUsed()
        {
            CodeCompletionDataUsageCache.IncrementUsage(_entity.DotNetName);
        }

        #region Complete

        public virtual void Complete(CompletionContext context)
        {
            MarkAsUsed();

            var insertedText = Text;
            var selectedClass = GetClassOrExtensionMethodClass(Entity);
            if (selectedClass != null)
            {
                // Class or Extension method is being inserted
                var editor = context.Editor;
                //  Resolve should return AmbiguousResolveResult or something like that when we resolve a name that exists in more imported namespaces
                //   - so that we would know that we always want to insert fully qualified name


                if (Entity is IClass && insertedText.EndsWith("Attribute") && IsInAttributeContext(editor, context.StartOffset))
                {
                    insertedText = insertedText.RemoveFromEnd("Attribute");
                }


                context.Editor.Document.Replace(context.StartOffset, context.Length, insertedText);
                context.EndOffset = context.StartOffset + insertedText.Length;
            }
            else
            {
                // Something else than a class or Extension method is being inserted - just insert text
                context.Editor.Document.Replace(context.StartOffset, context.Length, insertedText);
                context.EndOffset = context.StartOffset + insertedText.Length;
            }
        }

        private IClass GetClassOrExtensionMethodClass(IEntity selectedEntity)
        {
            var selectedClass = selectedEntity as IClass;
            if (selectedClass == null)
            {
                var method = selectedEntity as IMethod;
                if (method != null && method.IsExtensionMethod)
                    selectedClass = method.DeclaringType;
            }
            return selectedClass;
        }

        private bool IsKnownName(ResolveResult nameResult)
        {
            return (nameResult != null) && nameResult.IsValid;
        }

        /// <summary>
        /// Returns true if user is typing "Namespace.(*expr*)"
        /// </summary>
        private bool IsUserTypingFullyQualifiedName(CompletionContext context)
        {
            return (context.StartOffset > 0) && (context.Editor.Document.GetCharAt(context.StartOffset - 1) == '.');
        }

        private ResolveResult ResolveAtCurrentOffset(string className, CompletionContext context)
        {
            IDocument document = context.Editor.Document;
            Location position = document.OffsetToPosition(context.StartOffset);
            return ParserService.Resolve(new ExpressionResult(className), position.Line, position.Column,
                                         context.Editor.Document.Text, document.Text, _projectContent);
        }

        /// <summary>
        /// Returns true if the offset where we are inserting is in Attibute context, that is [*expr*
        /// </summary>
        private bool IsInAttributeContext(ITextEditor editor, int offset)
        {
            if (editor == null || editor.Document == null)
                return false;
            IExpressionFinder expressionFinder = ParserService.GetExpressionFinder(editor.Document.Text, _projectContent);
            if (expressionFinder == null)
                return false;
            ExpressionResult resolvedExpression = expressionFinder.FindFullExpression(editor.Document.Text, offset);
            return resolvedExpression.Context == ExpressionContext.Attribute;
        }

        #endregion

        #region Description

        private static readonly Dictionary<string, string> Test = new Dictionary<string, string>();
        private Description _description;
        private bool _descriptionCreated;

        public Description Description
        {
            get
            {
                lock (this)
                {
                    if (!_descriptionCreated)
                    {
                        _descriptionCreated = true;

                        _description = GetDescription(_entity);
                    }
                    return _description;
                }
            }
        }

        /// <summary>
        /// Converts the xml documentation string into a description object.
        /// </summary>
        public static Description GetDescription(IEntity entity)
        {
            IHeader header = null;

            var method = entity as IMethod;
            if (method != null)
            {
                header = new MethodHeader(method);
            }
            var field = entity as IField;
            if (field != null)
            {
                header = new FieldHeader(field);
            }

            var property = entity as IProperty;
            if (property != null)
            {
                header = new PropertyHeader(property);
            }

            if (header == null)
                header = new SimpleHeader(AmbienceService.GetCurrentAmbience().Convert(entity));


            var description = new Description(header);
            string xmlDocumentation = entity.Documentation;

            if (string.IsNullOrEmpty(xmlDocumentation))
                return description;

            try
            {
                // original pattern without escape symbols: \<see cref=\"[^\"]+\.([^\"]+)\"\ /\>
                const string seeCrefPattern = "\\<see cref=\\\"[^\\\"]+\\.([^\\\"]+)\\\"\\ /\\>";
                xmlDocumentation = Regex.Replace(xmlDocumentation, seeCrefPattern, "$1");

                XDocument xml = XDocument.Parse("<docroot>" + xmlDocumentation + "</docroot>");

                foreach (XElement element in xml.Root.Elements())
                {
                    Test[element.Name.LocalName] = element.ToString();
                }

                XElement summary = xml.Descendants("summary").FirstOrDefault();
                if (summary != null)
                    description.Summary = summary.Value.Trim();

                XElement[] xmlParameters = xml.Descendants("param").ToArray();
                foreach (XElement node in xmlParameters)
                {
                    string name = node.Attribute("name").Value;
                    string parameterDescription = node.Value;
                    Parameter parameterObject =
                        description.Parameters.FirstOrDefault(parameter => parameter.Name == name);
                    if (parameterObject != null)
                    {
                        parameterObject.Description = parameterDescription;
                    }
                }
            }
            catch (Exception)
            {
                return new SimpleDescription(xmlDocumentation);
            }
            return description;
        }

        #endregion
    }
}