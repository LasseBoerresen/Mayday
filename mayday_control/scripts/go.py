import math

from legcurve import legcurve
import time

from main import create_mayday


def go():
    robot = create_mayday()
    timestamps = range(30)
    for timestamp in timestamps:
        go_leggroup_a(timestamp, robot)
        go_leggroup_b(timestamp, robot)

        time.sleep(1)

def go_leggroup_a(timestamp, robot):
    leggroup_a = (0, 2, 4)
    for leg_id in leggroup_a:
        go_one_leg(leg_id, timestamp, robot)

def go_leggroup_b(timestamp, robot):
    leggroup_b = (1, 3, 5)
    for leg_id in leggroup_b:
        go_one_leg(leg_id, timestamp+3, robot)


def go_one_leg(leg_id, timestamp, robot):
    leg_angle = legcurve(timestamp)
    
    leg_angle = [r*math.tau for r in leg_angle]
    
    robot.set_leg_joint_positions(leg_id, leg_angle)
    print(f'leg_id: {leg_id}, time {timestamp}: {leg_angle}')

if __name__ == '__main__':
    go()
