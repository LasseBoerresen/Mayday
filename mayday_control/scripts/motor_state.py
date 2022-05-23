from dataclasses import dataclass

from physical_quantities.angle import Angle
from physical_quantities.angular_velocity import AngularVelocity
from physical_quantities.load import Load
from physical_quantities.temperature import Temperature


@dataclass
class MotorState:
    position: Angle = None
    velocity: AngularVelocity = None
    load: Load = None
    temperature: Temperature = None
    position_goal: Angle = None
