using Shared;

var inputData = DataLoader.LoadInputData(ReadInputLine);

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

static GameInput ReadInputLine(string input)
{
    var gameSplit = input.Split(':');
    var gameNum = int.Parse(gameSplit[0].Split(' ').Last());

    var gameSets = gameSplit[1].Split(';');

    var sets = new List<GameSet>();
    foreach (var gameSet in gameSets)
    {
        var groups = gameSet.Trim().Split(',');

        int redGroup = 0, blueGroup = 0, greenGroup = 0;
        foreach (var group in groups)
        {
            var groupSplit = group.Trim().Split(' ');
            var num = int.Parse(groupSplit.First());
            var groupName = groupSplit.Last();

            switch (groupName)
            {
                case "red":
                    redGroup += num;
                    break;
                case "blue":
                    blueGroup += num;
                    break;
                case "green":
                    greenGroup += num;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        sets.Add(new GameSet()
        {
            Blue = blueGroup,
            Green = greenGroup,
            Red = redGroup
        });
    }

    return new GameInput()
    {
        Id = gameNum,
        Sets = [.. sets]
    };
}

static int Part1(PuzzleInput<GameInput> input)
{
    return input.Content
        .Where(ValidateGame)
        .Sum(x => x.Id);
}

static bool ValidateGame(GameInput gameInput)
{
    return gameInput.Sets.All(ValidateSet);
}

static bool ValidateSet(GameSet gameSet)
{
    return gameSet.Red <= 12 && gameSet.Green <= 13 && gameSet.Blue <= 14;
}

static int Part2(PuzzleInput<GameInput> input)
{
    return input.Content
        // find minimum vaiable cube count
        .Select(x => new
        {
            MinRed = x.Sets.Max(y => y.Red),
            MinGreen = x.Sets.Max(y => y.Green),
            MinBlue = x.Sets.Max(y => y.Blue),
        })
        // calculate power
        .Select(x => x.MinRed * x.MinGreen * x.MinBlue)
        .Sum();
    // is the same as this
    //return input.Content.Sum(x => x.Power);
}

public readonly record struct GameInput
{
    public required int Id { get; init; }
    public required GameSet[] Sets { get; init; }

    public int MinRed { get => Sets.Max(x => x.Red); }
    public int MinGreen { get => Sets.Max(x => x.Green); }
    public int MinBlue { get => Sets.Max(x => x.Blue); }
    public int Power { get => MinRed * MinGreen * MinBlue; }
}

public readonly record struct GameSet
{
    public required int Red { get; init; }
    public required int Green { get; init; }
    public required int Blue { get; init; }
}
