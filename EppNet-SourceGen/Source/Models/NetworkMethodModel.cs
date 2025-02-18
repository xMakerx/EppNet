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

    [Flags]
    public enum NetworkMethodAnalysisError
    {
        None = 0,

        NotMethod = 1 << 0,

        /// <summary>
        /// Not declared in a class with a NetworkObject attribute
        /// </summary>
        NotNetworkObjectClass = 1 << 1,

        Inaccessible = 1 << 2,

        /// <summary>
        /// Flags are invalid
        /// </summary>
        InvalidFlags = 1 << 3,

        /// <summary>
        /// A parameter type is missing a resolver
        /// </summary>
        MissingResolver = 1 << 4
    }

    public readonly struct NetworkMethodModel(ISymbol symbol, string[] parameterTypes) : IEquatable<NetworkMethodModel>
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

        public string[] ParameterTypes { get; } = parameterTypes;

        public override bool Equals(object obj)
            => obj is NetworkMethodModel model &&
            Equals(model);

        public bool Equals(NetworkMethodModel other) =>
            Name == other.Name &&
            Namespace == other.Namespace &&
            ParameterTypes.Equals(other.ParameterTypes);

        public override string ToString()
        {
            StringBuilder builder = new($"{Name}(");

            for (int i = 0; i < ParameterTypes.Length; i++)
            {
                string type = ParameterTypes[i];
                builder.Append(type);

                if (i + 1 < ParameterTypes.Length)
                    builder.Append(", ");
            }

            builder.Append(")");
            return builder.ToString();
        }

        public override int GetHashCode() =>
            Name.GetHashCode() ^
            Namespace.GetHashCode() ^
            ParameterTypes.GetHashCode();

        public static bool operator ==(NetworkMethodModel left, NetworkMethodModel right) =>
            left.Equals(right);

        public static bool operator !=(NetworkMethodModel left, NetworkMethodModel right) =>
            !left.Equals(right);
    }

}
