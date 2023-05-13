using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace StatisticsServer;

internal static class Program
{
    private static async Task Main()
    {
        int minValue = 1;  // Minimum value of random number
        int maxValue = 100;  // Maximum value of random number
        int multicastPort = 12347;  // UDP multicast port number
        string multicastGroup = "239.255.255.250";  // UDP multicast group address

        // Create a UDP client and join the multicast group
        UdpClient client = new();
        client.JoinMulticastGroup(IPAddress.Parse(multicastGroup));

        // Generate and send random numbers indefinitely
        Random random = new();
        while (true)
        {
            await Task.Delay(1);
            int randomNumber = random.Next(minValue, maxValue + 1);
            byte[] data = Encoding.ASCII.GetBytes(randomNumber.ToString());
            await client.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Parse(multicastGroup), multicastPort));
        }
    }
}