
//using Microsoft.Office.Interop.Excel;
using LiveCore.Desktop.Common;
using LiveCore.Desktop.UI.Controls;

using NermaMetalManagementModule.BoExtensions;

using Prism.Ioc;

using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.PresentationModels;
using Sentez.Common.Utilities;
using Sentez.Data.BusinessObjects;
using Sentez.Data.MetaData;
using Sentez.Data.Tools;
using Sentez.Localization;
using Sentez.QuotationModule.PresentationModels;

using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Sentez.NermaMetalManagementModule
{
    public partial class NermaMetalManagementModule : LiveModule
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
            if(liveDetailPanel != null)
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
            //quotationReceiptPm.PreviewKeyDown += QuotationReceiptPm_PreviewKeyDown;
        }

        private void OnQuotationRecipeLoadExcelCommand(ISysCommandParam obj)
        {

        }

        //private void QuotationReceiptPm_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //}

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
        }

        private void ActiveBO_ColumnChanged_QuotationReceiptPm(object sender, DataColumnChangeEventArgs e)
        {
            //throw new NotImplementedException();
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
            //quotationReceiptPm.PreviewKeyDown -= QuotationReceiptPm_PreviewKeyDown;
        }
    }
}
