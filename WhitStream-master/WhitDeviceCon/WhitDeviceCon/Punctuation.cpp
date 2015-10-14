//Implementation of Punctuation.h
#include "Punctuation.h"
//using namespace std;

//Implementation of class: Pattern
//Get/Set functions
const int Pattern::Get_Pattern_Type()
{
	return pattern_type;
}

const int Pattern::Get_Lit_Val()
{
	if(pattern_type == LITERAL_PATTERN)
		return lit_val;
	else
		return NULL;
}

const int* Pattern::Get_Ran_Val()
{
	if(pattern_type == RANGE_PATTERN)
	{
		const int *r_vs = ran_vals;
		return r_vs;
	}
	else
		return NULL;
}
const vector<int> Pattern::Get_List_Val()
{
	if(pattern_type == LIST_PATTERN)
		return list_vals;
	else
	{
		vector<int> nothing;
		return nothing;
	}
}

void Pattern::Set_Val(void* vals)
{
	if(pattern_type == LITERAL_PATTERN)
	{
		lit_val = *((int*)vals);
	}
	else if(pattern_type == RANGE_PATTERN)
	{
		ran_vals[0] = *((int*)vals);
		ran_vals[1] = ran_vals[0] + factor;
	}
	else if(pattern_type == LIST_PATTERN)
	{
		list_vals = *((vector<int>*)vals);
	}
}
const int Pattern::Get_Value_Type()
{
	return value_type;
}
const int Pattern::Get_Factor()
{
	return factor;
}
void Pattern::Set_Factor(int f)
{
	if(pattern_type == LITERAL_PATTERN)
	{
		factor = f;
	}
	else if(pattern_type == RANGE_PATTERN)
	{
		factor = f;
	}
	else if(pattern_type == LIST_PATTERN)
	{
		cout << "\nWhy are you changing the factor in a List?\n";
	}
}
vector<int>* Pattern::Get_List_Not_Seen()
{
	if(pattern_type == LIST_PATTERN)
		return &list_not_seen;
	else
		return NULL;
}
//Constuctors
Pattern::~Pattern()
{
	list_vals.~vector();
	list_not_seen.~vector();
}
//Wildcard constructor
Pattern::Pattern()
{
	pattern_type = WILDCARD_PATTERN;
}
//Literal constructor
Pattern::Pattern(int l_v, int type)
{
	pattern_type = LITERAL_PATTERN;
	value_type = type;
	lit_val = l_v;
}
//Range constructor
Pattern::Pattern(int *r_g, int type)
{
	pattern_type = RANGE_PATTERN;
	value_type = type;
	ran_vals[0] = r_g[0];
	ran_vals[1] = r_g[1];
}
//List constructor
Pattern::Pattern(vector<int> l_v_v, int type)
{
	pattern_type = LIST_PATTERN;
	value_type = type;
	list_vals = l_v_v;
	std::sort(list_vals.begin(), list_vals.end());
	list_vals.resize(list_vals.size());
	for(unsigned int i = 0; i < list_vals.size(); i++)
	{
		list_not_seen.push_back(list_vals[i]);
	}
	list_not_seen.resize(list_not_seen.size());
}

//Implementation of class: Punctuation
Punctuation::~Punctuation()
{
	for(unsigned int i = 0; i < p.size(); i++)
	{
		p[i].~Pattern();
	}
	p.~vector();
}
void Punctuation::Add_Pattern(int pattern_type, void* vals, int val_type)
{
	//Add a WC pattern
	if(pattern_type == WILDCARD_PATTERN)
	{
		p.push_back(WC);
	}
	else if(pattern_type == LITERAL_PATTERN)
	{
		Pattern L_P(*((int*)vals), val_type);
		p.push_back(L_P);
	}
	else if(pattern_type == RANGE_PATTERN)
	{
		Pattern R_P((int*)vals, val_type);
		p.push_back(R_P);
	}
	else if(pattern_type == LIST_PATTERN)
	{
		//Will this still hold all the information for the vector?
		Pattern L_P(*((vector<int>*)vals), val_type);
		p.push_back(L_P);
	}
}

