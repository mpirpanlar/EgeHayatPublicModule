using LiveCore.Desktop.Common;
using LiveCore.Desktop.UI.Controls;

using Prism.Ioc;

using Sentez.Common.Commands;
using Sentez.Common.PresentationModels;
using Sentez.Common.Utilities;
using Sentez.Core.ParameterClasses;
using Sentez.Data.BusinessObjects;
using Sentez.Data.MetaData;
using Sentez.Data.Tools;
using Sentez.Localization;
using Sentez.OrderModule.PresentationModels;

using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sentez.EgeHayatDoorManagementModule
{
    public partial class EgeHayatDoorManagementModule : LiveModule
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

            //new OrderReceiptControlExtension(bo);
        }

        private bool OrderReceiptPm_OnListCommand(PMBase pm, PmParam parameter, ISysCommandParam commandParam)
        {
            return false;
        }

        private void OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails(PMBase pm, PmParam parameter)
        {
            orderReceiptPm = pm as OrderReceiptPM;
            if (orderReceiptPm == null)
            {
                return;
            }
        }

        private void OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails(object sender, RoutedEventArgs e)
        {
        }

        private void OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails(PMBase pm, PmParam parameter)
        {
            orderReceiptPm = pm as OrderReceiptPM;
            if (orderReceiptPm == null)
            {
                return;
            }
            Lists_OrderReceiptPM = orderReceiptPm.ActiveSession.LookupList.GetChild(UtilityFunctions.GetConnection(orderReceiptPm.ActiveSession.dbInfo.DBProvider, orderReceiptPm.ActiveSession.dbInfo.ConnectionString));
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
                                //OpenInventoryCategoryCard(visualelement, orderReceiptPm);
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
        }
    }
}
