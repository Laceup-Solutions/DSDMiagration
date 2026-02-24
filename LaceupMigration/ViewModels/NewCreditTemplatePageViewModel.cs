using CommunityToolkit.Mvvm.ComponentModel;
using LaceupMigration;
using LaceupMigration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    /// <summary>
    /// ViewModel for the Credit Template screen (same logic as Xamarin NewCreditTemplateActivity).
    /// Same as NewOrderTemplatePageViewModel but Add Credit button is always hidden and title is "Credit".
    /// When opened from NewOrderTemplatePage (Add Credit), back does not run FinalizeOrder so we return to the order template.
    /// </summary>
    public partial class NewCreditTemplatePageViewModel : NewOrderTemplatePageViewModel
    {
        private bool _fromOrderTemplate;

        public NewCreditTemplatePageViewModel(Controls.DialogService dialogService, Services.ILaceupAppService appService)
            : base(dialogService, appService)
        {
            ShowAddCredit = false;
            ActionButtonsColumnDefinitions = "*,*,*,*";
        }

        protected override bool FromCreditTemplate => true;

        protected override string GetTemplateRouteName() => "newcredittemplate";

        public override void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query != null && query.TryGetValue("fromOrderTemplate", out var fromVal) && fromVal != null &&
                int.TryParse(fromVal.ToString(), out var fromInt) && fromInt == 1)
                _fromOrderTemplate = true;
            base.ApplyQueryAttributes(query);
        }

        protected override System.Collections.Generic.IEnumerable<OrderDetail> GetOrderDetailsForSync()
        {
            if (_order?.Details == null) return Enumerable.Empty<OrderDetail>();
            return _order.Details.Where(x => x.IsCredit);
        }

        public void OnShowAddCreditChanged(bool value)
        {
            ShowAddCredit = false;
            ActionButtonsColumnDefinitions = "*,*,*,*";
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();
            ShowAddCredit = false;
            ActionButtonsColumnDefinitions = "*,*,*,*";
            if (_order != null)
                OrderTypeText = _order.AsPresale ? "Credit (Presale)" : "Credit";
        }

        public override async Task GoBackAsync()
        {
            if (_fromOrderTemplate)
            {
                NavigationHelper.RemoveNavigationState(GetTemplateRouteName());
                await Shell.Current.GoToAsync("..");
                return;
            }
            await base.GoBackAsync();
        }

        /// <summary>Credit finalize (mirrors NewCreditTemplateActivity.FinalizeOrder): Order/Bill/Locked just close; empty and non-empty branches.</summary>
        protected override async Task<bool> FinalizeOrderAsync()
        {
            if (_order == null) return true;

            if (_order.OrderType == OrderType.Order || _order.OrderType == OrderType.Bill || _order.Locked())
                return true;

            if (_order.Details.Count == 0)
            {
                if (_order.AsPresale)
                {
                    UpdateRoute(false);
                    var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
                    if (batch != null)
                    {
                        Logger.CreateLog($"Batch with id={batch.Id} DELETED (1 order without details)");
                        batch.Delete();
                    }
                    _order.Delete();
                    return true;
                }
                var o = RouteEx.Routes.FirstOrDefault(x => x.Order != null && x.Order.OrderId == _order.OrderId);
                if (string.IsNullOrEmpty(_order.PrintedOrderId) && o == null)
                {
                    _order.Delete();
                    return true;
                }
                var result = await _dialogService.ShowConfirmAsync(
                    "You have to set all quantities to zero. Do you want to void this order?", "Alert", "Yes", "No");
                if (result)
                {
                    _order.Finished = true;
                    _order.Void();
                    _order.Save();
                    return true;
                }
                return false;
            }

            if (_order.AsPresale && Config.ShipDateIsMandatory && _order.ShipDate.Year == 1)
            {
                await _dialogService.ShowAlertAsync("Please select ship date.", "Alert");
                return false;
            }
            if (_order.AsPresale && Config.SendOrderIsMandatory)
            {
                await _dialogService.ShowAlertAsync("Order must be sent.", "Alert");
                return false;
            }

            if (_order.EndDate == DateTime.MinValue)
            {
                _order.EndDate = DateTime.Now;
                _order.Save();
            }
            if (Session.session != null)
                Session.session.AddDetailFromOrder(_order);
            if (_order.AsPresale)
            {
                UpdateRoute(true);
                if (Config.GeneratePresaleNumber && string.IsNullOrEmpty(_order.PrintedOrderId))
                {
                    _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);
                    _order.Save();
                }
            }
            return true;
        }

        protected override async Task NavigateAfterFinalizeAsync()
        {
            NavigationHelper.RemoveNavigationState(GetTemplateRouteName());
            await Shell.Current.GoToAsync("..");
            if (_order != null && (_order.OrderType == OrderType.Order || _order.OrderType == OrderType.Bill))
                await Shell.Current.GoToAsync($"orderdetails?orderId={_order.OrderId}&asPresale={(_order.AsPresale ? 1 : 0)}");
        }
    }
}
