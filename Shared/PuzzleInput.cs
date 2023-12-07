
namespace Shared;

public record class PuzzleInput<T>
{
    public string Name { get; init; }
    public T[] Content { get; init; }

    public PuzzleInput(string name, IEnumerable<T> content)
    {
        Name = name;
        Content = content.ToArray();
    }

}
