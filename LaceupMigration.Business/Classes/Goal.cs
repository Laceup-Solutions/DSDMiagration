
using System.Collections.Generic;
using System;

namespace LaceupMigration
{
    public class Goal
    {
        public static List<Goal> List = new List<Goal>();

        public Goal()
        {
            this.GoalDetails = new List<GoalDetail>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ExtraFields { get; set; }
        public int Type { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public int WorkingDays { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime ModifiedOn { get; set; }
        public int Criteria { get; set; }

        public List<GoalDetail> GoalDetails { get; set; }

    }
}