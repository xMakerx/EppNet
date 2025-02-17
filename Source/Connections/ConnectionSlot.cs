///////////////////////////////////////////////////////
/// Filename: ConnectionSlot.cs
/// Date: February 17, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Collections;

using System;

namespace EppNet.Connections
{

    public struct ConnectionSlot : IPageable, IEquatable<ConnectionSlot>
    {

        public IPage Page { set; get; }

        public long ID { set; get; }

        public bool Allocated { set; get; }

        public Connection Connection { set; get; }

        public ConnectionSlot(IPage page, long id, Connection connection)
        {
            this.Page = page;
            this.ID = id;
            this.Connection = connection;
            this.Allocated = false;
        }

        public readonly override bool Equals(object obj)
            => obj is ConnectionSlot other &&
            Equals(other);

        public readonly bool Equals(ConnectionSlot other)
            => other.Page == Page &&
            other.ID == ID &&
            other.Connection == Connection;

        public readonly override int GetHashCode()
            => ID.GetHashCode() ^
            Page.GetHashCode() ^
            (Connection != null ? Connection.GetHashCode() : 1);

        public void Dispose()
        {
            if (Connection != null && Connection is IDisposable disposable)
                disposable.Dispose();

            this.Connection = null;
        }
    }

}
