///////////////////////////////////////////////////////
/// Filename: IObjectPool.cs
/// Date: September 25, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Objects
{

    public interface IObjectPool
    {
        void SetMaxCapacity(int maxCapacity);
        void SetCapacity(int capacity);

        bool TryReturnToPool(IPoolable obj);

        IPoolable Get();

    }

}
