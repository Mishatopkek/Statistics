namespace StatisticsClient;
public class MedianFinder
{
    PriorityQueue<int, int> left = new();
    PriorityQueue<int, int> right = new();
    bool odd = false;

    public void AddNum(int n)
    {
        odd = !odd;
        int m = right.EnqueueDequeue(n, -n);
        left.Enqueue(m, m);

        if (left.Count - 1 <= right.Count) return;
        m = left.Dequeue();
        right.Enqueue(m, -m);
    }
    
    public double FindMedian() =>
        odd ? left.Peek() : (left.Peek() + right.Peek()) / 2.0;
}