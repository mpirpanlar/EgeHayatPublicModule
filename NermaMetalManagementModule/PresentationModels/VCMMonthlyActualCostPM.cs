using LiveCore.Desktop.UI.Controls;
using Microsoft.Practices.Unity;
using Sentez.Common;
using Sentez.Common.Commands;
using Sentez.Common.Report;
using Sentez.Common.SystemServices;
using Sentez.Common.Utilities;
using Sentez.Data.MetaData;
using Sentez.Data.Tools;
using Sentez.Localization;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sentez.NermaMetalManagementModule.PresentationModels
{
    public partial class VCMMonthlyActualCostPM : ReportPM
    {
        #region Properties

        private string orderCode;
        public string OrderCode
        {
            get { return orderCode; }
            set
            {
                orderCode = value; OnPropertyChanged("OrderCode");
            }
        }
        LiveKeyFieldEdit workorderCodeTxt;
        LiveGridControl grdMonthlyActualCost = null;
        #endregion

        public VCMMonthlyActualCostPM(IUnityContainer container_)
            : base(container_)
        {

        }

        public override void LoadCommands()
        {
            base.LoadCommands();
            CmdList.AddCmd(501, "RefreshCommand", SLanguage.GetString("Yenile"), OnRefreshCommand, null);
        }
        public override void Init()
        {
            base.Init();
            if (pmParam.Tag != null)
                OrderCode = pmParam.Tag.ToString();
            workorderCodeTxt = FCtrl("CodeField") as LiveKeyFieldEdit; if (workorderCodeTxt != null) workorderCodeTxt.KeyDown += workOrderCodeTxt_KeyDown;
            if (grdMonthlyActualCost == null) grdMonthlyActualCost = FCtrl("grdMonthlyActualCost") as LiveGridControl;
            SetActualCostGrid();
            GetMonthlyActualCostData();
        }

        private void GetMonthlyActualCostData()
        {
            if (grdMonthlyActualCost != null && !string.IsNullOrEmpty(OrderCode))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Select (Select top 1 CurrentAccountCode from Erp_CurrentAccount with (nolock) where CurrentAccountId = ac.CurrentAccountId and CompanyId = {activeSession.ActiveCompany.RecId}) CustomerCode ");
                sb.AppendLine($", (Select top 1 CurrentAccountName from Erp_CurrentAccount with (nolock) where CurrentAccountId = ac.CurrentAccountId and CompanyId = {activeSession.ActiveCompany.RecId}) CustomerName ");
                sb.AppendLine($",(Select WorkOrderNo from Erp_WorkOrder wo with (nolock) where wo.RecId in (select WorkOrderId from Erp_WorkOrderItem woi with (nolock) where woi.RecId = ac.WorkOrderItemId) and wo.CompanyId = {activeSession.ActiveCompany.RecId}) WorkOrderNo ");
                sb.AppendLine($",* from Erp_ActualCost ac where CompanyId = {activeSession.ActiveCompany.RecId}");
                if (OrderCode == "Muhtelif Satışlar")
                    sb.AppendLine("and ac.Explanation = 'Muhtelif Satışlar'");
                else
                    sb.AppendLine($"and (Select WorkOrderNo from Erp_WorkOrder wo with (nolock) where RecId in (Select WorkOrderId from Erp_WorkOrderItem woi where woi.RecId = WorkOrderItemId )) in ('{OrderCode}')");
                DataTable dtActualCost = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_ActualCost", sb.ToString());
                grdMonthlyActualCost.ItemsSource = dtActualCost;
            }
        }
        void workOrderCodeTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (workorderCodeTxt != null && ((e.Key == Key.Tab && Keyboard.Modifiers == ModifierKeys.None) || (e.Key == Key.Return && Keyboard.Modifiers == ModifierKeys.None)))
            {
                string sql = "Select isnull(WorkOrderNo,'') WorkOrderNo,RecId as WorkOrderId from Erp_WorkOrder with (nolock) where WorkOrderType=15 and  WorkOrderNo='" + workorderCodeTxt.Text + "' and CompanyId=" + ActiveSession._CompanyInfo.RecId;
                DataTable dtworkOrder = UtilityFunctions.GetDataTableList(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, null, "Erp_WorkOrder", sql);

                if (dtworkOrder != null && dtworkOrder.Rows.Count > 0)
                {
                    OrderCode = dtworkOrder.Rows[0]["WorkOrderNo"].ToString();
                    GetMonthlyActualCostData();
                }
                else
                {
                    sysMng.ActWndMng.ShowMsg(SLanguage.GetString("Order No geçersiz! Lütfen kontrol ediniz."), ConstantStr.Warning);
                    e.Handled = true;
                    return;
                }
            }
            if (e.Key == Key.F9)
            {
                TextBox t = (TextBox)(e.OriginalSource as UIElement);
                if (t == null) return;
                SysMng.Instance.ActWndMng.ShowReport("Erp_WorkOrderList", true, this.WorkListValueHandler, new DlgArgs("RecId"), null, null, "WorkListW", ReportWorkMode.ChoseList);
            }
        }
        public void WorkListValueHandler(DlgArgs result)
        {
            if (result?.DlgReturnValue != null)
            {
                DataTable dt = UtilityFunctions.GetDataTableList(ActiveSession.dbInfo.DBProvider, ActiveSession.dbInfo.Connection, null, "Erp_WorkOrder", "select isnull(WorkOrderNo,'') WorkOrderNo, GLReceiptId from Erp_WorkOrder with (nolock) where RecId = " + result.DlgReturnValue);
                if (dt != null && dt.Rows.Count > 0)
                {
                    OrderCode = dt.Rows[0]["WorkOrderNo"].ToString();
                    GetMonthlyActualCostData();
                }
            }
        }
        private void OnRefreshCommand(ISysCommandParam obj)
        {
            try
            {
                sysMng.ShowWaitCursor();
                GetMonthlyActualCostData();
            }
            finally
            {
                sysMng.ShowArrowCursor();
            }
        }
        private void SetActualCostGrid()
        {
            if (grdMonthlyActualCost == null) return;
            ReceiptColumnCollection columnCollection = new ReceiptColumnCollection();
            columnCollection.Add(new ReceiptColumn { ColumnName = "CustomerCode", IsFixedColumn = true, Caption = SLanguage.GetString("Müşteri Kodu"), Width = 100, DataType = UdtTypes.GetUdtSystemType("UdtName") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "CustomerName", IsFixedColumn = true, Caption = SLanguage.GetString("Müşteri Adı"), Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "WorkOrderNo", IsFixedColumn = true, Caption = SLanguage.GetString("Order No"), Width = 100, DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "ApprovedExplanation", IsFixedColumn = true, Caption = SLanguage.GetString("Order Durumu"), Width = 50, DataType = UdtTypes.GetUdtSystemType("UdtName") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "ShipmentDate", IsFixedColumn = true, Caption = SLanguage.GetString("Yükleme Tarihi"), Width = 80, EditorType = EditorType.DateEditor, UsageType = FieldUsage.Date, DataType = UdtTypes.GetUdtSystemType("UdtDate") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "Year", Caption = SLanguage.GetString("Maliyet Yılı"), Width = 80, DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "Month", Caption = SLanguage.GetString("Maliyet Ayı"), Width = 80, DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "ForexCode", Caption = SLanguage.GetString("Döviz Kodu"), Width = 50, DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "Quantity", Caption = SLanguage.GetString("Order Adet"), Width = 80, IsCalculateTotalColumn = true, DataType = UdtTypes.GetUdtSystemType("UdtQuantity") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "CuttingQuantity", Caption = SLanguage.GetString("Kesim Adet"), Width = 80, IsCalculateTotalColumn = true, DataType = UdtTypes.GetUdtSystemType("UdtQuantity") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "ShipmentQuantity", Caption = SLanguage.GetString("Toplam F.Satış Adeti"), Width = 80, IsCalculateTotalColumn = true, DataType = UdtTypes.GetUdtSystemType("UdtQuantity") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlyShipmentQuantity", Caption = SLanguage.GetString("Dönem F.Satış Adeti"), Width = 80, IsCalculateTotalColumn = true, DataType = UdtTypes.GetUdtSystemType("UdtQuantity") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "RemainingQuantity", Caption = SLanguage.GetString("Kalan Adet"), Width = 80, IsCalculateTotalColumn = true, DataType = UdtTypes.GetUdtSystemType("UdtQuantity") });

            columnCollection.Add(new ReceiptColumn { ColumnName = "YarnCostAmount", Caption = SLanguage.GetString("İplik Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "YarnDyeCostAmount", Caption = SLanguage.GetString("İplik Boya Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "FabricCostAmount", Caption = SLanguage.GetString("Kumaş Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "FabricDyeCostAmount", Caption = SLanguage.GetString("Kumaş Boya Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "FabricPrintCostAmount", Caption = SLanguage.GetString("Kumaş Baskı Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "FabricOther1CostAmount", Caption = SLanguage.GetString("Diğer Fason İşçilik-1"), Width = 110, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "FabricOther2CostAmount", Caption = SLanguage.GetString("Diğer Fason İşçilik-2"), Width = 110, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "TrimCostAmount", Caption = SLanguage.GetString("Aksesuar Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "ReturnCostAmount", Caption = SLanguage.GetString("Artık Hammadde Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "CutCostAmount", Caption = SLanguage.GetString("Kesim Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "PieceWorkCostAmount", Caption = SLanguage.GetString("Parça İşçilik Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "ChemicalCostAmount", Caption = SLanguage.GetString("Kimyasal Maliyet"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });

            columnCollection.Add(new ReceiptColumn { ColumnName = "YarnCostAmount2", Caption = SLanguage.GetString("Dahili İplik Maliyeti "), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "YarnDyeCostAmount2", Caption = SLanguage.GetString("Dahili İplik Boya Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "FabricCostAmount2", Caption = SLanguage.GetString("Dahili Kumaş Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "FabricDyeCostAmount2", Caption = SLanguage.GetString("Dahili Kumaş Boya Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "FabricPrintCostAmount2", Caption = SLanguage.GetString("Dahili Kumaş Baskı Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "FabricOther1CostAmount2", Caption = SLanguage.GetString("Dahili Diğer Fason İşçilik-1"), Width = 110, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "FabricOther2CostAmount2", Caption = SLanguage.GetString("Dahili Diğer Fason İşçilik-2"), Width = 110, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "TrimCostAmount2", Caption = SLanguage.GetString("Dahili Aksesuar Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "ReturnCostAmount2", Caption = SLanguage.GetString("Dahili Artık Hammadde Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "CutCostAmount2", Caption = SLanguage.GetString("Dahili Kesim Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "PieceWorkCostAmount2", Caption = SLanguage.GetString("Dahili Parça İşçilik Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "ChemicalCostAmount2", Caption = SLanguage.GetString("Dahili Kimyasal Maliyet"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });

            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlySalesCost", Caption = SLanguage.GetString("Dönem Satış Tutarı"), Width = 120, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlySalesForexCost", Caption = SLanguage.GetString("Dönem Satış EURO Tutarı"), Width = 120, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlySalesForex2Cost", Caption = SLanguage.GetString("Dönem Satış USD Tutarı"), Width = 120, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlyUnitPriceCost", Caption = SLanguage.GetString("Birim Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.ForexUnitPrice, Width = 90, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlyUnitPriceForexCost", Caption = SLanguage.GetString("Birim EURO Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.ForexUnitPrice, Width = 90, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlyUnitPriceForex2Cost", Caption = SLanguage.GetString("Birim USD Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.ForexUnitPrice, Width = 90, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlyCost", Caption = SLanguage.GetString("Dönem Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlyForexCost", Caption = SLanguage.GetString("Dönem EURO Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlyForex2Cost", Caption = SLanguage.GetString("Dönem USD Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "TotalSalesCost", Caption = SLanguage.GetString("Toplam Satış Tutarı"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "TotalSalesForexCost", Caption = SLanguage.GetString("Toplam Satış EURO Tutarı"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "TotalSalesForex2Cost", Caption = SLanguage.GetString("Toplam Satış USD Tutarı"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "TotalCost", Caption = SLanguage.GetString("Toplam Maliyet"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "TotalForexCost", Caption = SLanguage.GetString("Toplam EURO Maliyet"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "TotalForex2Cost", Caption = SLanguage.GetString("Toplam USD Maliyet"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlyProfit", Caption = SLanguage.GetString("Dönem Kar/Zarar"), Width = 120, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlyForexProfit", Caption = SLanguage.GetString("Dönem EURO Kar/Zarar"), Width = 120, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            columnCollection.Add(new ReceiptColumn { ColumnName = "MonthlyForex2Profit", Caption = SLanguage.GetString("Dönem USD Kar/Zarar"), Width = 120, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            grdMonthlyActualCost.ColumnDefinitions = columnCollection;
        }
        public override void Dispose()
        {
            if (disposed)
                return;
            if (workorderCodeTxt != null) workorderCodeTxt.KeyDown -= workOrderCodeTxt_KeyDown;
            base.Dispose();
        }
    }
}
