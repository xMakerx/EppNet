/////////////////////////////////////////////
/// Filename: NetworkObjectModel.cs
/// Date: February 8, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using Microsoft.CodeAnalysis;

using System;

namespace EppNet.SourceGen.Models
{

    [Flags]
    public enum NetworkObjectAnalysisError
    {
        None                = 0,

        /// <summary>
        /// Located definition is not partial
        /// </summary>
        NotPartial          = 1 << 0,

        /// <summary>
        /// Located definition does not implement INetworkObject
        /// </summary>
        LacksInheritance    = 1 << 1,

        /// <summary>
        /// Is not a class
        /// </summary>
        NotClass            = 1 << 2,
    }

    /// <summary>
    /// Stores information about a particular network type resolver
    /// </summary>
    public readonly struct NetworkObjectModel(ISymbol symbol) : IEquatable<NetworkObjectModel>
    {
        public string Name { get; } = symbol.Name;

        /// <summary>
        /// Most resolvers will derive from <see cref="Globals.DataPath"/>
        /// </summary>
        public string Namespace { get; } = symbol.ContainingNamespace.Name;

        public string FullNamespace
        {
            get
            {
                string fullName = symbol.ToDisplayString();
                return fullName.Substring(0, fullName.Length - Name.Length - 1);
            }
        }

        public override bool Equals(object obj) =>
            obj is NetworkObjectModel model &&
            Equals(model);

        public bool Equals(NetworkObjectModel other) =>
            Name == other.Name &&
            Namespace == other.Namespace;

        public override string ToString() =>
            $"{Name}";

        public override int GetHashCode() => Name.GetHashCode() ^ Namespace.GetHashCode();

        public static bool operator ==(NetworkObjectModel left, NetworkObjectModel right) => left.Equals(right);
        public static bool operator !=(NetworkObjectModel left, NetworkObjectModel right) => !left.Equals(right);
    }

}
