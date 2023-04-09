using LiveCore.Desktop.UI.Controls;

using Microsoft.Practices.Composite.Modularity;

using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.PresentationModels;
using Sentez.Common.Utilities;
using Sentez.Data.BusinessObjects;
using Sentez.Data.Tools;
using Sentez.InventoryModule.PresentationModels;
using Sentez.Localization;
using Sentez.QuotationModule.PresentationModels;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sentez.NermaMetalManagementModule
{
    public partial class NermaMetalManagementModule : IModule, ISentezModule
    {
        private void VariantTypeBoCustomCons(ref short itemId, ref string keyColumn, ref string typeField, ref string[] Tables)
        {
            List<string> tableList = new List<string>();
            tableList.AddRange(Tables);

            tableList.Add("Erp_VariantItemMark");
            Tables = tableList.ToArray();
        }

        private void VariantTypeBo_Init_VariantItemMark(BusinessObjectBase bo, BoParam parameter)
        {
            bo.Lookups.AddLookUp("Erp_VariantItemMark", "VariantItemId", false, "Erp_VariantItem", "ItemCode", "ItemCode", "ItemName", "ItemName");
            bo.Lookups.AddLookUp("Erp_VariantItemMark", "MarkId", true, "Erp_Mark", "MarkName", "MarkName", "Explanation", "MarkExplanation");
            bo.ValueFiller.AddRule("Erp_VariantItemMark", "InUse", "Erp_VariantItem", "InUse", "FK_Erp_VariantItemMark_Erp_VariantItem");
        }

        private void VariantTypePm_Init_VariantItemMark(PMBase pm, PmParam parameter)
        {
            variantTypePm = pm as VariantTypePM;
            if (variantTypePm == null)
            {
                return;
            }
            Lists = variantTypePm.ActiveSession.LookupList.GetChild(UtilityFunctions.GetConnection(variantTypePm.ActiveSession.dbInfo.DBProvider, variantTypePm.ActiveSession.dbInfo.ConnectionString));
            LiveDocumentGroup liveDocumentGroup = variantTypePm.FCtrl("GenelDocumentPanel") as LiveDocumentGroup;
            if (liveDocumentGroup != null)
            {
                ldpVariantItemMark = new LiveDocumentPanel();
                ldpVariantItemMark.Caption = SLanguage.GetString("Markalar");
                liveDocumentGroup.Items.Add(ldpVariantItemMark);

                PMDesktop pMDesktop = variantTypePm.container.Resolve<PMDesktop>();
                var tsePublicParametersView = pMDesktop.LoadXamlRes("VariantItemMarksView");
                (tsePublicParametersView._view as UserControl).DataContext = variantTypePm;
                ldpVariantItemMark.Content = tsePublicParametersView._view;

                gridVariantItems = variantTypePm.FCtrl("gridDetail") as LiveGridControl;
                if (gridVariantItems != null)
                {
                    gridVariantItems.AfterCreateNewRow += GridVariantItems_AfterCreateNewRow;
                    gridVariantItems.CurrentItemChanged += GridVariantItems_CurrentItemChanged;
                }

                LiveGridControl[] grids = FrameworkTreeHelper.FindLogicalChilds<LiveGridControl>((pm as PMDesktop).ActiveViewControl);
                if (grids != null)
                {
                    foreach (LiveGridControl grd in grids.Where(b => b.Name == "gridVariantItemMarks"))
                    {
                        gridVariantItemMarks = grd;
                        gridVariantItemMarks.BeforeCreateNewRow += GridVariantItemMarks_BeforeCreateNewRow;
                        gridVariantItemMarks.AfterCreateNewRow += GridVariantItemMarks_AfterCreateNewRow;
                    }
                }
            }

            if (variantTypePm.ActiveBO != null)
            {
                //variantTypePm.ActiveBO.AfterGet += ActiveBO_AfterGet;
                //variantTypePm.ActiveBO.ColumnChanged += ActiveBO_ColumnChanged;
            }
        }

        private void GridVariantItemMarks_BeforeCreateNewRow(object sender, LiveGridControl.BeforeCreateNewRowEventArgs e)
        {
            //if (gridVariantItems?.CurrentItem != null)
            //{
            //    int variantItemId;
            //    int.TryParse((gridVariantItems.CurrentItem as DataRowView).Row["RecId"].ToString(), out variantItemId);
            //    if (gridVariantItemMarks != null)
            //    {
            //        DataView dataView = variantTypePm.ActiveBO.Data.Tables["Erp_VariantItemMark"].DefaultView;
            //        dataView.RowFilter = $"VariantItemId={variantItemId}";
            //        gridVariantItemMarks.ItemsSource = dataView;
            //    }
            //}
        }

        private void GridVariantItemMarks_AfterCreateNewRow(object sender, LiveGridControl.AfterCreateNewRowEventArgs e)
        {
            if (e.NewRow is DataRowView)
            {
                if (gridVariantItems?.CurrentItem != null)
                {
                    int variantItemId;
                    int.TryParse((gridVariantItems.CurrentItem as DataRowView).Row["RecId"].ToString(), out variantItemId);
                    if (gridVariantItemMarks != null)
                    {
                        DataView dataView = variantTypePm.ActiveBO.Data.Tables["Erp_VariantItemMark"].DefaultView;
                        dataView.RowFilter = $"VariantItemId={variantItemId}";
                        gridVariantItemMarks.ItemsSource = dataView;
                    }

                    (e.NewRow as DataRowView).Row["TypeId"] = variantTypePm.ActiveBO.CurrentRow["RecId"];
                    if (variantTypePm.ActiveBO.Data.Tables["Erp_VariantCard"].Rows.Count > 0)
                        (e.NewRow as DataRowView).Row["CardId"] = variantTypePm.ActiveBO.Data.Tables["Erp_VariantCard"].Rows[0]["RecId"];
                    (e.NewRow as DataRowView).Row["VariantItemId"] = variantItemId;
                }
            }
        }

        private void VariantTypePm_ViewLoaded_VariantItemMark(object sender, RoutedEventArgs e)
        {
            //
        }

        private void GridVariantItems_CurrentItemChanged(object sender, DevExpress.Xpf.Grid.CurrentItemChangedEventArgs e)
        {
            if (e.NewItem != null && e.NewItem is DataRowView)
            {
                int variantItemId;
                int.TryParse((e.NewItem as DataRowView).Row["RecId"].ToString(), out variantItemId);
                if (gridVariantItemMarks != null)
                {
                    DataView dataView = variantTypePm.ActiveBO.Data.Tables["Erp_VariantItemMark"].DefaultView;
                    dataView.RowFilter = $"VariantItemId={variantItemId}";
                    gridVariantItemMarks.ItemsSource = dataView;
                }
            }
        }

        private void GridVariantItems_AfterCreateNewRow(object sender, LiveGridControl.AfterCreateNewRowEventArgs e)
        {
        }

        private void VariantTypePm_Dispose_VariantItemMark(PMBase pm, PmParam parameter)
        {
            if (gridVariantItems != null)
            {
                gridVariantItems.AfterCreateNewRow -= GridVariantItems_AfterCreateNewRow;
                gridVariantItems.CurrentItemChanged -= GridVariantItems_CurrentItemChanged;
            }
            if (gridVariantItemMarks != null)
            {
                gridVariantItemMarks.BeforeCreateNewRow -= GridVariantItemMarks_BeforeCreateNewRow;
                gridVariantItemMarks.AfterCreateNewRow -= GridVariantItemMarks_AfterCreateNewRow;
            }
        }
    }
}
