





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public enum OrderStatus
    {
        In_Process = 0,
        Created = 1,
        Ready_To_Be_Picked = 2,
        Picking_Grouped = 3,
        Ready_To_Be_Checked = 4,
        // Checked = 5,
        Delivering = 6,
        // Delivered = 7,
        Ready_To_Export = 8,
        Exported = 9,
        Void = 10,
        Pre_Exported = 12,
        Shipped = 11,
        StandingOrder = 20,
        SBT = 21
    }
}