from __future__ import annotations

import numpy as np


class IncompatibleArrayShapeException(Exception):
    def __init__(self, array: np.ndarray, expected_shape: tuple):
        super().__init__(f'Incompatible array shape, expected {expected_shape}, got {array.shape}')
