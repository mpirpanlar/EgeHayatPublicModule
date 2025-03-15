using Sentez.Common.ModuleBase;
using Sentez.Common.Security;
using Sentez.Localization;

namespace Sentez.EgeHayatDoorManagementModule
{
    class EgeHayatDoorManagementModuleSecurity
    {
        public static void RegisterSecurityDefinitions()
        {
            short _moduleId = (short)Modules.ExternalModule15;

            SecurityDefinition mainSecurity = new SecurityDefinition(SLanguage.GetString("Maliyet Kontrol Modülü"), _moduleId, _moduleId, 0, 0, Privileges.Select);
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Satış-Sevkiyat Karşılaştırması"), _moduleId, _moduleId, (short)EgeHayatDoorManagementModuleSecurityItems.VariantItemMark, (short)EgeHayatDoorManagementModuleSecuritySubItems.None, Privileges.Select));
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Hata Kontrol Mekanizması"), _moduleId, _moduleId, (short)EgeHayatDoorManagementModuleSecurityItems.InventoryMark, (short)EgeHayatDoorManagementModuleSecuritySubItems.None, Privileges.Select));
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Hata Görev Kontrolü"), _moduleId, _moduleId, (short)EgeHayatDoorManagementModuleSecurityItems.FaultTaskControl, (short)EgeHayatDoorManagementModuleSecuritySubItems.None, Privileges.All));
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Aylık Gerçek Maliyet"), _moduleId, _moduleId, (short)EgeHayatDoorManagementModuleSecurityItems.MonthlyActualCost, (short)EgeHayatDoorManagementModuleSecuritySubItems.None, Privileges.All));
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Order Tarihçesi"), _moduleId, _moduleId, (short)EgeHayatDoorManagementModuleSecurityItems.OrderAllHistory, (short)EgeHayatDoorManagementModuleSecuritySubItems.None, Privileges.All));

            PrivilegeInfo.SecurityDefinitions.AddDefinition(mainSecurity);
        }
    }

}
