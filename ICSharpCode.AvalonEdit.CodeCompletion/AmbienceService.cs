using System;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.Refactoring;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public static class AmbienceService
    {
        private const string ambienceProperty = "SharpDevelop.UI.CurrentAmbience";
        private const string codeGenerationProperty = "SharpDevelop.UI.CodeGenerationOptions";
        private static readonly List<CodeGenerator> codeGenerators = new List<CodeGenerator>();

        static AmbienceService()
        {
            PropertyService.PropertyChanged += PropertyChanged;
        }

        public static Properties CodeGenerationProperties
        {
            get { return PropertyService.Get(codeGenerationProperty, new Properties()); }
        }

        public static bool GenerateDocumentComments
        {
            get { return CodeGenerationProperties.Get("GenerateDocumentComments", true); }
        }

        public static bool GenerateAdditionalComments
        {
            get { return CodeGenerationProperties.Get("GenerateAdditionalComments", true); }
        }

        public static bool UseFullyQualifiedNames
        {
            get { return CodeGenerationProperties.Get("UseFullyQualifiedNames", true); }
        }

        public static bool UseProjectAmbienceIfPossible
        {
            get { return PropertyService.Get("SharpDevelop.UI.UseProjectAmbience", true); }
            set { PropertyService.Set("SharpDevelop.UI.UseProjectAmbience", value); }
        }

        public static string DefaultAmbienceName
        {
            get { return PropertyService.Get(ambienceProperty, "C#"); }
            set { PropertyService.Set(ambienceProperty, value); }
        }

        private static void ApplyCodeGenerationProperties(CodeGenerator generator)
        {
            CodeGeneratorOptions options = generator.Options;
            System.CodeDom.Compiler.CodeGeneratorOptions cdo = new CodeDOMGeneratorUtility().CreateCodeGeneratorOptions;

            options.EmptyLinesBetweenMembers = cdo.BlankLinesBetweenMembers;
            options.BracesOnSameLine = CodeGenerationProperties.Get("StartBlockOnSameLine", true);
            ;
            options.IndentString = cdo.IndentString;
        }

        internal static void InitializeCodeGeneratorOptions(CodeGenerator generator)
        {
            codeGenerators.Add(generator);
            ApplyCodeGenerationProperties(generator);
        }

        /// <summary>
        /// Gets the current ambience.
        /// This method is thread-safe.
        /// </summary>
        /// <returns>Returns a new ambience object (ambience objects are never reused to ensure their thread-safety).
        /// Never returns null, in case of errors the <see cref="NetAmbience"/> is used.</returns>
        public static IAmbience GetCurrentAmbience()
        {
            return new NetAmbience();
        }

        private static void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.Key == ambienceProperty)
            {
                OnAmbienceChanged(EventArgs.Empty);
            }
            if (e.Key == codeGenerationProperty)
            {
                codeGenerators.ForEach(ApplyCodeGenerationProperties);
            }
        }

        private static void OnAmbienceChanged(EventArgs e)
        {
            if (AmbienceChanged != null)
            {
                AmbienceChanged(null, e);
            }
        }

        public static event EventHandler AmbienceChanged;
    }
}