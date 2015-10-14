/*
 * Created by SharpDevelop.
 * User: Pete
 * Date: 12/29/2005
 * Time: 12:19 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization;
using WhitStream.Utility;

namespace WhitStream.Data
{
    internal class DataValueManager
    {
        private const int INITCOUNT = 300;
        private const int DEFAULTATTRCOUNT = 10;
        private List<ArrayList> liALValues = new List<ArrayList>(INITCOUNT);
        private int iALValues;
        public DataValueManager()
        {
            Populate();
        }

        private void Populate()
        {
            liALValues.Clear();
            for (int i = 0; i < INITCOUNT; i++)
                liALValues.Add(new ArrayList());
            iALValues = 0;
        }
         
        public ArrayList GetALValues()
        {
            ArrayList alRet;

            lock (this)
            {
                if (iALValues == INITCOUNT)
                    Populate();
                alRet = liALValues[iALValues];
                liALValues[iALValues++] = null;
            }

            return alRet;
        }
    }

    #region DataItem
    /// <summary>
    /// Class to model a data item from a source (stream or table)
    /// </summary>
    [Serializable]
    public class DataItem : IDisposable, ISerializable
	{
        private const string CATTRS_SERIALIZATION = "cAttrs";
        private const string EOF_SERIALIZATION = "eof";
        private const string TS_SERIALIZATION = "Time";

        /// <summary>Pool of arraylists for values</summary>
        internal static DataValueManager dvm = new DataValueManager();

		/// <summary>Values for this data item</summary>
		protected ArrayList alValues;
		/// <summary>Number of attributes in the data item</summary>
		protected int cAttrs;
		/// <summary>How many attributes are in this item</summary>
		protected int m_storedCount = 0;
		/// <summary>Whether or not we've reached the end of the data source</summary>
		private bool fEOF = false;
        /// <summary>The data pool used to create this data item, so we can release it</summary>
        private Utility.DataItemPool.ReleaseDataItem reldataitem=null;

		/// <summary>
		/// Constructor for creating a data item with the given number of attributes
		/// </summary>
		/// <param name="c">The number of attributes to track</param>
        /// <param name="r">Delegate to release data item back to data pool</param>
		public DataItem(int c, Utility.DataItemPool.ReleaseDataItem r)
		{
			cAttrs = c;
            reldataitem = r;
			//ArrayList temp = new ArrayList(c);
			//#warning Testing out synchronization of data items
			//alValues = ArrayList.Synchronized(temp);
            //alValues = new ArrayList(c);
            alValues = dvm.GetALValues();
		}
        /// <summary>
        /// Copy constructor for a data item
        /// </summary>
        /// <param name="di">The data item to copy</param>
        /// <param name="c">The number of attributes more to track than the input data item has</param>
        /// <param name="r">Delegate to release data item back to data pool</param>
        public DataItem(DataItem di, uint c, Utility.DataItemPool.ReleaseDataItem r)
        {
            cAttrs = (int)c + di.Count;
            fEOF = di.fEOF;
            TimeStamp = di.TimeStamp;
            reldataitem = r;
            alValues = dvm.GetALValues();
            foreach (object o in di.alValues)
            {
                AddValue(o);
            }
        }

        /// <summary>
        /// Constructor to create a data item from a serialized one
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DataItem(SerializationInfo info, StreamingContext context)
        {
            cAttrs = info.GetInt32(CATTRS_SERIALIZATION);
            fEOF = info.GetBoolean(EOF_SERIALIZATION);
            TimeStamp = info.GetDateTime(TS_SERIALIZATION);
            reldataitem = null;
            alValues = dvm.GetALValues();

            int tmp;
            foreach (var si in info)
            {
                //Console.WriteLine("<{0} = {1}>", si.Name, si.Value);
                if (Int32.TryParse(si.Name, out tmp))
                    this.AddValue(si.Value);
            }
        }

        /// <summary>
        /// Override method to create a serialized data item
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(CATTRS_SERIALIZATION, cAttrs);
            info.AddValue(EOF_SERIALIZATION, fEOF);
            info.AddValue(TS_SERIALIZATION, TimeStamp);
            for (int i = 0; i < m_storedCount; i++)
            {
                info.AddValue(i.ToString(), alValues[i]);
            }
        }

		/// <summary>
		/// Property to determine if this is the last data item from the source
		/// </summary>
		public bool EOF
		{
			get { return fEOF; }
			set { fEOF = value; }
		}

        /// <summary>The time the data item came into the system</summary>
        public DateTime TimeStamp
        { get; set; }

		/// <summary>
		/// Property to determine how many attributes are in the data item
		/// </summary>
		public int Count
		{
			get { return m_storedCount; }
		}

		/// <summary>
		/// Append an attribute value to the data item
		/// </summary>
		/// <param name="o"></param>
		public virtual void AddValue(object o)
		{
			//If we have too many attributes, lets notify the server!
			if (alValues.Count >= cAttrs)
				throw new Exception("Not enough attributes to hold everything: " + new System.Diagnostics.StackTrace().ToString());
			
			//First we need to figure out where the value is going to be added
			int currentSize = alValues.Count;

			//Make sure that the mask has this bit turned on
			Mask(currentSize, true);
			//Finally add the value
			alValues.Add(o);
			m_storedCount++;
		}

		/// <summary>
		/// Support indexing on a particular attribute
		/// </summary>
		/// <param name="i">The attribute to read/write</param>
		/// <returns>The value for that attribute</returns>
		public virtual object this[int i]
		{
			get
			{
				//StackTrace st = new StackTrace();
				//Log.WriteMessage(string.Format("{0} : {1}", this.GetHashCode(), st.ToString()), Log.eMessageType.Debug);
				return alValues[FindMaskFromPosition(i)];
			}
			set
			{
				int newPos = FindMaskFromPosition(i);
				alValues[newPos] = value;
				Mask(newPos, true);
			}
		}

		/// <summary>
		/// Gets a value from the data item at a position
		/// </summary>
		/// <param name="position">Position in the list</param>
		/// <returns></returns>
		public object GetValue(int position)
		{
			return this[position];
		}

		/// <summary>
		/// Determine if two data items are equal (based on values of attributes)
		/// </summary>
		/// <param name="obj">The data item to compare to</param>
		/// <returns>True if the data items are equal</returns>
		public override bool Equals(object obj)
		{
			bool fEQ = obj is DataItem;
			if (fEQ)
			{
				DataItem diComp = obj as DataItem;
				fEQ |= diComp.Count == this.Count;

				for (int i = 0; fEQ && i < alValues.Count; i++)
				{
					fEQ &= (alValues[i].Equals(diComp[i]));
				}
			}
			return fEQ;
		}

		/// <summary>
		/// Compute a hash code for this data item
		/// </summary>
		/// <returns>The hash code for this object</returns>
		public override int GetHashCode()
		{
            //StringBuilder stb = new StringBuilder();
            //foreach (object o in alValues)
            //{
            //    stb.Append(o.ToString());
            //    stb.Append("##");
            //}

            //int hc = stb.ToString().GetHashCode();
            //return hc;

            int hc=0;
            foreach (object o in alValues)
                hc += o.GetHashCode();

            return hc;
		}

		/// <summary>
		/// Compute a hash code for this data item based on specific attributes
		/// </summary>
		/// <param name="attrs">The attributes to read from</param>
		/// <returns>True if the data items are equal</returns>
		public int GetSpecificHashCode(params int[] attrs)
		{
            //StringBuilder stb = new StringBuilder();
            //Array.Sort(attrs);
            //foreach (int i in attrs)
            //{
            //    stb.Append(alValues[i].ToString());
            //    stb.Append("##");
            //}

            //return stb.ToString().GetHashCode();

            int hc=0;

            Array.Sort(attrs);
            foreach (int i in attrs)
                hc += alValues[i].GetHashCode();

            return hc;
		}

		/// <summary>
		/// A string representation of the data item
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder stb = new StringBuilder("<");
			for (int i = 0; i < alValues.Count; i++)
			{
				stb.Append(alValues[i].ToString());
				if (i != alValues.Count - 1)
					stb.Append(", ");
			}
			stb.Append(">");
			return stb.ToString();
		}

		#region Masking

		UInt64 m_mask = 0;
		bool disposed = false;

		/// <summary>
		/// Mask the given position
		/// </summary>
		/// <param name="position">The zero-based position to mask</param>
		/// <param name="on">On (true) means that the attribute will be visible</param>
		public void Mask(int position, bool on)
		{
			//Make sure the position fits our stipulations
			//Can't be bigger than 64 or less than 0
			if (position >= sizeof(UInt64)*8 || position < 0)
				return;

			//We start out b00....001
			UInt64 tempmask = 1;
			/*Let's move it over position bits
			 * 1 << 0 = b1 so we will mask the first bit in mask
			 * 1 << 1 = b10 so we will mask the second bit in mask
			 * Note: This is a zero-based shift. We will mask the n+1 bit
			 * where n is the number shifted */
			tempmask = tempmask << position;

			//We are turning the bit on, lets make sure it is on
			if (on)
			{
				m_mask |= tempmask;
			}
			//We are turning the bit off, so we need to bitwise not the tempmask and then make sure the bit is off
			else
			{
				tempmask = ~tempmask;
				m_mask &= tempmask;
			}
		}

		/// <summary>
		/// Find the correct position based on an initial position of a data item in the list
		/// </summary>
		/// <param name="initialPosition">The initial position to use</param>
		/// <returns>The correct position</returns>
		/// <remarks>This is necessary to find the position if a data mask has been turned off</remarks>
		private int FindMaskFromPosition(int initialPosition)
		{
			//This case will only happen if a project operator has been called rendering the list
			//not the same size as the stored count
			if (m_storedCount != alValues.Count)
			{
				//First lets make a bit mask to represent the position
				UInt64 tempMask = 1;

				int newPosition = 0;

				/* What happens here is that we need to count 'unmasked' bits.  For example,
				 * if our mask is b110011 and we are looking for position 4.  Obviously, position
				 * 4 is unmasked which makes no sense to return.  Therefore, we need to skip the 0s
				 * and but we need to increment the new position.
				 * */

				//Lets start out loop to get the new position
				while (initialPosition != 0)
				{
					//If our mask contains this bit
					if ((tempMask & m_mask) != 0)
						//Lets decrement our count
						initialPosition--;
					//Lets move to the next mask
					tempMask = tempMask << 1;
					//Lets add a position
					newPosition++;
				}

                System.Diagnostics.Debug.Assert(newPosition < alValues.Count);
 				return newPosition;
			}

			//We don't need to do any processing
			return initialPosition;
		}

		/// <summary>
		/// Reset the mask back to 0
		/// </summary>
		public void ResetMask()
		{
			m_mask = 0;
		}

		#endregion
		
		/// <summary>
		/// Have we already released this dataitem back to the queue
		/// </summary>
		public bool Disposed
		{
			get { return disposed; }
			set { disposed = value; }
		}

		/// <summary>
		/// Release this data item back to the pool
		/// </summary>
		public void Dispose()
		{
			//if (!(this is Punctuation))
			//	Log.WriteMessage(this.ToString(), Log.eMessageType.Debug);
			if (!disposed)
			{
				Clear();
                if (reldataitem != null)
                    reldataitem(this);
			}
		}

		/// <summary>
		/// Completely resets this data item
		/// </summary>
		public void Clear()
		{
			alValues.Clear();
			m_storedCount = 0;
			ResetMask();
		}
        /// <summary>
        /// Add the capcity for more attributes to the data item
        /// </summary>
        /// <param name="count">The number of attributes more to track</param>
        public void AddCapacity(uint count)
        {
            cAttrs += (int)count;
        }
	}
	#endregion

	#region Punctuation
	/// <summary>
	/// Represents a punctuation.
	/// </summary>
	/// <seealso cref="DataItem"/>
    [Serializable]
	public class Punctuation : DataItem
	{
		#region Patterns
		/// <summary>
		/// Pattern objects for punctuation attributes
		/// </summary>
		public interface Pattern
		{
			/// <summary>
			/// Return true if the given value matches the pattern
			/// </summary>
			/// <param name="o">The value to match on</param>
			/// <returns>Whether the value matches the pattern</returns>
			bool Match(object o);

			/// <summary>
			/// Return the coercion (union) of the two patterns, if possible
			/// </summary>
			/// <param name="p">the pattern to coerce</param>
			/// <returns>the coercion (union) of the two patterns, or EmptyPattern if they cannot
			/// be coerced.</returns>
			Pattern Coerce(Pattern p);

			/// <summary>
			/// Return the removal of the given pattern from this punctuation, if possible
			/// </summary>
			/// <param name="p">the pattern to uncoerce</param>
			/// <returns>the "uncoercion" of the two patterns, or EmptyPattern if they cannot
			/// be uncoerced.</returns>
			Pattern Uncoerce(Pattern p);

            /// <summary>
            /// Return the combination (intersection) of the given pattern to this pattern, if possible
            /// </summary>
            /// <param name="p">the pattern to combine</param>
            /// <returns>the combination (intersection) of the two patterns, or EmptyPattern if they cannot
            /// be combined.</returns>
            Pattern Combine(Pattern p);
		}

		/// <summary>
		/// The wildcard pattern matches all data item values
		/// </summary>
        [Serializable]
		public class WildcardPattern : Pattern
		{
			/// <summary>
			/// Always returns true
			/// </summary>
			/// <param name="o">The object to match on</param>
			/// <returns>Always true</returns>
			public bool Match(object o)
			{
				return true;
			}

			/// <summary>
			/// Return the coercion of the two patterns, which will always be the wildcard
			/// </summary>
			/// <param name="p">the pattern to coerce</param>
			/// <returns>the wildcard, since it subsumes all other patterns.</returns>
			public Pattern Coerce(Pattern p)
			{
				return this;
			}

			/// <summary>
			/// Return the uncoercion of the two patterns, which will always be the wildcard
			/// </summary>
			/// <param name="p">the pattern to uncoerce</param>
			/// <returns>the wildcard, since it subsumes all other patterns.</returns>
			public Pattern Uncoerce(Pattern p)
			{
				return this;
			}

            /// <summary>
            /// Return the combination of p with the wildcard, which is always p
            /// </summary>
            /// <param name="p">the pattern to combine with</param>
            /// <returns>the combined pattern (always p)</returns>
            public Pattern Combine(Pattern p)
            {
                return p;
            }

			/// <summary>
			/// Compares WildcardPattern objects for equality
			/// </summary>
			/// <param name="obj">the pattern to compare</param>
			/// <returns>true if the two patterns are equal</returns>
			public override bool Equals(object obj)
			{
				return obj is WildcardPattern;
			}

			/// <summary>
			/// A string representation of the pattern
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return "*";
			}
		}

		/// <summary>
		/// The empty pattern matches no data item values
		/// </summary>
        [Serializable]
        public class EmptyPattern : Pattern
		{
			/// <summary>
			/// Always returns false
			/// </summary>
			/// <param name="o">The object to match on</param>
			/// <returns>Always false</returns>
			public bool Match(object o)
			{
				return false;
			}

			/// <summary>
			/// Return the coercion of the two patterns, which will always be the incoming pattern
			/// </summary>
			/// <param name="p">the pattern to coerce</param>
			/// <returns>the incoming pattern, since it subsumes the empty pattern.</returns>
			public Pattern Coerce(Pattern p)
			{
				return p;
			}

			/// <summary>
			/// Return the uncoercion of the two patterns, which will always be the empty pattern
			/// </summary>
			/// <param name="p">the pattern to uncoerce</param>
			/// <returns>the empty pattern.</returns>
			public Pattern Uncoerce(Pattern p)
			{
				return this;
			}

            /// <summary>
            /// Return the combination of p with the empty, which is always the empty pattern
            /// </summary>
            /// <param name="p">the pattern to combine with</param>
            /// <returns>the combined pattern (always empty)</returns>
            public Pattern Combine(Pattern p)
            {
                return this;
            }
            
            /// <summary>
			/// Compares EmptyPattern objects for equality
			/// </summary>
			/// <param name="obj">the pattern to compare</param>
			/// <returns>true if the two patterns are equal</returns>
			public override bool Equals(object obj)
			{
				return obj is EmptyPattern;
			}

			/// <summary>
			/// A string representation of the pattern
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return "E";
			}
		}

		/// <summary>
		/// A pattern to match on a singleton value
		/// </summary>
        [Serializable]
        public class LiteralPattern : Pattern
		{
			private object p = null;

			/// <summary>
			/// Constructs a pattern that matches a single value
			/// </summary>
			/// <param name="o">The pattern to match on</param>
			public LiteralPattern(object o)
			{
				p = o;
            }

			/// <summary> Get the value for this pattern </summary>
			public object Value
			{
				get { return p; }
				set { p = value; }
			}

			/// <summary>
			/// Returns whether the given value matches the singleton pattern
			/// </summary>
			/// <param name="o">The value to match on</param>
			/// <returns>True if the value is equal to the pattern</returns>
			public bool Match(object o)
			{
				return (p.Equals(o));
			}

			/// <summary>
			/// Return the coercion of the two patterns
			/// </summary>
			/// <param name="p">the pattern to coerce</param>
			/// <returns>the new pattern.</returns>
			public Pattern Coerce(Pattern p)
			{
				if (p is WildcardPattern)
					return p;
				else if (p is RangePattern)
				{
					RangePattern rp = p as RangePattern;
					if (((IComparable)rp.MinValue).CompareTo(Value) < 0 && ((IComparable)rp.MaxValue).CompareTo(Value) > 0)
						return rp;
					else
						return new EmptyPattern();
				}
				else if (p is ListPattern)
				{
					ListPattern listP = p as ListPattern;
					bool fFound = false;
					for (int i = 0; i < listP.Values.Length && fFound == false; i++)
						fFound = (listP.Values[i] == Value);

					if (!fFound)
					{
						List<object> lpNew = new List<object>(listP.Values);
						lpNew.Add(this.Value);
						object[] rgNew = new object[lpNew.Count];
						lpNew.CopyTo(rgNew);
						return new ListPattern(rgNew);
					}
					else
						return p;
				}
				else if (p is LiteralPattern)
				{
					LiteralPattern lp = p as LiteralPattern;
					if (lp.Value == Value)
						return this;
					else
					{
						LiteralPattern[] rglpNew = new LiteralPattern[2];
						rglpNew[0] = lp; rglpNew[1] = this;
						return new ListPattern(rglpNew);
					}
				}
				else if (p is EmptyPattern)
					return this;
				else
					return new EmptyPattern();
			}

			/// <summary>
			/// Return the uncoercion of the two patterns
			/// </summary>
			/// <param name="p">the pattern to coerce</param>
			/// <returns>the new pattern.</returns>
			public Pattern Uncoerce(Pattern p)
			{
				if (p is WildcardPattern)
					return this;
				else if (p is RangePattern)
				{
					RangePattern rp = p as RangePattern;
					if (((IComparable)rp.MinValue).CompareTo(Value) < 0 && ((IComparable)rp.MaxValue).CompareTo(Value) > 0)
						return this;
					else
						return new EmptyPattern();
				}
				else if (p is ListPattern)
				{
					ListPattern listP = p as ListPattern;
					int iFound = -1;
					for (int i = 0; i < listP.Values.Length && iFound == -1; i++)
					{
						if (listP.Values[i] == Value)
							iFound = i;
					}

					if (iFound == -1)
						return p;
					else
					{
						List<object> lpNew = new List<object>(listP.Values);
						lpNew.RemoveAt(iFound);
						object[] rgNew = new object[lpNew.Count];
						lpNew.CopyTo(rgNew);
						return new ListPattern(rgNew);
					}
				}
				else if (p is LiteralPattern || p is EmptyPattern)
					return this;
				else
					return new EmptyPattern();
			}

            /// <summary>
            /// Return the combination of p with this literal pattern
            /// </summary>
            /// <param name="p">the pattern to combine with</param>
            /// <returns>the combined pattern</returns>
            public Pattern Combine(Pattern p)
            {
                if (p is WildcardPattern)
                    return this;
                else if (p is EmptyPattern)
                    return p;
                else if (p is LiteralPattern)
                    return ((Equals(p)) ? (Pattern)this : (Pattern)new EmptyPattern());
                else if (p is ListPattern)
                {
                    ListPattern lp = p as ListPattern;
                    bool fFound = false;
                    for (int i = 0; i < lp.Values.Length && !fFound; i++)
                        fFound = lp.Values[i].Equals(this.Value);
                    return (fFound) ? (Pattern) this : (Pattern) new EmptyPattern();
                }
                else if (p is RangePattern)
                {
                    RangePattern rp = p as RangePattern;
                    return (((IComparable)rp.MinValue).CompareTo(Value) < 0 && ((IComparable)rp.MaxValue).CompareTo(Value) > 0) ? (Pattern)this : (Pattern)new EmptyPattern();
                }
                else
                    return new EmptyPattern();
            }

            /// <summary>
			/// Compares LiteralPattern objects for equality
			/// </summary>
			/// <param name="obj">the pattern to compare</param>
			/// <returns>true if the two literals are equal</returns>
			public override bool Equals(object obj)
			{
				bool fEQ = obj is LiteralPattern;

				if (fEQ)
					fEQ = p.Equals(((LiteralPattern)obj).p);

				return fEQ;
			}

			/// <summary>
			/// A string representation of the pattern
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return p.ToString();
			}

            /// <summary>Return the hash code of value of this pattern (and not the hash code of the pattern itself)</summary>
            /// <returns>The value's hash code</returns>
            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
		}

		/// <summary>
		/// A pattern to match on a range of values
		/// </summary>
        [Serializable]
        public class RangePattern : Pattern
		{
			private IComparable cmpMin = null;
			private IComparable cmpMax = null;

			/// <summary>
			/// Constructs a pattern to match on a range
			/// Note that the max and min values must support IComparable
			/// </summary>
			/// <param name="oMin">The minimum value in the range</param>
			/// <param name="oMax">The maximum value in the range</param>
			/// <seealso cref="IComparable"/>
			public RangePattern(object oMin, object oMax)
			{
				cmpMin = oMin as IComparable;
				cmpMax = oMax as IComparable;
			}

			/// <summary> Get the lower bound for this pattern </summary>
			public object MinValue
			{ get { return cmpMin; } }

			/// <summary> Get the upper bound for this pattern </summary>
			public object MaxValue
			{ get { return cmpMax; } }

			/// <summary>
			/// Checks to see if the value is within the pattern's range
			/// </summary>
			/// <param name="o">The value to match on</param>
			/// <returns>True if the value is in the range</returns>
			public bool Match(object o)
			{
				return (cmpMin.CompareTo(o) <= 0 && cmpMax.CompareTo(o) >= 0);
			}

			/// <summary>
			/// Compares RangePattern objects for equality
			/// </summary>
			/// <param name="obj">the pattern to compare</param>
			/// <returns>true if the two ranges are equal</returns>
			public override bool Equals(object obj)
			{
				bool fEQ = obj is RangePattern;

				if (fEQ)
					fEQ = cmpMin.Equals(((RangePattern)obj).cmpMin) &&
					    cmpMax.Equals(((RangePattern)obj).cmpMax);

				return fEQ;
			}

			/// <summary>
			/// Return the coercion of the two patterns
			/// </summary>
			/// <param name="p">the pattern to coerce</param>
			/// <returns>the new pattern.</returns>
			public Pattern Coerce(Pattern p)
			{
				if (p is WildcardPattern)
					return p;
				else if (p is RangePattern)
				{
					RangePattern rp = p as RangePattern;
					//[ptucker] All right, this is a bit tricky.
					//Suppose we have two ranges r1=[min1,max1], and r2=[min2,max2].
					// We either want that min1<max2 && min2<max1, or min1>max2 && min2>max1
					// Now, I could write a double if statement to check each case, but
					// instead I multiply the two values returned by compare to. If they
					// have the same sign, then multiplying the two together will give a positive
					// value. Otherwise it will give a negative value.
					//Sneaky, huh?
					if ((cmpMax.CompareTo(rp.MinValue) * cmpMin.CompareTo(rp.MaxValue)) >= 0)
					{
						IComparable cMin = (cmpMin.CompareTo(rp.MinValue) > 0) ? (rp.MinValue as IComparable) : cmpMin;
						IComparable cMax = (cmpMax.CompareTo(rp.MaxValue) > 0) ? cmpMax : (rp.MaxValue as IComparable);

						return new RangePattern(cMin, cMax);
					}
					else
						return new EmptyPattern();
				}
				else if (p is ListPattern)
					//Ranges and Lists don't play nicely together
					return new EmptyPattern();
				else if (p is LiteralPattern)
				{
					LiteralPattern litP = p as LiteralPattern;
					if (cmpMin.CompareTo(litP.Value) < 0 && cmpMax.CompareTo(litP.Value) > 0)
						return p;
					else
						return new EmptyPattern();
				}
				else if (p is EmptyPattern)
					return this;
				else
					return new EmptyPattern();
			}

			/// <summary>
			/// Return the Uncoercion of the two patterns
			/// </summary>
			/// <param name="p">the pattern to uncoerce</param>
			/// <returns>the new pattern.</returns>
			public Pattern Uncoerce(Pattern p)
			{
				if (p is WildcardPattern || p is RangePattern)
					return this;
				else if (p is ListPattern)
					//Ranges and Lists don't play nicely together
					return new EmptyPattern();
				else if (p is LiteralPattern)
				{
					LiteralPattern litP = p as LiteralPattern;
					if (cmpMin.CompareTo(litP.Value) < 0 && cmpMax.CompareTo(litP.Value) > 0)
						return this;
					else
						return new EmptyPattern();
				}
				else if (p is EmptyPattern)
					return this;
				else
					return new EmptyPattern();
			}

            /// <summary>
            /// Return the combination of p with this literal pattern
            /// </summary>
            /// <param name="p">the pattern to combine with</param>
            /// <returns>the combined pattern</returns>
            public Pattern Combine(Pattern p)
            {
                if (p is WildcardPattern)
                    return this;
                else if (p is EmptyPattern)
                    return p;
                else if (p is LiteralPattern)
                {
                    LiteralPattern lp = p as LiteralPattern;
                    return (((IComparable)MinValue).CompareTo(lp.Value) < 0 && ((IComparable)MaxValue).CompareTo(lp.Value) > 0) ? (Pattern)p : (Pattern)new EmptyPattern();
                }
                else if (p is ListPattern)
                {
                    ListPattern lp = p as ListPattern, lpNew = new ListPattern();
                    List<object> lilp = new List<object>();
                    for (int i = 0; i < lp.Values.Length; i++)
                    {
                        if (((IComparable)MinValue).CompareTo(lp.Values[i]) < 0 && ((IComparable)MaxValue).CompareTo(lp.Values[i]) > 0)
                            lilp.Add(lp.Values[i]);
                    }
                    return (lilp.Count > 0) ? new ListPattern(lilp.ToArray()) : (Pattern)new EmptyPattern();
                }
                else if (p is RangePattern)
                {
                    RangePattern rp = p as RangePattern;
                    if ((((IComparable)rp.MinValue).CompareTo(MaxValue) > 0) || (((IComparable)MinValue).CompareTo(rp.MaxValue) > 0))
                        return new EmptyPattern();
                    else
                    {
                        object oMin = (((IComparable)rp.MinValue).CompareTo(MinValue) > 0) ? rp.MinValue : MinValue;
                        object oMax = (((IComparable)rp.MaxValue).CompareTo(MaxValue) < 0) ? rp.MaxValue : MaxValue;
                        return new RangePattern(oMin, oMax);
                    }
                }
                else
                    return new EmptyPattern();
            }

            /// <summary>
			/// A string representation of the pattern
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return string.Format("[{0}, {1}]", cmpMin.ToString(), cmpMax.ToString());
			}
		}

		/// <summary>
		/// A pattern representing a list of values
		/// </summary>
        [Serializable]
        public class ListPattern : Pattern
		{
			private object[] alo = null;
            private IComparable[] alcomp = null;
            private bool sorted = false;

			/// <summary>
			/// Constructs a list pattern, given a list of objects
			/// </summary>
			/// <param name="rgo">The values to be in the list</param>
			public ListPattern(params object[] rgo)
			{
                BuildArray(false, rgo);
			}

            private ListPattern(bool fSorted, params object[] rgo)
            {
                BuildArray(fSorted, rgo);
            }

            private void BuildArray(bool fSorted, params object[] rgo)
            {
                if (fSorted)
                {
                    alo = rgo;
                    sorted = true;
                }
                else
                {
                    sorted = rgo[0] is IComparable;
                    if (sorted)
                    {
                        Array.Sort(rgo);
                        sorted = true;
                    }
                    alo = rgo;
                }
            }

            /// <summary>
            /// Merges two ListPatterns into a single listpattern
            /// </summary>
            /// <param name="lp1">The first list pattern to merge</param>
            /// <param name="lp2">The second list pattern to merge</param>
            public ListPattern(ListPattern lp1, ListPattern lp2)
            {
                sorted = lp1.sorted && lp2.sorted;
                int c1 = lp1.Values.Length, c2 = lp2.Values.Length, c = c1 + c2;
                alo = new object[c];

                if (sorted)
                {
                    int i1 = 0, i2 = 0;
                    for (int i = 0; i < c; i++)
                    {
                        if (i1 >= c1)
                            alo[i] = lp2.Values[i2++];
                        else if (i2 >= c2 || ((IComparable)lp1.Values[i1]).CompareTo(lp2.Values[i2]) < 0)
                            alo[i] = lp1.Values[i1++];
                        else
                            alo[i] = lp2.Values[i2++];
                    }
                }
                else
                {
                    for (int i = 0; i < c1; i++)
                        alo[i] = lp1.Values[i];
                    for (int i = 0; i < c2; i++)
                        alo[lp1.Values.Length + i] = lp2.Values[i];
                }
            }

			/// <summary>
			/// Checks to see if the given value is in the list
			/// </summary>
			/// <param name="o">The value to match on</param>
			/// <returns>True if the value is in the list</returns>
			public bool Match(object o)
			{
                bool f = false;
                if (sorted)
                {
                    int mid = alo.Length / 2;
                    int hi = alo.Length-1, lo = 0;
                    while (!f && hi >= lo)
                    {
                        if (!(f = alo[mid].Equals(o)))
                        {
                            if (((IComparable)alo[mid]).CompareTo(o) > 0)
                                hi = mid-1;
                            else
                                lo = mid+1;
                            mid = (hi + lo) / 2;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < alo.Length && f == false; i++)
                        f = alo[i].Equals(o);
                }
				return f;
			}

			/// <summary>
			/// Return the coercion (union) of the two patterns
			/// </summary>
			/// <param name="p">the pattern to coerce</param>
			/// <returns>the new pattern.</returns>
			public Pattern Coerce(Pattern p)
			{
				if (p is WildcardPattern)
					return p;
				else if (p is RangePattern)
					return new EmptyPattern();
				else if (p is ListPattern)
					return new ListPattern(this, p as ListPattern);
				else if (p is LiteralPattern)
				{
					LiteralPattern litP = p as LiteralPattern;
					bool fFound = false;
					for (int i = 0; fFound == false && i < Values.Length; i++)
						fFound = Values[i].Equals(litP.Value);

					if (fFound)
						return this;
					else
					{
						List<object> lpNew = new List<object>(Values);
						lpNew.Add(litP.Value);
						LiteralPattern[] rgNew = new LiteralPattern[lpNew.Count];
						lpNew.CopyTo(rgNew);
						return new ListPattern(rgNew);
					}
				}
				else
					return new EmptyPattern();
			}

			/// <summary>
			/// Return the uncoercion of the two patterns
			/// </summary>
			/// <param name="p">the pattern to uncoerce</param>
			/// <returns>the new pattern.</returns>
			public Pattern Uncoerce(Pattern p)
			{
				if (p is WildcardPattern)
					return this;
				else if (p is RangePattern)
					return new EmptyPattern();
				else if (p is ListPattern)
				{
					ListPattern listpat = p as ListPattern;
					List<object> listP = new List<object>(Values);
					foreach (object o in listpat.Values)
						listP.Remove(o);
                    if (listP.Count != 0)
                    {
                        object[] rgNew = new object[listP.Count];
                        listP.CopyTo(rgNew);
                        return new ListPattern(listpat.sorted, rgNew);
                    }
                    else
                    {
                        return new EmptyPattern();
                    }
				}
				else if (p is LiteralPattern)
				{
					LiteralPattern litP = p as LiteralPattern;
					List<object> lpNew = new List<object>(Values);
					lpNew.Remove(litP);
					object[] rgNew = new object[lpNew.Count];
					lpNew.CopyTo(rgNew);
					return new ListPattern(rgNew);
				}
				else
					return new EmptyPattern();
			}

            /// <summary>
            /// Return the combination of p with this literal pattern
            /// </summary>
            /// <param name="p">the pattern to combine with</param>
            /// <returns>the combined pattern</returns>
            public Pattern Combine(Pattern p)
            {
                if (p is WildcardPattern)
                    return this;
                else if (p is EmptyPattern)
                    return p;
                else if (p is LiteralPattern)
                {
                    LiteralPattern lp = p as LiteralPattern;
                    bool fFound = false;
                    for (int i = 0; i < Values.Length && !fFound; i++)
                        fFound = Values[i].Equals(lp.Value);
                    return (fFound) ? p : (Pattern)new EmptyPattern();
                }
                else if (p is ListPattern)
                {
                    ListPattern lp = p as ListPattern;
                    List<object> lilp = new List<object>();

                    if (Values.Length > 0 && Values[0] is IComparable)
                    {
                        int r = 0, l = 0;
                        while (r < Values.Length && l < lp.Values.Length)
                        {
                            if (Values[r].Equals(lp.Values[l]))
                            {
                                lilp.Add(Values[r]);
                                l++; r++;
                            }
                            else if (((IComparable)Values[r]).CompareTo(lp.Values[l]) < 0)
                                r++;
                            else
                                l++;
                        }
                    }
                    else
                    {
                        bool fFound = false;
                        for (int j = 0; j < Values.Length; j++)
                        {
                            fFound = false;
                            for (int i = 0; i < lp.Values.Length && !fFound; i++)
                                fFound = lp.Values[i].Equals(Values[j]);
                            if (fFound) lilp.Add(Values[j]);
                        }
                    }
                    return (lilp.Count > 0) ? (Pattern)new ListPattern((sorted && lp.sorted), lilp.ToArray()) : (Pattern)new EmptyPattern();
                }
                else if (p is RangePattern)
                {
                    RangePattern rp = p as RangePattern;
                    List<object> lilp = new List<object>();
                    for (int i = 0; i < Values.Length; i++)
                    {
                        if (((IComparable)rp.MinValue).CompareTo(Values[i]) < 0 && ((IComparable)rp.MaxValue).CompareTo(Values[i]) > 0)
                            lilp.Add(Values[i]);
                    }
                    return (lilp.Count > 0) ? new ListPattern(lilp.ToArray()) : (Pattern)new EmptyPattern();
                }
                else
                    return new EmptyPattern();
            }

            /// <summary> Get the literal patterns in this list </summary>
			public object[] Values
			{ get { return alo; } }

			/// <summary>
			/// Compares ListPattern objects for equality
			/// </summary>
			/// <param name="obj">the pattern to compare</param>
			/// <returns>true if the two lists are equal</returns>
			public override bool Equals(object obj)
			{
				bool fEQ = obj is ListPattern;

				if (fEQ)
				{
					ListPattern lpOther = obj as ListPattern;

					fEQ &= lpOther.alo.Length == alo.Length;

					for (int i = 0; i < alo.Length && fEQ; i++)
						fEQ &= alo[i].Equals(lpOther.alo[i]);
				}

				return fEQ;
			}

			/// <summary>
			/// A string representation of the pattern
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				StringBuilder stb = new StringBuilder("{");
				for (int i = 0; i < alo.Length; i++)
				{
					if (i != 0)
						stb.Append(", ");
					stb.Append(alo[i].ToString());
				}
				stb.Append("}");
				return stb.ToString();
			}
		}
		#endregion

		private bool allWildcard = true;

		/// <summary>
		/// Constructor for creating a data item with the given number of attributes
		/// </summary>
		/// <param name="c">The number of attributes to track</param>
		public Punctuation(int c) : base(c, null) { }

        /// <summary>
		/// Copy constructor for a punctuation
		/// </summary>
        /// <param name="p">The punctuation to copy</param>
		/// <param name="c">The number of attributes more to track than the copied punctuation</param>
        public Punctuation(Punctuation p, uint c) : base(p, c, null) { }

        /// <summary>
        /// Constructor to create a punctuation from a serialized one
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected Punctuation(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Override to serialize a punctuation
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

		/// <summary>
		/// Add a pattern to this punctuation, and check to see if this punctuation is all wildcards
		/// </summary>
		/// <param name="o">the pattern to add</param>
		public override void AddValue(object o)
		{
			base.AddValue(o);

			allWildcard &= o is WildcardPattern;
		}

		/// <summary>
		/// Support indexing on a particular attribute
		/// </summary>
		/// <param name="i">The attribute to read/write</param>
		/// <returns>The value for that attribute</returns>
		public override object this[int i]
		{
			get
			{
				return base[i];
			}
			set
			{
                System.Diagnostics.Debug.Assert(value is Pattern, "Assigning a non-pattern to a punctuation");
				allWildcard &= value is WildcardPattern;
				base[i] = value;
			}
		}

		/// <summary>
		/// Returns whether or not this punctuation matches the given data item
		/// </summary>
		/// <param name="d">The data item to match on</param>
		/// <returns>true if the punctuation matches the data item</returns>
		public virtual bool Match(DataItem d)
		{
			bool m = true;
			for (int i = 0; m == true && i < alValues.Count; i++)
			{
				Pattern p = alValues[i] as Pattern;
				m &= p.Match(d[i]);
			}

			return m;
		}

		/// <summary>
		/// Returns whether this punctuation "describes" the given attributes.
		/// That is, if the punctuation has wildcard values for all attributes
		/// not in the given list
		/// </summary>
		/// <param name="attrs">The attributes to check on</param>
		/// <returns>true if the punctuation describes the attributes</returns>
		public bool Describes(int[] attrs)
		{
			bool fOut = true;
			int j = 0;

			//Only output the punctuation if it describes the projected attributes
			for (int i = 0; fOut && i < Count; i++)
			{
				if (j >= attrs.Length || attrs[j] != i)
					fOut = (this[i] is Punctuation.WildcardPattern);
				else
					j++;
			}

			return fOut;
		}

        /// <summary>
        /// The combination (intersection) of the given punctuations
        /// (i.e. p1.match(d) &amp;&amp; p2.match(d) &lt;--&gt; Punctuation.Combine(p1,p2).match(d)
        /// </summary>
        /// <param name="p1">One punctuation to combine</param>
        /// <param name="p2">The other punctuation to combine</param>
        /// <returns>The punctuation that represents the combination of p1 and p2, or null of none exists</returns>
        public static Punctuation Combine(Punctuation p1, Punctuation p2)
        {
            if (p1.Count != p2.Count)
                return null;

            Punctuation pRet = new Punctuation(p1.Count);
            for (int ip = 0; ip < p1.Count; ip++)
            {
                Pattern pat = ((Pattern)p1[ip]).Combine((Pattern)p2[ip]);
                if (pat is EmptyPattern)
                    return null;
                else
                    pRet.AddValue(pat);
            }
            
            return pRet;
        }

        /// <summary>
        /// The coercion (union) of the given punctuations
        /// (i.e. p1.match(d) || p2.match(d) &lt;--&gt; Punctuation.Coerce(p1,p2).match(d)
        /// </summary>
        /// <param name="p1">One punctuation to coerce</param>
        /// <param name="p2">The other punctuation to coerce</param>
        /// <returns>The punctuation that represents the coercion of p1 and p2, or null of none exists</returns>
        public static Punctuation Coerce(Punctuation p1, Punctuation p2)
        {
            if (p1.Count != p2.Count)
                return null;

            Punctuation pRet = new Punctuation(p1.Count);
            for (int ip = 0; ip < p1.Count; ip++)
            {
                Pattern pat = ((Pattern)p1[ip]).Coerce((Pattern)p2[ip]);
                if (pat is EmptyPattern)
                    return null;
                else
                    pRet.AddValue(pat);
            }

            return pRet;
        }

        /// <summary>
        /// The removal (difference) of the second punctuation from the first
        /// </summary>
        /// <param name="p1">The original punctuation</param>
        /// <param name="p2">The punctuation to remove</param>
        /// <returns>The punctuation that represents the difference of p1 and p2, or null of none exists</returns>
        public static Punctuation Uncoerce(Punctuation p1, Punctuation p2)
        {
            if (p1.Count != p2.Count)
                return null;

            Punctuation pRet = new Punctuation(p1.Count);
            for (int ip = 0; ip < p1.Count; ip++)
            {
                Pattern pat = ((Pattern)p1[ip]).Uncoerce((Pattern)p2[ip]);
                if (pat is EmptyPattern)
                    return null;
                else
                    pRet.AddValue(pat);
            }

            return pRet;
        }

        /// <summary> Flatten out all list patterns into literal patterns </summary>
		/// <returns>The resulting punctuations from flattening the lists</returns>
		public List<Punctuation> Flatten()
		{
			List<Punctuation> ret = new List<Punctuation>();

			ret.Add(this);
			return Flatten(this, 0, ret);
		}

		private List<Punctuation> Flatten(Punctuation p, int iAttr, List<Punctuation> ret)
		{
			if (iAttr == p.Count)
			{
				return ret;
			}
			else
			{
				if (p[iAttr] is Punctuation.RangePattern)
					return null;
				else if (p[iAttr] is Punctuation.LiteralPattern || p[iAttr] is Punctuation.WildcardPattern)
					return Flatten(p, iAttr + 1, ret);
				else //ListPattern
				{
					for (int i = 0; i < ret.Count; i++)
					{
						for (int j = 0; j < ((ListPattern)ret[i][iAttr]).Values.Length; j++)
						{
							Punctuation pRet = ret[i].Copy();
							pRet[iAttr] = new LiteralPattern(((ListPattern)ret[i][iAttr]).Values[j]);
							ret.Insert(i + j + 1, pRet);
						}
						ret.RemoveAt(i);
						i += ((ListPattern)p[iAttr]).Values.Length - 1;
					}
					return Flatten(p, iAttr + 1, ret);
				}
			}
		}

		/// <summary>
		/// Determine if two data items are equal (based on values of attributes)
		/// </summary>
		/// <param name="obj">The data item to compare to</param>
		/// <returns>True if the data items are equal</returns>
		public override bool Equals(object obj)
		{
			bool fEQ = obj is Punctuation;
			if (fEQ)
			{
				Punctuation pComp = obj as Punctuation;
				fEQ |= pComp.Count == this.Count;

				for (int i = 0; fEQ && i < alValues.Count; i++)
				{
					fEQ &= (alValues[i].Equals(pComp[i]));
				}
			}
			return fEQ;
		}

		/// <summary> Make a copy of this punctuation </summary>
		/// <returns>a copy of this punctuation</returns>
		public Punctuation Copy()
		{
			Punctuation ret = new Punctuation(this.Count);
			for (int i = 0; i < this.Count; i++)
				ret.AddValue(this[i]);

			return ret;
		}

		/// <summary> Checks to see if all patterns in this punctuation are wildcards </summary>
		public bool IsAllWildcard
		{
			get { return allWildcard; }
		}

		/// <summary>
		/// Generate a string representation of this punctuation
		/// </summary>
		/// <returns>This punctuation in a string representation</returns>
		public override string ToString()
		{
			return string.Format("P:{0}", base.ToString());
		}

		/// <summary>
		/// A byte representation of the punctuation
		/// </summary>
		/// <returns></returns>
		/// <remarks>This is used when sending a punctuation schema to a source.  Create a dummy punctuation that 
		/// contains the factors, if necessary, for the patterns.
		/// The byte array has this pattern:
		/// [type of punctuation][type of pattern][type of value]{[factor],[max-min],[number in list]{[list]...}}...</remarks>
		public byte[] ToByte(Schema schema, int punc_type)
		{
			List<byte> lb = new List<byte>(Count);
			lb.Add((byte)punc_type);
			for (int i = 0; i < alValues.Count; i++)
			{
				Pattern pat = alValues[i] as Pattern;

				if (pat is WildcardPattern)
					lb.Add(1);
				else if (pat is LiteralPattern)
				{
					lb.Add(2);
					byte[] lit;
					if (schema.attributes[i].Type == Util.TypeByte)
					{
						lit = Utility.Util.IntToByte((byte)((LiteralPattern)pat).Value);
						lb.Add(1);
					}
					else if (schema.attributes[i].Type == Util.TypeUShort)
					{
						lb.Add(2);
						lit = Utility.Util.IntToByte((ushort)((LiteralPattern)pat).Value);
					}
					else if (schema.attributes[i].Type == Util.TypeUInt)
					{
						lb.Add(4);
						lit = Utility.Util.IntToByte((int)(uint)((LiteralPattern)pat).Value);
					}
					else
					{
						Log.WriteMessage("No matching type: ToByte()", Log.eMessageType.Error);
						return null;
					}
					lb.Add(lit[0]);
					lb.Add(lit[1]);
					lb.Add(lit[2]);
					lb.Add(lit[3]);
				}
				else if (pat is RangePattern)
				{
					lb.Add(3);

					byte[] lit;
					if (schema.attributes[i].Type == Util.TypeByte)
					{
						lit = Utility.Util.IntToByte((byte)((RangePattern)pat).MaxValue - (byte)((RangePattern)pat).MinValue);
						lb.Add(1);
					}
					else if (schema.attributes[i].Type == Util.TypeUShort)
					{
						lb.Add(2);
						lit = Utility.Util.IntToByte((ushort)((RangePattern)pat).MaxValue - (ushort)((RangePattern)pat).MinValue);
					}
					else if (schema.attributes[i].Type == Util.TypeUInt)
					{
						lb.Add(4);
						lit = Utility.Util.IntToByte((int)((uint)((RangePattern)pat).MaxValue - (uint)((RangePattern)pat).MinValue));
					}
					else
					{
						Log.WriteMessage("No matching type: ToByte()", Log.eMessageType.Error);
						return null;
					}
					lb.Add(lit[0]);
					lb.Add(lit[1]);
					lb.Add(lit[2]);
					lb.Add(lit[3]);
				}
				else if (pat is ListPattern)
				{
					lb.Add(4);

					if (schema.attributes[i].Type == Util.TypeByte)
					{
						lb.Add(1);
					}
					else if (schema.attributes[i].Type == Util.TypeUShort)
					{
						lb.Add(2);
					}
					else if (schema.attributes[i].Type == Util.TypeUInt)
					{
						lb.Add(4);
					}
					else
					{
						Log.WriteMessage("No matching type: ToByte()", Log.eMessageType.Error);
						return null;
					}

					byte[] lit = Util.IntToByte(((ListPattern)pat).Values.Length);
					lb.Add(lit[0]);
					lb.Add(lit[1]);
					lb.Add(lit[2]);
					lb.Add(lit[3]);

					object[] values = ((ListPattern)pat).Values;

					for (int j = 0; j < ((ListPattern)pat).Values.Length; j++)
					{
						if (schema.attributes[i].Type == Util.TypeByte)
						{
							lit = Utility.Util.IntToByte((byte)((LiteralPattern)values[j]).Value);
						}
						else if (schema.attributes[i].Type == Util.TypeUShort)
						{
							lit = Utility.Util.IntToByte((int)(ushort)((LiteralPattern)values[j]).Value);
						}
						else if (schema.attributes[i].Type == Util.TypeUInt)
						{
							lit = Utility.Util.IntToByte((int)(uint)((LiteralPattern)values[j]).Value);
						}
						else
						{
							Log.WriteMessage("No matching type: ToByte()", Log.eMessageType.Error);
							return null;
						}

						lb.Add(lit[0]);
						lb.Add(lit[1]);
						lb.Add(lit[2]);
						lb.Add(lit[3]);
					}
				}
				else
				{
					Log.WriteMessage("Empty or non-matched pattern.", Log.eMessageType.Error);
					return null;
				}
			}
			lb.Add(0);
			return lb.ToArray();
		}
	}
	#endregion

	#region MetaPunctuation
	/// <summary>
	/// Class to model a set of punctuations, to match all against a single data item
	/// This class is useful when "building up" punctuations.
	/// </summary>
	public class MetaPunctuation : Punctuation
	{
		List<Punctuation> listP = new List<Punctuation>();

		/// <summary>
		/// Construct a new MetaPunctuation object
		/// </summary>
		/// <param name="c"></param>
		public MetaPunctuation(int c) : base(c) { }

		/// <summary>
		/// Returns whether or not this punctuation matches the given data item
		/// </summary>
		/// <param name="d">The data item to match on</param>
		/// <returns>true if the punctuation matches the data item</returns>
		public override bool Match(DataItem d)
		{
			bool fMatch = false;

			for (int i = 0; i < listP.Count && fMatch == false; i++)
				fMatch = listP[i].Match(d);

			return fMatch;
		}

		/// <summary>
		/// Assimilate a punctuation into this meta-punctuation
		/// </summary>
		/// <param name="p">The punctuation to include</param>
		public void Include(Punctuation p)
		{
			bool fSimilar = false;
			int i;
			Punctuation pNew = new Punctuation(p.Count);
			for (i = 0; i < p.Count; i++)
				pNew.AddValue(new WildcardPattern());

			for (i = 0; i < listP.Count && fSimilar == false; i++)
			{
				fSimilar = true;
				for (int a = 0; a < p.Count && fSimilar; a++)
				{
					pNew[a] = ((Pattern)listP[i][a]).Coerce(((Pattern)p[a]));
					fSimilar = !(pNew[a] is EmptyPattern);
				}
			}
			i--;

			if (!fSimilar)
				listP.Add(p);
			else
			{
				for (int a = 0; a < p.Count; a++)
					listP[i][a] = pNew[a];
			}
		}

		/// <summary>
		/// Remove the given punctuation from this meta-punctuation
		/// </summary>
		/// <param name="p">The punctuation to remove</param>
		public void Remove(Punctuation p)
		{
			bool fSimilar = false;
			int i;
			Punctuation pNew = new Punctuation(p.Count);
			for (i = 0; i < p.Count; i++)
				pNew.AddValue(new WildcardPattern());

			for (i = 0; i < listP.Count && fSimilar == false; i++)
			{
				fSimilar = true;
				for (int a = 0; a < p.Count && fSimilar; a++)
				{
					pNew[a] = ((Pattern)listP[i][a]).Uncoerce(((Pattern)p[a]));
					fSimilar = !(pNew[a] is EmptyPattern);
				}
			}
			i--;

			if (fSimilar)
			{
				for (int a = 0; a < p.Count; a++)
					listP[i][a] = pNew[a];
			}
		}
	}
	#endregion

	#region Punctuation Scheme
	/// <summary>
	/// Class to model the blueprint of a punctuation.
	/// </summary>
	public class PunctuationScheme
	{
		private List<Punctuation.Pattern> scheme;
		private int max_size;

		/// <summary>
		/// Construct a new Punctuation Scheme
		/// </summary>
		/// <param name="size">The number of attributes of the actual punctuation</param>
		public PunctuationScheme(int size)
		{
			max_size = size;
			scheme = new List<Punctuation.Pattern>(size);
		}

		/// <summary>
		/// Add a pattern to the Scheme
		/// </summary>
		/// <param name="pattern">The pattern to add to the Scheme</param>
		public void AddPattern(Punctuation.Pattern pattern)
		{
			if (scheme.Count >= max_size) return;
			if (pattern == null) return;
			scheme.Add(pattern);
		}

		/// <summary>
		/// Fill the scheme with the requested number of WildCards
		/// </summary>
		/// <param name="number">Number of WildCard patterns to be added.  A negative number
		/// indicates to fill the rest of the Scheme with WildCards.</param>
		public void FillWC(int number)
		{
			if (number < 0)
				number = max_size;
			for (int i = scheme.Count; i <= max_size || i <= number; i++)
			{
				AddPattern(new Punctuation.WildcardPattern());
			}
		}

		/// <summary>
		/// Support indexing to find patterns in the scheme.
		/// </summary>
		/// <param name="i">The attribute to read</param>
		/// <returns>The value for that attribute</returns>
		/// <remarks>Read only.</remarks>
		public Punctuation.Pattern this[int i]
		{
			get
			{
				if (i < scheme.Count)
					return scheme[i];
				else
					return null;
			}
		}

		/// <summary>
		/// Gets the current number of patterns in the Scheme
		/// </summary>
		/// <remarks>Read only.</remarks>
		public int Count
		{
			get { return scheme.Count; }
		}

		/// <summary>
		/// Gets the max number of patterns that may be added to the Scheme
		/// </summary>
		/// <remarks>Read only.</remarks>
		public int Capacity
		{
			get { return max_size; }
		}
	}
	#endregion
}
