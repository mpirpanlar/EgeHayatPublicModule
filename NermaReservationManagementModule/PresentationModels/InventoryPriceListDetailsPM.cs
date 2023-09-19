using Prism.Ioc;
using Sentez.Common;
using Sentez.Common.PresentationModels;
using Sentez.Common.Commands;
using Sentez.Common.Utilities;
using Sentez.Data.Tools;
using Sentez.Common.Report;
using System.Data;
using Sentez.Data.Query;
using System;
using LiveCore.Desktop.UI.Controls;
using System.Windows.Input;
using System.Windows;
using System.Xml.Linq;
using Sentez.Core.ParameterClasses;
using DevExpress.Xpf.Grid;
using DevExpress.CodeParser;
using DevExpress.XtraRichEdit.SpellChecker;
using System.Text;
using DevExpress.Xpf.Core;
using static DevExpress.Mvvm.Native.Either;
using System.ComponentModel.Design;
using System.Windows.Media;
using Sentez.Data.BusinessObjects;
using System.Linq;
using Sentez.Common.ModuleBase;
using Sentez.Localization;
using Sentez.InventoryModule;

namespace Sentez.NermaReservationManagementModule.PresentationModels
{
    public partial class InventoryPriceListDetailsPM : PMDesktop
    {
        public LookupList Lists { get; set; }
        LiveGridControl gridDetailPrice, gridDetailPriceSale;

        private object inventoryGroupIAGridViewSelectedItem;
        public object InventoryGroupIAGridViewSelectedItem
        {
            get { return inventoryGroupIAGridViewSelectedItem; }
            set
            {
                inventoryGroupIAGridViewSelectedItem = value;
                this.OnPropertyChanged("InventoryGroupIAGridViewSelectedItem");
            }
        }

        public InventoryPriceListDetailsPM(IContainerExtension container_)
            : base(container_)
        {
        }

        public override void LoadCommands()
        {
            base.LoadCommands();
        }

        public override void Init()
        {
            base.Init();

            Lists = ActiveSession.LookupList.GetChild(UtilityFunctions.GetConnection(ActiveSession.dbInfo.DBProvider, ActiveSession.dbInfo.ConnectionString));

            if (ActiveBO != null)
            {
                using (DataTable tableInv = UtilityFunctions.GetDataTableList(ActiveBO.Provider, ActiveBO.Connection, ActiveBO.Transaction, "Erp_Inventory", $"select RecId from Erp_Inventory with (nolock) where CompanyId={ActiveBO.CompanyId} AND InventoryCode='CATEGORY_INV'"))
                {
                    if (tableInv?.Rows.Count == 0)
                    {
                        ActiveBO.NewRecord();
                        ActiveBO.CurrentRow["InventoryCode"] = "CATEGORY_INV";
                        ActiveBO.CurrentRow["InventoryName"] = "CATEGORY_INV";
                        ActiveBO.CurrentRow["InventoryType"] = 1;
                        ActiveBO.CurrentRow["HasRowVariant"] = 1;
                        ActiveBO.CurrentRow["IsClass"] = 0;
                        using (DataTable table = UtilityFunctions.GetDataTableList(ActiveBO.Provider, ActiveBO.Connection, ActiveBO.Transaction, "Meta_UnitSet", "select * from Meta_UnitSet with (nolock)"))
                        {
                            if (table?.Rows.Count > 0)
                            {
                                ActiveBO.CurrentRow["UnitId"] = table.Rows[0]["RecId"];
                            }
                        }
                        ActiveBO.ExtensionsEnabled = false;
                        PostResult postResult = ActiveBO.PostData();
                        if (postResult == PostResult.Succeed)
                        {

                        }
                        ActiveBO.ExtensionsEnabled = true;
                    }
                    else
                    {
                        if (ActiveBO.Get(Convert.ToInt64(tableInv.Rows[0]["RecId"])) > 0)
                        {

                        }
                    }
                }
                //if (_pmParam.itemID > 0)
                //{
                //    ActiveBO.Get(_pmParam.itemID);
                //}
                //else
                //{
                //    if (!ActiveBO.IsNewRecord) ActiveBO.NewRecord();
                //}

                ActiveBO.ColumnChanged += ActiveBO_ColumnChanged;

                //ActiveBO.GetAll(new WhereField(WhereFieldType.And, new WhereField[]{
                //new WhereField(ActiveBO.BaseTable,"CompanyId",ActiveBO.CompanyId, WhereCondition.Equal),
                //new WhereField(ActiveBO.BaseTable,"InventoryId",null, WhereCondition.IsNull),
                //new WhereField(ActiveBO.BaseTable,"IsPriceDiscount",1, WhereCondition.Equal),
                //new WhereField(ActiveBO.BaseTable,"ItemType",1, WhereCondition.Equal),
                //new WhereField(ActiveBO.BaseTable,"PriceType",2, WhereCondition.Equal)
                //}));


            }
            gridDetailPrice = FCtrl("gridDetailPrice") as LiveGridControl;
            gridDetailPriceSale = FCtrl("gridDetailPriceSale") as LiveGridControl;
            if (gridDetailPriceSale != null)
            {
                gridDetailPriceSale.GetCustomColumns();
                gridDetailPriceSale.CreatedNewRow += gridDetailPriceSale_CreatedNewRow;
                gridDetailPriceSale.BeforeCreateNewRow += gridDetailPriceSale_BeforeCreateNewRow;
                gridDetailPriceSale.BeforeDeleteItem += gridDetailPriceSale_BeforeDeleteItem;
                gridDetailPriceSale.CurrentColumnChanged += gridDetailPriceSale_CurrentColumnChanged;
                if (ActiveSession.ActiveUser.AccessCodes != null && ActiveSession.ActiveUser.AccessCodes.Count > 0)
                {
                    string accessCodeList = string.Empty;
                    foreach (var item in ActiveSession.ActiveUser.AccessCodes)
                    {
                        accessCodeList += string.Format(" or AccessCode = '{0}'", item.AccessCode);
                    }
                    accessCodeList = string.Format("PriceType=2 and (AccessCode is null or {0})", accessCodeList.Remove(0, 4));
                    gridDetailPriceSale.RowFilter = accessCodeList;
                }
            }
        }

