using Newtonsoft.Json;
using P2PDiscover.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PDiscover.server {

    class Program {
        static UdpClient udpServer = new UdpClient(5020);
        static int number = 0;
        static List<confirmTask> numberConfirms = new List<confirmTask>();
        static object lockNumber = new object();

        static void Main(string[] args) {

            Server();


        }



        static void Server() {
            List<userInfo> _userIPEPs = new List<userInfo>();

            Console.WriteLine("server is open on 5020");


            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            while (true) {
                var bytRecv = udpServer.Receive(ref remoteEP);
                string message = Encoding.Unicode.GetString(bytRecv, 0, bytRecv.Length);
                Console.WriteLine(string.Format("{0}[{1}]", remoteEP, message));


                var env = JsonConvert.DeserializeObject<envelope>(message);
                if (env.type == envelopeType.connect) {
                    //不同端口的话插入数据
                    if (_userIPEPs.FirstOrDefault(t => t.ip == remoteEP.Address.ToString() && t.port == remoteEP.Port) == null) {
                        _userIPEPs.Add(new userInfo { ip = remoteEP.Address.ToString(), port = remoteEP.Port });
                    } else {

                        send("请等待另一个用户接入", remoteEP);
                        continue;
                    }
                    if (_userIPEPs.Count == 2) {

                        sendContentMessage(_userIPEPs[0], _userIPEPs[1]);
                        sendContentMessage(_userIPEPs[1], _userIPEPs[0]);
                        _userIPEPs = new List<userInfo>();
                    }
                } else if (env.type == envelopeType.confirm) {
                    var cf = numberConfirms.FirstOrDefault(t => t.number == env.number);
                    if (cf != null) {
                        cf.isConfirm = true;
                    }
                }


            }
        }



        static void send(string msg, IPEndPoint remoteEP) {


            envelope env = new envelope();
            env.type = envelopeType.message;
            env.body = msg;
            send(env, remoteEP);
        }

        static void send(envelope env, IPEndPoint remoteEP) {
            var sendbytes = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(env));
            udpServer.Send(sendbytes, sendbytes.Length, remoteEP);
        }

        static async Task sendConfirm(envelope env, IPEndPoint remoteEP) {
            confirmTask cf = null;
            await Task.Run(() => {
                lock (lockNumber) {
                    env.number = ++number;
                }
                cf = new confirmTask() { number = number, isConfirm = false };

                numberConfirms.Add(cf);

                var sendbytes = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(env));

                for (int i = 0; i < 3; i++) {
                    udpServer.Send(sendbytes, sendbytes.Length, remoteEP);
                    Thread.Sleep(5000);
                    if (cf.isConfirm) break;
                }

            }).ContinueWith(t => numberConfirms.Remove(cf));
        }


        static void sendContentMessage(userInfo toUser, userInfo contentUser) {
            envelope env = new envelope();
            env.type = envelopeType.connectTo;
            connectTo body = new connectTo();
            body.ip = contentUser.ip;
            body.port = contentUser.port;
            env.body = JsonConvert.SerializeObject(body);
            sendConfirm(env, new IPEndPoint(IPAddress.Parse(toUser.ip), toUser.port));
        }

    }


    public class confirmTask {
        public int number { get; set; }
        public bool isConfirm { get; set; }
    }
}

