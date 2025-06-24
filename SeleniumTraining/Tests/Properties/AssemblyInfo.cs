// Run test fixtures (classes) in parallel with each other.
// The number of parallel workers is in the SeleniumTraining.csproj file.
[assembly: Parallelizable(ParallelScope.Fixtures)]
