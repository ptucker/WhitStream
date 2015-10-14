﻿using System;
using System.Collections.Generic;
using System.Text;
//using MySql.Data.MySqlClient;
using System.Data;
using WhitStream.Data;


namespace WhitStream.Database
{
    /// <summary>
    /// Class to hold persistent data with simple, database-like operations
    /// </summary>
    public class RelationTable
    {
        DataSet dset;
        DataTable dtable;
        /// <summary>
        /// Constructor for a Relation Table
        /// </summary>
        /// <param name="names">The names of the table's columns</param>
        /// <param name="types">The types of the table's columns</param>
        /// <param name="keys">The columns which will be primary keys</param>
        public RelationTable(string[] names, Type[] types, int[] keys)
        {
            dset = new DataSet();
            dtable = dset.Tables.Add();
            DataColumn[] dcols = new DataColumn[keys.Length];
            int idcols = 0;
            for (int i = 0; i < names.Length; i++)
            {
                DataColumn dcol = new DataColumn(names[i], types[i]);
                if (idcols < keys.Length && keys[idcols] == i)
                    dcols[idcols++] = dcol;
                dtable.Columns.Add(dcol);
            }
            dtable.PrimaryKey = dcols;
        }
        /// <summary>
        /// Drop the DataRow matching the primary keys
        /// </summary>
        /// <param name="keys">The values of the matching the primary columns</param>
        public void Drop(object[] keys)
        {
            DataRow drow = Find(keys);
            if (drow != null)
                dtable.Rows.Remove(drow);
        }
        /// <summary>
        /// Drops a given DataRow from the table
        /// </summary>
        /// <param name="dropRow">The DataRow from the table to drop</param>
        public void Drop(DataRow dropRow)
        {
            dtable.Rows.Remove(dropRow);
        }
        /// <summary>
        /// Add an array of objects to the datatable
        /// </summary>
        /// <param name="drow">The DataRow to add to the table</param>
        public void Add(DataRow drow)
        {
            dtable.Rows.Add(drow);
        }
        /// <summary>
        /// Add a DataRow for which will added later
        /// </summary>
        /// <returns>The new DataRow</returns>
        /// <remarks>The row must be added to the RelationTable with the add function</remarks>
        public DataRow GetRow()
        {
            return dtable.NewRow();
        }
        /// <summary>
        /// Finds a DataRow that matches the given primary keys
        /// </summary>
        /// <param name="keys">The values of the primary columns</param>
        /// <returns></returns>
        public DataRow Find(object[] keys)
        {
            return dtable.Rows.Find(keys);
        }
        /// <summary>
        /// Finds all the DataRows which match a given expression
        /// </summary>
        /// <param name="expression">The expression to query on</param>
        /// <returns></returns>
        public DataRow[] Find(string expression)
        {
            return dtable.Select(expression);
        }
        /// <summary>The number of rows in the table</summary>
        public int Rows
        {
            get { return dtable.Rows.Count; }
        }
    }


    ///// <summary>
    ///// Connects to a MySQL database
    ///// </summary>
    //public class MySqlDatabase
    //{
    //    MySqlConnection mycon;

    //    /// <summary>
    //    /// Connects to the local MySQL server with admin privileges and the input database
    //    /// </summary>
    //    /// <param name="database">The database to use</param>
    //    public MySqlDatabase(string database)
    //    {
    //        try
    //        {
    //            mycon = new MySqlConnection("datasource=localhost;username=whitstream;password=whitstream;database=" + database);
    //            mycon.Open();
    //        }
    //        catch (MySqlException ex)
    //        {
    //            Console.WriteLine(ex.Message);
    //        }
    //    }        

    //    /// <summary>
    //    /// Updates a data item with the first result from a query
    //    /// </summary>
    //    /// <param name="di">The input data item</param>
    //    /// <param name="query">The query to run</param>
    //    /// <returns>The possibly modified data item with the result of the query on the end</returns>
    //    public DataItem UpdateDI(DataItem di, string query)
    //    {
    //        //create a datatable
    //        DataTable mydt = new DataTable();
    //        try
    //        {
    //            //create a mysql DataAdapter
    //            MySqlDataAdapter myadp = new MySqlDataAdapter(query, mycon);
    //            //now fill and bind the DataGrid
    //            myadp.Fill(mydt);
    //            di.AddCapacity(1);
    //            di.AddValue(mydt.Rows[0][0]);
    //        }
    //        catch (MySqlException ex)
    //        {
    //            Console.WriteLine(ex.Message);
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine(ex.Message);
    //        }
    //        return di;
    //    }
    //    /// <summary>Start a transaction</summary>
    //    public void StartTransaction()
    //    {
    //        MySqlCommand tStart = new MySqlCommand("START TRANSACTION", mycon);
    //        tStart.Prepare();
    //        tStart.ExecuteNonQuery();
    //    }
    //    /// <summary>Commit the transaction to the Database</summary>
    //    public void CommitTransaction()
    //    {
    //        MySqlCommand tCommit = new MySqlCommand("COMMIT", mycon);
    //        tCommit.Prepare();
    //        tCommit.ExecuteNonQuery();
    //    }
    //    /// <summary>
    //    /// Runs a command on the connected database
    //    /// </summary>
    //    /// <param name="statement">The statment to execute</param>
    //    public void RunCommand(string statement)
    //    {
    //        MySqlCommand mycomm = new MySqlCommand(statement, mycon);
    //        mycomm.Prepare();
    //        mycomm.ExecuteNonQuery();
    //    }
    //}
}