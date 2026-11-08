using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using Stackup.Models;
using Stackup.Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Stackup.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment environment;

        public HomeController(IWebHostEnvironment env)
        {
            environment = env;
        }

        private string GetConnectionString(string databaseFileName)
        {
            return environment.IsDevelopment()
                ? @$"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={environment.ContentRootPath}\{databaseFileName};Integrated Security=True;Connect Timeout=15"
                    : @$"Data Source=.\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename={environment.ContentRootPath}{databaseFileName};User Instance=true;Connect Timeout=15";
        }
        public const string UserID = "UID";
        public const string InLoggad = "_InloggStatus";
        public const string TID = "TID";
        public const string UserName = "UserName";




        public IActionResult Index(int? Check)
        {

            if(Check == 1)
            {
                ViewData["Bomba"] = "In Order To Join Another Team You Must First Leave Your Current One!";
            }
            if(Check == 2)
            {
                ViewData["Bomba"] = " Accept Was Unsuccesful, That Team Is Currently Full!";

            }
            if (HttpContext.Session.GetString("_InloggStatus") == "yes")
            {
                DataTable nisse = new DataTable();
                DataTable Team = new DataTable();


                using (SqlConnection connection = new SqlConnection(GetConnectionString("StackUp.mdf")))
                {
                    connection.Open();


                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM UserTable WHERE UID = @UID", connection);

                    sqlDa.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));

                    sqlDa.Fill(nisse);
                    HttpContext.Session.SetInt32(TID, (Int32)nisse.Rows[0]["TID"]);


                    return View(nisse);
                }
            }
            else
            {
                return View();

            }


        }

        public IActionResult OAuthCallback()
        {
            return Content("callback");
        }


        //Account HANDLING
        public ActionResult Create()
        {
            return View();
        }
        

        [HttpPost]

        public ActionResult Create2(IFormFile file)
        {
 
            DataTable UserTbl = new DataTable();
            using (SqlConnection connection = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                connection.Open();
                string query = "SELECT * FROM UserTable WHERE Username = @Username AND tagline = @tagline";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, connection);
                sqlDa.SelectCommand.Parameters.AddWithValue("@Username", Request.Form["Username"].ToString());
                sqlDa.SelectCommand.Parameters.AddWithValue("@tagline", Request.Form["tagline"].ToString());
                sqlDa.Fill(UserTbl);


                 if (Request.Form["Username"] == "")
                {
                    ViewData["signupfail"] = "Please enter a username.";
                    ViewData["Ben"] = 1;


                }
                if (Request.Form["Password"] == "")
                {
                    ViewData["signupfail2"] = "Please enter a password.";
                    ViewData["Ben"] = 1;

                }
                if (Request.Form["tagline"] == "#")
                {
                    ViewData["signupfail3"] = "Please enter a tagline.";

                    ViewData["Ben"] = 1;

                }
                if (Request.Form["tagline"] == "")
                {
                    ViewData["signupfail3"] = "Please enter a tagline.";

                    ViewData["Ben"] = 1;

                }

                if ( Convert.ToInt32(ViewData["Ben"]) == 1)
                {
                    return View("Create");

                }
                if (UserTbl.Rows.Count > 0)
                {
                    ViewData["signupfail"] = "Account already exists. Please choose another.";
                    return View("Create");
                }
                else
                {
          


                    string query1 = ("INSERT INTO UserTable (Username, Password, [Desc], TID, tagline, rank) VALUES(@Usern, @Psw, @description, @TID, @tagline, @rank)");
                    SqlCommand sqlCmd = new SqlCommand(query1, connection);
                    sqlCmd.Parameters.AddWithValue("@Usern", Request.Form["Username"].ToString());
                    sqlCmd.Parameters.AddWithValue("@Psw", Request.Form["Password"].ToString());
                    sqlCmd.Parameters.AddWithValue("@description", Request.Form["description"].ToString());
                    sqlCmd.Parameters.AddWithValue("@TID", 0);
                    sqlCmd.Parameters.AddWithValue("@tagline", Request.Form["tagline"].ToString());

                    var rank = Request.Form["rank"].ToString();
                       
                    var tier = Request.Form["tier"].ToString();
                    var ben = "";
                    if(rank == "0")
                    {
                        ben = "r0";
                    } else if(rank == "25"){
                        ben = "r25";

                    }
                    else
                    {
                       
                        int zen = Convert.ToInt32(rank) + Convert.ToInt32(tier);
                        Convert.ToString(zen);
                        ben = "r" + zen;
                    }
                    sqlCmd.Parameters.AddWithValue("@rank", ben+".png");

                    sqlCmd.ExecuteNonQuery();

                    string query3 = ("UPDATE UserTable SET Picture = @img WHERE Username = @Username and tagline = @tagline");
                    SqlCommand sql = new SqlCommand(query3, connection);
                    sql.Parameters.AddWithValue("@Username", Request.Form["Username"].ToString());
                    sql.Parameters.AddWithValue("@tagline", Request.Form["tagline"].ToString());


                    if (file == null)
                    {
                        sql.Parameters.AddWithValue("@img", "dummy.png");
                        sql.ExecuteNonQuery();
                        DataTable Table = new DataTable();
                        string query2 = ("SELECT * FROM UserTable WHERE Username = @Name AND tagline = @tagline");
                        SqlDataAdapter sqlDa2 = new SqlDataAdapter(query2, connection);
                        sqlDa2.SelectCommand.Parameters.AddWithValue("@Name", Request.Form["Username"].ToString());
                        sqlDa2.SelectCommand.Parameters.AddWithValue("@tagline", Request.Form["tagline"].ToString());
                        sqlDa2.Fill(Table);

                        ViewData["Username"] = Convert.ToString(Table.Rows[0]["Username"]);
                        ViewData["Password"] = Convert.ToString(Table.Rows[0]["Password"]);

                    }
                    else
                    {
                        DataTable Table = new DataTable();
                        string query2 = ("SELECT * FROM UserTable WHERE Username = @Name AND tagline = @tagline");
                        SqlDataAdapter sqlDa2 = new SqlDataAdapter(query2, connection);
                        sqlDa2.SelectCommand.Parameters.AddWithValue("@Name", Request.Form["Username"].ToString());
                        sqlDa2.SelectCommand.Parameters.AddWithValue("@tagline", Request.Form["tagline"].ToString());
                        sqlDa2.Fill(Table);

                        int UID = Convert.ToInt32(Table.Rows[0]["UID"]);
                        ViewData["Username"] = Convert.ToString(Table.Rows[0]["Username"]);
                        ViewData["Password"] = Convert.ToString(Table.Rows[0]["Password"]);


                        sql.Parameters.AddWithValue("@img", Convert.ToString(UID) + file.FileName  );

                        var filepath = Path.Combine(this.environment.ContentRootPath, "wwwroot/Bilder/", Convert.ToString(UID) + file.FileName );

                        using var stream = System.IO.File.Create(filepath);

                        file.CopyTo(stream);

                        sql.ExecuteNonQuery();

                    }

                }
                string query5 = "SELECT * FROM UserTable WHERE Username = @Username AND Password = @Password";
                SqlDataAdapter sqlDa1 = new SqlDataAdapter(query5, connection);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@Username", ViewData["Username"]);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@Password", ViewData["Password"]);




                sqlDa1.Fill(UserTbl);

                HttpContext.Session.SetInt32(UserID, (Int32)UserTbl.Rows[0]["UID"]);
                HttpContext.Session.SetInt32(TID, (Int32)UserTbl.Rows[0]["TID"]);
                HttpContext.Session.SetString(UserName, (string)UserTbl.Rows[0]["UserName"]);

                HttpContext.Session.SetString(InLoggad, "yes");
                ViewData["UID"] = HttpContext.Session.GetInt32("UID");
                HttpContext.Session.GetString("_InloggStatus");
                return RedirectToAction("index");
            }


        }

        [HttpPost]
        //Account HANDLING

  
        //Account HANDLING

        public IActionResult Edit(int UID, IFormFile file)
        {
            DataTable Ben = new DataTable();
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();

                string query1 = "SELECT * FROM UserTable WHERE UID = @UID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query1, sqlConn);
                sqlDa.SelectCommand.Parameters.AddWithValue("@UID", UID);
                sqlDa.Fill(Ben);

                string u = Request.Form["UserName"].ToString();
                string t = Request.Form["Tagline"].ToString();
                string p = Request.Form["Password"].ToString();
                string l = Request.Form["Picture"].ToString();


                if (file != null)
                {
                     l = Convert.ToString(UID) + file.FileName;


                }
      



                if (Convert.ToString(Ben.Rows[0]["UserName"]) == u && Convert.ToString(Ben.Rows[0]["tagline"]) == t && Convert.ToString(Ben.Rows[0]["Password"]) == p && Request.Form["Picture"].ToString() == l)
                {
                    RedirectToAction("Settings", new { ben = 1 });
                }
                else
                {


                    if (file != null)
                    {
                        string query = "UPDATE UserTable SET Username= @Usern, Password = @Pws, Picture = @img WHERE UID = @UID";
                        SqlCommand sqlCmd = new SqlCommand(query, sqlConn);
                        sqlCmd.Parameters.AddWithValue("@UID", UID);
                        sqlCmd.Parameters.AddWithValue("@Usern", Request.Form["UserName"].ToString());
                        sqlCmd.Parameters.AddWithValue("@Pws", Request.Form["Password"].ToString());

                        sqlCmd.Parameters.AddWithValue("@img", Convert.ToString(UID) + file.FileName);

                        var filepath = Path.Combine(this.environment.ContentRootPath, "wwwroot/Bilder/", Convert.ToString(UID) + file.FileName);

                        using var stream = System.IO.File.Create(filepath);

                        file.CopyTo(stream);

                        sqlCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        string query = "UPDATE UserTable SET Username = @Usern, Password = @Pws, tagline = @tag WHERE UID = @UID";
                        SqlCommand sqlCmd = new SqlCommand(query, sqlConn);
                        sqlCmd.Parameters.AddWithValue("@UID", UID);
                        sqlCmd.Parameters.AddWithValue("@Usern", Request.Form["Username"].ToString());
                        sqlCmd.Parameters.AddWithValue("@Pws", Request.Form["Password"].ToString());
                        sqlCmd.Parameters.AddWithValue("@tag", Request.Form["Tagline"].ToString());

                        sqlCmd.ExecuteNonQuery();

                    }
                }
                return RedirectToAction("Settings", new { ben = 2 });

            }
        }
        //Account HANDLING

        public ActionResult Delete(int UID)
        {
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();
                string query = "DELETE FROM UserTbl WHERE UID = @UID";
                SqlCommand sqlCmd = new SqlCommand(query, sqlConn);
                sqlCmd.Parameters.AddWithValue("@UID", UID);
                sqlCmd.ExecuteNonQuery();
            }
            HttpContext.Session.Clear();

            return RedirectToAction("index");
        }


        //TEAM HANDLING

        public ActionResult ViewTeam(int TID, int? Ben, int? Zen, int? Bing, int? Check, int? INPUT)
        {
            DataTable Data = new DataTable();
            DataTable Beta = new DataTable();
            DataTable Data2 = new DataTable();


            ViewData["Check1"] = INPUT;
            ViewData["TID"] = TID;


            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();
                string query = "SELECT * FROM UserTable u INNER JOIN TeamTable t ON u.TID = t.TID WHERE t.TID = @TID";
                SqlDataAdapter sqlCmd = new SqlDataAdapter(query, sqlConn);
                sqlCmd.SelectCommand.Parameters.AddWithValue("@TID", TID);
                sqlCmd.Fill(Data);

                string query1 = "SELECT * FROM TeamTable WHERE TID = @TID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query1, sqlConn);
                sqlDa.SelectCommand.Parameters.AddWithValue("@TID", TID);
                sqlDa.Fill(Beta);


                if (HttpContext.Session.GetInt32("UID") != null)
                {
                    string query4 = "SELECT * FROM UserTable WHERE UID = @UID";
                    SqlDataAdapter sqlCmd1 = new SqlDataAdapter(query4, sqlConn);
                    sqlCmd1.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                    sqlCmd1.Fill(Data2);
                    ViewData["Picture"] = Data2.Rows[0]["Picture"];
                    ViewData["UserName"] = Data2.Rows[0]["UserName"];
                    ViewData["rank"] = Data2.Rows[0]["Rank"];
                    ViewData["tagline"] = Data2.Rows[0]["tagline"];

                }

            }

            for(int i = 0; i < 5; i++)
            {
                string text = "text" + i;
                string bext = "bext" + i;

                if (Zen == 1)
                {
                    if(Ben == i)
                    {
                        ViewData[text] = "Already invited!";

                    }
                } else if(Zen == 2)
                {
                    ViewData[bext] = "Invite Sent!";

                }
                else
                {
                    if (Ben == i)
                    {
                        ViewData[text] = "This User Does Not Exist!";

                    }
                }
            }
            if (Bing == 1)
            {
                ViewData["incorrect"] = "Password Incorrect";

                ViewData["Check"] = "yes";
            }
            if(Check == 1)
            {
                ViewData["Boop"] = "Cannot Kick Member, Make Sure To Stop Searching For A Lobby";
            }
            if(Check == 2)
            {
                ViewData["Boop"] = "Cannot Kick Member, Make Sure To Leave Any Joined Lobbies";

            }
            if(Check == 3)
            {
                ViewData["Boop"] = "Cannot Disband Team, Make Sure To Stop Searching For A Lobby";

            }
            if(Check == 4)
            {
                ViewData["Boop"] = "Cannot Disband Team, Make Sure To Leave Any Joined Lobbies";

            }
            if(Check == 5)
            {
                ViewData["Boop"] = "Cannot Leave Team, Ask Leader To Stop Searching For A Lobby";

            }
            if (Check == 6)
            {
                ViewData["Boop"] = "Cannot Leave Team, Ask Leader To Leave Any Joined Lobbies";

            }
            if(Check == 9)
            {
                ViewData["Boop"] = "Cannot Accept Request, No Available Slots In Your Team";

            }
            if (Check == 10)
            {
                ViewData["REP"] = 1;

            }
            ViewData["LID"] = Beta.Rows[0]["LID"];
            return View(Data);
        }
        public ActionResult JoinTeam(int UID, string? input, int? error)
        {
            DataTable Data = new DataTable();
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();
                string query = "SELECT * FROM UserTable WHERE UID = @UID";
                SqlDataAdapter sqlCmd = new SqlDataAdapter(query, sqlConn);
                sqlCmd.SelectCommand.Parameters.AddWithValue("@UID", UID);
                sqlCmd.Fill(Data);

            }
            if(input != null)
            {
                ViewData["input"] = input;

            }
            if(error == 1)
            {
                ViewData["error"] = "Team does not exist!";
            }
            if(error == 2)
            {
                ViewData["error"] = "Request Already Sent!";

            }
            if(error == 3)
            {
                ViewData["success"] = "Request Sent!";

            }
 
            return View(Data);
        }
        public ActionResult CreateTeam( int? ben)
        {
         

                if (ben == 5)
            {
                ViewData["ben"] = "This Team Already Exists, Please Try Another";
            }
            DataTable nisse = new DataTable();
            if(HttpContext.Session.GetInt32("UID") != null)
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString("StackUp.mdf")))
                {
                    connection.Open();


                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM UserTable WHERE UID = @UID", connection);

                    sqlDa.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));

                    sqlDa.Fill(nisse);
                    HttpContext.Session.SetInt32(TID, (Int32)nisse.Rows[0]["TID"]);


                    return View(nisse);
                }
            }
            else
            {
                return View(nisse);

            }



        }

        //TEAM HANDLING

        [HttpPost]
        public ActionResult Team(IFormFile file)
        {
            DataTable UserTbl = new DataTable();
            DataTable UserTbl2 = new DataTable();

            using (SqlConnection connection = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                connection.Open();
                string query = "SELECT * FROM TeamTable WHERE Name = @Name";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, connection);
                sqlDa.SelectCommand.Parameters.AddWithValue("@Name", Request.Form["Name"].ToString());
                sqlDa.Fill(UserTbl);
                if (UserTbl.Rows.Count > 0)
                {
                    return RedirectToAction("CreateTeam", new {ben = 5});
                }
               
                else
                {
                    string query1 = ("INSERT INTO TeamTable (Name, [Desc], Picture, LID) VALUES(@Name, @Desc, @img, @UID)");
                    SqlCommand sqlCmd = new SqlCommand(query1, connection);
                    sqlCmd.Parameters.AddWithValue("@Name", Request.Form["Name"].ToString());
                    sqlCmd.Parameters.AddWithValue("@Desc", Request.Form["Desc"].ToString());
                    sqlCmd.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));

                   

                    
                    if (file == null)
                    {
                        sqlCmd.Parameters.AddWithValue("@img", "dummy.png");
                        sqlCmd.ExecuteNonQuery();

                    }
                    else
                    {
                        sqlCmd.Parameters.AddWithValue("@img", file.FileName);

                        var filepath = Path.Combine(this.environment.ContentRootPath, "wwwroot/Bilder/", TID + file.FileName);

                        using var stream = System.IO.File.Create(filepath);

                        file.CopyTo(stream);

                        sqlCmd.ExecuteNonQuery();

                    }
                   

                    DataTable Table = new DataTable();

                    string query2 = ("SELECT t.TID FROM TeamTable t WHERE t.Name = @Name");
                    SqlDataAdapter sqlDa2 = new SqlDataAdapter(query2, connection);
                    sqlDa2.SelectCommand.Parameters.AddWithValue("@Name", Request.Form["Name"].ToString());
                    sqlDa.Fill(Table);
                    int tid = Convert.ToInt32(Table.Rows[0]["TID"]);

                    string query3 = ("UPDATE UserTable SET TID = @TID WHERE UID = @UID");
                    SqlCommand sql = new SqlCommand(query3, connection);
                    sql.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                    sql.Parameters.AddWithValue("@TID", tid);
                    sql.ExecuteNonQuery();
                    HttpContext.Session.SetInt32(TID, tid);


                }

            }
            return RedirectToAction("Index");
        }


        //TEAM HANDLING

        [HttpPost]

        public IActionResult TeamEdit(int TID, IFormFile file)
        {
            DataTable Ben = new DataTable();

            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();

                string query1 = "SELECT * FROM TeamTable WHERE TID = @TID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query1, sqlConn);
                sqlDa.SelectCommand.Parameters.AddWithValue("@TID", TID);
                sqlDa.Fill(Ben);

                string n = Request.Form["Name"].ToString();
                string d = Request.Form["Desc"].ToString();

                string l = "";

                if (file == null)
                {
                    l = Convert.ToString(Ben.Rows[0]["Picture"]);

                }
                else
                {
                     l = file.FileName;

                }



                if (Convert.ToString(Ben.Rows[0]["Name"]) == n && Convert.ToString(Ben.Rows[0]["Desc"]) == d && Convert.ToString(Ben.Rows[0]["Picture"]) == l)
                {
                    RedirectToAction("Settings", new { ben = 3 });
                }

                if (file != null)
                {
                    string query = "UPDATE TeamTable SET Name= @Usern, Picture = @img, [Desc] = @Desc WHERE TID = @TID";
                    SqlCommand sqlCmd = new SqlCommand(query, sqlConn);
                    sqlCmd.Parameters.AddWithValue("@TID", TID);
                    sqlCmd.Parameters.AddWithValue("@Usern", Request.Form["Name"].ToString());
                    sqlCmd.Parameters.AddWithValue("@img", file.FileName);
                    sqlCmd.Parameters.AddWithValue("@Desc", Request.Form["Desc"].ToString());


                    var filepath = Path.Combine(this.environment.ContentRootPath, "wwwroot/Bilder/" + file.FileName);

                    using var stream = System.IO.File.Create(filepath);

                    file.CopyTo(stream);

                    sqlCmd.ExecuteNonQuery();
                }
                else
                {
                    string query = "UPDATE TeamTable SET Name = @Usern, [Desc] = @Desc WHERE TID = @TID";
                    SqlCommand sqlCmd = new SqlCommand(query, sqlConn);
                    sqlCmd.Parameters.AddWithValue("@TID", TID);
                    sqlCmd.Parameters.AddWithValue("@Usern", Request.Form["Name"].ToString());
                    sqlCmd.Parameters.AddWithValue("@Desc", Request.Form["Desc"].ToString());
                    sqlCmd.ExecuteNonQuery();


                }
            }
            return RedirectToAction("Settings", new { ben =  4});
        }




        //TEAM HANDLING

        [HttpPost]

        public ActionResult DeleteTeam(int tid)
        {
            DataTable Data = new DataTable();
            DataTable Check = new DataTable();
            DataTable Check2 = new DataTable();


            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();

                string query4 = "SELECT * FROM LobbyTable l INNER JOIN TeamTable t ON l.TeamCreate = t.TID WHERE t.TID = @TID ";
                SqlDataAdapter sqlDa3 = new SqlDataAdapter(query4, sqlConn);
                sqlDa3.SelectCommand.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                sqlDa3.Fill(Check);
                string query3 = "SELECT * FROM LobbyTable l INNER JOIN TeamTable t ON l.TeamJoin = t.TID WHERE t.TID = @TID ";
                SqlDataAdapter sqlDa2 = new SqlDataAdapter(query3, sqlConn);
                sqlDa2.SelectCommand.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                sqlDa2.Fill(Check2);
                if (Check.Rows.Count > 0)
                {
                    return RedirectToAction("ViewTeam", new { TID = HttpContext.Session.GetInt32("TID"), Check = 3 });

                }
                else if (Check2.Rows.Count > 0)
                {
                    return RedirectToAction("ViewTeam", new { TID = HttpContext.Session.GetInt32("TID"), Check = 4 });

                }


                string query2 = "SELECT * FROM UserTable WHERE UserName = @UserName AND Password = @Password";
                SqlDataAdapter sqlDa1 = new SqlDataAdapter(query2, sqlConn);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@Password", Request.Form["password"].ToString());
                sqlDa1.SelectCommand.Parameters.AddWithValue("@UserName", HttpContext.Session.GetString("UserName"));
                sqlDa1.Fill(Data);
                if(Data.Rows.Count > 0)
                {
                    string query1 = "UPDATE UserTable SET TID = @N WHERE TID = @TID2";
                    SqlCommand sqlDa = new SqlCommand(query1, sqlConn);
                    sqlDa.Parameters.AddWithValue("@TID2", tid);
                    sqlDa.Parameters.AddWithValue("@N", 0);
                    sqlDa.ExecuteNonQuery();

                    string query = "DELETE FROM TeamTable WHERE TID = @TID";
                    SqlCommand sqlCmd = new SqlCommand(query, sqlConn);
                    sqlCmd.Parameters.AddWithValue("@TID", tid);
                    sqlCmd.ExecuteNonQuery();

                    HttpContext.Session.SetInt32(TID, (Int32)0);

                    return RedirectToAction("index");

                }
                else
                {
                    return RedirectToAction("ViewTeam", new { TID = HttpContext.Session.GetInt32("TID"), Bing = 1 });


                }




            }
        }
        [HttpGet]

        public ActionResult Leave()
        {
            DataTable Check = new DataTable();
            DataTable Check2 = new DataTable();
            
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();

                string query4 = "SELECT * FROM LobbyTable l INNER JOIN TeamTable t ON l.TeamCreate = t.TID WHERE t.TID = @TID ";
                SqlDataAdapter sqlDa3 = new SqlDataAdapter(query4, sqlConn);
                sqlDa3.SelectCommand.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                sqlDa3.Fill(Check);
                string query3 = "SELECT * FROM LobbyTable l INNER JOIN TeamTable t ON l.TeamJoin = t.TID WHERE t.TID = @TID ";
                SqlDataAdapter sqlDa2 = new SqlDataAdapter(query3, sqlConn);
                sqlDa2.SelectCommand.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                sqlDa2.Fill(Check2);
                if (Check.Rows.Count > 0)
                {
                    return RedirectToAction("ViewTeam", new { TID = HttpContext.Session.GetInt32("TID"), Check = 5 });

                }
                else if (Check2.Rows.Count > 0)
                {
                    return RedirectToAction("ViewTeam", new { TID = HttpContext.Session.GetInt32("TID"), Check = 6 });

                }

                string query1 = "UPDATE UserTable SET TID = @N WHERE UID = @UID";
                SqlCommand sqlDa = new SqlCommand(query1, sqlConn);
                sqlDa.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                sqlDa.Parameters.AddWithValue("@N", 0);
                sqlDa.ExecuteNonQuery();
                HttpContext.Session.SetInt32(TID, (Int32)0);

                return RedirectToAction("index");


            }


        }
        public ActionResult Kick(int UID)
        {
            DataTable Check = new DataTable();
            DataTable Check2 = new DataTable();

            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();

                string query2 = "SELECT * FROM LobbyTable l INNER JOIN TeamTable t ON l.TeamCreate = t.TID WHERE t.TID = @TID ";
                SqlDataAdapter sqlDa1 = new SqlDataAdapter(query2, sqlConn);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                sqlDa1.Fill(Check);
                string query3 = "SELECT * FROM LobbyTable l INNER JOIN TeamTable t ON l.TeamJoin = t.TID WHERE t.TID = @TID ";
                SqlDataAdapter sqlDa2 = new SqlDataAdapter(query3, sqlConn);
                sqlDa2.SelectCommand.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                sqlDa2.Fill(Check2);
                if (Check.Rows.Count > 0)
                {
                    return RedirectToAction("ViewTeam", new { TID = HttpContext.Session.GetInt32("TID"), Check = 1 });

                } else if(Check2.Rows.Count > 0)
                {
                    return RedirectToAction("ViewTeam", new { TID = HttpContext.Session.GetInt32("TID"), Check = 2 });

                }
                else
                {
                    string query1 = "UPDATE UserTable SET TID = @N WHERE UID = @UID";
                    SqlCommand sqlDa = new SqlCommand(query1, sqlConn);
                    sqlDa.Parameters.AddWithValue("@UID", UID);
                    sqlDa.Parameters.AddWithValue("@N", 0);
                    sqlDa.ExecuteNonQuery();
                    return RedirectToAction("ViewTeam", new { TID = HttpContext.Session.GetInt32("TID") });
                }

 
            }

        }



        //REQUEST HANDLING
        [HttpPost]

        public ActionResult CreateRequest(int TID, int? input)
        {
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                DataTable Data = new DataTable();
                DataTable Check = new DataTable();


                sqlConn.Open();
                string query2 = "SELECT * FROM UserTable WHERE Username = @Name ";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query2, sqlConn);
                sqlDa.SelectCommand.Parameters.AddWithValue("@Name", Request.Form["Request"].ToString());
                sqlDa.Fill(Data);
                if(Data.Rows.Count == 0)
                {
                    return RedirectToAction("ViewTeam", new { TID = HttpContext.Session.GetInt32("TID"), Ben = input, INPUT = input });

                }
                ViewData["UID"] = Data.Rows[0]["UID"];

                string query3 = "SELECT * FROM RequestTable WHERE UID = @UID AND TID = @TID ";
                SqlDataAdapter sqlDa1 = new SqlDataAdapter(query3, sqlConn);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@UID", Convert.ToInt32(ViewData["UID"]));
                sqlDa1.SelectCommand.Parameters.AddWithValue("@TID", TID);
                sqlDa1.Fill(Check);

                if(Check.Rows.Count > 0)
                {
                    return RedirectToAction("ViewTeam", new { TID = HttpContext.Session.GetInt32("TID"), Ben = input, Zen = 1, INPUT = input });

                }
                else
                {
                    string query = "INSERT INTO RequestTable (UID, TID) VALUES(@UID, @TID) ";
                    SqlCommand sqlCmd = new SqlCommand(query, sqlConn);
                    sqlCmd.Parameters.AddWithValue("@UID", Convert.ToInt32(ViewData["UID"]));
                    sqlCmd.Parameters.AddWithValue("@TID", TID);
                    sqlCmd.ExecuteNonQuery();
                    return RedirectToAction("ViewTeam", new { TID = HttpContext.Session.GetInt32("TID"), Ben = input, Zen = 2, INPUT = input });

                }




            }
        }

        public ActionResult CreateJoin(int UID, int TID)
        {
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                DataTable Data = new DataTable();
                DataTable Check = new DataTable();

                sqlConn.Open();

                string query3 = "SELECT * FROM JoinTable WHERE UID = @UID AND TID = @TID ";
                SqlDataAdapter sqlDa1 = new SqlDataAdapter(query3, sqlConn);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@TID", TID);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@UID", UID);
                sqlDa1.Fill(Check);

                if (Check.Rows.Count > 0)
                {
                    return RedirectToAction("Search2", new { error = 2 , input= 1});

                }
                else
                {
                    string query = "INSERT INTO JoinTable (UID, TID) VALUES(@UID, @TID) ";
                    SqlCommand sqlCmd = new SqlCommand(query, sqlConn);
                    sqlCmd.Parameters.AddWithValue("@TID", TID);
                    sqlCmd.Parameters.AddWithValue("@UID", UID);
                    sqlCmd.ExecuteNonQuery();
                    return RedirectToAction("Search2", new { error = 3, input = 1});

                }




            }
        }

        //REQUEST HANDLING 

        [HttpGet]

        public ActionResult CheckRequest(int uid, int TID, int rid)
        {
        
            if (HttpContext.Session.GetInt32("TID") == 0)
            {
                return RedirectToAction("AcceptRequest", new { RID = rid , UID = uid, tid = TID});

            }
            else
            {
                return RedirectToAction("index", new {Check = 1});
            }
        }


        public ActionResult AcceptRequest(int UID, int tid, int RID)
        {
            DataTable user = new DataTable();
            DataTable Check1 = new DataTable();

            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {

                string query3 = "SELECT * FROM UserTable u INNER JOIN TeamTable t ON u.TID = t.TID WHERE TID = @TID ";
                SqlDataAdapter sqlDa1 = new SqlDataAdapter(query3, sqlConn);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@TID", tid);
                sqlDa1.Fill(Check1);
                if (Check1.Rows.Count >= 5)
                {
                    return RedirectToAction("Index", new {Check = 2 });
                }

                sqlConn.Open();
                string query = " UPDATE UserTable SET TID = @TID WHERE UID = @UID ";
                SqlCommand sqlCmd = new SqlCommand(query, sqlConn);
                sqlCmd.Parameters.AddWithValue("@TID", tid);
                sqlCmd.Parameters.AddWithValue("@UID", UID);

                sqlCmd.ExecuteNonQuery();

             

                string query1 = " DELETE FROM JoinTable WHERE UID = @UID ";
                SqlCommand sqlCmd1 = new SqlCommand(query1, sqlConn);
                sqlCmd1.Parameters.AddWithValue("@UID", UID);

                sqlCmd1.ExecuteNonQuery();


           
            }
            HttpContext.Session.SetInt32(TID, tid);

            return RedirectToAction("DeclineRequest", new {rid = RID});



        }
        //REQUEST HANDLING


        public ActionResult DeclineRequest(int rid)
        {
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();
                string query = "DELETE FROM RequestTable WHERE RID = @RID ";
                SqlCommand sqlCmd = new SqlCommand(query, sqlConn);
                sqlCmd.Parameters.AddWithValue("@RID", rid);
                sqlCmd.ExecuteNonQuery();

            }
            return RedirectToAction("Index");
        }

        //REQUEST HANDLING


        public ActionResult AcceptJoin(int UID, int Tid, int JID)
        {
            DataTable Check1 = new DataTable();
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();

                string query3 = "SELECT * FROM UserTable u INNER JOIN TeamTable t ON u.TID = t.TID WHERE  t.TID = @TID ";
                SqlDataAdapter sqlDa1 = new SqlDataAdapter(query3, sqlConn);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@TID", Tid);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@UID", UID);
                sqlDa1.Fill(Check1);
                if(Check1.Rows.Count >= 5)
                {
                    return RedirectToAction("ViewTeam", new { TID = Tid, Check = 9 });
                }

                string query = " UPDATE UserTable SET TID = @TID WHERE UID = @UID ";
                SqlCommand sqlCmd = new SqlCommand(query, sqlConn);
                sqlCmd.Parameters.AddWithValue("@TID", Tid);
                sqlCmd.Parameters.AddWithValue("@UID", UID);

                sqlCmd.ExecuteNonQuery();

                string query1 = " DELETE FROM RequestTable WHERE TID = @TID AND UID = @UID ";
                SqlCommand sqlCmd1 = new SqlCommand(query1, sqlConn);
                sqlCmd1.Parameters.AddWithValue("@TID", Tid);
                sqlCmd1.Parameters.AddWithValue("@UID", UID);

                sqlCmd1.ExecuteNonQuery();

        

           
            }
            HttpContext.Session.SetInt32(TID, Tid);
           

            return RedirectToAction("DeclineJoin", new { tid = Tid, jid = JID });

        }
        //REQUEST HANDLING


        public ActionResult DeclineJoin(int tid, int jid)
        {
            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();
                string query = "DELETE FROM JoinTable WHERE JID = @JID ";
                SqlCommand sqlCmd = new SqlCommand(query, sqlConn);
                sqlCmd.Parameters.AddWithValue("@JID", jid);
                sqlCmd.ExecuteNonQuery();

            }
            return RedirectToAction("ViewTeam", new { TID = tid });
        }

        //PROFILE HANDLING 

        public ActionResult Profile(int UID)
        {
            DataTable Data = new DataTable();
            DataTable Data2 = new DataTable();

            using (SqlConnection sqlConn = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlConn.Open();
                string query = "SELECT * FROM UserTable WHERE UID = @UID";
                SqlDataAdapter sqlCmd = new SqlDataAdapter(query, sqlConn);
                sqlCmd.SelectCommand.Parameters.AddWithValue("@UID", UID);
                sqlCmd.Fill(Data);

                if(HttpContext.Session.GetInt32("UID") != null)
                {
                    string query1 = "SELECT * FROM UserTable WHERE UID = @UID";
                    SqlDataAdapter sqlCmd1 = new SqlDataAdapter(query1, sqlConn);
                    sqlCmd1.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                    sqlCmd1.Fill(Data2);
                    ViewData["Picture"] = Data2.Rows[0]["Picture"];
                    ViewData["UserName"] = Data2.Rows[0]["UserName"];
                    ViewData["rank"] = Data2.Rows[0]["Rank"];
                    ViewData["tagline"] = Data2.Rows[0]["tagline"];

                }


            }

            ViewData["UID"] = UID;
            return View(Data);
        }
        //PROFILE HANDLING



        //LOGIN HANDLING

        public ActionResult LogIn(string? USER,string? PASS, int? valid, string? TAG)
        {
            ViewData["1"] = USER;
            ViewData["2"] = PASS;
            ViewData["3"] = TAG;

            if (valid == 1)
            {
                ViewData["text1"] = "User does not exist";

            }
            else
            {
                ViewData["text1"] = "";
            }
            if (valid == 0)
            {
                ViewData["text"] = "Password Incorrect";

            }
            else
            {
                ViewData["text"] = "";
            }
                return View();
        }

        public ActionResult Lobbies(int? Check)
        {
            DataTable UserTbl = new DataTable();
            DataTable Ben = new DataTable();


            using (SqlConnection sqlCon = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                if(HttpContext.Session.GetInt32("UID") != null)
                {
                    sqlCon.Open();
                    string query = "SELECT * FROM UserTable u INNER JOIN TeamTable t ON t.LID = u.UID WHERE u.UID = @UID";
                    SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));

                    sqlDa.Fill(UserTbl);


                    string query1 = "SELECT * FROM UserTable WHERE UID = @UID";
                    SqlDataAdapter sqlDa1 = new SqlDataAdapter(query1, sqlCon);
                    sqlDa1.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                    sqlDa1.Fill(Ben);

                }





                if (UserTbl.Rows.Count > 0)
                {
                    ViewData["Check"] = 1;
                }

            }
            if(Check == 1)
            {
                ViewData["WIN"] = "Lobby Created!";
            }
            if(Check == 2)
            {
                ViewData["LOOSE"] = "Lobby Already Exists!";

            }

            for(int i = 3; i < 7; i++)
            {
                int y = 7 - i;
                if(Check == i)
                {
                    ViewData["member"] = $"You Must Add {y} Member{(y > 1 ? "s" : "")}!";

                }

            }
          
            if(Check == 10)
            {
                ViewData["JOIN"] = "Lobby Joined Succesfully!";
            }
            if(Check == 11)
            {
                ViewData["FAIL"] = "Unable To Join Lobby, You Must First Create A Team!";

            }
            if(Check == 12)
            {
                ViewData["FAIL"] = "Unable To Join Lobby, You Must First Close Your Current Lobby!";

            }
            if(Check == 13)
            {
                ViewData["FAIL"] = "Unable To Join Lobby, You Must First Leave Your Current Lobby!";

            }
            if(Check == 14)
            {
                ViewData["JOIN"] = "Lobby Left Succesfully!";

            }
            if(Check == 15)
            {
                ViewData["JOIN"] = "Lobby Closed Succesfully!";

            }
            return View(Ben);
        }
            

        public ActionResult CreateLobby()
        {
            DataTable Ben = new DataTable();
            DataTable Check = new DataTable();
            DataTable Zen = new DataTable();



            using (SqlConnection sqlCon = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {

                sqlCon.Open();
                string query1 = "SELECT * FROM LobbyTable l INNER JOIN UserTable u ON l.TeamCreate = u.TID WHERE u.UID = @UID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query1, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                sqlDa.Fill(Ben);
                string query2 = "SELECT * FROM UserTable u INNER JOIN TeamTable t ON u.TID = t.TID WHERE t.TID = @TID";
                SqlDataAdapter sqlDa1 = new SqlDataAdapter(query2, sqlCon);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                sqlDa1.Fill(Check);

                string query3 = "SELECT * FROM LobbyTable l INNER JOIN UserTable u ON l.TeamJoin = u.TID WHERE u.UID = @UID";
                SqlDataAdapter sqlDa2 = new SqlDataAdapter(query3, sqlCon);
                sqlDa2.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                sqlDa2.Fill(Zen);

                if (Check.Rows.Count != 5)
                {
                    if (Check.Rows.Count == 1)
                    {
                        return RedirectToAction("Lobbies", new { Check = 3 });

                    }
                    else if (Check.Rows.Count == 2)
                    {
                        return RedirectToAction("Lobbies", new { Check = 4 });

                    }
                    else if (Check.Rows.Count == 3)
                    {
                        return RedirectToAction("Lobbies", new { Check = 5 });

                    }
                    else if (Check.Rows.Count == 4)
                    {
                        return RedirectToAction("Lobbies", new { Check = 6 });

                    }
                }

                if (Ben.Rows.Count != 0)
                {
                    return RedirectToAction("Lobbies", new { Check = 2 });

                } else if (Zen.Rows.Count != 0) {
                    return RedirectToAction("Lobbies", new { Check = 13 });

                }

                else
                {
                    string query = "INSERT INTO LobbyTable (TeamCreate, Active) VALUES(@TID, @N)";
                    SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                    sqlCmd.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                    sqlCmd.Parameters.AddWithValue("@N", "NO");

                    sqlCmd.ExecuteNonQuery();
                    return RedirectToAction("Lobbies", new { Check = 1 });

                }


            }
        }
        public ActionResult JoinLobby(int lid)
        {
            DataTable Ben = new DataTable();
            DataTable Zen = new DataTable();
            DataTable Check = new DataTable();



            using (SqlConnection sqlCon = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlCon.Open();

                if(HttpContext.Session.GetInt32("UID") != null)
                {
                    string query1 = "SELECT * FROM LobbyTable l INNER JOIN UserTable u ON l.TeamCreate = u.TID WHERE u.UID = @UID ";
                    SqlDataAdapter sqlDa = new SqlDataAdapter(query1, sqlCon);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                    sqlDa.Fill(Ben);

                    string query2 = "SELECT * FROM LobbyTable l INNER JOIN UserTable u ON l.TeamJoin = u.TID WHERE u.UID = @UID ";
                    SqlDataAdapter sqlDa1 = new SqlDataAdapter(query2, sqlCon);
                    sqlDa1.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                    sqlDa1.Fill(Zen);
                }


                string query3 = "SELECT * FROM UserTable u INNER JOIN TeamTable t ON u.TID = t.TID WHERE t.TID = @TID";
                SqlDataAdapter sqlDa2 = new SqlDataAdapter(query3, sqlCon);
                sqlDa2.SelectCommand.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                sqlDa2.Fill(Check);



                if (HttpContext.Session.GetInt32("TID") == 0)
                {
                    return RedirectToAction("Lobbies", new { Check = 11 });

                } else if (Ben.Rows.Count > 0)
                {
                    return RedirectToAction("Lobbies", new { Check = 12 });

                }
             else if (Zen.Rows.Count > 0)
            {
                return RedirectToAction("Lobbies", new { Check = 13 });

            }
               
                
                    else if (Check.Rows.Count == 1)
                    {
                        return RedirectToAction("Lobbies", new { Check = 3 });

                    }
                    else if (Check.Rows.Count == 2)
                    {
                        return RedirectToAction("Lobbies", new { Check = 4 });

                    }
                    else if (Check.Rows.Count == 3)
                    {
                        return RedirectToAction("Lobbies", new { Check = 5 });

                    }
                    else if (Check.Rows.Count == 4)
                    {
                        return RedirectToAction("Lobbies", new { Check = 6 });

                    
                }
                else
                {
                    string query = "UPDATE LobbyTable SET TeamJoin = @TID, Active = @N WHERE LID = @LID";
                    SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                    sqlCmd.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                    sqlCmd.Parameters.AddWithValue("@N", "YES");
                    sqlCmd.Parameters.AddWithValue("@LID", lid);

                    sqlCmd.ExecuteNonQuery();


                    return RedirectToAction("Lobbies", new { Check = 10 });
                }

           

            }



        }




        public ActionResult Lobby(int LID, string Active)
        {
            DataTable Ben = new DataTable();
            DataTable Leader1 = new DataTable();
            DataTable Leader2 = new DataTable();

            using (SqlConnection sqlCon = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlCon.Open();
                string query1 = "SELECT * FROM UserTable u INNER JOIN LobbyTable l ON u.TID = l.TeamCreate WHERE l.LID = @LID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query1, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@LID", LID);
                sqlDa.Fill(Ben);
                if(Active == "YES")
                {
                    string query2 = "SELECT * FROM UserTable u INNER JOIN LobbyTable l ON u.TID = l.TeamJoin WHERE l.LID = @LID";
                    SqlDataAdapter sqlDa1 = new SqlDataAdapter(query2, sqlCon);
                    sqlDa1.SelectCommand.Parameters.AddWithValue("@LID", LID);
                    sqlDa1.Fill(Ben);
                    ViewData["Active"] = 0;
                    string query3 = "SELECT t.LID AS Leader FROM TeamTable t INNER JOIN LobbyTable l ON t.TID = l.TeamJoin WHERE l.LID = @LID";
                    SqlDataAdapter sqlDa2 = new SqlDataAdapter(query3, sqlCon);
                    sqlDa2.SelectCommand.Parameters.AddWithValue("@LID", LID);
                    sqlDa2.Fill(Leader1);
                    ViewData["JoinLeader"] = Leader1.Rows[0]["Leader"];

                }
                else
                {
                    ViewData["Active"] = 1;
 

                }


                string query4 = "SELECT t.LID AS Leader FROM TeamTable t INNER JOIN LobbyTable l ON t.TID = l.TeamCreate WHERE l.LID = @LID";
                SqlDataAdapter sqlDa3 = new SqlDataAdapter(query4, sqlCon);
                sqlDa3.SelectCommand.Parameters.AddWithValue("@LID", LID);
                sqlDa3.Fill(Leader2);

                ViewData["CreateLeader"] = Leader2.Rows[0]["Leader"];
                ViewData["LID"] = LID;

                return View(Ben);
            }
        }
        public ActionResult Lobby2(int LID, string Active)
        {
            DataTable Ben = new DataTable();
            DataTable Leader1 = new DataTable();
            DataTable Leader2 = new DataTable();

            using (SqlConnection sqlCon = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlCon.Open();
                string query1 = "SELECT * FROM UserTable u INNER JOIN HistoryTable l ON u.TID = l.TeamCreate WHERE l.HID = @LID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query1, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@LID", LID);
                sqlDa.Fill(Ben);
                if (Active == "YES")
                {
                    string query2 = "SELECT * FROM UserTable u INNER JOIN HistoryTable l ON u.TID = l.TeamJoin WHERE l.HID = @LID";
                    SqlDataAdapter sqlDa1 = new SqlDataAdapter(query2, sqlCon);
                    sqlDa1.SelectCommand.Parameters.AddWithValue("@LID", LID);
                    sqlDa1.Fill(Ben);
                    ViewData["Active"] = 0;
                    string query3 = "SELECT t.LID AS Leader FROM TeamTable t INNER JOIN HistoryTable l ON t.TID = l.TeamJoin WHERE l.HID = @LID";
                    SqlDataAdapter sqlDa2 = new SqlDataAdapter(query3, sqlCon);
                    sqlDa2.SelectCommand.Parameters.AddWithValue("@LID", LID);
                    sqlDa2.Fill(Leader1);
                    ViewData["JoinLeader"] = Leader1.Rows[0]["Leader"];

                }
                else
                {
                    ViewData["Active"] = 1;


                }


                string query4 = "SELECT t.LID AS Leader FROM TeamTable t INNER JOIN HistoryTable l ON t.TID = l.TeamCreate WHERE l.HID = @LID";
                SqlDataAdapter sqlDa3 = new SqlDataAdapter(query4, sqlCon);
                sqlDa3.SelectCommand.Parameters.AddWithValue("@LID", LID);
                sqlDa3.Fill(Leader2);

                ViewData["CreateLeader"] = Leader2.Rows[0]["Leader"];
                ViewData["LID"] = LID;
                ViewData["NO"] = 1;
                return View("Lobby",Ben);
            }
        }

        public ActionResult StopLobby()
        {
            DataTable Lobby = new DataTable();
            DataTable Messages = new DataTable();

            using (SqlConnection sqlCon = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlCon.Open();
                string query1 = "SELECT * FROM LobbyTable WHERE TeamCreate = @TID";
                SqlDataAdapter sqlCmd1 = new SqlDataAdapter(query1, sqlCon);
                sqlCmd1.SelectCommand.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                sqlCmd1.Fill(Lobby);

                if (Convert.ToString(Lobby.Rows[0]["Active"]) == "YES")
                {
                    string query2 = "INSERT INTO HistoryTable (TeamCreate, TeamJoin, LID) VALUES(@TID1, @TID2,@LID)";
                    SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                    sqlCmd2.Parameters.AddWithValue("@TID1", Lobby.Rows[0]["TeamCreate"]);
                    sqlCmd2.Parameters.AddWithValue("@TID2", Lobby.Rows[0]["TeamJoin"]);
                    sqlCmd2.Parameters.AddWithValue("@LID", Lobby.Rows[0]["LID"]);


                    sqlCmd2.ExecuteNonQuery();

                    string query4 = "DELETE FROM MessageTable WHERE LID = @LID";
                    SqlCommand sqlDa3 = new SqlCommand(query4, sqlCon);
                    sqlDa3.Parameters.AddWithValue("@LID", Lobby.Rows[0]["LID"]);

                    sqlDa3.ExecuteNonQuery();

                }

                string query = "DELETE FROM LobbyTable WHERE TeamCreate = @TID";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                sqlCmd.ExecuteNonQuery();


                return RedirectToAction("Lobbies", new { Check = 15});

            }
        }
        public ActionResult LeaveLobby()
        {
            DataTable Lobby = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlCon.Open();
                string query1 = "SELECT * FROM LobbyTable WHERE TeamJoin = @TID";
                SqlDataAdapter sqlCmd1 = new SqlDataAdapter(query1, sqlCon);
                sqlCmd1.SelectCommand.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));
                sqlCmd1.Fill(Lobby);

                string query2 = "INSERT INTO HistoryTable (TeamCreate, TeamJoin) VALUES(@TID1, @TID2)";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.Parameters.AddWithValue("@TID1", Lobby.Rows[0]["TeamCreate"]);
                sqlCmd2.Parameters.AddWithValue("@TID2", Lobby.Rows[0]["TeamJoin"]);

                sqlCmd2.ExecuteNonQuery();

                string query = "UPDATE LobbyTable SET TeamJoin = NULL, Active = 'NO' WHERE TeamJoin = @TID";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@TID", HttpContext.Session.GetInt32("TID"));

                sqlCmd.ExecuteNonQuery();


                string query4 = "DELETE FROM MessageTable WHERE LID = @LID";
                SqlCommand sqlDa3 = new SqlCommand(query4, sqlCon);
                sqlDa3.Parameters.AddWithValue("@LID", Lobby.Rows[0]["LID"]);

                sqlDa3.ExecuteNonQuery();

                return RedirectToAction("Lobbies", new {Check = 14});

            }
        }

        //MESSAGE HANDLING
        [HttpPost]
        public ActionResult Message(int? lID, int? TID, bool Lobby)
        {
            DataTable Ben = new DataTable();

            using (SqlConnection sqlCon = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlCon.Open();

                if(Lobby == true)
                {
                    string query = "INSERT INTO MessageTable (Content, LID, UID) VALUES(@Content, @LID, @UID)";
                    SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                    sqlCmd.Parameters.AddWithValue("@LID", lID);
                    sqlCmd.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                    sqlCmd.Parameters.AddWithValue("@Content", Request.Form["Content"].ToString());
                    sqlCmd.ExecuteNonQuery();


                    return RedirectToAction("Lobby", new { LID = lID, Active = "YES" });
                }
                else
                {
                    string query = "INSERT INTO MessageTable (Content, TID, UID) VALUES(@Content, @TID, @UID)";
                    SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                    sqlCmd.Parameters.AddWithValue("@TID", TID);
                    sqlCmd.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                    sqlCmd.Parameters.AddWithValue("@Content", Request.Form["Content"].ToString());
                    sqlCmd.ExecuteNonQuery();
                    return RedirectToAction("ViewTeam", new { TID = HttpContext.Session.GetInt32("TID"), Check = 10 });

                }



            }

        }





        //MESSAGE HANDLING

        //LOGIN HANDLING

        public IActionResult LogIn_Check()
        {
            DataTable Ben = new DataTable();
            DataTable UserTbl = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                sqlCon.Open();
                string query = "SELECT * FROM UserTable WHERE Username = @Username AND Password = @Password AND tagline = @tagline";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@Username", Request.Form["Username"].ToString());
                    sqlDa.SelectCommand.Parameters.AddWithValue("@Password", Request.Form["Password"].ToString());
                sqlDa.SelectCommand.Parameters.AddWithValue("@tagline", Request.Form["tagline"].ToString());




                sqlDa.Fill(UserTbl);
            }
            if (UserTbl.Rows.Count > 0)
            {
                HttpContext.Session.SetInt32(UserID, (Int32)UserTbl.Rows[0]["UID"]);
                HttpContext.Session.SetInt32(TID, (Int32)UserTbl.Rows[0]["TID"]);
                HttpContext.Session.SetString(UserName, (string)UserTbl.Rows[0]["UserName"]);

                HttpContext.Session.SetString(InLoggad, "yes");
                ViewData["UID"] = HttpContext.Session.GetInt32("UID");
                HttpContext.Session.GetString("_InloggStatus");
                return RedirectToAction("index");
            }
            else
            {
                using (SqlConnection sqlCon1 = new SqlConnection(GetConnectionString("StackUp.mdf")))
                {
                    string query1 = "SELECT * FROM UserTable WHERE Username = @1Username AND tagline = @tagline";
                    SqlDataAdapter sqlDa1 = new SqlDataAdapter(query1, sqlCon1);
                    sqlDa1.SelectCommand.Parameters.AddWithValue("@1Username", Request.Form["Username"].ToString());
                    sqlDa1.SelectCommand.Parameters.AddWithValue("@tagline", Request.Form["tagline"].ToString());
                    sqlDa1.Fill(Ben);


                }

            }

            if(Ben.Rows.Count > 0)
                {

                    return RedirectToAction("LogIn", new { USER = Request.Form["Username"].ToString(), PASS = Request.Form["Password"].ToString(), TAG = Request.Form["tagline"].ToString(), valid = 0 });
                }
                else
                {
                    return RedirectToAction("LogIn", new { USER = Request.Form["Username"].ToString(), PASS = Request.Form["Password"].ToString(), TAG = Request.Form["tagline"].ToString(), valid = 1 });


                }
            //LOGIN HANDLING



        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");

        }
        //LOGIN HANDLING

        [HttpGet]
        public ActionResult Search2(int? input, int? error, int? ben)
        {
            if (input == 1)
            {
                ViewData["input"] = "";
            }

            if (error == 2)
            {
                ViewData["error"] = "You Have Already Requested To Join This Team";
            }
            else if (error == 3)
            {
                ViewData["success"] = "Successfully Requested To Join Team";
            }

            if (HttpContext.Session.GetString("_InloggStatus") == "yes")
            {
                DataTable nisse = new DataTable();

                using (SqlConnection connection = new SqlConnection(GetConnectionString("StackUp.mdf")))
                {
                    connection.Open();

                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM UserTable WHERE UID = @UID", connection);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));
                    sqlDa.Fill(nisse);

                    HttpContext.Session.SetInt32(TID, (Int32)nisse.Rows[0]["TID"]);

                    if (ben == 1)
                    {
                        ViewData["ben"] = 1;
                    }

                    return View("Search", nisse);
                }
            }

            return View("Search");
        }

        public ActionResult Settings(int? ben)
        {
            DataTable nisse = new DataTable();
            DataTable nisse2 = new DataTable();


            using (SqlConnection connection = new SqlConnection(GetConnectionString("StackUp.mdf")))
            {
                connection.Open();
                string query = "";
                string query2 = "";

                if (HttpContext.Session.GetInt32("TID") != 0)
                {
                     query = "SELECT u.UserName, u.tagline, u.Password, u.Picture, t.Name AS TeamName, t.[Desc] AS TeamDesc, t.TID FROM UserTable u INNER JOIN TeamTable t ON u.TID = t.TID WHERE UID = @UID";
                }
                else {
                    query = "SELECT * FROM UserTable WHERE UID = @UID";

                }

                SqlDataAdapter sqlDa = new SqlDataAdapter(query, connection);

                sqlDa.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));

                sqlDa.Fill(nisse);

                SqlDataAdapter sqlDa1 = new SqlDataAdapter("SELECT * FROM TeamTable t INNER JOIN UserTable u ON t.LID = u.UID WHERE u.UID = @UID", connection);
                sqlDa1.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));

                sqlDa1.Fill(nisse2);
                if (nisse2.Rows.Count > 0)
                {
                    ViewData["Check"] = 1;
                }
                if (ben == 1)
                {
                    ViewData["ben"] = "No Changes Where Made";
                }
                if (ben == 2)
                {
                    ViewData["ben2"] = "User Information Has Been Updated Succesfully";

                }
                if (ben == 3)
                {
                    ViewData["team1"] = "No Changes Where Made";
                }
                if (ben == 4)
                {
                    ViewData["team2"] = "Team Information Has Been Updated Succesfully";

                }


            }

            return View(nisse);

        }




        [HttpPost]
        
        public IActionResult Search( int? input, int? error, int? ben)
        {
        

            if(input == 1)
            {
                ViewData["input"] = "";
            }
            else
            {
                ViewData["input"] = Request.Form["input"].ToString();

            }



            if (error == 2)
            {
                ViewData["error"] = "You Have Already Requested To Join This Team";
            }
            if(error == 3)
            {
                ViewData["success"] = "Succesfully Requested To Join Team";

            }
            if (HttpContext.Session.GetString("_InloggStatus") == "yes")
            {
                DataTable nisse = new DataTable();
                DataTable Team = new DataTable();


                using (SqlConnection connection = new SqlConnection(GetConnectionString("StackUp.mdf")))
                {
                    connection.Open();


                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM UserTable WHERE UID = @UID", connection);

                    sqlDa.SelectCommand.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("UID"));

                    sqlDa.Fill(nisse);

                    HttpContext.Session.SetInt32(TID, (Int32)nisse.Rows[0]["TID"]);


                    if(ben == 1)
                    {
                        ViewData["ben"] = 1;
                    }
                return View(nisse);

                }
            }
            else
            {
                return View();

            }

        }

      




    }

}

