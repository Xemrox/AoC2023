
namespace Shared;

public abstract record class AbstractPuzzleInput
{
    public string Name { get; init; }

    protected AbstractPuzzleInput(string name)
    {
        Name = name;
    }
}

public record class PuzzleInput<T> : AbstractPuzzleInput
{
    public T[] Content { get; init; }

    public PuzzleInput(string name, IEnumerable<T> content) : base(name)
    {
        Content = content.ToArray();
    }
}

public record class SinglePuzzleInput<T> : AbstractPuzzleInput
{
    public T Content { get; init; }

    public SinglePuzzleInput(string name, T content) : base(name)
    {
        Content = content;
    }
}
