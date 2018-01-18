using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NATTest.server
{
    class Program {

        static void Main(string[] args) {

            //Server();
            int serverPort = Convert.ToInt32(args[0]);

            Server(serverPort);
            Console.ReadLine();

        }



        static void Server(int port) {

            UdpClient udpServer = new UdpClient(port);

            Console.WriteLine("NATTest.Server is open on "+ port);

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            while (true) {

                var bytRecv = udpServer.Receive(ref remoteEP);
                string message = Encoding.Unicode.GetString(bytRecv, 0, bytRecv.Length);
                Console.WriteLine(string.Format("{0}[{1}]", remoteEP, message));
                NATInfo natInfo = new NATInfo() { ip = remoteEP.Address.ToString(), port = remoteEP.Port };
                byte[] sendbytes = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(natInfo));
                udpServer.Send(sendbytes, sendbytes.Length, remoteEP);

            }


        }

        public class NATInfo {
            public string ip { get; set; }
            public int port { get; set; }
        }


    }
}
