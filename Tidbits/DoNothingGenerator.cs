using Microsoft.CodeAnalysis;

namespace Generator
{
    /// <summary>
    /// A generator that does nothing.
    /// </summary>
    [Generator]
    public class DoNothingGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
