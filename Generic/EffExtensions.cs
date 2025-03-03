using LanguageExt;

namespace Generic;

public static class EffUnitExtensions
{
    public static readonly Eff<Unit> UnitEff = Eff<Unit>.Pure(Unit.Default);
    
}
