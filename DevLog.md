# Development Log

### Adding duration to movement commands for pacing
#### 2026-01-02 12:35 +01:00 @Home
So I got Timed<T> targets to compile everywhere in the codebase. And implemented interpolation in the dynamixel adapter. However, there are 3 issues. Because the pid controll is not strong right now, i.e. the 'P' term is only 200 (whatever unit that is), the motors do not overcome friction immediately and do not reach the final position under weight. This means, now that I do smaller movements and control behavior more directly, I should probably upp the P and I terms, to have a more direct result. Also, because I cannot trust it to reach the target angle, using the current angle as a baseline for each interpolated step does not work, because the first couple of steps are so small that the interpolated goal is further from the target that we started with, because the controller is a PID controller, and does not put infinite effort into reaching the goal. Actually, checking the current PID gains, it is 640, 0, 3600. So the reason we dont reach our goals is that we have 0 I. 
TODO:
1. DONE When setting GoalAngle, also store PreviousGoal, so we can reliably step towards the goal in linear steps, not affected by how close we originally was to the previous goal.
2. DONE Change pid to P 640 and I 2000
3. Use the Periodic Scheduler with accurate intervals, (by polling), so we take accurate timeSteps.

P.S. It worked, using previous goal means it moves consistently independent of offset. Having an I gain makes the robot twitch and has a spring to it in a satisfying way and also means it overcomes friction and gravity a bit better. Now I also dont need the current angle when setting goal, so I can decouple those in frequency. Because it is a bottle neck to communicate with the robot. And not having to read the angle every time saves 50%, which means we can get to approx 15ms period, if I dont "oversleep".

### Adding duration to movement commands for pacing
#### 2025-12-31 11:37 +01:00 @Home
My plan was to have the dynamixel communication adapter update the goal position at the update frequency, but really, setting different goals is the job of the motion planner. If we dont set goals only in the joint domain, the movement will not follow the trajectory, because different parts of the trajectory needs joints moving at different speeds. My thougts we centering on the fact, that only the specific dynamixel joints know how strong the motor is, and thus it could be the responsibility of the joint to pace itself for a given goal angle and destination time. And that might still be the case. Because if not, the motor will try to move to the goal destination, however far it is, at max speed. We could mitigate that, with frequent motion planning updates, but decoupling them would probably be better. Then the motion planner is free to chose any appropriate update requencey to determine how accurately it will follow its planned trajectory. So I want all these components to have a datetimeprovider, in order to both set destination timestamps and compare that to the current time. I mean... Humans do not know the absolute timestamp of anything, but estimate how long something will take. Like, I want to get up from this chair in like a few seconds. Or, do we even do that. We just move at our "regular pace". 

### Back on the horse. Or spider... 
#### 2025-02-07 19:07 +01:00 @Home
Ran the test suite. Everything works, except, the integration tests can only run one at a time manually.

Starting up with my first project in linear. Making mayday sway. No walking. Just inverse kinematics.
Could be cool to train it. Input vs output. Simply one leg at a time. To learn what joint angles give 
what leg positions. Or body position. It should be doable. Record some movements. And learn a mapping.
As long as I have forward kinematics. I can get the results. Then the backwards kinematics and dynamics can be learned,
presumably. For known tasks, like swaying. 

So, where should I start? Lets assume I have forward kinematics to work. I neeed to tdd, that I can record some data
to train on. 

So, start simple, with learning the movements of a single leg. Simply record its joint angles, calculate the tibia tip,
add a random small vector to it. Run the network for new joint angles. Actually move. Record the new tip position and 
calculate the error. Then add it to the data set and do an epoch. Sampling from newest data first, because it has been 
trained on less, but never forget the old samples. We can even do 6 legs at a time, because from their own perspective, 
they are equal. And we try to make the robot more robust. Because motors will fail. Stuff will happen. 

If it goes well, we can add not just position goal, but how quickly it should get there. Dynamics. It will learn from
the real world robot, auto calibrate to the weight etc. 

With that, we can do the sway. Calculating body position from leg ground plane. Lots of stuff to do. But we can also 
just do gamepad movement of all legs together. 

Maybe we can start by simply iterating through some grid positions of the joints, and learning from those. That way, the
legs will know the basic possible positions and their theoretical ground truth tip positions. But then with real world 
data, we will get much more realistic data. Actually, I will just skip this step. I think going to real world data is 
better. 
 


### Restarting implementing of mayday in c#
#### 2024-03-29 11:25 +01:00 @Bed with ivy sleeping
- TODO
  - Look through the structure.
  - Start with the sequence diagram, end to end. For just waking up and standing :)

- Log
  - I need to make an environment, where I can run tests against both a mock, who simply sends the position back that is requested, stable temperature, proportional force etc. This is a foolishly simple simulation. But I simply need it for working on the infrastructure. 



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
