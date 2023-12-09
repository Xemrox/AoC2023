
namespace Shared;

public static class DataLoaderSingle
{
    public static List<SinglePuzzleInput<T>> LoadInputData<T>(Func<string[], T> contentModificationFunction)
    {
        return DataLoader.ForInputFiles()
            .Select(fileInfo => new SinglePuzzleInput<T>(
                DataLoader.GetInputName(fileInfo),
                contentModificationFunction(File.ReadAllLines(fileInfo.FullName))
            ))
            .ToList();
    }
}
