using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatsonWebsocket;

namespace EFT_Posreader
{
    internal class SocketServer
    {
        private WatsonWsServer server;
        private static Logger logger;

        public SocketServer(Logger l)
        {
            server = new WatsonWsServer("localhost", 9000, false);
            server.ClientConnected += ClientConnected;
            server.ClientDisconnected += ClientDisconnected;
            server.MessageReceived += MessageReceived;
            server.Start();
            logger = l;
            logger.Log("Websocket: Start!");
        }

        public void Send(string message)
        {
            var clients = server.ListClients();
            foreach(string client in clients)
            {
                server.SendAsync(client, message);
            }
        }

        private static void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            logger.Log("Websocket: Nowy klient połączony!");
        }

        private static void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            logger.Log("Websocket: Klient rozłączył się!");
        }

        private static void MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            Trace.WriteLine("Message received from " + args.IpPort + ": " + Encoding.UTF8.GetString(args.Data));
        }
    }
}
