using LiveCore.Desktop.Common;
using LiveCore.Desktop.UI.Controls;

using Prism.Ioc;

//using Microsoft.Office.Interop.Excel;
using Sentez.Common.ModuleBase;
using Sentez.Common.PresentationModels;
using Sentez.Data.BusinessObjects;
using Sentez.Data.MetaData;
using Sentez.Data.Tools;
using Sentez.InventoryModule.PresentationModels;
using Sentez.Localization;

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sentez.NermaMetalManagementModule
{
    public partial class NermaMetalManagementModule : LiveModule
    {
        private void InventoryBoCustomCons(ref short itemId, ref string keyColumn, ref string typeField, ref string[] Tables)
        {
            List<string> tableList = new List<string>();
            tableList.AddRange(Tables);

            tableList.Add("Erp_InventoryUnitItemSizeSetDetails");
            tableList.Add("Erp_InventoryMark");
            Tables = tableList.ToArray();
        }

        private void InventoryPm_Init_InventoryUnitItemSizeSetDetails(PMBase pm, PmParam parameter)
        {
            inventoryPm = pm as InventoryPM;
            if (inventoryPm == null)
            {
                return;
            }
            Lists = inventoryPm.ActiveSession.LookupList.GetChild(UtilityFunctions.GetConnection(inventoryPm.ActiveSession.dbInfo.DBProvider, inventoryPm.ActiveSession.dbInfo.ConnectionString));
            LiveDocumentGroup liveDocumentGroup = inventoryPm.FCtrl("GenelDocumentPanel") as LiveDocumentGroup;
            if (liveDocumentGroup != null)
            {
                ldpInventoryUnitItemSizeSetDetails = new LiveDocumentPanel();
                ldpInventoryUnitItemSizeSetDetails.Caption = SLanguage.GetString("Ölçüler");
                liveDocumentGroup.Items.Add(ldpInventoryUnitItemSizeSetDetails);

                PMDesktop pMDesktop = inventoryPm.container.Resolve<PMDesktop>();
                var tsePublicParametersView = pMDesktop.LoadXamlRes("InventoryUnitItemSizeSetDetailsView");
                (tsePublicParametersView._view as UserControl).DataContext = inventoryPm;
                ldpInventoryUnitItemSizeSetDetails.Content = tsePublicParametersView._view;

                ldpInventoryMark = new LiveDocumentPanel();
                ldpInventoryMark.Caption = SLanguage.GetString("Markalar");
                liveDocumentGroup.Items.Add(ldpInventoryMark);

                var ldpInventoryMarkView = pMDesktop.LoadXamlRes("InventoryMarksView");
                (ldpInventoryMarkView._view as UserControl).DataContext = inventoryPm;
                ldpInventoryMark.Content = ldpInventoryMarkView._view;
            }
            if (inventoryPm.ActiveBO != null)
            {
                inventoryPm.ActiveBO.AfterGet += ActiveBO_AfterGet;
                inventoryPm.ActiveBO.ColumnChanged += ActiveBO_ColumnChanged;
            }
        }

        private void ActiveBO_AfterGet(object sender, EventArgs e)
        {
            int categoryId;
            int.TryParse(inventoryPm.ActiveBO.CurrentRow["CategoryId"].ToString(), out categoryId);
            inventoryPm.ActiveBO.CurrentRow["CategoryFullPath"] = GetCategoryFullPath(categoryId);
        }

        private void InventoryPm_Dispose_InventoryUnitItemSizeSetDetails(PMBase pm, PmParam parameter)
        {
            inventoryPm = pm as InventoryPM;
            if (inventoryPm == null)
            {
                return;
            }
            if (inventoryPm.ActiveBO != null)
            {
                inventoryPm.ActiveBO.AfterGet -= ActiveBO_AfterGet;
                inventoryPm.ActiveBO.ColumnChanged -= ActiveBO_ColumnChanged;
            }
        }

        private void ActiveBO_ColumnChanged(object sender, System.Data.DataColumnChangeEventArgs e)
        {
            if (_suppressEvent)
                return;
            try
            {
                if (inventoryPm.ActiveBO?.CurrentRow != null)
                {
                    if (e.Row.Table.TableName == "Erp_InventoryUnitItemSizeSetDetails")
                    {
                        if (e.Column.ColumnName == "SizeDetailCode")
                        {
                            long recId;
                            long.TryParse(e.Row[e.Column.ColumnName].ToString(), out recId);
                            if (recId > 0L)
                            {
                                _suppressEvent = true;
                                using (DataTable table = UtilityFunctions.GetDataTableList(inventoryPm.ActiveBO.Provider, inventoryPm.ActiveBO.Connection, inventoryPm.ActiveBO.Transaction, "Erp_UnitItemSizeSetDetails", $"select * from Erp_UnitItemSizeSetDetails with (nolock) where RecId={recId}"))
                                {
                                    if (table?.Rows.Count > 0)
                                        UpdateUnitItemSizeSetDetailsValue(e, table);
                                    else
                                    {
                                        using (DataTable table2 = UtilityFunctions.GetDataTableList(inventoryPm.ActiveBO.Provider, inventoryPm.ActiveBO.Connection, inventoryPm.ActiveBO.Transaction, "Erp_UnitItemSizeSetDetails", $"select * from Erp_UnitItemSizeSetDetails with (nolock) where SizeDetailCode='{e.Row["SizeDetailCode"]}'"))
                                        {
                                            if (table2?.Rows.Count > 0)
                                                UpdateUnitItemSizeSetDetailsValue(e, table2);
                                        }
                                    }
                                }
                                _suppressEvent = false;
                            }
                            else
                            {
                                _suppressEvent = true;
                                using (DataTable table = UtilityFunctions.GetDataTableList(inventoryPm.ActiveBO.Provider, inventoryPm.ActiveBO.Connection, inventoryPm.ActiveBO.Transaction, "Erp_UnitItemSizeSetDetails", $"select * from Erp_UnitItemSizeSetDetails with (nolock) where SizeDetailCode='{e.Row["SizeDetailCode"]}'"))
                                {
                                    if (table?.Rows.Count > 0)
                                        UpdateUnitItemSizeSetDetailsValue(e, table);
                                }
                                _suppressEvent = false;
                            }
                        }
                    }
                    else if (e.Row.Table.TableName == "Erp_Inventory")
                    {
                        if (e.Column.ColumnName == "CategoryName" && !e.Row.IsNull("CategoryId"))
                        {
                            _suppressEvent = true;
                            int categoryId;
                            int.TryParse(e.Row["CategoryId"].ToString(), out categoryId);
                            e.Row["CategoryFullPath"] = GetCategoryFullPath(categoryId);
                            _suppressEvent = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _suppressEvent = false;
            }
        }

        private object GetCategoryFullPath(int categoryId)
        {
            int catId = categoryId;
            string catPath = "";
            while (catId > 0)
            {
                using (DataTable table = UtilityFunctions.GetDataTableList(inventoryPm.ActiveBO.Provider, inventoryPm.ActiveBO.Connection, inventoryPm.ActiveBO.Transaction, "Erp_Category", $"select * from Erp_Category with (nolock) where RecId={catId}"))
                {
                    if (table?.Rows.Count > 0)
                    {
                        if (string.IsNullOrEmpty(catPath))
                            catPath = table.Rows[0]["CategoryName2"].ToString();
                        else catPath = $"{table.Rows[0]["CategoryName2"]} > {catPath}";
                        int parentId;
                        int.TryParse(table.Rows[0]["ParentId"].ToString().Trim(), out parentId);
                        catId = parentId;
                    }
                }
            }
            return catPath;
        }

        private static void UpdateUnitItemSizeSetDetailsValue(DataColumnChangeEventArgs e, DataTable table)
        {
            foreach (DataColumn dataColumn in table.Columns)
            {
                if (dataColumn.ColumnName == "RecId")
                    continue;
                if (e.Row.Table.Columns.Contains(dataColumn.ColumnName))
                    e.Row[dataColumn.ColumnName] = table.Rows[0][dataColumn.ColumnName];
            }
        }

        private static void UpdateCategoryAttributeSetDetailsValue(DataColumnChangeEventArgs e, DataTable table)
        {
            foreach (DataColumn dataColumn in table.Columns)
            {
                if (dataColumn.ColumnName == "RecId")
                    continue;
                if (e.Row.Table.Columns.Contains(dataColumn.ColumnName))
                    e.Row[dataColumn.ColumnName] = table.Rows[0][dataColumn.ColumnName];
            }
        }

        //private void InventoryPm_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    if (e.Key == Key.F9)
        //    {

        //    }
        //}

        private void InventoryBo_Init_InventoryUnitItemSizeSetDetails(BusinessObjectBase bo, BoParam parameter)
        {
            bo.ValueFiller.AddRule("Erp_InventoryUnitItemSizeSetDetails", "InUse", 1);
            bo.ValueFiller.AddRule("Erp_InventoryUnitItemSizeSetDetails", "IsMainUnit", 0);
            bo.ValueFiller.AddRule("Erp_InventoryUnitItemSizeSetDetails", "IsDefault", 0);

            bo.ValueFiller.AddRule("Erp_InventoryMark", "InUse", 1);

            bo.Lookups.AddLookUp("Erp_InventoryMark", "MarkId", true, "Erp_Mark", "MarkName", "MarkName", "Explanation", "MarkExplanation");
            if (bo.Data.Tables.Contains("Erp_Inventory"))
            {
                if (!bo.Data.Tables["Erp_Inventory"].Columns.Contains("CategoryFullPath"))
                    bo.Data.Tables["Erp_Inventory"].Columns.Add(new DataColumn() { ColumnName = "CategoryFullPath", DataType = UdtTypes.GetUdtSystemType(Schema.Tables["Erp_InventoryReceiptItem"].Fields["Explanation"].UdtType) });
            }
        }

        private void CategoryBoCustomCons(ref short itemId, ref string keyColumn, ref string typeField, ref string[] Tables)
        {
            List<string> tableList = new List<string>();
            tableList.AddRange(Tables);

            tableList.Add("Erp_InventoryUnitItemSizeSetDetails");
            tableList.Add("Erp_CategoryAttributeSetDetails");
            Tables = tableList.ToArray();
        }

        private void CategoryBo_Init_InventoryUnitItemSizeSetDetails(BusinessObjectBase bo, BoParam parameter)
        {
            bo.ValueFiller.AddRule("Erp_InventoryUnitItemSizeSetDetails", "InUse", 1);
            bo.ValueFiller.AddRule("Erp_InventoryUnitItemSizeSetDetails", "IsMainUnit", 0);
            bo.ValueFiller.AddRule("Erp_InventoryUnitItemSizeSetDetails", "IsDefault", 0);

            bo.ValueFiller.AddRule("Erp_CategoryAttributeSetDetails", "InUse", 1);
            bo.ValueFiller.AddRule("Erp_CategoryAttributeSetDetails", "IsSelect", 0);
            bo.ValueFiller.AddRule("Erp_CategoryAttributeSetDetails", "IsDeleted", 0);

            bo.Lookups.AddLookUp("Erp_Category", "AttributeSetId", true, "Erp_InventoryAttributeSet", "AttributeCode", "AttributeCode", "AttributeName", "AttributeName");
        }

        private void CategoryPm_Dispose_InventoryUnitItemSizeSetDetails(PMBase pm, PmParam parameter)
        {
        }

        private void CategoryPm_Init_InventoryUnitItemSizeSetDetails(PMBase pm, PmParam parameter)
        {
            categoryPm = pm as CardPM;
            if (categoryPm == null)
            {
                return;
            }
            Lists = categoryPm.ActiveSession.LookupList.GetChild(UtilityFunctions.GetConnection(categoryPm.ActiveSession.dbInfo.DBProvider, categoryPm.ActiveSession.dbInfo.ConnectionString));
            LiveTabControl liveDocumentGroup = categoryPm.FCtrl("GenelTab") as LiveTabControl;
            if (liveDocumentGroup != null)
            {
                ldpCategoryUnitItemSizeSetDetails = new LiveTabItem();
                ldpCategoryUnitItemSizeSetDetails.Header = SLanguage.GetString("Ölçüler");
                liveDocumentGroup.Items.Add(ldpCategoryUnitItemSizeSetDetails);

                PMDesktop pMDesktop = categoryPm.container.Resolve<PMDesktop>();
                var tsePublicParametersView = pMDesktop.LoadXamlRes("CategoryUnitItemSizeSetDetailsView");
                (tsePublicParametersView._view as UserControl).DataContext = categoryPm;
                ldpCategoryUnitItemSizeSetDetails.Content = tsePublicParametersView._view;

                ldpCategoryAttributeSetDetails = new LiveTabItem();
                ldpCategoryAttributeSetDetails.Header = SLanguage.GetString("Özellikler");
                liveDocumentGroup.Items.Add(ldpCategoryAttributeSetDetails);

                var tseCategoryAttributeSetDetailsView = pMDesktop.LoadXamlRes("CategoryAttributeSetDetailsView");
                (tseCategoryAttributeSetDetailsView._view as UserControl).DataContext = categoryPm;
                ldpCategoryAttributeSetDetails.Content = tseCategoryAttributeSetDetailsView._view;
            }
            if (categoryPm.ActiveBO != null)
            {
                //categoryPm.ActiveBO.AfterGet += ActiveBO_AfterGet;
                categoryPm.ActiveBO.ColumnChanged += categoryPm_ActiveBO_ColumnChanged;
            }
        }

        private void categoryPm_ActiveBO_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (_suppressEvent)
                return;
            try
            {
                if (categoryPm.ActiveBO?.CurrentRow != null)
                {
                    if (e.Row.Table.TableName == "Erp_InventoryUnitItemSizeSetDetails")
                    {
                        if (e.Column.ColumnName == "SizeDetailCode")
                        {
                            long recId;
                            long.TryParse(e.Row[e.Column.ColumnName].ToString(), out recId);
                            if (recId > 0L)
                            {
                                _suppressEvent = true;
                                using (DataTable table = UtilityFunctions.GetDataTableList(categoryPm.ActiveBO.Provider, categoryPm.ActiveBO.Connection, categoryPm.ActiveBO.Transaction, "Erp_UnitItemSizeSetDetails", $"select * from Erp_UnitItemSizeSetDetails with (nolock) where RecId={recId}"))
                                {
                                    if (table?.Rows.Count > 0)
                                        UpdateUnitItemSizeSetDetailsValue(e, table);
                                    else
                                    {
                                        using (DataTable table2 = UtilityFunctions.GetDataTableList(categoryPm.ActiveBO.Provider, categoryPm.ActiveBO.Connection, inventoryPm.ActiveBO.Transaction, "Erp_UnitItemSizeSetDetails", $"select * from Erp_UnitItemSizeSetDetails with (nolock) where SizeDetailCode='{e.Row["SizeDetailCode"]}'"))
                                        {
                                            if (table2?.Rows.Count > 0)
                                                UpdateUnitItemSizeSetDetailsValue(e, table2);
                                        }
                                    }
                                }
                                _suppressEvent = false;
                            }
                            else
                            {
                                _suppressEvent = true;
                                using (DataTable table = UtilityFunctions.GetDataTableList(categoryPm.ActiveBO.Provider, categoryPm.ActiveBO.Connection, categoryPm.ActiveBO.Transaction, "Erp_UnitItemSizeSetDetails", $"select * from Erp_UnitItemSizeSetDetails with (nolock) where SizeDetailCode='{e.Row["SizeDetailCode"]}'"))
                                {
                                    if (table?.Rows.Count > 0)
                                        UpdateUnitItemSizeSetDetailsValue(e, table);
                                }
                                _suppressEvent = false;
                            }
                        }
                    }
                    else if (e.Row.Table.TableName == "Erp_CategoryAttributeSetDetails")
                    {
                        if (e.Column.ColumnName == "AttributeSetCode")
                        {
                            long recId;
                            long.TryParse(e.Row[e.Column.ColumnName].ToString(), out recId);
                            if (recId > 0L)
                            {
                                _suppressEvent = true;
                                using (DataTable table = UtilityFunctions.GetDataTableList(categoryPm.ActiveBO.Provider, categoryPm.ActiveBO.Connection, categoryPm.ActiveBO.Transaction, "Erp_AttributeSetDetails", $"select * from Erp_AttributeSetDetails with (nolock) where RecId={recId}"))
                                {
                                    if (table?.Rows.Count > 0)
                                        UpdateCategoryAttributeSetDetailsValue(e, table);
                                    else
                                    {
                                        using (DataTable table2 = UtilityFunctions.GetDataTableList(categoryPm.ActiveBO.Provider, categoryPm.ActiveBO.Connection, inventoryPm.ActiveBO.Transaction, "Erp_AttributeSetDetails", $"select * from Erp_AttributeSetDetails with (nolock) where AttributeSetCode='{e.Row["AttributeSetCode"]}'"))
                                        {
                                            if (table2?.Rows.Count > 0)
                                                UpdateCategoryAttributeSetDetailsValue(e, table2);
                                        }
                                    }
                                }
                                _suppressEvent = false;
                            }
                            else
                            {
                                _suppressEvent = true;
                                using (DataTable table = UtilityFunctions.GetDataTableList(categoryPm.ActiveBO.Provider, categoryPm.ActiveBO.Connection, categoryPm.ActiveBO.Transaction, "Erp_AttributeSetDetails", $"select * from Erp_AttributeSetDetails with (nolock) where AttributeSetCode='{e.Row["AttributeSetCode"]}'"))
                                {
                                    if (table?.Rows.Count > 0)
                                        UpdateCategoryAttributeSetDetailsValue(e, table);
                                }
                                _suppressEvent = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
