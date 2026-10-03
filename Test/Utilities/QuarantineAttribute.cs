using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.Utilities;

/// <summary>
/// Marks a test, or every test in a class, as known to fail for a documented <paramref name="reason"/>.
/// </summary>
/// <remarks>
/// Quarantined tests carry the trait <c>Quarantine=true</c>. CI excludes them from the blocking run
/// (<c>--filter "Quarantine!=true"</c>) and runs them separately without blocking, so a quarantined test that
/// starts passing is noticed. Quarantine records a failure, it never changes what a test expects: fix the test or
/// the code, then remove the attribute.
/// </remarks>
[TraitDiscoverer("Test.Utilities.QuarantineTraitDiscoverer", "Test")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class QuarantineAttribute(string reason) : Attribute, ITraitAttribute
{
    public string Reason { get; } = reason;
}

/// <summary>Turns <see cref="QuarantineAttribute"/> into the filterable traits xUnit reports.</summary>
public sealed class QuarantineTraitDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        yield return new("Quarantine", "true");
        yield return new("QuarantineReason", (string)traitAttribute.GetConstructorArguments().First());
    }
}

/// <summary>Reasons shared by several quarantined tests, so one root cause reads the same everywhere.</summary>
public static class QuarantineReasons
{
    public const string EmptyLegPostureMap =
        "Test object mother builds an empty LegPostureByPositionMap, which LegPostureByPositionMapDictImpl rejects "
        + "with EmptyMapException.";
}
