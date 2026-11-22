using Xunit;

namespace Test.Utilities;

/// <summary>
/// The xUnit collection of tests that measure timing on the real clock. It runs alone, after every parallel
/// collection has finished, so other tests do not compete for the CPU while it measures.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class RealClockCollection
{
    /// <summary>The collection's name, for <c>[Collection(RealClockCollection.Name)]</c>.</summary>
    public const string Name = "Real clock";
}
