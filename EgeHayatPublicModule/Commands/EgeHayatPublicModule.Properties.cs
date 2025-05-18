using LiveCore.Desktop.Common;
using LiveCore.Desktop.UI.Controls;

using Prism.Ioc;

using Sentez.Common.ModuleBase;
using Sentez.Common.PresentationModels;
using Sentez.Common.Utilities;
using Sentez.CRMModule.PresentationModels;
using Sentez.Finance.PresentationModels;
using Sentez.InventoryModule.PresentationModels;
using Sentez.OrderModule.PresentationModels;
using Sentez.QuotationModule.PresentationModels;

namespace Sentez.EgeHayatPublicModule
{
    public partial class EgeHayatPublicModule : LiveModule
    {
        CurrentAccountPM currentAccountPm;
        IContainerExtension _container;
        public LookupList Lists { get; set; }
        public LookupList Lists_QuotationReceiptPM { get; set; }
        public LookupList Lists_OrderReceiptPM { get; set; }
        LiveDocumentPanel ldpCurrentAccountChecklist, ldpInventoryMark, ldpVariantItemMark;
        LiveTabItem ldpCategoryUnitItemSizeSetDetails, ldpCategoryAttributeSetDetails, ltiCurrentAccountChecklist;
        InventoryPM inventoryPm;
        CardPM categoryPm, inventoryAttributeSetPm;
        VariantTypePM variantTypePm;
        LiveGridControl gridVariantItems, gridVariantItemMarks;
        QuotationReceiptPM quotationReceiptPm;
        OrderReceiptPM orderReceiptPm;
        CRMCustomerTransactionPM crmCustomerTransactionPm;
        bool _suppressEvent = false;
    }
}
