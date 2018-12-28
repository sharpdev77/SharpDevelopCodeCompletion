using System;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description
{
    public static class ClassNameFormater
    {
         public static string FormatName(this IReturnType type)
         {
             if (type == null)
                 return string.Empty;

             if (type.Name == "Nullable")
             {
                 return string.Format("{0}?", type.CastToConstructedReturnType().TypeArguments[0].FormatName());
             }

             var formattedName = type.GetFormattedNameOrNull();
             if (formattedName != null)
                 return formattedName;
             
             var underlyingClass = type.GetUnderlyingClass();
             if(underlyingClass != null)
                return underlyingClass.FormatName();

             return type.Name;
         }
        
         private static readonly Dictionary<string, string> BaseTypesAliases = new Dictionary<string, string>
                                                                                    {
                                                                                        {"System.Int32", "int"},
                                                                                        {"System.Double", "double"},
                                                                                        {"System.Int16", "short"},
                                                                                        {"System.Int64", "long"},
                                                                                        {"System.Boolean", "bool"},
                                                                                        {"System.String", "string"},
                                                                                        {"System.Single", "float"},
                                                                                        {"System.Void", "void"},
                                                                                    }; 
 
         public static string FormatName(this IClass type)
         {
             string alias;
             return BaseTypesAliases.TryGetValue(type.DotNetName, out  alias) ? alias : type.Name;
         }
    }
}