        bool _suppressEvents;
        private void ActiveBO_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (_suppressEvents)
                return;
            if (e.Column.ColumnName == "ItemVariant1Code" || e.Column.ColumnName == "ItemVariant2Code" || e.Column.ColumnName == "ItemVariant3Code" || e.Column.ColumnName == "ItemVariant4Code" || e.Column.ColumnName == "ItemVariant5Code")
            {
                try
                {
                    _suppressEvents = true;
                    string _variantTypeCode = "";
                    if (e.Column.ColumnName == "ItemVariant1Code")
                        _variantTypeCode = ActiveBO.ActiveSession.ParamService.GetParameterClass<InventoryParameters>().DefaultVariant1TypeCode;
                    else if (e.Column.ColumnName == "ItemVariant2Code")
                        _variantTypeCode = ActiveBO.ActiveSession.ParamService.GetParameterClass<InventoryParameters>().DefaultVariant2TypeCode;
                    else if (e.Column.ColumnName == "ItemVariant3Code")
                        _variantTypeCode = ActiveBO.ActiveSession.ParamService.GetParameterClass<InventoryParameters>().DefaultVariant3TypeCode;
                    else if (e.Column.ColumnName == "ItemVariant4Code")
                        _variantTypeCode = ActiveBO.ActiveSession.ParamService.GetParameterClass<InventoryParameters>().DefaultVariant4TypeCode;
                    else if (e.Column.ColumnName == "ItemVariant5Code")
                        _variantTypeCode = ActiveBO.ActiveSession.ParamService.GetParameterClass<InventoryParameters>().DefaultVariant5TypeCode;
                    StringBuilder sbSql = new StringBuilder();
                    sbSql.AppendLine("SELECT EVI.RecId,EVI.ItemCode,EVI.ItemName, EVC.TypeId, EVT.TypeName");
                    sbSql.AppendLine("FROM Erp_VariantItem EVI WITH(NOLOCK)");
                    sbSql.AppendLine("LEFT JOIN Erp_VariantCard EVC WITH(NOLOCK) ON EVI.CardId = EVC.RecId");
                    sbSql.AppendLine("LEFT JOIN Erp_VariantType EVT WITH(NOLOCK) ON EVC.TypeId = EVT.RecId");
                    sbSql.AppendLine($"WHERE EVT.CompanyId = {ActiveBO.CompanyId} AND EVT.TypeName = '{_variantTypeCode}' AND ItemCode='{e.ProposedValue}'");

                    using (DataTable table = UtilityFunctions.GetDataTableList(ActiveBO.Provider, ActiveBO.Connection, ActiveBO.Transaction, "Erp_VariantItem", sbSql.ToString()))
                    {
                        if (table?.Rows.Count > 0)
                        {
                            if (e.Column.ColumnName == "ItemVariant1Code")
                            {
                                e.Row["RecId"] = e.Row["RecId"];
                                e.Row["ItemVariant1Id"] = table.Rows[0]["RecId"];
                                e.Row["ItemVariant1Name"] = table.Rows[0]["ItemName"];
                                e.Row["ItemVariant1TypeId"] = table.Rows[0]["TypeId"];
                            }
                            else if (e.Column.ColumnName == "ItemVariant2Code")
                            {
                                e.Row["RecId"] = e.Row["RecId"];
                                e.Row["ItemVariant2Id"] = table.Rows[0]["RecId"];
                                e.Row["ItemVariant2Name"] = table.Rows[0]["ItemName"];
                                e.Row["ItemVariant2TypeId"] = table.Rows[0]["TypeId"];
                            }
                            else if (e.Column.ColumnName == "ItemVariant3Code")
                            {
                                e.Row["RecId"] = e.Row["RecId"];
                                e.Row["ItemVariant3Id"] = table.Rows[0]["RecId"];
                                e.Row["ItemVariant3Name"] = table.Rows[0]["ItemName"];
                                e.Row["ItemVariant3TypeId"] = table.Rows[0]["TypeId"];
                            }
                            else if (e.Column.ColumnName == "ItemVariant4Code")
                            {
                                e.Row["RecId"] = e.Row["RecId"];
                                e.Row["ItemVariant4Id"] = table.Rows[0]["RecId"];
                                e.Row["ItemVariant4Name"] = table.Rows[0]["ItemName"];
                                e.Row["ItemVariant4TypeId"] = table.Rows[0]["TypeId"];
                            }
                            else if (e.Column.ColumnName == "ItemVariant5Code")
                            {
                                e.Row["RecId"] = e.Row["RecId"];
                                e.Row["ItemVariant5Id"] = table.Rows[0]["RecId"];
                                e.Row["ItemVariant5Name"] = table.Rows[0]["ItemName"];
                                e.Row["ItemVariant5TypeId"] = table.Rows[0]["TypeId"];
                            }
                        }
                    }
                    _suppressEvents = false;
                }
                catch
                {
                    _suppressEvents = false;
                }
            }
        }

