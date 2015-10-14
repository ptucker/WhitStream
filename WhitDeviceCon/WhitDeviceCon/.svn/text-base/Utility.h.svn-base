#pragma once

//Includes
//Standard #include directories
#include <iostream>
#include <string>
#include <vector>
#include <algorithm>

//Usage of time
#include <time.h>

//#include directories needed for WinPCap
#include <pcap.h>
#include <remote-ext.h>

//Needed for multithreading
#include <process.h>

//IP Address - holds format byte1.byte2....
struct ip_address {
	u_char byte1;
	u_char byte2;
	u_char byte3;
	u_char byte4;
};

//IPv4 Header
struct ip_header {
	u_char ver_ihl;		// Version (4 bits) + Internet header length (4 bits)
	u_char tos;			// Type of service 
	u_short tlen;			// Total length 
	u_short identification; // Identification
	u_short flags_fo;		// Flags (3 bits) + Fragment offset (13 bits)
	u_char ttl;			// Time to live
	u_char proto;			// Protocol
	u_short crc;			// Header checksum
	ip_address saddr;		// Source address
	ip_address daddr;		// Destination address
	u_int op_pad;			// Option + Padding
};

//UDP Header
struct address_header {
	u_short sport;			// Source port
	u_short dport;			// Destination port
	u_short len;			// Datagram length
	u_short crc;			// Checksum
};

//Hold the socket information to use anywhere
struct socket_info {
	int l_sockfd;			// Local sockfd
	struct hostent *s_info;	// Server info
};

//Hold pcap stuff for the receive thread
struct pcap_info {
	pcap_t* adhandle;
	struct bpf_program fcode;
	char* pcap_filter;
	bpf_u_int32 netmask;

};

//Converts ints to byte[] (char[])
unsigned char* converti_to_b(int i);
//Converts byte[4] to an int
int convertb_to_i(unsigned char* b);
//Converts an int to a byte[] by passing pointers
void converti_to_b_r(int i, unsigned char* b);
//Read thread for the client class
unsigned __stdcall Runthread(void * param);
//Packet handler for the client class
void packethandler(u_char *param, const struct pcap_pkthdr *header, const u_char *pkt_data);

//Definitions
//Patterns
#define WILDCARD_PATTERN 1
#define LITERAL_PATTERN 2
#define RANGE_PATTERN 3
#define LIST_PATTERN 4

//Punctuation Flag - determining which punctuation should be sent
#define PUNC_TIME_SEC	2
#define PUNC_FRAG		4
//Used in send_schema - don't change: These are specific to a TCP packet
//and changing them will create useless data on the server side
#define TIME_OFFSET 8
#define FLAG_OFFSET 1

#define TSEC_START		3+FLAG_OFFSET
#define TSEC_STOP		0+FLAG_OFFSET
#define TUSEC_START		7+FLAG_OFFSET
#define TUSEC_STOP		4+FLAG_OFFSET
#define EH_START		13+TIME_OFFSET+FLAG_OFFSET
#define EH_STOP			0+TIME_OFFSET+FLAG_OFFSET
#define VIHL			14+TIME_OFFSET+FLAG_OFFSET
#define TOS				15+TIME_OFFSET+FLAG_OFFSET
#define TL_START		17+TIME_OFFSET+FLAG_OFFSET
#define TL_STOP			16+TIME_OFFSET+FLAG_OFFSET
#define ID_START		19+TIME_OFFSET+FLAG_OFFSET
#define ID_STOP			18+TIME_OFFSET+FLAG_OFFSET
#define FLFRAG_START	21+TIME_OFFSET+FLAG_OFFSET
#define FLFRAG_STOP		20+TIME_OFFSET+FLAG_OFFSET
#define TTL				22+TIME_OFFSET+FLAG_OFFSET
#define PROTO			23+TIME_OFFSET+FLAG_OFFSET
#define HCSUM_START		25+TIME_OFFSET+FLAG_OFFSET
#define HCSUM_STOP		24+TIME_OFFSET+FLAG_OFFSET
#define SIP_START		29+TIME_OFFSET+FLAG_OFFSET
#define SIP_STOP		26+TIME_OFFSET+FLAG_OFFSET
#define DIP_START		33+TIME_OFFSET+FLAG_OFFSET
#define DIP_STOP		30+TIME_OFFSET+FLAG_OFFSET
#define OP_START		37+TIME_OFFSET+FLAG_OFFSET
#define OP_STOP			34+TIME_OFFSET+FLAG_OFFSET
#define TCP_SP_START	34+TIME_OFFSET+FLAG_OFFSET
#define TCP_SP_STOP		35+TIME_OFFSET+FLAG_OFFSET
#define TCP_DP_START	36+TIME_OFFSET+FLAG_OFFSET
#define TCP_DP_STOP		37+TIME_OFFSET+FLAG_OFFSET
#define SQ_START		41+TIME_OFFSET+FLAG_OFFSET
#define SQ_STOP			38+TIME_OFFSET+FLAG_OFFSET

//Pcap defines
#define READ_TIMEOUT 10

//Socket defines
#define CLOSED_CONNECTION -1