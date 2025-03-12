/////////////////////////////////////////////
/// Filename: NetworkMethodModel.cs
/// Date: February 17, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using Microsoft.CodeAnalysis;

using System;
using System.Text;

namespace EppNet.SourceGen.Models
{
    public readonly struct NetworkMethodModel(ISymbol symbol, EquatableList<NetworkParameterTypeModel> parameters) : IEquatable<NetworkMethodModel>
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

        public EquatableList<NetworkParameterTypeModel> Parameters { get; } = parameters;

        public override bool Equals(object obj)
            => obj is NetworkMethodModel model &&
            Equals(model);

        public bool Equals(NetworkMethodModel other) =>
            Name == other.Name &&
            Namespace == other.Namespace &&
            Parameters == other.Parameters;

        public override string ToString()
        {
            StringBuilder builder = new($"{Name}(");

            for (int i = 0; i < Parameters.Count; i++)
            {
                NetworkParameterTypeModel type = Parameters[i];
                builder.Append(type);

                if (i + 1 < Parameters.Count)
                    builder.Append(", ");
            }

            builder.Append(")");
            return builder.ToString();
        }

        public override int GetHashCode() =>
            Name.GetHashCode() ^
            Namespace.GetHashCode() ^
            Parameters.GetHashCode();

        public static bool operator ==(NetworkMethodModel left, NetworkMethodModel right) =>
            left.Equals(right);

        public static bool operator !=(NetworkMethodModel left, NetworkMethodModel right) =>
            !left.Equals(right);
    }

}
