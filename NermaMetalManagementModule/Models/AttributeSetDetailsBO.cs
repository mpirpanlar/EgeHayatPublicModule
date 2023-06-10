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
    public class AttributeSetDetailsBO : BusinessObjectBase
    {
        
        public AttributeSetDetailsBO(IContainerExtension container)
            : base(container, 0, "AttributeSetCode", string.Empty, new string[] { "Erp_AttributeSetDetails" })
        {
            KeyFields.Add(new WhereField("Erp_AttributeSetDetails", "CompanyId", _companyId, WhereCondition.Equal));
            KeyFields.Add(WhereField.GetIsDeletedRule("Erp_AttributeSetDetails"));

            ValueFiller.AddRule("Erp_AttributeSetDetails", "InUse", 1);
            ValueFiller.AddRule("Erp_AttributeSetDetails", "IsDeleted", 0);
            SecurityChecker.LogicalModuleID = (short)Modules.ExternalModule15;
        }
    }
}
