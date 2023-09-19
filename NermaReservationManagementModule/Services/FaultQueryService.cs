using Prism.Ioc;

using Sentez.Common.Commands;
using Sentez.Common.SystemServices;
using Sentez.Data.Tools;
using System;
using System.Data;
using System.Text;

namespace NermaReservationManagementModule.Services
{
    public class FaultQueryService : SystemServiceBase
    {
        ISystemService createInventoryReceiptSubContractorService;
        public FaultQueryService(SysMng smgr, Guid sid) : base(smgr, sid)
        {
            createInventoryReceiptSubContractorService = smgr._container.Resolve<ISystemService>("CreateInventoryReceiptSubContractorService");
        }

        public override object Execute(object[] inputs)
        {
            if (inputs.Length == 3 && inputs[1] is int && inputs[2] is int)
                return FaultTable((string)inputs[0], (int?)inputs[1], (int?)inputs[2]);
            return null;
        }
        private DataTable FaultTable(string workOrderIds, int? DischargeMonth, int? DischargeYear)
        {
            DataTable subcontractorTypeTable = (DataTable)createInventoryReceiptSubContractorService.Execute();

            if (string.IsNullOrEmpty(workOrderIds))
                workOrderIds = "0";
            StringBuilder sb = new StringBuilder();
            # region 1,2,3,8,9,10,11,12,13,20,21,22 Nolu Fiyat Yok Hataları --OK 37 nolu hata hata tiplerine eklenecek kumaş stok olupta fason seçilmeyen fiyat yok hatası
            sb.AppendLine("select distinct wo.WorkOrderNo WorkOrderNo,r.WorkOrderId WorkOrderId,iri.RecId RecId,ir.ReceiptType ReceiptType,ir.ReceiptUpType ReceiptUpType,ir.ReceiptSubType ReceiptSubType");
            sb.AppendLine(",iri.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee with(nolock) where ee.RecId in (Select EmployeeId from Meta_User where RecId = iri.InsertedBy)) [EmployeeName]");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId = wo.CurrentAccountId) CurrentAccountName");
            sb.AppendLine(",wo.CurrentAccountId CurrentAccountId");
            sb.AppendLine(",0 SubContractorType");
            sb.AppendLine(",case when ir.ReceiptType=1 and ir.ReceiptUpType=3 then 1");
            sb.AppendLine("when ir.ReceiptType=122 and ir.ReceiptUpType=3 then 2");
            sb.AppendLine("when ir.ReceiptType=1 and ir.ReceiptUpType=1 then 8");
            sb.AppendLine("when ir.ReceiptType=122 and ir.ReceiptUpType=1 then 9");
            sb.AppendLine("when ir.ReceiptType=1 and ir.ReceiptUpType=2 then 20");
            sb.AppendLine("when ir.ReceiptType=122 and ir.ReceiptUpType=2 then 21");
            sb.AppendLine("when ir.ReceiptType=12 and ir.ReceiptUpType=1 then 11");
            sb.AppendLine("else 37 end FaultType");
            sb.AppendLine(",0 SentQuantity");
            sb.AppendLine(",0 ReceivedQuantity");
            sb.AppendLine("," + InventoryReceiptType.GetInventoryReceiptTypeSQLStr(0, "ir.ReceiptType", "ReceiptTypeName"));
            sb.AppendLine("," + InventoryReceiptUpType.GetInventoryReceiptUpTypeSQLStr(0, "ir.ReceiptUpType", "ReceiptUpTypeName"));
            sb.AppendLine("," + InventoryReceiptType.GetInventoryReceiptSubTypeSQLStr(0, "iri.ReceiptSubType", "ReceiptSubTypeName"));
            sb.AppendLine(",ir.ReceiptNo +' Nolu fişte fiyat yok. Malzeme: ' + isnull((Select InventoryCode from Erp_Inventory where RecId = iri.InventoryId),'')+' Renk :' +isnull((Select ItemCode + ' ' + ItemName from Erp_VariantItem where RecId = r.Variant1Id),'') [Explanation]");
            sb.AppendLine("from Erp_Requirement r with(nolock)");
            sb.AppendLine("left join Erp_InventoryAllocation ia with(nolock) on ia.RequirementId = r.RecId");
            sb.AppendLine("left join Erp_WorkOrder wo with(nolock) on wo.RecId = r.WorkOrderId");
            sb.AppendLine("left join Erp_InventoryReceiptItem iri with(nolock) on iri.RecId = ia.InventoryReceiptItemId");
            sb.AppendLine("left join Erp_InventoryReceipt ir with(nolock) on ir.RecId = iri.InventoryReceiptId");
            sb.AppendLine("where ia.InventoryReceiptType in(1, 11, 122 ,12)");
            sb.AppendLine($"and wo.RecId in ({workOrderIds})");
            sb.AppendLine("and ir.ReceiptUpType <> 0");
            sb.AppendLine($"and ir.CompanyId = {SysMng.Instance.getSession().ActiveCompany.RecId} ");
            sb.AppendLine("and iri.UnitPrice < 0.001 and iri.UnitPrice is not null");
            #endregion

