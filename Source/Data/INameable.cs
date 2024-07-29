///////////////////////////////////////////////////////
/// Filename: INameable.cs
/// Date: July 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Data
{

    public interface INameable
    {

        public string Name { set; get; }

        /// <summary>
        /// Checks if the name associated with this is valid<br/>
        /// Default is !<see cref="string.IsNullOrEmpty(string?)"/>
        /// </summary>
        /// <returns></returns>
        public bool IsNameValid() => !string.IsNullOrEmpty(Name);

    }

}