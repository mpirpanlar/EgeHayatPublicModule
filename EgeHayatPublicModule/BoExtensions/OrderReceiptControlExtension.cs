using Sentez.Common.SystemServices;
using Sentez.Data.BusinessObjects;
using Sentez.Data.Tools;
using Sentez.Localization;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Sentez.EgeHayatPublicModule.BoExtensions
{
    public class OrderReceiptControlExtension : BoExtensionBase
    {
        public OrderReceiptControlExtension(BusinessObjectBase bo)
            : base(bo)
        {
        }

        protected override void SetBusinessObject(BusinessObjectBase businessObject)
        {
            base.SetBusinessObject(businessObject);
            if (BusinessObject == null)
                return;
        }

        protected override void OnColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            base.OnColumnChanged(sender, e);
            if (!Enabled || _suppressEvents)
                return;
            if (e.Row.Table.TableName == "Erp_OrderReceiptItem" && (e.Column.ColumnName == "InventoryType" || e.Column.ColumnName == "InventoryId"))
            {
                try
                {
                    _suppressEvents = true;
                    long inventoryId;
                    long.TryParse(e.Row["InventoryId"].ToString().Trim(), out inventoryId);
                    if (inventoryId != 0L)
                    {
                        short inventoryType;
                        short.TryParse(e.Row["InventoryType"].ToString(), out inventoryType);
                        if (inventoryType == 0 || inventoryType == (short)InventoryItemType.InventoryItemTypeEnum.ComboBox)
                        {
                            if (e.Row.HasVersion(DataRowVersion.Original))
                            {

                            }
                            else
                            {
                                DataRow[] oldRows = e.Row.Table.Select($"ParentItemId={e.Row["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine}", "", DataViewRowState.CurrentRows);
                                if (oldRows?.Length > 0)
                                {
                                    foreach (DataRow oldRow in oldRows)
                                        oldRow.Delete();
                                }
                            }
                            using (DataTable table = UtilityFunctions.GetDataTableList(BusinessObject.Provider, BusinessObject.Connection, BusinessObject.Transaction, "Erp_RecipeItem", $"select * from Erp_RecipeItem with (nolock) where OwnerInventoryId={inventoryId}"))
                            {
                                if (table?.Rows.Count > 0)
                                {
                                    foreach (DataRow dataRow in table.Rows)
                                    {
                                        long recipeInventoryId;
                                        long.TryParse(dataRow["InventoryId"].ToString(), out recipeInventoryId);
                                        if (recipeInventoryId > 0L)
                                        {
                                            DataRow newChildItemRow = e.Row.Table.NewRow();
                                            e.Row.Table.Rows.Add(newChildItemRow);
                                            newChildItemRow.SetParentRow(BusinessObject.CurrentRow.Row);
                                            newChildItemRow.SetParentRow(e.Row);
                                            newChildItemRow["ItemType"] = (short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine;
                                            newChildItemRow["ReceiptType"] = BusinessObject.CurrentRow["ReceiptType"];
                                            newChildItemRow["InventoryId"] = recipeInventoryId;
                                            newChildItemRow["Quantity"] = dataRow["Quantity"];
                                            newChildItemRow["UD_MaterialType"] = dataRow["UD_MaterialType"];

                                            if (!dataRow.IsNull("UnitId"))
                                            {
                                                using (DataTable table2 = UtilityFunctions.GetDataTableList(BusinessObject.Provider, BusinessObject.Connection, BusinessObject.Transaction, "Erp_InventoryUnitItemSize", $"select * from Erp_InventoryUnitItemSize with (nolock) where RecId={dataRow["UnitId"]}"))
                                                {
                                                    if (table2?.Rows.Count > 0)
                                                    {
                                                        newChildItemRow["UnitId"] = table2.Rows[0]["UnitItemId"];
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    _suppressEvents = false;
                }
                catch { _suppressEvents = false; }
            }
            else if (e.Row.Table.TableName == "Erp_OrderReceiptItem" && (e.Column.ColumnName == "Quantity"))
            {
                try
                {
                    _suppressEvents = true;
                    long inventoryId;
                    long.TryParse(e.Row["InventoryId"].ToString().Trim(), out inventoryId);
                    if (inventoryId != 0L)
                    {
                        short inventoryType;
                        short.TryParse(e.Row["InventoryType"].ToString(), out inventoryType);
                        if (inventoryType == 0 || inventoryType == (short)InventoryItemType.InventoryItemTypeEnum.ComboBox)
                        {
                            using (DataTable table = UtilityFunctions.GetDataTableList(BusinessObject.Provider, BusinessObject.Connection, BusinessObject.Transaction, "Erp_RecipeItem", $"select * from Erp_RecipeItem with (nolock) where OwnerInventoryId={inventoryId}"))
                            {
                                if (table?.Rows.Count > 0)
                                {
                                    foreach (DataRow dataRow in table.Rows)
                                    {
                                        long recipeInventoryId;
                                        long.TryParse(dataRow["InventoryId"].ToString(), out recipeInventoryId);
                                        if (recipeInventoryId > 0L)
                                        {
                                            DataRow[] receiptItemRow = e.Row.Table.Select($"InventoryId={recipeInventoryId} and UD_MaterialType='{dataRow["UD_MaterialType"]}' and ParentItemId={e.Row["RecId"]}", "", DataViewRowState.CurrentRows);
                                            if (receiptItemRow?.Length > 0)
                                            {
                                                decimal orderItemQuantity, recipeItemQuantity;
                                                decimal.TryParse(dataRow["Quantity"].ToString(), out recipeItemQuantity);
                                                decimal.TryParse(e.Row["Quantity"].ToString(), out orderItemQuantity);
                                                receiptItemRow[0]["Quantity"] = orderItemQuantity * recipeItemQuantity;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    _suppressEvents = false;
                }
                catch { _suppressEvents = false; }
            }

            #region Kasa Hesaplamaları
            else if (e.Row.Table.TableName == "Erp_OrderReceipt" && (e.Column.ColumnName == "UD_FrameHeight"))
            {
                DataRow[] mainRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.Inventory}", "", DataViewRowState.CurrentRows);
                foreach (DataRow mainRow in mainRows)
                {
                    decimal udFrameHeight, rowHeight;
                    decimal.TryParse(e.Row["UD_FrameHeight"].ToString(), out udFrameHeight);
                    decimal.TryParse(mainRow["UD_Height"].ToString(), out rowHeight);
                    DataRow[] frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kasa'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Height"] = rowHeight - udFrameHeight;
                    }
                }
            }
            else if (e.Row.Table.TableName == "Erp_OrderReceiptItem" && (e.Column.ColumnName == "UD_Height"))
            {
                DataRow[] mainRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"RecId={e.Row["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.Inventory}", "", DataViewRowState.CurrentRows);

                #region Detaydan Hesaplama (Kasa)
                foreach (DataRow mainRow in mainRows)
                {
                    decimal udFrameHeight, rowHeight;
                    decimal.TryParse(BusinessObject.CurrentRow.Row["UD_FrameHeight"].ToString(), out udFrameHeight);
                    decimal.TryParse(mainRow["UD_Height"].ToString(), out rowHeight);
                    DataRow[] frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kasa'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Height"] = rowHeight - udFrameHeight;
                    }
                }
                #endregion

                #region Detaydan Hesaplama (Kanat)
                foreach (DataRow mainRow in mainRows)
                {
                    decimal rowFrameHeight = 0M;
                    DataRow[] frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kasa'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        decimal.TryParse(frameRow["UD_Height"].ToString(), out rowFrameHeight);
                    }

                    decimal udWingHeight;
                    decimal.TryParse(BusinessObject.CurrentRow.Row["UD_WingHeight"].ToString(), out udWingHeight);
                    frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kanat'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Height"] = rowFrameHeight - udWingHeight;
                    }
                }
                #endregion

                #region Detaydan hesaplama (Pervaz)
                foreach (DataRow mainRow in mainRows)
                {
                    decimal rowFrameHeight = 0M, rowFrameWidth = 0M;
                    decimal udSideArchitraveWidth;
                    decimal.TryParse(BusinessObject.CurrentRow["UD_SideArchitraveWidth"].ToString(), out udSideArchitraveWidth);
                    if (udSideArchitraveWidth == 0M)
                        udSideArchitraveWidth = 0.8M;
                    DataRow[] frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kasa'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        decimal.TryParse(frameRow["UD_Height"].ToString(), out rowFrameHeight);
                        decimal.TryParse(frameRow["UD_Width"].ToString(), out rowFrameWidth);
                    }

                    decimal udArchitraveHeight, udArchitraveWidth;
                    decimal.TryParse(BusinessObject.CurrentRow.Row["UD_ArchitraveHeight"].ToString(), out udArchitraveHeight);
                    decimal.TryParse(BusinessObject.CurrentRow.Row["UD_ArchitraveWidth"].ToString(), out udArchitraveWidth);
                    frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Boy Pervaz'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Width"] = udArchitraveWidth;
                        frameRow["UD_Height"] = (rowFrameHeight + udArchitraveWidth) - udSideArchitraveWidth;
                    }

                    frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Tepe Pervaz'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Width"] = udArchitraveWidth;
                        frameRow["UD_Height"] = rowFrameWidth - 1.6m;
                    }
                }
                #endregion
            }
            else if (e.Row.Table.TableName == "Erp_OrderReceipt" && (e.Column.ColumnName == "UD_FrameWidth"))
            {
                DataRow[] mainRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.Inventory}", "", DataViewRowState.CurrentRows);
                foreach (DataRow mainRow in mainRows)
                {
                    decimal udFrameWidth, rowWidth;
                    decimal.TryParse(e.Row["UD_FrameWidth"].ToString(), out udFrameWidth);
                    decimal.TryParse(mainRow["UD_Width"].ToString(), out rowWidth);
                    DataRow[] frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kasa'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Width"] = rowWidth - udFrameWidth;
                    }
                }

            }
            else if (e.Row.Table.TableName == "Erp_OrderReceiptItem" && (e.Column.ColumnName == "UD_Width"))
            {
                DataRow[] mainRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"RecId={e.Row["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.Inventory}", "", DataViewRowState.CurrentRows);

                #region Detaydan Hesaplama (Kasa)
                foreach (DataRow mainRow in mainRows)
                {
                    decimal udFrameWidth, rowWidth;
                    decimal.TryParse(BusinessObject.CurrentRow.Row["UD_FrameWidth"].ToString(), out udFrameWidth);
                    decimal.TryParse(mainRow["UD_Width"].ToString(), out rowWidth);
                    DataRow[] frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kasa'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Width"] = rowWidth - udFrameWidth;
                    }
                }
                #endregion

                #region Detaydan Hesaplama (Kanat)
                foreach (DataRow mainRow in mainRows)
                {
                    decimal rowFrameWidth = 0M;
                    DataRow[] frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kasa'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        decimal.TryParse(frameRow["UD_Width"].ToString(), out rowFrameWidth);
                    }

                    decimal udWingWidth;
                    decimal.TryParse(BusinessObject.CurrentRow.Row["UD_WingWidth"].ToString(), out udWingWidth);
                    frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kanat'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Width"] = rowFrameWidth - udWingWidth;
                    }
                }
                #endregion

                #region Detaydan Hesaplama (Pervaz)
                foreach (DataRow mainRow in mainRows)
                {
                    decimal rowFrameHeight = 0M, rowFrameWidth = 0M;
                    decimal udSideArchitraveWidth;
                    decimal.TryParse(BusinessObject.CurrentRow["UD_SideArchitraveWidth"].ToString(), out udSideArchitraveWidth);
                    if (udSideArchitraveWidth == 0M)
                        udSideArchitraveWidth = 0.8M;
                    DataRow[] frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kasa'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        decimal.TryParse(frameRow["UD_Height"].ToString(), out rowFrameHeight);
                        decimal.TryParse(frameRow["UD_Width"].ToString(), out rowFrameWidth);
                    }

                    decimal udArchitraveHeight, udArchitraveWidth;
                    decimal.TryParse(BusinessObject.CurrentRow.Row["UD_ArchitraveHeight"].ToString(), out udArchitraveHeight);
                    decimal.TryParse(BusinessObject.CurrentRow.Row["UD_ArchitraveWidth"].ToString(), out udArchitraveWidth);
                    frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Boy Pervaz'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Width"] = udArchitraveWidth;
                        frameRow["UD_Height"] = (rowFrameHeight + udArchitraveWidth) - udSideArchitraveWidth;
                    }

                    frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Tepe Pervaz'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Width"] = udArchitraveWidth;
                        frameRow["UD_Height"] = rowFrameWidth - 1.6m;
                    }
                }
                #endregion
            }
            #endregion

            #region Kanat Hesaplamaları
            else if (e.Row.Table.TableName == "Erp_OrderReceipt" && (e.Column.ColumnName == "UD_WingHeight"))
            {
                DataRow[] mainRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.Inventory}", "", DataViewRowState.CurrentRows);
                foreach (DataRow mainRow in mainRows)
                {
                    decimal rowFrameHeight = 0M;
                    DataRow[] frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kasa'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        decimal.TryParse(frameRow["UD_Height"].ToString(), out rowFrameHeight);
                    }

                    decimal udWingHeight;
                    decimal.TryParse(e.Row["UD_WingHeight"].ToString(), out udWingHeight);
                    frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kanat'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Height"] = rowFrameHeight - udWingHeight;
                    }
                }
            }
            else if (e.Row.Table.TableName == "Erp_OrderReceipt" && (e.Column.ColumnName == "UD_WingWidth"))
            {
                DataRow[] mainRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.Inventory}", "", DataViewRowState.CurrentRows);
                foreach (DataRow mainRow in mainRows)
                {
                    decimal rowFrameWidth = 0M;
                    DataRow[] frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kasa'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        decimal.TryParse(frameRow["UD_Width"].ToString(), out rowFrameWidth);
                    }

                    decimal udWingWidth;
                    decimal.TryParse(e.Row["UD_WingWidth"].ToString(), out udWingWidth);
                    frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kanat'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Width"] = rowFrameWidth - udWingWidth;
                    }
                }
            }
            #endregion

            #region Tepe Pervaz Hesaplamaları
            else if (e.Row.Table.TableName == "Erp_OrderReceipt" && (e.Column.ColumnName == "UD_ArchitraveHeight" || e.Column.ColumnName == "UD_ArchitraveWidth"))
            {
                DataRow[] mainRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.Inventory}", "", DataViewRowState.CurrentRows);
                foreach (DataRow mainRow in mainRows)
                {
                    decimal rowFrameHeight = 0M, rowFrameWidth = 0M;
                    decimal udSideArchitraveWidth;
                    decimal.TryParse(BusinessObject.CurrentRow["UD_SideArchitraveWidth"].ToString(), out udSideArchitraveWidth);
                    if (udSideArchitraveWidth == 0M)
                        udSideArchitraveWidth = 0.8M;
                    DataRow[] frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Kasa'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        decimal.TryParse(frameRow["UD_Height"].ToString(), out rowFrameHeight);
                        decimal.TryParse(frameRow["UD_Width"].ToString(), out rowFrameWidth);
                    }

                    decimal udArchitraveHeight, udArchitraveWidth;
                    decimal.TryParse(e.Row["UD_ArchitraveHeight"].ToString(), out udArchitraveHeight);
                    decimal.TryParse(e.Row["UD_ArchitraveWidth"].ToString(), out udArchitraveWidth);
                    frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Boy Pervaz'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Width"] = udArchitraveWidth;
                        frameRow["UD_Height"] = (rowFrameHeight + udArchitraveWidth) - udSideArchitraveWidth;
                    }

                    frameRows = BusinessObject.Data.Tables["Erp_OrderReceiptItem"].Select($"ParentItemId={mainRow["RecId"]} and ItemType={(short)ReceiptItemTypeDefinition.ReceiptItemTypeItem.SelectInvoiceLine} and UD_MaterialType='Tepe Pervaz'", "", DataViewRowState.CurrentRows);
                    foreach (DataRow frameRow in frameRows)
                    {
                        frameRow["UD_Width"] = udArchitraveWidth;
                        frameRow["UD_Height"] = rowFrameWidth - 1.6m;
                    }
                }
            }
            #endregion
        }

        protected override void OnBeforePost(object sender, CancelEventArgs e)
        {
            if (!Enabled || _suppressEvents)
                return;
            base.OnBeforePost(sender, e);
        }
    }
}
