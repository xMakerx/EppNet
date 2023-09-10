# E++Net
Not to be confused as an implementation of ENet in C++, E++Net is a C# networking library built on top
of [ENet-CSharp](https://github.com/nxrighthere/ENet-CSharp/), an independent implementation of ENet with additional features
such as IPv6 support. It offers a high-level abstraction layer that implements a lot of features multiplayer games often
need such as:
1. **Snapshotting**: "Rewind" the world on the server side; useful for real-time games where you need to determine if a user completed a headshot.
2. **Security**: By default, E++Net is server authoritative and ensures only clients with the right game version can connect.
3. **Interest System**: Large multiplayer worlds don't need to update every player on everything going on within the game. The interest system only updates clients interested in specific objects, zones, etc.

## Why E++Net?
E++Net is meant to drastically improve development time by providing a generic UDP solution for multiplayer worlds of varying
scales. Anything from a simple pong game all the way up to an MMO. Spend less time being a data plumber and spend more time
building out your game! This is supposed to be the perfect solution for Godot and Unity games.

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

### Inspiration
This project has been inspired by:
- [Online Theme Park System (Panda3D's Distributed Networking System)](https://docs.panda3d.org/1.10/python/programming/networking/distributed/index)
- [Valve's Game Networking Sockets](https://github.com/ValveSoftware/GameNetworkingSockets)
