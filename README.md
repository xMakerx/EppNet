# E++Net
![Test Status](https://github.com/xMakerx/EppNet/actions/workflows/dotnet.yml/badge.svg)

Not to be confused as an implementation of ENet in C++, E++Net is a C# networking library built on top
of [ENet-CSharp](https://github.com/nxrighthere/ENet-CSharp/), an independent implementation of ENet with additional features
such as IPv6 support. It offers a high-level abstraction layer that implements a lot of features multiplayer games often
need such as:
1. **Snapshotting**: "Rewind" the world on the server side; useful for real-time games where you need to determine if a user completed a headshot.
2. **Security**: By default, E++Net is server authoritative and ensures only clients with the right game version can connect.
3. **Interest System**: Large multiplayer worlds don't need to update every player on everything going on within the game. The interest system only updates clients interested in specific objects, zones, etc.

## Why E++Net?
E++Net is a high performance, portable, and general-purpose UDP solution for multiplayer worlds. From simple pong games all the way up to a full-fledged MMO, you can reap
the benefits of a high throughput and maintainable multiplayer library to power your virtual worlds. E++Net abstracts the complicated components of networking so you
can focus on building out your game rather than considering how data will move from point A to point B.

Instead of messing around with packets, you could do the following to specify that a `SetHealth` update can only be sent by
the server, saved (called once more when a new client gains interest), and broadcast to every client interested in the object.
```
// Subject to change
[NetworkAttribute(required = false, flags = BROADCAST | SERVER_SEND | RAM)]
public void SetHealth(int health)
{
    this.hp = health;
}
```
E++Net will take care of the rest!

### FAQ

> ## Is E++Net intended to be used with a particular game engine such as Godot or Unity?
> **No!** E++Net is a general purpose networking library that is designed with freedom in mind.
>
> ## Does E++Net support multi-threading?
> **Yes!** E++Net internally utilizes a .NET port of [LMAX Exchange's Disruptor](https://github.com/LMAX-Exchange/disruptor) called [Disruptor-Net](https://github.com/disruptor-net/Disruptor-net) for high throughput messaging.
>
> ## Is E++Net memory or bandwidth intensive?
> E++Net leverages [Microsoft's RecyclableMemoryStreams](https://github.com/microsoft/Microsoft.IO.RecyclableMemoryStream) (memory stream pooling) and stack allocated byte arrays for datagram reading and writing. In addition,
> E++Net sends updates on a "need-to-know" basis rather than a constant flow of potentially unnecessary state updates to every interested client.
>
> ## How does E++Net handle seemingly unknown object types at runtime? Isn't reflection slow?
> E++Net uses compiled LINQ expressions to manipulate objects which is several orders of magnitude faster than standard reflection or an `Activator#CreateInstance()` approach when generating objects. However,
> this comes as a tradeoff for slower startup times as time is needed to compile the necessary expressions for object generation and manipulation.


### Inspiration

This project has been inspired by:
- [Online Theme Park System (Panda3D's Distributed Networking System)](https://docs.panda3d.org/1.10/python/programming/networking/distributed/index)
- [Valve's Game Networking Sockets](https://github.com/ValveSoftware/GameNetworkingSockets)
