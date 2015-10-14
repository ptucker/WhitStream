/*
 * Created by SharpDevelop.
 * User: Pete
 * Date: 1/5/2006
 * Time: 1:14 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using WhitStream.Data;

namespace WhitStream.Expression {
	/// <summary>
	/// Interface for all expression objects
	/// </summary>
	public interface IExpr {
		
		/// <summary>
		/// Function to determine if an expression object evaluates to
		/// true or false, based on the current array of DataItem objects
		/// </summary>
		/// <param name="dis">The current DataItem objects</param>
		/// <returns>True if this expression evaluates to true, false otherwise</returns>
		/// <seealso cref="Data.DataItem" />
		bool Evaluate(params DataItem[] dis);
	}
}
