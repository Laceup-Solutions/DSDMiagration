using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class CreditsByReasonPrinter : ZebraFourInchesPrinter1
    {
        protected override void FillDictionary()
        {
            base.FillDictionary();

            linesTemplates[OrderDetailsHeaderSectionName] = "^CF0,25^FO400,{0}^FD{1}^FS";
        }

        protected override IEnumerable<string> GetDetailsRowsInOneDoc(ref int startY, bool preOrder, Dictionary<string, OrderLine> sales, Dictionary<string, OrderLine> credit, Dictionary<string, OrderLine> returns, Order order)
        {
            if (!Config.CreditReasonInLine)
                return base.GetDetailsRowsInOneDoc(ref startY, preOrder, sales, credit, returns, order);

            List<string> list = new List<string>();

            list.AddRange(GetDetailTableHeader(ref startY));

            var s = string.Empty;
            s = new string('-', WidthForNormalFont - s.Length) + s;
            list.Add(string.Format(CultureInfo.InvariantCulture, linesTemplates[OrderDetailsLineSeparator], startY, s));
            startY += font18Separation;

            int factor = 1;
            // anderson crap
            if (order.Client.ExtraProperties != null)
            {
                var terms = order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "PRINTPRICE");
                if (terms != null && terms.Item2 == "N")
                    factor = 0;
            }

            if (sales.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, sales.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(sales.Values.ToList());

                var listXX = lines.ToList();
                var relatedDetailIds = listXX.Where(x => x.OrderDetail.RelatedOrderDetail > 0).Select(x => x.OrderDetail.RelatedOrderDetail).ToList();
                var removedList = listXX.Where(x => relatedDetailIds.Contains(x.OrderDetail.OrderDetailId)).ToList();
                foreach (var r in removedList)
                    listXX.Remove(r);
                // reinsert
                // If grouping, add at the end
                if (Config.GroupRelatedWhenPrinting)
                {
                    foreach (var r in removedList)
                        listXX.Add(r);
                }
                else
                    foreach (var r in removedList)
                    {
                        for (int index = 0; index < listXX.Count; index++)
                            if (listXX[index].OrderDetail.RelatedOrderDetail == r.OrderDetail.OrderDetailId)
                            {
                                listXX.Insert(index + 1, r);
                                break;
                            }
                    }


                list.AddRange(GetSectionRowsInOneDoc(ref startY, listXX, GetOrderDetailSectionHeader(-1), factor == 0 ? 0 : 1, order, preOrder));
                startY += font36Separation;
            }

            if (credit.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, credit.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(credit.Values.ToList());

                var grouped = new Dictionary<int, List<OrderLine>>();
                foreach (var item in lines)
                {
                    var r = item.OrderDetail.ReasonId;
                    if (!grouped.ContainsKey(r))
                        grouped.Add(r, new List<OrderLine>());
                    grouped[r].Add(item);
                }

                foreach (var g in grouped)
                {
                    var reason = Reason.Find(g.Key);

                    list.AddRange(GetSectionRowsInOneDoc(ref startY, g.Value, reason != null ? reason.Description : string.Empty, factor == 0 ? 0 : -1, order, preOrder));
                    startY += font36Separation;
                }
                
            }
            if (returns.Keys.Count > 0)
            {
                IQueryable<OrderLine> lines;

                if (Config.UseDraggableTemplate)
                    lines = SortDetails.SortedDetails(order.Client.ClientId, returns.Values.ToList());
                else
                    lines = SortDetails.SortedDetails(returns.Values.ToList());

                var grouped = new Dictionary<int, List<OrderLine>>();
                foreach (var item in lines)
                {
                    var r = item.OrderDetail.ReasonId;
                    if (!grouped.ContainsKey(r))
                        grouped.Add(r, new List<OrderLine>());
                    grouped[r].Add(item);
                }

                foreach (var g in grouped)
                {
                    var reason = Reason.Find(g.Key);

                    list.AddRange(GetSectionRowsInOneDoc(ref startY, g.Value, reason != null ? reason.Description : string.Empty, factor == 0 ? 0 : -1, order, preOrder));
                    startY += font36Separation;
                }
            }

            return list;
        }

    }
}