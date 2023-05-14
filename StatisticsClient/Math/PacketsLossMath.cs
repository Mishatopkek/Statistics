namespace StatisticsClient.Math;

public class PacketsLossMath
{
    private ulong _packetsStarted;
    private ulong _currentPackets;
    private ulong _receivedPackets;

    public void Add(double number)
    {
        _currentPackets = (ulong)number;
        _receivedPackets++;
    }

    public double Get()
    {
        double sendPackets = _currentPackets - _packetsStarted;
        return (sendPackets - _receivedPackets) / sendPackets * 100;
    }

    public void Start(ulong number)
    {
        _packetsStarted = number;
    }
}