            #region 4,14,23 Nolu Alım ile Tedarik Arasında %8 veya %3 Fark Olması Hataları
            sb.AppendLine("union all ");
            sb.AppendLine("select (Select WorkOrderNo from Erp_WorkOrder where RecId = r.WorkOrderId) WorkOrderNo,r.WorkOrderId WorkOrderId,r.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType");
            sb.AppendLine(",r.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee with(nolock) where ee.RecId in (Select EmployeeId from Meta_User where RecId = r.InsertedBy)) [EmployeeName]");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId in (Select CurrentAccountId from Erp_WorkOrder where RecId = r.WorkOrderId)) CurrentAccountName");
            sb.AppendLine(",(Select CurrentAccountId from Erp_WorkOrder where RecId = r.WorkOrderId) CurrentAccountId");
            sb.AppendLine(",0 SubContractorType");
            sb.AppendLine(",case when r.RequirementType=0 then 4 when r.RequirementType=2 then 23 else 14 end FaultyType");
            sb.AppendLine(",r.Quantity SentQuantity");
            sb.AppendLine(",((isnull((select sum(IA.Quantity) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (1,5,16,17,99) or (IA.InventoryReceiptType in (2,3)");
            sb.AppendLine("and (IA.ReturnType is null or IA.ReturnType=0)) or (IA.InventoryReceiptType=11 and IA.InventoryReceiptSubType=1)) and IA.RequirementId=r.RecId and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0)");
            sb.AppendLine(" + isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where not exists(select ia.RecId from Erp_InventoryAllocation ia with(nolock) where (isnull(ia.IsDeleted, 0) = 0 ");
            sb.AppendLine(" and isnull(ia.IsCancelled,0)= 0) and ia.InventoryReceiptItemId = IA.InventoryReceiptItemId and ia.InventoryReceiptType = 29 and ia.RequirementId = r.RecId  and(ia.ReturnType = 1))");
            sb.AppendLine("and IA.InventoryReceiptType = 29  and(IA.ReturnType is null or IA.ReturnType = 0) and IA.RequirementId = r.RecId and IsNull(IA.IsDeleted, 0) = 0 and IsNull(IA.IsCancelled, 0) = 0),0) )");
            sb.AppendLine("- isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (12,142) and IA.InventoryReceiptSubType=1 or IA.InventoryReceiptType in (122)) and IA.RequirementId=r.RecId and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0)) as ReceivedQuantity");
            sb.AppendLine(",'' ReceiptTypeName");
            sb.AppendLine(",'' ReceiptUpTypeName");
            sb.AppendLine(",'' ReceiptSubTypeName");
            sb.AppendLine(",case when r.RequirementType in (0,2) then 'İlgili malzemenin tedariği ile alımı arasında %8 oranının dışında bir miktar farkı var. Malzeme: ' + isnull((Select InventoryCode from Erp_Inventory where RecId = r.InventoryId),'') + ' Renk: ' + isnull((Select ItemCode + ' ' + ItemName from Erp_VariantItem where RecId = r.Variant1Id),'') ");
            sb.AppendLine("else 'İlgili malzemenin tedariği ile alımı arasında %3 oranının dışında bir miktar farkı var. Malzeme: ' + isnull((Select InventoryCode from Erp_Inventory where RecId = r.InventoryId),'') + ' Renk: ' + isnull((Select ItemCode + ' ' + ItemName from Erp_VariantItem where RecId = r.Variant1Id),'')  end [Explanation]");
            sb.AppendLine("from Erp_Requirement r with (nolock)");
            sb.AppendLine($"where r.WorkOrderId in ({workOrderIds})");
            sb.AppendLine(" and r.RequirementType in (0, 1, 2) and (case when r.RequirementType = 0 or r.RequirementType = 2 then 8 else 3 end) <= ");
            sb.AppendLine(" ABS(case when r.Quantity>0 then");
            sb.AppendLine("((isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (1,5,16,17,99) or (IA.InventoryReceiptType in (2,3) ");
            sb.AppendLine("and (IA.ReturnType is null or IA.ReturnType=0)) or (IA.InventoryReceiptType=11 and IA.InventoryReceiptSubType=1)) and IA.RequirementId=r.RecId and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0)");
            sb.AppendLine(" + isnull((select sum(IA.Quantity) from Erp_InventoryAllocation IA with (nolock) where not exists(select ia.RecId from Erp_InventoryAllocation ia with(nolock) where (isnull(ia.IsDeleted, 0) = 0 ");
            sb.AppendLine(" and isnull(ia.IsCancelled,0)= 0) and ia.InventoryReceiptItemId = IA.InventoryReceiptItemId and ia.InventoryReceiptType = 29 and ia.RequirementId = r.RecId  and(ia.ReturnType = 1)) ");
            sb.AppendLine(" and IA.InventoryReceiptType = 29  and(IA.ReturnType is null or IA.ReturnType = 0) and IA.RequirementId = r.RecId and IsNull(IA.IsDeleted, 0) = 0 and IsNull(IA.IsCancelled, 0) = 0),0) )");
            sb.AppendLine("- isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (12,142) and IA.InventoryReceiptSubType=1 or IA.InventoryReceiptType in (122)) and IA.RequirementId=r.RecId and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0)) * 100 / r.Quantity - 100 else 0 end)");
            #endregion

