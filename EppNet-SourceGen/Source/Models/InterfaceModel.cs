/////////////////////////////////////////////
/// Filename: InterfaceModel.cs
/// Date: February 8, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using System;

namespace EppNet.SourceGen.Models
{

    public readonly struct InterfaceModel : IEquatable<InterfaceModel>
    {

        public string FullQualifiedName { get; }

        public override bool Equals(object other) =>
            other is InterfaceModel model && Equals(model);

        public bool Equals(InterfaceModel other) => FullQualifiedName == other.FullQualifiedName;

        public override int GetHashCode() => FullQualifiedName.GetHashCode();

        public static bool operator ==(InterfaceModel left, InterfaceModel right) => left.Equals(right);
        public static bool operator !=(InterfaceModel left, InterfaceModel right) => !left.Equals(right);
    }

}
