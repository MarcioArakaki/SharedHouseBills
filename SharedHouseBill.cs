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
using System.Data;

namespace Personal
{
    public static class SharedHouseBill
    {
        [FunctionName("SharedHouseBill")]
        public static async Task<IActionResult> RunSharedHouseBill(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            List<Bill> bills = new List<Bill>();
            bills = await GetBillsFromDatabase((int?)data?.month);

            string responseMessage = JsonConvert.SerializeObject(bills);
            return new OkObjectResult(responseMessage);
        }

        [FunctionName("AddNewBill")]
        public static async Task<IActionResult> RunAddNewBill(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var bill = JsonConvert.DeserializeObject<Bill>(requestBody);

            InsertNewBillInDb(bill);

            string responseMessage = $"Bill succesfully inserted";
            return new OkObjectResult(responseMessage);
        }

        private static void InsertNewBillInDb(Bill bill)
        {
            var str = System.Environment.GetEnvironmentVariable("ConnectionStrings:SQLConnectionString");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var query = "INSERT INTO Bill (BillTypeId,Value,PaymentDate,DueDate)"
                             + "VALUES(@param1,@param2,@param3,@param4)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@param1", SqlDbType.Int).Value = bill.BillTypeId;
                    cmd.Parameters.Add("@param2", SqlDbType.Float).Value = bill.Value;
                    cmd.Parameters.Add("@param3", SqlDbType.DateTime2).Value = bill.PaymentDate;
                    cmd.Parameters.Add("@param4", SqlDbType.DateTime2).Value = bill.DueDate;
                    cmd.CommandType = CommandType.Text;

                    // Execute the command and log the # rows affected.
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine(e.Message.ToString(), "Error Message");
                    }
                }                
            }
        }

        private static async Task<List<Bill>> GetBillsFromDatabase(int? month = null)
        {
            var str = System.Environment.GetEnvironmentVariable("ConnectionStrings:SQLConnectionString");
            var bills = new List<Bill>();
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var query = "SELECT  * from Bill";
                if (month != null)
                    query += $" WHERE MONTH(PaymentDate) = {month}";

                using (SqlCommand cmd = new SqlCommand(query, conn))
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
