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
    public class TeamSearchViewComponent : ViewComponent
    {

        private readonly IWebHostEnvironment environment;

        public TeamSearchViewComponent(IWebHostEnvironment env)
        {
            environment = env;
        }

        private string GetConnectionString(string databaseFileName)
        {
            return environment.IsDevelopment()
                ? @$"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={environment.ContentRootPath}\{databaseFileName};Integrated Security=True;Connect Timeout=15"
                    : @$"Data Source=.\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename={environment.ContentRootPath}{databaseFileName};User Instance=true;Connect Timeout=15";
        }


        public IViewComponentResult Invoke(string input)
        {


            DataTable localTbl = new();
            DataTable localTbl2 = new();

            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {

                sqlConn.Open();
                string query = "SELECT * FROM TeamTable t INNER JOIN UserTable u ON t.LID = u.UID WHERE Name LIKE @input";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlConn);
                sqlDa.SelectCommand.Parameters.AddWithValue("@input", "%" + input + "%");
                sqlDa.Fill(localTbl);

                ViewData["Count"] = localTbl.Rows.Count;


        
                }
            return View(localTbl);

        }
    }
    }


