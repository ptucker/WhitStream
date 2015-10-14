#include "Utility.h"

//Converters
//Causes memory leaks
unsigned char* converti_to_b(int i)
{
	unsigned char* b = new unsigned char[4];
	b[3] = (char)(i >> 24);
	i -= (int)(b[3] << 24);
	b[2] = (char)(i >> 16);
	i -= (int)(b[2] << 16);
	b[1] = (char)(i >> 8);
	i -= (int)(b[1] << 8);
	b[0] = (char)i;

	return b;
}

//Memory leak safe
void converti_to_b_r(int i, unsigned char* b)
{
	b[3] = (char)(i >> 24);
	i -= (int)(b[3] << 24);
	b[2] = (char)(i >> 16);
	i -= (int)(b[2] << 16);
	b[1] = (char)(i >> 8);
	i -= (int)(b[1] << 8);
	b[0] = (char)i;
}
//Memory leak safe
int convertb_to_i(unsigned char* b)
{
	int i = ((int)b[3] << 24) | ((int)b[2] << 16) | ((int)b[1] << 8) | (int)b[0];
	return i;
}


