namespace Sentez.NermaReservationManagementModule
{
    public enum MenuSubRoots : short
    {
        Descriptions = 1000, 
        Transactions, 
        Operations, 
        Reports, 
        Settings 
    }
    public enum NermaReservationManagementModuleSecurityItems : short
    {
        None,
        VariantItemMark,
        InventoryMark,
        FaultTaskControl,
        MonthlyActualCost,
        OrderAllHistory
    }
    public enum NermaReservationManagementModuleSecuritySubItems : short
    {
        None
    }
}
