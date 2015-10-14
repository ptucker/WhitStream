//Needed for custom defines
#include "Utility.h"

//Needed for the punctuation classes
#include "Punctuation.h"

//Needed for cout/cin
using namespace std;

#ifndef __CLIENT_H__
#define __CLIENT_H__

class Client {

//Members

private:
	//Name of thise client
	char* name;
	//Punctuations client can handle
	char** puncs;
	//IP Address of the server
	string s_ip;
	//Port number of the server
	int port;
	//The socket for connecting to the listening part of the server
	int init_socket;
	//Thread id of the read thread
	unsigned int thread_id;
	//Handle to the read thread
	uintptr_t thread_handle;
	//Buffer used by send_punc
	unsigned char* punc_data;
	
public:
	//Holds the information about the read/write socket
	struct socket_info s_i;
	//If the client is connected to the server
	bool connected;
	//Holds any information that pcap functions would need
	struct pcap_info p_i;
	//Recompile - does the filter need to be recompiled
	//Send_data - should the packethandler send the packet to the server
	bool recompile, send_data;
	//Buffer to hold data needing to be sent to the server
	unsigned char* new_data;
	//Length of new_data - used in making the buffer bigger if needed
	unsigned int nd_length;
	//Which punctuations to send
	char punc_flag;
	//Holds the punctuations
	vector<Punctuation> sec_punc_v;
	vector<Punctuation> frag_punc_v;

//Functions

private:
	//Connects the client to the server by connecting to the listening
	//port, receiving a new port, and then creating a permanent connection
	//to the new port
	int ConnectServer();
	//Initialize/set the device (NIC) and get the user's choice
	int Init_Devices();
	//Initiliaze Punctuations
	void Init_Puncs();
	//Starts the read thread
	int Start_thread();
	//Sends the specific schema to this client
	int send_Schema();
	//Used in send_schema - no need to use it anywhere else
	int send_Attr(int start, int finish, char* attr_name);
	//Starts capturing packets
	void Start_Capture();

public:
	//Sends a punctuation to the server using byte flags
	void byte_send_punc(long ts);
	//Sends a punctuation using a string literal
	void pstr_send_sec(long ts);
	//Compile a filter - possible use in pushed down operators
	int CompileFilter(char* filter);
	//Starts the client - handles any initializing
	void Start();
	//Default constructor
	Client() {}
	//Constructor to do basic initialization of the class
	Client(char* name, string s_ip, int port, char* filter);
	//Default deconstructor
	~Client();
};

#endif //__CLIENT_H__