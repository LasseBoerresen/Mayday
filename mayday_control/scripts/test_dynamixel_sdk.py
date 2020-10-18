import pytest
from official_dynamixel_sdk_tests.protocol2_0 import read_write


class TestDynamixelSdk:
    def test_read_write(self):
        read_write.main()


if __name__ == '__main__':
    pytest.main()
