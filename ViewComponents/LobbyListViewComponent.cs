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
    public class LobbyListViewComponent : ViewComponent
    {
        private readonly IWebHostEnvironment environment;

        public LobbyListViewComponent(IWebHostEnvironment env)
        {
            environment = env;
        }

        private string GetConnectionString(string databaseFileName)
        {
            return environment.IsDevelopment()
                ? @$"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={environment.ContentRootPath}\{databaseFileName};Integrated Security=True;Connect Timeout=15"
                    : @$"Data Source=.\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename={environment.ContentRootPath}{databaseFileName};User Instance=true;Connect Timeout=15";
        }


        public IViewComponentResult Invoke()
        {
            DataTable localTbl = new();
            DataTable UserTbl = new();
            DataTable UserTbl2 = new();
            DataTable UserTbl3 = new();
            DataTable Check = new();



            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {

                sqlConn.Open();
                string query = "SELECT * FROM LobbyTable l INNER JOIN TeamTable t ON l.TeamCreate = t.TID WHERE l.Active = 'NO'";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlConn);
                sqlDa.Fill(localTbl);

                if (HttpContext.Session.GetInt32("UID") != null)
                {
                    string query1 = "SELECT * FROM UserTable u INNER JOIN TeamTable t ON t.LID = u.UID WHERE u.UID = @UID";
                    SqlDataAdapter sqlDa1 = new SqlDataAdapter(query1, sqlConn);
                    sqlDa1.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));

                    sqlDa1.Fill(UserTbl);


                 

                    if (UserTbl.Rows.Count == 1 )
                    {
                        ViewData["Check"] = 1;
                    }
                 
                        string query2 = "SELECT * FROM LobbyTable l INNER JOIN UserTable u ON l.TeamCreate = u.TID WHERE u.UID = @UID";
                    SqlDataAdapter sqlDa2 = new SqlDataAdapter(query2, sqlConn);
                    sqlDa2.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));

                    sqlDa2.Fill(UserTbl2);


                    string query3 = "SELECT * FROM LobbyTable l INNER JOIN UserTable u ON l.TeamJoin = u.TID WHERE u.UID = @UID";
                    SqlDataAdapter sqlDa3 = new SqlDataAdapter(query3, sqlConn);
                    sqlDa3.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));

                    sqlDa3.Fill(UserTbl3);



                    if (UserTbl2.Rows.Count == 1)
                    {
                        ViewData["Check2"] = 1;

                    }

                    if (UserTbl3.Rows.Count == 1)
                    {
                        ViewData["Check3"] = 1;

                    }



                }


            }

            return View(localTbl);
        }


    }
}
