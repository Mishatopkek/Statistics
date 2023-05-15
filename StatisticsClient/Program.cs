using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using StatisticsClient.Math;

namespace StatisticsClient;

internal static class Program
{
    private static Action<double>? _addData;
    private static bool _isFirstInput = true;
    private static readonly Dictionary<string, string> APPSETTINGS = new();
    private static Task Main()
    {
        MedianMath median = new();
        StandardDeviationMath sd = new();
        ModeMath mode = new();
        AverageMath average = new();
        PacketsLossMath pl = new();
        _addData = median.Add;
        _addData += sd.Add;
        _addData += mode.Add;
        _addData += average.Add;
        LoadAppsettings();

        _ = UserInput(pl);
        while (true)
        {
            Console.ReadLine();
            Console.WriteLine(
@"Average {0:F}
Standard deviation {1:F}
Mode {2}
Median {3}
Packets loss rate {4:F}", average.Get(), sd.Get(), mode.Get(), median.Get(), pl.Get());
        }
    }

    private static void LoadAppsettings()
    {
        XmlDocument xmlDoc = new();
        xmlDoc.Load("appsettings.xml");
        XmlNodeList? settingNodes = xmlDoc.SelectNodes("//config/setting");
        foreach (XmlNode settingNode in settingNodes)
        {
            string key = settingNode.Attributes?["key"].Value;
            string value = settingNode.Attributes?["value"].Value;
            APPSETTINGS.Add(key, value);
        }
    }

    private static async Task UserInput(PacketsLossMath pl)
    {
        IPEndPoint localEp = new(IPAddress.Any, 12347);
        int lagDelay = int.Parse(APPSETTINGS["Delay"]);

        UdpClient client = CreateUdpClient(localEp);
        client.JoinMulticastGroup(IPAddress.Parse(APPSETTINGS["MulticastGroup"]));

        Stopwatch watch = Stopwatch.StartNew();

        while (true)
        {
            if (watch.ElapsedMilliseconds >= 1000)
            {
                await Task.Delay(lagDelay);
                watch.Restart();
            }

            UdpReceiveResult result = await client.ReceiveAsync();
            ThreadPool.QueueUserWorkItem(_ => { SaveData(pl, result); });
        }
    }

    private static UdpClient CreateUdpClient(EndPoint localEp)
    {
        UdpClient client = new();
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        client.ExclusiveAddressUse = false;
        client.Client.Bind(localEp);
        return client;
    }

    private static void SaveData(PacketsLossMath pl, UdpReceiveResult result)
    {
        ulong currentPackets = BitConverter.ToUInt64(result.Buffer, result.Buffer.Length - 8);
        pl.Add(currentPackets);
        if (_isFirstInput)
        {
            _isFirstInput = false;
            pl.Start(currentPackets);
        }

        string message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length - 8);
        int currentValue = int.Parse(message);

        _addData?.Invoke(currentValue);
    }
}