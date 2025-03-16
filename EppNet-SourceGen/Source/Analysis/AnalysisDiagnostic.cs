/////////////////////////////////////////////
/// Filename: AnalysisErrorBase.cs
/// Date: March 11, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

namespace EppNet.SourceGen.Analysis
{

    public class AnalysisDiagnostic
    {

        /// <summary>
        /// The descriptor to use for the message.
        /// </summary>
        public readonly DiagnosticDescriptor DiagnosticDescriptor;
        
        /// <summary>
        /// The context of what caused the error
        /// </summary>
        public readonly CSharpSyntaxNode Context;

        /// <summary>
        /// The message to send
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// Where this error is occurring
        /// </summary>
        public readonly Location[] Locations;

        public AnalysisDiagnostic(DiagnosticDescriptor descriptor, CSharpSyntaxNode context, string message, params Location[] locations)
        {
            this.DiagnosticDescriptor = descriptor;
            this.Context = context;
            this.Message = message;
            this.Locations = locations;
        }

        public AnalysisDiagnostic(DiagnosticDescriptor descriptor, CSharpSyntaxNode context, string message, IEnumerable<Location> locations)
        {
            this.DiagnosticDescriptor = descriptor;
            this.Context = context;
            this.Message = message;
            this.Locations = locations.ToArray();
        }

        /// <summary>
        /// Creates the diagnostic for the analyzer
        /// </summary>
        /// <returns></returns>
        public Diagnostic CreateDiagnostic()
        {
            Location mainLocation = Location.None;
            Location[] extLocations = Locations;
            string message = Message;

            if (Context != null)
            {
                if (Context is BaseTypeDeclarationSyntax btds)
                    message = $"{btds.Identifier.Text}: {Message}";

                mainLocation = Context.GetLocation();
            }

            if (mainLocation == Location.None && Locations.Length > 0)
                mainLocation = Locations[0];

            if (extLocations == null || extLocations.Length == 0)
                return Diagnostic.Create(DiagnosticDescriptor, mainLocation, message);

            return Diagnostic.Create(DiagnosticDescriptor, mainLocation, extLocations, message);
        }

    }

}
