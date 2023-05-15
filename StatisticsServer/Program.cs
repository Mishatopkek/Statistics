using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
// ReSharper disable FunctionNeverReturns

namespace StatisticsServer;

internal static class Program
{
    private static async Task Main()
    {
        var appSettings = LoadAppSettings();
        ulong packetsSent = 0;
        int minValue = int.Parse(appSettings["Min"]);
        int maxValue = int.Parse(appSettings["Max"]);
        string multicastGroup = appSettings["MulticastGroup"];
        const int multicastPort = 12347;

        UdpClient client = CreateUdpClient(multicastGroup, multicastPort, out IPEndPoint endpoint);

        Random random = new();
        while (true)
        {
            int randomNumber = random.Next(minValue, maxValue);

            byte[] dataAndCountPackets = CombineDataAndCountPackets(randomNumber, packetsSent);

            await client.SendAsync(dataAndCountPackets, dataAndCountPackets.Length, endpoint);

            packetsSent++;
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

    private static UdpClient CreateUdpClient(string multicastGroup, int multicastPort, out IPEndPoint endpoint)
    {
        UdpClient client = new();
        client.JoinMulticastGroup(IPAddress.Parse(multicastGroup));
        endpoint = new IPEndPoint(IPAddress.Parse(multicastGroup), multicastPort);
        return client;
    }

    private static byte[] CombineDataAndCountPackets(int randomNumber, ulong packetsSent)
    {
        byte[] data = Encoding.ASCII.GetBytes(randomNumber.ToString());
        byte[] countPacketHasBeenSent = BitConverter.GetBytes(packetsSent);
        byte[] dataAndCountPackets = new byte[data.Length + countPacketHasBeenSent.Length];
        Array.Copy(data, dataAndCountPackets, data.Length);
        Array.Copy(countPacketHasBeenSent, 0, dataAndCountPackets, data.Length, countPacketHasBeenSent.Length);
        return dataAndCountPackets;
    }
}