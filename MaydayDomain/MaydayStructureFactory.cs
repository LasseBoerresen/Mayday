using Generic;
using MaydayDomain.Components;
using RobotDomain.Structures;

namespace MaydayDomain;

/// <summary>
/// Factory class for assembling a complex MaydayStructure
/// </summary>
public class MaydayStructureFactory(MaydayLegFactory LegFactory)
{
    public DefaultMaydayStructure CreateDefault()
    {
        var thorax = Link.CreateThorax;
        var legs = LegFactory.CreateAll();

        legs.ForEach(kvp => 
            Attachment.NewBetween(thorax, kvp.Value.BaseLink, Thorax.TransformFor(kvp.Key)));

        return new(thorax, legs);
    }

    public static DefaultMaydayStructure CreateEcho()
    {
        return new MaydayStructureFactory(MaydayLegFactory.NewEchoLegFactory()).CreateDefault();
    }
}
