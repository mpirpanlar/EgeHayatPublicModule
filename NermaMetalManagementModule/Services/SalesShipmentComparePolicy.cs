using Microsoft.Practices.Unity;
using Reeb.SqlOM;
using Sentez.Common.Report;
using Sentez.Common.SqlBuilder;
using Sentez.Data.MetaData;
using Sentez.Localization;
using System;
using System.Text;

namespace Sentez.NermaMetalManagementModule.Services
{
    class SalesShipmentComparePolicy : ReportBase
    {
        public SalesShipmentComparePolicy(IUnityContainer container)
           : base(container)
        {
            Name = "SalesShipmentComparePolicy";
            Title = "Satış-Sevkiyat Karşılaştırması";
            WorkMode = ReportWorkMode.WorkList;
        }

        override public void Init()
        {
            InitBegin();
            InitStatements(null);
            InitEnd();
        }
        override public void InitStatements(object prm)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"Select Base.[Order Numarası],Base.[Kalite Kodu],Base.[Order Kayıt No] [Order Kayıt No],Avg(Base.[Çeki Miktarı])  [Çeki Miktarı]
                            ,Avg(Base.[Çeki Miktarı])*avg(Base.[Çeki Birim Tutarı]) [Çeki Tutarı],Base.[Çeki Döviz Cinsi]
                            ,SUM(Base.[Fatura Miktarı]) [Fatura Miktarı] 
                            ,Base.[Fatura Döviz Cinsi],SUM(Base.[Fatura Döviz Toplamı]) [Fatura Döviz Toplamı]
                            ,SUM(Base.[Fatura Tutarı]) [Fatura Tutarı],SUM(Base.[Fatura İndirim Tutarı]) [Fatura İndirim Tutarı] from (select 
                             isnull(wo.WorkOrderNo, N'') [Order Numarası]
                            , isnull(wo.RecId, 0) [Order Kayıt No]
                            , isnull((Select QualityName from Erp_QualityType qt with (nolock) where qt.RecId in ([erp_inventoryreceiptitem].QualityTypeId)),'') [Kalite Kodu] ");
            sb.AppendLine($",isnull((Select sum(isnull(pli.Quantity,0)) from Erp_PackingListItem pli with(nolock) where pli.PackingListId in (Select RecId from Erp_PackingList pl with(nolock) where pl.WorkOrderItemId in (Select woi1.RecId from Erp_WorkOrderItem woi1 where woi1.WorkOrderId=wo.RecId) and Month(pl.CheckingDate) = {((DateTime)Parameteres["CompareDate"]).Date.Month} and Year(pl.CheckingDate) = {((DateTime)Parameteres["CompareDate"]).Date.Year} and pl.UD_Quality =(Select QualityName from Erp_QualityType qt with (nolock) where qt.RecId in ([erp_inventoryreceiptitem].QualityTypeId))) and pli.ItemType=1 and pli.ForexId=[erp_invoice].ForexId),0) [Çeki Miktarı]");
            sb.AppendLine($",isnull((Select top 1 pli.ForexUnitPrice from Erp_PackingListItem pli with(nolock) where pli.PackingListId in (Select RecId from Erp_PackingList pl with(nolock) where pl.WorkOrderItemId in (Select woi1.RecId from Erp_WorkOrderItem woi1 where woi1.WorkOrderId=wo.RecId) and Month(pl.CheckingDate) = {((DateTime)Parameteres["CompareDate"]).Date.Month} and Year(pl.CheckingDate) = {((DateTime)Parameteres["CompareDate"]).Date.Year} and pl.UD_Quality =(Select QualityName from Erp_QualityType qt with (nolock) where qt.RecId in ([erp_inventoryreceiptitem].QualityTypeId))) and pli.ItemType=1 and pli.ForexId=[erp_invoice].ForexId),0) [Çeki Birim Tutarı]");
            sb.AppendLine($",isnull((Select ForexCode from Meta_Forex where RecId in (Select top 1 pli.ForexId from Erp_PackingListItem pli with(nolock) where pli.PackingListId in (Select RecId from Erp_PackingList pl with(nolock) where pl.WorkOrderItemId in (Select woi1.RecId from Erp_WorkOrderItem woi1 where woi1.WorkOrderId=wo.RecId)" +
                $" and Month(pl.CheckingDate) = {((DateTime)Parameteres["CompareDate"]).Date.Month} and Year(pl.CheckingDate) = {((DateTime)Parameteres["CompareDate"]).Date.Year} and pl.UD_Quality =(Select QualityName from Erp_QualityType qt with (nolock) where qt.RecId in ([erp_inventoryreceiptitem].QualityTypeId)) ) and pli.ItemType=1 and pli.ForexId=[erp_invoice].ForexId)), '')  [Çeki Döviz Cinsi]");
            sb.AppendLine(@", isnull(sum(isnull(erp_inventoryreceiptitem.Quantity,0)), 0) [Fatura Miktarı]
                            , isnull((Select ForexCode from Meta_Forex where RecId = erp_inventoryreceiptitem.ForexId), N'') [Fatura Döviz Cinsi]
                            , isnull(sum(isnull(erp_inventoryreceiptitem.ItemTotalForex,0)), 0) [Fatura Döviz Toplamı]
                            , isnull(sum(isnull(erp_inventoryreceiptitem.ItemTotal,0)), 0) [Fatura Tutarı]
                            , isnull(sum(isnull(erp_invoice.DiscountsTotal,0)), 0) [Fatura İndirim Tutarı] 
                            from [Erp_InventoryReceipt] [erp_inventoryreceipt] with (nolock)  
                            left join [Erp_Invoice] [erp_invoice] with (nolock) on ([erp_inventoryreceipt].[InvoiceId] = [erp_invoice].[RecId]) 
                            left join [Erp_InventoryReceiptItem] [erp_inventoryreceiptitem] with (nolock) on ([erp_inventoryreceipt].[RecId] = [erp_inventoryreceiptitem].[InventoryReceiptId]) 
                            left join Erp_WorkOrderItem woi with (nolock) on woi.RecId = erp_inventoryreceiptitem.WorkOrderReceiptItemId 
                            left join Erp_WorkOrder wo with (nolock) on wo.RecId = woi.WorkOrderId
                            where erp_invoice.ReceiptType in (120,121) and isnull(erp_inventoryreceiptitem.ItemType,1) = 1 and isnull(wo.IsChecked,1) = 1");
            sb.AppendLine($"and erp_invoice.CompanyId = {activeSession._CompanyInfo.RecId} and wo.WorkOrderNo is not null and isnull(wo.UD_PMaliyet,0) = 0");
            if (Parameteres.ContainsKey("CompareDate") && Parameteres["CompareDate"] is DateTime)
                sb.AppendLine($"and MONTH(erp_invoice.DischargeDate) = {((DateTime)Parameteres["CompareDate"]).Date.Month} and YEAR(erp_invoice.DischargeDate) = {((DateTime)Parameteres["CompareDate"]).Date.Year} ");
            sb.AppendLine(@"group by [erp_inventoryreceiptitem].QualityTypeId, [erp_inventoryreceiptitem].[WorkOrderReceiptItemId],[erp_invoice].ForexId
                            , [erp_inventoryreceiptitem].[ForexId],wo.WorkOrderNo,wo.RecId) Base group by Base.[Order Numarası],Base.[Order Kayıt No],Base.[Fatura Döviz Cinsi],Base.[Kalite Kodu],Base.[Çeki Döviz Cinsi]");

            Statement _statement = new Statement("Erp_Invoice");
            _statement.AddTable("Erp_Invoice", "erp_invoice");
            _statement.AddTable("Erp_InventoryReceipt", "erp_inventoryreceipt");
            _statement.AddTable("Erp_InventoryReceiptItem", "erp_inventoryreceiptitem");

            _statement.SetBaseTable("erp_invoice");
            _statement.AddColCalc(SLanguage.GetString("Order Numarası"), SLanguage.GetString("Order Numarası"), SqlDataType.String, FieldUsage.Name, "");
            _statement.AddColCalc(SLanguage.GetString("Kalite Kodu"), SLanguage.GetString("Kalite Kodu"), SqlDataType.String, FieldUsage.Name, "");
            _statement.AddColCalc(SLanguage.GetString("Order Kayıt No"), SLanguage.GetString("Order Kayıt No"), SqlDataType.String, FieldUsage.Name, "", false);
            _statement.AddColCalc(SLanguage.GetString("Çeki Miktarı"), SLanguage.GetString("Çeki Miktarı"), SqlDataType.Number, FieldUsage.Amount, 0);
            _statement.AddColCalc(SLanguage.GetString("Çeki Tutarı"), SLanguage.GetString("Çeki Tutarı"), SqlDataType.Number, FieldUsage.Amount, 0);
            _statement.AddColCalc(SLanguage.GetString("Çeki Döviz Cinsi"), SLanguage.GetString("Çeki Döviz Cinsi"), SqlDataType.String, FieldUsage.Name, "");
            _statement.AddColCalc(SLanguage.GetString("Fatura Miktarı"), SLanguage.GetString("Fatura Miktarı"), SqlDataType.Number, FieldUsage.Amount, 0);
            _statement.AddColCalc(SLanguage.GetString("Fatura Döviz Cinsi"), SLanguage.GetString("Fatura Döviz Cinsi"), SqlDataType.String, FieldUsage.Name, "");
            _statement.AddColCalc(SLanguage.GetString("Fatura Döviz Toplamı"), SLanguage.GetString("Fatura Döviz Toplamı"), SqlDataType.Number, FieldUsage.Amount, 0);
            _statement.AddColCalc(SLanguage.GetString("Fatura Tutarı"), SLanguage.GetString("Fatura Tutarı"), SqlDataType.Number, FieldUsage.Amount, 0);
            _statement.AddColCalc(SLanguage.GetString("Fatura İndirim Tutarı"), SLanguage.GetString("Fatura İndirim Tutarı"), SqlDataType.Number, FieldUsage.Amount, 0);

            _statement.LoadAllFields(false);
            _statement.AddSql(sb.ToString());
            AddStatement(_statement);
            ViewStatement = _statement;
        }
    }
}
