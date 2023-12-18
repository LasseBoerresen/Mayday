using System;
using mayday.mayday;

namespace Test.Structure;

public class FakeMaydayStructure(MaydayPosture posture) : mayday.MaydayStructure
{
    public override MaydayPosture Posture
    {
        get => posture;
        set
        {
            posture = value;
            Console.WriteLine(posture);
        }
    }
}
