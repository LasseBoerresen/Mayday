# Mayday
Hexapod robot designed from the bottom up to look organic, with smooth curves and move organically by having motorcontrol trained using a neural network, rather than hard coded as a set of movements. Its purpose is to be. To exist and explore its surroundings.

![Mayday on display](https://github.com/LasseBoerresen/Mayday/blob/master/Media/_DSC6254.JPG)

## Architecture and Domain Model
The sofware running Mayday is custom built from first principles, at least within the robotics domain, from high level behavior and decisions design, through kinematics to low level actuator control. 

The software is grouped in 5 main layers, as detailed on the diagram below
0. Main
1. Behavior Control
2. Motion Planning
3. Mayday Robot Structure
4. Core Robot Structure
5. Dynamixel Motor Adapter

The architecture is based on clean architecture, with an emphasis on decoupling and dependency inversion, with modules only depending on more stable ones. Deepest in the core, is the basic robot structure, defining Abstract robot concepts such as joints, attachements and links and their interaction. On this, the specific Mayday Robot Structure depends, defining how to manipulate the six legs and how they are conencted, exposing information about the robot and how to interact with it. The motion planning layer defines high level movements, like move in cardinal directions, but without control of individual limbs and joints of the robot. The motion planner, is given instructions from the behavior controller, thus, the motion planner only executes movements, if ordered to do so, by the behvior controller. This is where desires and plans come from. For now, only manual behavior controllers are implemented, specifically TerminalPostureBehaviorController and BabyLegsBehaviorController. The first give manual control to very specific predefined posture from the terminal via commands such as Sitting, Wake up, Stand etc. The second, is an auto-matic kicking behavior, giving each leg a random small movement, designed to test running the robot continuously and learning how to move the body from every kick, using continuous reinforcement learning. 

## The Code
The code itself is also following clean code priciples and is a usion of object oriented structure with highly functional computations. The current pure C# version is a total rebuild, closely inspiered of the first python version. Both were test-driven to a high degree. 

![Architechture and domain model](https://github.com/LasseBoerresen/Mayday/blob/master/Media/Mayday%20Architecture.jpg)


[Demo Video](https://youtu.be/liucpPML-Sw)
