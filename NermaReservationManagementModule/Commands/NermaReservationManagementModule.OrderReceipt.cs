
//using Microsoft.Office.Interop.Excel;
using DevExpress.CodeParser;
using DevExpress.Data.Helpers;

using FastExpressionCompiler.LightExpression;

using LiveCore.Desktop.Common;
using LiveCore.Desktop.UI.Controls;

using NermaReservationManagementModule.BoExtensions;

using Prism.Ioc;

using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.PresentationModels;
using Sentez.Common.Utilities;
using Sentez.Core.ParameterClasses;
using Sentez.Data.BusinessObjects;
using Sentez.Data.MetaData;
using Sentez.Data.Tools;
using Sentez.Localization;
using Sentez.OrderModule.PresentationModels;
using Sentez.QuotationModule.PresentationModels;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sentez.NermaReservationManagementModule
{
    public partial class NermaReservationManagementModule : LiveModule
    {
        private void OrderReceiptBoCustomCons(ref short itemId, ref string keyColumn, ref string typeField, ref string[] Tables)
        {
            List<string> tableList = new List<string>();
            tableList.AddRange(Tables);

            tableList.Add("Erp_OrderReceiptRecipeItem");
            Tables = tableList.ToArray();
        }

        private void OrderReceiptBo_Init_InventoryUnitItemSizeSetDetails(BusinessObjectBase bo, BoParam parameter)
        {
            bo.Lookups.AddLookUp("Erp_OrderReceiptItem", "InventoryUnitItemSizeSetDetailsId", true, "Erp_InventoryUnitItemSizeSetDetails", "SizeDetailCode", "InventoryUnitItemSizeSetDetails_SizeDetailCode"
            , new string[] {
                "SizeDetailName", 
                //"UnitItemId", 
                "UnitFactor",
                "UnitDivisor",
                "UnitWidth",
                "UnitLength",
                "UnitHeight"
            }
            , new string[] {
                "InventoryUnitItemSizeSetDetails_SizeDetailName", 
                //"InventoryUnitItemSizeSetDetails_UnitItemId", 
                "InventoryUnitItemSizeSetDetails_UnitFactor",
                "InventoryUnitItemSizeSetDetails_UnitDivisor",
                "InventoryUnitItemSizeSetDetails_UnitWidth",
                "InventoryUnitItemSizeSetDetails_UnitLength",
                "InventoryUnitItemSizeSetDetails_UnitHeight"
            });
            foreach (LookUpParameter lp in bo.Lookups.LookUps.ToArray())
            {
                if (lp.FKTable == "Erp_OrderReceiptItem" && lp.FKColumn == "InventoryId")
                {
                    bo.Lookups.LookUps.Remove(lp);
                    break;
                }
            }
            bo.Lookups.AddLookUp("Erp_OrderReceiptItem", "InventoryId", true, "Erp_Inventory", "InventoryCode", "InventoryCode", new string[] { "InventoryName", "HasVariant", "HasRowVariant", "Variant1TypeId", "Variant2TypeId", "Variant3TypeId", "Variant4TypeId", "Variant5TypeId", "Variant1TypeControlType", "Variant2TypeControlType", "Variant3TypeControlType", "Variant4TypeControlType", "Variant5TypeControlType", "MarkId", "ModelId", "InUse", "CategoryId", "GroupId", "IsSurfaceTreatment" }, new string[] { "InventoryName", "HasVariant", "HasRowVariant", "Variant1TypeId", "Variant2TypeId", "Variant3TypeId", "Variant4TypeId", "Variant5TypeId", "Variant1TypeControlType", "Variant2TypeControlType", "Variant3TypeControlType", "Variant4TypeControlType", "Variant5TypeControlType", "InventoryMarkId", "InventoryModelId", "InventoryInUse", "InventoryCategoryId", "InventoryGroupId", "InventoryIsSurfaceTreatment" });


            bo.Lookups.AddLookUp("Erp_OrderReceiptItem", "CategoryAttributeSetDetailsId", true, "Erp_CategoryAttributeSetDetails", "AttributeSetCode", "CategoryAttributeSetDetails_AttributeSetCode"
            , new string[] {
                "AttributeSetName",
                "SpecialCode01",
                "SpecialCode02",
                "SpecialCode03",
                "SpecialCode04",
                "SpecialCode05"
            }
            , new string[] {
                "CategoryAttributeSetDetails_AttributeSetName",
                "CategoryAttributeSetDetails_SpecialCode01",
                "CategoryAttributeSetDetails_SpecialCode02",
                "CategoryAttributeSetDetails_SpecialCode03",
                "CategoryAttributeSetDetails_SpecialCode04",
                "CategoryAttributeSetDetails_SpecialCode05"
            });

            bo.Lookups.AddLookUp("Erp_OrderReceiptItem", "AttributeSetItemId", true, "Erp_InventoryAttributeSetItem", "AttributeItemCode", "AttributeItemCode", new string[] { "AttributeItemName", "IsSelect" }, new string[] { "AttributeItemName", "AttributeItemIsSelect" });

            if (bo.BoBoParam != null)
                bo.ValueFiller.AddRule("Erp_OrderReceipt", "ReceiptSubType", bo.BoBoParam.DetailType);

            new OrderReceiptControlExtension(bo);
        }

        private bool OrderReceiptPm_OnListCommand(PMBase pm, PmParam parameter, ISysCommandParam commandParam)
        {
            if (commandParam?.cmdName == "ListCommand" && orderReceiptPm != null)
            {
                var focusScope = FocusManager.GetFocusScope(orderReceiptPm.ActiveViewControl);
                var element = FocusManager.GetFocusedElement(focusScope) as FrameworkElement;
                var visualelement = FrameworkTreeHelper.FindVisualParent<LiveGridControl>(element);
                string name = orderReceiptPm.GetFocusedField();
                if (visualelement is LiveGridControl && (visualelement?.Name == "GridDetail"))
                {
                    if (visualelement.CurrentColumn?.FieldName == "ItemVariant2Code")
                    {
                        (visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = "";
                        if (visualelement.CurrentItem is DataRowView)
                        {
                            bool inventoryIsSurfaceTreatment;
                            bool.TryParse((visualelement.CurrentItem as DataRowView).Row["InventoryIsSurfaceTreatment"].ToString(), out inventoryIsSurfaceTreatment);
                            if (!inventoryIsSurfaceTreatment)
                            {
                                orderReceiptPm.sysMng.ActWndMng.ShowMsg($"{(visualelement.CurrentItem as DataRowView).Row["ItemCode"]}-{(visualelement.CurrentItem as DataRowView).Row["ItemName"]} {SLanguage.GetString("malzemesi için yüzey işlem yapılacak özelliği aktif edilmemiş!")}", ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK, Common.InformationMessages.MessageBoxImage.Warning);
                                return true;
                            }
                            else
                            {
                                if (!(visualelement.CurrentItem as DataRowView).Row.IsNull("MarkId"))
                                {
                                    //(visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = $" [erp_variantitem].[RecId] in (select VariantItemId from Erp_VariantItemMark with (nolock) where MarkId in (select MarkId from Erp_VariantItemMark with (nolock) where MarkId={(visualelement.CurrentItem as DataRowView).Row["MarkId"]}))";
                                    //(visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = $" [erp_variantitem].[RecId] in (select VariantItemId from Erp_VariantItemMark with (nolock) where MarkId in (select MarkId from Erp_VariantItemMark where MarkId={(visualelement.CurrentItem as DataRowView).Row["MarkId"]} and CardId in (select RecId from Erp_VariantCard where TypeId={(visualelement.CurrentItem as DataRowView).Row["Variant2TypeId"]})) and RecId in (select VariantItemId from Erp_VariantItemMark where MarkId={(visualelement.CurrentItem as DataRowView).Row["MarkId"]}))";
                                    (visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = $" [erp_variantitem].[RecId] IN (SELECT RecId FROM Erp_VariantItem EVI WITH (NOLOCK) WHERE EVI.RecId IN(SELECT EVIM.VariantItemId FROM Erp_VariantItemMark EVIM WITH (NOLOCK) WHERE EVIM.MarkId = {(visualelement.CurrentItem as DataRowView).Row["MarkId"]}) AND EVI.CardId IN (SELECT EVC.RecId FROM Erp_VariantCard EVC WITH (NOLOCK) WHERE EVC.TypeId={(visualelement.CurrentItem as DataRowView).Row["Variant2TypeId"]}))";

                                }
                                return false;
                            }
                        }
                    }
                    else if (visualelement.CurrentColumn?.FieldName == "MarkName")
                    {
                        (visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = "";
                        if (visualelement.CurrentItem is DataRowView)
                        {
                            bool inventoryIsSurfaceTreatment;
                            bool.TryParse((visualelement.CurrentItem as DataRowView).Row["InventoryIsSurfaceTreatment"].ToString(), out inventoryIsSurfaceTreatment);
                            if (!inventoryIsSurfaceTreatment)
                            {
                                orderReceiptPm.sysMng.ActWndMng.ShowMsg($"{(visualelement.CurrentItem as DataRowView).Row["ItemCode"]}-{(visualelement.CurrentItem as DataRowView).Row["ItemName"]} {SLanguage.GetString("malzemesi için yüzey işlem yapılacak özelliği aktif edilmemiş!")}", ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK, Common.InformationMessages.MessageBoxImage.Warning);
                                return true;
                            }
                            else
                            {
                                if (!(visualelement.CurrentItem as DataRowView).Row.IsNull("ItemVariant2Id"))
                                {
                                    //(visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = $" and [erp_mark].[RecId] in (select RecId from Erp_InventoryMark with (nolock) where InventoryId={(visualelement.CurrentItem as DataRowView).Row["InventoryId"]})";
                                    (visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = $" and [erp_mark].[RecId] in (select MarkId from Erp_VariantItemMark with (nolock) where VariantItemId={(visualelement.CurrentItem as DataRowView).Row["ItemVariant2Id"]})";
                                }
                                return false;
                            }
                        }
                    }
                    else if (visualelement.CurrentColumn?.FieldName == "NER_SurfaceType")
                    {
                        if (visualelement.CurrentItem is DataRowView)
                        {
                            bool inventoryIsSurfaceTreatment;
                            bool.TryParse((visualelement.CurrentItem as DataRowView).Row["InventoryIsSurfaceTreatment"].ToString(), out inventoryIsSurfaceTreatment);
                            if (!inventoryIsSurfaceTreatment)
                            {
                                orderReceiptPm.sysMng.ActWndMng.ShowMsg($"{(visualelement.CurrentItem as DataRowView).Row["ItemCode"]}-{(visualelement.CurrentItem as DataRowView).Row["ItemName"]} {SLanguage.GetString("malzemesi için yüzey işlem yapılacak özelliği aktif edilmemiş!")}", ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK, Common.InformationMessages.MessageBoxImage.Warning);
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else if (visualelement.CurrentColumn?.FieldName == "ItemCode")
                    {
                        string whereStr = "";
                        if (visualelement.CurrentItem is DataRowView)
                        {
                            (visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = "";
                            DataRow itemRow = (visualelement.CurrentItem as DataRowView).Row;
                            if (itemRow != null)
                            {
                                short receiptSubType;
                                short.TryParse(itemRow["ReceiptSubType"].ToString(), out receiptSubType);
                                if (receiptSubType > 0)
                                {
                                    if (!string.IsNullOrEmpty(orderReceiptPm.ActiveSession.ParamService.GetParameterClass<QuotationParameters>().IssuedQuotationReceiptSubTypeValues))
                                    {
                                        DataSet dataSet = new DataSet();
                                        StringReader sr = new StringReader(orderReceiptPm.ActiveSession.ParamService.GetParameterClass<QuotationParameters>().IssuedQuotationReceiptSubTypeValues);
                                        dataSet.ReadXml(sr);
                                        if (dataSet?.Tables.Count > 0)
                                        {
                                            if (dataSet.Tables[0].Columns.Contains("InventoryCodes"))
                                            {
                                                foreach (DataRow row in dataSet.Tables[0].Rows)
                                                {
                                                    short paramReceiptSubType;
                                                    short.TryParse(row["TypeId"].ToString().ToString(), out paramReceiptSubType);
                                                    if (receiptSubType == paramReceiptSubType && !string.IsNullOrEmpty(row["InventoryCodes"].ToString()))
                                                    {
                                                        string[] invCodes = row["InventoryCodes"].ToString().Trim().Split(',');
                                                        foreach (string invCode in invCodes)
                                                        {
                                                            if (string.IsNullOrEmpty(whereStr))
                                                            {
                                                                whereStr = $"i.InventoryCode LIKE '{invCode}%'";
                                                            }
                                                            else
                                                            {
                                                                whereStr += $" OR i.InventoryCode LIKE '{invCode}%'";
                                                            }
                                                        }
                                                        break;
                                                    }
                                                }
                                                if (!string.IsNullOrEmpty(whereStr))
                                                {
                                                    if (orderReceiptPm.ActiveSession.ParamService.GetParameterClass<InventoryParameters>().IsInventoryListBasedOnCategory == 1)
                                                    {
                                                        whereStr = $" And ({whereStr})";
                                                        (visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = whereStr;
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else if (commandParam?.cmdName == "CardCommand" && orderReceiptPm != null)
            {
                var focusScope = FocusManager.GetFocusScope(orderReceiptPm.ActiveViewControl);
                var element = FocusManager.GetFocusedElement(focusScope) as FrameworkElement;
                var visualelement = FrameworkTreeHelper.FindVisualParent<LiveGridControl>(element);
                if (visualelement is LiveGridControl && (visualelement?.Name == "GridDetail"))
                {
                    if (visualelement.CurrentColumn?.FieldName == "InventoryUnitItemSizeSetDetails_SizeDetailCode")
                    {
                        OpenInventoryCategoryCard(visualelement, orderReceiptPm);
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            return false;
        }

        private void OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails(PMBase pm, PmParam parameter)
        {
            orderReceiptPm = pm as OrderReceiptPM;
            if (orderReceiptPm == null)
            {
                return;
            }
            if (orderReceiptPm.ActiveBO != null)
            {
                orderReceiptPm.ActiveBO.ColumnChanged -= ActiveBO_ColumnChanged_QuotationReceiptPm;
            }
            orderReceiptPm.PreviewKeyDown -= OrderReceiptPm_PreviewKeyDown;
        }

        private void OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails(object sender, RoutedEventArgs e)
        {
            if (orderReceiptPm?.ReceiptColumnCollection != null)
            {
                if (!orderReceiptPm.ReceiptColumnCollection.Contains("InventoryUnitItemSizeSetDetails_SizeDetailCode"))
                    orderReceiptPm.ReceiptColumnCollection.Add(new ReceiptColumn() { ColumnName = "InventoryUnitItemSizeSetDetails_SizeDetailCode", Caption = "Ölçü Kodu", EditorType = EditorType.ListSelector, Width = 100, LookUpTable = "Erp_InventoryUnitItemSizeSetDetails", LookUpField = "SizeDetailCode", IsVisible = false });
                if (!orderReceiptPm.ReceiptColumnCollection.Contains("InventoryUnitItemSizeSetDetails_SizeDetailName"))
                    orderReceiptPm.ReceiptColumnCollection.Add(new ReceiptColumn() { ColumnName = "InventoryUnitItemSizeSetDetails_SizeDetailName", Caption = "Ölçü Adı", EditorType = EditorType.ReadOnlyTextEditor, Width = 120, LookUpTable = "Erp_InventoryUnitItemSizeSetDetails", LookUpField = "SizeDetailName", IsVisible = false });

                if (!orderReceiptPm.ReceiptColumnCollection.Contains("CategoryAttributeSetDetails_AttributeSetCode"))
                    orderReceiptPm.ReceiptColumnCollection.Add(new ReceiptColumn() { ColumnName = "CategoryAttributeSetDetails_AttributeSetCode", Caption = "Özellik Kodu", EditorType = EditorType.ListSelector, Width = 100, LookUpTable = "Erp_CategoryAttributeSetDetails", LookUpField = "AttributeSetCode", IsVisible = false });
                if (!orderReceiptPm.ReceiptColumnCollection.Contains("CategoryAttributeSetDetails_AttributeSetName"))
                    orderReceiptPm.ReceiptColumnCollection.Add(new ReceiptColumn() { ColumnName = "CategoryAttributeSetDetails_AttributeSetName", Caption = "Özellik Adı", EditorType = EditorType.ReadOnlyTextEditor, Width = 120, LookUpTable = "Erp_CategoryAttributeSetDetails", LookUpField = "AttributeSetName", IsVisible = false });

                if (!orderReceiptPm.ReceiptColumnCollection.Contains("AttributeItemCode"))
                    orderReceiptPm.ReceiptColumnCollection.Add(new ReceiptColumn() { ColumnName = "AttributeItemCode", Caption = "Özellik Seti Detay Kodu", EditorType = EditorType.ListSelector, Width = 100, LookUpTable = "Erp_InventoryAttributeSetItem", LookUpField = "AttributeItemCode", IsVisible = false });
                if (!orderReceiptPm.ReceiptColumnCollection.Contains("AttributeItemName"))
                    orderReceiptPm.ReceiptColumnCollection.Add(new ReceiptColumn() { ColumnName = "AttributeItemName", Caption = "Özellik Seti Detay Adı", EditorType = EditorType.ReadOnlyTextEditor, Width = 120, LookUpTable = "Erp_InventoryAttributeSetItem", LookUpField = "AttributeItemName", IsVisible = false });
            }
        }

        private void OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails(PMBase pm, PmParam parameter)
        {
            orderReceiptPm = pm as OrderReceiptPM;
            if (orderReceiptPm == null)
            {
                return;
            }
            Lists_OrderReceiptPM = orderReceiptPm.ActiveSession.LookupList.GetChild(UtilityFunctions.GetConnection(orderReceiptPm.ActiveSession.dbInfo.DBProvider, orderReceiptPm.ActiveSession.dbInfo.ConnectionString));
            if (orderReceiptPm.ActiveBO != null)
            {
                orderReceiptPm.ActiveBO.ColumnChanged += ActiveBO_ColumnChanged_OrderReceiptPm;
            }
            LiveDocumentGroup liveDetailPanel = orderReceiptPm.FCtrl("DetailPanel") as LiveDocumentGroup;
            if (liveDetailPanel != null)
            {
                LiveDocumentPanel ldpOrderRecipeItemView = new LiveDocumentPanel();
                ldpOrderRecipeItemView.Caption = SLanguage.GetString("Sipariş POZ Detayı");
                liveDetailPanel.Items.Add(ldpOrderRecipeItemView);

                PMDesktop pMDesktop = orderReceiptPm.container.Resolve<PMDesktop>();
                var tsePublicParametersView = pMDesktop.LoadXamlRes("OrderRecipeItemView");
                (tsePublicParametersView._view as UserControl).DataContext = orderReceiptPm;
                ldpOrderRecipeItemView.Content = tsePublicParametersView._view;
            }

            orderReceiptPm.CmdList.AddCmd(317, "OrderRecipeLoadExcelCommand", SLanguage.GetString("Excelden Yükle"), OnOrderRecipeLoadExcelCommand, null);
            orderReceiptPm.PreviewKeyDown += OrderReceiptPm_PreviewKeyDown;
        }

        private void OrderReceiptPm_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F9 && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                var focusScope = FocusManager.GetFocusScope(orderReceiptPm.ActiveViewControl);
                var element = FocusManager.GetFocusedElement(focusScope) as FrameworkElement;
                var visualelement = FrameworkTreeHelper.FindVisualParent<LiveGridControl>(element);
                string name = orderReceiptPm.GetFocusedField();
                if (visualelement is LiveGridControl && (visualelement?.Name == "GridDetail"))
                {
                    if (visualelement.CurrentItem != null && (visualelement.CurrentColumn.Tag is ReceiptColumn))
                    {
                        if ((visualelement.CurrentItem as DataRowView).Row.Table.Columns.Contains("InventoryCategoryId") && !(visualelement.CurrentItem as DataRowView).Row.IsNull("InventoryCategoryId"))
                        {
                            if ((visualelement.CurrentColumn.Tag as ReceiptColumn).ColumnName == "InventoryUnitItemSizeSetDetails_SizeDetailCode")
                            {
                                OpenInventoryCategoryCard(visualelement, orderReceiptPm);
                            }
                        }
                    }
                }
                e.Handled = true;
            }
        }

        private void OnOrderRecipeLoadExcelCommand(ISysCommandParam obj)
        {
        }

        private void ActiveBO_ColumnChanged_OrderReceiptPm(object sender, DataColumnChangeEventArgs e)
        {
            if (orderReceiptPm?.ActiveBO == null)
                return;
            if (_suppressEvent)
                return;
            if (e.Column.ColumnName == "QuotationReceiptItemId")
            {
                try
                {
                    _suppressEvent = true;
                    using (DataTable quotationItemTable = UtilityFunctions.GetDataTableList(orderReceiptPm.ActiveBO.Provider, orderReceiptPm.ActiveBO.Connection, orderReceiptPm.ActiveBO.Transaction, "Erp_QuotationReceiptItem", $"select * from Erp_QuotationReceiptItem with (nolock) where RecId={e.ProposedValue}"))
                    {
                        if (quotationItemTable?.Rows.Count > 0)
                        {
                            e.Row["InventoryUnitItemSizeSetDetailsId"] = quotationItemTable.Rows[0]["InventoryUnitItemSizeSetDetailsId"];
                            e.Row["CategoryAttributeSetDetailsId"] = quotationItemTable.Rows[0]["CategoryAttributeSetDetailsId"];
                            e.Row["AttributeSetItemId"] = quotationItemTable.Rows[0]["AttributeSetItemId"];
                        }
                    }
                    _suppressEvent = false;
                }
                catch
                {
                    _suppressEvent = false;
                }
            }
            else if (e.Column.ColumnName == "InventoryId")
            {

            }
        }
    }
}
