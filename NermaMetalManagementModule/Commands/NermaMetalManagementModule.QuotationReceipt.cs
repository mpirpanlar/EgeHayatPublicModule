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
using Sentez.QuotationModule.PresentationModels;

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
        private void QuotationReceiptBo_Init_InventoryUnitItemSizeSetDetails(BusinessObjectBase bo, BoParam parameter)
        {
            bo.Lookups.AddLookUp("Erp_QuotationReceiptItem", "UnitItemSizeSetDetailsId", true, "Erp_UnitItemSizeSetDetails", string.Empty, string.Empty, new string[] { "SizeDetailCode", "SizeDetailName", "UnitItemId", "UnitFactor", "UnitDivisor", "UnitWidth", "UnitLength", "UnitHeight" }
            , new string[] { "UnitItemSizeSetDetails_SizeDetailCode", "UnitItemSizeSetDetails_SizeDetailName", "UnitItemSizeSetDetails_UnitItemId", "UnitItemSizeSetDetails_UnitFactor", "UnitItemSizeSetDetails_UnitDivisor", "UnitItemSizeSetDetails_UnitWidth", "UnitItemSizeSetDetails_UnitLength", "UnitItemSizeSetDetails_UnitHeight" });
        }

        private void QuotationReceiptPm_Init_InventoryUnitItemSizeSetDetails(PMBase pm, PmParam parameter)
        {
            quotationReceiptPm = pm as QuotationReceiptPM;
            if (quotationReceiptPm == null)
            {
                return;
            }
        }
    }
}
