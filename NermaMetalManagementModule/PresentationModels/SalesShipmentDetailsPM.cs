using LiveCore.Desktop.UI.Controls;
using Microsoft.Practices.Unity;
using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.PresentationModels;
using Sentez.Common.SystemServices;
using Sentez.Common.Utilities;
using Sentez.Data.BusinessObjects;
using Sentez.Data.MetaData;
using Sentez.Data.Tools;
using Sentez.Localization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Input;

namespace Sentez.NermaMetalManagementModule.PresentationModels
{
    public partial class SalesShipmentDetailsPM : ReportPM
    {
        #region Properties
        public DateTime detailViewCompareDate;
        DataView SalesDetailsTableView { get; set; }
        DataView ShipmentDetailsTableView { get; set; }

        LiveGridControl grdSales, grdShipment;

        public string inventoryReceiptItemIds;
        List<long> invoiceIdList;
        string invoiceIds;

        DataTable salesDetailsTable, shipmentDetailsTable;
        public DataTable SalesDetailsTable
        {
            get { return salesDetailsTable; }
            set { salesDetailsTable = value; OnPropertyChanged("SalesDetailsTable"); }
        }
        public DataTable ShipmentDetailsTable
        {
            get { return shipmentDetailsTable; }
            set { shipmentDetailsTable = value; OnPropertyChanged("ShipmentDetailsTable"); }
        }
        private object salesSelectedItem;
        public object SalesSelectedItem
        {
            get { return salesSelectedItem; }
            set { salesSelectedItem = value; OnPropertyChanged("SalesSelectedItem"); }
        }
        private object shipmentSelectedItem;
        public object ShipmentSelectedItem
        {
            get { return shipmentSelectedItem; }
            set { shipmentSelectedItem = value; OnPropertyChanged("ShipmentSelectedItem"); }
        }
        private string viewOrderNo;
        public string ViewOrderNo
        {
            get { return viewOrderNo; }
            set { viewOrderNo = value; OnPropertyChanged("ViewOrderNo"); }
        }

        #endregion

