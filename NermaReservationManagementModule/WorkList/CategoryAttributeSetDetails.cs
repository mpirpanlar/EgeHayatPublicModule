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

namespace Sentez.NermaReservationManagementModule.WorkList
{
    public class CategoryAttributeSetDetails : ReportBase
    {
        public override bool CacheResults
        {
            get
            {
                return true;
            }
        }

        public CategoryAttributeSetDetails(IContainerExtension container)
            : base(container)
        {
            Name = "Erp_CategoryAttributeSetDetailsAttributeSetCodeList";
            Title = SLanguage.GetString("Malzeme Ölçü Listesi");
            WorkMode = ReportWorkMode.WorkList;
        }

        public override void Init()
        {
            InitBegin();

            Statement _statement1 = new Statement("Erp_CategoryAttributeSetDetails");
            _statement1.AddTable("Erp_CategoryAttributeSetDetails", "erp_categoryattributesetdetails");
            _statement1.SetBaseTable("erp_categoryattributesetdetails");

            _statement1.LoadAllFields();

            _statement1.AddCol("RecId", "erp_categoryattributesetdetails", "RecId", false);

            _statement1.AddColMandatory("AttributeSetCode", "erp_categoryattributesetdetails", SLanguage.GetString("Özellik Kodu"));
            _statement1.AddColMandatory("AttributeSetName", "erp_categoryattributesetdetails", SLanguage.GetString("Özellik Adı"));

            _statement1.AddMandatoryFilters(activeSession);

            if (PolicyParam?.ObjectActiveRow != null)
            {
                if (PolicyParam.ObjectActiveRow is DataRowView && (PolicyParam.ObjectActiveRow as DataRowView).Row.Table.Columns.Contains("InventoryCategoryId"))
                {
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
                        _statement1.AddWhere(WhereTermType.AddSql, "a", "b", $" [erp_categoryattributesetdetails].[CategoryId] in ({catPath})");
                    }
                    else
                    {
                        _statement1.AddWhere(WhereTermType.AddSql, "a", "b", $" [erp_categoryattributesetdetails].[CategoryId]=-1");
                    }
                }

                if (PolicyParam.ObjectActiveRow is DataRowView && (PolicyParam.ObjectActiveRow as DataRowView).Row.Table.Columns.Contains("AttributeItemIsSelect"))
                {
                    bool attributeItemIsSelect;
                    bool.TryParse((PolicyParam.ObjectActiveRow as DataRowView).Row["AttributeItemIsSelect"].ToString(), out attributeItemIsSelect);
                    if (!attributeItemIsSelect)
                    {
                        _statement1.AddWhere(WhereTermType.AddSql, "a", "b", $" [erp_categoryattributesetdetails].[CategoryId]=-1");
                    }
                }
            }

            _statement1.OrderBy("erp_categoryattributesetdetails", "AttributeSetCode", OrderByDirection.Ascending);

            AddStatement(_statement1);

            InitEnd();
        }

        public override object GetResultFieldValue(int row)
        {
            if (!Data.Tables[0].Columns.Contains(GetResultFieldName())) return null; return Data.Tables[0].DefaultView[row][GetResultFieldName()];
        }
    }
}
