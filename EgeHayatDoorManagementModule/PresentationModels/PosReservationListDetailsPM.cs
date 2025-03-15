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
using Sentez.Core.ParameterClasses;
using DevExpress.Xpf.Grid;
using System.Text;
using Sentez.Data.BusinessObjects;
using Sentez.Common.ModuleBase;
using Sentez.Localization;
using Sentez.InventoryModule;
using Sentez.Common.SystemServices;
using Sentez.MetaPosModule.ParameterClasses;

namespace Sentez.EgeHayatDoorManagementModule.PresentationModels
{
    public partial class PosReservationListDetailsPM : PMDesktop
    {
        PosParameters _posParams;
        DateHelper _dateHelper;
        int warehouseId = 0;
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

        private DateTime _currentDate;
        public DateTime CurrentDate
        {
            get
            {
                return _currentDate;
            }
            set
            {
                _currentDate = value;
                OnPropertyChanged("CurrentDate");
            }
        }

        public PosReservationListDetailsPM(IContainerExtension container_)
            : base(container_)
        {
        }

        public override void LoadCommands()
        {
            base.LoadCommands();
            CmdList.AddCmd(301, "PrevDayCommand", SLanguage.GetString("Önceki Gün"), OnPrevDayCommand, CanPrevDayCommand);
            CmdList.AddCmd(301, "NextDayCommand", SLanguage.GetString("Sonraki Gün"), OnNextDayCommand, CanNextDayCommand);
        }

        private bool CanNextDayCommand(ISysCommandParam param)
        {
            return true;
        }

        private void OnNextDayCommand(ISysCommandParam param)
        {
            CurrentDate = CurrentDate.AddDays(1);
            GetPosReceipt(CurrentDate);
        }

        private bool CanPrevDayCommand(ISysCommandParam param)
        {
            return true;
        }

        private void OnPrevDayCommand(ISysCommandParam param)
        {
            CurrentDate = CurrentDate.AddDays(-1);
            GetPosReceipt(CurrentDate);
        }

        public override void Init()
        {
            base.Init();
            Lists = ActiveSession.LookupList.GetChild(UtilityFunctions.GetConnection(ActiveSession.dbInfo.DBProvider, ActiveSession.dbInfo.ConnectionString));
            CurrentDate = DateTime.Now.Date;
            if (ActiveBO != null)
            {
                using (DataTable table = UtilityFunctions.GetDataTableList(ActiveBO.Provider, ActiveBO.Connection, ActiveBO.Transaction, "", $"select * from Erp_Warehouse with (nolock) where CompanyId={ActiveBO.CompanyId} and IsNull(InUse,0)=1"))
                {
                    if (table?.Rows.Count > 0)
                    {
                        warehouseId = Convert.ToInt32(table.Rows[0]["RecId"].ToString());
                    }
                }

                _posParams = ActiveBO.GetParameterClass("PosParameters") as PosParameters;
                _dateHelper = new DateHelper
                {
                    OperationMode = OperationMode.UserParameterMode,
                    Container = _container
                };
                CurrentDate = (DateTime)GetToday();
                GetPosReceipt(CurrentDate);
                ActiveBO.ColumnChanged += ActiveBO_ColumnChanged;
                ActiveBO.PreBeforePost += ActiveBO_PreBeforePost;
                ActiveBO.AfterSucceededPost += ActiveBO_AfterSucceededPost;
                ActiveBO.PropertyChanged += ActiveBO_PropertyChanged;
                (ActiveBO as BusinessObjectBase).ValueFiller.RemoveRule("Erp_Pos", "ReceiptDate");
                (ActiveBO as BusinessObjectBase).ValueFiller.AddRule("Erp_Pos", "ReceiptDate", CurrentDate);
                if (ActiveBO.CurrentRow.Row.RowState == DataRowState.Added)
                    ActiveBO.CurrentRow["ReceiptDate"] = CurrentDate;

            }
            gridDetailPrice = FCtrl("gridDetailPrice") as LiveGridControl;
            gridDetailPriceSale = FCtrl("gridDetail") as LiveGridControl;
            if (gridDetailPriceSale != null)
            {
                gridDetailPriceSale.GetCustomColumns();
                gridDetailPriceSale.CreatedNewRow += gridDetailPriceSale_CreatedNewRow;
                gridDetailPriceSale.BeforeCreateNewRow += gridDetailPriceSale_BeforeCreateNewRow;
                gridDetailPriceSale.BeforeDeleteItem += gridDetailPriceSale_BeforeDeleteItem;
                gridDetailPriceSale.CurrentColumnChanged += gridDetailPriceSale_CurrentColumnChanged;
            }
        }

