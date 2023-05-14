using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace StatisticsServer;

internal static class Program
{
    private static void Main()
    {
        ulong packetsSent = 0;
        int minValue = 1;  // Minimum value of random number
        int maxValue = 99;  // Maximum value of random number
        int multicastPort = 12347;  // UDP multicast port number
        string multicastGroup = "239.255.255.250";  // UDP multicast group address

        // Create a UDP client and join the multicast group
        UdpClient client = new();
        client.JoinMulticastGroup(IPAddress.Parse(multicastGroup));

        // Generate and send random numbers indefinitely
        Random random = new();
        byte[] content = new byte[247];
        byte[] serverAmountOfSendContent = new byte[8];
        byte[] contentPlusAmountOfSendContent = new byte[255];
        IPEndPoint endpoint = new(IPAddress.Parse(multicastGroup), multicastPort);
        TimeSpan timespan = new(10000);
        while (true)
        {
            Thread.Sleep(timespan);
            int randomNumber = random.Next(minValue, maxValue);

            int dataLength = Encoding.ASCII.GetBytes(randomNumber.ToString(), 0, randomNumber.ToString().Length, content, 0);
            BitConverter.TryWriteBytes(serverAmountOfSendContent, packetsSent);

            Buffer.BlockCopy(content, 0, contentPlusAmountOfSendContent, 0, dataLength);
            Buffer.BlockCopy(serverAmountOfSendContent, 0, contentPlusAmountOfSendContent, dataLength, 8);

            _ = client.SendAsync(contentPlusAmountOfSendContent, contentPlusAmountOfSendContent.Length, endpoint);

            packetsSent++;
        }
    }
}