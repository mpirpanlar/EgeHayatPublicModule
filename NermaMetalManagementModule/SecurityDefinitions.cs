using Sentez.Common.ModuleBase;
using Sentez.Common.Security;
using Sentez.Localization;

namespace Sentez.NermaMetalManagementModule
{
    class NermaMetalManagementModuleSecurity
    {
        public static void RegisterSecurityDefinitions()
        {
            short _moduleId = (short)Modules.ExternalModule15;

            SecurityDefinition mainSecurity = new SecurityDefinition(SLanguage.GetString("Maliyet Kontrol Modülü"), _moduleId, _moduleId, 0, 0, Privileges.Select);
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Satış-Sevkiyat Karşılaştırması"), _moduleId, _moduleId, (short)NermaMetalManagementModuleSecurityItems.VariantItemMark, (short)NermaMetalManagementModuleSecuritySubItems.None, Privileges.Select));
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Hata Kontrol Mekanizması"), _moduleId, _moduleId, (short)NermaMetalManagementModuleSecurityItems.InventoryMark, (short)NermaMetalManagementModuleSecuritySubItems.None, Privileges.Select));
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Hata Görev Kontrolü"), _moduleId, _moduleId, (short)NermaMetalManagementModuleSecurityItems.FaultTaskControl, (short)NermaMetalManagementModuleSecuritySubItems.None, Privileges.All));
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Aylık Gerçek Maliyet"), _moduleId, _moduleId, (short)NermaMetalManagementModuleSecurityItems.MonthlyActualCost, (short)NermaMetalManagementModuleSecuritySubItems.None, Privileges.All));
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Order Tarihçesi"), _moduleId, _moduleId, (short)NermaMetalManagementModuleSecurityItems.OrderAllHistory, (short)NermaMetalManagementModuleSecuritySubItems.None, Privileges.All));

            PrivilegeInfo.SecurityDefinitions.AddDefinition(mainSecurity);
        }
    }

}
