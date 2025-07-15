using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial_Port_Emulator
{
    public static class Packet
    {
        public struct CortexPacket
        {
            public bool validpacket;
            public byte[] prefix;
            public string commandtype;
            public int datasize;
            public byte[] data;
            public byte[] reserved;
            public byte[] crc14;
        }
        public static CortexPacket readCommand(byte[] commandbytes)
        {
            CortexPacket packet = new CortexPacket();
            byte[] prefix = new byte[4];
            byte[] commandtype = new byte[1];
            byte[] sizeencoded = new byte[4];
            byte[] crc14 = new byte[2];
            Array.Copy(commandbytes, 0, prefix, 0, 4);
            Array.Copy(commandbytes, 4, commandtype, 0, 1);
            Array.Copy(commandbytes, 5, sizeencoded, 0, 1);
            int datasize = BitConverter.ToInt32(sizeencoded);
            byte[] data = new byte[datasize];
            Array.Copy(commandbytes, 6, data, 0, datasize);
            Array.Copy(commandbytes, datasize + 7, crc14, 0, 2);
            packet.prefix = prefix;
            packet.commandtype = Encoding.UTF8.GetString(commandtype);
            packet.datasize = datasize;
            packet.data = data;
            packet.reserved = Convert.FromHexString("00");
            packet.validpacket = true;
            packet.crc14 = crc14;
            return packet;
        }
    }
}
