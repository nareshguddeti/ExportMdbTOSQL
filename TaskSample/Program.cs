using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var modified = ReadMSAccess();
            var original = GetGuns();

            StringBuilder sb = new StringBuilder();       


            var mod = GetModifiedRows(original, modified);
            foreach(DataRow dr in mod.AsEnumerable())
            {
                sb.Append($"UPDATE dbo.Campaign SET Status = '{dr["Status"]}' WHERE [ID] = '{dr["ID"]}' ; \n");
            }

            var insert = GetNewRows(original, modified);
            foreach (DataRow dr in insert.AsEnumerable())
            {
                sb.Append($"INSERT INTO [dbo].[Campaign] VALUES ('{dr["ID"]}'," +
                        $"'{dr["First_Name"]}','{dr["Last_Name"]}','{dr["Status"]}','{dr["Entry_Date"]}') ; \n");
            }

            string test = sb.ToString();
           // UpdatedData(sb.ToString());
        }

        public static DataTable ReadMSAccess()
        {
            // Connection string and SQL query    
            string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;
                        Data Source=C:\Users\nguddeti\Downloads\campaign_template1\Test.mdb";
            string strSQL = "SELECT * FROM Campaign_Table";
            DataTable localDT = new DataTable();
            // Create a connection    
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                // Create a command and set its connection    
                // Open the connection and execute the select command.    
                try
                {
                    // Open connecton    
                    connection.Open();
                    OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(strSQL, connection);
                    oleDbDataAdapter.Fill(localDT);                   
                   
                }
                catch (Exception ex)
                {
                   
                }
            }
            return localDT;
        }

        public static DataTable GetGuns()
        {
            DataTable dt = new DataTable();
            using (SqlConnection myConnection = 
                new SqlConnection(ConfigurationManager.ConnectionStrings["PROD"].ConnectionString))
            {
                var qry = "select * from Campaign";
                myConnection.Open();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(qry, myConnection);
                sqlDataAdapter.Fill(dt);
            }
            return dt;
        } 

        

        public static void UpdatedData(string qry)
        {
            using (SqlConnection myConnection = 
                new SqlConnection(ConfigurationManager.ConnectionStrings["PROD"].ConnectionString))
            {
                using (SqlCommand sqlCommand = myConnection.CreateCommand())
                {
                    sqlCommand.CommandText = qry;
                    myConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                    myConnection.Close();
                }
            }
        }     
        /// <summary>
        /// Get Modified Rows
        /// </summary>
        /// <param name="Old"></param>
        /// <param name="New"></param>
        /// <returns></returns>
        private static DataTable GetModifiedRows(DataTable Old, DataTable New)
        {
            var a = from  n in New.AsEnumerable()
                    join  o in Old.AsEnumerable()
                    on n.Field<int>("ID").ToString() equals o.Field<string>("ID")
                    where o.Field<string>("Status") != n.Field<string>("Status")
                    select n;
            return a.Any() ? a.CopyToDataTable(): Old.Clone();

        }
        /// <summary>
        /// Get New Rows
        /// </summary>
        /// <param name="Old"></param>
        /// <param name="New"></param>
        /// <returns></returns>
        private static DataTable GetNewRows(DataTable Old, DataTable New)
        {
            var modifiedIds = New.AsEnumerable().Select(b => b.Field<int>("ID").ToString()).Except(Old.AsEnumerable().Select(c => c.Field<string>("ID"))).ToList();
    
            var modifiedRows = New.AsEnumerable().Where(b => modifiedIds.Contains(b.Field<int>("ID").ToString()));

            return modifiedRows.Any() ? modifiedRows.CopyToDataTable() : New.Clone();
        }
    }
}
