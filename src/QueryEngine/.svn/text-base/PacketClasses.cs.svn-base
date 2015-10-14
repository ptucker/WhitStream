/*Have no use for this anymore since all this has been outdate by the Schema class*/

//using System.Net;

//namespace WhitStream.QueryEngine.QueryOperators
//{
//    /// <summary>
//    /// IPv4 Header.  Holds all the relevant data for a packet
//    /// </summary>
//    /// <remarks></remarks>
//    public class IPHeader
//    {
//        /// <summary>
//        /// Version (4 bits) + Internet header length (4 bits
//        /// </summary>
//        public byte ver_ihl;
//        /// <summary>
//        /// Type of service
//        /// </summary>
//        public byte tos;
//        /// <summary>
//        /// Total length
//        /// </summary>
//        public ushort tlen;
//        /// <summary>
//        /// Identification
//        /// </summary>
//        public ushort identification;
//        /// <summary>
//        /// Flags (3 bits) + Fragment offset (13 bits)
//        /// </summary>
//        public ushort flags_fo;
//        /// <summary>
//        /// Time to live
//        /// </summary>
//        public byte ttl;
//        /// <summary>
//        /// Protocol
//        /// </summary>
//        public byte proto;
//        /// <summary>
//        /// Header checksum
//        /// </summary>
//        public ushort h_crc;
//        /// <summary>
//        /// Source address
//        /// </summary>
//        public IPAddress src_ip;
//        /// <summary>
//        /// Destination address
//        /// </summary>
//        public IPAddress dest_ip;
//        /// <summary>
//        /// Option + Padding
//        /// </summary>
//        public uint op_pad;

//        //UDP header information
//        /// <summary>
//        /// Source port
//        /// </summary>
//        public ushort sport;
//        /// <summary>
//        /// Destination port
//        /// </summary>
//        public ushort dport;
//        /// <summary>
//        /// Datagram length
//        /// </summary>
//        public ushort data_len;
//        /// <summary>
//        /// Checksum
//        /// </summary>
//        public ushort crc;
//    }
//}

///// <summary>
//        /// Parses a packet in an IPHeader instance
//        /// </summary>
//        /// <param name="pck_data"> The packet data in a byte[] </param>
//        private IPHeader parse_packet(byte[] pck_data)
//        {
//            IPHeader ip_h = new IPHeader();

//            byte[] byte_ip = new byte[4];

//            //Get the source's IP Address
//            byte_ip[0] = pck_data[26];
//            byte_ip[1] = pck_data[27];
//            byte_ip[2] = pck_data[28];
//            byte_ip[3] = pck_data[29];

//            ip_h.src_ip = new IPAddress(byte_ip);

//            //Get the destination's IP Address
//            byte_ip[0] = pck_data[30];
//            byte_ip[1] = pck_data[31];
//            byte_ip[2] = pck_data[32];
//            byte_ip[3] = pck_data[33];

//            ip_h.dest_ip = new IPAddress(byte_ip);

//            // Version (4 bits) + Internet header length (4 bits)
//            ip_h.ver_ihl = pck_data[14]; //14 = Length of ethernet header

//            //Protocol
//            ip_h.proto = pck_data[23];

//            //Header checksum
//            ip_h.h_crc = (ushort)((int)pck_data[25] << 8 | (ushort)pck_data[24]);

//            //Type of service
//            ip_h.tos = pck_data[15];

//            //Total length
//            ip_h.tlen = (ushort)((int)pck_data[17] << 8 | (ushort)pck_data[16]);

//            //Identification
//            ip_h.identification = (ushort)((int)(ushort)pck_data[19] << 8 | (ushort)pck_data[18]);

//            //Flags and fragment offset
//            ip_h.flags_fo = (ushort)((int)pck_data[21] << 8 | (ushort)pck_data[20]);

//            //Time to live
//            ip_h.ttl = pck_data[22];

//            //Options and padding
//            ip_h.op_pad = ((uint)pck_data[37] << 24) | ((uint)pck_data[36] << 16) | ((uint)pck_data[35] << 8) | (uint)pck_data[34];

//            //Retrieve position of address
//            uint ip_len = (uint)(ip_h.ver_ihl & 0xf) * 4;

//            /* The port is really in NBO, however if I place the bytes in reverse order,
//            *  I have successfully converted NBO to HBO.  Therefore no need for unnecessary
//            *  ntoh calls.
//            */

//            //Find the source port
//            ip_h.sport = (ushort)((int)pck_data[14 + ip_len] << 8 | pck_data[14 + ip_len + 1]);

//            //Find the dest port
//            ip_h.dport = (ushort)((int)pck_data[14 + ip_len + 2] << 8 | pck_data[14 + ip_len + 3]);

//            //Get the datagram length
//            ip_h.data_len = (ushort)((int)pck_data[14 + ip_len + 5] << 8 | pck_data[14 + ip_len + 4]);

//            //Get the address header check sum
//            ip_h.crc = (ushort)((int)pck_data[14 + ip_len + 7] << 8 | pck_data[14 + ip_len + 6]);

//            return ip_h;
//        }