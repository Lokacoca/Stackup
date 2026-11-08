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

    public class RequestsViewComponent : ViewComponent
    {
        private readonly IWebHostEnvironment environment;

        public RequestsViewComponent(IWebHostEnvironment env)
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
                string query = "SELECT * FROM TeamTable t INNER JOIN RequestTable r ON t.TID = r.TID WHERE r.UID = @UID";
                    SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlConn);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                    sqlDa.Fill(localTbl);



                }

                return View(localTbl);
            }


        }
    }




