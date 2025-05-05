using Sentez.Common.ModuleBase;
using Sentez.Common.Security;
using Sentez.Localization;

namespace Sentez.EgeHayatVehicleFleetManagementModule
{
    class EgeHayatVehicleFleetManagementModuleSecurity
    {
        public static void RegisterSecurityDefinitions()
        {
            short _moduleId = (short)Modules.ExternalModule15;

            SecurityDefinition mainSecurity = new SecurityDefinition(SLanguage.GetString("Maliyet Kontrol Modülü"), _moduleId, _moduleId, 0, 0, Privileges.Select);
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Satış-Sevkiyat Karşılaştırması"), _moduleId, _moduleId, (short)EgeHayatVehicleFleetManagementModuleSecurityItems.VariantItemMark, (short)EgeHayatVehicleFleetManagementModuleSecuritySubItems.None, Privileges.Select));
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Hata Kontrol Mekanizması"), _moduleId, _moduleId, (short)EgeHayatVehicleFleetManagementModuleSecurityItems.InventoryMark, (short)EgeHayatVehicleFleetManagementModuleSecuritySubItems.None, Privileges.Select));
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Hata Görev Kontrolü"), _moduleId, _moduleId, (short)EgeHayatVehicleFleetManagementModuleSecurityItems.FaultTaskControl, (short)EgeHayatVehicleFleetManagementModuleSecuritySubItems.None, Privileges.All));
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Aylık Gerçek Maliyet"), _moduleId, _moduleId, (short)EgeHayatVehicleFleetManagementModuleSecurityItems.MonthlyActualCost, (short)EgeHayatVehicleFleetManagementModuleSecuritySubItems.None, Privileges.All));
            mainSecurity.AddChild(new SecurityDefinition(SLanguage.GetString("Order Tarihçesi"), _moduleId, _moduleId, (short)EgeHayatVehicleFleetManagementModuleSecurityItems.OrderAllHistory, (short)EgeHayatVehicleFleetManagementModuleSecuritySubItems.None, Privileges.All));

            PrivilegeInfo.SecurityDefinitions.AddDefinition(mainSecurity);
        }
    }

}
