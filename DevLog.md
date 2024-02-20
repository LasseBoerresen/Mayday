# Development Log


2024-02-20 05:39:00 +01:00 @bed Thoughts from Marc Raibert interview
In general, the takeaway is that, dynamics is about utillizing stored up energy, having springyness built into the mechanics, not just the motors. But also, if you want to be natural looking, you gotta both be unstable and balanced. Natural movements are not static, usually. 

Nowadays, this is probably most easily achieved with reinforcement learning, through simulation, think the tripod gait of the ETH Zürich robot where the leg is held in place.  

It inspires me to try to make mayday jump. Be dynamic. Use built up energy in her momentum, and height. The thing I'm missing, is a way to store energy in the joints, e.g. ligaments or springs... My legs can contain a bit of flex, but not a meaningful amount. Anyway. I'm getting ahead of myself. It still can't even walk in any way. But, like the slogan... "sometimes you gotta run, before you can walk", in a sense, that dynamics should be incorporated from day one. He calls it "energetics". Taking built up energy into account. 

He also mentions how, in order to be dynamic, you have to plan ahead, at least a few seconds, long enough for you to correct any movements that will mess up the planned motion. Servoing is not enough, because it is only reactive, feed back. You need "feed forward", anticipating future movements and future forces on the robot. 


Marc Raibert: https://www.youtube.com/watch?v=5VnbBCm_ZyQ&t=1738s&ab_channel=LexFridman
Robert Playter: https://www.youtube.com/watch?v=cLVdsZ3I5os&ab_channel=LexFridman
