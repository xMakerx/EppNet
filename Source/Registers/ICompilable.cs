//////////////////////////////////////////////
/// Filename: ICompilable.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

using System;

namespace EppNet.Registers
{

    public struct CompilationResult
    {

        public bool Successful;
        public int NumCompiled;
        public Exception Error;

        public CompilationResult()
        {
            this.Successful = false;
            this.NumCompiled = 0;
            this.Error = null;
        }

        public CompilationResult(bool success, int numCompiled, Exception error)
        {
            this.Successful = success;
            this.NumCompiled = numCompiled;
            this.Error = error;
        }

    }

    public interface ICompilable
    {

        public CompilationResult Compile();
        public bool IsCompiled();

    }

}
