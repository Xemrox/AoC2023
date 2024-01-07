using System;
using System.Buffers;
using System.Diagnostics;

using Shared;

namespace SeedFertilizer;

internal class Program
{
    private static void Main(string[] args)
    {
        var inputData = DataLoaderSingle.LoadInputData<GameAlmanac>(TransformInput);

        foreach (var inputSet in inputData)
        {
            if (inputSet.Content == default || inputSet.Name == "xemrox")
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

        static ulong Part1(SinglePuzzleInput<GameAlmanac> input)
        {
            var almanac = input.Content;

            var test = almanac.SeedToSoil.Lookup(almanac.SeedNumbers[0]);
            var test2 = almanac.SeedToSoil.ReverseLookup(test);

            var locations = almanac.SeedNumbers
                .Select(seed => LookupAll(almanac, seed))
                .ToArray();

            return locations.Min();
        }

        static ulong LookupAll(GameAlmanac gameAlmanac, ulong seed)
        {
            var currentValue = seed;

            foreach (var map in gameAlmanac.AllMaps)
            {
                currentValue = map.Lookup(currentValue);
            }

            return currentValue;
        }

        static int Part2(SinglePuzzleInput<GameAlmanac> input)
        {
            var almanac = input.Content;

            //var biggestLocation = almanac.HumidityToLocation.Ranges.Select(x => x.Destination + x.Range).Max();
            //var smallestLocation = almanac.HumidityToLocation.Ranges.Min(x => x.Destination);

            var orderedLocations = almanac.HumidityToLocation.Ranges.OrderBy(x => x.Destination);
            foreach (var range in orderedLocations)
            {

                //var start = range.Destination;
                //var end = range.Destination + range.Range;
                foreach (var stepper in almanac.AllMaps.Reverse().Skip(1))
                {
                    var overlaps = new List<GameRange>();

                    foreach (var reverseRange in stepper.Ranges)
                    {
                        var combinedRange = reverseRange.ReverseRange(range);
                        if (combinedRange != default)
                        {
                            overlaps.Add(combinedRange);
                        }
                    }

                    /*var candidates = new List<GameMapRange>();
                    {
                        if (reverseRange.InReverseRange())
                            candidates.Add(reverseRange);
                    }*/
                }
            }


            /*for (ulong currentLocation = smallestLocation; currentLocation < biggestLocation; currentLocation++)
            {
                
            }*/
            return 0;
        }
    }

    private static GameAlmanac TransformInput(string[] lines)
    {
        var seeds = lines[0]
            .Split(':')[1]
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(num => ulong.Parse(num, System.Globalization.NumberStyles.Integer))
            .ToArray();

        var strMaps = lines.AsSpan()[1..];

        var maps = new GameMap[GameAlmanac.ExpectedMaps];

        for (var mapIndex = 0; mapIndex < GameAlmanac.ExpectedMaps; mapIndex++)
        {
            var skipTo = strMaps.IndexOfAnyExcept(string.Empty);
            var spanEnd = strMaps[skipTo..].IndexOf(string.Empty);
            spanEnd = spanEnd == -1 ? strMaps.Length : skipTo + spanEnd;

            var mapLines = strMaps[skipTo..spanEnd];

            maps[mapIndex] = ReadGameMap(mapLines);

            strMaps = strMaps[spanEnd..];
        }

        return new GameAlmanac()
        {
            SeedNumbers = seeds,
            AllMaps = maps
        };
    }

    private static GameMap ReadGameMap(ReadOnlySpan<string> lines)
    {
        // skip header
        var header = lines[0];
        lines = lines[1..];
        // read 3 pairs
        var gameMapRanges = new GameMapRange[lines.Length];
        for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
        {
            var segements = lines[lineNumber]
                .Split(' ')
                .Select(uint.Parse)
                .ToArray();

            gameMapRanges[lineNumber] = new GameMapRange()
            {
                Destination = segements[0],
                Source = segements[1],
                Range = segements[2],
            };
        }

        return new GameMap()
        {
            Name = header,
            Ranges = gameMapRanges
        };
    }
}

public readonly record struct GameRange
{
    public required ulong Start { get; init; }
    public required ulong Range { get; init; }
    public ulong End { get => Start + Range-1; }

    public readonly bool InRange(ulong input)
    {
        return input >= Start && input <= End;
    }
}

public readonly record struct GameMapRange
{
    public required ulong Destination { get; init; }
    public ulong DestinationEnd { get => Destination + Range-1; }
    public required ulong Source { get; init; }
    public ulong SourceEnd { get => Source + Range -1; }
    public required ulong Range { get; init; }

    public readonly bool InRange(ulong input)
    {
        return input >= Source && input <= SourceEnd;
    }

    public readonly ulong Lookup(ulong input)
    {
        if (!InRange(input))
            return input;

        return input - Source + Destination;
    }

    public readonly bool InReverseRange(ulong output)
    {
        return output >= Destination && output <= DestinationEnd;
    }

    public readonly bool HasReverseOverlap(GameMapRange range)
    {
        // start within range
        return (range.Destination >= Source && range.Destination <= SourceEnd) ||
            // end within range
            (range.DestinationEnd >= Source && range.DestinationEnd <= SourceEnd) ||
            // full overlap
            (range.Destination <= Source && range.DestinationEnd >= SourceEnd);
            /*// only overlap on start
            (range.Destination <= Source && range.DestinationEnd >= Source) ||
            // only overlap on end
            (range.Destination <= SourceEnd && range.DestinationEnd >= SourceEnd);*/
    }

    public readonly GameRange ReverseRange(GameMapRange range)
    {
        if (!HasReverseOverlap(range))
            return default;
        var start = Math.Max(Math.Min(Source, range.Destination + range.Range), range.Destination);
        var end = Math.Min(range.Destination + range.Range, Source + Range);

        return new GameRange()
        {
            Start = start,
            Range = end - start,
        };
    }

    public readonly ulong ReverseLookup(ulong output)
    {
        if (!InReverseRange(output))
            return output;

        return output - Destination + Source;
    }
}

public readonly record struct GameMap
{
    public required string Name { get; init; }
    public required GameMapRange[] Ranges { get; init; }

    public readonly ulong Lookup(ulong input)
    {
        foreach (var range in Ranges)
        {
            if (range.InRange(input))
                return range.Lookup(input);
        }
        return input;
    }

    public readonly ulong ReverseLookup(ulong output)
    {
        foreach (var range in Ranges)
        {
            if (range.InReverseRange(output))
                return range.ReverseLookup(output);
        }
        return output;
    }
}

public readonly record struct GameAlmanac
{
    public static int ExpectedMaps { get; } = 7;

    public readonly ulong[] SeedNumbers { get; init; }
    public readonly GameRange[] SeedRanges
    {
        get
        {
            var ranges = new GameRange[SeedNumbers.Length / 2];
            for (var seedIndex = 0; seedIndex < SeedNumbers.Length; seedIndex += 2)
            {
                ranges[seedIndex] = new GameRange()
                {
                    Start = SeedNumbers[seedIndex],
                    Range = SeedNumbers[seedIndex + 1]
                };
            }
            return ranges;
        }
    }
    public readonly GameMap SeedToSoil { get => AllMaps[0]; }
    public readonly GameMap SoilToFertilizer { get => AllMaps[1]; }
    public readonly GameMap FertilizerToWater { get => AllMaps[2]; }
    public readonly GameMap WaterToLight { get => AllMaps[3]; }
    public readonly GameMap LightToTemperature { get => AllMaps[4]; }
    public readonly GameMap TemperatureToHumidity { get => AllMaps[5]; }
    public readonly GameMap HumidityToLocation { get => AllMaps[6]; }
    private readonly GameMap[] _allMaps;
    public required GameMap[] AllMaps
    {
        get => _allMaps; init
        {
            if (value.Length < ExpectedMaps)
                throw new ArgumentOutOfRangeException(nameof(value));
            _allMaps = value;
        }
    }
}