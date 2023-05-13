/*using System.Net;
using System.Net.Sockets;
using System.Text;
 
using var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
 
var localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
// начинаем прослушивание входящих сообщений
udpSocket.Bind(localIP);
Console.WriteLine("UDP-сервер запущен...");
 
byte[] data = new byte[256]; // буфер для получаемых данных
//адрес, с которого пришли данные
EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
// получаем данные в массив data
while (true)
{
    var result = await udpSocket.ReceiveFromAsync(data, remoteIp);
    var message = Encoding.UTF8.GetString(data, 0, result.ReceivedBytes);
 
    Console.WriteLine($"Получено {result.ReceivedBytes} байт");
    Console.WriteLine($"Удаленный адрес: {result.RemoteEndPoint}");
}*/

using System.Net;
using System.Net.Sockets;
using System.Text;
// namespace ClientApplication
// {
//     class Program
//     {
//         static void Main(string[] args)
//         {
//             // Create a socket.
//             using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//
//             // Bind the socket to a port.
//             socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555));
//
//             // Start receiving random numbers.
//             while (true)
//             {
//                 // Receive a random number from the server.
//                 var bytes = new byte[10];
//                 socket.Receive(bytes, 0, bytes.Length, SocketFlags.None);
//
//                 // Convert the random number to an integer.
//                 var randomNumber = BitConverter.ToInt32(bytes, 0);
//
//                 // Add the random number to a list.
//                 List<int> numbers = new() {randomNumber};
//
//                 // Calculate the average, standard deviation, mode, and median.
//                 var average = numbers.Average();
//                 var standardDeviation = numbers.StandardDeviation(average);
//                 var mode = numbers.Mode();
//                 var median = numbers.Median();
//
//                 // Print the statistics.
//                 Console.WriteLine("Average: {0}", average);
//                 Console.WriteLine("Standard deviation: {0}", standardDeviation);
//                 Console.WriteLine("Mode: {0}", mode);
//                 Console.WriteLine("Median: {0}", median);
//
//                 // Wait for the user to press enter.
//                 Console.ReadLine();
//             }
//         }
//     }
// }

namespace StatisticsClient;

internal static class Program
{
    private static async Task Main()
    {
        var median = new MedianFinder();
        _ = UserInput(median);
        while (true)
        {
            Console.ReadLine();
            Console.WriteLine("Ti tupoi " + median.FindMedian());
        }
    }

    private static async Task UserInput(MedianFinder median)
    {
        int multicastPort = 12347;  // UDP multicast port number
        string multicastGroup = "239.255.255.250";  // UDP multicast group address
        IPEndPoint localEP = new (IPAddress.Any, multicastPort);  // Local endpoint to bind to

        // Create a UDP client and bind it to the local endpoint
        UdpClient client = new ();
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        client.ExclusiveAddressUse = false;
        client.Client.Bind(localEP);

        // Join the multicast group asynchronously
        client.JoinMulticastGroup(IPAddress.Parse(multicastGroup));

        // Receive multicast messages indefinitely
        while (true)
        {
            UdpReceiveResult result = await client.ReceiveAsync();
            string message = Encoding.ASCII.GetString(result.Buffer);
            
            median.AddNum(int.Parse(message));
        }
    }
}