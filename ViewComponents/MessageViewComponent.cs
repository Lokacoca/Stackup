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
    public class MessageViewComponent: ViewComponent
    {
        private readonly IWebHostEnvironment environment;

        public MessageViewComponent(IWebHostEnvironment env)
        {
            environment = env;
        }

        private string GetConnectionString(string databaseFileName)
        {
            return environment.IsDevelopment()
                ? @$"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={environment.ContentRootPath}\{databaseFileName};Integrated Security=True;Connect Timeout=15"
                    : @$"Data Source=.\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename={environment.ContentRootPath}{databaseFileName};User Instance=true;Connect Timeout=15";
        }



        public IViewComponentResult Invoke(int Lobby, int? LID, int? TID)
        {
            DataTable localTbl = new();
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();
                if(Lobby == 1)
                {
                    string query = "SELECT m.Content,u.UserName,u.tagline,t.Name,u.TID,u.Picture FROM MessageTable m INNER JOIN UserTable u ON m.UID = u.UID INNER JOIN TeamTable t ON u.TID = t.TID WHERE m.LID = @LID ORDER BY m.MID";
                    SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlConn);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@LID", LID);
                    sqlDa.Fill(localTbl);

                }
                else
                {
                    string query = "SELECT * FROM MessageTable m INNER JOIN UserTable u ON m.UID = u.UID INNER JOIN TeamTable t ON u.TID = t.TID WHERE m.TID = @TID ORDER BY m.MID";
                    SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlConn);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@TID", TID);
                    sqlDa.Fill(localTbl);
                }



            }
            ViewData["Check"] = Lobby;
            return View(localTbl);
        }



    }
}
