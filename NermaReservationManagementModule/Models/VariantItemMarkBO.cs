using Prism.Ioc;

using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.Security;
using Sentez.Data.BusinessObjects;
using Sentez.Data.Query;
using Sentez.NermaReservationManagementModule;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NermaReservationManagementModule.Models
{
    [BusinessObjectExplanation("Varyant Kartı Marka Bağlantıları")]
    [SecurityModuleId((short)Modules.ExternalModule15)]
    [SecurityItemId((short)NermaReservationManagementModuleSecurityItems.VariantItemMark)]
    public class VariantItemMarkBO : BusinessObjectBase
    {
        
        public VariantItemMarkBO(IContainerExtension container)
            : base(container, 0, "GroupCode", string.Empty, new string[] { "Erp_CurrentAccountGroup" })
        {
            KeyFields.Add(new WhereField("Erp_CurrentAccountGroup", "CompanyId", _companyId, WhereCondition.Equal));

            Lookups.AddLookUp("Erp_VariantItemMark", "VariantItemId", true, "Erp_VariantItem", "ItemCode", "ItemCode", "ItemName", "ItemName");
            Lookups.AddLookUp("Erp_VariantItemMark", "MarkId", true, "Erp_Mark", "MarkName", "MarkName", "Explanation", "MarkExplanation");
            
            ValueFiller.AddRule("Erp_VariantItemMark", "InUse", 1);

            SecurityChecker.LogicalModuleID = (short)Modules.ExternalModule15;
        }
    }
}
