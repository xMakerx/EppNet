/////////////////////////////////////////////
/// Filename: NetworkParameterTypeModel.cs
/// Date: February 18, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using Microsoft.CodeAnalysis;

using System;
using System.Text;

namespace EppNet.SourceGen.Models
{

    public readonly struct NetworkParameterTypeModel(ISymbol symbol, 
        EquatableList<NetworkParameterTypeModel> subtypes, 
        string underlyingType = null, 
        bool isNetObject = false)
        : IEquatable<NetworkParameterTypeModel>
    {
        public string Name { get; } = symbol.Name;
        public string Namespace { get; } = symbol.ContainingNamespace.Name;
        public string FullyQualifiedName { get; } = $"{symbol.ContainingNamespace.ToDisplayString()}.{symbol.Name}";
        public string UnderlyingTypeFullyQualifiedName { get; } = underlyingType;

        public string TypeAsWritten { get; } = symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

        public EquatableList<NetworkParameterTypeModel> Subtypes { get; } = subtypes;

        public bool IsNetObject { get; } = isNetObject;

        public override bool Equals(object obj) =>
            obj is NetworkParameterTypeModel model &&
            Equals(model);

        public bool Equals(NetworkParameterTypeModel other) =>
            Name == other.Name &&
            Namespace == other.Namespace &&
            FullyQualifiedName == other.FullyQualifiedName &&
            Subtypes == other.Subtypes &&
            UnderlyingTypeFullyQualifiedName == other.UnderlyingTypeFullyQualifiedName &&
            // let's not include this so we don't rerun the pipeline TypeAsWritten == other.TypeAsWritten &&
            IsNetObject == other.IsNetObject;

        public override string ToString()
        {
            if (Subtypes != null && Subtypes.Count > 0)
            {
                StringBuilder builder = new($"{FullyQualifiedName}<");
                
                for (int i = 0; i < Subtypes.Count; i++)
                {
                    builder.Append(Subtypes[i].ToString());

                    if (i + 1 < Subtypes.Count)
                        builder.Append(", ");
                }

                builder.Append(">");
                return builder.ToString();
            }

            return FullyQualifiedName;
        }

        public override int GetHashCode() =>
            Name.GetHashCode() ^
            Namespace.GetHashCode() ^
            FullyQualifiedName.GetHashCode() ^
            ((Subtypes != null) ? Subtypes.GetHashCode() : 1) ^
            ((UnderlyingTypeFullyQualifiedName != null) ? UnderlyingTypeFullyQualifiedName.GetHashCode() : 1) ^
            IsNetObject.GetHashCode();

        public static bool operator ==(NetworkParameterTypeModel left, NetworkParameterTypeModel right) =>
            left.Equals(right);

        public static bool operator !=(NetworkParameterTypeModel left, NetworkParameterTypeModel right) =>
            !left.Equals(right);

    }
}
