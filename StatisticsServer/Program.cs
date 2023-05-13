/*using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
 
using var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
 
EndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
while (true)
{
    Thread.Sleep(10);
    byte[] data = new byte[256];
    var rng = RandomNumberGenerator.Create();
    rng.GetBytes(data);
    int bytes = await udpSocket.SendToAsync(data, remotePoint);
    Console.WriteLine($"Отправлено {bytes} байт");
}*/
using System;
using System.Net;
using System.Net.Sockets;

namespace ServerApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a socket.
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Bind the socket to a port.
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555));

            // Start sending random numbers.
            while (true)
            {
                // Generate a random number.
                var randomNumber = new Random().Next(1, 1000);
                var data = BitConverter.GetBytes(randomNumber);

                // Send the random number to the client.
                socket.Send(data, 0, data.Length, SocketFlags.None);
            }
        }
    }
}
