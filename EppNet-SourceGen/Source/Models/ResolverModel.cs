/////////////////////////////////////////////
/// Filename: ResolverModel.cs
/// Date: February 8, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using Microsoft.CodeAnalysis;

using System;

namespace EppNet.SourceGen.Models
{

    [Flags]
    public enum ResolverAnalysisError
    {
        None = 0,
        NoAttribute      = 1 << 0,
        LacksInheritance = 1 << 1,
        LacksSingleton   = 1 << 2,
        NotClass         = 1 << 3,
    }

    /// <summary>
    /// Stores information about a particular network type resolver
    /// </summary>
    public readonly struct ResolverModel(ISymbol symbol, ITypeSymbol typeSymbol) : IEquatable<ResolverModel>
    {
        public string Name { get; } = symbol.Name;

        /// <summary>
        /// Most resolvers will derive from <see cref="Globals.DataPath"/>
        /// </summary>
        public string Namespace { get; } = symbol.ContainingNamespace.Name;

        /// <summary>
        /// The fully qualified name of the type this resolver resolves.
        /// </summary>
        public string ResolvedTypeFullName { get; } = typeSymbol.ContainingNamespace.Name + "." + typeSymbol.Name;

        public override bool Equals(object obj) =>
            obj is ResolverModel model &&
            Equals(model);

        public bool Equals(ResolverModel other) =>
            Name == other.Name &&
            Namespace == other.Namespace &&
            ResolvedTypeFullName == other.ResolvedTypeFullName;

        public override string ToString() =>
            $"{Name}<{ResolvedTypeFullName}>";

        public override int GetHashCode() => Name.GetHashCode() ^ Namespace.GetHashCode() ^ ResolvedTypeFullName.GetHashCode();

        public static bool operator ==(ResolverModel left, ResolverModel right) => left.Equals(right);
        public static bool operator !=(ResolverModel left, ResolverModel right) => !left.Equals(right);
    }

}
