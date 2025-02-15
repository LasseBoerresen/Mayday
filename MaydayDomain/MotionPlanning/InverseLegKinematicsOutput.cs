using UnitsNet;
using static System.Math;

namespace MaydayDomain.MotionPlanning;

public class InverseLegKinematicsOutput
{
    public InverseLegKinematicsOutput() {}

    public InverseLegKinematicsOutput(
        float coxaSin, float coxaCos, float femurSin, float femurCos, float tibiaSin, float tibiaCos) : this()
    {
        CoxaSin = coxaSin;
        CoxaCos = coxaCos;
        FemurSin = femurSin;
        FemurCos = femurCos;
        TibiaSin = tibiaSin;
        TibiaCos = tibiaCos;
    }

    public float CoxaSin { get; init; } 
    public float CoxaCos { get; init; } 
    public float FemurSin { get; init; } 
    public float FemurCos { get; init; } 
    public float TibiaSin { get; init; }
    public float TibiaCos { get; init; }
    
    public static int Length => OutputColumnNames.Length;
    
    public static InverseLegKinematicsOutput Create(MaydayLegPosture p)
    {
        return new(
            coxaSin: (float)Sin(p.CoxaAngle.Radians),
            coxaCos: (float)Cos(p.CoxaAngle.Radians),
            femurSin: (float)Sin(p.FemurAngle.Radians),
            femurCos: (float)Cos(p.FemurAngle.Radians),
            tibiaSin: (float)Sin(p.TibiaAngle.Radians),
            tibiaCos: (float)Cos(p.TibiaAngle.Radians)
        );
    }

    public MaydayLegPosture ToPosture()
     {
         return MaydayLegPosture.FromSines(
            CoxaSin,
            CoxaCos,
            FemurSin,
            FemurCos,
            TibiaSin,
            TibiaCos);
     }
     
     public static string[] OutputColumnNames =
     [
         nameof(CoxaSin),
         nameof(CoxaCos),
         nameof(FemurSin),
         nameof(FemurCos),
         nameof(TibiaSin),
         nameof(TibiaCos)
     ];

     public static InverseLegKinematicsOutput FromArray(float[] output)
     {
         return new(output[0], output[1], output[2], output[3], output[4], output[5]);
     }
};