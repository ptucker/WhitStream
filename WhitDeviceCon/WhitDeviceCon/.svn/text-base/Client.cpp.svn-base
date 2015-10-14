#include "Client.h"
#include <sstream>
//Constructor/Deconstructor
Client::Client(char* n, string sip, int p, char* filter)
{
	connected = false;
	name = n;
	s_ip = sip;
	port = p;
	recompile = false;
	send_data = false;
	nd_length = 65536;
	new_data = new unsigned char[nd_length];
	punc_data = new unsigned char[200];
	p_i.pcap_filter = filter;
	punc_flag = 0;
	time_t t_i = time(NULL);
	puncs = NULL;
}

Client::~Client()
{
	WSACleanup();

	unsigned int i = 0;
	for(; i < sec_punc_v.size() || i < frag_punc_v.size(); i++)
	{
		if(i < sec_punc_v.size())
			sec_punc_v[i].~Punctuation();
		if(i < frag_punc_v.size())
			frag_punc_v[i].~Punctuation();
	}
	sec_punc_v.~vector();
	frag_punc_v.~vector();

	delete new_data;
	//delete timestamp;
	if (puncs != NULL)
		delete puncs;
}

//Init Functions
int Client::Init_Devices()
{
	pcap_if_t *alldevices;	//List of Devices
	pcap_if_t *device;		//Used in printing the devices
	int sd;					//User device choice
	int num_dev = 0;			//Number of devices
	char errbuf[PCAP_ERRBUF_SIZE];	//Error buffer
	int i;

	//Retrieve device list
	if (pcap_findalldevs(&alldevices, errbuf) == -1) {
		cout << "Error in pcap_findalldevs: " << errbuf;
		return -1;
	}

	//Print the devices to command prompt
	for (device = alldevices; device; device = device->next) {
		//cout << ++num_dev << device->name << endl;
		++num_dev;
		if (device->description)
			;//cout << device->description << endl;
		else
			;//cout << "No description avaliable\n";
	}

	if (num_dev == 0) {
		cout << "\nNo devices found, install WinPCap.\n";
		return -1;
	}

	//cout << "\nEnter preferred device number (1-" << num_dev << "): ";
	//cin >> sd;
	sd = 2;

	//Is sd a valid device?
	while (sd < 1 || sd > num_dev) {
		cout << "Invalid device number!\nEnter preferred device number (1-" << num_dev << "): ";
		cin >> sd;
		cout << endl;
	}

	//Go to the preferred device
	for (i = 0, device = alldevices; i < sd - 1; device = device->next, i++);

	//Open preferred device
	/* Function Call:
	 name of the device
	 portion of the packet to capture, 65536 grants that the whole packet will be captured on all the MACs.
	 promiscuous mode (nonzero means promiscuous)
	 read timeout
	 */
	if ((p_i.adhandle = pcap_open_live(device->name, 65536,	1, READ_TIMEOUT, errbuf)) == NULL) {
		cout << "\nUnable to open the adapter.  It is not supported by WinPcap\n";
		pcap_freealldevs(alldevices);
		return -1;
	}

	//Make sure device is ethernet
	if (pcap_datalink(p_i.adhandle) != DLT_EN10MB) {
		cout << "\nThis program works only on Ethernet networks.\n";
		pcap_freealldevs(alldevices);
		return -1;
	}

	if (device->addresses != NULL)
		//Retrieve mask of first address in device
		p_i.netmask = ((sockaddr_in *)(device->addresses->netmask))->sin_addr.S_un.S_addr;
	else
		p_i.netmask = 0xffffff;

	CompileFilter(p_i.pcap_filter);

	//cout << "\nListening on " << device->description << ".\n";

	//No longer need the devices
	pcap_freealldevs(alldevices);

	return 1;
}
void Client::Init_Puncs()
{
	puncs = new char*[2];
	puncs[0] = "<c[]{},*,*,*,*,*,*,*,*,*,*,*,*,*,*,*,*>";
	puncs[1] = "<*,*,*,*,*,*,c,*,*,*,*,c,*,*,c,*,*>";
}
int Client::Start_thread()
{
	thread_handle = _beginthreadex(NULL, 0, Runthread, (void*)this, 0, &thread_id);
	if(thread_handle == 0) {
		cout << "Error creating thread.";
		return -1;
	}
	return 1;
}

