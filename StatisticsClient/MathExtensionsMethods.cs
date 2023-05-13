namespace ClientApplication;

public static class MathExtensionsMethods
{
    public static double StandardDeviation(this IEnumerable<int> numbers, double average) => 
        Math.Sqrt(numbers.Select(x => (x - average) * (x - average)).Average());
    
    public static int Mode(this IEnumerable<int> numbers) =>
        numbers.GroupBy(x => x).Max(g => g.Count());
    
    public static int Median(this IEnumerable<int> numbers)
    {
        var enumerable = numbers as int[] ?? numbers.ToArray();
        return enumerable.OrderBy(x => x).Skip((enumerable.Length - 1) / 2).First();
    }
}