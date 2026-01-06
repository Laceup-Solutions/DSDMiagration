using System.Collections.Generic;

namespace LaceupMigration
{

    public interface IPrinter
    {
        bool PrintOrder(Order order, bool asPreOrder, bool fromBatch = false);

        bool PrintInventory(IEnumerable<Product> SortedList);

        bool PrintSetInventory(IEnumerable<InventoryLine> SortedList);

        bool PrintAddInventory(IEnumerable<InventoryLine> SortedList, bool final);

        bool PrintInventoryCheck(IEnumerable<InventoryLine> SortedList);

        bool PrintSalesCreditReport();

        bool PrintReceivedPaymentsReport(int index, int count);

        bool PrintOrdersCreatedReport(int index, int count);

        bool PrintCreditReport(int index, int count);

        bool ConfigurePrinter();

        bool PrintPayment(InvoicePayment invoicePayment);

        bool PrintTransferOnOff(IEnumerable<InventoryLine> sortedList, bool isOn, bool isFinal, string comment = "", string siteName = "");

        bool PrintRouteReturn(IEnumerable<RouteReturnLine> sortedList, bool isFinal);

        bool PrintOrderLoad(bool isFinal);

        bool InventorySettlement(int index, int count);

        bool InventorySummary(int index, int count, bool isBase = true);


        bool PrintConsignment(Order order, bool asPreOrder, bool printCounting = true, bool printcontract = true, bool allways = false);

        bool PrintBatteryEndOfDay(int index, int count);

        bool PrintOpenInvoice(Invoice invoice);

        bool PrintAcceptLoad(IEnumerable<InventoryLine> SortedList, string docNumber, bool final);

        bool PrintFullConsignment(Order order, bool asPreOrder);

        bool PrintInventoryProd(List<InventoryProd> SortedList);

        bool PrintPaymentBatch();

        bool PrintVehicleInformation(bool fromEOD, int index = 0, int count = 0, bool isReport = false);

        bool PrintClientStatement(Client client);

        bool PrintInventoryCount(List<CycleCountItem> items);

        bool PrintAcceptedOrders(List<Order> orders, bool final);

        bool PrintRefusalReport(int index, int count);

        bool PrintLabels(List<Order> orders);

        bool PrintProductLabel(string label);

        bool PrintProofOfDelivery(Order order);

        bool PrintPickTicket(Order order);
    }
}

