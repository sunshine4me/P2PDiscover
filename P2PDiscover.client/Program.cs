using Newtonsoft.Json;
using P2PDiscover.message;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PDiscover.client
{
    class Program {
        static UdpClient udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
        static IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("47.96.150.211"), 5020);
        static IPEndPoint connectUser = null;
        static void Main(string[] args) {

            Receive();
            Thread.Sleep(1000);

            Console.WriteLine("等待对方连接...");
            //进行连接
            envelope env = new envelope();
            env.type = envelopeType.connect;
            send(env, serverEP);

           

            while (true) {
                var str = Console.ReadLine();
                if (connectUser != null) {
                    send(str, connectUser);
                }
            }




        }

        static async Task Receive() {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            await Task.Run(() => {
                while (true) {
                    var bytRecv = udpClient.Receive(ref remoteEP);
                    string message = Encoding.Unicode.GetString(bytRecv, 0, bytRecv.Length);
                    Console.WriteLine($"form {remoteEP} : {message}");

                    var env = JsonConvert.DeserializeObject<envelope>(message);

                    //需要确认的信息
                    if (env.number > 0) {
                        envelope confirmEnv = new envelope();
                        confirmEnv.type = envelopeType.confirm;
                        confirmEnv.number = env.number;
                        send(confirmEnv, remoteEP);
                    }

                    if (connectUser == null && env.type == envelopeType.connectTo) {
                        var user = JsonConvert.DeserializeObject<connectTo>(env.body);
                        connectUser = new IPEndPoint(IPAddress.Parse(user.ip), user.port);
                        send("你好", connectUser);
                        Console.WriteLine($"你现在可以和[{user.ip}:{user.port}]发送信息了");
                    }


                }

            });
        }


        static void send(string msg, IPEndPoint remoteEP) {
            envelope env = new envelope();
            env.type = envelopeType.message;
            env.body = msg;
            send(env, remoteEP);
        }

        static void send(envelope env, IPEndPoint remoteEP) {
            var sendbytes = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(env));
            udpClient.Send(sendbytes, sendbytes.Length, remoteEP);
        }
    }






   
}
