# Multi-Source

The extension of the LiveScan3D suite to include multiple sources has been implemented through the use of a ***Source ID*** propagated throughout frame transmission from client to UE. 

The ***Source ID*** is represented by 1 byte throughout the network resulting in 128 available IDs from 0-127.

# Packet Structure

The below outlines the packet structure for transmission from clients to server and from server to UE. The aspects in **bold** indicate additions by Andy Pack as part of the multi-source extension project.

## Kinect Server (Client -> Server)

As a result of the higher number of message types between client and server, some of which have variable header lengths or are source agnostic, source IDs make up the first byte of the payload.

*Header **(13 Bytes)***

* (1 Byte) Message Type
* (4 Bytes) Data Length (bytes)
    - Without Header
* (4 Bytes) Compression
    - Typically unused, compression seems to cause odd issues or zero functionality
* (4 Bytes) Timestamp

*Payload*

* **(1 Byte) Source ID**
* (4 Bytes) Population of point cloud
* Point Cloud
    - Serialised point by point in following format
        - (1 Byte) R
        - (1 Byte) G
        - (1 Byte) B
        - (2 Bytes) x
        - (2 Bytes) y
        - (2 Bytes) z
* (4 Bytes) Number of Bodies
* Bodies
    - (1 Byte) Tracked?
    - (4 Bytes) Number of Joints
    - Joints
        - (4 Bytes) Joint Type
        - (4 Bytes) Tracking State
        - (4 Bytes) x
        - (4 Bytes) y
        - (4 Bytes) z
        - Colour Space
            - (4 Bytes) x
            - (4 Bytes) y

## Transfer Server (Server -> UE)

Packets delivered from server to UE contain the source ID within the header of packets.

*Header **(14 Bytes)***

* (1 Byte) Message Type
* **(1 Byte) Source ID**
* (4 Bytes) Data Length (bytes)
    - Without Header
* (4 Bytes) Compression
    - Typically unused, compression seems to cause odd issues or zero functionality
* (4 Bytes) Timestamp

*Payload **(Point cloud population x 9 Bytes)***

* Vertices
    - (2 Bytes) x
    - (2 Bytes) y
    - (2 Bytes) z
* RGB
    - (1 Byte) R
    - (1 Byte) G
    - (1 Byte) B