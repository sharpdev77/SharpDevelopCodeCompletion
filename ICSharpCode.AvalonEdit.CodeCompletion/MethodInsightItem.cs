using System;
using ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems;
using ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// An insight item that represents an entity.
    /// </summary>
    public class MethodInsightItem : IInsightItem
    {
        private int _highlightParameter;

        public MethodInsightItem(IEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");


            Entity = entity;
            var description = CodeCompletionItem.GetDescription(Entity);
            description.IsInsisghtWindow = true;

            Header = description.Header;
            Content = description;

            _highlightParameter = -1;
        }

        public IEntity Entity { get; private set; }

        public int HighlightParameter
        {
            get { return _highlightParameter; }
            set
            {
                if (_highlightParameter != value)
                {
                    var methodHeader = Header as MethodHeader;
                    if(methodHeader == null) return;

                    _highlightParameter = value;
                    int i = 0;
                    foreach (Parameter parameter in methodHeader.Parameters)
                    {
                        parameter.IsHighlighted = _highlightParameter == i;
                        i++;
                    }
                }
            }
        }
        
        public object Header { get; private set; }

        public object Content { get; private set; }

    }
}