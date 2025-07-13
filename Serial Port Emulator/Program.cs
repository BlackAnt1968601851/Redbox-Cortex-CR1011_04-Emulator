using System;
using System.IO.Ports;
using System.Text;

namespace Serial_Port_Emulator
{
    internal class Program
    {
        public struct CortexPacket
        {
            public byte[] prefix;
            public string commandtype;
            public int datasize;
            public byte data;
            public byte[] reserved;
            public byte[] crc14;
        }
        public static string readCommand(byte[] commandbytes)
        {
            CortexPacket packet = new CortexPacket();
            byte[] prefix = new byte[4];
            byte[] commandtype = new byte[1];
            byte[] sizeencoded = new byte[4];
            Array.Copy(commandbytes, 0, prefix, 0, 4);
            Array.Copy(commandbytes, 4, commandtype, 0, 1);
            Array.Copy(commandbytes, 5, sizeencoded, 0, 1);
            int datasize = BitConverter.ToInt32(sizeencoded);
            byte[] data = new byte[datasize];
            Array.Copy(commandbytes, 6, data, 0, datasize);

            return Encoding.UTF8.GetString(commandtype) + Encoding.UTF8.GetString(data);
        }
        static void Main(string[] args)
        {
            device();
        }
        static void device()
        {
            SerialPort serialport = new SerialPort("COM5");
            serialport.Open();
            while (true)
            {
                var bytestoread = serialport.BytesToRead;
                byte[] buffer = new byte[bytestoread];
                if (serialport.Read(buffer, 0, bytestoread) != 0)
                {
                    string command = readCommand(buffer);
                    Console.WriteLine(command);
                    //Console.WriteLine(Convert.ToHexString(buffer));
                    if (command == "G(21D)")
                    {
                        Console.WriteLine("Get USB Speed");
                        serialport.Write(Convert.FromHexString("01581E61702F6430303030303030310004"), 0, 17);
                    }
                    if (command == "G(42)")
                    {
                        Console.WriteLine("Expect Acknowledgement From Host");
                        serialport.Write(Convert.FromHexString("01581E61702F6430303030303030310004"), 0, 17);
                    }
                    if (command == "G(08)")
                    {
                        Console.WriteLine("Reader Packet Format");
                        serialport.Write(Convert.FromHexString("01581E61702F6430303030303030320004"), 0, 17);
                    }
                    if (command.Contains("Y") == true)
                    {
                        Console.WriteLine("Acknowlegement Signal");
                        serialport.Write(Convert.FromHexString("01581E61702F6430303030303030310004"), 0, 17);
                    }
                }
            }
            serialport.Close();
        }
    }
}