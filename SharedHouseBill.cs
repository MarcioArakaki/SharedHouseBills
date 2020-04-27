using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Configuration;
using Shared_House_Bills.Models;
using System.Collections.Generic;

namespace Personal
{
    public static class SharedHouseBill
    {
        [FunctionName("SharedHouseBill")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            List<Bill> bills = new List<Bill>();
            bills = await GetBillsFromDatabase();

            string responseMessage = JsonConvert.SerializeObject(bills);
            return new OkObjectResult(responseMessage);
        }

        private static async Task<List<Bill>> GetBillsFromDatabase()
        {
            var str = System.Environment.GetEnvironmentVariable("ConnectionStrings:SQLConnectionString");
            var bills = new List<Bill>();
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var text = "SELECT  * from Bill";

                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    // Execute the command and log the # rows affected.
                    var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        bills.Add(new Bill()
                        {

                            Id = (int)reader["Id"],
                            BillTypeId = (int)reader["BillTypeId"],
                            Value = (double)reader["Value"],
                            PaymentDate = (DateTime?)reader["PaymentDate"],
                            DueDate = (DateTime?)reader["DueDate"],
                        });
                    }
                }
            }

            return bills;
        }
    }
}
