using Prism.Ioc;
using Reeb.SqlOM;
using Sentez.Common.Report;
using Sentez.Common.SqlBuilder;
using Sentez.Data.MetaData;
using Sentez.Localization;

namespace Sentez.NermaReservationManagementModule.Services
{
    class FaultTaskControlPolicy : ReportBase
    {
        public FaultTaskControlPolicy(IContainerExtension container)
           : base(container)
        {
            Name = "FaultTaskControlPolicy";
            Title = "Hata Görev Kontrolü";
            WorkMode = ReportWorkMode.WorkList;
        }

        override public void Init()
        {
            InitBegin();
            InitStatements(null);
            InitEnd();
        }
        override public void InitStatements(object prm)
        {
             Statement _statement = new Statement("Erp_CustomerTransaction");
            _statement.AddTable("Erp_CustomerTransaction", "ect"); 
            _statement.AddTable("Erp_CustomerTransactionActivity", "ecta");

            _statement.SetBaseTable("ect");

            _statement.AddCol("RecId", "ect", SLanguage.GetString("RecId"), false);
            _statement.AddCol("SourceId", "ect", SLanguage.GetString("SourceId"), false);
            _statement.AddCol("SourceDocumentNo", "ect", SLanguage.GetString("WorkOrderId"), false);
            //_statement.AddCol("ProcessId", "erp_planning", SLanguage.GetString("ProcessId"), false);
            //_statement.AddColCalc("(Select ProcessCode from Erp_Process where RecId = erp_planning.ProcessId)", SLanguage.GetString("Hata Tipi"), SqlDataType.String, FieldUsage.Name, "");
            _statement.AddCol("Explanation", "ecta", SLanguage.GetString("Order Numarası"));
            _statement.AddCol("EmployeeId", "ecta", SLanguage.GetString("EmployeeId"), false);
            _statement.AddColCalc("(Select isnull(ee.EmployeeName,'') +' '+ isnull(ee.EmployeeSurname,'') from Erp_Employee ee where ee.RecId = ecta.EmployeeId)", SLanguage.GetString("Çalışan Adı"), SqlDataType.String, FieldUsage.Name, "");
            _statement.AddCol("Explanation", "ect", SLanguage.GetString("Hata Sebebi"), "");
            _statement.AddColCalc("ecta.Quantity", SLanguage.GetString("Çıkış"), SqlDataType.Number, FieldUsage.Quantity, 0);
            _statement.AddColCalc("ecta.UnitPrice", SLanguage.GetString("Giriş"), SqlDataType.Number, FieldUsage.Quantity, 0);
            _statement.AddCol("StartDate", "ecta", SLanguage.GetString("Başlangıç"), "");
            _statement.AddCol("EndDate", "ecta", SLanguage.GetString("Bitiş"), "");
            _statement.AddCol("ApprovedExplanation", "ect", SLanguage.GetString("Hata Açıklaması"), "");
            _statement.AddCol("IsApproved", "ect", SLanguage.GetString("Tamamlandı"), 0);

            _statement.JoinTables("ect", "ecta", "RecId", "CustomerTransactionId", JoinType.Left);

            _statement.LoadAllFields(false);
            AddStatement(_statement);
            ViewStatement = _statement;
        }
    }
}
