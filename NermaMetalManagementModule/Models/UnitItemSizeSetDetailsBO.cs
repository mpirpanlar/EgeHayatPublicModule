using Sentez.Data.BusinessObjects;
using Prism.Ioc;
using Sentez.Data.Query;
using Sentez.Common.ModuleBase;
using Sentez.Common.Security;

namespace Sentez.NermaMetalManagementModule.Models
{
    [BusinessObjectExplanation("Ölçü Kodları")]
    [SecurityModuleId((short)Modules.ExternalModule15)]
    //[SecurityItemId((short)RbKaresiModuleSecurityItems.CurrentAccountAnalysis)]
    public class UnitItemSizeSetDetailsBO : BusinessObjectBase
    {
        
        public UnitItemSizeSetDetailsBO(IContainerExtension container)
            : base(container, 0, "SizeDetailCode", string.Empty, new string[] { "Erp_UnitItemSizeSetDetails" })
        {
            KeyFields.Add(new WhereField("Erp_UnitItemSizeSetDetails", "CompanyId", _companyId, WhereCondition.Equal));
            KeyFields.Add(WhereField.GetIsDeletedRule("Erp_UnitItemSizeSetDetails"));

            ValueFiller.AddRule("Erp_UnitItemSizeSetDetails", "InUse", 1);
            ValueFiller.AddRule("Erp_UnitItemSizeSetDetails", "IsDeleted", 0);
            SecurityChecker.LogicalModuleID = (short)Modules.ExternalModule15;
        }
    }
}
