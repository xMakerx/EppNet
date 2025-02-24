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

            /*foreach ()

            for (int i = 0; i < Parameters.Length; i++)
            {
                string type = ParameterTypes[i];
                builder.Append(type);

                if (i + 1 < ParameterTypes.Length)
                    builder.Append(", ");
            }

            builder.Append(")");*/
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
