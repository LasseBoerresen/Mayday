using RobotDomain.Geometry;
using Tensorflow;
using Tensorflow.NumPy;
using static System.Math;

namespace MaydayDomain.MotionPlanning;

public record InverseLegKinematicsInput(
    float EndX, float EndY, float EndZ,
    float StartX, float StartY, float StartZ,
    float StartCoxaSin, float StartFemurSin, float StartTibiaSin,
    float StartCoxaCos, float StartFemurCos, float StartTibiaCos)
{
    public static Shape Shape => new(InputColumnNames.Length); 
    public Xyz EndXyz => new(EndX, EndY, EndZ);
    
    /// <summary>
    /// must have first dimension = 1, representing a batch size of 1
    /// </summary>
    public NDArray ToNdArray
    {
        get
        {
            return np.array(new[,]
            {{
                EndX,
                EndY,
                EndZ,
                StartX,
                StartY,
                StartZ,
                StartCoxaSin,
                StartFemurSin,
                StartTibiaSin,
                StartCoxaCos,
                StartFemurCos,
                StartTibiaCos}});
        }
    }

    public static string[] InputColumnNames
    {
        get
        {
            return
            [
                nameof(EndX),
                nameof(EndY),
                nameof(EndZ),
                nameof(StartX),
                nameof(StartY),
                nameof(StartZ),
                nameof(StartCoxaSin),
                nameof(StartFemurSin),
                nameof(StartTibiaSin),
                nameof(StartCoxaCos),
                nameof(StartFemurCos),
                nameof(StartTibiaCos)
            ];
        }
    }

    public static InverseLegKinematicsInput Create(
        Xyz endXyz,
        Xyz startXyz,
        MaydayLegPosture startPosture)
    {
        return new(
            (float)endXyz.X.Meters,
            (float)endXyz.Y.Meters,
            (float)endXyz.Z.Meters,
            (float)startXyz.X.Meters,
            (float)startXyz.Y.Meters,
            (float)startXyz.Z.Meters,
            (float)Sin(startPosture.CoxaAngle.Radians),
            (float)Sin(startPosture.FemurAngle.Radians),
            (float)Sin(startPosture.TibiaAngle.Radians),
            (float)Cos(startPosture.CoxaAngle.Radians),
            (float)Cos(startPosture.FemurAngle.Radians),
            (float)Cos(startPosture.TibiaAngle.Radians));
    }
};
