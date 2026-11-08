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

    public class ActiveViewComponent : ViewComponent
    {
        private readonly IWebHostEnvironment environment;

        public ActiveViewComponent(IWebHostEnvironment env)
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
                string query = "SELECT l.*, tc.Name AS Name1, tc.Picture AS Logo1, tj.Name AS Name2, tj.Picture AS Logo2 FROM LobbyTable l INNER JOIN TeamTable tc ON tc.TID = l.TeamCreate INNER JOIN TeamTable tj ON tj.TID = l.TeamJoin WHERE l.Active = 'YES'";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlConn);
                sqlDa.Fill(localTbl);



            }

            return View(localTbl);
        }


    }
}

