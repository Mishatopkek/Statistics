namespace StatisticsClient;

public class RunningAverage 
{
    private double sum = 0;
    private int count = 0;

    public void Add(int num) {
        sum += num;
        count++;
    }

    public double CurrentAverage() {
        return sum / count;
    }
}
