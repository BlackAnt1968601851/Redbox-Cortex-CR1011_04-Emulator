using System.Text;

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
        public struct ResponsePacket
        {
            public bool validpacket;
            public byte[] ReaderID;
            public int packetnumber;
            public byte[] timestamp;
            public int datasize;
            public byte[] data;
            public byte[] crc16;
            public byte[] rawdata;
        }
        public static ResponsePacket readResponse(byte[] commandbytes)
        {
            ResponsePacket response = new ResponsePacket();
            response.validpacket = false;
            response.rawdata = commandbytes;
            byte[] ReaderID = new byte[4];
            byte[] packetnumber = new byte[1];
            byte[] timestamp = new byte[4];
            byte[] datasize = new byte[2];
            Array.Copy(commandbytes, 4, ReaderID, 0, 4);
            Array.Copy(commandbytes, 8, packetnumber, 0, 1);
            Array.Copy(commandbytes, 9, timestamp, 0, 4);
            Array.Copy(commandbytes, 13, datasize, 0, 2);
            Array.Reverse(datasize);
            packetnumber = Convert.FromHexString(Convert.ToHexString(packetnumber) + "00");
            response.packetnumber = BitConverter.ToInt16(packetnumber);
            response.ReaderID = ReaderID;
            response.timestamp = timestamp;
            response.datasize = BitConverter.ToInt16(datasize);
            byte[] data = new byte[response.datasize];
            Array.Copy(commandbytes, 15, data, 0, response.datasize);
            response.data = data;
            byte[] crc16 = new byte[2];
            Array.Copy(commandbytes, 15 + response.datasize, crc16, 0, 2);
            byte[] commandbyteswithoutcrc = new byte[commandbytes.Length - 2];
            Array.Copy(commandbytes, 0, commandbyteswithoutcrc, 0, commandbytes.Length - 2);
            if (Convert.ToHexString(CRC.CRC16(0, commandbyteswithoutcrc)) == Convert.ToHexString(crc16))
            {
                response.validpacket = true;
            }
            Console.WriteLine("Raw Response: " + Convert.ToHexString(response.rawdata));
            Console.WriteLine("Raw Response in UTF8: " + Encoding.UTF8.GetString(commandbytes));
            Console.WriteLine("Reader ID: " + Convert.ToHexString(response.ReaderID));
            Console.WriteLine("Packet Number: " + response.packetnumber.ToString());
            Console.WriteLine("Time Stamp: " + Convert.ToHexString(response.timestamp));
            Console.WriteLine("Packet Size: " + response.datasize.ToString());
            Console.WriteLine("Data: " + Convert.ToHexString(response.data));
            Console.WriteLine("CRC: " + Convert.ToHexString(crc16));
            Console.WriteLine("CRC Valid: " + response.validpacket);
            return response;
        }
        public static CortexPacket readCommand(byte[] commandbytes)
        {
            CortexPacket packet = new CortexPacket();
            packet.validpacket = false;
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
            packet.crc14 = crc14;
            byte[] commandbyteswithoutcrc = new byte[commandbytes.Length - 2];
            Array.Copy(commandbytes, 0, commandbyteswithoutcrc, 0, commandbytes.Length - 2);
            byte[] crcoriginalcheck = Convert.FromHexString(Convert.ToHexString(Encoding.UTF8.GetBytes(packet.commandtype)) + Convert.ToHexString(new byte[] { (byte)packet.datasize }) + Convert.ToHexString(packet.data) + "00");
            if (Convert.ToHexString(commandbytes) == Convert.ToHexString(CRC.PrepCommand(crcoriginalcheck)))
            {
                packet.validpacket = true;
            }
            if (packet.validpacket == true)
            {
                Console.WriteLine("CRC Check: Success");
            }
            if (packet.validpacket == false)
            {
                Console.WriteLine("CRC Check: Failed");
            }
            Console.WriteLine(Convert.ToHexString(commandbytes));
            Console.WriteLine(Convert.ToHexString(commandbyteswithoutcrc));
            return packet;
        }
    }
}