int Client::ConnectServer()
{
	struct sockaddr_in local_sa_i;

	//Start up WSADATA
	WSADATA wsaData;
	if (WSAStartup(MAKEWORD(1, 1), &wsaData) != 0) {
		fprintf(stderr, "WSAStartup failed.\n");
		exit(1);
	}

	//Create the handles to the sockets we opened
	init_socket = (int)socket(PF_INET, SOCK_STREAM, 0);
	s_i.l_sockfd = (int)socket(PF_INET, SOCK_STREAM, 0);

	//If sockfd is less than zero, it didn't create a successful handle
	if(init_socket < 0) {
		printf("Unable to open socket.\n");
		return 0;
	}

	if(init_socket < 0) {
		printf("Unable to open new_socket.\n");
		return 0;
	}

	//Create the structure with necessary connection info
	local_sa_i.sin_family = AF_INET;
	local_sa_i.sin_port = htons(port);

	if (s_ip == "0.0.0.0")
	{
		char ac[80];
		if (gethostname(ac, sizeof(ac)) == SOCKET_ERROR) {
			cerr << "Error " << WSAGetLastError() << " when getting local host name." << endl;
			return 0;
		}

		struct hostent *phe = gethostbyname(ac);
		if (phe == 0) {
			cerr << "Yow! Bad host lookup." << endl;
			return 0;
		}

		struct in_addr addr;
		memcpy(&addr, phe->h_addr_list[0], sizeof(struct in_addr));
		local_sa_i.sin_addr.s_addr = addr.S_un.S_addr;
		unsigned char* c_str_ip = converti_to_b((int)local_sa_i.sin_addr.s_addr);
		stringstream ss;

		s_ip.clear();
		ss << (int)c_str_ip[0];
		ss << ".";
		ss << (int)c_str_ip[1];
		ss << ".";
		ss << (int)c_str_ip[2];
		ss << ".";
		ss << (int)c_str_ip[3];
		ss >> s_ip;
		
	} 
	else
		local_sa_i.sin_addr.s_addr = inet_addr(s_ip.c_str());
	
	memset(local_sa_i.sin_zero, '\0', sizeof local_sa_i.sin_zero);

	//Attempt to connect the socket with the port of the server
	if(connect(init_socket, (struct sockaddr*)&local_sa_i, sizeof(struct sockaddr)) < 0) {
		printf("Unable to connect to server.\n");
		return 0;
	} else {
		int s_r_len = 0;
		s_i.s_info = gethostbyname(s_ip.c_str());
		printf("Successfully connected to: %s:%i\n", s_i.s_info->h_name, port);

		//Send our name so that we may be identified more easily
		char* c_name = (char*)converti_to_b((int)strlen(name));
		while(s_r_len != -1 && s_r_len != 4)
			s_r_len += send(init_socket, c_name + s_r_len, 4 - s_r_len, 0);
		delete c_name;
		s_r_len = 0;
		while(s_r_len != -1 && s_r_len != (int)strlen(name))
			s_r_len += send(init_socket, name + s_r_len, (int)strlen(name) - s_r_len, 0);

		//Receive the new port that we should send data to
		unsigned char* c_new_port = new unsigned char[4];
		s_r_len = 0;
		while(s_r_len != -1 && s_r_len != 4)
			s_r_len += recv(init_socket, (char*)c_new_port + s_r_len, 4 - s_r_len, 0);
		
		//Convert the port to an int
		port = convertb_to_i(c_new_port);
		delete c_new_port;
		cout << "Received new port: " << port << " creating a new connection.\n";

		//Close the connection socket
		closesocket(init_socket);

		//Reset the connection information just incase it ever got changed
		local_sa_i.sin_family = AF_INET;
		local_sa_i.sin_port = htons(port);
		local_sa_i.sin_addr.s_addr = inet_addr(s_ip.c_str());
		memset(local_sa_i.sin_zero, '\0', sizeof local_sa_i.sin_zero);
	}

	//The server is out there, but it may not be accepting our connection quite yet
	//Check the server 10 times for connection
	for(int i = 0; i < 10; i++) {
		if(connect(s_i.l_sockfd, (struct sockaddr*)&local_sa_i, sizeof(struct sockaddr)) < 0) {
			cout << "Unable to connect to server with the new connection.  Trying again....\n";
			Sleep(1000);
		} else {
			connected = true;
			printf("Successfully connected to: %s:%i\n", s_i.s_info->h_name, port);
			send_Schema();
			return 1;
		}
	}

	cout << "Unable to connect to server.  Exiting.";
	return -1;
}
void Client::Start()
{
	if(ConnectServer())
	{
		if(Init_Devices())
			if(Start_thread())
			{
				//int i = 2000;
				//unsigned char* b_packet_l = new unsigned char[4], *new_data;
				////Temporary for testing
				//while(true)
				//{
				//	b_packet_l = converti_to_b(i);
				//	new_data = new unsigned char[i];
				//	int sent_len = send(s_i.l_sockfd, (char*)b_packet_l, 4, 0);
				//	while (sent_len != 4)
				//	{
				//		cout << "Didn't send full packet length: " << sent_len << ".  Resending.\n";
				//		sent_len += send(s_i.l_sockfd, (char*)b_packet_l + sent_len, 4 - sent_len, 0);
				//	}
				//	sent_len = send(s_i.l_sockfd, (char*)new_data, i, 0);
				//	while (sent_len != i)
				//	{
				//		cout << "Didn't send full packet: " << sent_len << ".  Resending.\n";
				//		sent_len += send(s_i.l_sockfd, (char*)new_data + sent_len, i - sent_len, 0);
				//	}
				//	::Sleep(1);
				//}

				//pcap_loop(p_i.adhandle, 0, packethandler, (u_char*)this);
				Init_Puncs();
				Start_Capture();
			}
	}
	else
	{
		cout << "Error in Client::Start().";
	}
}
int Client::send_Schema()
{
	//Time - sec
	send_Attr(TSEC_START, TSEC_STOP, "Time: sec");								//1
	//Time - usec
	send_Attr(TUSEC_START, TUSEC_STOP, "Time: micro-sec");						//2
	//Ethernet header
	send_Attr(EH_START, EH_STOP, "Ethernet Header");							//3
	//Version + IHL
	send_Attr(VIHL, VIHL, "Version and Internet-header length");				//4
	//Type of service
	send_Attr(TOS, TOS, "Type of service");										//5
	//Total length
	send_Attr(TL_START, TL_STOP, "Total length");								//6
	//Identification
	send_Attr(ID_START, ID_STOP, "Identification");								//7
	//Flags and fragment offset
	send_Attr(FLFRAG_START, FLFRAG_STOP, "Flags and fragment offset");			//8
	//Time to live
	send_Attr(TTL, TTL, "Time to live");										//9
	//Protocol
	send_Attr(PROTO, PROTO, "Protocol");										//10
	//Header checksum
	send_Attr(HCSUM_START, HCSUM_STOP, "Header checksum");						//11
	//Source IP
	send_Attr(SIP_START, SIP_STOP, "Source IP");								//12
	//Dest IP
	send_Attr(DIP_START, DIP_STOP, "Dest IP");									//13
	//Options and padding
	send_Attr(OP_START, OP_STOP, "Options and padding");						//14
	//TCP only - replace with a better way
	//Source port
	send_Attr(TCP_SP_START, TCP_SP_STOP, "TCP: Source port");					//15
	//Dest port
	send_Attr(TCP_DP_START, TCP_DP_STOP, "TCP: Dest port");						//16
	//Sequence number
	send_Attr(SQ_START, SQ_STOP, "TCP: Sequence number");						//17
	//17 Attributes so far

	//Send a zero byte[] telling we're done
	send_Attr(0, 0, "We're done!");

	return 1;
}
int Client::send_Attr(int start, int finish, char* attr_name)
{
	char* pos = new char[2];
	int len = (int)strlen(attr_name);
	char* b_len = new char[4];
	b_len = (char*)converti_to_b(len);

	pos[0] = start;
	pos[1] = finish;

	int sent_len = 0;

	while(sent_len != -1 && sent_len != 2)
		sent_len += send(s_i.l_sockfd, pos+sent_len, 2-sent_len, 0);

	sent_len = 0;
	while(sent_len != -1 && sent_len != 4)
		sent_len += send(s_i.l_sockfd, b_len+sent_len, 4-sent_len, 0);

	sent_len = 0;
	while(sent_len != -1 && sent_len != len)
		sent_len += send(s_i.l_sockfd, attr_name+sent_len, len-sent_len, 0);

	delete pos;
	delete b_len;

	return 1;
}