        public override void OnListCommand(ISysCommandParam obj)
        {
            var focusScope = FocusManager.GetFocusScope(ActiveViewControl);
            var element = FocusManager.GetFocusedElement(focusScope) as FrameworkElement;
            LiveGridControl grid = FrameworkTreeHelper.FindParent<LiveGridControl>(element);
            if (grid != null && grid.CurrentColumn != null && grid.Name == "gridDetail")
            {
                var gc = grid.CurrentColumn.FieldName;
                if (gc == "Variant1" || gc == "Variant2" || gc == "Variant3" || gc == "Variant4" || gc == "Variant5"
                 || gc == "ItemVariant1Code" || gc == "ItemVariant2Code" || gc == "ItemVariant3Code" || gc == "ItemVariant4Code" || gc == "ItemVariant5Code")
                {
                    if (gc == "ItemVariant1Code")
                    {
                        OpenWorkList("ItemVariant1Code", ActiveBO.ActiveSession.ParamService.GetParameterClass<InventoryParameters>().DefaultVariant1TypeCode);
                        (grid.View as TableView).MoveNextCell();
                        return;
                    }
                    else if (gc == "ItemVariant2Code")
                    {
                        OpenWorkList("ItemVariant2Code", ActiveBO.ActiveSession.ParamService.GetParameterClass<InventoryParameters>().DefaultVariant2TypeCode);
                        (grid.View as TableView).MoveNextCell();
                        return;
                    }
                    else if (gc == "ItemVariant3Code")
                    {
                        OpenWorkList("ItemVariant3Code", ActiveBO.ActiveSession.ParamService.GetParameterClass<InventoryParameters>().DefaultVariant3TypeCode);
                        (grid.View as TableView).MoveNextCell();
                        return;
                    }
                    else if (gc == "ItemVariant4Code")
                    {
                        OpenWorkList("ItemVariant4Code", ActiveBO.ActiveSession.ParamService.GetParameterClass<InventoryParameters>().DefaultVariant4TypeCode);
                        (grid.View as TableView).MoveNextCell();
                        return;
                    }
                    else if (gc == "ItemVariant5Code")
                    {
                        OpenWorkList("ItemVariant5Code", ActiveBO.ActiveSession.ParamService.GetParameterClass<InventoryParameters>().DefaultVariant5TypeCode);
                        (grid.View as TableView).MoveNextCell();
                        return;
                    }
                }
                else base.OnListCommand(obj);
            }
            else base.OnListCommand(obj);
        }

        void OpenWorkList(string returnRow, string whereId)
        {
            using (DataTable table = UtilityFunctions.GetDataTableList(ActiveBO.Provider, ActiveBO.Connection, ActiveBO.Transaction, "Erp_VariantType", $"select * from Erp_VariantType with (nolock) where CompanyId={ActiveBO.CompanyId} and TypeName='{whereId}'"))
            {
                if (table?.Rows.Count > 0)
                {
                    PolicyParams policyparam = new PolicyParams();
                    policyparam.WhereStr = "erp_variantcard.TypeId=" + table?.Rows[0]["RecId"].ToString();
                    policyparam.ResultFieldName = "Varyant Kodu";
                    SysMng.Instance.ActWndMng.ShowReport("Erp_VariantItemItemCodeList", true, this.ListValueHandler, new DlgArgs(returnRow/*"ItemVariant1Code"*/), null, policyparam, "WorkListW", ReportWorkMode.ChoseList);
                }
            }
        }

