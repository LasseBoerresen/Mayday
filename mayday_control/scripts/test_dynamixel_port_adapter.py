import pytest
import pandas as pd

from dynamixel_port_adapter import DynamixelPortAdapter


@pytest.fixture()
def dxl_port_adapter():
    return DynamixelPortAdapter()


class TestDynamixelAdapterTestCase:
    def test_when_init__then_ctrl_table_is_pd_dataframe(self, dxl_port_adapter):
        assert type(dxl_port_adapter.control_table) is pd.DataFrame

    def test_when_init__then_ctrl_table_index_is_data_name(self, dxl_port_adapter):
        assert dxl_port_adapter.control_table.index.name == 'Data Name'

    def test_when_init__then_torque_enable_address_is_64(self, dxl_port_adapter):
        actual_te_address = dxl_port_adapter.control_table.loc['Torque Enable', 'Address']

        assert 64, actual_te_address


if __name__ == '__main__':
    pytest.main()
