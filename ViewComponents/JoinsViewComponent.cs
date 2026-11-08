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

    public class JoinsViewComponent : ViewComponent
    {
        private readonly IWebHostEnvironment environment;

        public JoinsViewComponent(IWebHostEnvironment env)
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
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {

                sqlConn.Open();
                string query = "SELECT * FROM UserTable t INNER JOIN JoinTable r ON t.UID = r.UID WHERE r.TID = @TID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlConn);
                sqlDa.SelectCommand.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                sqlDa.Fill(localTbl);



            }

            return View(localTbl);
        }


    }
}

