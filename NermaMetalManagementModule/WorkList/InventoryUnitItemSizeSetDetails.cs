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
using System.Data;
using Sentez.InventoryModule.PresentationModels;

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

        public InventoryUnitItemSizeSetDetails(IContainerExtension container)
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
                if (PolicyParam.ObjectActiveRow is DataRowView && (PolicyParam.ObjectActiveRow as DataRowView).Row.Table.Columns.Contains("InventoryId"))
                {
                    //long inventoryId;
                    //long.TryParse((PolicyParam.ObjectActiveRow as DataRowView).Row["InventoryId"].ToString(), out inventoryId);
                    //_statement1.AddWhere(new FilterItem(WhereTermType.Compare, SqlDataType.Number, CompareOperator.Equal, "erp_inventoryunititemsizesetdetails", "InventoryId", null)).valueList[0] = inventoryId;
                    int inventoryCategoryId;
                    int.TryParse((PolicyParam.ObjectActiveRow as DataRowView).Row["InventoryCategoryId"].ToString(), out inventoryCategoryId);
                    int catId = inventoryCategoryId;
                    string catPath = "";
                    while (catId > 0)
                    {
                        using (DataTable table = UtilityFunctions.GetDataTableList(activeSession.dbInfo.DBProvider, activeSession.dbInfo.Connection, null, "Erp_Category", $"select * from Erp_Category with (nolock) where RecId={catId}"))
                        {
                            if (table?.Rows.Count > 0)
                            {
                                if (string.IsNullOrEmpty(catPath))
                                    catPath = table.Rows[0]["RecId"].ToString();
                                else catPath += "," + $"{table.Rows[0]["RecId"]}";
                                int parentId;
                                int.TryParse(table.Rows[0]["ParentId"].ToString().Trim(), out parentId);
                                catId = parentId;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(catPath))
                    {
                        _statement1.AddWhere(WhereTermType.AddSql, "a", "b",$" [erp_inventoryunititemsizesetdetails].[CategoryId] in ({catPath})");
                    }
                    else 
                    {
                        _statement1.AddWhere(WhereTermType.AddSql, "a", "b", $" [erp_inventoryunititemsizesetdetails].[CategoryId]=-1");
                    }
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
