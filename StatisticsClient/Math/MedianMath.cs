namespace StatisticsClient.Math;

public class MedianMath
{
    private readonly PriorityQueue<int, int> _left = new();
    private readonly PriorityQueue<int, int> _right = new();
    private bool _odd;
    private readonly AutoResetEvent _addEvent = new(false);
    private readonly Queue<double> _numbersToAdd = new();

    public MedianMath()
    {
        ThreadPool.QueueUserWorkItem(AddNumbers);
    }

    private void AddNumbers(object? state)
    {
        while (true)
        {
            _addEvent.WaitOne();
            lock (_numbersToAdd)
            {
                while (_numbersToAdd.Count > 0)
                {
                    double number = _numbersToAdd.Dequeue();
                    int currentValue = (int)number;
                    _odd = !_odd;
                    int member = _right.EnqueueDequeue(currentValue, -currentValue);
                    _left.Enqueue(member, member);

                    if (_left.Count - 1 <= _right.Count) continue;
                    member = _left.Dequeue();
                    _right.Enqueue(member, -member);
                }
            }
        }
    }

    public void Add(double number)
    {
        lock (_numbersToAdd)
        {
            _numbersToAdd.Enqueue(number);
        }
        _addEvent.Set();
    }

    public double Get() => _odd ? _left.Peek() : (_left.Peek() + _right.Peek()) / 2.0;
}