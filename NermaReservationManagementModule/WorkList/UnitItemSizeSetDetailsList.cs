using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sentez.Common.SqlBuilder;
using Sentez.Data.BusinessObjects;
using Reeb.SqlOM;
using Sentez.Common.Report;
using Sentez.Data.Tools;
using System.Xml;
using System.Windows.Controls;
using Sentez.Common.Commands;
using System.IO;
using Prism.Ioc;
using Sentez.Localization;
using Sentez.Common.ModuleBase;

namespace Sentez.NermaReservationManagementModule.WorkList
{
    public class UnitItemSizeSetDetailsList : ReportBase
    {
        public override bool CacheResults
        {
            get
            {
                return true;
            }
        }

        public UnitItemSizeSetDetailsList(IContainerExtension container)
            : base(container)
        {
            Name = "Erp_UnitItemSizeSetDetailsSizeDetailCodeList";
            Title = SLanguage.GetString("Ölçü Listesi");
            WorkMode = ReportWorkMode.WorkList;
        }

        public override void Init()
        {
            InitBegin();

            Statement _statement1 = new Statement("Erp_UnitItemSizeSetDetails");
            _statement1.AddTable("Erp_UnitItemSizeSetDetails", "erp_unititemsizesetdetails");
            _statement1.SetBaseTable("erp_unititemsizesetdetails");

            _statement1.LoadAllFields();

            _statement1.AddCol("RecId", "erp_unititemsizesetdetails", "RecId", false);

            _statement1.AddColMandatory("SizeDetailCode", "erp_unititemsizesetdetails", SLanguage.GetString("Ölçü Kodu"));
            _statement1.AddColMandatory("SizeDetailName", "erp_unititemsizesetdetails", SLanguage.GetString("Ölçü Adı"));

            _statement1.AddMandatoryFilters(activeSession);

            _statement1.OrderBy("erp_unititemsizesetdetails", "SizeDetailCode", OrderByDirection.Ascending);

            AddStatement(_statement1);

            InitEnd();
        }

        public override object GetResultFieldValue(int row)
        {
            if (!Data.Tables[0].Columns.Contains(GetResultFieldName())) return null; return Data.Tables[0].DefaultView[row][GetResultFieldName()];
        }
    }
}
