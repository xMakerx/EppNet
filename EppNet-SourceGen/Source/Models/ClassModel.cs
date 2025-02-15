/////////////////////////////////////////////
/// Filename: ClassModel.cs
/// Date: February 8, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using Microsoft.CodeAnalysis;

using System;

namespace EppNet.SourceGen.Models
{

    public readonly struct ClassModel : IEquatable<ClassModel>
    {

        public string Name { get; }
        public string Namespace { get; }
        public string FullName { get => Namespace + "." + Name; }

        public EquatableList<InterfaceModel> Interfaces { get; }

        public ClassModel(ISymbol symbol) : this(symbol, null) { }

        public ClassModel(ISymbol symbol, EquatableList<InterfaceModel> interfaces)
        {
            this.Name = symbol.Name;
            this.Namespace = symbol.ContainingNamespace.ToDisplayString();
            this.Interfaces = interfaces;
        }

        public override bool Equals(object other) =>
            other is ClassModel model && Equals(model);

        public bool Equals(ClassModel other) =>
            Name == other.Name &&
            Namespace == other.Namespace &&
            Interfaces == other.Interfaces;

        public override int GetHashCode() => Name.GetHashCode() ^ Namespace.GetHashCode() ^ Interfaces.GetHashCode();

        public static bool operator ==(ClassModel left, ClassModel right) => left.Equals(right);
        public static bool operator !=(ClassModel left, ClassModel right) => !left.Equals(right);
    }

}
