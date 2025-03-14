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

        public EquatableList<NetworkParameterTypeModel> Parameters { get; } = parameters;

        public override bool Equals(object obj)
            => obj is NetworkMethodModel model &&
            Equals(model);

        public bool Equals(NetworkMethodModel other) =>
            Name == other.Name &&
            Parameters == other.Parameters;

        public override string ToString()
        {
            int paramCount = Parameters?.Count ?? 0;
            StringBuilder builder = new($"Parameters: {paramCount} {Name}(");

            for (int i = 0; i < Parameters.Count; i++)
            {
                NetworkParameterTypeModel type = Parameters[i];
                int subtypeCount = type.Subtypes?.Count ?? 0;
                builder.Append($"st:{subtypeCount} ");
                builder.Append(type);

                if (i + 1 < Parameters.Count)
                    builder.Append(", ");
            }

            builder.Append(")");
            return builder.ToString();
        }

        public override int GetHashCode() =>
            Name.GetHashCode() ^
            Parameters.GetHashCode();

        public static bool operator ==(NetworkMethodModel left, NetworkMethodModel right) =>
            left.Equals(right);

        public static bool operator !=(NetworkMethodModel left, NetworkMethodModel right) =>
            !left.Equals(right);
    }

}
