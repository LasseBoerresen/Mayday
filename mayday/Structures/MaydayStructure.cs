using System;
using System.Collections;
using mayday.mayday;
using mayday.Structures;
using UnitsNet;

namespace mayday;

public class MaydayStructure(
    IEnumerable<Joint> joints,
    IEnumerable<Attachment> attachments,
    IEnumerable<Link> links) : Structure(joints, attachments, links)
{
    public static MaydayStructure Create()
    {
        Joint[] joints = { };
        Attachment[] attachments = { };
        Link[] links = { };

        return new(joints, attachments, links);
    }

    private MaydayPosture _posture = MaydayPosture.Neutral;

    public MaydayPosture Posture
    {
        get => _posture;
        set
        {
            _posture = value;
            Console.WriteLine(_posture);
        }
    }
};
