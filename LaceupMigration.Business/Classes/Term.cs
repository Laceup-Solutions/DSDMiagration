
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LaceupMigration
{
    public class Term
    {
        public static List<Term> List = new List<Term>();
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int StandardDueDates { get; set; }
        public int StandardDiscountDays { get; set; }
        public double DiscountPercentage { get; set; }
        public string ExtraFields { get; set; }
        public string OriginalId { get; set; }
    }
}