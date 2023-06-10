using LiveCore.Desktop.Common;
using LiveCore.Desktop.UI.Controls;

using Prism.Ioc;

using Sentez.Common.ModuleBase;
using Sentez.Common.PresentationModels;
using Sentez.Common.Utilities;
using Sentez.InventoryModule.PresentationModels;
using Sentez.QuotationModule.PresentationModels;

namespace Sentez.NermaMetalManagementModule
{
    public partial class NermaMetalManagementModule : LiveModule
	{
		public LookupList Lists { get; set; }
        public LookupList Lists_QuotationReceiptPM { get; set; }
        LiveDocumentPanel ldpInventoryUnitItemSizeSetDetails, ldpInventoryMark, ldpVariantItemMark;
		LiveTabItem ldpCategoryUnitItemSizeSetDetails, ldpCategoryAttributeSetDetails;
        InventoryPM inventoryPm;
		CardPM categoryPm, inventoryAttributeSetPm;
        VariantTypePM variantTypePm;
		LiveGridControl gridVariantItems, gridVariantItemMarks;
        QuotationReceiptPM quotationReceiptPm;

        bool _suppressEvent = false;
    }
}
