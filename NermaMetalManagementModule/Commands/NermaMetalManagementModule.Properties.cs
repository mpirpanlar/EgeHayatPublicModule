using LiveCore.Desktop.UI.Controls;

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
		public LookupList Lists { get; set; }
        public LookupList Lists_QuotationReceiptPM { get; set; }
        LiveDocumentPanel ldpInventoryUnitItemSizeSetDetails, ldpInventoryMark, ldpVariantItemMark;
		InventoryPM inventoryPm;
		VariantTypePM variantTypePm;
		LiveTabItem ltiVariantItemMark;
		LiveGridControl gridVariantItems, gridVariantItemMarks;
        QuotationReceiptPM quotationReceiptPm;
		bool _suppressEvent = false;
	}
}
