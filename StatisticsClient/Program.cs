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

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using StatisticsClient.Math;

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
    private static Action<double>? _addData;
    private static bool _isFirstInput = true;
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

    private static async Task UserInput(PacketsLossMath pl)
    {
        int multicastPort = 12347; // UDP multicast port number
        string multicastGroup = "239.255.255.250"; // UDP multicast group address
        IPEndPoint localEp = new(IPAddress.Any, multicastPort); // Local endpoint to bind to

        // Create a UDP client and bind it to the local endpoint
        UdpClient client = new();
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        client.ExclusiveAddressUse = false;
        client.Client.Bind(localEp);

        // Join the multicast group asynchronously
        client.JoinMulticastGroup(IPAddress.Parse(multicastGroup));

        // Receive multicast messages indefinitely
        byte[] countPacket = new byte[8];
        byte[] rawMessage = new byte[247];
        char[] charBuffer = new char[247];
        Stopwatch watch = Stopwatch.StartNew();

        while (true)
        {
            if (watch.ElapsedMilliseconds >= 1000)
            {
                await Task.Delay(10);
                watch.Restart();
            }
            UdpReceiveResult result = await client.ReceiveAsync();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                NewMethod(pl, countPacket, result, rawMessage, charBuffer);
            });
        }
    }

    private static void NewMethod(
        PacketsLossMath pl, 
        byte[] countPacket, 
        UdpReceiveResult result, 
        byte[] rawMessage,
        char[] charBuffer)
    {
        Array.Clear(countPacket, 0, countPacket.Length);
        Array.Copy(result.Buffer, result.Buffer.Length - 8, countPacket, 0, 8);
        ulong currentPackets = BitConverter.ToUInt64(countPacket, 0);
        pl.Add(currentPackets);
        if (_isFirstInput)
        {
            _isFirstInput = false;
            pl.Start(currentPackets);
        }

        Array.Clear(rawMessage, 0, rawMessage.Length);
        Array.Copy(result.Buffer, 0, rawMessage, 0, result.Buffer.Length - 8);
        int messageLength = result.Buffer.Length - 8;
        Encoding.ASCII.GetChars(rawMessage, 0, messageLength, charBuffer, 0);
        string message = new string(charBuffer, 0, messageLength);
        int currentValue = int.Parse(message);

        _addData?.Invoke(currentValue);
    }
}