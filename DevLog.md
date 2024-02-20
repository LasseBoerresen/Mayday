# Development Log

### Thoughts on connecting to dynamixelSDK from C#
#### 2024-02-20 08:25 +01:00 @Amarger Kaffebaren
Aparrently, DynamixelSDK for C# is only available on windows, which is just not good enough for me. I could write it myself, what I need. Testdriven. It wouldn't be that bad. But it is yak shaving. I could use ROS to communicate with my existing well written facade to the python sdk... Could be good. And an exercise in reuse. But I still kind of need to do that with ros... Also ok. I will need multiple other tools. Rviz, simulators etc. I need ros no matter what. There even is a ROS library, 

I am pondering in general, how to do robust end-to-end tests. Tests with the actual robot, doing certain movements. The purpose is to make sure the robot does not stop doing what it is supposed to do, even with new features. But also, to make sure all the parts are well integrated. ALL the parts. So, what is the answer? Well, invert. 

Q: What would some bad end-to-end tests look like?

A:
1. Hard to run. Must manually open different programs. 
2. Flaky... Not always deterministic or with a clear result. 
3. Hard to update, unstructured, 
4. Platform or device specific. Only able to run on a single machine. 
5. Manual install. Does not work out of the box when cloning the repo and running install scripts. 


But... ROS is only for linux, isn't it? Would I ever want to really run it in windows...? Well, I'm running windows now... M



### Thoughts from Marc Raibert interview
#### 2024-02-20 05:39 +01:00 @bed 
In general, the takeaway is that, dynamics is about utillizing stored up energy, having springyness built into the mechanics, not just the motors. But also, if you want to be natural looking, you gotta both be unstable and balanced. Natural movements are not static, usually. 

Nowadays, this is probably most easily achieved with reinforcement learning, through simulation, think the tripod gait of the ETH Zürich robot where the leg is held in place.  

It inspires me to try to make mayday jump. Be dynamic. Use built up energy in her momentum, and height. The thing I'm missing, is a way to store energy in the joints, e.g. ligaments or springs... My legs can contain a bit of flex, but not a meaningful amount. Anyway. I'm getting ahead of myself. It still can't even walk in any way. But, like the slogan... "sometimes you gotta run, before you can walk", in a sense, that dynamics should be incorporated from day one. He calls it "energetics". Taking built up energy into account. 

He also mentions how, in order to be dynamic, you have to plan ahead, at least a few seconds, long enough for you to correct any movements that will mess up the planned motion. Servoing is not enough, because it is only reactive, feed back. You need "feed forward", anticipating future movements and future forces on the robot. 


Marc Raibert: https://www.youtube.com/watch?v=5VnbBCm_ZyQ&t=1738s&ab_channel=LexFridman
Robert Playter: https://www.youtube.com/watch?v=cLVdsZ3I5os&ab_channel=LexFridman
