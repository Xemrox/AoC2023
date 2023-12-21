using System.Buffers;

using Shared;

namespace Scratchcards;

internal class Program
{
    private static void Main(string[] args)
    {
        var inputData = DataLoader.LoadInputData(TransformInput);

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
    }

    static long Part1(PuzzleInput<GameCard> input)
    {
        return input.Content.Sum(x => x.Score);
    }

    static int Part2(PuzzleInput<GameCard> input)
    {
        var cards = input.Content;
        var instances = new int[cards.Length];

        for (var i = 0; i < cards.Length; i++)
            instances[i] = 1; // each card exists at least once

        for (int cardIndex = 0; cardIndex < cards.Length; cardIndex++)
        {
            var card = cards[cardIndex];
            var winnerCount = card.ScoreNumbers.Count(); // count created copies

            for (int incrementIndex = cardIndex + 1; incrementIndex < cardIndex + winnerCount + 1; incrementIndex++)
            {
                if (incrementIndex >= cards.Length)
                    break; // reached end of array

                // increment by amount of current cards (including all previous obtained copies!)
                instances[incrementIndex] += instances[cardIndex];
            }
        }

        return instances.Sum();
    }

    static GameCard TransformInput(string strLine)
    {
        /*var fragments = line.Split(':', StringSplitOptions.TrimEntries);
        var header = fragments[0];
        var data = fragments[1].Split('|', StringSplitOptions.TrimEntries);
        var winningSection = data[0].Split(' ', StringSplitOptions.TrimEntries);
        var scratchSection = data[1].Split(' ', StringSplitOptions.TrimEntries);*/

        var line = strLine.AsSpan();

        var nameSeperator = line.IndexOf(':');
        var dataSeperator = line[nameSeperator..].IndexOf("|") + nameSeperator;

        // trim seperator +1
        var winningNumbersSpan = line[(nameSeperator + 1)..(dataSeperator)];
        var scratchNumbersSpan = line[(dataSeperator + 1)..];

        var cardNumber = int.Parse(line[4..nameSeperator]);
        var winningNumbers = ReadNumbersSorted(winningNumbersSpan);
        var scratchNumbers = ReadNumbersSorted(scratchNumbersSpan);

        return new GameCard()
        {
            CardNumber = cardNumber,
            WinningNumbers = winningNumbers,
            ScratchNumbers = scratchNumbers,
        };
    }

    static readonly SearchValues<char> Numbers = SearchValues.Create("0123456789");
    static IReadOnlySet<int> ReadNumbersSorted(ReadOnlySpan<char> span)
    {
        var set = new SortedSet<int>();

        while (!span.IsEmpty)
        {
            var skipTo = span.IndexOfAny(Numbers);
            if (skipTo == -1) // trailing whitespaces
                break;

            var splitPos = span[skipTo..].IndexOf(' ');
            // no whitespace to split -> last number
            var splitTo = splitPos >= 0 ? (splitPos + skipTo) : span.Length;

            set.Add(int.Parse(span[skipTo..splitTo]));

            span = span[splitTo..];
        }

        return set;
    }
}

public readonly record struct GameCard
{
    public required int CardNumber { get; init; }
    public required IReadOnlySet<int> WinningNumbers { get; init; }
    public required IReadOnlySet<int> ScratchNumbers { get; init; }

    public readonly IEnumerable<int> ScoreNumbers { get => WinningNumbers.Intersect(ScratchNumbers); }

    public readonly long Score
    {
        get
        {
            var wonNumbers = ScoreNumbers.Count() - 1;
            return wonNumbers < 0 ? 0 : 1 << wonNumbers;
        }
    }
}
