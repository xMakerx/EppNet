///////////////////////////////////////////////////////
/// Filename: ISimUnit.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
/// Simulation units are components that propagate updates
/// on the network

namespace EppNet.Simulation
{

    public interface ISimUnit
    {

        public uint ID { internal set; get; }

    }

}
