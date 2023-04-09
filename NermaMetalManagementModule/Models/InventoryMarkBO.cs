using Microsoft.Practices.Unity;

using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.Security;
using Sentez.Data.BusinessObjects;
using Sentez.Data.Query;
using Sentez.NermaMetalManagementModule;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NermaMetalManagementModule.Models
{
    [BusinessObjectExplanation("Malzeme Kartı Marka Bağlantıları")]
    [SecurityModuleId((short)Modules.ExternalModule15)]
    [SecurityItemId((short)NermaMetalManagementModuleSecurityItems.VariantItemMark)]
    public class InventoryMarkBO : BusinessObjectBase
    {
        [InjectionConstructor()]
        public InventoryMarkBO(IUnityContainer container)
            : base(container, 0, "RecId", string.Empty, new string[] { "Erp_InventoryMark" })
        {
            Lookups.AddLookUp("Erp_InventoryMark", "VariantItemId", true, "Erp_Inventory", "InventoryCode", "InventoryCode", "InventoryName", "InventoryName");
            Lookups.AddLookUp("Erp_InventoryMark", "MarkId", true, "Erp_Mark", "MarkName", "MarkName", "Explanation", "MarkExplanation");
            
            ValueFiller.AddRule("Erp_InventoryMark", "InUse", 1);

            SecurityChecker.LogicalModuleID = (short)Modules.ExternalModule15;
        }
    }
}
