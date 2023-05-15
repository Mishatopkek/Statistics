using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;

// ReSharper disable FunctionNeverReturns

namespace StatisticsClient;

internal static class Program
{
    private static bool _isFirstInput = true;

    private static Task Main()
    {
        MathHandler mathHandler = new();
        _ = UserInput(mathHandler);
        while (true)
        {
            Console.ReadLine();
            mathHandler.GetStatistics();
        }
    }

    private static Dictionary<string, string> LoadAppSettings()
    {
        const string key = "key";
        const string value = "value";
        Dictionary<string, string> appSettings = new();
        XmlDocument xmlDoc = new();
        xmlDoc.Load("appsettings.xml");
        XmlNodeList? settingNodes = xmlDoc.SelectNodes("//config/setting");
        if (settingNodes == null) return appSettings;

        foreach (XmlNode settingNode in settingNodes)
        {
            string keyData = settingNode.Attributes?[key]?.Value ??
                             throw new ArgumentNullException(nameof(key), "The key was not found");
            string valueData = settingNode.Attributes?[value]?.Value ??
                               throw new ArgumentNullException(nameof(value), "The value was not found");
            appSettings.Add(keyData, valueData);
        }

        return appSettings;
    }

    private static async Task UserInput(MathHandler mathHandler)
    {
        var appSettings = LoadAppSettings();
        
        IPEndPoint localEp = new(IPAddress.Any, 12347);
        int lagDelay = int.Parse(appSettings["Delay"]);
        UdpClient client = CreateUdpClient(localEp);
        client.JoinMulticastGroup(IPAddress.Parse(appSettings["MulticastGroup"]));

        var watch = Stopwatch.StartNew();

        while (true)
        {
            await FakeLags(watch, lagDelay);

            UdpReceiveResult result = await client.ReceiveAsync();
            ThreadPool.QueueUserWorkItem(_ => { SaveData(mathHandler, result); });
        }
    }

    private static async Task FakeLags(Stopwatch watch, int lagDelay)
    {
        if (watch.ElapsedMilliseconds >= 1000)
        {
            await Task.Delay(lagDelay);
            watch.Restart();
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

    private static void SaveData(MathHandler mathHandler, UdpReceiveResult result)
    {
        ulong currentPackets = BitConverter.ToUInt64(result.Buffer, result.Buffer.Length - 8);
        InitializeDataIfFirstRun(mathHandler, currentPackets);
        mathHandler.PacketsLoss.Add(currentPackets);

        string message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length - 8);
        int currentValue = int.Parse(message);

        mathHandler.AddAll(currentValue);
    }

    private static void InitializeDataIfFirstRun(MathHandler mathHandler, ulong currentPackets)
    {
        if (!_isFirstInput) return;
        _isFirstInput = false;
        mathHandler.PacketsLoss.Start(currentPackets);
    }
}