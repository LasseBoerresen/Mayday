def legcurve(t):
    p0 = (0.07, -0.17, -0.08)
    p1 = (-0.07, -0.17, -0.08)
    p2 = (-0.07, -0.17, -0.07)
    p3 = (0.07, -0.17, -0.07)

    # unit: partial circles.
    p0 = (0.05, 0.13, 0.00)
    p1 = (-0.05, 0.13, 0.00)
    p2 = (-0.05, 0.13, -0.16)
    p3 = (0.05, 0.13, -0.16)

    if t%5 == 0:
        return p0
    if t%5 == 1:
        return p1
    if t%5 ==2:
        return p1
    if t%5 ==3:
        return p2
    if t%5 ==4:
        return p3
