using LiveCore.Desktop.UI.Controls;

//using Microsoft.Office.Interop.Excel;
using Microsoft.Practices.Composite.Modularity;

using Sentez.Common.ModuleBase;
using Sentez.Common.PresentationModels;
using Sentez.Common.Utilities;
using Sentez.Data.BusinessObjects;
using Sentez.Data.Tools;
using Sentez.InventoryModule.PresentationModels;
using Sentez.Localization;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sentez.NermaMetalManagementModule
{
    public partial class NermaMetalManagementModule : IModule, ISentezModule
    {
        public LookupList Lists { get; set; }
        LiveDocumentPanel ldpInventoryUnitItemSizeSetDetails;
        InventoryPM inventoryPm;
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
            }
            if (inventoryPm.ActiveBO != null)
            {
                inventoryPm.ActiveBO.AfterGet += ActiveBO_AfterGet;
                inventoryPm.ActiveBO.ColumnChanged += ActiveBO_ColumnChanged;
            }
        }

        bool _suppressEvent = false;
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
                }
            }
            catch (Exception ex)
            {
            }
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

        private void İnventoryPm_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F9)
            {

            }
        }

        private void ActiveBO_AfterGet(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void InventoryBo_Init_InventoryUnitItemSizeSetDetails(BusinessObjectBase bo, BoParam parameter)
        {
            bo.ValueFiller.AddRule("Erp_InventoryUnitItemSizeSetDetails", "InUse", 1);
            bo.ValueFiller.AddRule("Erp_InventoryUnitItemSizeSetDetails", "IsMainUnit", 0);
            bo.ValueFiller.AddRule("Erp_InventoryUnitItemSizeSetDetails", "IsDefault", 0);
            bo.AfterGet += Bo_AfterGet;
        }

        private void Bo_AfterGet(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}