            #region 5,15,24 Nolu Tedarik Miktarının 0 Olması Hataları --OK
            sb.AppendLine("union all");
            sb.AppendLine("select (Select WorkOrderNo from Erp_WorkOrder where RecId = r.WorkOrderId) WorkOrderNo,r.WorkOrderId WorkOrderId,r.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType");
            sb.AppendLine(",r.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee with(nolock) where ee.RecId in (Select EmployeeId from Meta_User where RecId = r.InsertedBy)) [EmployeeName]");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId in (Select CurrentAccountId from Erp_WorkOrder where RecId = r.WorkOrderId)) CurrentAccountName");
            sb.AppendLine(",(Select CurrentAccountId from Erp_WorkOrder where RecId = r.WorkOrderId) CurrentAccountId");
            sb.AppendLine(",0 SubContractorType");
            sb.AppendLine(",case when r.RequirementType=0 then 5 when r.RequirementType=2 then 24 else 15 end FaultyType");
            sb.AppendLine(",0 SentQuantity");
            sb.AppendLine(" ,((isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (1,5,16,17,99) or (IA.InventoryReceiptType in (2,3) and (IA.ReturnType is null or IA.ReturnType=0)) or (IA.InventoryReceiptType=11 and IA.InventoryReceiptSubType=1)) and IA.RequirementId=r.RecId and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0) ");
            sb.AppendLine(" + isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where not exists(select ia.RecId from Erp_InventoryAllocation ia with(nolock) where (isnull(ia.IsDeleted, 0) = 0 and isnull(ia.IsCancelled,0)= 0) and ia.InventoryReceiptItemId = IA.InventoryReceiptItemId and ia.InventoryReceiptType = 29 and ia.RequirementId = r.RecId  and(ia.ReturnType = 1)) ");
            sb.AppendLine(" and IA.InventoryReceiptType = 29  and(IA.ReturnType is null or IA.ReturnType = 0) and IA.RequirementId = r.RecId and IsNull(IA.IsDeleted, 0) = 0 and IsNull(IA.IsCancelled, 0) = 0),0) ) - isnull((select sum(IA.Quantity) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (12,142) and IA.InventoryReceiptSubType=1 or IA.InventoryReceiptType in (122)) and IA.RequirementId=r.RecId and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0)) ReceivedQuantity "); 
            sb.AppendLine(",'' ReceiptTypeName");
            sb.AppendLine(",'' ReceiptUpTypeName");
            sb.AppendLine(",'' ReceiptSubTypeName");
            sb.AppendLine(",'Tedarik Miktarı Sıfır. Malzeme: ' + isnull((Select InventoryCode from Erp_Inventory where RecId = r.InventoryId),'') + ' Renk: ' + isnull((Select ItemCode + ' ' + ItemName from Erp_VariantItem where RecId = r.Variant1Id),'') [Explanation]");
            sb.AppendLine("from Erp_Requirement r with (nolock)  left join Erp_WorkOrder wo with (nolock) on wo.RecId = r.WorkOrderId ");
            sb.AppendLine($"where r.WorkOrderId in ({workOrderIds})");
            sb.AppendLine("and r.RequirementType in (0,1,2) and r.Quantity <= 0");
            #endregion

            #region 6,16,25 Nolu Boyaya Gidene Göre Daha Az/Hiç Gelen Olması Hataları
            sb.AppendLine("union all");
            sb.AppendLine("select (Select WorkOrderNo from Erp_WorkOrder where RecId = r.WorkOrderId) WorkOrderNo,r.WorkOrderId WorkOrderId,r.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType");
            sb.AppendLine(",r.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee with(nolock) where ee.RecId in (Select EmployeeId from Meta_User where RecId = r.InsertedBy)) [EmployeeName]");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId = wo.CurrentAccountId) CurrentAccountName");
            sb.AppendLine(",wo.CurrentAccountId CurrentAccountId");
            sb.AppendLine(",0 SubContractorType");
            sb.AppendLine(",case when r.RequirementType=0 then 6 when r.RequirementType=1 then 16 else 25 end FaultyType");
            sb.AppendLine(",(isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=2),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=2),0)) SentQuantity ");
            sb.AppendLine(",(isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=2),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=2),0)) ReceivedQuantity ");
            sb.AppendLine(",'' ReceiptTypeName");
            sb.AppendLine(",'' ReceiptUpTypeName");
            sb.AppendLine(",'' ReceiptSubTypeName");
            sb.AppendLine(",'Boyaya Gidene Göre Az/Hiç Geliş Yok ise. Malzeme: ' + isnull((Select InventoryCode from Erp_Inventory where RecId = r.InventoryId),'') + ' Renk: ' + isnull((Select ItemCode + ' ' + ItemName from Erp_VariantItem where RecId = r.Variant1Id),'') [Explanation]");
            sb.AppendLine("from Erp_Requirement r with (nolock) left join Erp_WorkOrder wo on wo.RecId = r.WorkOrderId");
            sb.AppendLine($"where r.WorkOrderId in ({workOrderIds})");
            sb.AppendLine("and r.RequirementType in (0,1,2) and ");
            sb.AppendLine("(((isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=2),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=2),0))>0");
            sb.AppendLine("and ");
            sb.AppendLine("(isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=2),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=2),0) = 0))");
            sb.AppendLine("or ");
            sb.AppendLine("((isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=2),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=2),0)) = 0");
            sb.AppendLine("and");
            sb.AppendLine("(isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=2),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=2),0) > 0))");
            sb.AppendLine("or");
            sb.AppendLine("((isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=2),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where  ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=2),0)) > 0");
            sb.AppendLine("and (isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=2),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where  ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=2),0))+1");
            sb.AppendLine(" <  (isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=2),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=2),0)))");
            sb.AppendLine("or");
            sb.AppendLine("((isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=2),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=2),0)) > 0");
            sb.AppendLine("and (isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=2),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where  ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=2),0))");
            sb.AppendLine(" >  (isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=2),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=2),0))+1))");
            #endregion

            #region 7,17,26 Nolu Tamire Gidene Göre Daha Az/Hiç Gelen Olması Hataları
            sb.AppendLine("union all");
            sb.AppendLine("select wo.WorkOrderNo WorkOrderNo,r.WorkOrderId WorkOrderId,r.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType");
            sb.AppendLine(",r.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee with(nolock) where ee.RecId in (Select EmployeeId from Meta_User where RecId = r.InsertedBy)) [EmployeeName]");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId = wo.CurrentAccountId) CurrentAccountName");
            sb.AppendLine(",wo.CurrentAccountId CurrentAccountId");
            sb.AppendLine(",0 SubContractorType");
            sb.AppendLine(",case when r.RequirementType=0 then 7 when r.RequirementType=1 then 17 else 26 end FaultyType");
            sb.AppendLine(",(isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=4),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=4),0)) SentQuantity ");
            sb.AppendLine(",(isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=4),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=4),0)) ReceivedQuantity ");
            sb.AppendLine(",'' ReceiptTypeName");
            sb.AppendLine(",'' ReceiptUpTypeName");
            sb.AppendLine(",'' ReceiptSubTypeName");
            sb.AppendLine(",'Tamire Gidene Göre Az/Hiç Geliş Yok ise. Malzeme: ' + isnull((Select InventoryCode from Erp_Inventory where RecId = r.InventoryId),'') + ' Renk: ' + isnull((Select ItemCode + ' ' + ItemName from Erp_VariantItem where RecId = r.Variant1Id),'') [Explanation]");
            sb.AppendLine("from Erp_Requirement r with (nolock) left join Erp_WorkOrder wo on wo.RecId = r.WorkOrderId");
            sb.AppendLine($"where r.WorkOrderId in ({workOrderIds})");
            sb.AppendLine("and r.RequirementType in (0,1,2) and");
            sb.AppendLine("(((isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=4),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=4),0))>0");
            sb.AppendLine("and ");
            sb.AppendLine("(isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=4),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=4),0) = 0))");
            sb.AppendLine("or ");
            sb.AppendLine("((isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=4),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=4),0)) = 0");
            sb.AppendLine("and");
            sb.AppendLine("(isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=4),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=4),0) > 0))");
            sb.AppendLine("or");
            sb.AppendLine("((isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=4),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where  ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=4),0)) > 0");
            sb.AppendLine("and (isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=4),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where  ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=4),0))+1");
            sb.AppendLine(" <  (isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=4),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=4),0)))");
            sb.AppendLine("or");
            sb.AppendLine("((isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=4),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=4),0)) > 0");
            sb.AppendLine("and (isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=4),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where  ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=4),0))");
            sb.AppendLine(" > (isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=4),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=4),0))+1))");
            #endregion

            #region 18-Kumaş Baskıya Gidene Göre Daha Az/Hiç Gelen Olması Hatası
            sb.AppendLine("union all");
            sb.AppendLine("select (Select WorkOrderNo from Erp_WorkOrder where RecId = r.WorkOrderId) WorkOrderNo,r.WorkOrderId WorkOrderId,r.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType");
            sb.AppendLine(",r.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee with(nolock) where ee.RecId in (Select EmployeeId from Meta_User where RecId = r.InsertedBy)) [EmployeeName]");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId = wo.CurrentAccountId) CurrentAccountName");
            sb.AppendLine(",wo.CurrentAccountId CurrentAccountId");
            sb.AppendLine(",0 SubContractorType");
            sb.AppendLine(",18 FaultyType");
            sb.AppendLine(",(isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=3),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=3),0)) SentQuantity ");
            sb.AppendLine(",(isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=3),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=3),0)) ReceivedQuantity ");
            sb.AppendLine(",'' ReceiptTypeName");
            sb.AppendLine(",'' ReceiptUpTypeName");
            sb.AppendLine(",'' ReceiptSubTypeName");
            sb.AppendLine(",'Baskıya Gidene Göre Az/Hiç Geliş Yok ise. Malzeme: ' + isnull((Select InventoryCode from Erp_Inventory where RecId = r.InventoryId),'') + ' Renk: ' + isnull((Select ItemCode + ' ' + ItemName from Erp_VariantItem where RecId = r.Variant1Id),'') [Explanation]");
            sb.AppendLine("from Erp_Requirement r with (nolock) left join Erp_WorkOrder wo on wo.RecId = r.WorkOrderId");
            sb.AppendLine($"where r.WorkOrderId in ({workOrderIds})");
            sb.AppendLine("and r.RequirementType in (1) and");
            sb.AppendLine("(((isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=3),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=3),0))>0");
            sb.AppendLine("and ");
            sb.AppendLine("(isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=3),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=3),0) = 0))");
            sb.AppendLine("or ");
            sb.AppendLine("((isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=3),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=3),0)) = 0");
            sb.AppendLine("and");
            sb.AppendLine("(isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=3),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=3),0) > 0))");
            sb.AppendLine("or");
            sb.AppendLine("((isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=3),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where  ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=3),0)) > 0");
            sb.AppendLine("and (isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=3),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where  ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=3),0))+1");
            sb.AppendLine(" <  (isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=3),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=3),0)))");
            sb.AppendLine("or");
            sb.AppendLine("((isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=3),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=3),0)) > 0");
            sb.AppendLine("and (isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =134 and ia.InventoryReceiptSubType=3),0) -isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where  ia.RequirementId=r.RecId and ia.InventoryReceiptType =12 and ia.InventoryReceiptSubType=3),0))");
            sb.AppendLine(" > (isnull((select sum(isnull(ia.GrossQuantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =11 and ia.InventoryReceiptSubType=3),0)-isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia where ia.RequirementId=r.RecId and ia.InventoryReceiptType =142 and ia.InventoryReceiptSubType=3),0))+1))");
            #endregion

            #region 19-Kumaşa Baskı Öngörülmüş(Reçete) ama İşlem Yok Hatası
            sb.AppendLine("union all Select distinct");
            sb.AppendLine(" wo.WorkOrderNo WorkOrderNo,woi.WorkOrderId WorkOrderId,wo.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType");
            sb.AppendLine(",woi.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee with(nolock) where ee.RecId in (Select EmployeeId from Meta_User where RecId = woi.InsertedBy)) [EmployeeName]");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId = wo.CurrentAccountId) CurrentAccountName");
            sb.AppendLine(",wo.CurrentAccountId CurrentAccountId");
            sb.AppendLine(",0 SubContractorType");
            sb.AppendLine(",19 FaultType");
            sb.AppendLine(",isnull((select sum(ia.Quantity) from Erp_InventoryAllocation ia with (nolock) where ia.InventoryReceiptType=134 and ia.InventoryReceiptSubType=3 and ia.RequirementId = r.RecId),0) SentQuantity");
            sb.AppendLine(",0 ReceivedQuantity");
            sb.AppendLine(",'' ReceiptTypeName");
            sb.AppendLine(",'' ReceiptUpTypeName");
            sb.AppendLine(",'' ReceiptSubTypeName");
            sb.AppendLine(",'Kumaşa Baskı Öngörülmüş(Reçete) ama İşlem Yok. Malzeme: ' + isnull((Select InventoryCode from Erp_Inventory where RecId = ri.InventoryId),'') + ' '  [Explanation]");
            sb.AppendLine("FROM Erp_WorkOrderItem woi  with (nolock)");
            sb.AppendLine("left join Erp_WorkOrder wo with (nolock) on wo.RecId=woi.WorkOrderId");
            sb.AppendLine("left join Erp_Requirement r with (nolock) on r.WorkOrderId=woi.WorkOrderId and r.StyleId=woi.InventoryId and r.RequirementType=1");
            sb.AppendLine("left join Erp_RecipeItem ri with (nolock) on ri.RecipeType=1 and ri.OwnerInventoryId=r.StyleId");
            sb.AppendLine("where ri.OperationType in (2,3)   -- işlem  baskı olanlar");
            sb.AppendLine("and isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia with (nolock) where ia.InventoryReceiptType=134 and ia.InventoryReceiptSubType=3 and ia.RequirementId = r.RecId),0) <=0");
            sb.AppendLine($"and woi.WorkOrderId in ({workOrderIds})");
            #endregion

            #region 27-Orderda Hiç İplik/Kumaş/Aksesuar Kullanılmamış --OK
            sb.AppendLine("union all Select distinct");
            sb.AppendLine(" (Select WorkOrderNo from Erp_WorkOrder where RecId = r.WorkOrderId) WorkOrderNo,r.WorkOrderId WorkOrderId,r.WorkOrderId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType");
            sb.AppendLine(",r.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee where ee.RecId in (Select EmployeeId from Meta_User where RecId = r.InsertedBy)) [EmployeeName]");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId in (Select CurrentAccountId from Erp_WorkOrder where RecId = r.WorkOrderId)) CurrentAccountName");
            sb.AppendLine(",(Select CurrentAccountId from Erp_WorkOrder where RecId = r.WorkOrderId) CurrentAccountId");
            sb.AppendLine(",0 SubContractorType");
            sb.AppendLine(",27 FaultType");
            sb.AppendLine(",0 SentQuantity");
            sb.AppendLine(",(select isnull(sum(ia.Quantity),0) from Erp_InventoryAllocation ia where ia.RequirementId = r.RecId) ReceivedQuantity");
            sb.AppendLine(",'' ReceiptTypeName");
            sb.AppendLine(",'' ReceiptUpTypeName");
            sb.AppendLine(",'' ReceiptSubTypeName");
            sb.AppendLine(",'Orderda hiç '+Case r.RequirementType when 0 then 'İplik' when 1 then 'Kumaş' when '2' then 'Aksesuar'else ''end +' kullanılmamış Malzeme: '+isnull((Select InventoryCode from Erp_Inventory where RecId = r.InventoryId),'')+' TedarikM: '+Convert(Varchar,cast(r.Quantity as decimal(20,2))) [Explanation] ");
            sb.AppendLine("from Erp_Requirement r with(nolock)");
            sb.AppendLine("where (select count(*) from Erp_InventoryAllocation ia where ia.RequirementId = r.RecId)=0");
            sb.AppendLine($"and r.Quantity > 0 and r.WorkOrderId in ({workOrderIds})");
            #endregion

            #region 28-Örülen Kumaş Var, Fakat İplik Tahsisi Yok 
            sb.AppendLine("union all Select distinct ");
            sb.AppendLine(" (Select WorkOrderNo from Erp_WorkOrder where RecId = wo.RecId) WorkOrderNo,wo.RecId WorkOrderId,wo.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType ");
            sb.AppendLine(" ,0 EmployeeId,'' [EmployeeName] ");
            sb.AppendLine(" ,(Select CurrentAccountName from Erp_CurrentAccount where RecId in (wo.CurrentAccountId)) CurrentAccountName ");
            sb.AppendLine(" ,wo.CurrentAccountId CurrentAccountId ");
            sb.AppendLine(" ,0 SubContractorType ");
            sb.AppendLine(" ,28 FaultType ");
            sb.AppendLine(" ,(Select isnull(sum(ia.Quantity),0) from Erp_InventoryAllocation ia where ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=1) and ia.InventoryReceiptSubType=1 and ia.InventoryReceiptType=11)");
            sb.AppendLine(" -(Select isnull(sum(ia.Quantity),0) from Erp_InventoryAllocation ia where ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=1) and ia.InventoryReceiptSubType=1 and ia.InventoryReceiptType=142) SentQuantity ");
            sb.AppendLine(" ,((isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (1,5,16,17,99) ");
            sb.AppendLine(" or (IA.InventoryReceiptType in (2,3) and (IA.ReturnType is null or IA.ReturnType=0)) or (IA.InventoryReceiptType=11 and IA.InventoryReceiptSubType=1)) ");
            sb.AppendLine(" and IA.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=0) and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0) ");
            sb.AppendLine(" + isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where not exists(select ia.RecId from Erp_InventoryAllocation ia with(nolock) where (isnull(ia.IsDeleted, 0) = 0 ");
            sb.AppendLine(" and isnull(ia.IsCancelled,0)= 0) and ia.InventoryReceiptItemId = IA.InventoryReceiptItemId and ia.InventoryReceiptType = 29 and ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=0)  and(ia.ReturnType = 1)) ");
            sb.AppendLine(" and IA.InventoryReceiptType = 29  and(IA.ReturnType is null or IA.ReturnType = 0) and IA.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=0) and IsNull(IA.IsDeleted, 0) = 0 and IsNull(IA.IsCancelled, 0) = 0),0)) ");
            sb.AppendLine(" - isnull((select sum(IA.Quantity) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (12,142) and IA.InventoryReceiptSubType=1 or IA.InventoryReceiptType in (122)) ");
            sb.AppendLine(" and IA.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=0) and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0)) ReceivedQuantity ");
            sb.AppendLine(" ,'' ReceiptTypeName ");
            sb.AppendLine(" ,'' ReceiptUpTypeName ");
            sb.AppendLine(" ,'' ReceiptSubTypeName ");
            sb.AppendLine(" ,'Örülen kumaş var,İplik Tahsisi Yok '[Explanation]");
            sb.AppendLine(" from Erp_WorkOrder  wo with(nolock)");
            sb.AppendLine(" where  ");
            sb.AppendLine(" (Select isnull(sum(ia.Quantity),0) from Erp_InventoryAllocation ia where ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=1) and ia.InventoryReceiptSubType=1 and ia.InventoryReceiptType=11)");
            sb.AppendLine(" -(Select isnull(sum(ia.Quantity),0) from Erp_InventoryAllocation ia where ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=1) and ia.InventoryReceiptSubType=1 and ia.InventoryReceiptType=142) > 0  ");
            sb.AppendLine(" and");
            sb.AppendLine(" ((isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (1,5,16,17,99) ");
            sb.AppendLine(" or (IA.InventoryReceiptType in (2,3) and (IA.ReturnType is null or IA.ReturnType=0)) or (IA.InventoryReceiptType=11 and IA.InventoryReceiptSubType=1)) ");
            sb.AppendLine(" and IA.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=0) and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0) ");
            sb.AppendLine(" + isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where not exists(select ia.RecId from Erp_InventoryAllocation ia with(nolock) where (isnull(ia.IsDeleted, 0) = 0 ");
            sb.AppendLine(" and isnull(ia.IsCancelled,0)= 0) and ia.InventoryReceiptItemId = IA.InventoryReceiptItemId and ia.InventoryReceiptType = 29 and ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=0)  and(ia.ReturnType = 1)) ");
            sb.AppendLine(" and IA.InventoryReceiptType = 29  and(IA.ReturnType is null or IA.ReturnType = 0) and IA.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=0) and IsNull(IA.IsDeleted, 0) = 0 and IsNull(IA.IsCancelled, 0) = 0),0)) ");
            sb.AppendLine(" - isnull((select sum(IA.Quantity) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (12,142) and IA.InventoryReceiptSubType=1 or IA.InventoryReceiptType in (122)) ");
            sb.AppendLine(" and IA.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=0) and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0)) <= 0 ");
            sb.AppendLine(" and ");
            sb.AppendLine($" wo.RecId in ({workOrderIds})");
            sb.AppendLine(" group by wo.RecId ,wo.CurrentAccountId");
            #endregion

            #region 29-Kumaşın İpliğe oranı %5-%3 daha az
            sb.AppendLine("union all ");
            sb.AppendLine("Select * from (Select distinct");
            sb.AppendLine("(Select WorkOrderNo from Erp_WorkOrder where RecId = wo.RecId) WorkOrderNo,wo.RecId WorkOrderId,wo.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType");
            sb.AppendLine(",wo.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee where ee.RecId in (Select EmployeeId from Meta_User where RecId = wo.InsertedBy)) [EmployeeName]");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId in (wo.CurrentAccountId)) CurrentAccountName");
            sb.AppendLine(",wo.CurrentAccountId  CurrentAccountId");
            sb.AppendLine(",0 SubContractorType");
            sb.AppendLine(",29 FaultType");
            sb.AppendLine(",(Select isnull(sum(ia.Quantity),0) from Erp_InventoryAllocation ia where ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=0) and ia.InventoryReceiptSubType=1 and ia.InventoryReceiptType=134)");
            sb.AppendLine(" -(Select isnull(sum(ia.Quantity),0) from Erp_InventoryAllocation ia where ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=0) and ia.InventoryReceiptSubType=1 and ia.InventoryReceiptType=12) SentQuantity");
            sb.AppendLine(" ,(Select isnull(sum(ia.Quantity),0) from Erp_InventoryAllocation ia where ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=1) and ia.InventoryReceiptSubType=1 and ia.InventoryReceiptType=11)");
            sb.AppendLine(" -(Select isnull(sum(ia.Quantity),0) from Erp_InventoryAllocation ia where ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=1) and ia.InventoryReceiptSubType=1 and ia.InventoryReceiptType=142)ReceivedQuantity ");
            sb.AppendLine(",'' ReceiptTypeName");
            sb.AppendLine(",'' ReceiptUpTypeName");
            sb.AppendLine(",'' ReceiptSubTypeName");
            sb.AppendLine(",'Kumaşın Girişin İplik Fasona Çıkışa oranı %5-%3 dışında TİP:'+convert(varchar,(Select isnull(sum(cast(ia.Quantity as decimal(30,2))),0) from Erp_InventoryAllocation ia where ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=0) and ia.InventoryReceiptSubType=1 and ia.InventoryReceiptType=134)");
            sb.AppendLine(" -(Select isnull(sum(cast(ia.Quantity as decimal(30,2))),0) from Erp_InventoryAllocation ia where ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=0) and ia.InventoryReceiptSubType=1 and ia.InventoryReceiptType=12))");
            sb.AppendLine(" +' TKUM:'+ convert(varchar,(Select isnull(sum(cast(ia.Quantity as decimal(30,2))),0) from Erp_InventoryAllocation ia where ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=1) and ia.InventoryReceiptSubType=1 and ia.InventoryReceiptType=11)");
            sb.AppendLine(" -(Select isnull(sum(cast(ia.Quantity as decimal(30,2))),0) from Erp_InventoryAllocation ia where ia.RequirementId in (Select RecId from Erp_Requirement r where r.WorkOrderId=wo.RecId and r.RequirementType=1) and ia.InventoryReceiptSubType=1 and ia.InventoryReceiptType=142))+''[Explanation] ");
            sb.AppendLine("FROM  Erp_WorkOrder wo with (nolock) where ");
            sb.AppendLine($"wo.RecId in ({workOrderIds})) A where ");
            sb.AppendLine("A.SentQuantity > 0 and( A.ReceivedQuantity > A.SentQuantity or (100*(ABS(A.SentQuantity-A.ReceivedQuantity)/case A.SentQuantity when null then 1 when 0 then 1 else A.SentQuantity end)) not between 3 and 5)");
            #endregion

            #region 30-Örülen+Alınan Kumaş Ham Kumaş > Boyaya Giden
            sb.AppendLine("union all ");
            sb.AppendLine("select *,'Örülen Kumaş+Alınan :'+convert(nvarchar,Cast(a.ReceivedQuantity as decimal(15,2)))+' Boyaya Giden :'+convert(nvarchar,Cast(a.SentQuantity as decimal(15,2))) [Explanation] ");
            sb.AppendLine("from(Select distinct (Select WorkOrderNo from Erp_WorkOrder where RecId = r.WorkOrderId) WorkOrderNo,r.WorkOrderId WorkOrderId,r.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType  ");
            sb.AppendLine(",r.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee where ee.RecId in (Select EmployeeId from Meta_User where RecId = r.InsertedBy)) [EmployeeName]  ");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId in (wo.CurrentAccountId)) CurrentAccountName  ");
            sb.AppendLine(",wo.CurrentAccountId  CurrentAccountId  ");
            sb.AppendLine(",0 SubContractorType  ");
            sb.AppendLine(",30 FaultType  ");
            sb.AppendLine(",(isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia with (nolock) where ia.InventoryReceiptType=134 and InventoryReceiptSubType=2 and ia.RequirementId=r.RecId),0)-");
            sb.AppendLine("isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia with (nolock) where ia.InventoryReceiptType=12 and InventoryReceiptSubType=2 and ia.RequirementId=r.RecId),0)) SentQuantity ");
            sb.AppendLine(" ,((isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (1,5,16,17,99) ");
            sb.AppendLine(" or (IA.InventoryReceiptType in (2,3) and (IA.ReturnType is null or IA.ReturnType=0)) or (IA.InventoryReceiptType=11 and IA.InventoryReceiptSubType=1)) ");
            sb.AppendLine(" and IA.RequirementId=r.RecId and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0) ");
            sb.AppendLine(" + isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where not exists(select ia.RecId from Erp_InventoryAllocation ia with(nolock) where (isnull(ia.IsDeleted, 0) = 0 ");
            sb.AppendLine(" and isnull(ia.IsCancelled,0)= 0) and ia.InventoryReceiptItemId = IA.InventoryReceiptItemId and ia.InventoryReceiptType = 29 and ia.RequirementId = r.RecId  and(ia.ReturnType = 1)) ");
            sb.AppendLine(" and IA.InventoryReceiptType = 29  and(IA.ReturnType is null or IA.ReturnType = 0) and IA.RequirementId = r.RecId and IsNull(IA.IsDeleted, 0) = 0 and IsNull(IA.IsCancelled, 0) = 0),0)) ");
            sb.AppendLine(" - isnull((select sum(IA.Quantity) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (12,142) and IA.InventoryReceiptSubType=1 or IA.InventoryReceiptType in (122)) ");
            sb.AppendLine(" and IA.RequirementId=r.RecId and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0)) ReceivedQuantity ");
            sb.AppendLine(",'' ReceiptTypeName,'' ReceiptUpTypeName,'' ReceiptSubTypeName");
            sb.AppendLine("FROM Erp_Requirement r with (nolock) left join Erp_WorkOrder wo on wo.RecId = r.WorkOrderId ");
            sb.AppendLine($"where r.RequirementType=1 and r.WorkOrderId in ({workOrderIds})) a ");
            sb.AppendLine("where a.SentQuantity > a.ReceivedQuantity ");
            #endregion

            #region 31-Örülen ve Alınan Kumaştan Çok Tamir Giden Var 
            sb.AppendLine("union all ");
            sb.AppendLine("select *,'Örülen Kumaş+Alınan :'+convert(nvarchar,Cast(a.ReceivedQuantity as decimal(15,2)))+' Tamire Giden :'+convert(nvarchar,Cast(a.SentQuantity as decimal(15,2))) [Explanation] ");
            sb.AppendLine("from(Select distinct (Select WorkOrderNo from Erp_WorkOrder where RecId = r.WorkOrderId) WorkOrderNo,r.WorkOrderId WorkOrderId,r.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType  ");
            sb.AppendLine(",r.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee where ee.RecId in (Select EmployeeId from Meta_User where RecId = r.InsertedBy)) [EmployeeName]  ");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId in (wo.CurrentAccountId)) CurrentAccountName  ");
            sb.AppendLine(",wo.CurrentAccountId  CurrentAccountId  ");
            sb.AppendLine(",0 SubContractorType  ");
            sb.AppendLine(",31 FaultType  ");
            sb.AppendLine(",(isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia with (nolock) where ia.InventoryReceiptType=134 and InventoryReceiptSubType=4 and ia.RequirementId=r.RecId),0)-");
            sb.AppendLine("isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia with (nolock) where ia.InventoryReceiptType=12 and InventoryReceiptSubType=4 and ia.RequirementId=r.RecId),0)) SentQuantity ");
            sb.AppendLine(" ,((isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (1,5,16,17,99) ");
            sb.AppendLine(" or (IA.InventoryReceiptType in (2,3) and (IA.ReturnType is null or IA.ReturnType=0)) or (IA.InventoryReceiptType=11 and IA.InventoryReceiptSubType=1)) ");
            sb.AppendLine(" and IA.RequirementId=r.RecId and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0) ");
            sb.AppendLine(" + isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where not exists(select ia.RecId from Erp_InventoryAllocation ia with(nolock) where (isnull(ia.IsDeleted, 0) = 0 ");
            sb.AppendLine(" and isnull(ia.IsCancelled,0)= 0) and ia.InventoryReceiptItemId = IA.InventoryReceiptItemId and ia.InventoryReceiptType = 29 and ia.RequirementId = r.RecId  and(ia.ReturnType = 1)) ");
            sb.AppendLine(" and IA.InventoryReceiptType = 29  and(IA.ReturnType is null or IA.ReturnType = 0) and IA.RequirementId = r.RecId and IsNull(IA.IsDeleted, 0) = 0 and IsNull(IA.IsCancelled, 0) = 0),0)) ");
            sb.AppendLine(" - isnull((select sum(IA.Quantity) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (12,142) and IA.InventoryReceiptSubType=1 or IA.InventoryReceiptType in (122)) ");
            sb.AppendLine(" and IA.RequirementId=r.RecId and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0)) ReceivedQuantity ");
            sb.AppendLine(",'' ReceiptTypeName,'' ReceiptUpTypeName,'' ReceiptSubTypeName");
            sb.AppendLine("FROM Erp_Requirement r with (nolock) left join Erp_WorkOrder wo on wo.RecId = r.WorkOrderId ");
            sb.AppendLine($"where r.RequirementType=1 and r.WorkOrderId in ({workOrderIds})) a ");
            sb.AppendLine("where a.SentQuantity > a.ReceivedQuantity ");
            #endregion

            #region 32-Örülen+Alınan Kumaştan Çok Baskıya Giden Var 
            sb.AppendLine("union all ");
            sb.AppendLine("select *,'Örülen Kumaş+Alınan :'+convert(nvarchar,Cast(a.ReceivedQuantity as decimal(15,2)))+' Baskıya Giden :'+convert(nvarchar,Cast(a.SentQuantity as decimal(15,2))) [Explanation] ");
            sb.AppendLine("from(Select distinct (Select WorkOrderNo from Erp_WorkOrder where RecId = r.WorkOrderId) WorkOrderNo,r.WorkOrderId WorkOrderId,r.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType  ");
            sb.AppendLine(",r.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee where ee.RecId in (Select EmployeeId from Meta_User where RecId = r.InsertedBy)) [EmployeeName]  ");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId in (wo.CurrentAccountId)) CurrentAccountName  ");
            sb.AppendLine(",wo.CurrentAccountId  CurrentAccountId  ");
            sb.AppendLine(",0 SubContractorType  ");
            sb.AppendLine(",32 FaultType  ");
            sb.AppendLine(",(isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia with (nolock) where ia.InventoryReceiptType=134 and InventoryReceiptSubType=3 and ia.RequirementId=r.RecId),0)-");
            sb.AppendLine("isnull((select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia with (nolock) where ia.InventoryReceiptType=12 and InventoryReceiptSubType=3 and ia.RequirementId=r.RecId),0)) SentQuantity ");
            sb.AppendLine(" ,((isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (1,5,16,17,99) ");
            sb.AppendLine(" or (IA.InventoryReceiptType in (2,3) and (IA.ReturnType is null or IA.ReturnType=0)) or (IA.InventoryReceiptType=11 and IA.InventoryReceiptSubType=1)) ");
            sb.AppendLine(" and IA.RequirementId=r.RecId and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0) ");
            sb.AppendLine(" + isnull((select sum(isnull(IA.Quantity,0)) from Erp_InventoryAllocation IA with (nolock) where not exists(select ia.RecId from Erp_InventoryAllocation ia with(nolock) where (isnull(ia.IsDeleted, 0) = 0 ");
            sb.AppendLine(" and isnull(ia.IsCancelled,0)= 0) and ia.InventoryReceiptItemId = IA.InventoryReceiptItemId and ia.InventoryReceiptType = 29 and ia.RequirementId = r.RecId  and(ia.ReturnType = 1)) ");
            sb.AppendLine(" and IA.InventoryReceiptType = 29  and(IA.ReturnType is null or IA.ReturnType = 0) and IA.RequirementId = r.RecId and IsNull(IA.IsDeleted, 0) = 0 and IsNull(IA.IsCancelled, 0) = 0),0)) ");
            sb.AppendLine(" - isnull((select sum(IA.Quantity) from Erp_InventoryAllocation IA with (nolock) where (IA.InventoryReceiptType in (12,142) and IA.InventoryReceiptSubType=1 or IA.InventoryReceiptType in (122))");
            sb.AppendLine(" and IA.RequirementId=r.RecId and IsNull(IA.IsDeleted,0)=0 and IsNull(IA.IsCancelled,0)=0),0)) ReceivedQuantity ");
            sb.AppendLine(",'' ReceiptTypeName,'' ReceiptUpTypeName,'' ReceiptSubTypeName ");
            sb.AppendLine("FROM Erp_Requirement r with (nolock) left join Erp_WorkOrder wo on wo.RecId = r.WorkOrderId ");
            sb.AppendLine($"where r.RequirementType=1 and r.WorkOrderId in ({workOrderIds})) a ");
            sb.AppendLine("where a.SentQuantity > a.ReceivedQuantity ");
            #endregion

            #region 33-Boyadan Gelen NET KG Kumaş < Kesime Çıkış KG
            sb.AppendLine("union all Select distinct");
            sb.AppendLine(" wo.WorkOrderNo WorkOrderNo,woi.WorkOrderId WorkOrderId,wo.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType");
            sb.AppendLine(",woi.InsertedBy EmployeeId,(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee where ee.RecId in (Select EmployeeId from Meta_User where RecId = woi.InsertedBy)) [EmployeeName]");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId in (wo.CurrentAccountId)) CurrentAccountName");
            sb.AppendLine(",wo.CurrentAccountId  CurrentAccountId");
            sb.AppendLine(",0 SubContractorType");
            sb.AppendLine(",33 FaultType");
            sb.AppendLine(",(select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia with (nolock) where ia.InventoryReceiptType=129 and ia.RequirementId = r.RecId) SentQuantity");
            sb.AppendLine(",(select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia with (nolock) where ia.InventoryReceiptType=11 and ia.InventoryReceiptSubType=2 and ia.RequirementId = r.RecId) ReceivedQuantity");
            sb.AppendLine(",'' ReceiptTypeName");
            sb.AppendLine(",'' ReceiptUpTypeName");
            sb.AppendLine(",'' ReceiptSubTypeName");
            sb.AppendLine(",'Boyadan gelen NET KG Kumaş < Kesime çıkış KG Malzeme: ' + isnull((Select InventoryCode from Erp_Inventory where RecId = r.InventoryId),'') + ' Renk: ' + isnull((Select ItemCode + ' ' + ItemName from Erp_VariantItem where RecId = r.Variant1Id),'') [Explanation] ");
            sb.AppendLine("FROM Erp_WorkOrderItem woi  with (nolock)");
            sb.AppendLine("left join Erp_WorkOrder wo with (nolock) on wo.RecId=woi.WorkOrderId");
            sb.AppendLine("left join Erp_Requirement r with (nolock) on r.WorkOrderId=woi.WorkOrderId and r.StyleId=woi.InventoryId and r.RequirementType=1");
            sb.AppendLine("left join Erp_RecipeItem ri with (nolock) on ri.RecipeType=1 and ri.OwnerInventoryId=r.StyleId");
            sb.AppendLine("where ri.OperationType in (1,3)   -- işlem boya ve boya baskı olanlar");
            sb.AppendLine("and (select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia with (nolock) where ia.InventoryReceiptType=11 and ia.InventoryReceiptSubType=2 and ia.RequirementId = r.RecId)");
            sb.AppendLine("< (select sum(isnull(ia.Quantity,0)) from Erp_InventoryAllocation ia with (nolock) where ia.InventoryReceiptType=129 and ia.RequirementId = r.RecId)");
            sb.AppendLine($"and woi.WorkOrderId in ({workOrderIds})");
            #endregion

            #region 34-Çeki Var, Dikilen Adet Yok veya Sevk Adetten Az
            sb.AppendLine("union all Select");
            sb.AppendLine(" wo.WorkOrderNo WorkOrderNo,wo.RecId WorkOrderId,wo.RecId RecId,0 ReceiptType,0 ReceiptUpType, 0 ReceiptSubType,0 [EmployeeId] ");
            sb.AppendLine(",'' [EmployeeName] ");
            sb.AppendLine(",(Select top 1  CurrentAccountName from Erp_CurrentAccount where RecId in (Select CurrentAccountId from Erp_WorkOrder where RecId = wo.RecId)) CurrentAccountName");
            sb.AppendLine(",wo.CurrentAccountId CurrentAccountId");
            sb.AppendLine(",0 SubContractorType");
            sb.AppendLine(",34 FaultType");
            sb.AppendLine(",(Select isnull(sum(pli.Quantity),0) from Erp_PackingListItem pli where pli.ItemType=1 and pli.WorkOrderItemId in (Select woi.RecId from Erp_WorkOrderItem woi where woi.WorkOrderId=wo.RecId)) SentQuantity");
            sb.AppendLine(",(Select isnull(sum(wop.Quantity),0) from Erp_WorkOrderProduction wop where wop.ProcessId=15 and ProductionType=2 and ProductionSubType=0 and wop.InOut=2 and wop.WorkOrderItemId in (Select woi.RecId from Erp_WorkOrderItem woi where woi.WorkOrderId=wo.RecId )) ReceivedQuantity ");
            sb.AppendLine(",'' ReceiptTypeName");
            sb.AppendLine(",'' ReceiptUpTypeName");
            sb.AppendLine(",'' ReceiptSubTypeName");
            sb.AppendLine(",'Çeki var, dikilen adet yok veya sevk adetten az'[Explanation] ");
            sb.AppendLine("from Erp_WorkOrder wo with(nolock)");
            sb.AppendLine("where (Select isnull(sum(pli.Quantity),0) from Erp_PackingListItem pli where pli.ItemType=1 and pli.WorkOrderItemId in (Select woi.RecId from Erp_WorkOrderItem woi where woi.WorkOrderId=wo.RecId))>");
            sb.AppendLine("(Select isnull(sum(wop.Quantity),0) from Erp_WorkOrderProduction wop where wop.ProcessId=15 and ProductionType=2 and ProductionSubType=0 and wop.InOut=2 and wop.WorkOrderItemId in (Select woi.RecId from Erp_WorkOrderItem woi where woi.WorkOrderId=wo.RecId ))");
            sb.AppendLine($"and wo.RecId in ({workOrderIds})");
            #endregion

            #region 35-Fatura ve Çeki Miktarları Farklı Olma Hatası
            sb.AppendLine("union all select *");
            sb.AppendLine("from(select distinct wo.WorkOrderNo WorkOrderNo");
            sb.AppendLine(",wo.RecId WorkOrderId");
            sb.AppendLine(",wo.RecId RecId");
            sb.AppendLine(",0 ReceiptType");
            sb.AppendLine(",0 ReceiptUpType");
            sb.AppendLine(", 0 ReceiptSubType ");
            sb.AppendLine(",wo.InsertedBy EmployeeId ");
            sb.AppendLine(",(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee where ee.RecId in (Select EmployeeId from Meta_User where RecId = wo.InsertedBy)) [EmployeeName] ");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId in (wo.CurrentAccountId)) CurrentAccountName ");
            sb.AppendLine(",wo.CurrentAccountId  CurrentAccountId ");
            sb.AppendLine(",0 SubContractorType ");
            sb.AppendLine(",35 FaultType ");
            sb.AppendLine(",isnull((Select sum(isnull(pli.Quantity,0)) from Erp_PackingListItem pli with(nolock)  ");
            sb.AppendLine("where pli.PackingListId in (Select RecId from Erp_PackingList pl with(nolock) where pli.WorkOrderId=wo.RecId  ");
            sb.AppendLine($"and Month(pl.CheckingDate) = {DischargeMonth} ");
            sb.AppendLine($"and Year(pl.CheckingDate) = {DischargeYear} ) and pli.ItemType=1),0) SentQuantity ");
            sb.AppendLine(",isnull((select sum(isnull(iri.Quantity,0)) from Erp_InventoryReceiptItem iri ");
            sb.AppendLine("left join Erp_InventoryReceipt ir on ir.RecId=iri.InventoryReceiptId ");
            sb.AppendLine("left join Erp_Invoice inv on inv.RecId=ir.InvoiceId ");
            sb.AppendLine("where inv.ReceiptType in (120,121) and isnull(iri.ItemType,1) = 1 and inv.CompanyId = 9  ");
            sb.AppendLine($"and MONTH(inv.DischargeDate) = {DischargeMonth} and YEAR(inv.DischargeDate) = {DischargeYear}  ");
            sb.AppendLine("and iri.WorkOrderReceiptItemId in (Select woi2.RecId from Erp_WorkOrderItem woi2  ");
            sb.AppendLine("where woi2.WorkOrderId=wo.RecId)), 0) ReceivedQuantity   ");
            sb.AppendLine(",'' ReceiptTypeName   ");
            sb.AppendLine(",'' ReceiptUpTypeName   ");
            sb.AppendLine(",'' ReceiptSubTypeName   ");
            sb.AppendLine($",'{DischargeMonth.ToString()}'+'. Aya ait çeki ve fatura uyuşmazlığı var' [Explanation]  ");
            sb.AppendLine("from  Erp_WorkOrder wo with (nolock) ");
            sb.AppendLine($"where  wo.RecId in ({workOrderIds}))A ");
            sb.AppendLine($"where A.SentQuantity<>A.ReceivedQuantity");
            #endregion

            #region 38-Ordera Bağlanmayan Faturalar
            sb.AppendLine("union all Select ");
            sb.AppendLine("'' WorkOrderNo  ");
            sb.AppendLine(",'' WorkOrderId  ");
            sb.AppendLine(",'' RecId  ");
            sb.AppendLine(",ir.ReceiptType ReceiptType  ");
            sb.AppendLine(",'' ReceiptUpType  ");
            sb.AppendLine(",'' ReceiptSubType   ");
            sb.AppendLine(",ir.EmployeeId EmployeeId   ");
            sb.AppendLine(",(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee where ee.RecId in (Select EmployeeId from Meta_User where RecId = ir.InsertedBy)) [EmployeeName]   ");
            sb.AppendLine(",(Select CurrentAccountName from Erp_CurrentAccount where RecId in (ir.CurrentAccountId)) CurrentAccountName   ");
            sb.AppendLine(",ir.CurrentAccountId  CurrentAccountId   ");
            sb.AppendLine(",0 SubContractorType   ");
            sb.AppendLine(",38 FaultType   ");
            sb.AppendLine(",sum(iri.Quantity) SentQuantity   ");
            sb.AppendLine(",0 ReceivedQuantity     ");
            sb.AppendLine(",'' ReceiptTypeName     ");
            sb.AppendLine(",'' ReceiptUpTypeName     ");
            sb.AppendLine(",'' ReceiptSubTypeName     ");
            sb.AppendLine(",'Ordera Bağlanmamış Fatura Var. Fatura No :'+convert(nvarchar,inv.ReceiptNo) [Explanation]   ");
            sb.AppendLine("from Erp_InventoryReceipt ir with(nolock)   ");
            sb.AppendLine("left join Erp_InventoryReceiptItem iri with(nolock)  on iri.InventoryReceiptId=ir.RecId ");
            sb.AppendLine("left join Erp_Invoice inv with(nolock)  on ir.InvoiceId=inv.RecId ");
            sb.AppendLine("where iri.WorkOrderReceiptItemId is null ");
            sb.AppendLine("and inv.ReceiptType in (120,121) and isnull(iri.ItemType,1) = 1 and inv.CompanyId = 9    ");
            sb.AppendLine($"and MONTH(inv.DischargeDate) = {DischargeMonth} and YEAR(inv.DischargeDate) = {DischargeYear} ");
            sb.AppendLine("group by ir.ReceiptType,ir.EmployeeId,ir.InsertedBy,ir.CurrentAccountId,inv.ReceiptNo ");
            #endregion

            DataTable faultTable = UtilityFunctions.GetDataTableList(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, null, "FaultTable", sb.ToString());
            if (faultTable != null && faultTable.Rows.Count > 0 && subcontractorTypeTable != null && subcontractorTypeTable.Rows.Count > 0)
            {
                foreach (DataRow row in faultTable.Select("ReceiptType in (11,142)", "", DataViewRowState.CurrentRows))
                {
                    int subType = 0;
                    int.TryParse(row["ReceiptSubType"].ToString(), out subType);
                    int index = 0;
                    foreach (DataRow scrow in subcontractorTypeTable.Rows)
                    {
                        index++;
                        if (subType == index)
                        {
                            int scType = 0;
                            int.TryParse(scrow["DefaultType"].ToString(), out scType);
                            row["SubContractorType"] = scType;

                            int rType = 0;
                            int.TryParse(row["ReceiptType"].ToString(), out rType);
                            int upType = 0;
                            int.TryParse(row["ReceiptUpType"].ToString(), out upType);
                            if (rType == 11 && upType == 3 && scType == 11) row["FaultType"] = 3; // iplik boya
                            if (rType == 11 && upType == 1 && scType == 1) row["FaultType"] = 10; // kumaş örmeden geldi.
                            if (rType == 11 && upType == 1 && scType == 11) row["FaultType"] = 12; // kumaş boya
                            if (rType == 11 && upType == 1 && subType == 4) row["FaultType"] = 13; // kumaş tamir
                            if (rType == 11 && upType == 1 && subType == 3) row["FaultType"] = 36; // kumaş baskı
                            if (rType == 11 && upType == 2 && scType == 11) row["FaultType"] = 22; // aksesuar boya
                            if (rType == 142 && upType == 1 && scType == 1) row["FaultType"] = 11; // kumaş örme gelen iadeden
                            break;
                        }
                    }
                }
                faultTable.AcceptChanges();
            }
            return faultTable;
        }
    }
}
