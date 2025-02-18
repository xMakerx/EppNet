//////////////////////////////////////////////
/// Filename: ObjectMethodDefinition.cs
/// Date: February 17, 2025
/// Author: Maverick Liberty
//////////////////////////////////////////////

namespace EppNet.Objects
{

    public class ObjectMethodDefinition<T> where T : struct
    {

        public string Name { get; }

        public T Parameters { get; }

        public int NumParameters { get; }

        public ObjectMethodDefinition(string name, T parameters, int numParameters)
        {
            this.Name = name;
            this.Parameters = parameters;
            this.NumParameters = numParameters;
        }
    }

}
