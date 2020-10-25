from dataclasses import dataclass


@dataclass
class MotorState:
    position: float = None
    velocity: float = None
    torque: float = None
    temperature: float = None
    position_goal: float = None
