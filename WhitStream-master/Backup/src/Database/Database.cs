using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using WhitStream.Data;


namespace WhitStream.Database
{

    /// <summary>
    /// Connects to a MySQL database
    /// </summary>
    public class MySqlDatabase
    {
        MySqlConnection mycon;
        /// <summary>
        /// Defaults the database to the test database
        /// </summary>
        public MySqlDatabase()
        {
            try
            {   
                mycon = new MySqlConnection("datasource=localhost;username=whitstream;password=whitstream;database=test");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Connects to the local MySQL server with admin privileges and the input database
        /// </summary>
        /// <param name="database">The database to use</param>
        public MySqlDatabase(string database)
        {
            try
            {
                mycon = new MySqlConnection("datasource=localhost;username=whitstream;password=whitstream;database=" + database);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }        

        /// <summary>
        /// Run a query over an open MySQL database
        /// </summary>
        /// <param name="query">A MySQL/SQL style query</param>
        /// <returns>A table with the results of the query</returns>
        public DataTable QueryDatabase(string query)
        {
            //create a datatable
            DataTable mydt = new DataTable();
            try
            {
                //create a mysql DataAdapter
                MySqlDataAdapter myadp = new MySqlDataAdapter(query, mycon);
                //now fill and bind the DataGrid
                myadp.Fill(mydt);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return mydt;
        }

        /*public void UpdateDI(DataItem di, string query)
        { 
            
        }*/
    }
}
