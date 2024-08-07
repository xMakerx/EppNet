# E++Net
![Test Status](https://github.com/xMakerx/EppNet/actions/workflows/dotnet.yml/badge.svg)

Not to be confused as an implementation of ENet in C++, E++Net is a C# networking library built on top
of [ENet-CSharp](https://github.com/nxrighthere/ENet-CSharp/), an independent implementation of ENet with additional features
such as IPv6 support. E++Net offers a high-level abstraction layer that implements a lot of features multiplayer games often
need such as:
1. **Snapshotting**: "Rewind" the world on the server side; useful for real-time games where you need to determine if a user completed a headshot.
2. **Security**: By default, E++Net is server authoritative and ensures only clients with the right game version can connect.
3. **Interest System**: Large multiplayer worlds don't need to update every player on everything going on within the game. The interest system only updates clients interested in specific objects, zones, etc.

## Why E++Net?
E++Net is a high performance, portable, and general-purpose UDP solution for multiplayer worlds. From simple pong games all the way up to a full-fledged MMO, you can reap
the benefits of a high throughput and maintainable multiplayer library to power your virtual worlds. E++Net abstracts the complicated components of networking so you
can focus on building out your game rather than considering how data will move from point A to point B.

Take the following pseudo code featuring a client-side implementation of a Player for example.
```csharp
// The "Dist" flag dictates which distribution should use this implementation. The following
// attribute setup specifies that this class "PlayerClient" should only be used on clients.
[NetworkObject(Dist = Distribution.Client)]
class PlayerClient : ISimUnit {

    // Players will have an integer health property which can be sent by the owner
    // of the player object.
    [NetworkProperty(NetworkFlags.Persistant | NetworkFlags.OwnerSend)]
    public int Health { get; set; }

    // Players will also require a position to be generated. The snapshot
    // network flag tells E++Net to enable interpolation and extrapolation, and
    // store the value of the getter along with the current snapshot time. Synchronization
    // is done automatically and the snapshot flag implies persistance. 
    [NetworkMethod(NetworkFlags.Required | NetworkFlags.Snapshot)]
    public void SetPosition(Vector3 position) {}

    // Where's the attribute you ask? E++Net is smart enough to locate getter methods
    // if they have an identical name to the setter except with "get" instead of "set".
    public Vector3 GetPosition() {}
}
```
E++Net will take care of the rest!

### FAQ

> ## Is E++Net intended to be used with a particular game engine such as Godot or Unity?
> **No!** E++Net is a general purpose networking library that is designed with freedom in mind.
>
> ## Does E++Net support multi-threading?
> **Yes!** E++Net internally utilizes [System.Threading.Channels](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels) for high throughput messaging.
>
> ## Is E++Net memory or bandwidth intensive?
> E++Net leverages [Microsoft's RecyclableMemoryStreams](https://github.com/microsoft/Microsoft.IO.RecyclableMemoryStream) (memory stream pooling) and stack allocated byte arrays for datagram reading and writing.
> This means that E++Net does not create garbage when writing or reading from a datagram. To save bandwidth, E++Net sends updates on a "need-to-know" basis rather than a constant flow of potentially unnecessary
> state updates to every interested client, and relies on integer IDs and byte indices rather than string names for object updates.
>
> ## How does E++Net handle seemingly unknown object types at runtime? Isn't reflection slow?
> E++Net uses compiled LINQ expressions to manipulate objects which is several orders of magnitude faster than standard reflection or an `Activator#CreateInstance()` approach when generating objects. However,
> this comes as a tradeoff for slower startup times as time is needed to compile the necessary expressions for object generation and manipulation.


### Inspiration

This project has been inspired by:
- [Online Theme Park System (Panda3D's Distributed Networking System)](https://docs.panda3d.org/1.10/python/programming/networking/distributed/index)
- [Valve's Game Networking Sockets](https://github.com/ValveSoftware/GameNetworkingSockets)

### Special Thanks
- [Tanner Gooding](https://github.com/tannergooding) Microsoft .NET Team - Architecture advice
- [Stuart Turner](https://github.com/viceroypenguin) Architecture advice
- [Brian Lach](https://github.com/lachbr) - For listening to my ramblings, rants, and giving advice
- [Stanislav Denisov](https://github.com/nxrighthere/) - Creator and Maintainer of C# port of ENet
- [Lee Salzman](https://github.com/lsalzman) - Creator of ENet (to my knowledge)