//Packet Handling
int Client::CompileFilter(char* filter)
{
	//Compile the new filter
	int compile = pcap_compile(p_i.adhandle, &(p_i.fcode), filter, 1, p_i.netmask);
	if (compile < 0) {
		cout << "\nUnable to compile packet filter.  Check syntax.\n";
		recompile = false;
		return -1;
	}
	else {
		cout << "Compiling filter.\n";
		//Set the new filter
		if (pcap_setfilter(p_i.adhandle, &(p_i.fcode)) < 0) {
			cout << "\nError setting the filter\n";
			recompile = false;
			return -1;
		}
		else
			cout << "Setting filter.\n";
	}
	recompile = false;

	//delete filter;

	return 1;
}
void Client::Start_Capture()
{
	struct pcap_pkthdr *header;		//Header defined by pcap for the data
	const unsigned char *pkt_data;		//Actual packet data
	unsigned char* b_packet_l = new unsigned char[4];			//Length of the packet in bytes
	unsigned char* t = new unsigned char[4];					//Used in converting the timestamp to bytes
	ip_header *ih;						//The IP header
	address_header *ah;					//The defined protocol header
	u_int ip_len;						//IPHeader length
//	u_short sport/*, dport*/;			//Source, destination port numbers	
	time_t sys_time;					//System time
	bool found_packet = false;			//Did we get a packet - used for the flag punctuation

	while(connected)
	{
		//Send a packet?
		if(send_data) {
			if(pcap_next_ex(p_i.adhandle, &header, &pkt_data) == 1)
			{
				found_packet = true;

				//Reallocate a bigger buffer if needed
				if(header->len+TIME_OFFSET > nd_length) {
					delete new_data;
					new_data = new unsigned char[header->len+TIME_OFFSET+FLAG_OFFSET];
					nd_length = header->len+TIME_OFFSET+FLAG_OFFSET;
				}

				//Get packet length
				//b_packet_l = converti_to_b(header->len+TIME_OFFSET+FLAG_OFFSET);
				int total_len = header->len+TIME_OFFSET+FLAG_OFFSET;
				b_packet_l[3] = (char)(total_len >> 24);
				total_len -= (int)(b_packet_l[3] << 24);
				b_packet_l[2] = (char)(total_len >> 16);
				total_len -= (int)(b_packet_l[2] << 16);
				b_packet_l[1] = (char)(total_len >> 8);
				total_len -= (int)(b_packet_l[1] << 8);
				b_packet_l[0] = (char)total_len;

				//Get the timestamp
				//t = converti_to_b((int)header->ts.tv_sec);
				int temp_time = header->ts.tv_sec;
				t[3] = (char)(temp_time >> 24);
				temp_time -= (int)(t[3] << 24);
				t[2] = (char)(temp_time >> 16);
				temp_time -= (int)(t[2] << 16);
				t[1] = (char)(temp_time >> 8);
				temp_time -= (int)(t[1] << 8);
				t[0] = (char)temp_time;
				memcpy(new_data+FLAG_OFFSET, t, TIME_OFFSET-4);
				//memcpy(timestamp, t, 4);
				//delete t;
				//t = converti_to_b((int)header->ts.tv_usec);
				temp_time = header->ts.tv_usec;
				t[3] = (char)(temp_time >> 24);
				temp_time -= (int)(t[3] << 24);
				t[2] = (char)(temp_time >> 16);
				temp_time -= (int)(t[2] << 16);
				t[1] = (char)(temp_time >> 8);
				temp_time -= (int)(t[1] << 8);
				t[0] = (char)temp_time;
				memcpy(new_data+FLAG_OFFSET+4, t, TIME_OFFSET-4);
				//memcpy(timestamp+4,t , 4);
				//delete t;

				new_data[0] = 0;
				//memcpy(new_data+FLAG_OFFSET, timestamp, TIME_OFFSET);
				memcpy(new_data+TIME_OFFSET+FLAG_OFFSET, pkt_data, header->len);

				//Retrieve position of IP header
				ih = (ip_header *) (pkt_data + 14);		//14 = length of ethernet header

				//Retrieve position of address
				ip_len = (ih->ver_ihl & 0xf) * 4;
				ah = (address_header *) ((u_char*)ih + ip_len);

				//cout << "SIP: " << (int)ih->saddr.byte1 << (int)ih->saddr.byte2 << (int)ih->saddr.byte3 << (int)ih->saddr.byte4;
				//cout << "\tDIP: " << (int)ih->daddr.byte1 << (int)ih->daddr.byte2 << (int)ih->daddr.byte3 << (int)ih->daddr.byte4 << endl;

				int sent_len = send(s_i.l_sockfd, (char*)b_packet_l, 4, 0);
				while (sent_len != -1 && sent_len < 4)
				{
					cout << "Didn't send full packet length: " << sent_len << ".  Resending.\n";
					sent_len += send(s_i.l_sockfd, (char*)b_packet_l + sent_len, 4 - sent_len, 0);
				}
				sent_len = send(s_i.l_sockfd, (char*)new_data, (int)header->len+TIME_OFFSET+FLAG_OFFSET, 0);
				while (sent_len != -1 && sent_len < (int)header->len+TIME_OFFSET+FLAG_OFFSET)
				{
					cout << "Didn't send full packet: " << sent_len << ".  Resending.\n";
					sent_len += send(s_i.l_sockfd, (char*)new_data + sent_len, (int)header->len+TIME_OFFSET+FLAG_OFFSET - sent_len, 0);
				}

				//Clean up
				//delete b_packet_l;
			}
			else
				found_packet = false;

			//Send a punctuation?
			if((punc_flag & 0xff) != 0)
			{
				//send sec punctuation
				if (punc_flag & PUNC_TIME_SEC)
				{
					time(&sys_time);
					for(unsigned int i = 0; i < sec_punc_v.size(); i++)
					{
						//Factor
						int fac = sec_punc_v.at(i).Get_Pattern(0).Get_Factor();
						//Pattern Type
						int pat = sec_punc_v.at(i).Get_Pattern(0).Get_Pattern_Type();
						//Literal
						if(pat == LITERAL_PATTERN)
						{
							//cout << endl << sec_punc_v.at(i).Get_Pattern(0).Get_Lit_Val()/fac << " " << (sys_time - 5)/fac << endl;
							if(sec_punc_v.at(i).Get_Pattern(0).Get_Lit_Val()/fac < (sys_time - 5)/fac)
							{
								sec_punc_v.at(i).Send(s_i.l_sockfd, punc_data);
								cout << "Sent punctuation: " << sec_punc_v.at(i).Get_Pattern(0).Get_Lit_Val()/fac << endl;
								cout << "Current Punctuation: " << sys_time/fac << endl;
								sec_punc_v.at(i).Set_Pattern_Val(0, (void*)&sys_time);
							}
						}
						//Range
						else if(pat == RANGE_PATTERN)
						{
							const int *range_val = sec_punc_v.at(i).Get_Pattern(0).Get_Ran_Val();
							if(range_val[1] < (sys_time - 5))
							{
								sec_punc_v.at(i).Send(s_i.l_sockfd, punc_data);
								cout << "Current Punctuation: " << sys_time << " to " << (int)sys_time + fac << endl;
								cout << "Sent punctuation: " << range_val[0] << " to " << range_val[1] << endl;
								sec_punc_v.at(i).Set_Pattern_Val(0, (void*)&sys_time);
							}
						}
						//List
						else if(pat == LIST_PATTERN && found_packet)
						{
							vector<int> *not_seen = sec_punc_v.at(i).Get_Pattern(0).Get_List_Not_Seen();
							if(not_seen->empty())
							{
								sec_punc_v.at(i).Send(s_i.l_sockfd, punc_data);
								punc_flag &= !PUNC_TIME_SEC;
								vector<Punctuation>::iterator iter = sec_punc_v.begin();
								iter += 5;
								sec_punc_v.erase(iter);
							}
							else
							{
								int sec = (int)sys_time;
								if(sec == not_seen->front())
								{
									not_seen->erase(not_seen->begin());
								}
							}
						}
					}
				}
				//send frag punctuation
				if(punc_flag & PUNC_FRAG && found_packet)
				{
					for(unsigned int i = 0; i < frag_punc_v.size(); i++)
					{
						//If the DNF is not set and the MF is not set - there is an end of fragment packet
						if(!(ih->flags_fo & 0x01000000b) && !(ih->flags_fo & 0x00100000b))
						{
							unsigned int values = ih->identification;
							frag_punc_v[i].Set_Pattern_Val(6, (void*)&values);
							values = convertb_to_i(new_data+SIP_STOP);
							frag_punc_v[i].Set_Pattern_Val(11, (void*)&values);
							values = ah->sport;
							frag_punc_v[i].Set_Pattern_Val(14, (void*)&values);
							frag_punc_v[i].Send(s_i.l_sockfd, punc_data);
						}
					}
				}
			}

			//Recompile the filter?
			if(recompile) {
				CompileFilter(p_i.pcap_filter);
			}
		}
		else
		{
			::Sleep(10);
		}
	} //end while
}
//Pcap_loop call function - not used -> pcap_next_ex is now used
void packethandler(u_char *param, const struct pcap_pkthdr *header, const u_char *pkt_data) {
	Client *c = (Client*)(param);

	if(c->send_data) {
		unsigned char* b_packet_l;
		unsigned char* t;
		ip_header *ih;			//Whats in the header
		address_header *ah;		//Addresses
		u_int ip_len;			//IPHeader length
//		u_short sport, dport;	//Source, destination port numbers
		//char* thechar = new char[8];
		//string packet_info;

		//cout << "Len: " << header->len << endl;
		b_packet_l = converti_to_b(header->len+TIME_OFFSET+FLAG_OFFSET);
		t = converti_to_b((int)header->ts.tv_sec);
//		memcpy(c->timestamp, t, 4);
		delete t;
		t = converti_to_b((int)header->ts.tv_usec);
//		memcpy(c->timestamp+4,t , 4);
		delete t;

		//cout << "Timestamp: " << header->ts.tv_sec << endl;

		if(header->len+TIME_OFFSET > c->nd_length) {
			c->new_data = new unsigned char[header->len+TIME_OFFSET+FLAG_OFFSET];
			c->nd_length = header->len+TIME_OFFSET+FLAG_OFFSET;
		}
		
		c->new_data[0] = 0;
//		memcpy(c->new_data+FLAG_OFFSET, c->timestamp, TIME_OFFSET);
		memcpy(c->new_data+TIME_OFFSET+FLAG_OFFSET, pkt_data, header->len);

		if(c->recompile) {
			c->CompileFilter(c->p_i.pcap_filter);
		}

		//Retrieve position of IP header
		ih = (ip_header *) (pkt_data + 14);		//14 = length of ethernet header

		//Retrieve position of address
		ip_len = (ih->ver_ihl & 0xf) * 4;
		ah = (address_header *) ((u_char*)ih + ip_len);

		//Convert from network byte order to host byte order
//		sport = ntohs( ah->sport );
		//dport = ntohs( ah->dport );

		//Convert timestamp to readable format
		//ltime = _localtime64((__time64_t*)&header->ts.tv_sec);
		//strftime(timestr, sizeof timestr, "%H:%M:%S", ltime);

		//packet_info = "Length: ";
		//packet_info += _itoa(header->len, thechar, 10);
		//packet_info += "\tProtocol: ";
		//packet_info += _itoa(ih->proto, thechar, 10);
		//packet_info += "\tAddr: ";
		//packet_info += _itoa(ih->saddr.byte1, thechar, 10);
		//packet_info += ".";
		//packet_info += _itoa(ih->saddr.byte2, thechar, 10);
		//packet_info += ".";
		//packet_info += _itoa(ih->saddr.byte3, thechar, 10);
		//packet_info += ".";
		//packet_info += _itoa(ih->saddr.byte4, thechar, 10);
		//packet_info += ".";
		//packet_info += _itoa(sport, thechar, 10);
		//packet_info += "\t -> ";
		//packet_info += _itoa(ih->daddr.byte1, thechar, 10);
		//packet_info += ".";
		//packet_info += _itoa(ih->daddr.byte2, thechar, 10);
		//packet_info += ".";
		//packet_info += _itoa(ih->daddr.byte3, thechar, 10);
		//packet_info += ".";
		//packet_info += _itoa(ih->daddr.byte4, thechar, 10);
		//packet_info += ".";
		//packet_info += _itoa(dport, thechar, 10);
		//packet_info += "\n";

		//cout << packet_info;

		int sent_len = send(c->s_i.l_sockfd, (char*)b_packet_l, 4, 0);
		while (sent_len != 4)
		{
			cout << "Didn't send full packet length: " << sent_len << ".  Resending.\n";
			sent_len += send(c->s_i.l_sockfd, (char*)b_packet_l + sent_len, 4 - sent_len, 0);
		}
		sent_len = send(c->s_i.l_sockfd, (char*)c->new_data, (int)header->len+TIME_OFFSET+FLAG_OFFSET, 0);
		while (sent_len != (int)header->len+TIME_OFFSET+FLAG_OFFSET)
		{
			cout << "Didn't send full packet: " << sent_len << ".  Resending.\n";
			sent_len += send(c->s_i.l_sockfd, (char*)c->new_data + sent_len, (int)header->len+TIME_OFFSET+FLAG_OFFSET - sent_len, 0);
		}

		//delete thechar;
		delete b_packet_l;

		////send sec punctuation
		//if ((punc_flag & PUNC_TIME_SEC))
		//{


		//}
		////send usec punctuation
		//if((punc_flag & PUNC_TIME_USEC))
		//{

		//}
		////send frag punctuation
		//if((punc_flag & PUNC_FRAG))
		//{

		//}
		//if((c->punc_cur_time[0]/60) < ((header->ts.tv_sec - 5)/60))
		//{
		//	c->punc_cur_time[0] = (header->ts.tv_sec);
		//	//c->byte_send_punc(c->cur_time[0]);
		//	c->pstr_send_sec(c->punc_cur_time[0] - 60);
		//	cout << "Sent punctuation: " << c->punc_cur_time[0]/60 - 1 << endl;
		//}
	}
}





