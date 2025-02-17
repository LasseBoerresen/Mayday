# Mayday
Hexapod robot designed from the bottom up to look organic, with smooth curves and move organically by having motorcontrol trained using a neural network, rather than hard coded as a set of movements. Its purpose is to be. To exist and explore its surroundings.

![Mayday on display](https://github.com/LasseBoerresen/Mayday/blob/master/Media/_DSC6254.JPG)

## Architecture and Domain Model
The sofware running Mayday is custom built from first principles, at least within the robotics domain, from high level behavior and decisions design, through kinematics to low level actuator control. 

The software is grouped in 5 main layers, as detailed on the diagram below
1. Behavior Control
2. Motion Planning
3. Mayday Robot Structure
4. Core Robot Structure
5. Dynamixel Motor Adapter

The architecture is based on clean architecture, with an emphasis on decoupling and dependency inversion, with modules only depending on more stable ones. The code itself is also following clean code priciples and is a usion of object oriented structure with highly functional computations. The current pure C# version is a total rebuild, closely inspiered of the first python version. Both were test-driven to a high degree. 

![Architechture and domain model](https://github.com/LasseBoerresen/Mayday/blob/master/Media/Mayday%20Architecture.jpg)


[Demo Video](https://youtu.be/liucpPML-Sw)
