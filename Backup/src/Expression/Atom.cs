/*
 * Created by SharpDevelop.
 * User: Pete
 * Date: 12/30/2005
 * Time: 4:59 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using WhitStream.Data;

namespace WhitStream.Expression
{
	#region IAtom
	/// <summary>
	/// Interface to model Atom objects in an expression (Constants and Values)
	/// </summary>
	interface IAtom : IExpr {
		/// <summary>
		/// Function to get the value of this specific atom object
		/// </summary>
		/// <param name="dis">The current data items</param>
		/// <returns>The value of this atom</returns>
		/// <seealso cref="Data.DataItem" />
		object Val(params DataItem[] dis);
	}
	#endregion

	#region Constant
	/// <summary>
	/// Class to model constants in an expression
	/// </summary>
	/// <seealso cref="IAtom" />
	/// <seealso cref="IExpr" />
	class Constant : IAtom {
		/// <summary>
		/// The value of this atom
		/// </summary>
		private object constVal = null;
		
		

		/// <summary>
		/// Default constructor for creating an empty Constant object
		/// </summary>
		public Constant() {}

		/// <summary>
		/// Constructor for creating a Constant object with a specific object.
		/// If the object is a string prefaced by ', then it is a string object
		/// If the object is a string prefaced by B, then it is a Byte object
		/// If the object is a string prefaced by S, then it is a ushort (uInt16) object
		/// If the object is a string prefaced by I, then it is an uint (uInt32) object
		/// Else the object is parsed as a Int32
		/// </summary>
		/// <param name="obj">The object to store</param>
		public Constant(object obj)
		{
			string st = obj.ToString();

			//Check for string and double
			if (st[0] == '\'')
				constVal = st.Substring(1, st.Length - 2);
			else if (st[0] == 'B')
				constVal = Byte.Parse(st.Substring(1));
			else if (st[0] == 'S')
				constVal = UInt16.Parse(st.Substring(1));
			else if (st[0] == 'I')
				constVal = UInt32.Parse(st.Substring(1));
			else
				constVal = UInt64.Parse(st);
		}
		
		/// <summary>
		/// Return the value of this object.
		/// As it is a Constant objects, the incoming DataItem objects are ignored
		/// </summary>
		/// <param name="dis">The current DataItem objects</param>
		/// <returns>The constant value</returns>
		public object Val(params DataItem[] dis) {
			return constVal;
		}
		
		/// <summary>
		/// The evaluation of this constant as an expression
		/// </summary>
		/// <param name="dis">The current DataItem objects</param>
		/// <returns>True always</returns>
		public bool Evaluate(params DataItem[] dis) {
			//Constants always evaluate to true
			return true;
		}
	}
	#endregion

	#region Value
	/// <summary>
	/// Class to model DataItem values
	/// </summary>
	/// <seealso cref="IAtom" />
	/// <seealso cref="IExpr" />
	class Value : IAtom {
		/// <summary>
		/// The attribute from the DataItem to retrieve
		/// </summary>
		private int attrVal;
		/// <summary>
		/// Which DataItem to retreive from from the incoming array
		/// </summary>
		private int iData;

		/// <summary>
		/// Default constructor for making an empty Value object
		/// </summary>
		public Value() {}
		
		/// <summary>
		/// The constructor for making a Value object with know attribute and data item
		/// </summary>
		/// <param name="i">Which DataItem object to fetch from</param>
		/// <param name="attr">Which attribute to fetch from</param>
		/// <returns>The value from the appropriate DataItem object</returns>
		/// <seealso cref="Data.DataItem" />
		public Value(int i, int attr) {
			attrVal = attr-1;	//use attr-1 so that the interface is 1-based
			iData = i-1;		//use i-1 so that the interface is 1-based
		}
		
		/// <summary>
		/// The value of this Value object
		/// </summary>
		/// <param name="dis">The DataItem objects to read from</param>
		/// <returns>The appropriate value</returns>
		public object Val(params DataItem[] dis) {
			return dis[iData][attrVal];
		}
		
		/// <summary>
		/// The evaluation of this Value as an expression
		/// </summary>
		/// <param name="dis">The current DataItem objects</param>
		/// <returns>True always</returns>
		public bool Evaluate(params DataItem[] dis) {
			//values are always true
			return true;
		}
	}
	#endregion
}