//Dealing with Punctuations
void Client::byte_send_punc(long ts)
{
	//Make the flag part of the punctuation 1
	punc_data[0] = 1;
	//The first attr is a literal pattern
	punc_data[1] = LITERAL_PATTERN;
	unsigned char* c_punc = converti_to_b((int)ts);
	unsigned char* punc_len = converti_to_b(22);
	punc_data[2] = c_punc[0];
	punc_data[3] = c_punc[1];
	punc_data[4] = c_punc[2];
	punc_data[5] = c_punc[3];
	memset(punc_data + 6, 1, 16);
	//Send the punctuation
	send(s_i.l_sockfd, (char*)punc_len, 4, 0);
	send(s_i.l_sockfd, (char*)punc_data, 22, 0);

	delete c_punc;
	delete punc_len;
}
void Client::pstr_send_sec(long ts)
{
	int index = 0;
	//Make the flag part of the punctuation 1
	punc_data[index++] = 1;
	//The first attr is a literal pattern
	punc_data[index++] = '<';
	punc_data[index++] = 'c';
	unsigned char* c_punc = converti_to_b((int)ts);
	punc_data[index++] = c_punc[0];
	punc_data[index++] = c_punc[1];
	punc_data[index++] = c_punc[2];
	punc_data[index++] = c_punc[3];
	for(int i = 0; i < 16; i++)
	{
		punc_data[index++] = ',';
		punc_data[index++] = '*';
	}
	punc_data[index++] = '>';
	unsigned char* punc_len = converti_to_b(index);
	//Send the punctuation
	int sent_len = send(s_i.l_sockfd, (char*)punc_len, 4, 0); 
	while (sent_len != 4)
	{
		cout << "Didn't send full punctuation length: " << sent_len << ".  Resending.\n";
		sent_len += send(s_i.l_sockfd, (char*)punc_len + sent_len, 4 - sent_len, 0); 
	}
	sent_len = send(s_i.l_sockfd, (char*)punc_data, index, 0);
	while (sent_len != index)
	{
		cout << "Didn't send full punctuation: " << sent_len << ".  Resending.\n";
		sent_len += send(s_i.l_sockfd, (char*)punc_data + sent_len, index - sent_len, 0);
	}

	delete c_punc;
	delete punc_len;
}


