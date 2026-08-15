using System.Runtime.CompilerServices;

// QuarterExtensions is internal — the quarter arithmetic that decides which initiative wins.
// Exposed to the test project rather than made public, so the characterisation tests in
// tests/MR.Tests can cover it without widening the production surface.
[assembly: InternalsVisibleTo("MR.Tests")]
