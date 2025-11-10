using System.Collections.Generic;

using iText.Layout;


namespace LaceupMigration
{
    public interface IPdfProvider
    {
        string GetTransferPdf(IEnumerable<InventoryLine> sortedList, bool isOn);

        string GetOrderPdf(Order order);

        string GetInvoicePdf(Invoice invoice);

        string GetConsignmentPdf(Order order, bool counting);

        string GetLoadPdf(Order order);

        string GetOrdersPdf(List<Order> order);

        string GetInvoicesPdf(List<Invoice> invoice);

        string GetGoalPdf(GoalProgressDTO goal);

        string GetPaymentPdf(InvoicePayment payment);

        string GetDepositPdf(BankDeposit bankDeposit);

        string GetReportPdf();
        string  GetStatementReportPdf(Client client);

        public interface IXlsxProvider
        {
            string GetOrderXlsx(Order order);

        }
    } 
}