
//using Microsoft.Office.Interop.Excel;
using DevExpress.CodeParser;
using DevExpress.Data.Helpers;
using DevExpress.XtraRichEdit.Model;

using FastExpressionCompiler.LightExpression;

using LiveCore.Desktop.Common;
using LiveCore.Desktop.UI.Controls;

using NermaReservationManagementModule.BoExtensions;

using Prism.Ioc;

using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.PresentationModels;
using Sentez.Common.Report;
using Sentez.Common.Utilities;
using Sentez.Core.ParameterClasses;
using Sentez.Data.BusinessObjects;
using Sentez.Data.MetaData;
using Sentez.Data.Tools;
using Sentez.InventoryModule;
using Sentez.Localization;
using Sentez.OrderModule.PresentationModels;
using Sentez.QuotationModule.PresentationModels;

using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using static DevExpress.Mvvm.Native.Either;

namespace Sentez.NermaReservationManagementModule
{
    public partial class NermaReservationManagementModule : LiveModule
    {
        private void QuotationReceiptBo_Init_InventoryUnitItemSizeSetDetails(BusinessObjectBase bo, BoParam parameter)
        {
            bo.Lookups.AddLookUp("Erp_QuotationReceiptItem", "InventoryUnitItemSizeSetDetailsId", true, "Erp_InventoryUnitItemSizeSetDetails", "SizeDetailCode", "InventoryUnitItemSizeSetDetails_SizeDetailCode"
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
                if (lp.FKTable == "Erp_QuotationReceiptItem" && lp.FKColumn == "InventoryId")
                {
                    bo.Lookups.LookUps.Remove(lp);
                    break;
                }
            }
            bo.Lookups.AddLookUp("Erp_QuotationReceiptItem", "InventoryId", true, "Erp_Inventory", "InventoryCode", "InventoryCode", new string[] { "InventoryName", "HasVariant", "HasRowVariant", "Variant1TypeId", "Variant2TypeId", "Variant3TypeId", "Variant4TypeId", "Variant5TypeId", "Variant1TypeControlType", "Variant2TypeControlType", "Variant3TypeControlType", "Variant4TypeControlType", "Variant5TypeControlType", "MarkId", "ModelId", "InUse", "CategoryId", "GroupId", "IsSurfaceTreatment" }, new string[] { "InventoryName", "HasVariant", "HasRowVariant", "Variant1TypeId", "Variant2TypeId", "Variant3TypeId", "Variant4TypeId", "Variant5TypeId", "Variant1TypeControlType", "Variant2TypeControlType", "Variant3TypeControlType", "Variant4TypeControlType", "Variant5TypeControlType", "InventoryMarkId", "InventoryModelId", "InventoryInUse", "InventoryCategoryId", "InventoryGroupId", "InventoryIsSurfaceTreatment" });


            bo.Lookups.AddLookUp("Erp_QuotationReceiptItem", "CategoryAttributeSetDetailsId", true, "Erp_CategoryAttributeSetDetails", "AttributeSetCode", "CategoryAttributeSetDetails_AttributeSetCode"
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

            bo.Lookups.AddLookUp("Erp_QuotationReceiptItem", "AttributeSetItemId", true, "Erp_InventoryAttributeSetItem", "AttributeItemCode", "AttributeItemCode", new string[] { "AttributeItemName", "IsSelect" }, new string[] { "AttributeItemName", "AttributeItemIsSelect" });

            if (bo.BoBoParam != null)
                bo.ValueFiller.AddRule("Erp_QuotationReceipt", "ReceiptSubType", bo.BoBoParam.DetailType);

            new DemandReceiptControlExtension(bo);
        }

        private void QuotationReceiptPm_Init_InventoryUnitItemSizeSetDetails(PMBase pm, PmParam parameter)
        {
            quotationReceiptPm = pm as QuotationReceiptPM;
            if (quotationReceiptPm == null)
            {
                return;
            }
            Lists_QuotationReceiptPM = quotationReceiptPm.ActiveSession.LookupList.GetChild(UtilityFunctions.GetConnection(quotationReceiptPm.ActiveSession.dbInfo.DBProvider, quotationReceiptPm.ActiveSession.dbInfo.ConnectionString));
            if (quotationReceiptPm.ActiveBO != null)
            {
                quotationReceiptPm.ActiveBO.ColumnChanged += ActiveBO_ColumnChanged_QuotationReceiptPm;
            }
            LiveDocumentGroup liveDetailPanel = quotationReceiptPm.FCtrl("DetailPanel") as LiveDocumentGroup;
            if (liveDetailPanel != null)
            {
                LiveDocumentPanel ldpQuotationRecipeItemView = new LiveDocumentPanel();
                ldpQuotationRecipeItemView.Caption = SLanguage.GetString("Teklif POZ Detayı");
                liveDetailPanel.Items.Add(ldpQuotationRecipeItemView);

                PMDesktop pMDesktop = quotationReceiptPm.container.Resolve<PMDesktop>();
                var tsePublicParametersView = pMDesktop.LoadXamlRes("QuotationRecipeItemView");
                (tsePublicParametersView._view as UserControl).DataContext = quotationReceiptPm;
                ldpQuotationRecipeItemView.Content = tsePublicParametersView._view;
            }

            quotationReceiptPm.CmdList.AddCmd(317, "QuotationRecipeLoadExcelCommand", SLanguage.GetString("Excelden Yükle"), OnQuotationRecipeLoadExcelCommand, null);
            quotationReceiptPm.PreviewKeyDown += QuotationReceiptPm_PreviewKeyDown;
        }

        private void QuotationReceiptPm_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F9 && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                var focusScope = FocusManager.GetFocusScope(quotationReceiptPm.ActiveViewControl);
                var element = FocusManager.GetFocusedElement(focusScope) as FrameworkElement;
                var visualelement = FrameworkTreeHelper.FindVisualParent<LiveGridControl>(element);
                string name = quotationReceiptPm.GetFocusedField();
                if (visualelement is LiveGridControl && (visualelement?.Name == "GridDetail"))
                {
                    if (visualelement.CurrentItem != null && (visualelement.CurrentColumn.Tag is ReceiptColumn))
                    {
                        if ((visualelement.CurrentItem as DataRowView).Row.Table.Columns.Contains("InventoryCategoryId") && !(visualelement.CurrentItem as DataRowView).Row.IsNull("InventoryCategoryId"))
                        {
                            if ((visualelement.CurrentColumn.Tag as ReceiptColumn).ColumnName == "InventoryUnitItemSizeSetDetails_SizeDetailCode")
                            {
                                OpenInventoryCategoryCard(visualelement, quotationReceiptPm);
                            }
                        }
                    }
                }
                e.Handled = true;
            }
        }

