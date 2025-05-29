// Run test fixtures (classes) in parallel with each other
[assembly: Parallelizable(ParallelScope.Fixtures)]
// Set the default number of worker threads NUnit should use.
[assembly: LevelOfParallelism(6)]
