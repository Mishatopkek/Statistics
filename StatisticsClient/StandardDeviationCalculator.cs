namespace StatisticsClient;

public class StandardDeviationCalculator
{
    private double mean;
    private double m2;
    private long count;

    public void AddDataPoint(double x)
    {
        count++;
        var delta = x - mean;
        mean += delta / count;
        var delta2 = x - mean;
        m2 += delta * delta2;
    }

    public double GetStandardDeviation()
    {
        if (count < 2)
        {
            throw new InvalidOperationException("Not enough data points to calculate standard deviation.");
        }

        return Math.Sqrt(m2 / (count - 1));
    }
}
