using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NATTest.client
{
    class Program
    {

        static void Main(string[] args)
        {

            IPEndPoint serverEP1 = new IPEndPoint(IPAddress.Parse("47.96.150.211"), 5021);
            IPEndPoint serverEP2 = new IPEndPoint(IPAddress.Parse("47.93.202.40"), 5022);
            UdpClient udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0));

            NATInfo nat1;
            NATInfo nat2;

            byte[] sendbytes = Encoding.Unicode.GetBytes("test");
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);


            Console.WriteLine("开始NAT测试,如果长时间无响应表示该网络可能不支持P2P......");

            //发送UDP信息到服务器A
            udpClient.Send(sendbytes, sendbytes.Length, serverEP1);

            //接收服务器A返回信息,并打印NAT信息
            var bytRecv1 = udpClient.Receive(ref remoteEP);
            var message1 = Encoding.Unicode.GetString(bytRecv1);
            Console.WriteLine($"from {remoteEP} : {message1}");
            nat1 = JsonConvert.DeserializeObject<NATInfo>(message1);


            //发送UDP信息到服务器B
            udpClient.Send(sendbytes, sendbytes.Length, serverEP2);
            //接收服务器B返回信息,并打印NAT信息
            var bytRecv2 = udpClient.Receive(ref remoteEP);
            var message2 = Encoding.Unicode.GetString(bytRecv1);
            Console.WriteLine($"from {remoteEP} : {message2}");
            nat2 = JsonConvert.DeserializeObject<NATInfo>(message2);

            if(nat1.ip==nat2.ip && nat1.port==nat2.port)
                Console.WriteLine("可以进行P2P通信");
            else
                Console.WriteLine("你的网络可能不支持P2P");
            Console.ReadLine();

        }
    }

    public class NATInfo {
        public string ip { get; set; }
        public int port { get; set; }
    }
}
