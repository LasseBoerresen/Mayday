from __future__ import annotations


class UnsupportedMultiplicationException(Exception):
    def __init__(self, a, b):
        super().__init__(f'Multiplication of {type(a)} and {type(b)} is not implemented')
