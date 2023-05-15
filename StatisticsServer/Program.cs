using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;

namespace StatisticsServer;

internal static class Program
{
    private static readonly Dictionary<string, string> APPSETTINGS = new();
    private static void Main()
    {
        LoadAppsettings();
        ulong packetsSent = 0;
        int minValue = int.Parse(APPSETTINGS["Min"]);
        int maxValue = int.Parse(APPSETTINGS["Max"]);
        string multicastGroup = APPSETTINGS["MulticastGroup"]; 
        const int multicastPort = 12347;

        UdpClient client = CreateUdpClient(multicastGroup, multicastPort, out IPEndPoint endpoint);

        Random random = new();
        while (true)
        {
            int randomNumber = random.Next(minValue, maxValue);

            byte[] data = Encoding.ASCII.GetBytes(randomNumber.ToString());
            byte[] countPacketHasBeenSent = BitConverter.GetBytes(packetsSent);
            byte[] dataAndCountPackets = new byte[data.Length + countPacketHasBeenSent.Length];
            Array.Copy(data, dataAndCountPackets, data.Length);
            Array.Copy(countPacketHasBeenSent, 0, dataAndCountPackets, data.Length, countPacketHasBeenSent.Length);

            _ = client.SendAsync(dataAndCountPackets, dataAndCountPackets.Length, endpoint);

            packetsSent++;
        }
    }

    private static UdpClient CreateUdpClient(string multicastGroup, int multicastPort, out IPEndPoint endpoint)
    {
        UdpClient client = new();
        client.JoinMulticastGroup(IPAddress.Parse(multicastGroup));
        endpoint = new IPEndPoint(IPAddress.Parse(multicastGroup), multicastPort);
        return client;
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
}