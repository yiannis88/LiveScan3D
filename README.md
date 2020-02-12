# LiveScan3D #
LiveScan3D is a system designed for real time 3D reconstruction using multiple Kinect v2 depth sensors simultaneously at real time speed.

If you use this software in your research, then please use the following citation:

Kowalski, M.; Naruniec, J.; Daniluk, M.: "LiveScan3D: A Fast and Inexpensive 3D Data
Acquisition System for Multiple Kinect v2 Sensors". in 3D Vision (3DV), 2015 International Conference on, Lyon, France, 2015

## Authors ##
  * Marek Kowalski <m.kowalski@ire.pw.edu.pl>, homepage: http://home.elka.pw.edu.pl/~mkowals6/
  * Jacek Naruniec <j.naruniec@ire.pw.edu.pl>, homepage: http://home.elka.pw.edu.pl/~jnarunie/

## Multisource
As part of an under-graduate dissertation, Andy Pack <andrewjpack@gmail.com> is extending the functionality of the suite to include multiple sources. Here a source is defined by the current use case of a single or multiple-view configuration of a single scene. The multisource capabilities will allow multiple locations to be received for simultaneous delivery.

#### Current Work
The native OpenGL display has been modified in order to allow simultaneous display of mulitple point clouds. 

Individual scenes can be manipulated within the display space with arbitrary rotation and translation using keyboard controls.

The ongoing development for the multisource network capabilities is based on the work of Ioannis Selenis to include a system of frame buffers throughout the stream, use multiple TCP connections per client and make the network actions non-blocking. 
  
## Future
Further development is undergo to study the behaviour of a holographic system under various networks (e.g. 5G, 4G, Wi-Fi) with different characteristics. The app has already been modified to run on mobile devices (e.g. mobile phones, and not only on hololenses as it was originally developed)

    * Ioannis Selinis <5GIC University of Surrey> 2019 (e-mail: selinis.g@gmail.com)
    
## Issues:
    - If there is an issue with the live show, try to disable the compression level (set it to 0, in the settings)
