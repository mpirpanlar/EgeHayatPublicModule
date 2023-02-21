using LiveCore.Desktop.UI.Controls;

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
using System.Linq;
using System.Text;
using System.Windows.Controls;

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
            if(inventoryPm.ActiveBO!=null)
                inventoryPm.ActiveBO.AfterGet += ActiveBO_AfterGet;
        }

        private void ActiveBO_AfterGet(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void InventoryBo_Init_InventoryUnitItemSizeSetDetails(BusinessObjectBase bo, BoParam parameter)
        {
            bo.ValueFiller.AddRule("Erp_InventoryUnitItemSizeSetDetails", "InUse", 1);
            bo.AfterGet += Bo_AfterGet;
        }

        private void Bo_AfterGet(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}
