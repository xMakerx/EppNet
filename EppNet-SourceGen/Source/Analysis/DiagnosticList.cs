/////////////////////////////////////////////
/// Filename: DiagnosticList.cs
/// Date: March 13, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace EppNet.SourceGen.Analysis
{

    /// <summary>
    /// Wrapper around a lazily created list of <see cref="AnalysisDiagnostic"/>
    /// </summary>
    public class DiagnosticList
    {
        public bool AnalysisMode { get; }

        public bool IsValueCreated { get => _diagnostics.IsValueCreated; }

        private readonly Lazy<List<AnalysisDiagnostic>> _diagnostics;

        public DiagnosticList(ExecutionContext context)
        {
            this.AnalysisMode = context.IsAnalysis;
            this._diagnostics = new(() => []);
        }

        public DiagnosticList(bool analysisMode)
        {
            this.AnalysisMode = analysisMode;
            this._diagnostics = new(() => []);
        }

        public bool Add(AnalysisDiagnostic diagnostic)
        {
            if (!AnalysisMode)
                return false;

            _diagnostics.Value.Add(diagnostic);
            return true;
        }

        public bool AddRange(IEnumerable<AnalysisDiagnostic> diagnostics)
        {
            if (!AnalysisMode)
                return false;

            _diagnostics.Value.AddRange(diagnostics);
            return true;
        }

        public List<AnalysisDiagnostic> AddAndGet(AnalysisDiagnostic diagnostic)
        {
            Add(diagnostic);
            return Get();
        }

        public List<AnalysisDiagnostic> Get() =>
            _diagnostics.IsValueCreated ?
            _diagnostics.Value :
            Globals.EmptyDiagnostics;
    }

}
