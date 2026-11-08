using Stackup.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;

using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Azure.Identity;
using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using Microsoft.Identity.Client;
using System.Runtime.Intrinsics.X86;


namespace Stackup.ViewComponents
{
    public class HistoryViewComponent : ViewComponent
    {
        private readonly IWebHostEnvironment environment;

        public HistoryViewComponent(IWebHostEnvironment env)
        {
            environment = env;
        }

        private string GetConnectionString(string databaseFileName)
        {
            return environment.IsDevelopment()
                ? @$"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={environment.ContentRootPath}\{databaseFileName};Integrated Security=True;Connect Timeout=15"
                    : @$"Data Source=.\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename={environment.ContentRootPath}{databaseFileName};User Instance=true;Connect Timeout=15";
        }


        public IViewComponentResult Invoke(int UID)
        {

            DataTable localTbl = new();
            DataTable Ben = new();

            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {

                sqlConn.Open();
                string query1 = "SELECT * FROM UserTable WHERE UID = @UID";
                SqlDataAdapter sqlDa1 = new SqlDataAdapter(query1, sqlConn);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@UID", UID);
                sqlDa1.Fill(Ben);
                int TID = Convert.ToInt32(Ben.Rows[0]["TID"]);


                string query = "SELECT l.HID, tc.Name AS Name1, tc.Picture AS Logo1, tj.Name AS Name2, tj.Picture AS Logo2 FROM HistoryTable l INNER JOIN TeamTable tc ON tc.TID = l.TeamCreate INNER JOIN TeamTable tj ON tj.TID = l.TeamJoin WHERE l.TeamCreate = @TID OR l.TeamJoin = @TID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlConn);
                sqlDa.SelectCommand.Parameters.AddWithValue("@TID", TID);

                sqlDa.Fill(localTbl);



            }




            return View(localTbl);
        }



        }
}