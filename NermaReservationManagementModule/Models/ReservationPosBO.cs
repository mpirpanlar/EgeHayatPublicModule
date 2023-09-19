using Prism.Ioc;

using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.Security;
using Sentez.Common.SystemServices;
using Sentez.Common.Utilities;
using Sentez.Data.BusinessObjects;
using Sentez.Data.Query;
using Sentez.Data.Tools;
using Sentez.MetaPosModule.ParameterClasses;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace NermaMetalManagementModule.Models
{
    [BusinessObjectExplanation("Varyant Kartı Marka Bağlantıları")]
    [SecurityModuleId((short)Modules.ExternalModule15)]
    public class ReservationPosBO : BusinessObjectBase
    {

        private readonly DateHelper _dateHelper;
        readonly PosParameters _posParams;

        public ReservationPosBO(IContainerExtension container)
            : base(container, 0, "", "TransactionType", new string[] { "Erp_Pos" })
        {
            _dateHelper = new DateHelper
            {
                OperationMode = OperationMode.UserParameterMode,
                Container = _container
            };

            _posParams = GetParameterClass("PosParameters") as PosParameters;
            KeyFields.Add(new WhereField("Erp_Pos", "CompanyId", _companyId, WhereCondition.Equal));

            Lookups.AddLookUp("Erp_Pos", "InsertedBy", true, "Meta_User", "UserCode", "InsertedUserCode", "UserName", "InsertedUserName");
            Lookups.AddLookUp("Erp_Pos", "LoyalityCardId", true, "Erp_LoyalityCard", "LoyalityCardCode", "LoyalityCardCode", new string[] { "Explanation", "InUse", "CustomerName", "ProxyNo", "Limit", "UsedLimit", "LimitForex", "UsedLimitForex", "StartDate", "EndDate", "LoyalityCardTypeId", "PriceGroupCode", "DiscountGroupCode", "Discount" }, new string[] { "LoyalityCardName", "LoyalityCardInUse", "LoyalityCardCustomerName", "LoyalityCardProxyNo", "LoyalityCardLimit", "LoyalityCardUsedLimit", "LoyalityCardLimitForex", "LoyalityCardUsedLimitForex", "LoyalityCardStartDate", "LoyalityCardEndDate", "LoyalityCardTypeId", "LoyalityCardPriceGroupCode", "LoyalityCardDiscountGroupCode", "LoyalityCardDiscount" });
            Lookups.AddLookUp("Erp_Pos", "CurrentAccountId", true, "Erp_CurrentAccount", "CurrentAccountCode", "CurrentAccountCode", new string[] { "CurrentAccountName", "InUse", "SpecialCode", "GroupId", "ForexId", "IsEInvoice", "EInvoiceScenario", "EInvoiceAlias", "EMail", "TaxNo", "TaxOfficeId", "GroupId", "GsmPhone", "TradeName", "SalesDiscountId", "PurchaseDiscountId", "WarehouseId", "PriceGroupCode", "DiscountGroupCode", "CategoryId" }, new string[] { "CurrentAccountName", "CurrentAccountInUse", "CurrentAccountSpecialCode", "CurrentAccountGroupId", "CurrentAccountForexId", "CurrentAccountIsEInvoice", "CurrentAccountEInvoiceScenario", "CurrentAccountEInvoiceAlias", "CurrentAccountEMail", "CurrentAccountTaxNo", "CurrentAccountTaxOfficeId", "CurrentAccountGroupId", "CurrentAccountGsmPhone", "CurrentAccountTradeName", "CurrentAccountSalesDiscountId", "CurrentAccountPurchaseDiscountId", "CurrentAccountWarehouseId", "CurrentAccountPriceGroupCode", "CurrentAccountDiscountGroupCode", "CurrentAccountCategoryId" });
            Lookups.AddLookUp("Erp_Pos", "DealerId", true, "Erp_CurrentAccount", "CurrentAccountCode", "DealerCode", new string[] { "CurrentAccountName", "InUse", "GsmPhone" }, new string[] { "DealerName", "DealerInUse", "DealerGSMPhone" }, new WhereField[] { WhereField.GetIsDeletedRule("Erp_CurrentAccount"), new WhereField("Erp_CurrentAccount", "IsDealer", 1, WhereCondition.Equal) });
            Lookups.AddLookUp("Erp_Pos", "DepartmentId", true, "Erp_Department", "DepartmentCode", "DepartmentCode", new string[] { "DepartmentName", "InUse", "SpecialCode", "ServiceId" }, new string[] { "DepartmentName", "DepartmentInUse", "DepartmentSpecialCode", "DepartmentServiceId" });
            Lookups.AddLookUp("Erp_Pos", "CashId", true, "Erp_Cash", "CashCode", "CashCode", "Explanation", "CashName");
            Lookups.AddLookUp("Erp_Pos", "WorkplaceId", true, "Erp_Workplace", "WorkplaceCode", "WorkplaceCode", "WorkplaceName", "WorkplaceName");
            Lookups.AddLookUp("Erp_Pos", "ForexId", true, "Meta_Forex", "ForexCode", "ForexCode", "ForexName", "ForexName");
            Lookups.AddLookUp("Erp_Pos", "WarehouseId", true, "Erp_Warehouse", "WarehouseCode", "WarehouseCode", new string[] { "WarehouseName", "InUse", "StartDate", "EndDate", "IsLocked" }, new string[] { "WarehouseName", "WarehouseInUse", "WarehouseStartDate", "WarehouseEndDate", "WarehouseIsLocked" });
            Lookups.AddLookUp("Erp_Pos", "EmployeeId", true, "Erp_Employee", "EmployeeCode", "EmployeeCode", new string[] { "EmployeeName", "InUse" }, new string[] { "EmployeeName", "EmployeeInUse" });
            Lookups.AddLookUp("Erp_Pos", "CourierId", true, "Erp_Employee", "EmployeeCode", "CourierCode", new string[] { "EmployeeName", "GsmPhone", "InUse" }, new string[] { "CourierName", "GsmPhone", "CourierInUse" });
            Lookups.AddLookUp("Erp_Pos", "CashierId", true, "Erp_Employee", "EmployeeCode", "CashierCode", new string[] { "EmployeeName", "InUse" }, new string[] { "CashierName", "CashierInUse" });
            Lookups.AddLookUp("Erp_Pos", "CostCenterId", true, "Erp_CostCenter", "CostCenterCode", "CostCenterCode", new string[] { "CostCenterName", "InUse" }, new string[] { "CostCenterName", "CostCenterInUse" });
            Lookups.AddLookUp("Erp_Pos", "TransporterId", true, "Erp_Transporter", "TransporterCode", "TransporterCode", new string[] { "TransporterName", "InUse" }, new string[] { "TransporterName", "TransporterInUse" });
            Lookups.AddLookUp("Erp_Pos", "VehicleId", true, "Erp_Vehicle", "VehicleCode", "VehicleCode", new string[] { "VehicleName", "InUse" }, new string[] { "VehicleName", "VehicleInUse" });

            /*
                            new WhereField(ActiveBO.BaseTable,"TransactionType",(short)PosSalesTypeDefinition.PosTransactionType.Reservation, WhereCondition.Equal),
                            new WhereField(ActiveBO.BaseTable,"ReceiptType",(short)PosSalesTypeDefinition.PosReceiptType.Sales, WhereCondition.Equal),
                            new WhereField(ActiveBO.BaseTable,"SalesType",(short)PosSalesTypeDefinition.PosSalesType.IsReservation, WhereCondition.Equal)
            */

            ValueFiller.AddRule("Erp_Pos", "TransactionType", (short)PosSalesTypeDefinition.PosTransactionType.Reservation);
            ValueFiller.AddRule("Erp_Pos", "ReceiptType", (short)PosSalesTypeDefinition.PosReceiptType.Sales);
            ValueFiller.AddRule("Erp_Pos", "SalesType", (short)PosSalesTypeDefinition.PosSalesType.IsReservation);
            ValueFiller.AddRule("Erp_Pos", "ReceiptUpType", 0);
            ValueFiller.AddRule("Erp_Pos", "ReceiptSubType", 0);
            ValueFiller.AddRule("Erp_Pos", "OnlineCourierType", 0);
            ValueFiller.AddRule("Erp_Pos", "IsClosed", 0);
            ValueFiller.AddRule("Erp_Pos", "IsCancelled", 0);
            ValueFiller.AddRule("Erp_Pos", "IsDeleted", 0);
            ValueFiller.AddRule("Erp_Pos", "CashRegisterTransactionOk", 0);

            ValueFiller.AddRule("Erp_Pos", "ReceiptDate", GetToday);
            ValueFiller.AddRule("Erp_Pos", "ReceiptTime", GetCreateTime);
            if (ActiveSession.Department != null && ActiveSession.Department.RecId.HasValue)
                ValueFiller.AddRule("Erp_Pos", "DepartmentId", ActiveSession.Department.RecId.Value);
            if (ActiveSession.Department != null && ActiveSession.Department.CostCenterId.HasValue)
                ValueFiller.AddRule("Erp_Pos", "CostCenterId", ActiveSession.Department.CostCenterId.Value);

            if (ActiveSession.Cash != null && ActiveSession.Cash.RecId.HasValue) ValueFiller.AddRule("Erp_Pos", "CashCode", ActiveSession.Cash.CashCode);
            if (ActiveSession != null && ActiveSession.Workplace != null && ActiveSession.Workplace.RecId.HasValue) ValueFiller.AddRule("Erp_Pos", "WorkplaceId", ActiveSession.Workplace.RecId.Value);
            if (_posParams.EmployeeFromUser && !_posParams.EmployeeInDetail && ActiveSession.ActiveUser != null && ActiveSession.ActiveUser.EmployeeId.HasValue) ValueFiller.AddRule("Erp_Pos", "EmployeeId", ActiveSession.ActiveUser.EmployeeId.Value);
            if (ActiveSession.Warehouse != null && ActiveSession.Warehouse.RecId.HasValue) ValueFiller.AddRule("Erp_Pos", "WarehouseId", ActiveSession.Warehouse.RecId.Value);
            ValueFiller.AddRule("Erp_Pos", "ComplimentaryType", PosComplimentaryType.None);

            //new CodeGenerator(this, "PosCashRegisterReceiptNoCodeGenerator", CompanyId, null, (byte)Modules.InvoiceModule, 1, "Erp_Pos", "CashRegisterReceiptNo", "TransactionType", true, true, false, true).TemplateString = "########";

            //new CodeGenerator(this, "PosCashRegisterReceiptNoCodeGenerator", CompanyId, null, (byte)Modules.InvoiceModule, 1, "Erp_Pos", "CashRegisterReceiptNo", "TransactionType", true, true, true, true);
            //new CodeGenerator(this, "PosReceiptNoCodeGenerator", CompanyId, null, (byte)Modules.InvoiceModule, 1, "Erp_Pos", "ReceiptNo", "TransactionType", true, true, true, true);

            SecurityChecker.LogicalModuleID = (short)Modules.ExternalModule15;
        }

        private object GetToday()
        {
            if (_posParams.OtelApp && _posParams.OtelToDayRetail)
            {
                if (_dateHelper != null)
                {
                    _dateHelper.OperationMode = OperationMode.AgileMode;
                    return _dateHelper.GetToday(Transaction);
                }
                return _dateHelper.GetToday();
            }
            return _dateHelper.GetToday();
        }

        private object GetCreateTime()
        {
            return new DateTime(1899, 12, 30, DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, DateTime.Now.TimeOfDay.Seconds);
        }

        public override void Init(BoParam boParam)
        {
            base.Init(boParam);
            if (boParam == null) return;
            int transactionType = boParam.Type;

            //var o = Extensions["PosCashRegisterReceiptNoCodeGenerator"] as CodeGenerator;
            //if (o != null) o.SubModule = 1;
            //var codeGenerator1 = Extensions["PosCashRegisterReceiptNoCodeGenerator"] as CodeGenerator;
            //if (codeGenerator1 != null) codeGenerator1.TemplateString = "########";

            //var posReceiptNoCodeGenerator = Extensions["PosReceiptNoCodeGenerator"] as CodeGenerator;
            //if (posReceiptNoCodeGenerator != null)
            //{
            //    if (boParam.DetailType >= 1 && boParam.DetailType <= short.MaxValue)
            //        posReceiptNoCodeGenerator.SubModule = Convert.ToInt16(boParam.DetailType);
            //    else
            //        posReceiptNoCodeGenerator.SubModule = 1;
            //    posReceiptNoCodeGenerator.TemplateString = "########";
            //    posReceiptNoCodeGenerator.TypeField = "TransactionType";
            //    posReceiptNoCodeGenerator.DateField = "ReceiptDate";
            //}
        }
    }
}
