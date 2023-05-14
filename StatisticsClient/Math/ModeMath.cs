namespace StatisticsClient.Math;

public class ModeMath
{
    private readonly Dictionary<double, int> _freq = new Dictionary<double, int>();
    private double _mode, _maxFreq;
    private readonly AutoResetEvent _addEvent = new AutoResetEvent(false);
    private readonly Queue<double> _numbersToAdd = new Queue<double>();

    public ModeMath()
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

                    if (_freq.ContainsKey(number))
                    {
                        _freq[number]++;
                    }
                    else
                    {
                        _freq[number] = 1;
                    }

                    if (_freq[number] > _maxFreq)
                    {
                        _mode = number;
                        _maxFreq = _freq[number];
                    }
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

    public double Get() => _mode;
}