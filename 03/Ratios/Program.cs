
using System.Diagnostics;

using Shared;

var inputData = DataLoaderSingle.LoadInputData<GameBoard>(TransformInput);

foreach (var inputSet in inputData)
{
    if (inputSet.Content == default)
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

static int Part1(SinglePuzzleInput<GameBoard> input)
{
    var board = input.Content;

    var validNumbers = board.GameNumbers
        .Where(x => IsValidNumber(board, x))
        .ToArray();

    /*var invalidNumbers = board.GameNumbers
        .Where(x => !IsValidNumber(board, x))
        .ToArray();*/

    return validNumbers
        .Select(x => x.Value(board))
        .Sum();
}

static int Part2(SinglePuzzleInput<GameBoard> input)
{
    var board = input.Content;

    var validNumbers = board.GameNumbers
        .Where(x => IsValidNumber(board, x));

    var gears = validNumbers
        .Select(x => new { number = x, neighbor = board.GetNeighbor(x) })
        .Where(pair => pair.neighbor?.Value(board) == Constants.Gear);

    var specialGears = gears
        .GroupBy(x => x.neighbor)
        .Where(g => g.Count() >= 2);

    var ratios = specialGears
        .Select(group => group.Select(x => x.number))
        .Select(number => GetRatio(board, number))
        .ToArray();

    return ratios.Sum();
}

static int GetRatio(GameBoard board, IEnumerable<GameNumber> numbers)
{
    return numbers
        .Select(x => x.Value(board))
        .Aggregate((previous, next) => previous * next);
}

static bool IsValidNumber(GameBoard board, GameNumber gameNumber)
{
    return board.GetNeighbor(gameNumber) is not null;
}

static GameBoard TransformInput(string[] lines)
{
    var inputMatrix = TransposeInput(lines);
    var gameNumbers = ExtractNumbers(inputMatrix);

    return new GameBoard()
    {
        InputMatrix = inputMatrix,
        GameNumbers = gameNumbers
    };
}

static GameNumber[] ExtractNumbers(char[][] inputMatrix)
{
    var width = inputMatrix.Length;
    var height = inputMatrix[0].Length;

    var gameNumbers = new List<GameNumber>();

    for (int rowIndex = 0; rowIndex < height; rowIndex++)
    {
        var rowSpan = inputMatrix[rowIndex].AsSpan();
        int? columnDigitStartIndex = null;
        for (int colIndex = 0; colIndex <= width; colIndex++)
        {
            if (colIndex == width || !char.IsAsciiDigit(rowSpan[colIndex]))
            {
                if (columnDigitStartIndex is not null)
                {
                    // we reached the end of a number
                    gameNumbers.Add(new GameNumber()
                    {
                        RowIndex = rowIndex,
                        ColumnStartIndex = columnDigitStartIndex.Value,
                        ColumnEndIndex = colIndex - 1
                    });

                    columnDigitStartIndex = null;
                }

                continue;
            }
            else
            {
                columnDigitStartIndex ??= colIndex;
            }
        }
    }

    return gameNumbers.ToArray();
}

static char[][] TransposeInput(string[] lines)
{
    var lineWidth = lines.First().Length;
    var height = lines.Length;

    var inputMatrix = new char[height][];

    for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
    {
        inputMatrix[lineIndex] = lines[lineIndex].ToCharArray();
    }
    return inputMatrix;
}

static class Constants
{
    internal const char PlaceHolder = '.';
    internal const char Gear = '*';
    internal const string ValidTokens = "0123456789.";
}

public readonly record struct GameBoard
{
    public required char[][] InputMatrix { get; init; }
    public GameNumber[] GameNumbers { get; init; }

    public readonly Span<char> SpanOf<T>(ISymbol<T> symbol)
    {
        return InputMatrix[symbol.RowIndex].AsSpan()[symbol.ColumnStartIndex..(symbol.ColumnEndIndex + 1)];
    }

    public readonly GameSymbol? GetNeighbor<T>(ISymbol<T> symbol)
    {
        var matrix = InputMatrix;

        var left = Math.Max(symbol.ColumnStartIndex - 1, 0);
        var right = Math.Min(symbol.ColumnEndIndex + 1, matrix[0].Length - 1) + 1; //exclusive count

        var topSpan = symbol.RowIndex > 0 ?
            matrix[symbol.RowIndex - 1].AsSpan()[left..right] : [];

        var botSpan = symbol.RowIndex < matrix.Length - 1 ?
            matrix[symbol.RowIndex + 1].AsSpan()[left..right] : [];

        var isLeft = symbol.ColumnStartIndex - 1 >= 0 && matrix[symbol.RowIndex][left] != Constants.PlaceHolder;
        var isRight = symbol.ColumnEndIndex + 1 < matrix[0].Length && matrix[symbol.RowIndex][right - 1] != Constants.PlaceHolder;

        var topSearch = topSpan.IndexOfAnyExcept(Constants.PlaceHolder);
        if (topSearch != -1)
            return new GameSymbol()
            {
                // relative index might shift if we don't align with left / right border
                ColumnStartIndex = symbol.ColumnStartIndex + topSearch + (symbol.ColumnStartIndex > 0 ? -1 : 0),
                RowIndex = symbol.RowIndex - 1
            };

        var botSearch = botSpan.IndexOfAnyExcept(Constants.PlaceHolder);
        if (botSearch != -1)
            return new GameSymbol()
            {
                // relative index might shift if we don't align with left / right border
                ColumnStartIndex = symbol.ColumnStartIndex + botSearch + (symbol.ColumnStartIndex > 0 ? -1 : 0),
                RowIndex = symbol.RowIndex + 1
            };

        if (isLeft)
            return new GameSymbol()
            {
                ColumnStartIndex = left,
                RowIndex = symbol.RowIndex
            };

        if (isRight)
            return new GameSymbol()
            {
                ColumnStartIndex = right - 1,
                RowIndex = symbol.RowIndex
            };

        return null;
    }
}

public interface ISymbol<T>
{
    int RowIndex { get; }
    int ColumnStartIndex { get; }
    int ColumnEndIndex { get; }
    T Value(GameBoard board);
}

[DebuggerDisplay("({RowIndex}:{ColumnStartIndex})")]
public readonly record struct GameSymbol : ISymbol<char>
{
    public required int RowIndex { get; init; }
    public required int ColumnStartIndex { get; init; }
    public int ColumnEndIndex { get => ColumnStartIndex; }

    public char Value(GameBoard board)
    {
        return board.SpanOf(this)[0];
    }
}

[DebuggerDisplay("({RowIndex}:{ColumnStartIndex}:{ColumnEndIndex})")]
public readonly record struct GameNumber : ISymbol<int>
{
    public required int RowIndex { get; init; }
    public required int ColumnStartIndex { get; init; }
    public required int ColumnEndIndex { get; init; }

    public readonly int Value(GameBoard board)
    {
        return int.Parse(board.SpanOf(this));
    }
}

