namespace StatisticsClient.Math;

public class StandardDeviationMath
{
    private double _mean;
    private double _m2;
    private long _count;

    public void Add(double number)
    {
        _count++;
        double delta = number - _mean;
        _mean += delta / _count;
        double delta2 = number - _mean;
        _m2 += delta * delta2;
    }

    public double Get()
    {
        if (_count < 2)
        {
            throw new InvalidOperationException("Not enough data points to calculate standard deviation.");
        }

        return System.Math.Sqrt(_m2 / (_count - 1));
    }
}