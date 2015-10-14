//Standard includes/custom defines
#include "Utility.h"
using namespace std;

class Pattern
{
//Private variables
private:
	//Holds the literal value
	int lit_val;
	//Holds the range values
	//0=min, 1=max
	int ran_vals[2];
	//Holds the list of values	
	vector<int> list_vals, list_not_seen;
	//Pattern: wildcard=1, literal=2, range=3, list=4
	int pattern_type;
	//Value type: 1=byte, 2=short, 4=int
	int value_type;
	//Used by literal and range to increment the values - not necessarily used
	int factor;

//Public variables
public:

//Private functions
private:

//Public functions
public:
	//Get/Set functions
	const int Get_Lit_Val();
	const int* Get_Ran_Val();
	const vector<int> Get_List_Val();
	void Set_Val(void* vals);
	const int Get_Pattern_Type();
	const int Get_Value_Type();
	const int Get_Factor();
	void Set_Factor(int f);
	vector<int>* Get_List_Not_Seen();


	//Constructors/Deconstructor
	Pattern();
	~Pattern();
	//Literal constructor
	Pattern(int l_v, int type);
	//Range constructor
	Pattern(int *r_g, int type);
	//List constructor
	Pattern(vector<int> l_v_v, int type);
};

class Punctuation
{
//private variables
private:
	//Holds all the patterns that make up this punctuation
	vector<Pattern> p;
	//Standard WC pattern
	const Pattern WC;

//public variables
public:

//private functions
private:

//public functions
public:
	//Add a pattern to the punctuation
	void Add_Pattern(int pattern_type, void* vals, int val_type);
	void Create_Pattern(int pattern_type, int fac, int val_type);
	//Sets a pattern's value, granted it is the right type of value
	void Set_Pattern_Val(int position, void* param);
	void Set_Last_Pattern_Fac(int f);
	//Get a pattern by a position
	Pattern Get_Pattern(int position);
	//Get the last pattern added to the punctuation
	Pattern Get_Last_Pattern();
	//Make the vector the final size of the punctuation
	void Finalize_Punc();
	//Fills the punctuation with the specified number of WCs
	void Fill_WC(int num_of_WC);
	//Sends the punctuation to the given socket handle and a string
	//buffer that it can use
	void Send(int sock_handle, unsigned char* punc_data);

	//Constructor/Deconstructor
	Punctuation() {}
	~Punctuation();

	//Operators
	Punctuation operator=(const Punctuation rhs);
};