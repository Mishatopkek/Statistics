using StatisticsClient.Math;

namespace StatisticsClient;

public class MathHandler
{
    private readonly AverageMath _average = new();
    private readonly StandardDeviationMath _standardDeviation = new();
    private readonly ModeMath _mode = new();
    private readonly MedianMath _median = new();
    
    public PacketsLossMath PacketsLoss { get; }= new();

    public void AddAll(double number)
    {
        _average.Add(number);
        _standardDeviation.Add(number);
        _mode.Add(number);
        _median.Add(number);
    }

    public void GetStatistics()
    {
        Console.WriteLine(
            @"Average {0:F}
Standard deviation {1:F}
Mode {2}
Median {3}
Packets loss rate {4:F}", _average.Get(), _standardDeviation.Get(), _mode.Get(), _median.Get(), PacketsLoss.Get());
    }
}