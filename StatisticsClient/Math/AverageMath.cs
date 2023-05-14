namespace StatisticsClient.Math;

public class AverageMath
{
    private double _sum;
    private int _count;

    public void Add(double number)
    {
        _sum += number;
        _count++;
    }

    public double Get()
    {
        return _sum / _count;
    }
}