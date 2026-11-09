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
using Microsoft.Extensions.Hosting;



namespace Stackup.ViewComponents
{
    public class CheckProfileViewComponent : ViewComponent
    {
        private readonly IWebHostEnvironment environment;

        public CheckProfileViewComponent(IWebHostEnvironment env)
        {
            environment = env;
        }

        private string GetConnectionString(string databaseFileName)
        {
            return environment.IsDevelopment()
                ? @$"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={environment.ContentRootPath}\{databaseFileName};Integrated Security=True;Connect Timeout=15"
                    : @$"Data Source=.\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename={environment.ContentRootPath}{databaseFileName};User Instance=true;Connect Timeout=15";
        }



        public IViewComponentResult Invoke(int UID, int BEN)
        {
            DataTable localTbl = new();
            DataTable localTbl2 = new();
            DataTable UserTbl2 = new();
            DataTable UserTbl3 = new();
            DataTable Check = new();
            DataTable UserTbl = new();
            DataTable Zen = new();

            if (BEN == 1)
            {
                ViewData["BEN"] = 1;
            }


            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();

                if (HttpContext.Session.GetInt32("UID") != null)
                {
                    string query1 = "SELECT * FROM UserTable u INNER JOIN TeamTable t ON t.LID = u.UID WHERE u.UID = @UID";
                    SqlDataAdapter sqlDa1 = new SqlDataAdapter(query1, sqlConn);
                    sqlDa1.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));

                    sqlDa1.Fill(UserTbl);

                    if (UserTbl.Rows.Count == 1)
                    {
                        ViewData["Check"] = 1;
                    }


                }


                string query = "SELECT * FROM LobbyTable l INNER JOIN TeamTable t ON l.TeamCreate = t.TID INNER JOIN UserTable u ON t.TID = u.TID WHERE u.UID = @UID AND l.Active = 'NO'";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlConn);
                sqlDa.SelectCommand.Parameters.AddWithValue("@UID", UID);

                sqlDa.Fill(localTbl);

                // Hide "Join" when the viewer leads the team that created this lobby
                if (localTbl.Rows.Count > 0 && UserTbl.Rows.Count == 1
                    && Convert.ToInt32(UserTbl.Rows[0]["TID"]) == Convert.ToInt32(localTbl.Rows[0]["TeamCreate"]))
                {
                    ViewData["Check"] = 0;
                }

                if (localTbl.Rows.Count == 0)
                {
                    string query1 = "SELECT * FROM LobbyTable l INNER JOIN UserTable t ON l.TeamJoin = t.TID WHERE t.UID = @UID AND l.Active = 'YES'";
                    SqlDataAdapter sqlDa1 = new SqlDataAdapter(query1, sqlConn);
                    sqlDa1.SelectCommand.Parameters.AddWithValue("@UID", UID);
                    sqlDa1.Fill(localTbl2);

                    string query4 = "SELECT * FROM LobbyTable l INNER JOIN UserTable t ON l.TeamCreate = t.TID WHERE t.UID = @UID AND l.Active = 'YES'";
                    SqlDataAdapter sqlDa4 = new SqlDataAdapter(query4, sqlConn);
                    sqlDa4.SelectCommand.Parameters.AddWithValue("@UID", UID);
                    sqlDa4.Fill(localTbl2);

                    if (localTbl2.Rows.Count > 0)
                    {

                        string query5 = "SELECT l.LID, l.TeamCreate, l.TeamJoin, tc.Name AS Name1, tc.Picture AS Logo1, tj.Name AS Name2, tj.Picture AS Logo2 FROM LobbyTable l INNER JOIN TeamTable tc ON tc.TID = l.TeamCreate INNER JOIN TeamTable tj ON tj.TID = l.TeamJoin WHERE l.LID = @LID AND l.Active = 'YES'";
                        SqlDataAdapter sqlDa5 = new SqlDataAdapter(query5, sqlConn);
                        sqlDa5.SelectCommand.Parameters.AddWithValue("@LID", localTbl2.Rows[0]["LID"]);

                        sqlDa5.Fill(Zen);
                        ViewData["TWO"] = 1;

                        return View(Zen);


                    }
                    else
                    {
                        return View(localTbl);

                    }

                }
                else
                {
                    return View(localTbl);

                }


            }

        }




    }
}
