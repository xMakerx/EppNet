# E++Net (**IN DEV**)
![Test Status](https://github.com/xMakerx/EppNet/actions/workflows/dotnet.yml/badge.svg)

Not to be confused as an implementation of ENet in C++, E++Net is a C# networking library built on top
of [ENet-CSharp](https://github.com/nxrighthere/ENet-CSharp/), a C# port of ENet with additional features
such as IPv6 support. 

## Features
- [x] _**No manual data serialization required!**_ Register a custom object, write your methods and properties, and add attributes to let E++Net know how to interact with them.

      **Custom method parameter types unknown to E++Net can be registered. Game engines that don't support `new()`
      to construct a game object can specify a custom generator to make one properly.**
- [X] _**Subscription based interest system!**_ Clients only receive updates about objects (and their children) they have interest in. No wasted bandwidth on unnecessary data!
- [X] _**Tree hierarchy for objects!**_ Objects can have as many children objects as you would like. You can reparent objects on the fly and clients automatically subscribe to new children objects.
      
      **Network methods and properties are inherited from base types that declare them!**
- [X] _**Snapshotting!**_ E++Net keeps track of the last N "ticks" of the simulation for interpolation, extrapolation, and to offer a smooth and responsive experience for even the most high latency clients.
- [X] _**Security!**_ E++Net allows you to implement your own authentication method for newly connected clients, limit what kinds of datagrams they can send, and automatically timeout clients that fail to identify themselves.

      E++Net, by default, is server authoritative and clients only know about the remote server connection.
      Commands can only be sent by clients if the server explicitly allows it.
- [X] _**Concurrency!**_ E++Net leverages modern .NET features for high throughput message serialization and deserialization automatically.
- [X] _**Modular!**_ E++Net is designed to be extendable, easy to maintain, and easy to turn features on and off via a JSON configuration or through code.
- [X] _**Flexible!**_ Don't like a particular abstraction or service? The API is forgiving and allows changes from your assemblies. Extend an existing type and override existing methods to make it your own!

## Why E++Net?
E++Net is a high performance, portable, and general-purpose UDP solution designed to abstract away the most frustrating aspects of multiplayer application development -- especially for games. As a multiplayer game
developer myself, I was absolutely tired of constantly writing different serializables, unique packet or datagram types, and spending a ton of time designing a bunch of data plumbing -- it gets even worse when concurrency
is involved! Sometimes writing helper classes or extensions methods would save me some work of course, but, in other libraries you might accumulate a bunch of garbage or wasted CPU cycles because you did something the library or framework
developer didn't expect. The last thing I wanted to do was spend my precious free time doing data plumbing. E++Net is designed to do all the heavy lifting but leave enough flexibility for developers who need to do some fine-tuning.

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
> **No!** E++Net is a general purpose networking library with portability in mind. In the future, I intend on making demos or optional extension packages for specific game engines.
>
> ## Does E++Net support multi-threading?
> **Yes!** E++Net internally utilizes [System.Threading.Channels](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels) for high throughput messaging.
>
> ## Is E++Net memory or bandwidth intensive?
> E++Net leverages [Microsoft's RecyclableMemoryStreams](https://github.com/microsoft/Microsoft.IO.RecyclableMemoryStream) (memory stream pooling) and stack allocated byte arrays for datagram reading and writing.
> This means that E++Net does not create garbage when reading or writing from a datagram. E++Net only updates remote clients about objects and events they've subscribed to.
>
> ## How does E++Net handle seemingly unknown object types at runtime? Isn't reflection slow?
> E++Net uses compiled expression trees to manipulate objects which is several orders of magnitude faster than standard reflection or an `Activator#CreateInstance()` approach when generating objects. However,
> this comes as a tradeoff for slower startup times as time is needed to compile the necessary expressions for object generation and manipulation.


### Inspiration

This project has been inspired by:
- [Online Theme Park System (Panda3D's Distributed Networking System)](https://docs.panda3d.org/1.10/python/programming/networking/distributed/index)
- [Valve's Game Networking Sockets](https://github.com/ValveSoftware/GameNetworkingSockets)

### Special Thanks
- [Tanner Gooding](https://github.com/tannergooding) Microsoft .NET Team - Architecture advice
- [Stuart Turner](https://github.com/viceroypenguin) - Architecture advice
- [Brian Lach](https://github.com/lachbr) - For listening to my ramblings, rants, and giving advice
- [Stanislav Denisov](https://github.com/nxrighthere/) - Creator and Maintainer of C# port of ENet
- [Lee Salzman](https://github.com/lsalzman) - Creator of ENet (to my knowledge)