        private void ActiveBO_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentRow")
            {
                if (ActiveBO?.CurrentRow != null)
                {
                    if (ActiveBO.CurrentRow.Row.RowState == DataRowState.Added)
                        ActiveBO.CurrentRow["ReceiptDate"] = CurrentDate;
                }
            }
        }

        private void ActiveBO_AfterSucceededPost(object sender, EventArgs e)
        {
        }

        private void ActiveBO_PreBeforePost(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (DataRow row in ActiveBO.Data.Tables[ActiveBO.BaseTable].Select("", "", DataViewRowState.CurrentRows))
            {
                Guid guid = Guid.NewGuid();
                if (string.IsNullOrEmpty(row["ReceiptNo"].ToString()))
                    row["ReceiptNo"] = guid.ToString().Substring(0, 25);
                if (string.IsNullOrEmpty(row["CashRegisterReceiptNo"].ToString()))
                    row["CashRegisterReceiptNo"] = guid.ToString().Substring(0, 30);
            }
        }

        private void GetPosReceipt(DateTime selDate)
        {
            ActiveBO.GetAll(new WhereField(WhereFieldType.And, new WhereField[]{
                new WhereField(ActiveBO.BaseTable,"CompanyId",ActiveBO.CompanyId, WhereCondition.Equal),
                new WhereField(ActiveBO.BaseTable,"ReceiptDate",selDate, WhereCondition.Equal),
                new WhereField(ActiveBO.BaseTable,"TransactionType",(short)PosSalesTypeDefinition.PosTransactionType.Reservation, WhereCondition.Equal),
                new WhereField(ActiveBO.BaseTable,"ReceiptType",(short)PosSalesTypeDefinition.PosReceiptType.Sales, WhereCondition.Equal),
                new WhereField(ActiveBO.BaseTable,"SalesType",(short)PosSalesTypeDefinition.PosSalesType.IsReservation, WhereCondition.Equal)
                }));
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
                e.View["ReceiptDate"] = CurrentDate;
                e.View["TransactionType"] = (short)PosSalesTypeDefinition.PosTransactionType.Reservation;
                e.View["ReceiptType"] = (short)PosSalesTypeDefinition.PosReceiptType.Sales;
                e.View["SalesType"] = (short)PosSalesTypeDefinition.PosSalesType.IsReservation;
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
            if (sender is LiveGridControl && (sender as LiveGridControl).SelectedItem != null && !SysMng.Instance.CheckRights(Common.OperationType.Delete, (short)Modules.InventoryModule, (short)Modules.InventoryModule, (short)InventorySecurityItems.InventoryCard, (short)InventoryCardSubItems.SalesPriceDefinitions))
            {
                sysMng.ActWndMng.ShowMsg(SLanguage.GetString("Satış Fiyatlarını Silme Yetkiniz Bulunmamaktadır."), ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK);
                e.Cancel = true;
            }
        }

        private void gridDetailPriceSale_CurrentColumnChanged(object sender, DevExpress.Xpf.Grid.CurrentColumnChangedEventArgs e)
        {
            if (e != null && e.OldColumn != null && gridDetailPrice != null && gridDetailPrice.SelectedItem != null && (gridDetailPrice.SelectedItem is DataRowView) && (gridDetailPrice.SelectedItem as DataRowView).Row != null
                && !SysMng.Instance.CheckRights(Common.OperationType.Update, (short)Modules.InventoryModule, (short)Modules.InventoryModule, (short)InventorySecurityItems.InventoryCard, (short)InventoryCardSubItems.SalesPriceDefinitions))
            {
                sysMng.ActWndMng.ShowMsg(SLanguage.GetString("Satış Fiyatlarını Değiştirme Yetkiniz Bulunmamaktadır."), ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK);
            }
        }


        private object GetToday()
        {
            if (_posParams.OtelApp && _posParams.OtelToDayRetail)
            {
                if (_dateHelper != null)
                {
                    _dateHelper.OperationMode = OperationMode.AgileMode;
                    return _dateHelper.GetToday(ActiveBO.Transaction);
                }
                return _dateHelper.GetToday();
            }
            return _dateHelper.GetToday();
        }
        private object GetCreateTime()
        {
            return new DateTime(1899, 12, 30, DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, DateTime.Now.TimeOfDay.Seconds);
        }

        public override void Dispose()
        {
            if (disposed) return;
            if (gridDetailPriceSale != null)
            {
                gridDetailPriceSale.CreatedNewRow -= gridDetailPriceSale_CreatedNewRow;
                gridDetailPriceSale.BeforeCreateNewRow -= gridDetailPriceSale_BeforeCreateNewRow;
                gridDetailPriceSale.BeforeDeleteItem -= gridDetailPriceSale_BeforeDeleteItem;
                gridDetailPriceSale.CurrentColumnChanged -= gridDetailPriceSale_CurrentColumnChanged;
            }

            base.Dispose();
        }
    }
}
