import pytest
import json
from official_dynamixel_sdk_tests.protocol2_0 import read_write

with open('mayday_config.json', 'r') as f:
    config = json.load(f)


@pytest.mark.skipif(not config['robot_is_connected'], reason='Robot is not connectd')
class TestDynamixelSdk:
    def test_read_write(self):
        read_write.main()


if __name__ == '__main__':
    pytest.main()
