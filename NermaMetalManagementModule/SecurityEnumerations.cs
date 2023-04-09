namespace Sentez.NermaMetalManagementModule
{
    public enum MenuSubRoots : short
    {
        Descriptions = 1000, 
        Transactions, 
        Operations, 
        Reports, 
        Settings 
    }
    public enum NermaMetalManagementModuleSecurityItems : short
    {
        None,
        VariantItemMark,
        InventoryMark,
        FaultTaskControl,
        MonthlyActualCost,
        OrderAllHistory
    }
    public enum NermaMetalManagementModuleSecuritySubItems : short
    {
        None
    }
}