void Punctuation::Set_Pattern_Val(int position, void* param)
{
	if(p.at(position).Get_Pattern_Type() != WILDCARD_PATTERN)
	{
		p.at(position).Set_Val(param);
	}
}
Pattern Punctuation::Get_Pattern(int position)
{
	return p.at(position);
}
Pattern Punctuation::Get_Last_Pattern()
{
	return p.back();
}
void Punctuation::Set_Last_Pattern_Fac(int f)
{
	p.back().Set_Factor(f);
}
void Punctuation::Finalize_Punc()
{
	p.resize(p.size());
}
void Punctuation::Fill_WC(int num_of_WC)
{
	for(int i = 0; i < num_of_WC; i++)
	{
		p.push_back(WC);
	}
}
void Punctuation::Send(int sock_handle, unsigned char* punc_data)
{
	int index = 0;
	unsigned char* c_punc;

	//Make the flag part of the punctuation 1
	punc_data[index++] = 1;
	//The first attr is a literal pattern
	punc_data[index++] = '<';
	//Go through the entire punctuation, converting it all to a string
	for(unsigned int i = 0; i < p.size(); i++)
	{
		//Don't add a comma at the very beginning
		if(i != 0)
			punc_data[index++] = ',';

		Pattern pat = p.at(i);
		int p_t = pat.Get_Pattern_Type();
		if(p_t == WILDCARD_PATTERN)
		{
			punc_data[index++] = '*';
		}
		else if(p_t == LITERAL_PATTERN)
		{
			punc_data[index++] = 'c';
			c_punc = converti_to_b(pat.Get_Lit_Val());
			for(int j = 0; j < pat.Get_Value_Type(); j++)
			{
				punc_data[index++] = c_punc[j];
			}
		}
		else if(p_t == RANGE_PATTERN)
		{
			punc_data[index++] = '[';
			const int* r_v = pat.Get_Ran_Val();
			c_punc = converti_to_b(r_v[0]);

			//Add the minimum value
			for(int j = 0; j < pat.Get_Value_Type(); j++)
			{
				punc_data[index++] = c_punc[j];
			}

			//Add the maximum value
			c_punc = converti_to_b(r_v[1]);
			for(int j = 0; j < pat.Get_Value_Type(); j++)
			{
				punc_data[index++] = c_punc[j];
			}
			punc_data[index++] = ']';
		}
		else if(p_t == LIST_PATTERN)
		{
			punc_data[index++] = '{';
			const vector<int> l_v = pat.Get_List_Val();

			for(unsigned int num = 0; num < l_v.size(); num++)
			{
				c_punc = converti_to_b(l_v.at(num));

				for(int j = 0; j < pat.Get_Value_Type(); j++)
				{
					punc_data[index++] = c_punc[j];
				}
			}
			punc_data[index++] = '}';
			l_v.~vector();
		}
	}
	punc_data[index++] = '>';
	unsigned char* punc_len = converti_to_b(index);
	//for(int i = 0; i <= index; i++)
	//	cout << punc_data[i];

	//Send the punctuation
	int sent_len = send(sock_handle, (char*)punc_len, 4, 0); 
	while (sent_len != -1 && sent_len != 4)
	{
		cout << "Didn't send full punctuation length: " << sent_len << ".  Resending.\n";
		sent_len += send(sock_handle, (char*)punc_len + sent_len, 4 - sent_len, 0); 
	}
	sent_len = send(sock_handle, (char*)punc_data, index, 0);
	while (sent_len != -1 && sent_len != index)
	{
		cout << "Didn't send full punctuation: " << sent_len << ".  Resending.\n";
		sent_len += send(sock_handle, (char*)punc_data + sent_len, index - sent_len, 0);
	}

	delete c_punc;
	delete punc_len;
}
Punctuation Punctuation::operator=(const Punctuation rhs)
{
	//Shallow copy
	return rhs;
}