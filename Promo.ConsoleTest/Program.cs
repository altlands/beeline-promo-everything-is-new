using System;
using System.Configuration;
using Promo.EverythingIsNew.DAL.Cbn;
using Promo.EverythingIsNew.DAL.Cbn.Dto;
using Newtonsoft.Json;

namespace Promo.ConsoleTest
{
    class Program
    {

        public static string CbnUrl = ConfigurationManager.AppSettings["CbnUrl"];
        public static string CbnUser = ConfigurationManager.AppSettings["CbnUser"];
        public static string CbnPassword = ConfigurationManager.AppSettings["CbnPassword"];

        static void Main(string[] args)
        {
            var client = new CbnClient(CbnUrl, CbnUser, CbnPassword);
            var result = client.Update(new Update
            {
                ctn = "111",
                email = "222",
                name = "333",
            }).Result;

            Console.WriteLine(JsonConvert.SerializeObject(result));
            Console.ReadKey();

        }
    }
}
