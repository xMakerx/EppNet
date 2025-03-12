/////////////////////////////////////////////
/// Filename: NetworkObjectModel.cs
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
    public readonly struct NetworkObjectModel(INamedTypeSymbol symbol, 
        int distType, 
        EquatableList<string> baseNetObjectsFQNs, 
        EquatableDictionary<string, EquatableHashSet<NetworkMethodModel>> methodDict) : IEquatable<NetworkObjectModel>
    {
        public string Name { get; } = symbol.Name;

        public string Namespace { get; } = symbol.ContainingNamespace.Name;

        public string FullNamespace
        {
            get
            {
                string fullName = symbol.ToDisplayString();
                return fullName.Substring(0, fullName.Length - Name.Length - 1);
            }
        }

        public string FullyQualifiedName { get; } = $"{symbol.ContainingNamespace.ToDisplayString()}.{symbol.Name}";
        
        /// <summary>
        /// The hierarchy of network objects where the first element is the first network object in the tree
        /// that doesn't inherit from another network object.
        /// </summary>
        public EquatableList<string> NetObjectHierarchy { get; } = baseNetObjectsFQNs;

        public int Distribution { get; } = distType;

        public EquatableDictionary<string, EquatableHashSet<NetworkMethodModel>> Methods { get; } = methodDict;

        public override bool Equals(object obj) =>
            obj is NetworkObjectModel model &&
            Equals(model);

        public bool Equals(NetworkObjectModel other) =>
            Name == other.Name &&
            Namespace == other.Namespace &&
            FullNamespace == other.FullNamespace &&
            FullyQualifiedName == other.FullyQualifiedName &&
            NetObjectHierarchy == other.NetObjectHierarchy &&
            Methods == other.Methods;

        public override string ToString()
        {
            if (NetObjectHierarchy.Count > 0)
                return $"{FullNamespace}({NetObjectHierarchy[0]}) Methods: {Methods.Count}";

            return $"{FullNamespace} Methods: {Methods.Count}";
        }

        public override int GetHashCode() =>
            Name.GetHashCode() ^
            Namespace.GetHashCode() ^
            FullyQualifiedName.GetHashCode() ^
            (NetObjectHierarchy != null ? NetObjectHierarchy.GetHashCode() : 1) ^
            Methods.GetHashCode();

        public static bool operator ==(NetworkObjectModel left, NetworkObjectModel right) => left.Equals(right);
        public static bool operator !=(NetworkObjectModel left, NetworkObjectModel right) => !left.Equals(right);
    }

}
