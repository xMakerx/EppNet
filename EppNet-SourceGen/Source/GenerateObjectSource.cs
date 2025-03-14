/////////////////////////////////////////////
/// Filename: GenerateObjectSource.cs
/// Date: March 14, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////
using EppNet.SourceGen.Models;

using System.Collections.Generic;

namespace EppNet.SourceGen.Source
{

    public class GenerateObjectSource
    {

        public NetworkObjectModel Model { get; }

        public string FullyQualifiedName { get; }

        public HashSet<string> Imports;

        public GenerateObjectSource(NetworkObjectModel model, string fullyQualifiedName)
        {
            this.Model = model;
            this.FullyQualifiedName = fullyQualifiedName;
        }

        

    }

}
