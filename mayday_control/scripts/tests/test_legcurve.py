import pytest

from legcurve import legcurve


def test_p3_after_3_seconds():
    res = legcurve(3)

    assert res == (0.07, -0.17, -0.07)

def test_p2_after_2_seconds():
    res = legcurve(2)

    assert res == (-0.07, -0.17, -0.07)


if __name__ == '__main__':
    pytest.main()