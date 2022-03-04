﻿using System;
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

            DataView dataView = new DataView(modified);
            dataView.RowFilter = $"ID > 2";

            StringBuilder sb = new StringBuilder();
             
            foreach(DataRow dr in modified.Rows)
            {
                DataView dv = new DataView(original);
                dv.RowFilter = $"ID = {dr["ID"]}";

                string newStatus = dr["Status"].ToString();                    
                if (dv.Count > 0  && newStatus != dv.ToTable().Rows[0]["Status"].ToString())
                {
                    sb.Append($"UPDATE dbo.Campaign SET Status = '{newStatus}' WHERE [ID] = '{dr["ID"]}' ; ");
                }
                else if(dv.Count == 0)
                {
                    sb.Append($"INSERT INTO [dbo].[Campaign] VALUES ('{dr["ID"]}','{dr["First_Name"]}','{dr["Last_Name"]}','{dr["Status"]}','{dr["Entry_Date"]}') ; ");
                }
                

            }
            UpdatedData(sb.ToString());
        }

        public static DataTable ReadMSAccess()
        {
            // Connection string and SQL query    
            string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\nguddeti\Downloads\campaign_template1\Test.mdb";
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
                    Console.WriteLine(ex.Message);
                }
            }
            return localDT;
        }

        public static DataTable GetGuns()
        {
            DataTable dt = new DataTable();
            using (SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PROD"].ConnectionString))
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
            using (SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PROD"].ConnectionString))
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



        public static void InsertData(DataTable dt)
        {
            using (SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PROD"].ConnectionString))
            {
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(myConnection))
                {
                    sqlBulkCopy.DestinationTableName = "dbo.Campaign";
                    myConnection.Open();
                    sqlBulkCopy.WriteToServer(dt);
                    myConnection.Close();
                }
            }
        }
    }
}