        public SalesShipmentDetailsPM(IUnityContainer container_)
            : base(container_)
        {

        }
        public override void LoadCommands()
        {
            base.LoadCommands();
        }
        public override void Init()
        {
            base.Init();
            ViewOrderNo = pmParam.Tag.ToString();
            string QualityCodestr = pmParam.Tag3.ToString();
            string ForexCode = pmParam.TagStr.ToString();
            detailViewCompareDate = Convert.ToDateTime(pmParam.Tag2);
            grdSales = FCtrl<LiveGridControl>("grdSales");
            grdShipment = FCtrl<LiveGridControl>("grdShipment");
            CreateSalesDetailsTable(viewOrderNo,QualityCodestr, ForexCode, detailViewCompareDate);
            CreateShipmentDetailsTable(viewOrderNo, QualityCodestr, ForexCode, detailViewCompareDate);
            if (grdSales != null)
                grdSales.MouseDoubleClick += grdSales_MouseDoubleClick;
            if (grdShipment != null)
                grdShipment.MouseDoubleClick += grdShipment_MouseDoubleClick;
        }
        private void grdSales_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (grdSales.CurrentItem != null)
            {
                BoParam boparam2 = new BoParam(Convert.ToInt32((grdSales.CurrentItem as DataRowView).Row["ReceiptType"]), Convert.ToInt32((grdSales.CurrentItem as DataRowView).Row["RecId"])) { LogicalModuleId = (short)Modules.InventoryModule };
                PmParam pmparam = new PmParam("InvoicePM", "BOCardContext");
                SysCommandParam prm = new SysCommandParam("Invoice", "InvoicePM", pmparam, "InvoiceBO", boparam2, "", "");
                SysMng.Instance.GetCmd("CmdGeneralOpen").Execute(prm);
            }
            else
                SysMng.Instance.activeReceiptBO.ShowMessage("Fatura Seçimi Yapınız.", "Dikkat", Common.InformationMessages.MessageBoxImage.Warning);
        }
        private void grdShipment_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (grdShipment.CurrentItem != null)
            {
                BoParam boparam2 = new BoParam(1, Convert.ToInt32((grdShipment.CurrentItem as DataRowView).Row["PackingListId"])) { LogicalModuleId = (short)Modules.VModule };
                PmParam pmparam = new PmParam("ShipmentPM", "BOCardContext");
                SysCommandParam prm = new SysCommandParam("Shipment", "ShipmentPM", pmparam, "ShipmentBO", boparam2, "", "");
                SysMng.Instance.GetCmd("CmdGeneralOpen").Execute(prm);
            }
            else
                SysMng.Instance.activeReceiptBO.ShowMessage("Çeki Listesi Seçimi Yapınız.", "Dikkat", Common.InformationMessages.MessageBoxImage.Warning);
        }
        /// <summary>
        /// Satış detay tablosu (Faturalar) oluşturuluyor
        /// </summary>
        /// <param name="workOrderNo"></param>
        /// <param name="currentAccountId"></param>
        /// <param name="compareDate"></param>
        /// <returns></returns>
        private DataTable CreateSalesDetailsTable(string workOrderNo, string QualityCode, string ForexCode, DateTime compareDate)
        {
            grdSales.ItemsSource = null;
            SalesDetailsTable = new DataTable("SalesDetailsTable");
            SalesDetailsTableView = new DataView();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Select A.RecId,A.ReceiptType,A.ReceiptNo,A.CurrentAccountName,A.ReceiptDate");
            sb.AppendLine(",SUM(A.Quantity) Quantity,AVG(A.UnitPrice) UnitPrice");
            sb.AppendLine(",A.ForexCode,A.ForexRate");
            sb.AppendLine(",AVG(A.ForexUnitPrice) ForexUnitPrice,AVG(A.DiscountsTotal) DiscountsTotal");
            sb.AppendLine(",AVG(A.DiscountsTotalForex) DiscountsTotalForex,Sum(A.SubTotal) SubTotal,Sum(A.SubTotalForex) SubTotalForex");
            sb.AppendLine("from ");
            sb.AppendLine("(select erp_invoice.RecId,erp_invoice.ReceiptType");
            sb.AppendLine(",erp_invoice.ReceiptNo");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount ca with(nolock) where RecId = [erp_invoice].CurrentAccountId) CurrentAccountName");
            sb.AppendLine(",erp_inventoryreceiptitem.Quantity Quantity");
            sb.AppendLine(",erp_invoice.ReceiptDate");
            sb.AppendLine(",erp_inventoryreceiptitem.UnitPrice UnitPrice");
            sb.AppendLine(",(Select ForexCode from Meta_Forex where RecId = erp_inventoryreceiptitem.ForexId) ForexCode");
            sb.AppendLine(",erp_inventoryreceiptitem.ForexRate");
            sb.AppendLine(",erp_inventoryreceiptitem.ForexUnitPrice ForexUnitPrice");
            sb.AppendLine(",erp_invoice.DiscountsTotal DiscountsTotal");
            sb.AppendLine(",erp_invoice.DiscountsTotalForex DiscountsTotalForex");
            sb.AppendLine(",erp_inventoryreceiptitem.ItemTotal SubTotal");
            sb.AppendLine(",erp_inventoryreceiptitem.ItemTotalForex SubTotalForex");
            sb.AppendLine("from Erp_InventoryReceipt [erp_inventoryreceipt] with (nolock)");
            sb.AppendLine("left join Erp_Invoice [erp_invoice] with (nolock) on ([erp_inventoryreceipt].[InvoiceId] = [erp_invoice].[RecId]) ");
            sb.AppendLine("left join Erp_InventoryReceiptItem [erp_inventoryreceiptitem] with (nolock) ");
            sb.AppendLine("on ([erp_inventoryreceipt].[RecId] = [erp_inventoryreceiptitem].[InventoryReceiptId]) and [erp_inventoryreceiptitem].ItemType = 1");
            sb.AppendLine($"where (erp_invoice.ReceiptType in (120,121) and erp_invoice.CompanyId = {SysMng.Instance.getSession().ActiveCompany.RecId}) and Year([erp_invoice].DischargeDate) = {compareDate.Year} and Month([erp_invoice].DischargeDate) = {compareDate.Month}");
            sb.AppendLine($"and (Select qt.RecId from Erp_QualityType qt with (nolock) where qt.QualityName='{QualityCode}') = erp_inventoryreceiptitem.QualityTypeId ");
            sb.AppendLine($"and [erp_inventoryreceiptitem].WorkOrderReceiptItemId in (Select RecId from Erp_WorkOrderItem woi where woi.WorkOrderId =(Select RecId from Erp_WorkOrder where WorkOrderNo='{workOrderNo}')))A");
            sb.AppendLine($"where A.ForexCode='{ForexCode}'"); 
            sb.AppendLine("group by A.RecId,A.ReceiptNo,A.CurrentAccountName,A.ReceiptType,A.ReceiptDate,A.ForexCode,A.ForexRate");

            SalesDetailsTable = UtilityFunctions.GetDataTableList(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, null, "SalesDetailsTable", sb.ToString());

            ReceiptColumnCollection columnsCollection = new ReceiptColumnCollection
            {
                new ReceiptColumn {ColumnName = "RecId", Caption = SLanguage.GetString("Kayıt Numarası"), Width = 120, IsVisible = false, UsageType = FieldUsage.Integer, DataType = UdtTypes.GetUdtSystemType("UdtInt32") },
                new ReceiptColumn {ColumnName = "ReceiptType", Caption = SLanguage.GetString("Fiş Tipi"), Width = 15, IsVisible = false, UsageType = FieldUsage.Integer, DataType = UdtTypes.GetUdtSystemType("UdtCode") },
                new ReceiptColumn {ColumnName = "ReceiptNo", Caption = SLanguage.GetString("Fatura Numarası"), Width = 100, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn {ColumnName = "CurrentAccountName", Caption = SLanguage.GetString("Cari Hesap Adı"), Width = 150, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn {ColumnName = "Quantity", Caption = SLanguage.GetString("Miktar"), Width = 60, UsageType = FieldUsage.Quantity, DataType = UdtTypes.GetUdtSystemType("UdtQuantity") },
                new ReceiptColumn {ColumnName = "ReceiptDate", Caption = SLanguage.GetString("Fatura Tarihi"), Width = 80, UsageType = FieldUsage.Date, DataType = UdtTypes.GetUdtSystemType("UdtDate") },
                new ReceiptColumn {ColumnName = "UnitPrice", Caption = SLanguage.GetString("Birim Fiyat"), Width = 60, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn {ColumnName = "ForexCode", Caption = SLanguage.GetString("Döviz"), Width = 50, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn {ColumnName = "ForexRate", Caption = SLanguage.GetString("Döviz Kuru"), Width = 70, UsageType = FieldUsage.CostUnitPrice, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn {ColumnName = "ForexUnitPrice", Caption = SLanguage.GetString("Döviz Birim Fiyat"), Width = 80, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn {ColumnName = "DiscountsTotal", Caption = SLanguage.GetString("İndirim Tutarı"), Width = 80, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn {ColumnName = "DiscountsTotalForex", Caption = SLanguage.GetString("Döviz İndirim Tutarı"), Width = 80, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn {ColumnName = "SubTotal", Caption = SLanguage.GetString("Toplam TL Tutar"), Width = 80, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn {ColumnName = "SubTotalForex", Caption = SLanguage.GetString("Toplam Döviz Tutar"), Width = 80, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") }
            };
            grdSales.ColumnDefinitions = columnsCollection;
            SalesDetailsTableView = SalesDetailsTable?.DefaultView;
            grdSales.ItemsSource = SalesDetailsTable;
            return SalesDetailsTable;
        }

        /// <summary>
        /// Sevkiyat detay tablosu (Çeki listeleri) oluşturuluyor
        /// </summary>
        /// <param name="workOrderNo"></param>
        /// <param name="compareDate"></param>
        /// <returns></returns>
        private DataTable CreateShipmentDetailsTable(string workOrderNo, string QualityCode, string ForexCode, DateTime compareDate)
        {
            grdShipment.ItemsSource = null;
            ShipmentDetailsTable = new DataTable("ShipmentDetailsTable");
            ShipmentDetailsTableView = new DataView();
            invoiceIdList = new List<long>();
            if (SalesDetailsTable != null && SalesDetailsTable.Rows.Count > 0)
            {
                foreach (DataRow drInvoice in SalesDetailsTable.Rows)
                {
                    if (!string.IsNullOrEmpty(drInvoice["RecId"].ToString()))
                        invoiceIdList.Add(Convert.ToInt64(drInvoice["RecId"]));
                }
                invoiceIds = string.Join(",", invoiceIdList);
            }
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Select *,isnull(A.ForexUnitPrice * A.TotalQuantity,0) Amount from (Select pl.RecId PackingListId,pl.ReceiptNo,pl.CheckingDate");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId = pl.CurrentAccountId) CurrentAccountName");
            sb.AppendLine(",SUM(isnull(pli.TotalQuantity,0)) TotalQuantity, (Select ForexCode from Meta_Forex where RecId = pli.ForexId) ForexCode, AVG(isnull(pli.ForexUnitPrice,0)) ForexUnitPrice");
            sb.AppendLine(" from Erp_PackingList pl with (nolock) ");
            sb.AppendLine("left join Erp_PackingListItem pli with (nolock) on pl.RecId=pli.PackingListId");
            sb.AppendLine($"where pli.ItemType = 1 and pl.CompanyId = {SysMng.Instance.getSession().ActiveCompany.RecId}");
            sb.AppendLine($"and pli.WorkOrderId in (Select RecId from Erp_WorkOrder where WorkOrderNo='{workOrderNo}') and Year(pl.CheckingDate) = {compareDate.Year} and Month(pl.CheckingDate) = {compareDate.Month}");
            sb.AppendLine($"and pl.UD_Quality='{QualityCode}' and (Select mf.ForexCode from Meta_Forex mf where mf.RecId=pli.ForexId) ='{ForexCode}'");
            sb.AppendLine("Group By pl.RecId,pl.ReceiptNo,pl.CheckingDate,pl.CurrentAccountId,pli.ForexId) A");
            ShipmentDetailsTable = UtilityFunctions.GetDataTableList(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, null, "ShipmentDetailsTable", sb.ToString());

            ReceiptColumnCollection columnsCollection = new ReceiptColumnCollection
            {
                new ReceiptColumn {ColumnName = "PackingListId", Caption = SLanguage.GetString("Kayıt Numarası"), Width = 120, IsVisible = false, UsageType = FieldUsage.Integer, DataType = UdtTypes.GetUdtSystemType("UdtInt32") },
                new ReceiptColumn {ColumnName = "ReceiptNo", Caption = SLanguage.GetString("Çeki Fiş Numarası"), Width = 120, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn {ColumnName = "CheckingDate", Caption = SLanguage.GetString("Çeki Tarihi"), Width = 80, UsageType = FieldUsage.Date, DataType = UdtTypes.GetUdtSystemType("UdtDate") },
                new ReceiptColumn {ColumnName = "CurrentAccountName", Caption = SLanguage.GetString("Cari Hesap Adı"), Width = 120, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn {ColumnName = "TotalQuantity", Caption = SLanguage.GetString("Miktar"), Width = 120, UsageType = FieldUsage.Quantity, DataType = UdtTypes.GetUdtSystemType("UdtQuantity") },
                new ReceiptColumn {ColumnName = "ForexCode", Caption = SLanguage.GetString("Döviz"), Width = 50, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn {ColumnName = "ForexUnitPrice", Caption = SLanguage.GetString("Döviz Birim Fiyat"), Width = 120, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn {ColumnName = "Amount", Caption = SLanguage.GetString("Tutar"), Width = 120, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") }
            };
            grdShipment.ColumnDefinitions = columnsCollection;
            ShipmentDetailsTableView = ShipmentDetailsTable?.DefaultView;
            grdShipment.ItemsSource = ShipmentDetailsTable;
            return ShipmentDetailsTable;
        }

        public override void Dispose()
        {
            if (disposed)
                return;
            if (grdSales != null)
                grdSales.MouseDoubleClick -= grdSales_MouseDoubleClick;
            if (grdShipment != null)
                grdShipment.MouseDoubleClick -= grdShipment_MouseDoubleClick;
            base.Dispose();
        }
    }
}
