using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace HistoricalImporter
{
    class HI
    {
        static void Main(string[] args)
        {
            try
            {
                MySqlConnection mycon = new MySqlConnection("datasource=localhost;username=whitstream;password=whitstream;database=linearroad");
                mycon.Open();

                MySqlCommand tStart = new MySqlCommand("START TRANSACTION", mycon);
                tStart.Prepare();
                MySqlCommand tCommit = new MySqlCommand("COMMIT", mycon);
                tCommit.Prepare();

                MySqlCommand mycommand = new MySqlCommand("INSERT INTO tollhistory VALUES(@VID, @Xway, @Day, @Toll)", mycon);
                mycommand.Parameters.Add("@VID", MySqlDbType.UInt32);
                mycommand.Parameters.Add("@Xway", MySqlDbType.UInt16);
                mycommand.Parameters.Add("@Day", MySqlDbType.UInt16);
                mycommand.Parameters.Add("@Toll", MySqlDbType.UInt32);
                mycommand.Prepare();

                StreamReader sr = new StreamReader("C:\\Users\\djackson11\\Desktop\\WhitStream\\LinearRoad\\historical-tolls.out");
                string line = sr.ReadLine();

                tStart.ExecuteNonQuery();
                while (line != null)
                {
                    string[] tokens = line.Split(',');
                    mycommand.Parameters["@VID"].Value = UInt32.Parse(tokens[0]);
                    mycommand.Parameters["@Day"].Value = UInt16.Parse(tokens[1]);
                    mycommand.Parameters["@Xway"].Value = UInt16.Parse(tokens[2]);
                    mycommand.Parameters["@Toll"].Value = UInt32.Parse(tokens[3]);
                   
                    mycommand.ExecuteNonQuery();
                    line = sr.ReadLine();
                    Console.WriteLine(tokens[0]);
                }
                tCommit.ExecuteNonQuery();
                sr.Close();
                mycon.Close();
            }
            catch (MySqlException msx)
            {
                Console.WriteLine(msx.Message);
            }
            catch (IOException iox)
            {
                Console.WriteLine(iox.Message);
            }
        }
    }
}
