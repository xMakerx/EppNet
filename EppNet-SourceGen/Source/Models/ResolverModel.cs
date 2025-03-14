/////////////////////////////////////////////
/// Filename: ResolverModel.cs
/// Date: February 8, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using Microsoft.CodeAnalysis;

using System;

namespace EppNet.SourceGen.Models
{

    /// <summary>
    /// Stores information about a particular network type resolver
    /// </summary>
    public readonly struct ResolverModel(ISymbol symbol, ITypeSymbol typeSymbol) : IEquatable<ResolverModel>
    {
        public string Name { get; } = symbol.Name;

        /// <summary>
        /// Most resolvers will derive from <see cref="Globals.DataPath"/>
        /// </summary>
        public string Namespace { get; } = symbol.ContainingNamespace?.Name ?? string.Empty;

        /// <summary>
        /// The fully qualified name of the type this resolver resolves.
        /// </summary>
        public string ResolvedTypeFullName { get; } = typeSymbol.ToDisplayString(Globals.DisplayFormat);

        public override bool Equals(object obj) =>
            obj is ResolverModel model &&
            Equals(model);

        public bool Equals(ResolverModel other) =>
            Name == other.Name &&
            Namespace == other.Namespace &&
            ResolvedTypeFullName == other.ResolvedTypeFullName;

        public override string ToString() =>
            $"{Name}<{ResolvedTypeFullName}>";

        public override int GetHashCode() =>
            Name.GetHashCode() ^ 
            Namespace.GetHashCode() ^
            ResolvedTypeFullName.GetHashCode();

        public static bool operator ==(ResolverModel left, ResolverModel right) =>
            left.Equals(right);

        public static bool operator !=(ResolverModel left, ResolverModel right) =>
            !left.Equals(right);
    }

}
