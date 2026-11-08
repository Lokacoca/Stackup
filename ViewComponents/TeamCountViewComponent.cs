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
    public class TeamCountViewComponent : ViewComponent
    {

        private readonly IWebHostEnvironment environment;

        public TeamCountViewComponent(IWebHostEnvironment env)
        {
            environment = env;
        }

        private string GetConnectionString(string databaseFileName)
        {
            return environment.IsDevelopment()
                ? @$"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={environment.ContentRootPath}\{databaseFileName};Integrated Security=True;Connect Timeout=15"
                    : @$"Data Source=.\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename={environment.ContentRootPath}{databaseFileName};User Instance=true;Connect Timeout=15";
        }


        public IViewComponentResult Invoke(int TID)
        {


            DataTable localTbl = new();

            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {

                sqlConn.Open();
                string query = "SELECT * FROM UserTable u INNER JOIN TeamTable t ON u.TID = t.TID WHERE t.TID = @TID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlConn);
                sqlDa.SelectCommand.Parameters.AddWithValue("@TID", TID);
                sqlDa.Fill(localTbl);

                ViewData["COT"] = localTbl.Rows.Count;
                ViewData["TID"] = TID;
                if (HttpContext.Session.GetInt32("UID") != null)
                {
                    if (Convert.ToInt32(HttpContext.Session.GetInt32("TID")) == 0)
                    {
                        ViewData["Check1"] = 1;
                    }

                }
                return View();
            }
        }
    }

}