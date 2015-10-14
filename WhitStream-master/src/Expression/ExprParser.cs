/*
 * Created by SharpDevelop.
 * User: Pete
 * Date: 1/5/2006
 * Time: 2:34 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Text;
using System.Collections.Generic;
using WhitStream.Data;

namespace WhitStream.Expression {
	/// <summary>
	/// Models expression parsing
	/// </summary>
	public class ExprParser {
		/// <summary>
		/// Tokens to parse out of the string
		/// </summary>
		private static string[] tokens = {" "};
		
		/// <summary>
		/// Parse the string predicate so it can be evaluated over the incoming DataItem objects
		/// </summary>
		/// <param name="stExpr">The predicate in String format</param>
		/// <returns>An IExpr object for evaluating this predicate</returns>
		/// <seealso cref="Expression.IExpr" />
		public static IExpr Parse(string stExpr) {
			preprocess(ref stExpr);
			string[] rgstExprs = stExpr.Split(tokens, StringSplitOptions.RemoveEmptyEntries);
			Stack<IExpr> stk = new Stack<IExpr>();
			IExpr eCurr = null, eNext;
			
			for (int i=0; i<rgstExprs.Length; i++) {
				stk.Push(ParseExpr(rgstExprs, ref i, stk));
			}
			
			//Build up the expression
			while (stk.Count>0) {
				eNext = stk.Pop();
				if (eNext is OpNOT) {
					OpNOT op = eNext as OpNOT;
					op.Expr = eCurr;
					eCurr = op;
				}
				else if (eNext is OpAND) {
					OpAND op = eNext as OpAND;
					eNext = stk.Pop();
					op.LExpr = eCurr;
					op.RExpr = eNext;
					eCurr = op;
				}
				else if (eNext is OpOR) {
					OpOR op = eNext as OpOR;
					eNext = stk.Pop();
					op.LExpr = eCurr;
					op.RExpr = eNext;
					eCurr = op;
				}
				else {
					eCurr = eNext;
				}
			}
			
			return eCurr;
		}

		/// <summary>
		/// Preprocess the incoming string to prepare it for parsing.
		/// </summary>
		/// <param name="st">The predicate string</param>
		private static void preprocess(ref string st) {
			StringBuilder stb = new StringBuilder();
			
			for (int i=0; i<st.Length; i++) {
				if (st[i] == '>' || st[i] == '<' || st[i] == '!') {
					stb.Append(' ');
					stb.Append(st[i]);
					if (st[i+1] == '=')
						stb.Append(st[++i]);
					stb.Append(' ');
				}
				else if (st[i] == '=' || st[i] == '(' || st[i] == ')') {
					stb.Append(' ');
					stb.Append(st[i]);
					stb.Append(' ');
				}
				else
					stb.Append(Char.ToUpper(st[i]));
				
			}
			
			st = stb.ToString();
		}
		
		/// <summary>
		/// Parses an individual expression (e.g. $1.4 = 10)
		/// </summary>
		/// <param name="rgstExprs">The expression pieces of the predicate string</param>
		/// <param name="i">Which expression to focus on</param>
		/// <param name="stk">The current stack</param>
		/// <returns>An IExpr representation of the expression</returns>
		/// <seealso cref="IExpr" />
		private static IExpr ParseExpr(string[] rgstExprs, ref int i, Stack<IExpr> stk) {
			IExpr e = null;
			IAtom aL, aR;
			
			switch (rgstExprs[i]) {
				case "AND":
					e = new OpAND();
					break;
				case "OR":
					e = new OpOR();
					break;
				case "NOT":
					e = new OpNOT();
					break;
				case "=":
					aL = (IAtom) stk.Pop();
					aR = ParseAtom(rgstExprs[++i]);
					e = new OpEQ(aL, aR);
					break;
				case "!=":
					aL = (IAtom) stk.Pop();
					aR = ParseAtom(rgstExprs[++i]);
					e = new OpNE(aL, aR);
					break;
				case ">":
					aL = (IAtom) stk.Pop();
					aR = ParseAtom(rgstExprs[++i]);
					e = new OpGR(aL, aR);
					break;
				case "<":
					aL = (IAtom) stk.Pop();
					aR = ParseAtom(rgstExprs[++i]);
					e = new OpLT(aL, aR);
					break;
				case "<=":
					aL = (IAtom) stk.Pop();
					aR = ParseAtom(rgstExprs[++i]);
					e = new OpLE(aL, aR);
					break;
				case ">=":
					aL = (IAtom) stk.Pop();
					aR = ParseAtom(rgstExprs[++i]);
					e = new OpGE(aL, aR);
					break;
				default:
					e = ParseAtom(rgstExprs[i]);
					break;
			}
			
			return e;
		}
		
		/// <summary>
		/// Parse a single Atom object from the string
		/// </summary>
		/// <param name="st">The string represenation of the Atom</param>
		/// <returns>The resulting IAtom object</returns>
		/// <seealso cref="IAtom" />
		private static IAtom ParseAtom(string st) {
			IAtom a;
			if (st[0] == '$') {
				//We have a value atom
				char[] tokensVal = {'$', '.'};
				string[] parts = st.Split(tokensVal, StringSplitOptions.RemoveEmptyEntries);
				a = new Value(int.Parse(parts[0]), int.Parse(parts[1]));
			} else {
				//We have a constant
				a = new Constant(st);
			}
			return a;
		}
	}
}
