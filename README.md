# Unity-UdpSocket-BitStream-Utilities
This project provides source codes and a UnityPackage for using c# udp sockets and bitwise memory streams.

## Udp sockets
> *For not experiencing .NET compatibility issues after exporting your project to other platforms, change Api compatibility LEVEL to .NET 2.0:  File -> Build settings -> Player Settings -> Other settings -> Optimization -> Api compatibility LEVEL = .NET 2.0*

Although Unity provides a networking api and protocol for socket programming and multiplayer game implementation, sometimes you need to establish your own bare sockets and communication protocol.

As at the time i didn't find a ready to use, clear and multithread example of c# sockets usage in unity i started to write a simple one for udp communications.

There is a *UdpSocketManager* class in this package which is responsible for udp communications and has below methods:
* **UdpSocketManager(string serverIp, int serverPort)** receives serverIp & serverPort for further use
* **IEnumerator initSocket()** Resets UdpSocketManager state to default and starts Send & Receive threads
* **bool isInitializationCompleted()** Returns true if listen port has bound successfully
* **int getListenPort()** Returns current client UDP listen port
* **void send(byte[] data)** Adds an array of bytes to Queue for sending to server
* **IList<byte[]> receive()** Fetches received byte arrays from queue and return them as a list
* **void closeSocketThreads()** Closes Send & Receive threads and ends listening

Also there is a *UdpSocketManagerUsageExample* MonoBehavior script in the package which demonstrates a simple udp communication.

## Bitwise Memory streams
> *this section of package uses some ideas from the [Multiplayer Game Programming](https://www.pearson.com/us/higher-education/program/Glazer-Multiplayer-Game-Programming-Architecting-Networked-Games/PGM317032.html) book.*

In realtime multiplayer games you need to send something like 32 packets per second to server(and receive), also your packets shouldn't be bigger than MTU size(1500 bytes with tcp/ip headers) for not being chunked in lower tcp/ip layers.
So the packet size is very important and your game usually has lots of data which are mostly numbers.
In other hand you don't need to send int variables with max value of 127 with traditionally 32 bits, you only need 7 bits!
Also you can convert low-range not very precise float values to ints and send them with fewer bits.

For saving data with fewer bits like above, you can use Bitwise memory stream classes in this package.

### BitwiseMemoryOutputStream
BitwiseMemoryOutputStream class is responsible for writing data with bit precision to stream and has below methods:
* **void writeInt(int data)** writes an int value to buffer by 32 bits
* **void writeInt(int data, int bitCount)** writes an int value to buffer by specified bits count
* **void writeLong(long data)** writes a long value to buffer by 64 bits
* **void writeLong(long data, int bitCount)** writes a long value to buffer by specified bits count
* **void writeDouble(double data)** writes a double value to buffer by 64 bits
* **void writeFloat(float data)** writes a float value to buffer by 32 bits
* **void writeSigned(int data)** writes a signed int value to buffer by 32 bits
* **void writeSigned(int data, int bitCount)** writes a signed int value to buffer by specified bits count
* **void writeSignedLong(long data)** writes a signed long value to buffer by 64 bits
* **void writeSignedLong(long data, int bitCount)** writes a signed long value to buffer by specified bits count
* **void writeByte(byte data)** writes a byte value to buffer by 8 bits
* **void writeByte(byte data, int bitCount)** writes a byte value to buffer by specified bits count
* **void writeBool(bool data)** writes a bool value to buffer by 1 bit
* **void writeString(string data)** writes a string value to buffer based on string's length
* **byte[] getBuffer()** returns buffer as a byte array
* **int getBitLength()** returns buffer length in bits
* **int getByteLength()** returns buffer length in bytes

### BitwiseMemoryInputStream
BitwiseMemoryInputStream class is responsible for reading data with bit precision from stream and has below methods:
* **int readInt()** reads an int value from buffer by 32 bits
* **int readInt(int bitCount)** reads an int value from buffer by specified bits count
* **long readLong()** reads a long value from buffer by 64 bits
* **long readLong(int bitCount)** reads a long value from buffer by specified bits count
* **double readDouble()** reads a double value from buffer by 64 bits
* **float readFloat()** reads a float value from buffer by 32 bits
* **int readSignedInt()** reads a signed int value from buffer by 32 bits
* **int readSignedInt(int bitCount)** reads a signed int value from buffer by specified bits count
* **long readSignedLong()** reads a signed long value from buffer by 64 bits
* **long readSignedLong(int bitCount)** reads a signed long value from buffer by specified bits count
* **byte readByte()** reads a byte value from buffer by 8 bits
* **bool readBool()** reads a bool value from buffer by 1 bit
* **string readString()** reads a string value from buffer based on string's length
* **byte[] getBuffer()** returns buffer as a byte array
* **int getRemainingBytes()** returns count of remaining buffer bytes to read

#### Notices
* You must read data from stream in same order you wrote.
* As float/double representation in binary is different than int/long, i didn't provide methods for writing/reading them with fewer bits.
* As writeSigned methods first write number's sign as a bool and then write data's absolute value as an int or long and writeString method first writes string length as an int and then writes string's bytes, for reading this types you need to use same Stream classes or write a same implementation in other languages.

> *I have wrote a Java implementation of this bitwise memory stream classes in a different repository which you can use in your Java server: [Java-memory-bit-stream](https://github.com/M-Aghasi/Java-memory-bit-stream).*

## How to use
The easiest way to use this utilities is to install it as a UnityPackage, you can find the last build in [Releases](https://github.com/M-Aghasi/Unity-UdpSocket-BitStream-Utilities/releases) section.

Also you can manually import scripts from [this folder](https://github.com/M-Aghasi/Unity-UdpSocket-BitStream-Utilities/tree/master/Assets/Scripts/UdpSocket_BitStream_Utilities) to your unity project.
