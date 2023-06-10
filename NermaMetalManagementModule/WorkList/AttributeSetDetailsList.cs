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

namespace Sentez.NermaMetalManagementModule.WorkList
{
    public class AttributeSetDetailsList : ReportBase
    {
        public override bool CacheResults
        {
            get
            {
                return true;
            }
        }

        public AttributeSetDetailsList(IContainerExtension container)
            : base(container)
        {
            Name = "Erp_AttributeSetDetailsAttributeSetCodeList";
            Title = SLanguage.GetString("Özellik Listesi");
            WorkMode = ReportWorkMode.WorkList;
        }

        public override void Init()
        {
            InitBegin();

            Statement _statement1 = new Statement("Erp_AttributeSetDetails");
            _statement1.AddTable("Erp_AttributeSetDetails", "erp_attributesetdetails");
            _statement1.SetBaseTable("erp_attributesetdetails");

            _statement1.LoadAllFields();

            _statement1.AddCol("RecId", "erp_attributesetdetails", "RecId", false);

            _statement1.AddColMandatory("AttributeSetCode", "erp_attributesetdetails", SLanguage.GetString("Özellik Kodu"));
            _statement1.AddColMandatory("AttributeSetName", "erp_attributesetdetails", SLanguage.GetString("Özellik Adı"));

            _statement1.AddMandatoryFilters(activeSession);

            _statement1.OrderBy("erp_attributesetdetails", "AttributeSetCode", OrderByDirection.Ascending);

            AddStatement(_statement1);

            InitEnd();
        }

        public override object GetResultFieldValue(int row)
        {
            if (!Data.Tables[0].Columns.Contains(GetResultFieldName())) return null; return Data.Tables[0].DefaultView[row][GetResultFieldName()];
        }
    }
}