        private bool QuotationReceiptPm_OnListCommand(PMBase pm, PmParam parameter, ISysCommandParam commandParam)
        {
            if (commandParam?.cmdName == "ListCommand" && quotationReceiptPm != null)
            {
                var focusScope = FocusManager.GetFocusScope(quotationReceiptPm.ActiveViewControl);
                var element = FocusManager.GetFocusedElement(focusScope) as FrameworkElement;
                var visualelement = FrameworkTreeHelper.FindVisualParent<LiveGridControl>(element);
                string name = quotationReceiptPm.GetFocusedField();
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
                                quotationReceiptPm.sysMng.ActWndMng.ShowMsg($"{(visualelement.CurrentItem as DataRowView).Row["ItemCode"]}-{(visualelement.CurrentItem as DataRowView).Row["ItemName"]} {SLanguage.GetString("malzemesi için yüzey işlem yapılacak özelliği aktif edilmemiş!")}", ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK, Common.InformationMessages.MessageBoxImage.Warning);
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
                                quotationReceiptPm.sysMng.ActWndMng.ShowMsg($"{(visualelement.CurrentItem as DataRowView).Row["ItemCode"]}-{(visualelement.CurrentItem as DataRowView).Row["ItemName"]} {SLanguage.GetString("malzemesi için yüzey işlem yapılacak özelliği aktif edilmemiş!")}", ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK, Common.InformationMessages.MessageBoxImage.Warning);
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
                                quotationReceiptPm.sysMng.ActWndMng.ShowMsg($"{(visualelement.CurrentItem as DataRowView).Row["ItemCode"]}-{(visualelement.CurrentItem as DataRowView).Row["ItemName"]} {SLanguage.GetString("malzemesi için yüzey işlem yapılacak özelliği aktif edilmemiş!")}", ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK, Common.InformationMessages.MessageBoxImage.Warning);
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
                                    if (!string.IsNullOrEmpty(quotationReceiptPm.ActiveSession.ParamService.GetParameterClass<QuotationParameters>().IssuedQuotationReceiptSubTypeValues))
                                    {
                                        DataSet dataSet = new DataSet();
                                        StringReader sr = new StringReader(quotationReceiptPm.ActiveSession.ParamService.GetParameterClass<QuotationParameters>().IssuedQuotationReceiptSubTypeValues);
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
                                                    if (quotationReceiptPm.ActiveSession.ParamService.GetParameterClass<InventoryParameters>().IsInventoryListBasedOnCategory == 1)
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
            else if (commandParam?.cmdName == "CardCommand" && quotationReceiptPm != null)
            {
                var focusScope = FocusManager.GetFocusScope(quotationReceiptPm.ActiveViewControl);
                var element = FocusManager.GetFocusedElement(focusScope) as FrameworkElement;
                var visualelement = FrameworkTreeHelper.FindVisualParent<LiveGridControl>(element);
                if (visualelement is LiveGridControl && (visualelement?.Name == "GridDetail"))
                {
                    if (visualelement.CurrentColumn?.FieldName == "InventoryUnitItemSizeSetDetails_SizeDetailCode")
                    {
                        OpenInventoryCategoryCard(visualelement, quotationReceiptPm);
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

        private void OpenInventoryCategoryCard(LiveGridControl visualelement, PMDesktop pm)
        {
            int categoryId;
            int.TryParse((visualelement.CurrentItem as DataRowView).Row["InventoryCategoryId"].ToString(), out categoryId);
            if (categoryId != 0)
            {
                using (DataTable table = UtilityFunctions.GetDataTableList(pm.ActiveBO.Provider, pm.ActiveBO.Connection, pm.ActiveBO.Transaction, "Erp_Category", $"select * from Erp_Category with (nolock) where RecId={categoryId}"))
                {
                    if (table?.Rows.Count > 0)
                    {
                        int parentCategoryId;
                        int.TryParse(table.Rows[0]["ParentId"].ToString(), out parentCategoryId);
                        while (parentCategoryId != 0)
                        {
                            categoryId = parentCategoryId;
                            using (DataTable table1 = UtilityFunctions.GetDataTableList(pm.ActiveBO.Provider, pm.ActiveBO.Connection, pm.ActiveBO.Transaction, "Erp_Category", $"select * from Erp_Category with (nolock) where RecId={parentCategoryId}"))
                            {
                                if (table1?.Rows.Count > 0)
                                {
                                    int.TryParse(table1.Rows[0]["ParentId"].ToString(), out parentCategoryId);
                                }
                            }
                        }
                    }
                }

                BoParam boparam2 = new BoParam()
                {
                    LogicalModuleId = (short)Modules.InventoryModule,
                    ActiveRecordId = categoryId
                };
                PmParam pmparam = new PmParam("CardPM", "BOCardContext")
                {
                    Name = "Category",
                    Tag2 = "FromReceipt",
                    isModal = true
                };

                SysCommandParam prm = new SysCommandParam("Category", "CardPM", pmparam, "CategoryBO", boparam2, "", "");
                prm.logicalModuleID = (short)Modules.InventoryModule;
                prm.moduleID = (short)Modules.InventoryModule;
                prm.secID = (short)InventorySecurityItems.Category;
                prm.subsecID = (short)MarkSubItems.None;
                prm.isModal = pmparam.isModal;
                SysMng.Instance.GetCmd("CmdGeneralOpen").Execute(prm);
            }
        }

        private void OnQuotationRecipeLoadExcelCommand(ISysCommandParam obj)
        {
        }

        private void QuotationReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails(object sender, RoutedEventArgs e)
        {
            if (quotationReceiptPm?.ReceiptColumnCollection != null)
            {
                if (!quotationReceiptPm.ReceiptColumnCollection.Contains("InventoryUnitItemSizeSetDetails_SizeDetailCode"))
                    quotationReceiptPm.ReceiptColumnCollection.Add(new ReceiptColumn() { ColumnName = "InventoryUnitItemSizeSetDetails_SizeDetailCode", Caption = "Ölçü Kodu", EditorType = EditorType.ListSelector, Width = 100, LookUpTable = "Erp_InventoryUnitItemSizeSetDetails", LookUpField = "SizeDetailCode", IsVisible = false });
                if (!quotationReceiptPm.ReceiptColumnCollection.Contains("InventoryUnitItemSizeSetDetails_SizeDetailName"))
                    quotationReceiptPm.ReceiptColumnCollection.Add(new ReceiptColumn() { ColumnName = "InventoryUnitItemSizeSetDetails_SizeDetailName", Caption = "Ölçü Adı", EditorType = EditorType.ReadOnlyTextEditor, Width = 120, LookUpTable = "Erp_InventoryUnitItemSizeSetDetails", LookUpField = "SizeDetailName", IsVisible = false });

                if (!quotationReceiptPm.ReceiptColumnCollection.Contains("CategoryAttributeSetDetails_AttributeSetCode"))
                    quotationReceiptPm.ReceiptColumnCollection.Add(new ReceiptColumn() { ColumnName = "CategoryAttributeSetDetails_AttributeSetCode", Caption = "Özellik Kodu", EditorType = EditorType.ListSelector, Width = 100, LookUpTable = "Erp_CategoryAttributeSetDetails", LookUpField = "AttributeSetCode", IsVisible = false });
                if (!quotationReceiptPm.ReceiptColumnCollection.Contains("CategoryAttributeSetDetails_AttributeSetName"))
                    quotationReceiptPm.ReceiptColumnCollection.Add(new ReceiptColumn() { ColumnName = "CategoryAttributeSetDetails_AttributeSetName", Caption = "Özellik Adı", EditorType = EditorType.ReadOnlyTextEditor, Width = 120, LookUpTable = "Erp_CategoryAttributeSetDetails", LookUpField = "AttributeSetName", IsVisible = false });

                if (!quotationReceiptPm.ReceiptColumnCollection.Contains("AttributeItemCode"))
                    quotationReceiptPm.ReceiptColumnCollection.Add(new ReceiptColumn() { ColumnName = "AttributeItemCode", Caption = "Özellik Seti Detay Kodu", EditorType = EditorType.ListSelector, Width = 100, LookUpTable = "Erp_InventoryAttributeSetItem", LookUpField = "AttributeItemCode", IsVisible = false });
                if (!quotationReceiptPm.ReceiptColumnCollection.Contains("AttributeItemName"))
                    quotationReceiptPm.ReceiptColumnCollection.Add(new ReceiptColumn() { ColumnName = "AttributeItemName", Caption = "Özellik Seti Detay Adı", EditorType = EditorType.ReadOnlyTextEditor, Width = 120, LookUpTable = "Erp_InventoryAttributeSetItem", LookUpField = "AttributeItemName", IsVisible = false });
            }
            //if (quotationReceiptPm != null)
            //{
            //    foreach(var item in quotationReceiptPm.contextMenu.Items)
            //    {
            //        if (item is Separator)
            //            continue;
            //        if((item as MenuItem).Name == "")
            //        {

            //        }
            //    }
            //}
        }

        private void ActiveBO_ColumnChanged_QuotationReceiptPm(object sender, DataColumnChangeEventArgs e)
        {
            if (_suppressEvent) return;
            if (e.Row.Table.TableName == "Erp_QuotationReceiptItem"
                && (
                    e.Column.ColumnName == "ItemVariant1Code" || e.Column.ColumnName == "ItemVariant1Id" ||
                    e.Column.ColumnName == "ItemVariant2Code" || e.Column.ColumnName == "ItemVariant2Id" ||
                    e.Column.ColumnName == "ItemVariant3Code" || e.Column.ColumnName == "ItemVariant3Id" ||
                    e.Column.ColumnName == "MarkId" ||
                    e.Column.ColumnName == "InventoryCategoryId"
                    )
                )
            {
                IBusinessObject inventoryBO = quotationReceiptPm.ActiveBO.Container.Resolve<IBusinessObject>("InventoryBO");
                try
                {
                    _suppressEvent = true;
                    long inventoryId;
                    long.TryParse(e.Row["InventoryId"].ToString(), out inventoryId);
                    if (inventoryId > 0L)
                    {
                        if (inventoryBO.Get("CATEGORY_INV") > 0)
                        {
                            DataRow[] priceListRows = null;
                            string whereStr = "IsPriceDiscount=1 and ItemType=1 and PriceType=2 and IsNull(InUse,0)=1";
                            if (!e.Row.IsNull("InventoryCategoryId"))
                            {
                                int categoryId;
                                int.TryParse(e.Row["InventoryCategoryId"].ToString(), out categoryId);
                                if (categoryId != 0)
                                {
                                    using (DataTable table = UtilityFunctions.GetDataTableList(quotationReceiptPm.ActiveBO.Provider, quotationReceiptPm.ActiveBO.Connection, quotationReceiptPm.ActiveBO.Transaction, "Erp_Category", $"select * from Erp_Category with (nolock) where RecId={categoryId}"))
                                    {
                                        if (table?.Rows.Count > 0)
                                        {
                                            int parentCategoryId;
                                            int.TryParse(table.Rows[0]["ParentId"].ToString(), out parentCategoryId);
                                            while (parentCategoryId != 0)
                                            {
                                                categoryId = parentCategoryId;
                                                using (DataTable table1 = UtilityFunctions.GetDataTableList(quotationReceiptPm.ActiveBO.Provider, quotationReceiptPm.ActiveBO.Connection, quotationReceiptPm.ActiveBO.Transaction, "Erp_Category", $"select * from Erp_Category with (nolock) where RecId={parentCategoryId}"))
                                                {
                                                    if (table1?.Rows.Count > 0)
                                                    {
                                                        int.TryParse(table1.Rows[0]["ParentId"].ToString(), out parentCategoryId);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (categoryId != 0)
                                {
                                    whereStr += $" and InventoryCategoryId={categoryId}";
                                }
                            }
                            decimal itemVariant1Price = 0M, itemVariant2Price = 0M, itemVariant3Price = 0M, itemVariant4Price = 0M, itemVariant5Price = 0M;
                            int itemVariantId;
                            if (!e.Row.IsNull("MarkId"))
                                whereStr += $" and MarkId={e.Row["MarkId"]}";
                            if (!string.IsNullOrEmpty(e.Row["ItemVariant1Code"].ToString()))
                            {
                                string localWhereStr = whereStr;
                                localWhereStr += $" and IsNull(UD_Variant1PriceEfective,false)=true and ItemVariant1Code='{e.Row["ItemVariant1Code"]}'";
                                priceListRows = inventoryBO.Data.Tables["Erp_InventoryPriceList"].Select(localWhereStr, "", DataViewRowState.CurrentRows);
                                if (priceListRows?.Length > 0)
                                {
                                    whereStr += $" and ItemVariant1Code='{e.Row["ItemVariant1Code"]}'";
                                    int.TryParse(e.Row["ItemVariant1Id"].ToString(), out itemVariantId);
                                    if (itemVariantId != 0)
                                        itemVariant1Price = GetItemVariantPrice(itemVariantId, inventoryBO);
                                }
                            }
                            if (!string.IsNullOrEmpty(e.Row["ItemVariant2Code"].ToString()))
                            {
                                string localWhereStr = whereStr;
                                localWhereStr += $" and IsNull(UD_Variant2PriceEfective,false)=true and ItemVariant2Code='{e.Row["ItemVariant2Code"]}'";
                                priceListRows = inventoryBO.Data.Tables["Erp_InventoryPriceList"].Select(localWhereStr, "", DataViewRowState.CurrentRows);
                                if (priceListRows?.Length > 0)
                                {
                                    whereStr += $" and ItemVariant2Code='{e.Row["ItemVariant2Code"]}'";
                                    int.TryParse(e.Row["ItemVariant2Id"].ToString(), out itemVariantId);
                                    if (itemVariantId != 0)
                                        itemVariant2Price = GetItemVariantPrice(itemVariantId, inventoryBO);
                                }
                            }
                            if (!string.IsNullOrEmpty(e.Row["ItemVariant3Code"].ToString()))
                            {
                                string localWhereStr = whereStr;
                                localWhereStr += $" and IsNull(UD_Variant3PriceEfective,false)=true and ItemVariant3Code='{e.Row["ItemVariant3Code"]}'";
                                priceListRows = inventoryBO.Data.Tables["Erp_InventoryPriceList"].Select(localWhereStr, "", DataViewRowState.CurrentRows);
                                if (priceListRows?.Length > 0)
                                {
                                    whereStr += $" and ItemVariant3Code='{e.Row["ItemVariant3Code"]}'";
                                    int.TryParse(e.Row["ItemVariant3Id"].ToString(), out itemVariantId);
                                    if (itemVariantId != 0)
                                        itemVariant3Price = GetItemVariantPrice(itemVariantId, inventoryBO);
                                }
                            }
                            if (!string.IsNullOrEmpty(e.Row["ItemVariant4Code"].ToString()))
                            {
                                string localWhereStr = whereStr;
                                localWhereStr += $" and IsNull(UD_Variant4PriceEfective,false)=true and ItemVariant4Code='{e.Row["ItemVariant4Code"]}'";
                                priceListRows = inventoryBO.Data.Tables["Erp_InventoryPriceList"].Select(localWhereStr, "", DataViewRowState.CurrentRows);
                                if (priceListRows?.Length > 0)
                                {
                                    whereStr += $" and ItemVariant4Code='{e.Row["ItemVariant4Code"]}'";
                                    int.TryParse(e.Row["ItemVariant4Id"].ToString(), out itemVariantId);
                                    if (itemVariantId != 0)
                                        itemVariant4Price = GetItemVariantPrice(itemVariantId, inventoryBO);
                                }
                            }
                            if (!string.IsNullOrEmpty(e.Row["ItemVariant5Code"].ToString()))
                            {
                                string localWhereStr = whereStr;
                                localWhereStr += $" and IsNull(UD_Variant5PriceEfective,false)=true and ItemVariant5Code='{e.Row["ItemVariant5Code"]}'";
                                priceListRows = inventoryBO.Data.Tables["Erp_InventoryPriceList"].Select(localWhereStr, "", DataViewRowState.CurrentRows);
                                if (priceListRows?.Length > 0)
                                {
                                    whereStr += $" and ItemVariant5Code='{e.Row["ItemVariant5Code"]}'";
                                    int.TryParse(e.Row["ItemVariant5Id"].ToString(), out itemVariantId);
                                    if (itemVariantId != 0)
                                        itemVariant5Price = GetItemVariantPrice(itemVariantId, inventoryBO);
                                }
                            }
                            priceListRows = inventoryBO.Data.Tables["Erp_InventoryPriceList"].Select(whereStr, "", DataViewRowState.CurrentRows);
                            if (priceListRows?.Length > 0)
                            {
                                if (!priceListRows[0].IsNull("ForexId"))
                                {
                                    e.Row["ForexId"] = priceListRows[0]["ForexId"];
                                    decimal basedPrice;
                                    decimal.TryParse(priceListRows[0]["Price"].ToString(), out basedPrice);
                                    e.Row["ForexUnitPrice"] = basedPrice + itemVariant1Price + itemVariant2Price + itemVariant3Price + itemVariant4Price + itemVariant5Price;
                                }
                                else
                                {
                                    decimal basedPrice;
                                    decimal.TryParse(priceListRows[0]["Price"].ToString(), out basedPrice);
                                    e.Row["UnitPrice"] = basedPrice + itemVariant1Price + itemVariant2Price + itemVariant3Price + itemVariant4Price + itemVariant5Price;
                                }
                            }
                        }
                    }
                    _suppressEvent = false;
                }
                catch (Exception ex)
                {
                    _suppressEvent = false;
                }
                finally
                {
                    inventoryBO?.Dispose();
                }
            }
        }

        private static decimal GetItemVariantPrice(int itemVariantId, IBusinessObject inventoryBO)
        {
            decimal price = 0;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("SELECT");
            stringBuilder.AppendLine("EVC.RecId");
            stringBuilder.AppendLine(", EVI.CardId");
            stringBuilder.AppendLine(", EVC.TypeId");
            stringBuilder.AppendLine(", EVT.TypeName");
            stringBuilder.AppendLine(", EVT.CompanyId");
            stringBuilder.AppendLine(", EVI.ItemCode");
            stringBuilder.AppendLine(", EVI.ItemName");
            stringBuilder.AppendLine(", EVI.ForexId");
            stringBuilder.AppendLine(", EVI.Price");
            stringBuilder.AppendLine("FROM Erp_VariantItem EVI WITH(NOLOCK)");
            stringBuilder.AppendLine("LEFT JOIN Erp_VariantCard EVC WITH(NOLOCK)");
            stringBuilder.AppendLine("ON EVI.CardId = EVC.RecId");
            stringBuilder.AppendLine("LEFT JOIN Erp_VariantType EVT");
            stringBuilder.AppendLine("ON EVC.TypeId = EVT.RecId");
            stringBuilder.AppendLine($"WHERE EVI.RecId={itemVariantId}");

            using (DataTable table = UtilityFunctions.GetDataTableList(inventoryBO.Provider, inventoryBO.Connection, inventoryBO.Transaction, "Erp_VariantItem", stringBuilder.ToString()))
            {
                if (table?.Rows.Count > 0)
                {
                    decimal.TryParse(table.Rows[0]["Price"].ToString(), out price);
                }
            }

            return price;
        }

        private void QuotationReceiptPm_Dispose_InventoryUnitItemSizeSetDetails(PMBase pm, PmParam parameter)
        {
            quotationReceiptPm = pm as QuotationReceiptPM;
            if (quotationReceiptPm == null)
            {
                return;
            }
            if (quotationReceiptPm.ActiveBO != null)
            {
                quotationReceiptPm.ActiveBO.ColumnChanged -= ActiveBO_ColumnChanged_QuotationReceiptPm;
            }
            quotationReceiptPm.PreviewKeyDown -= QuotationReceiptPm_PreviewKeyDown;
        }
    }
}
