///////////////////////////////////////////////////////
/// Filename: ObjectRegistration.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Sim;

namespace EppNet.Registers
{

    public class ObjectRegistration<T> : Registration<T> where T : ISimUnit
    {

        public ObjectRegistration() : base() { }

        protected void _Internal_CompileMethods()
        {

        }

        public override bool Compile()
        {
            if (IsCompiled())
                return false;

            _Internal_CompileConstructors();
            _compiled = true;

            return true;
        }

    }

}
