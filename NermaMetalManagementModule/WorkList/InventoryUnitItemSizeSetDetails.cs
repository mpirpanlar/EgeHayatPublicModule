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
using Microsoft.Practices.Unity;
using Sentez.Localization;
using Sentez.Common.ModuleBase;
using System.Data;

namespace Sentez.NermaMetalManagementModule.WorkList
{
    public class InventoryUnitItemSizeSetDetails : ReportBase
    {
        public override bool CacheResults
        {
            get
            {
                return true;
            }
        }

        public InventoryUnitItemSizeSetDetails(IUnityContainer container)
            : base(container)
        {
            Name = "Erp_InventoryUnitItemSizeSetDetailsSizeDetailCodeList";
            Title = SLanguage.GetString("Malzeme Ölçü Listesi");
            WorkMode = ReportWorkMode.WorkList;
        }

        public override void Init()
        {
            InitBegin();

            Statement _statement1 = new Statement("Erp_InventoryUnitItemSizeSetDetails");
            _statement1.AddTable("Erp_InventoryUnitItemSizeSetDetails", "erp_inventoryunititemsizesetdetails");
            _statement1.SetBaseTable("erp_inventoryunititemsizesetdetails");

            _statement1.LoadAllFields();

            _statement1.AddCol("RecId", "erp_inventoryunititemsizesetdetails", "RecId", false);

            _statement1.AddColMandatory("SizeDetailCode", "erp_inventoryunititemsizesetdetails", SLanguage.GetString("Ölçü Kodu"));
            _statement1.AddColMandatory("SizeDetailName", "erp_inventoryunititemsizesetdetails", SLanguage.GetString("Ölçü Adı"));

            _statement1.AddMandatoryFilters(activeSession);

            if (PolicyParam?.ObjectActiveRow != null)
            {
                if(PolicyParam.ObjectActiveRow is DataRowView && (PolicyParam.ObjectActiveRow as DataRowView).Row.Table.Columns.Contains("InventoryId"))
                {
                    long inventoryId;
                    long.TryParse((PolicyParam.ObjectActiveRow as DataRowView).Row["InventoryId"].ToString(), out inventoryId);
                    _statement1.AddWhere(new FilterItem(WhereTermType.Compare, SqlDataType.Number, CompareOperator.Equal, "erp_inventoryunititemsizesetdetails", "InventoryId", null)).valueList[0] = inventoryId;
                }
            }

            _statement1.OrderBy("erp_inventoryunititemsizesetdetails", "SizeDetailCode", OrderByDirection.Ascending);

            AddStatement(_statement1);

            InitEnd();
        }

        public override object GetResultFieldValue(int row)
        {
            if (!Data.Tables[0].Columns.Contains(GetResultFieldName())) return null; return Data.Tables[0].DefaultView[row][GetResultFieldName()];
        }
    }
}
