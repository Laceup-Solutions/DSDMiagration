





using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    
    public class RouteExpenses
    {
        public static RouteExpenses CurrentExpenses;

        public List<RouteExpenseDetail> Details = new List<RouteExpenseDetail>();

        public string SessionId { get; set; }

        public static void LoadExpenses()
        {
            if (File.Exists(Config.ExpensesPath))
            {
                using (StreamReader reader = new StreamReader(Config.ExpensesPath))
                {
                    string line = reader.ReadLine();

                    var sessionId = line;

                    var routeExpenses = new RouteExpenses();

                    routeExpenses.SessionId = sessionId;

                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split("|");

                        var productId = Convert.ToInt32(parts[0]);
                        var amount = Convert.ToDouble(parts[1]);

                        routeExpenses.Details.Add(new RouteExpenseDetail() { Amount = amount, ProductId = productId });
                    }

                    CurrentExpenses = routeExpenses;
                }
            }
        }

        public static void SaveExpenses(RouteExpenses expenses)
        {
            CurrentExpenses = expenses;

            if (File.Exists(Config.ExpensesPath))
                File.Delete(Config.ExpensesPath);

            using (StreamWriter writer = new StreamWriter(Config.ExpensesPath))
            {
                writer.WriteLine(expenses.SessionId);
                foreach (var l in expenses.Details)
                    writer.WriteLine(l.ProductId + "|" + l.Amount);
            }
        }
    }


    public class RouteExpenseDetail
    {
        public double Amount { get; set; }
        public int ProductId { get; set; }
    }

}