        public void ListValueHandler(DlgArgs result)
        {
            ActiveBO.CurrentRow[result.DlgInputValue.ToString()] = result.DlgReturnValue;
        }

        public void OnInventoryIAGLAccountListCommand(ISysCommandParam obj)
        {
            //SysMng.Instance.ActWndMng.ShowReport("Erp_GLAccountAccountCodeList", true, this.InventoryIAListValueHandler, new DlgArgs("GLAccountId"), null, null, "WorkListW", ReportWorkMode.WorkList);
        }

        public void InventoryIAListValueHandler(DlgArgs result)
        {
            DataRowView trw = (DataRowView)InventoryGroupIAGridViewSelectedItem;
            trw.Row[result.DlgInputValue as string] = result.DlgReturnValue;
            OnCloseCommand(null);
        }

        public bool CanInventoryIAGLAccountListCommand(ISysCommandParam arg)
        {
            return true;
        }

        void gridDetailPriceSale_CreatedNewRow(object sender, NewRowViewEventArgs e)
        {
            if (e?.View != null)
            {
                if (e.View.Row.Table.Columns.Contains("PriceType")) e.View["PriceType"] = 2;
                if (ActiveSession?.ActiveUser != null && ActiveSession.ActiveUser.AccessCodes.Count == 1)
                {
                    if (e.View.Row.Table.Columns.Contains("AccessCode")) e.View["AccessCode"] = ActiveSession.ActiveUser.AccessCodes[0].AccessCode;
                }
                if (e.View.Row.Table.Columns.Contains("VatIncluded"))
                {
                    e.View["VatIncluded"] = 0;
                    if (((InventoryParameters)ActiveBO.Parameters["InventoryParameters"]).DefaultSalesPriceVatIncluded == 1) e.View["VatIncluded"] = 1;
                }
                if (e.View.Row.Table.Columns.Contains("Priority")) e.View["Priority"] = e.View.Row.Table.AsEnumerable().Count(y => !y.IsNull("PriceType") && y.Field<byte>("PriceType") == 2) + 1;
            }
        }

        private void gridDetailPriceSale_BeforeCreateNewRow(object sender, LiveGridControl.BeforeCreateNewRowEventArgs e)
        {
            if (!SysMng.Instance.CheckRights(OperationType.Insert, (short)Modules.InventoryModule, (short)Modules.InventoryModule, (short)InventorySecurityItems.InventoryCard, (short)InventoryCardSubItems.SalesPriceDefinitions))
            {
                sysMng.ActWndMng.ShowMsg(SLanguage.GetString("Satış Fiyatı Ekleme Yetkiniz Bulunmamaktadır."), ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK);
                e.Cancel = true;
            }
        }

        private void gridDetailPriceSale_BeforeDeleteItem(object sender, LiveGridControl.BeforeDeleteItemEventArgs e)
        {
            if (sender is LiveGridControl && (sender as LiveGridControl).SelectedItem != null && !SysMng.Instance.CheckRights(OperationType.Delete, (short)Modules.InventoryModule, (short)Modules.InventoryModule, (short)InventorySecurityItems.InventoryCard, (short)InventoryCardSubItems.SalesPriceDefinitions))
            {
                sysMng.ActWndMng.ShowMsg(SLanguage.GetString("Satış Fiyatlarını Silme Yetkiniz Bulunmamaktadır."), ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK);
                e.Cancel = true;
            }
        }

        private void gridDetailPriceSale_CurrentColumnChanged(object sender, DevExpress.Xpf.Grid.CurrentColumnChangedEventArgs e)
        {
            if (e != null && e.OldColumn != null && gridDetailPrice != null && gridDetailPrice.SelectedItem != null && (gridDetailPrice.SelectedItem is DataRowView) && (gridDetailPrice.SelectedItem as DataRowView).Row != null
                && !SysMng.Instance.CheckRights(OperationType.Update, (short)Modules.InventoryModule, (short)Modules.InventoryModule, (short)InventorySecurityItems.InventoryCard, (short)InventoryCardSubItems.SalesPriceDefinitions))
            {
                sysMng.ActWndMng.ShowMsg(SLanguage.GetString("Satış Fiyatlarını Değiştirme Yetkiniz Bulunmamaktadır."), ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK);
            }
        }

        public override void Dispose()
        {
            if (disposed) return;
            base.Dispose();
        }

    }
}
