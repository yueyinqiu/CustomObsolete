using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace CustomObsolete
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CustomObsoleteAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Warning = new DiagnosticDescriptor(
            "CObsW01",
            "Reference to something with custom obsolete attributes",
            "{0} is attributed as [{1}] with message: {2}",
            "CustomObsolete", 
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor Error = new DiagnosticDescriptor(
            "CObsE01",
            "Reference to something with custom obsolete attributes",
            "{0} is attributed as [{1}] with message: {2}",
            "CustomObsolete",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
            => ImmutableArray.Create(Warning, Error);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(AnalyzeOperation, OperationKind.MethodReference);
        }

        private static AttributeData IsObsolete(
            INamedTypeSymbol customObsoleteInterface,
            ISymbol symbol)
        {
            var comparer = SymbolEqualityComparer.Default;

            foreach (var attribute in symbol.GetAttributes())
            {
                foreach (var attributeInterface in attribute.AttributeClass.AllInterfaces)
                {
                    if (comparer.Equals(attributeInterface, customObsoleteInterface))
                        return attribute;
                }
            }

            return null;
        }

        private static bool CheckObsolete(AttributeData obsolete, ISymbol symbol)
        {
            var obsoleteSymbol = obsolete.AttributeClass;
            var comparer = SymbolEqualityComparer.Default;
            if (symbol.GetAttributes().Any(x => comparer.Equals(x.AttributeClass, obsoleteSymbol)))
                return true;
            if (symbol.ContainingSymbol is null)
                return false;
            return CheckObsolete(obsolete, symbol.ContainingSymbol);
        }

        private static (string name, string message, bool isError) ResolveObsolete(AttributeData obsolete)
        {
            string message = null;
            bool foundMessage = false;
            bool isError = false;
            bool foundIsError = false;
            foreach (var argument in obsolete.NamedArguments)
            {
                if(argument.Key is "Message" && argument.Value.Value is string s)
                {
                    message = s;
                    if (foundIsError)
                        break;
                    foundMessage = true;
                }
                else if (argument.Key is "IsError" && argument.Value.Value is bool b)
                {
                    isError = b;
                    if (foundMessage)
                        break;
                    foundIsError = true;
                }
            }

            var attributeName = obsolete.AttributeClass.Name;
            if (attributeName.EndsWith("Attribute"))
            {
                attributeName = attributeName.Substring(
                    0, attributeName.Length - "Attribute".Length);
            }
            return (attributeName, message, isError);
        }

        private static void AnalyzeOperation(OperationAnalysisContext context)
        {
            var customObsolete =
                context.Compilation.GetTypeByMetadataName("CustomObsolete.ICustomObsoleteAttribute");

            var member = ((IMemberReferenceOperation)context.Operation).Member;
            var obsolete = IsObsolete(customObsolete, member);

            if (obsolete == null)
                return;

            if (CheckObsolete(obsolete, context.ContainingSymbol))
                return;

            var (attributeName, message, isError) = ResolveObsolete(obsolete);
            var descriptor = isError ? Error : Warning;

            context.ReportDiagnostic(Diagnostic.Create(
                descriptor,
                context.Operation.Syntax.GetLocation(),
                member.Name, attributeName, message));
        }
    }
}
