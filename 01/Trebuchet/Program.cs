
using System.Linq;

using Shared;

var inputData = DataLoader.LoadInputData();

foreach (var inputSet in inputData)
{
    if (inputSet.Content.Length == 0)
    {
        continue;
    }
    Console.WriteLine($"> Part-1 for {inputSet.Name}");

    var value = Part1(inputSet);
    Console.WriteLine("Value: {0}", value);

    Console.WriteLine($"< Part-1 for {inputSet.Name}");


    Console.WriteLine($"> Part-2 for {inputSet.Name}");

    var value2 = Part2(inputSet);
    Console.WriteLine("Value: {0}", value2);

    Console.WriteLine($"< Part-2 for {inputSet.Name}");
}

static int Part1(PuzzleInput<string> input)
{
    return input.Content
        .Select(x => x.Where(char.IsAsciiDigit))
        .Select(x => new { first = x.First() - '0', last = x.Last() - '0' })
        .Select(x => x.first * 10 + x.last)
        .Sum();
}

static int Part2(PuzzleInput<string> input)
{
    return input.Content
        .Select(x => x
            .Replace("one", "one1one")
            .Replace("two", "two2two")
            .Replace("three", "three3three")
            .Replace("four", "four4four")
            .Replace("five", "five5five")
            .Replace("six", "six6six")
            .Replace("seven", "seven7seven")
            .Replace("eight", "eight8eight")
            .Replace("nine", "nine9nine"))
        .Select(x => x.Where(char.IsAsciiDigit).ToArray())
        .Select(x => new { first = x.First() - '0', last = x.Last() - '0' })
        .Select(x => x.first * 10 + x.last)
        .Sum();
}

