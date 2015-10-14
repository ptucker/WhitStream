/*
 * Created by SharpDevelop.
 * User: Pete
 * Date: 1/5/2006
 * Time: 1:13 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using WhitStream.Data;

namespace WhitStream.Expression
{
	#region Binary Comparison
	/// <summary>
	/// Models general behavior for all comparison operators (including '=' and '&lt;')
	/// We assume that comparison operators only work on Atom objects, and not
	/// other comparison objects (e.g., we don't support a = b = c)
	/// </summary>
	/// <seealso cref="IAtom" />
	abstract class BinaryCompOp : IExpr {
		/// <summary>
		/// The left operand
		/// </summary>
		protected IAtom latom;
		/// <summary>
		/// The left operand
		/// </summary>
		protected IAtom ratom;
		
		/// <summary>
		/// Generic constructor for Comparison operators.
		/// </summary>
		/// <param name="l">The left operand</param>
		/// <param name="r">The right operand</param>
		public BinaryCompOp(IAtom l, IAtom r) {
			latom = l;
			ratom = r;
		}

		/// <summary>
		/// Evaluates this comparison, given any DataItem objects that are relevant
		/// </summary>
		/// <param name="dis">The DataItem objects that are involved in the comparison</param>
		/// <returns>True if the operator passes, False if it fails</returns>
		/// <seealso cref="Data.DataItem" />
		/// <seealso cref="Expression.IExpr" />
		public virtual bool Evaluate(params DataItem[] dis) { return false; } //should never get here
	}
	#endregion

	#region Unary Logic
	/// <summary>
	/// Models general behavior for all unary logical operators (e.g. NOT)
	/// </summary>
	abstract class UnaryLogOp : IExpr {
		/// <summary>
		/// The operand for this operator
		/// </summary>
		protected IExpr expr=null;
		
		/// <summary>
		/// Default constructor for an empty logical operator
		/// </summary>
		public UnaryLogOp() {}
		/// <summary>
		/// Constructor for the case when the operand is known
		/// </summary>
		public UnaryLogOp(IExpr e) {
			expr = e;
		}

		/// <summary>
		/// Property to set the expression for this operator
		/// </summary>
		public IExpr Expr {
			set { expr = value; }
		}
		
		/// <summary>
		/// Evaluate this operator, given the relevant DataItem objects
		/// </summary>
		/// <param name="dis">The current DataItem objects</param>
		/// <returns>True if the operator passes, False if it fails</returns>
		/// <seealso cref="Data.DataItem" />
		/// <seealso cref="Expression.IExpr" />
		public virtual bool Evaluate(params DataItem[] dis) { return false; } //should never get here
	}
	#endregion

	#region Bindary Logic
	/// <summary>
	/// Models general behavior for binary logical operators (e.g. AND, OR)
	/// </summary>
	abstract class BinaryLogOp : IExpr {
		/// <summary>
		/// The left operand for this operator
		/// </summary>
		protected IExpr lexpr=null;
		/// <summary>
		/// The right operand for this operator
		/// </summary>
		protected IExpr rexpr=null;
		
		/// <summary>
		/// Default constructor for an empty binary logical operator
		/// </summary>
		public BinaryLogOp() {}
		
		/// <summary>
		/// Constructor for setting both operands for this operator
		/// </summary>
		/// <param name="l">The left operand</param>
		/// <param name="r">The right operand</param>
		/// <seealso cref="IExpr" />
		public BinaryLogOp(IExpr l, IExpr r) {
			lexpr = l;
			rexpr = r;
		}

		/// <summary>
		/// Property for setting the left operand for this operator
		/// </summary>
		public IExpr LExpr {
			set { lexpr = value; }
		}
		
		/// <summary>
		/// Property for setting the right operand for this operator
		/// </summary>
		public IExpr RExpr {
			set { rexpr = value; }
		}

		/// <summary>
		/// Evaluate this operator based on the relevant DataItem objects
		/// </summary>
		/// <param name="dis">The DataItem objects for evaluating this expression</param>
		/// <returns>True if the expression passes, false otherwise</returns>
		/// <seealso cref="Data.DataItem" />
		/// <seealso cref="IExpr" />
		public virtual bool Evaluate(params DataItem[] dis) { return false; } //should never get here
	}
	#endregion

	#region OpEQ - =
	/// <summary>
	/// Models the comparison operator EQUALS (=)
	/// </summary>
	class OpEQ : BinaryCompOp {
		/// <summary>
		/// Constructor for OpEQ, to compare l=r
		/// </summary>
		/// <param name="l">Left operand</param>
		/// <param name="r">Right operand</param>
		/// <seealso cref="IAtom" />
		public OpEQ(IAtom l, IAtom r) : base(l, r) {}
		
		/// <summary>
		/// Compares the two atoms for equality
		/// </summary>
		/// <param name="dis">The data items for comparison</param>
		/// <returns>True if left operand is equal to right operand, false otherwise</returns>
		/// <seealso cref="IAtom" />
		public override bool Evaluate(params DataItem[] dis) {
			return (latom.Val(dis).Equals(ratom.Val(dis)));
		}
	}
	#endregion

	#region OpNE - !=
	/// <summary>
	/// Models the comparison operator NOT EQUALS (!=)
	/// </summary>
	class OpNE : BinaryCompOp {
		/// <summary>
		/// Constructor for OpNE, to compare l!=r
		/// </summary>
		/// <param name="l">Left operand</param>
		/// <param name="r">Right operand</param>
		/// <seealso cref="IAtom" />
		public OpNE(IAtom l, IAtom r) : base(l, r) {}
		
		/// <summary>
		/// Compares the two atoms for inequality
		/// </summary>
		/// <param name="dis">The data items for comparison</param>
		/// <returns>True if left operand is not equal to right operand, false otherwise</returns>
		/// <seealso cref="IAtom" />
		public override bool Evaluate(params DataItem[] dis) {
			return (! latom.Val(dis).Equals(ratom.Val(dis)));
		}
	}
	#endregion

	#region OpGR - >
	/// <summary>
	/// Models the comparison operator GREATER THAN (&gt;)
	/// </summary>
	class OpGR : BinaryCompOp {
		/// <summary>
		/// Constructor for OpGR, to compare l&gt;r
		/// </summary>
		/// <param name="l">Left operand</param>
		/// <param name="r">Right operand</param>
		/// <seealso cref="IAtom" />
		public OpGR(IAtom l, IAtom r) : base(l, r) {}

		/// <summary>
		/// Compares the two atoms to see if left is greater than right
		/// </summary>
		/// <param name="dis">The data items for comparison</param>
		/// <returns>True if left operand is greater than right operand, false otherwise</returns>
		/// <remarks>The two operands must support IComparable, and must be able to be
		/// compared to each other</remarks>
		/// <seealso cref="IAtom" />
		/// <seealso cref="IComparable" />
		public override bool Evaluate(params DataItem[] dis) {
			bool ret = false;
			object ol = latom.Val(dis);
			object or = ratom.Val(dis);
			if (ol is IComparable && or is IComparable) {
				IComparable l = ol as IComparable, r = or as IComparable;
				
				ret = (l.CompareTo(r) > 0);
			}
			return ret;
		}
	}
	#endregion

	#region OpLT - <
	/// <summary>
	/// Models the comparison operator LESS THAN (&lt;)
	/// </summary>
	class OpLT : BinaryCompOp {
		/// <summary>
		/// Constructor for OpLT, to compare l&lt;r
		/// </summary>
		/// <param name="l">Left operand</param>
		/// <param name="r">Right operand</param>
		/// <seealso cref="IAtom" />
		public OpLT(IAtom l, IAtom r) : base(l, r) {}

		/// <summary>
		/// Compares the two atoms to see if left is less than right
		/// </summary>
		/// <param name="dis">The data items for comparison</param>
		/// <returns>True if left operand is less than right operand, false otherwise</returns>
		/// <remarks>The two operands must support IComparable, and must be able to be
		/// compared to each other</remarks>
		/// <seealso cref="IAtom" />
		/// <seealso cref="IComparable" />
		public override bool Evaluate(params DataItem[] dis) {
			bool ret = false;
			object ol = latom.Val(dis);
			object or = ratom.Val(dis);
			if (ol is IComparable && or is IComparable) {
				IComparable l = ol as IComparable, r = or as IComparable;
				
				ret = (l.CompareTo(r) < 0);
			}
			return ret;
		}
	}
	#endregion

	#region OpGE - >=
	/// <summary>
	/// Models the comparison operator GREATER THAN OR EQUAL TO (&gt;=)
	/// </summary>
	class OpGE : BinaryCompOp {
		/// <summary>
		/// Constructor for OpGE, to compare l&gt;=r
		/// </summary>
		/// <param name="l">Left operand</param>
		/// <param name="r">Right operand</param>
		/// <seealso cref="IAtom" />
		public OpGE(IAtom l, IAtom r) : base(l, r) {}

		/// <summary>
		/// Compares the two atoms to see if left is greater than or equal to right
		/// </summary>
		/// <param name="dis">The data items for comparison</param>
		/// <returns>True if left operand is greater than or equal to right operand, false otherwise</returns>
		/// <remarks>The two operands must support IComparable, and must be able to be
		/// compared to each other</remarks>
		/// <seealso cref="IAtom" />
		/// <seealso cref="IComparable" />
		public override bool Evaluate(params DataItem[] dis) {
			bool ret = false;
			object ol = latom.Val(dis);
			object or = ratom.Val(dis);
			if (ol is IComparable && or is IComparable) {
				IComparable l = ol as IComparable, r = or as IComparable;
				
				ret = (l.CompareTo(r) >= 0);
			}
			return ret;
		}
	}
	#endregion

	#region OpLE - <=
	/// <summary>
	/// Models the comparison operator LESS THAN OR EQUAL TO (&lt;=)
	/// </summary>
	class OpLE : BinaryCompOp {
		/// <summary>
		/// Constructor for OpGE, to compare l&lt;=r
		/// </summary>
		/// <param name="l">Left operand</param>
		/// <param name="r">Right operand</param>
		/// <seealso cref="IAtom" />
		public OpLE(IAtom l, IAtom r) : base(l, r) {}

		/// <summary>
		/// Compares the two atoms to see if left is less than or equal to right
		/// </summary>
		/// <param name="dis">The data items for comparison</param>
		/// <returns>True if left operand is less than or equal to right operand, false otherwise</returns>
		/// <remarks>The two operands must support IComparable, and must be able to be
		/// compared to each other</remarks>
		/// <seealso cref="IAtom" />
		/// <seealso cref="IComparable" />
		public override bool Evaluate(params DataItem[] dis) {
			bool ret = false;
			object ol = latom.Val(dis);
			object or = ratom.Val(dis);
			if (ol is IComparable && or is IComparable) {
				IComparable l = ol as IComparable, r = or as IComparable;
				
				ret = (l.CompareTo(r) <= 0);
			}
			return ret;
		}
	}
	#endregion

	#region OpAND - &&
	/// <summary>
	/// Models the logical AND operator
	/// </summary>
	class OpAND : BinaryLogOp {
		/// <summary>
		/// Default constructor to create an empty AND operator
		/// </summary>
		public OpAND() : base() {}

		/// <summary>
		/// Constructor to create an OR operator with known operands
		/// </summary>
		/// <param name="l">The left operand</param>
		/// <param name="r">The right operand</param>
		public OpAND(IExpr l, IExpr r) : base(l, r) {}
		
		/// <summary>
		/// Evaluate the operand with logical AND
		/// </summary>
		/// <param name="dis">The data items to operate with</param>
		/// <returns>True if both expressions are true, false otherwise</returns>
		public override bool Evaluate(params DataItem[] dis)
		{
			return lexpr.Evaluate(dis) && rexpr.Evaluate(dis);
		}
	}
	#endregion

	#region OpOR - ||
	/// <summary>
	/// Models the logical OR operator
	/// </summary>
	class OpOR : BinaryLogOp {
		/// <summary>
		/// Default constructor to create an empty OR operator
		/// </summary>
		public OpOR() : base() {}
		
		/// <summary>
		/// Constructor to create an OR operator with known operands
		/// </summary>
		/// <param name="l">The left operand</param>
		/// <param name="r">The right operand</param>
		public OpOR(IExpr l, IExpr r) : base(l, r) {}
		
		/// <summary>
		/// Evaluate the operand with logical OR
		/// </summary>
		/// <param name="dis">The data items to operate with</param>
		/// <returns>True if either the left or right operand are true, false otherwise</returns>
		public override bool Evaluate(params DataItem[] dis)
		{
			return lexpr.Evaluate(dis) || rexpr.Evaluate(dis);
		}
	}
	#endregion

	#region OpNOT - !
	/// <summary>
	/// Models the logical NOT operator
	/// </summary>
	class OpNOT : UnaryLogOp {
		/// <summary>
		/// Default constructor to create an empty NOT operator
		/// </summary>
		public OpNOT() : base() {}

		/// <summary>
		/// Constructor to create an NOT operator with known operand
		/// </summary>
		/// <param name="e">The operand</param>
		public OpNOT(IExpr e) : base(e) {}

		/// <summary>
		/// Evaluate the operand with logical NOT
		/// </summary>
		/// <param name="dis">The data items to operate with</param>
		/// <returns>True if the expression is false, false otherwise</returns>
		public override bool Evaluate(params DataItem[] dis)
		{
			return !expr.Evaluate(dis);
		}
	}
	#endregion
}
