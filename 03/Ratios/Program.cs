using System.Diagnostics.CodeAnalysis;

using Shared;

const char PlaceHolder = '.';
const string ValidTokens = "0123456789.";

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
    return 0;
}

static bool IsValidNumber(GameBoard board, GameNumber gN)
{
    var matrix = board.InputMatrix;

    var left = Math.Max(gN.ColumnStartIndex - 1, 0);
    var right = Math.Min(gN.ColumnEndIndex + 1, matrix[0].Length - 1) + 1; //exclusive count

    var topSpan = gN.RowIndex > 0 ?
        matrix[gN.RowIndex - 1].AsSpan()[left..right] : [];

    var botSpan = gN.RowIndex < matrix.Length - 1 ?
        matrix[gN.RowIndex + 1].AsSpan()[left..right] : [];

    //var midSpan = matrix[gameNumber.RowIndex].AsSpan()[left..right];
    var isLeft = gN.ColumnStartIndex - 1 >= 0 && matrix[gN.RowIndex][left] != PlaceHolder;
    var isRight = gN.ColumnEndIndex + 1 < matrix[0].Length && matrix[gN.RowIndex][right - 1] != PlaceHolder;

    //var tokens = "0123456789.".AsSpan();
    //var tokens = ValidTokens.AsSpan();

    return topSpan.ContainsAnyExcept(PlaceHolder) ||
        isLeft || isRight ||
        botSpan.ContainsAnyExcept(PlaceHolder);
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

public readonly record struct GameBoard
{
    public required char[][] InputMatrix { get; init; }
    public GameNumber[] GameNumbers { get; init; }
}

public readonly record struct GameNumber
{
    public required int RowIndex { get; init; }
    public required int ColumnStartIndex { get; init; }
    public required int ColumnEndIndex { get; init; }

    public Span<char> Digits(GameBoard board)
    {
        return board.InputMatrix[RowIndex].AsSpan()[ColumnStartIndex..(ColumnEndIndex + 1)];
    }

    public int Value(GameBoard board)
    {
        return int.Parse(Digits(board));
    }
}

