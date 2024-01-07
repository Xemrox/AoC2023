using Shared;

var inputDataA = DataLoader.LoadAllRawInputData<BoatRace>(TransformInputA);
var inputDataB = DataLoader.LoadAllRawInputData<BoatRace>(TransformInputB);

for (int i = 0; i < inputDataA.Count; i++)
{
    var inputSetA = inputDataA[i];
    var inputSetB = inputDataB[i];

    if (inputSetA.Content.Length == 0 || inputSetB.Content.Length == 0)
    {
        continue;
    }
    Console.WriteLine($"> Part-1 for {inputSetA.Name}");

    var valueA = Solve(inputSetA);
    Console.WriteLine($"Value: {valueA}");

    Console.WriteLine($"< Part-1 for {inputSetA.Name}");

    Console.WriteLine($"> Part-2 for {inputSetB.Name}");

    var valueB = Solve(inputSetB);
    Console.WriteLine($"Value: {valueB}");

    Console.WriteLine($"< Part-2 for {inputSetB.Name}");
}

static IEnumerable<BoatRace> TransformInputA(string input)
{
    var splitOpts = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
    var lines = input.Split("\r\n", splitOpts);
    var times = lines[0].Split(' ', splitOpts);
    var distances = lines[1].Split(' ', splitOpts);

    if (times.Length != distances.Length)
        throw new InvalidDataException();

    var result = new List<BoatRace>();

    for (int i = 1; i < times.Length; i++)
    {
        result.Add(new BoatRace()
        {
            Time = int.Parse(times[i]),
            RecordDistance = int.Parse(distances[i])
        });
    }

    return result;
}

IEnumerable<BoatRace> TransformInputB(string input)
{
    var splitOpts = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
    var lines = input.Split("\r\n", splitOpts);

    var timeFragments = lines[0]
        .Split(':', splitOpts)[1] // split on header
        .Split(' ', splitOpts); // cut all whitespaces
    var time = string.Join("", timeFragments); // glue to one number
    var distanceFragments = lines[1]
        .Split(':', splitOpts)[1]
        .Split(' ', splitOpts); // cut all whitespaces;
    var distance = string.Join("", distanceFragments); // glue to one number

    return [
        new BoatRace()
        {
            Time = int.Parse(time),
            RecordDistance = double.Parse(distance)
        }
    ];
}

static int Solve(PuzzleInput<BoatRace> input)
{
    return input.Content
        .Select(x => x.WinningCandidates)
        .Aggregate((current, next) => current * next);
}

readonly record struct BoatRace
{
    public required double Time { get; init; }
    public required double RecordDistance { get; init; }
    // solve first index of winning time -> quadratic equation -x^2+Tx-D=0 from (Di = (T - Ti)*Ti)
    // a = -1 ; b = T ; c = -D; ax^2+bx+c=0
    public double MinimumTime { get => (Time - Math.Sqrt(Time * Time - 4 * RecordDistance)) / 2.0; }
    public int WinningCandidates { get => (int)(Time - Math.Floor(MinimumTime) * 2 - 1); }
}
