using BasicCore7.Data;
using BasicCore7.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BasicCore7.Services
{

    // Middleware class to manage the globals
    public class Globals
    {

        static Timer CleanUpTimer;
        static public string EmailAccount { get; set; }
        static public string EmailEmail { get; set; }
        static public string EmailPassword { get; set; }
        static public int EmailPort { get; set; }
        static public bool EmailSecurity { get; set; }
        static public string EmailSender { get; set; }
        static public string EmailServer { get; set; }

        struct UserStatistics
        {
            public DateTime FirstEntered { get; set; }
            public DateTime LastEntered { get; set; }
            public int Count { get; set; }
            public BasicCore7User User { get; set; }
        }


        readonly RequestDelegate _next; // verwijzing naar volgende middelware methode
        static Dictionary<string, UserStatistics> UserDictionary = new Dictionary<string, UserStatistics>(); 


        // Middleware constructor
        public Globals(RequestDelegate next)
        {
            _next = next;
            CleanUpTimer = new Timer(CleanUp, null, 21600000, 21600000);
        }


        // Middleware task
        public async Task Invoke(HttpContext httpContext, BasicCore7DbContext dbContext)
        {
            // Haal de gebruikersnaam op
            string name = httpContext.User.Identity.Name == null ? "dummy" : httpContext.User.Identity.Name;
            
            
            try  // Als de gebruiker al eerder opgehaald werd
            {
                UserStatistics us = UserDictionary[name];
                us.Count++;
                us.LastEntered = DateTime.Now;
            }
            catch // Als die nog niet opgehaald werd: Doe dat dan nu
            {
                AddUser(name, dbContext);
            }

            // Voer de volgende middleware task uit
            await _next(httpContext);
        }

        private static void AddUser(string name, BasicCore7DbContext dbContext, int count = 1)
        {
            BasicCore7User user = dbContext.Users
            .FirstOrDefault(u => u.UserName == name);

            // Voeg de gebruiker aan de dictionary toe
            UserDictionary[name] = new UserStatistics
            {
                User = user,
                Count = 1,
                FirstEntered = DateTime.Now,
                LastEntered = DateTime.Now
            };

        }

        public static BasicCore7User GetUser(string? userName)
        {
            return UserDictionary[userName==null ? "dummy" : userName].User;
        }


        // Voer een periodieke cleanup uit
        private void CleanUp(object? state)
        {
            DateTime checkTime = DateTime.Now - new TimeSpan(0, 6, 0, 0, 0);
            Dictionary<string, UserStatistics> remove = new Dictionary<string, UserStatistics>();
            foreach (KeyValuePair<string, UserStatistics> us in UserDictionary)
            {
                if (us.Value.LastEntered < checkTime)
                    remove[us.Key] = us.Value;
            }
            foreach (KeyValuePair<string, UserStatistics> us in remove)
                UserDictionary.Remove(us.Key);
        }

        public static void ReloadUser(string userName, BasicCore7DbContext dbContext)
        {
            int count = UserDictionary[userName].Count;
            UserDictionary.Remove(userName);
            AddUser(userName, dbContext, count);
        }


        public static void ApplyGlobals(BasicCore7DbContext context)
        {
            List<Global> parameters = context.Globals.ToList();
            Globals.EmailServer = parameters.FirstOrDefault(p => p.Id == "EmailServer").Value;
            Globals.EmailPort = Convert.ToInt32(parameters.FirstOrDefault(p => p.Id == "EmailPort").Value);
            Globals.EmailAccount = parameters.FirstOrDefault(p => p.Id == "EmailAccount").Value;
            Globals.EmailPassword = parameters.FirstOrDefault(p => p.Id == "EmailPassword").Value;
            Globals.EmailEmail = parameters.FirstOrDefault(p => p.Id == "EmailEmail").Value;
            Globals.EmailSender = parameters.FirstOrDefault(p => p.Id == "EmailSender").Value;
            Globals.EmailSecurity = bool.Parse(parameters.FirstOrDefault(p => p.Id == "EmailSecurity").Value);
            Globals.EmailSecurity = false;  // true zet ssl or tls aan
        }

    }
}
