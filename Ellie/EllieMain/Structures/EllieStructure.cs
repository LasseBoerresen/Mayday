using UnitsNet;

namespace EllieMain.Structures;

public interface EllieStructure
{
    /// <summary>
    /// Start physical devices. 
    /// </summary>
    void Start(CancellationToken ct);

    void DriveAllWheelsAt<TNew>(Speed speed);

    State GetState();
}
