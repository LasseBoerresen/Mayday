using System;
using mayday.mayday;

namespace mayday;

public class MaydayStructure
{
    private MaydayPosture _posture;

    public virtual MaydayPosture Posture
    {
        get => _posture;
        set
        {
            _posture = value;
            Console.WriteLine(_posture);
        }
    }
};
