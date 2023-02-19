using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Grid;
using LiveCore.Desktop.UI.Controls;
using Microsoft.Practices.Unity;
using Sentez.Common;
using Sentez.Common.Commands;
using Sentez.Common.Report;
using Sentez.Common.SystemServices;
using Sentez.Common.Utilities;
using Sentez.Core.ParameterClasses;
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
    public partial class OrderAllHistoryPM : ReportPM
    {
        #region Properties
        LiveTreeListView AllocationHistoryView;

        private string workOrderNo;
        public string WorkOrderNo
        {
            get { return workOrderNo; }
            set
            {
                workOrderNo = value; OnPropertyChanged("WorkOrderNo");
            }
        }
        LiveKeyFieldEdit workorderCodeTxt;
        LiveGridControl grdOrderAll = null;

        private DataTable orderHistoryTable;
        public DataTable OrderHistoryTable
        {
            get { return orderHistoryTable; }
            set
            {
                orderHistoryTable = value;
                OnPropertyChanged("OrderHistoryTable");
            }
        }
        private ReceiptColumnCollection orderAllHistoryColumnCollection;
        public ReceiptColumnCollection OrderAllHistoryColumnCollection
        {
            get { return orderAllHistoryColumnCollection; }
            set
            {
                orderAllHistoryColumnCollection = value;
                OnPropertyChanged("OrderAllHistoryColumnCollection");
            }
        }
        #endregion

        public OrderAllHistoryPM(IUnityContainer container_) : base(container_) {  }

        public override void LoadCommands()
        {
            base.LoadCommands();
            CmdList.AddCmd(501, "RefreshCommand", SLanguage.GetString("Yenile"), OnRefreshCommand, null);
        }
        public override void Init()
        {
            base.Init();
            if (pmParam.Tag != null)
                WorkOrderNo = pmParam.Tag.ToString();
            workorderCodeTxt = FCtrl("CodeField") as LiveKeyFieldEdit; if (workorderCodeTxt != null) workorderCodeTxt.KeyDown += workOrderCodeTxt_KeyDown;
            if (grdOrderAll == null) grdOrderAll = FCtrl("grdOrderAll") as LiveGridControl;
            AllocationHistoryView = FCtrl("AllocationHistoryView") as LiveTreeListView;
            SetOrderAllocationHistoryGrid();
            InitOrderAllGrid();
            if (WorkOrderNo != null)
                GetOrderAllocationHistoryData();
        }
        void InitOrderAllGrid()
        {
            if (grdOrderAll != null)
            {
                var liveTreeListView = grdOrderAll.View as LiveTreeListView;
                if (liveTreeListView != null)
                {
                    liveTreeListView.AllowConditionalFormattingMenu = true;
                    liveTreeListView.FormatConditions.Add(new FormatCondition() { Format = new Format() { FontWeight = FontWeights.Bold }, Expression = "[ParentId] == 0" });
                }
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
                    WorkOrderNo = dtworkOrder.Rows[0]["WorkOrderNo"].ToString();
                    GetOrderAllocationHistoryData();
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
                    WorkOrderNo = dt.Rows[0]["WorkOrderNo"].ToString();
                    GetOrderAllocationHistoryData();
                }
            }
        }
        private void OnRefreshCommand(ISysCommandParam obj)
        {
            try
            {
                sysMng.ShowWaitCursor();
                GetOrderAllocationHistoryData();
            }
            finally
            {
                sysMng.ShowArrowCursor();
            }
        }
        private void GetOrderAllocationHistoryData()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"select case when A.ModulNo in (2,3) then ROW_NUMBER() OVER(ORDER BY ParentId,Id ASC) + A.ParentId else  A.Id end ParentRecId, * from (");
            sb.AppendLine($"select 1 ModulNo, R.RecId Id, 0 ParentId,R.InventoryId,'' ReceiptTypeName,'' ReceiptSubTypeName, '' ReceiptDate,WO.WorkOrderNo");
            sb.AppendLine($",ISNULL((Select CurrentAccountCode from Erp_CurrentAccount CA with(nolock) where CA.RecId = WO.CurrentAccountId), '') CurrentAccountCode, ISNULL((Select CurrentAccountName from Erp_CurrentAccount CA with(nolock) where CA.RecId = WO.CurrentAccountId), '') CurrentAccountName,'' ReceiptNo,'' PartyNo,'' DocumentNo,'' Explanation, 0 AllocationQuantity, ");
            sb.AppendLine($"0 Quantity, 0 WastageQuantity, 0 GrossQuantity, 0 IAGrossQuantity, 0 RebateAllocationQuantity, (select  top 1 UnitCode from Meta_UnitSetItem where RecId=R.UnitId) UnitCode, 0 UnitPrice, 0 Amount, '' TermDate, '' ForexCode, 0 ForexUnitPrice, 0 ForexAmount, '' AllocationReceiptDate,'' Warehouse");
            sb.AppendLine($",I.InventoryCode InventoryCode,I.InventoryName InventoryName,");
            sb.AppendLine($"'' ItemVariant1Code,'' ItemVariant2Code,'' ItemVariant3Code,");
            sb.AppendLine($"'' ItemVariant1Name,'' ItemVariant2Name,'' ItemVariant3Name");
            sb.AppendLine($"from Erp_Requirement R with (nolock)");
            sb.AppendLine($"left join Erp_Inventory I with (nolock) on I.RecId=R.InventoryId");
            sb.AppendLine($"left join Erp_WorkOrder WO with (nolock) on WO.RecId=R.WorkOrderId");
            sb.AppendLine($"Left Join Erp_InventoryVariant as invv with(nolock) On R.InventoryVariantId = invv.RecId");
            sb.AppendLine($"where WO.WorkOrderType=15 and WO.CompanyId={activeSession.ActiveCompany.RecId} and WO.WorkOrderNo='{WorkOrderNo}' and R.RecId is not null");
            sb.AppendLine($"union all select 2 ModulNo, ia.RecId Id, R.RecId ParentId,R.InventoryId,");
            sb.AppendLine(OrderReceiptType.GetOrderReceiptTypeSQLStr(0, "irt.ReceiptType", "ReceiptTypeName"));
            if (!string.IsNullOrEmpty(ActiveSession.ParamService.GetParameterClass<InventoryParameters>().SubContractorTypeValues))
                sb.AppendLine("," + InventoryReceiptType.GetInventoryReceiptSubTypeSQLStr(0, "irt.ReceiptSubType", "ReceiptSubTypeName"));
            else
                sb.AppendLine(",'' as ReceiptSubTypeName");
            sb.AppendLine(",ir.ReceiptDate,WO.WorkOrderNo WorkOrderNo,ca.CurrentAccountCode,ca.CurrentAccountName,ir.ReceiptNo as ReceiptNo,irt.PartyNo as PartyNo,ir.DocumentNo as DocumentNo,irt.Explanation as Explanation");
            sb.AppendLine(",isnull((ia.Quantity),0) as AllocationQuantity,isnull((irt.Quantity),0) as Quantity,isnull((ia.WastageQuantity),0) as WastageQuantity,isnull(irt.GrossQuantity,0) GrossQuantity,isnull(ia.GrossQuantity,0) IAGrossQuantity,0 RebateAllocationQuantity,(select top 1 isnull(UnitCode,'')UnitCode from Meta_UnitSetItem with (nolock) where RecId=irt.UnitId) UnitCode,irt.UnitPrice UnitPrice,(isnull((irt.UnitPrice),0)*isnull((ia.Quantity),0)) as Amount ,irt.DeliveryDate as TermDate");
            sb.AppendLine(",(select TOP 1 ForexCode from Meta_Forex MF with (nolock) where MF.RecId = irt.ForexId) as ForexCode");
            sb.AppendLine(",irt.ForexUnitPrice ForexUnitPrice,(isnull((irt.ForexUnitPrice),0)*isnull((ia.Quantity),0)) as ForexAmount");
            sb.AppendLine(", ia.ReceiptDate as AllocationReceiptDate");
            sb.AppendLine($",(select top 1 COALESCE(w.WarehouseCode,'')+'-'+COALESCE(w.WarehouseName,'') from Erp_Warehouse w where w.CompanyId = {ActiveSession.ActiveCompany.RecId} and w.RecId = irt.WarehouseId ) Warehouse");
            sb.AppendLine($",I.InventoryCode InventoryCode,I.InventoryName InventoryName,");
            sb.AppendLine($"ISNULL(vi1.ItemCode, '') ItemVariant1Code,ISNULL(vi2.ItemCode, '') ItemVariant2Code,ISNULL(vi3.ItemCode, '') ItemVariant3Code,");
            sb.AppendLine($"ISNULL(vi1.ItemName, '') ItemVariant1Name,ISNULL(vi2.ItemName, '') ItemVariant2Name,ISNULL(vi3.ItemName, '') ItemVariant3Name");
            sb.AppendLine($"from Erp_Requirement R with (nolock)");
            sb.AppendLine($"Left Join Erp_InventoryAllocation ia with(nolock) on R.RecId=ia.RequirementId ");
            sb.AppendLine($"left join Erp_OrderReceiptItem irt  with (nolock) on irt.RecId=ia.OrderReceiptItemId ");
            sb.AppendLine($"left join Erp_OrderReceipt ir  with (nolock) on ir.RecId=irt.OrderReceiptId ");
            sb.AppendLine($"left join Erp_CurrentAccount ca  with (nolock) on ca.RecId=ir.CurrentAccountId ");
            sb.AppendLine($"left join Erp_Inventory I with (nolock) on I.RecId=R.InventoryId");
            sb.AppendLine($"left join Erp_WorkOrder WO with (nolock) on WO.RecId=R.WorkOrderId");
            sb.AppendLine("Left Join Erp_InventoryVariant as invv with(nolock) On ia.InventoryVariantId = invv.RecId");
            sb.AppendLine($"Left Join Erp_VariantItem as vi1 with(nolock) On invv.Variant1Id = vi1.RecId Left Join Erp_VariantItem as vi2 with(nolock) On invv.Variant2Id = vi2.RecId");
            sb.AppendLine($"Left Join Erp_VariantItem as vi3 with(nolock) On invv.Variant3Id = vi3.RecId");
            sb.AppendLine($" where WO.WorkOrderType=15 and WO.CompanyId={activeSession.ActiveCompany.RecId} and WO.WorkOrderNo='{WorkOrderNo}' and ia.RecId is not null");
            sb.AppendLine($" union all select 3 ModulNo, ia.RecId Id, R.RecId ParentId,R.InventoryId,");
            sb.AppendLine(InventoryReceiptType.GetInventoryReceiptTypeSQLStr(0, "irt.ReceiptType", "ReceiptTypeName"));
            if (!string.IsNullOrEmpty(ActiveSession.ParamService.GetParameterClass<InventoryParameters>().SubContractorTypeValues))
                sb.AppendLine("," + InventoryReceiptType.GetInventoryReceiptSubTypeSQLStr(0, "irt.ReceiptSubType", "ReceiptSubTypeName"));
            else
                sb.AppendLine(",'' as ReceiptSubTypeName");
            sb.AppendLine(",ir.ReceiptDate,WO.WorkOrderNo WorkOrderNo,ca.CurrentAccountCode,ca.CurrentAccountName,ir.ReceiptNo as ReceiptNo,irt.PartyNo as PartyNo,ir.DocumentNo as DocumentNo,irt.Explanation as Explanation");
            sb.AppendLine($",case when irt.ReceiptType = 29 and (ia.ReturnType = 1) then 0 else  isnull((ia.Quantity),0) end as AllocationQuantity,isnull((irt.Quantity),0) as Quantity,isnull((ia.WastageQuantity),0) as WastageQuantity,isnull(irt.GrossQuantity,0) GrossQuantity,isnull(ia.GrossQuantity,0) IAGrossQuantity,(case when irt.ReceiptType = 29 then (select isnull(sum(isnull(IAS.Quantity,0)),0) from Erp_InventoryAllocation IAS with(nolock) where IAS.InventoryReceiptItemId = irt.RecId and IAS.ReturnType = 1 and IAS.WorkOrderItemId in (select woi.RecId from Erp_WorkOrderItem woi with(nolock) left join Erp_WorkOrder wo with(nolock) on woi.WorkOrderId = wo.RecId where wo.CompanyId={ActiveSession.ActiveCompany.RecId} and WorkOrderNo='{WorkOrderNo}')) else 0 end) RebateAllocationQuantity,(select top 1 isnull(UnitCode,'')UnitCode from Meta_UnitSetItem with (nolock) where RecId=irt.UnitId) UnitCode,irt.UnitPrice UnitPrice,(isnull((irt.UnitPrice),0)*isnull((ia.Quantity),0)) as Amount  ,'' as TermDate");
            sb.AppendLine(",(select TOP 1 ForexCode from Meta_Forex MF with (nolock) where MF.RecId = irt.ForexId) as ForexCode");
            sb.AppendLine(",irt.ForexUnitPrice ForexUnitPrice,(isnull((irt.ForexUnitPrice),0)*isnull((ia.Quantity),0)) as ForexAmount");
            sb.AppendLine(", ia.ReceiptDate as AllocationReceiptDate");
            sb.AppendLine($",(select top 1 COALESCE(w.WarehouseCode,'')+'-'+COALESCE(w.WarehouseName,'') from Erp_Warehouse w where w.CompanyId = {ActiveSession.ActiveCompany.RecId} and w.RecId = (case when irt.InWarehouseId is null then irt.OutWarehouseId when irt.OutWarehouseId is null then irt.InWarehouseId when irt.InWarehouseId is not null and irt.OutWarehouseId is not null then irt.InWarehouseId else 0 end)) Warehouse");
            sb.AppendLine($",I.InventoryCode InventoryCode,I.InventoryName InventoryName,");
            sb.AppendLine($"ISNULL(vi1.ItemCode, '') ItemVariant1Code,ISNULL(vi2.ItemCode, '') ItemVariant2Code,ISNULL(vi3.ItemCode, '') ItemVariant3Code,");
            sb.AppendLine($"ISNULL(vi1.ItemName, '') ItemVariant1Name,ISNULL(vi2.ItemName, '') ItemVariant2Name,ISNULL(vi3.ItemName, '') ItemVariant3Name");
            sb.AppendLine($"from Erp_Requirement R with (nolock)");
            sb.AppendLine($"Left Join Erp_InventoryAllocation ia with(nolock) on R.RecId=ia.RequirementId ");
            sb.AppendLine($"left join Erp_InventoryReceiptItem irt  with (nolock) on irt.RecId=ia.InventoryReceiptItemId ");
            sb.AppendLine($"left join Erp_InventoryReceipt ir  with (nolock) on ir.RecId=irt.InventoryReceiptId ");
            sb.AppendLine($"left join Erp_CurrentAccount ca  with (nolock) on ca.RecId=ir.CurrentAccountId ");
            sb.AppendLine($"left join Erp_Inventory I with (nolock) on I.RecId=R.InventoryId");
            sb.AppendLine($"left join Erp_WorkOrder WO with (nolock) on WO.RecId=R.WorkOrderId");
            sb.AppendLine($"Left Join Erp_InventoryVariant as invv with(nolock) On ia.InventoryVariantId = invv.RecId");
            sb.AppendLine($"Left Join Erp_VariantItem as vi1 with(nolock) On invv.Variant1Id = vi1.RecId Left Join Erp_VariantItem as vi2 with(nolock) On invv.Variant2Id = vi2.RecId");
            sb.AppendLine($"Left Join Erp_VariantItem as vi3 with(nolock) On invv.Variant3Id = vi3.RecId");
            sb.AppendLine($" where WO.WorkOrderType=15 and WO.CompanyId={activeSession.ActiveCompany.RecId} and WO.WorkOrderNo='{WorkOrderNo}' and ia.RecId is not null) A");
            OrderHistoryTable = UtilityFunctions.GetDataTableList(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, null, "OrderHistoryTable", sb.ToString());
            AllocationHistoryView.ExpandAllNodes();
        }
        private void SetOrderAllocationHistoryGrid()
        {
            if (grdOrderAll == null) return;

            orderAllHistoryColumnCollection = new ReceiptColumnCollection();
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ParentRecId", Caption = SLanguage.GetString("Id"), Width = 30, EditorType = EditorType.ReadOnlyTextEditor, IsVisible = false, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InitialCostItem"].Fields["RecId"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ParentId", Caption = SLanguage.GetString("Baglı Kayıt No"), Width = 30, EditorType = EditorType.ReadOnlyTextEditor, IsVisible = false, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InitialCostItem"].Fields["RecId"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "WorkOrderNo", Caption = SLanguage.GetString("Order No"), Width = 100, DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "InventoryCode", Caption = SLanguage.GetString("Model Kodu"), Width = 100, DataType = UdtTypes.GetUdtSystemType("UdtName") });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "InventoryName", Caption = SLanguage.GetString("Model Adı"), Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ItemVariant1Code", TableName = "Erp_Requirement", Cacheable = false, Caption = SLanguage.GetString("Varyant-1"), Width = 80, IsVisible = true, EditorType = EditorType.ListSelector, LookUpTable = "Erp_VariantItem", LookUpField = "ItemCode", LookUpFieldCaption = SLanguage.GetString("Varyant Kodu"), ListIdField = "Variant1Id", ListWorkListName = "Erp_VariantItemItemCodeList", DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_VariantItem"].Fields["ItemCode"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ItemVariant1Name", Caption = SLanguage.GetString("Varyant-1 Açıklaması"), Width = 120, IsReadOnly = true, IsVisible = true, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_VariantItem"].Fields["ItemName"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ItemVariant2Code", TableName = "Erp_Requirement", Cacheable = false, Caption = SLanguage.GetString("Varyant-2"), Width = 80, IsVisible = true, EditorType = EditorType.ListSelector, LookUpTable = "Erp_VariantItem", LookUpField = "ItemCode", LookUpFieldCaption = SLanguage.GetString("Varyant Kodu"), ListIdField = "Variant2Id", ListWorkListName = "Erp_VariantItemItemCodeList", DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_VariantItem"].Fields["ItemCode"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ItemVariant2Name", Caption = SLanguage.GetString("Varyant-2 Açıklaması"), Width = 120, IsReadOnly = true, IsVisible = true, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_VariantItem"].Fields["ItemName"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ItemVariant3Code", TableName = "Erp_Requirement", Cacheable = false, Caption = SLanguage.GetString("Varyant-3"), Width = 80, IsVisible = false, EditorType = EditorType.ListSelector, LookUpTable = "Erp_VariantItem", LookUpField = "ItemCode", LookUpFieldCaption = SLanguage.GetString("Varyant Kodu"), ListIdField = "Variant3Id", ListWorkListName = "Erp_VariantItemItemCodeList", DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_VariantItem"].Fields["ItemCode"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ItemVariant3Name", Caption = SLanguage.GetString("Varyant-3 Açıklaması"), Width = 120, IsReadOnly = true, IsVisible = false, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_VariantItem"].Fields["ItemName"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ModuleNo", Caption = SLanguage.GetString("Modül"), Width = 50, IsReadOnly = true, IsVisible = false, DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ReceiptDate", Caption = SLanguage.GetString("Tarih"), Width = 80, IsReadOnly = true, IsVisible = true, EditorType = EditorType.DateEditor, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryReceipt"].Fields["ReceiptDate"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ReceiptTypeName", Caption = SLanguage.GetString("İşlem Tipi"), Width = 150, IsReadOnly = true, IsVisible = true, DataType = UdtTypes.GetUdtSystemType("UdtName") });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ReceiptSubTypeName", Caption = SLanguage.GetString("Alt Tipi"), Width = 80, IsReadOnly = true, IsVisible = true, DataType = UdtTypes.GetUdtSystemType("UdtName") });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ReceiptNo", Caption = SLanguage.GetString("Fiş No"), Width = 80, IsReadOnly = true, IsVisible = false, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryReceipt"].Fields["ReceiptNo"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "CurrentAccountCode", Caption = SLanguage.GetString("Cari Hesap Kodu"), Width = 90, IsReadOnly = true, IsVisible = true, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_CurrentAccount"].Fields["CurrentAccountCode"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "CurrentAccountName", Caption = SLanguage.GetString("Cari Hesap Adı"), Width = 140, IsReadOnly = true, IsVisible = true, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_CurrentAccount"].Fields["CurrentAccountName"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "PartyNo", Caption = SLanguage.GetString("Parti No"), Width = 80, IsReadOnly = true, IsVisible = true, DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "DocumentNo", Caption = SLanguage.GetString("Belge No"), Width = 80, IsReadOnly = true, IsVisible = true, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryReceipt"].Fields["DocumentNo"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "Quantity", Caption = SLanguage.GetString("Miktar"), Width = 90, IsCalculateTotalColumn = true, IsReadOnly = true, IsVisible = true, UsageType = FieldUsage.Quantity, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryAllocation"].Fields["Quantity"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ReceiptQuantity", Caption = SLanguage.GetString("Fiş Miktarı"), Width = 90, IsCalculateTotalColumn = true, IsReadOnly = true, IsVisible = true, UsageType = FieldUsage.Quantity, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryAllocation"].Fields["Quantity"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "WastageQuantity", Caption = SLanguage.GetString("Fire"), Width = 40, IsCalculateTotalColumn = true, IsReadOnly = true, IsVisible = true, UsageType = FieldUsage.Quantity, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryAllocation"].Fields["Quantity"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "AllocationQuantity", Caption = SLanguage.GetString("Tahsis"), Width = 90, IsCalculateTotalColumn = true, IsReadOnly = true, IsVisible = true, UsageType = FieldUsage.Quantity, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryAllocation"].Fields["Quantity"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "GrossQuantity", Caption = SLanguage.GetString("Brüt Miktar"), Width = 90, IsCalculateTotalColumn = true, IsReadOnly = true, IsVisible = true, UsageType = FieldUsage.Quantity, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryReceiptItem"].Fields["GrossQuantity"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "IAGrossQuantity", Caption = SLanguage.GetString("Tahsis Brüt"), Width = 90, IsCalculateTotalColumn = true, IsReadOnly = true, IsVisible = true, UsageType = FieldUsage.Quantity, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryAllocation"].Fields["GrossQuantity"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "RebateAllocationQuantity", Caption = SLanguage.GetString("İade Tahsis"), Width = 90, IsCalculateTotalColumn = true, IsReadOnly = true, IsVisible = true, UsageType = FieldUsage.Quantity, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryAllocation"].Fields["Quantity"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "AllocationReceiptDate", Caption = SLanguage.GetString("Tahsis Tarihi"), Width = 80, IsReadOnly = true, IsVisible = true, EditorType = EditorType.DateEditor, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryAllocation"].Fields["ReceiptDate"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "Warehouse", Caption = SLanguage.GetString("Depo"), Width = 80, IsReadOnly = true, IsVisible = true, EditorType = EditorType.DateEditor, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryReceiptItem"].Fields["Explanation"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "UnitCode", Caption = SLanguage.GetString("Birim"), Width = 70, IsReadOnly = true, IsVisible = true, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Meta_UnitSetItem"].Fields["UnitCode"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "TermDate", Caption = SLanguage.GetString("Termin"), Width = 80, IsReadOnly = true, IsVisible = true, EditorType = EditorType.DateEditor, UsageType = FieldUsage.Date, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryReceipt"].Fields["ReceiptDate"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ForexCode", Caption = SLanguage.GetString("Döviz"), Width = 40, IsReadOnly = true, IsVisible = true, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Meta_Forex"].Fields["ForexCode"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "UnitPrice", Caption = SLanguage.GetString("Birim Fiyat"), Width = 70, IsReadOnly = true, IsVisible = true, UsageType = FieldUsage.UnitPrice, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryReceiptItem"].Fields["UnitPrice"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "Amount", Caption = SLanguage.GetString("Tutar"), Width = 70, IsCalculateTotalColumn = true, IsReadOnly = true, IsVisible = true, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryReceiptItem"].Fields["ItemTotal"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ForexUnitPrice", Caption = SLanguage.GetString("Döviz Birim Fiyat"), Width = 70, IsCalculateTotalColumn = true, IsReadOnly = true, IsVisible = true, UsageType = FieldUsage.ForexUnitPrice, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryReceiptItem"].Fields["ForexUnitPrice"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "ForexAmount", Caption = SLanguage.GetString("Döviz Tutar"), Width = 70, IsCalculateTotalColumn = true, IsReadOnly = true, IsVisible = true, UsageType = FieldUsage.ForexAmount, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryReceiptItem"].Fields["ItemTotalForex"].UdtType) });
            orderAllHistoryColumnCollection.Add(new ReceiptColumn() { ColumnName = "Explanation", Caption = SLanguage.GetString("Açıklama"), Width = 140, IsReadOnly = true, IsVisible = true, DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryReceiptItem"].Fields["Explanation"].UdtType) });
            grdOrderAll.ColumnDefinitions = orderAllHistoryColumnCollection;
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