//Thread call function
unsigned __stdcall Runthread(void * param) {
	//Sets that contain the socket
	Client *c = (Client*)(param);
	fd_set readfds, master, except;
	//Timeout for select if it doesn't find anything
	timeval time_out;
	time_out.tv_sec = 5;
	time_out.tv_usec = 0;

	//Initialize the sets to 0
	FD_ZERO(&readfds);
	FD_ZERO(&master);
	FD_ZERO(&except);

	//Place the socket in the master set
	FD_SET(c->s_i.l_sockfd, &master);

	unsigned char* c_len = new unsigned char[5];

	while(c->connected) {
		//Set the read set to the master set
		readfds = master;
		except = master;
		//Determing if the socket as anything to read
		select((c->s_i.l_sockfd + 1), &readfds, NULL, &except, &time_out);
		
		if(FD_ISSET(c->s_i.l_sockfd, &except))
		{
			exit(0);
		}

		//If socket contains data - recv it
		if(FD_ISSET(c->s_i.l_sockfd, &readfds)) {
			//Receive the length of the packet
			if(recv(c->s_i.l_sockfd, (char*)c_len, 4, MSG_WAITALL) == -1)
				exit(0);

			//Convert the char* to an int
			int len = convertb_to_i(c_len);

			//If the length = 0, then we're getting a signal
			if(len == 0) {
				recv(c->s_i.l_sockfd, (char*)c_len, 1, 0);
				//Convert the char* to an int
				len = (int)(c_len[0]);

				if(len == 0)
					c->send_data = false;
				else if(len == 1)
				{
					c->send_data = true;
				}
				else
				{
					c->send_data = true;
					
					//Sec punctuation
					if(len == PUNC_TIME_SEC)
					{
						time_t t_i = time(NULL);
						Punctuation p;
						c->punc_flag |= len;

						while(true)
						{
							int rec_len;
							//Get the type of pattern
							recv(c->s_i.l_sockfd, (char*)c_len, 1, 0);
							//cout << (int)c_len[0] << endl;
							if(c_len[0] == 0)
								break;

							if(c_len[0] == WILDCARD_PATTERN)
							{
								p.Add_Pattern(WILDCARD_PATTERN, NULL, 0);
								//cout << "Added a WC pattern\n";
							}
							//Literal pattern
							else if(c_len[0] == LITERAL_PATTERN)
							{
								rec_len = recv(c->s_i.l_sockfd, (char*)c_len, 5, MSG_WAITALL);
								while(rec_len != 5)
								{
									cout << "Still didn't waitall\n";
									rec_len += recv(c->s_i.l_sockfd, (char*)c_len + rec_len, 5 - rec_len, MSG_WAITALL);
									if(rec_len == CLOSED_CONNECTION)
											c->connected = false;
								}
								//cout << rec_len << endl;
								p.Add_Pattern(LITERAL_PATTERN, (void*)&t_i, (int)c_len[0]);
								p.Set_Last_Pattern_Fac(convertb_to_i(c_len+1));
								//cout << "Added a Literal pattern\n";

							}
							//Range pattern
							else if(c_len[0] == RANGE_PATTERN)
							{
								rec_len = recv(c->s_i.l_sockfd, (char*)c_len, 5, MSG_WAITALL);
								while(rec_len != 5)
								{
									rec_len += recv(c->s_i.l_sockfd, (char*)c_len + rec_len, 5 - rec_len, MSG_WAITALL);
									if(rec_len == CLOSED_CONNECTION)
											c->connected = false;
								}
								int factor = convertb_to_i(c_len+1);
								int range_val[2] = {(int)t_i, (int)t_i + factor};
								p.Add_Pattern(RANGE_PATTERN, (void*)range_val, (int)c_len[0]);
								p.Set_Last_Pattern_Fac(factor);
								cout << "Added a Range pattern\n";
							}
							//List pattern
							else if(c_len[0] == LIST_PATTERN)
							{
								int sec = (int)t_i;
								vector<int> l_v;
								rec_len = recv(c->s_i.l_sockfd, (char*)c_len, 5, MSG_WAITALL);
								while(rec_len != 5)
								{
									rec_len += recv(c->s_i.l_sockfd, (char*)c_len + rec_len, 5 - rec_len, MSG_WAITALL);
									if(rec_len == CLOSED_CONNECTION)
											c->connected = false;
								}
								int val_type = (int)c_len[0];
								int num_vals = convertb_to_i(c_len+1);
								for(int i = 0; i < num_vals; i++)
								{
									rec_len = recv(c->s_i.l_sockfd, (char*)c_len, 4, MSG_WAITALL);
									while(rec_len != 4)
									{
										rec_len += recv(c->s_i.l_sockfd, (char*)c_len + rec_len, 4 - rec_len, MSG_WAITALL);
										if(rec_len == CLOSED_CONNECTION)
											c->connected = false;
									}
									int val = convertb_to_i(c_len);
									if(val > sec)
										l_v.push_back(val);
								}
								p.Add_Pattern(LIST_PATTERN, (void*)&l_v, val_type);
								cout << "Added a List pattern\n";
							}
						}
						p.Finalize_Punc();
						c->sec_punc_v.push_back(p);
						cout << "Added punctuation: Timestamp: Sec\n";

					}
					else if(len == PUNC_FRAG)
					{
						Punctuation p;
						p.Fill_WC(6);
						int temp = 0;
						p.Add_Pattern(LITERAL_PATTERN, (void*)&temp, 2);
						p.Fill_WC(4);
						p.Add_Pattern(LITERAL_PATTERN, (void*)&temp, 4);
						p.Fill_WC(2);
						p.Add_Pattern(LITERAL_PATTERN, (void*)&temp, 2);
						p.Fill_WC(2);
						p.Finalize_Punc();
						c->sec_punc_v.push_back(p);
						cout << "Added punctuation: Fragment\n";
					}
				}
				len = 0;
			}

			//If there is a message - receive it
			if(len > 0) {
				char *buffer = new char[len];

				//Receive the message - MSG_WAITALL will wait until the entire message is received
				int rec_len = 0;
				while(c->connected && rec_len != len)
				{
					rec_len += recv(c->s_i.l_sockfd, buffer+rec_len, len - rec_len, MSG_WAITALL);
					if(rec_len == CLOSED_CONNECTION)
						c->connected = false;
				}

				cout << "Received new filter: " << buffer << endl;

				c->p_i.pcap_filter = buffer;
				c->recompile = true;
				delete buffer;
			}
		}
		//Put to sleep for 5 seconds, don't need to check that often
		::Sleep(5000);
	}
	delete c_len;
	return NULL;
}



