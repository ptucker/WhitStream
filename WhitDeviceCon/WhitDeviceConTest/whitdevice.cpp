#include "client.h"
using namespace std;

int main()
{
	cout << "Enter the IP of the server. Enter 0.0.0.0 for the local IP: ";
	string ip = "0.0.0.0";
	cin >> ip;

	Client c("WhitDevice", ip, 4000, "tcp");

	c.Start();
	return 0;
}