using LiveCore.Desktop.UI.Controls;
using Microsoft.Practices.Unity;
using Sentez.Common.Commands;
using Sentez.Common.PresentationModels;
using Sentez.Common.SystemServices;
using Sentez.Common.Utilities;
using Sentez.Core.ParameterClasses;
using Sentez.Data.BusinessObjects;
using Sentez.Data.MetaData;
using Sentez.Data.Query;
using Sentez.Data.Tools;
using Sentez.Localization;
using Sentez.VModule.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using static Sentez.VModule.Services.ActualCostModelService;

namespace Sentez.NermaMetalManagementModule.PresentationModels
{
    public partial class CollectiveActualCostPM : ReportPM
    {
        #region Properties
        Dictionary<String, String> ImportCostNames = new Dictionary<String, String>();
        List<String> totalCostColList = new List<string>();
        List<String> totalCostColEuroList = new List<string>();
        List<String> totalCostColUSDList = new List<string>();
        public VogueParameters wparams;
        public GeneralParameters gparams;
        IBusinessObject actualCostBO = null;
        string OrderNo, QualityCode;
        int amountDec = 0, RateId;
        decimal OrderQuantity = 0M, ShipQuantity = 0M, CutQuantity = 0M, MonthlyShipQuantity = 0M, TotalMonthlyShipQty = 0M, RemainQuantity = 0M;
        decimal TotalCost = 0M, BeforeMonthlyCost = 0M, TotalSalesCost = 0M;
        decimal MonthlyUnitPriceCost = 0M, MonthlyCost = 0M, MonthlyProfit = 0M, MonthlySalesTotal = 0M;
        decimal TotalForexCostEUR = 0M, TotalSalesForexCostEUR = 0M, MonthlyUnitPriceForexCostEUR = 0M, MonthlyForexCostEUR, MonthlySalesTotalForexEUR, MonthlyForexProfitEUR, BeforeMonthlyForexCostEUR;
        decimal TotalForexCostUSD = 0M, TotalSalesForexCostUSD = 0M, MonthlyUnitPriceForexCostUSD = 0M, MonthlyForexCostUSD, MonthlySalesTotalForexUSD, MonthlyForexProfitUSD, BeforeMonthlyForexCostUSD;
        public LookupList Lists { get; set; }
        public LiveGridControl grdPieceWorkDetail;

        ActualCostModelService _actualCostCalculate;
        ActualCostModelService ActualCostCalculate
        {
            get { return _actualCostCalculate; }
            set { _actualCostCalculate = value; OnPropertyChanged("ActualCostCalculate"); }
        }

        LiveGridControl grdActualCost;

        public DataTable OrderTable;
        public DataTable Muh2KTable;
        public DataTable CostDifferenceTable;

        private DateTime costDate = DateTime.Today;
        public DateTime CostDate
        {
            get { return costDate; }
            set { costDate = value; OnPropertyChanged("CostDate"); }
        }
        private DateTime woStartDate;
        public DateTime WOStartDate
        {
            get { return woStartDate; }
            set { woStartDate = value; OnPropertyChanged("WOStartDate"); }
        }
        private DateTime woFinishDate;
        public DateTime WOFinishDate
        {
            get { return woFinishDate; }
            set { woFinishDate = value; OnPropertyChanged("WOFinishDate"); }
        }
        private bool calculateTerm = false;
        public bool CalculateTerm
        {
            get { return calculateTerm; }
            set { calculateTerm = value; OnPropertyChanged("CalculateTerm"); }
        }
        private DataView orderDataView;

        public DataView OrderDataView
        {
            get { return orderDataView; }
            set { orderDataView = value; OnPropertyChanged("OrderDataView"); }
        }
        private DataTable tempCostTable;

        public DataTable TempCostTable
        {
            get { return tempCostTable; }
            set
            {
                tempCostTable = value;
                OnPropertyChanged("TempCostTable");
            }
        }
        object orderSelectedItem;

        public object OrderSelectedItem
        {
            get { return orderSelectedItem; }
            set
            {
                orderSelectedItem = value;
                OnPropertyChanged("OrderSelectedItem");
            }
        }
        private DataTable pieceWorkDetailTable;
        public DataTable PieceWorkDetailTable
        {
            get { return pieceWorkDetailTable; }
            set
            {
                pieceWorkDetailTable = value;
                OnPropertyChanged("PieceWorkDetailTable");
            }
        }
        private object pieceWorkDetailSelectedItem;
        public object PieceWorkDetailSelectedItem
        {
            get { return pieceWorkDetailSelectedItem; }
            set
            {
                pieceWorkDetailSelectedItem = value;
                OnPropertyChanged("PieceWorkDetailSelectedItem");
            }
        }
        private ReceiptColumnCollection actualCostColumnCollection;
        public ReceiptColumnCollection ActualCostColumnCollection
        {
            get { return actualCostColumnCollection; }
            set
            {
                actualCostColumnCollection = value;
                OnPropertyChanged("ActualCostColumnCollection");
            }
        }
        private ReceiptColumnCollection pieceWorkDetailColumnCollection;
        public ReceiptColumnCollection PieceWorkDetailColumnCollection
        {
            get { return pieceWorkDetailColumnCollection; }
            set
            {
                pieceWorkDetailColumnCollection = value;
                OnPropertyChanged("PieceWorkDetailColumnCollection");
            }
        }

        #endregion

        public CollectiveActualCostPM(IUnityContainer container_)
            : base(container_)
        {
            gparams = sysMng.getSession().ParamService.GetParameterClass<GeneralParameters>();
            wparams = sysMng.getSession().ParamService.GetParameterClass<VogueParameters>();
            amountDec = gparams.AmountDec;
            //deneme
        }

        public override void LoadCommands()
        {
            base.LoadCommands();
            CmdList.AddCmd(501, "RefreshCommand", SLanguage.GetString("Yenile"), OnRefreshCommand, null);
            CmdList.AddCmd(502, "SaveActualCostCommand", SLanguage.GetString("Kaydet"), OnSaveActualCostCommand, null);
        }
        public override void Init()
        {
            base.Init();
            Lists = ActiveSession.LookupList.GetChild(UtilityFunctions.GetConnection(ActiveSession.dbInfo.DBProvider, ActiveSession.dbInfo.ConnectionString));

            if (ActiveBO != null)
                ActiveBO.Init(new BoParam());
            actualCostBO = _container.Resolve<IBusinessObject>("ActualCostBO");
            actualCostBO.Init(new BoParam());
            if (ActualCostCalculate == null) ActualCostCalculate = _container.Resolve<ActualCostModelService>();
            if (grdActualCost == null) grdActualCost = FCtrl("grdActualCost") as LiveGridControl;
            if (grdActualCost != null)
            {
                grdActualCost.MouseDoubleClick += GrdActualCost_MouseDoubleClick;
                grdActualCost.CurrentItemChanged += GrdActualCost_CurrentItemChanged;
            }
            if (grdPieceWorkDetail == null) grdPieceWorkDetail = FCtrl("grdPieceWorkDetail") as LiveGridControl;
            InitializeActualCostGrid();
            TempCostTable = CreateTempCostTable();
            InitializePieceWorkGrid();
            FillImportDepertmantNames();
        }
        private void GrdActualCost_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (grdActualCost?.View != null && OrderSelectedItem != null && OrderSelectedItem is DataRowView)
            {
                OrderNo = (OrderSelectedItem as DataRowView).Row[SLanguage.GetString("WorkOrderNo")].ToString();
                PmParam pmparam = new PmParam("VCMMonthlyActualCostPM", "BOCardContext");
                pmparam.Tag = OrderNo;
                pmparam.Tag2 = CostDate;
                BoParam boparam2 = new BoParam();
                SysCommandParam prm = new SysCommandParam("VCMMonthlyActualCost", "VCMMonthlyActualCostPM", pmparam, "", boparam2, SLanguage.GetString("Aylık Gerçek Maliyet"), "") { isModal = true };
                SysMng.Instance.GetCmd("CmdGeneralOpen").Execute(prm);
            }
        }
        private void OnRefreshCommand(ISysCommandParam obj)
        {
            if (ActiveBO.GetAll(new WhereField[] { new WhereField("Erp_ActualCost", "Year", CostDate.Year, WhereCondition.Equal), new WhereField("Erp_ActualCost", "Month", CostDate.Month, WhereCondition.Equal) }) > 0)
            {
                SysMng.ActWndMng.ShowMsg(SLanguage.GetString("Bu Aya Ait Kayıtlı Order Maliyeti Mevcut. Ekranda bu bilgileri göreceksiniz."), ConstantStr.Information);
                try
                {
                    SysMng.ShowWaitCursor();
                    TempCostTable.Clear();
                    DataRow newRow;
                    foreach (DataRow actRow in ActiveBO.Data.Tables["Erp_ActualCost"].Rows)
                    {
                        newRow = TempCostTable.NewRow();
                        TempCostTable.Rows.Add(newRow);
                        foreach (DataColumn clmview in TempCostTable.Columns)
                        {
                            foreach (DataColumn columnsBo in ActiveBO.Data.Tables["Erp_ActualCost"].Columns)
                            {
                                if (columnsBo.ColumnName.Contains(clmview.ColumnName.ToString()))
                                    newRow[clmview.ColumnName] = actRow[clmview.ColumnName];
                            }
                        }
                        newRow["IsClosed"] = actRow["ApprovedExplanation"];
                        newRow["MonthlyForexCostEUR"] = actRow["MonthlyForexCost"];
                        newRow["MonthlyForexCostUSD"] = actRow["MonthlyForex2Cost"];
                        newRow["MonthlySalesForexCostEUR"] = actRow["MonthlySalesForexCost"];
                        newRow["MonthlySalesForexCostUSD"] = actRow["MonthlySalesForex2Cost"];
                        newRow["MonthlyUnitPriceForexCostEUR"] = actRow["MonthlyUnitPriceForexCost"];
                        newRow["MonthlyUnitPriceForexCostUSD"] = actRow["MonthlyUnitPriceForex2Cost"];
                        newRow["MonthlyForexProfitEUR"] = actRow["MonthlyForexProfit"];
                        newRow["MonthlyForexProfitUSD"] = actRow["MonthlyForex2Profit"];
                        newRow["TotalSalesForexCostEUR"] = actRow["TotalSalesForexCost"];
                        newRow["TotalSalesForexCostUSD"] = actRow["TotalSalesForex2Cost"];
                        newRow["TotalForexCostEUR"] = actRow["TotalForexCost"];
                        newRow["TotalForexCostUSD"] = actRow["TotalForex2Cost"];
                        newRow["StyleCode"] = actRow["InventoryCode"];
                        if (actRow.IsNull("WorkOrderNo"))
                            newRow["WorkOrderNo"] = "Muhtelif Satışlar";
                    }
                    DataRow newPRow = null;
                    PieceWorkDetailTable = new DataTable();
                    InitializePieceWorkDataColumns();
                    foreach (DataRow actRow in ActiveBO.Data.Tables["Erp_ActualCostProcessDetail"].Rows)
                    {
                        DataTable dtOrder = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_Process", $"Select ewo.WorkOrderNo FROM Erp_WorkOrder ewo WHERE RecId IN (Select eac.WorkOrderId FROM Erp_ActualCost eac WHERE eac.RecId = {actRow["ActualCostId"]})");
                        DataTable dtProcess = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_Process", $"Select ProcessName from Erp_Process where RecId = {actRow["ProcessId"]}");
                        DataTable dtApprovedExplanation = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_ActualCost", $"Select ApprovedExplanation from Erp_ActualCost where RecId = {actRow["ActualCostId"]}");
                        newPRow = PieceWorkDetailTable.NewRow();
                        PieceWorkDetailTable.Rows.Add(newPRow);
                        if (dtOrder != null && dtOrder.Rows.Count > 0)
                            newPRow["WorkOrderNo"] = dtOrder.Rows[0]["WorkOrderNo"];
                        if (dtProcess != null && dtProcess.Rows.Count > 0)
                            newPRow["ProcessName"] = dtProcess.Rows[0]["ProcessName"];
                        newPRow["TotalAmount"] = actRow["TotalAmount"];
                        newPRow["OrderStatus"] = dtApprovedExplanation.Rows[0]["ApprovedExplanation"];
                        newPRow["TotalForexAmountEUR"] = actRow["TotalForexAmountEUR"];
                        newPRow["TotalForexAmountUSD"] = actRow["TotalForexAmountUSD"];
                        newPRow["MonthlyAmount"] = actRow["MonthlyAmount"];
                        newPRow["MonthlyForexAmountEUR"] = actRow["MonthlyForexAmountEUR"];
                        newPRow["MonthlyForexAmountUSD"] = actRow["MonthlyForexAmountUSD"];
                    }
                    return;
                }
                finally
                {
                    SysMng.ShowArrowCursor();
                }
            }
            try
            {
                sysMng.ShowWaitCursor();
                ActualCost();
            }
            catch (Exception ex)
            {
                SysMng.ActWndMng.ShowMsg(SLanguage.GetString(ex.Message), ConstantStr.Warning);
            }
            finally
            {
                sysMng.ShowArrowCursor();
            }
        }
        private void ActualCost()
        {
            OrderTable = GetWorkOrderList();
            WOStartDate = new DateTime(CostDate.Year, CostDate.Month, 1);
            WOFinishDate = WOStartDate.AddMonths(1).AddDays(-1);
            DataSet resultTotal = null;
            TempCostTable.Clear();

            #region 1. Kalite ve Normal Order İşlemleri
            if (OrderTable?.Rows?.Count > 0)
            {
                foreach (DataRow orderRow in OrderTable.Select("WorkOrderNo is not null"))
                {
                    using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
                    {
                        QualityCode = "1";
                        resultTotal = (DataSet)ActualCostCalculate.Execute(orderRow["WorkOrderNo"], orderRow["StyleCode"], WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 1, DateTime.MinValue, QualityCode);
                        if (resultTotal is DataSet && resultTotal != null && resultTotal.Tables.Contains("CostItemTable") && resultTotal.Tables["CostItemTable"].Rows.Count > 0)
                        {
                            DataTable inTable = (resultTotal as DataSet).Tables["CostItemTable"];
                            DataRow newRow = null;
                            string oldWorkOrderNo = string.Empty;
                            foreach (DataRow inRow in inTable.Select("ParentId = 0", "WorkOrderNo"))
                            {
                                if (inRow.IsNull("WorkOrderNo")) continue;
                                if (!inRow["WorkOrderNo"].ToString().Equals(oldWorkOrderNo) && !(TempCostTable.Select($"WorkOrderId = {orderRow["WorkOrderId"]}").Length > 0))
                                {
                                    newRow = TempCostTable.NewRow();
                                    TempCostTable.Rows.Add(newRow);

                                    SetDefaultColumns(ActualCostCalculate, newRow, orderRow, inRow);
                                    newRow["WorkOrderItemId"] = orderRow["WorkOrderItemId"];

                                    CalcTotalandMonthlyValues(orderRow);
                                    if (newRow != null)
                                    {
                                        //İçerde Yapılan İşçiliklerin Maliyet Sütunları
                                        SetImportDepartmentCost(newRow, inRow["WorkOrderNo"], orderRow["StyleCode"]/*, QualityCode*/);
                                    }
                                    #region Birim Maliyet(MonthlyUnitPriceCost)-Dönem Maliyeti(monthlyCost)-Kar/Zarar Hesaplamaları
                                    DateTime shipmentDate = DateTime.MinValue;
                                    if (orderRow["ShipmentDate"] != DBNull.Value)
                                        shipmentDate = Convert.ToDateTime(orderRow["ShipmentDate"]);

                                    //Daha önceki ayların maliyet kayıtları
                                    StringBuilder str = new StringBuilder();
                                    str.AppendLine($"Select eac.MonthlyCost,eac.MonthlyForexCost,eac.MonthlyForex2Cost,eac.TotalCost,eac.MonthlyShipmentQuantity " +
                                        @",eac.MonthlyYarnCostAmount2 ,eac.MonthlyYarnDyeCostAmount2 ,eac.MonthlyFabricCostAmount2 ,eac.MonthlyFabricDyeCostAmount2
                                             ,eac.MonthlyFabricPrintCostAmount2 ,eac.MonthlyFabricOther1CostAmount2 ,eac.MonthlyFabricOther2CostAmount2 ,eac.MonthlyTrimCostAmount2
                                             ,eac.MonthlyReturnCostAmount2 ,eac.MonthlyCutCostAmount2 ,eac.MonthlyPieceWorkCostAmount2 ,eac.MonthlyChemicalCostAmount2 ,eac.MonthlyYarnCostAmount2EUR
                                             ,eac.MonthlyYarnDyeCostAmount2EUR ,eac.MonthlyFabricCostAmount2EUR ,eac.MonthlyFabricDyeCostAmount2EUR ,eac.MonthlyFabricPrintCostAmount2EUR 
                                             ,eac.MonthlyFabricOther1CostAmount2EUR ,eac.MonthlyFabricOther2CostAmount2EUR ,eac.MonthlyTrimCostAmount2EUR ,eac.MonthlyReturnCostAmount2EUR
                                             ,eac.MonthlyCutCostAmount2EUR ,eac.MonthlyPieceWorkCostAmount2EUR ,eac.MonthlyChemicalCostAmount2EUR ,eac.MonthlyYarnCostAmount2USD
                                             ,eac.MonthlyYarnDyeCostAmount2USD ,eac.MonthlyFabricCostAmount2USD ,eac.MonthlyFabricDyeCostAmount2USD ,eac.MonthlyFabricPrintCostAmount2USD
                                             ,eac.MonthlyFabricOther1CostAmount2USD ,eac.MonthlyFabricOther2CostAmount2USD ,eac.MonthlyTrimCostAmount2USD ,eac.MonthlyReturnCostAmount2USD
                                             ,eac.MonthlyCutCostAmount2USD ,eac.MonthlyPieceWorkCostAmount2USD ,eac.MonthlyChemicalCostAmount2USD " +
                                        $"  from Erp_ActualCost eac where eac.CompanyId = {activeSession.ActiveCompany.RecId} and eac.Year = {CostDate.Year} and eac.Month < {CostDate.Month} " +
                                                $" and (Select WorkOrderNo from Erp_WorkOrder wo with (nolock) where RecId in (Select WorkOrderId from Erp_WorkOrderItem woi where woi.RecId = WorkOrderItemId )) = '{orderRow["WorkOrderNo"]}' order by eac.Month desc");
                                    DataTable actualDt = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_ActualCost", str.ToString());
                                    if (actualDt != null && actualDt.Rows.Count > 0)
                                    {
                                        BeforeMonthlyCost = (from DataRow rRow in actualDt.Rows where !rRow.IsNull("MonthlyCost") select Convert.ToDecimal(rRow["MonthlyCost"])).Sum();
                                        BeforeMonthlyForexCostEUR = (from DataRow rRow in actualDt.Rows where !rRow.IsNull("MonthlyForexCost") select Convert.ToDecimal(rRow["MonthlyForexCost"])).Sum();
                                        BeforeMonthlyForexCostUSD = (from DataRow rRow in actualDt.Rows where !rRow.IsNull("MonthlyForex2Cost") select Convert.ToDecimal(rRow["MonthlyForex2Cost"])).Sum();
                                        TotalMonthlyShipQty = (from DataRow rRow in actualDt.Rows where !rRow.IsNull("MonthlyForexCost") select Convert.ToDecimal(rRow["MonthlyShipmentQuantity"])).Sum();
                                    }

                                    newRow["TotalForexCostEUR"] = TotalForexCostEUR;
                                    newRow["TotalForexCostUSD"] = TotalForexCostUSD;
                                    newRow["MonthlyShipmentQuantity"] = MonthlyShipQuantity;
                                    newRow["MonthlySalesForexCostUSD"] = MonthlySalesTotalForexUSD;
                                    newRow["MonthlySalesForexCostEUR"] = MonthlySalesTotalForexEUR;
                                    newRow["MonthlySalesCost"] = MonthlySalesTotal;
                                    newRow["TotalSalesCost"] = TotalSalesCost;
                                    newRow["TotalSalesForexCostEUR"] = TotalSalesForexCostEUR;
                                    newRow["TotalSalesForexCostUSD"] = TotalSalesForexCostUSD;
                                    newRow["ShipmentQuantity"] = ShipQuantity;

                                    RemainQuantity = OrderQuantity - ShipQuantity;
                                    if (RemainQuantity < 0) RemainQuantity = 0;
                                    newRow["RemainingQuantity"] = RemainQuantity;

                                    #region Ekstra Yapılan (İstisnai Durumlar İçin) Kontroller
                                    //Maliyeti olmadan direkt satışı yapılan veya maliyeti daha önce yedirilmiş olan Order(%100 kar)
                                    if (MonthlyShipQuantity > 0 && (TotalCost == 0 || (BeforeMonthlyCost) == TotalCost))
                                    {
                                        MonthlyUnitPriceCost = 0;
                                        MonthlyUnitPriceForexCostEUR = 0;
                                        MonthlyUnitPriceForexCostUSD = 0;
                                        MonthlyCost = 0;
                                        MonthlyForexCostEUR = 0;
                                        MonthlyForexCostUSD = 0;
                                        MonthlyProfit = MonthlySalesTotal;
                                        MonthlyForexProfitEUR = MonthlySalesTotalForexEUR;
                                        MonthlyForexProfitUSD = MonthlySalesTotalForexUSD;
                                        //Dahili Maliyet Dönem Sütunları
                                        foreach (var item in ImportCostNames)
                                        {
                                            if (item.Value.Contains("MonthlyCost"))
                                                newRow[item.Key] = 0;
                                        }
                                        foreach (var item in ImportCostNames)
                                        {
                                            if (item.Value.Contains("MonthlyEUR"))
                                                newRow[item.Key] = 0;
                                        }
                                        foreach (var item in ImportCostNames)
                                        {
                                            if (item.Value.Contains("MonthlyUSD"))
                                                newRow[item.Key] = 0;
                                        }
                                        newRow["IsClosed"] = "KZ(%100 Kar)";
                                    }
                                    #endregion

                                    if (string.IsNullOrEmpty(newRow["IsClosed"].ToString()))
                                    {
                                        #region Açık Order
                                        if (shipmentDate == DateTime.MinValue)
                                        {
                                            CalcOpenOrder();
                                            CalcImportDepartmentCostsForOpenOrder(newRow);
                                            newRow["IsClosed"] = "Açık-YT Boş";
                                        }
                                        #endregion
                                        #region Kapalı Order
                                        if (shipmentDate != DateTime.MinValue)
                                        {
                                            if (CostDate.Month < shipmentDate.Month && DoubleUtil.CompareFloat("<", (TotalMonthlyShipQty + MonthlyShipQuantity), ShipQuantity, FieldUsage.Quantity)) //Kapalı olmasına rağmen Açık Order mantığı çalışıyor
                                            {
                                                CalcOpenOrder();
                                                CalcImportDepartmentCostsForOpenOrder(newRow);
                                                newRow["IsClosed"] = "Açık-Kapalı ama ÇT<YT";
                                            }
                                            else // Kapalı Order
                                            {
                                                if (DoubleUtil.CompareFloat("=", ShipQuantity, MonthlyShipQuantity, FieldUsage.Quantity)) //Daha önce yüklemesi yoksa
                                                {
                                                    //(Birim Fiyatlar bilgi amaçlıdır)
                                                    MonthlyUnitPriceCost = TotalCost / ShipQuantity;
                                                    MonthlyUnitPriceForexCostEUR = TotalForexCostEUR / ShipQuantity;
                                                    MonthlyUnitPriceForexCostUSD = TotalForexCostUSD / ShipQuantity;
                                                    MonthlyCost = TotalCost;
                                                    MonthlyForexCostEUR = TotalForexCostEUR;
                                                    MonthlyForexCostUSD = TotalForexCostUSD;
                                                    //Dahili Maliyet Dönem Sütunları
                                                    foreach (DataColumn dtcol in tempCostTable.Columns)
                                                    {
                                                        if (totalCostColList.Contains(dtcol.ColumnName))
                                                            newRow[$"Monthly{dtcol.ColumnName}"] = newRow[dtcol.ColumnName];
                                                        if (totalCostColEuroList.Contains(dtcol.ColumnName))
                                                            newRow[$"Monthly{dtcol.ColumnName}"] = newRow[dtcol.ColumnName];
                                                        if (totalCostColUSDList.Contains(dtcol.ColumnName))
                                                            newRow[$"Monthly{dtcol.ColumnName}"] = newRow[dtcol.ColumnName];
                                                    }
                                                    newRow["IsClosed"] = "Kapalı-Tek Yükleme";
                                                }
                                                else //Yüklemesi bir ayda bitmemiş
                                                {
                                                    if (TotalMonthlyShipQty == 0) //Daha önceki aylarda yüklemesi yok ve toplam yüklenen dönem yüklenenden farklı
                                                    {
                                                        CalcOpenOrder();
                                                        CalcImportDepartmentCostsForOpenOrder(newRow);
                                                        newRow["IsClosed"] = "Açık-Kapalı ama İleri Yüklemesi Var";
                                                    }
                                                    else //Daha önceki aylarda yüklemesi varsa (Birim Fiyatlar bilgi amaçlıdır)
                                                    {
                                                        if ((TotalMonthlyShipQty + MonthlyShipQuantity) == ShipQuantity)
                                                        {
                                                            MonthlyUnitPriceCost = TotalCost / ShipQuantity;
                                                            MonthlyUnitPriceForexCostEUR = TotalForexCostEUR / ShipQuantity;
                                                            MonthlyUnitPriceForexCostUSD = TotalForexCostUSD / ShipQuantity;
                                                            MonthlyUnitPriceCost = Math.Round(MonthlyUnitPriceCost, amountDec);
                                                            MonthlyUnitPriceForexCostEUR = Math.Round(MonthlyUnitPriceForexCostEUR, amountDec);
                                                            MonthlyUnitPriceForexCostUSD = Math.Round(MonthlyUnitPriceForexCostUSD, amountDec);
                                                            MonthlyCost = TotalCost - BeforeMonthlyCost;
                                                            MonthlyForexCostEUR = TotalForexCostEUR - BeforeMonthlyForexCostEUR;
                                                            MonthlyForexCostUSD = TotalForexCostUSD - BeforeMonthlyForexCostUSD;
                                                            // Dahili Maliyet Dönem Sütunları
                                                            foreach (DataColumn dtcol in tempCostTable.Columns)
                                                            {
                                                                if (totalCostColList.Contains(dtcol.ColumnName))
                                                                {
                                                                    decimal importTotalCost = 0M, beforeImportMonthlyCost = 0;
                                                                    decimal.TryParse(newRow[dtcol.ColumnName].ToString(), out importTotalCost);
                                                                    beforeImportMonthlyCost = (from DataRow rRow in actualDt.Rows where !rRow.IsNull($"Monthly{dtcol.ColumnName}") select Convert.ToDecimal(rRow[$"Monthly{dtcol.ColumnName}"])).Sum();
                                                                    newRow[$"Monthly{dtcol.ColumnName}"] = importTotalCost - beforeImportMonthlyCost;
                                                                }
                                                            }
                                                            foreach (DataColumn dtcol in tempCostTable.Columns)
                                                            {
                                                                if (totalCostColEuroList.Contains(dtcol.ColumnName))
                                                                {
                                                                    decimal importTotalCostEUR = 0M, beforeImportMonthlyCostEUR = 0;
                                                                    decimal.TryParse(newRow[dtcol.ColumnName].ToString(), out importTotalCostEUR);
                                                                    beforeImportMonthlyCostEUR = (from DataRow rRow in actualDt.Rows where !rRow.IsNull($"Monthly{dtcol.ColumnName}") select Convert.ToDecimal(rRow[$"Monthly{dtcol.ColumnName}"])).Sum();
                                                                    newRow[$"Monthly{dtcol.ColumnName}"] = importTotalCostEUR - beforeImportMonthlyCostEUR;
                                                                }
                                                            }
                                                            foreach (DataColumn dtcol in tempCostTable.Columns)
                                                            {
                                                                if (totalCostColUSDList.Contains(dtcol.ColumnName))
                                                                {
                                                                    decimal importTotalCostUSD = 0M, beforeImportMonthlyCostUSD = 0;
                                                                    decimal.TryParse(newRow[dtcol.ColumnName].ToString(), out importTotalCostUSD);
                                                                    beforeImportMonthlyCostUSD = (from DataRow rRow in actualDt.Rows where !rRow.IsNull($"Monthly{dtcol.ColumnName}") select Convert.ToDecimal(rRow[$"Monthly{dtcol.ColumnName}"])).Sum();
                                                                    newRow[$"Monthly{dtcol.ColumnName}"] = importTotalCostUSD - beforeImportMonthlyCostUSD;
                                                                }
                                                            }
                                                            newRow["IsClosed"] = "Kapalı-Bu Ay Son Yükleme";
                                                        }
                                                        else
                                                        {
                                                            CalcOpenOrder();
                                                            CalcImportDepartmentCostsForOpenOrder(newRow);
                                                            newRow["IsClosed"] = "Açık-Kapalı ama Hem Eski Hem İleri Yüklemesi Var";
                                                        }
                                                    }
                                                }
                                                //Kar-Zarar
                                                MonthlyProfit = MonthlySalesTotal - MonthlyCost;
                                                MonthlyForexProfitEUR = MonthlySalesTotalForexEUR - MonthlyForexCostEUR;
                                                MonthlyForexProfitUSD = MonthlySalesTotalForexUSD - MonthlyForexCostUSD;
                                            }
                                        }
                                        #endregion
                                    }
                                    newRow["MonthlyUnitPriceCost"] = MonthlyUnitPriceCost;
                                    newRow["MonthlyCost"] = MonthlyCost;
                                    newRow["MonthlyProfit"] = MonthlyProfit;
                                    newRow["MonthlyUnitPriceForexCostEUR"] = MonthlyUnitPriceForexCostEUR;
                                    newRow["MonthlyUnitPriceForexCostUSD"] = MonthlyUnitPriceForexCostUSD;
                                    newRow["MonthlyForexCostEUR"] = MonthlyForexCostEUR;
                                    newRow["MonthlyForexCostUSD"] = MonthlyForexCostUSD;
                                    newRow["MonthlyForexProfitEUR"] = MonthlyForexProfitEUR;
                                    newRow["MonthlyForexProfitUSD"] = MonthlyForexProfitUSD;
                                    #endregion
                                }
                                if (newRow != null)
                                {
                                    SetExportDepartmentCost(newRow, inRow);
                                    oldWorkOrderNo = inRow["WorkOrderNo"].ToString();
                                }
                                TotalMonthlyShipQty = 0;
                                MonthlyShipQuantity = 0;
                                ShipQuantity = 0;
                            }

                        }
                        if (resultTotal.Tables["CostItemTable"].Rows.Count == 0 && resultTotal.Tables["IncomeItemTable"].Rows.Count > 0)
                        {
                            #region Ekstra Yapılan (İstisnai Durumlar İçin) Kontroller
                            DataRow newRow = null;
                            //Maliyeti olmadan direkt satışı yapılan 
                            if (TotalCost > 0)
                            {
                                newRow = TempCostTable.NewRow();
                                TempCostTable.Rows.Add(newRow);
                                MonthlyUnitPriceCost = 0;
                                MonthlyUnitPriceForexCostEUR = 0;
                                MonthlyUnitPriceForexCostUSD = 0;
                                MonthlyCost = 0;
                                MonthlyForexCostEUR = 0;
                                MonthlyForexCostUSD = 0;
                                MonthlyProfit = MonthlySalesTotal;
                                MonthlyForexProfitEUR = MonthlySalesTotalForexEUR;
                                MonthlyForexProfitUSD = MonthlySalesTotalForexUSD;
                                OrderQuantity = ActualCostCalculate.OrderQuantity;
                                CutQuantity = ActualCostCalculate.CutQuantity;
                                TotalCost = ActualCostCalculate.GrandTotalCost;
                                newRow["CurrentAccountCode"] = ActualCostCalculate.OrderCustomerCode;
                                newRow["CurrentAccountName"] = ActualCostCalculate.OrderCustomerName;
                                newRow["WorkOrderId"] = orderRow["WorkOrderId"];
                                newRow["CurrentAccountId"] = orderRow["CurrentAccountId"];
                                newRow["WorkOrderNo"] = orderRow["WorkOrderNo"];
                                newRow["StyleCode"] = orderRow["StyleCode"];
                                newRow["ShipmentDate"] = orderRow["ShipmentDate"];
                                newRow["DeliveryDate"] = ActualCostCalculate.OrderDeliveryDate;
                                newRow["ForexCode"] = ActualCostCalculate.OrderForexCode;
                                newRow["Quantity"] = OrderQuantity;
                                newRow["CuttingQuantity"] = CutQuantity;
                                newRow["TotalCost"] = TotalCost;
                                newRow["Year"] = CostDate.Year;
                                newRow["Month"] = CostDate.Month;
                                //Dahili Maliyet Dönem Sütunları
                                foreach (var item in ImportCostNames)
                                {
                                    if (item.Value.Contains("MonthlyCost"))
                                        newRow[item.Key] = 0;
                                }
                                foreach (var item in ImportCostNames)
                                {
                                    if (item.Value.Contains("MonthlyEUR"))
                                        newRow[item.Key] = 0;
                                }
                                foreach (var item in ImportCostNames)
                                {
                                    if (item.Value.Contains("MonthlyUSD"))
                                        newRow[item.Key] = 0;
                                }
                                newRow["IsClosed"] = "KZ(%100 Kar)";
                                CalcTotalandMonthlyValues(orderRow);
                                newRow["TotalForexCostEUR"] = TotalForexCostEUR;
                                newRow["TotalForexCostUSD"] = TotalForexCostUSD;
                                newRow["TotalSalesCost"] = TotalSalesCost;
                                newRow["TotalSalesForexCostEUR"] = TotalSalesForexCostEUR;
                                newRow["TotalSalesForexCostUSD"] = TotalSalesForexCostUSD;
                                newRow["ShipmentQuantity"] = ShipQuantity;
                            }
                            #endregion
                        }
                    }
                }
            }
            #endregion

            #region İptal Order
            StringBuilder strCancel = new StringBuilder();
            strCancel.AppendLine(@"select wo.RecId WorkOrderId,wo.WorkOrderNo,wo.CurrentAccountId
                                ,(Select InventoryCode from Erp_Inventory where RecId in (Select InventoryId from Erp_WorkOrderItem where WorkOrderId = wo.RecId)) StyleCode, (Select ForexCode from Meta_Forex with(nolock) where RecId = wo.ForexId) ForexCode 
                                ,convert(nvarchar(10),wo.ShipmentDate,104) ShipmentDate from Erp_WorkOrder wo with(nolock) where wo.CompanyId =" + activeSession.ActiveCompany.RecId + " and wo.Status=4");
            DataTable isCancelledOrder = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_WorkOrder", strCancel.ToString());
            if (isCancelledOrder?.Rows?.Count > 0)
            {
                DataSet resultCancel = null;
                DataRow newRowC = null;
                foreach (DataRow drCancel in isCancelledOrder.Rows)
                {
                    using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
                    {
                        QualityCode = "1";
                        resultCancel = (DataSet)ActualCostCalculate.Execute(drCancel["WorkOrderNo"], drCancel["StyleCode"], WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId);
                        if (resultCancel is DataSet && resultCancel != null && resultCancel.Tables.Contains("CostItemTable") && resultCancel.Tables["CostItemTable"].Rows.Count > 0)
                        {
                            DataTable cancelTable = (resultCancel as DataSet).Tables["CostItemTable"];
                            string oldWorkOrderNo = string.Empty;
                            OrderQuantity = ActualCostCalculate.OrderQuantity;
                            CutQuantity = ActualCostCalculate.CutQuantity;
                            TotalCost = ActualCostCalculate.GrandTotalCost;
                            foreach (DataRow coRow in cancelTable.Select("ParentId = 0", "WorkOrderNo"))
                            {
                                if (coRow.IsNull("WorkOrderNo")) continue;
                                if (!coRow["WorkOrderNo"].ToString().Equals(oldWorkOrderNo) && !(TempCostTable.Select($"WorkOrderId = {drCancel["WorkOrderId"]}").Length > 0))
                                {
                                    newRowC = null;
                                    CalcTotalandMonthlyValues(coRow);
                                    strCancel.Clear();
                                    strCancel.AppendLine($"select sum(isnull(MonthlyCost,0)) MonthlyCost,sum(isnull(MonthlyForexCost,0)) MonthlyForexCost,sum(isnull(MonthlyForex2Cost,0)) MonthlyForex2Cost" +
                                        @",sum(isnull(eac.MonthlyYarnCostAmount2,0)) MonthlyYarnCostAmount2      ,sum(isnull(eac.MonthlyYarnDyeCostAmount2,0)) MonthlyYarnDyeCostAmount2
                                         ,sum(isnull(eac.MonthlyFabricCostAmount2,0)) MonthlyFabricCostAmount2      ,sum(isnull(eac.MonthlyFabricDyeCostAmount2,0)) MonthlyFabricDyeCostAmount2
                                         ,sum(isnull(eac.MonthlyFabricPrintCostAmount2,0)) MonthlyFabricPrintCostAmount2      ,sum(isnull(eac.MonthlyFabricOther1CostAmount2,0)) MonthlyFabricOther1CostAmount2
                                         ,sum(isnull(eac.MonthlyFabricOther2CostAmount2,0)) MonthlyFabricOther2CostAmount2      ,sum(isnull(eac.MonthlyTrimCostAmount2,0)) MonthlyTrimCostAmount2
                                         ,sum(isnull(eac.MonthlyReturnCostAmount2,0)) MonthlyReturnCostAmount2      ,sum(isnull(eac.MonthlyCutCostAmount2,0)) MonthlyCutCostAmount2
                                         ,sum(isnull(eac.MonthlyPieceWorkCostAmount2,0)) MonthlyPieceWorkCostAmount2      ,sum(isnull(eac.MonthlyChemicalCostAmount2,0)) MonthlyChemicalCostAmount2
                                         ,sum(isnull(eac.MonthlyYarnCostAmount2EUR,0)) MonthlyYarnCostAmount2EUR      ,sum(isnull(eac.MonthlyYarnDyeCostAmount2EUR,0)) MonthlyYarnDyeCostAmount2EUR
                                         ,sum(isnull(eac.MonthlyFabricCostAmount2EUR,0)) MonthlyFabricCostAmount2EUR      ,sum(isnull(eac.MonthlyFabricDyeCostAmount2EUR,0)) MonthlyFabricDyeCostAmount2EUR
                                         ,sum(isnull(eac.MonthlyFabricPrintCostAmount2EUR,0)) MonthlyFabricPrintCostAmount2EUR      ,sum(isnull(eac.MonthlyFabricOther1CostAmount2EUR,0)) MonthlyFabricOther1CostAmount2EUR
                                         ,sum(isnull(eac.MonthlyFabricOther2CostAmount2EUR,0)) MonthlyFabricOther2CostAmount2EUR      ,sum(isnull(eac.MonthlyTrimCostAmount2EUR,0)) MonthlyTrimCostAmount2EUR
                                         ,sum(isnull(eac.MonthlyReturnCostAmount2EUR,0)) MonthlyReturnCostAmount2EUR      ,sum(isnull(eac.MonthlyCutCostAmount2EUR,0)) MonthlyCutCostAmount2EUR
                                         ,sum(isnull(eac.MonthlyPieceWorkCostAmount2EUR,0)) MonthlyPieceWorkCostAmount2EUR      ,sum(isnull(eac.MonthlyChemicalCostAmount2EUR,0)) MonthlyChemicalCostAmount2EUR
                                         ,sum(isnull(eac.MonthlyYarnCostAmount2USD,0)) MonthlyYarnCostAmount2USD      ,sum(isnull(eac.MonthlyYarnDyeCostAmount2USD,0)) MonthlyYarnDyeCostAmount2USD
                                         ,sum(isnull(eac.MonthlyFabricCostAmount2USD,0)) MonthlyFabricCostAmount2USD      ,sum(isnull(eac.MonthlyFabricDyeCostAmount2USD,0)) MonthlyFabricDyeCostAmount2USD
                                         ,sum(isnull(eac.MonthlyFabricPrintCostAmount2USD,0)) MonthlyFabricPrintCostAmount2USD      ,sum(isnull(eac.MonthlyFabricOther1CostAmount2USD,0)) MonthlyFabricOther1CostAmount2USD
                                         ,sum(isnull(eac.MonthlyFabricOther2CostAmount2USD,0)) MonthlyFabricOther2CostAmount2USD      ,sum(isnull(eac.MonthlyTrimCostAmount2USD,0)) MonthlyTrimCostAmount2USD
                                         ,sum(isnull(eac.MonthlyReturnCostAmount2USD,0)) MonthlyReturnCostAmount2USD      ,sum(isnull(eac.MonthlyCutCostAmount2USD,0)) MonthlyCutCostAmount2USD
                                         ,sum(isnull(eac.MonthlyPieceWorkCostAmount2USD,0)) MonthlyPieceWorkCostAmount2USD      ,sum(isnull(eac.MonthlyChemicalCostAmount2USD ,0)) MonthlyChemicalCostAmount2USD" +
                                        $"  from Erp_ActualCost eac with(nolock) " +
                                        $"where eac.CompanyId = {activeSession.ActiveCompany.RecId} and eac.Year = {CostDate.Year} and eac.Month < {CostDate.Month} and eac.WorkOrderId in (Select RecId from Erp_WorkOrder where WorkOrderNo= '{drCancel["WorkOrderNo"]}') ");



                                    DataTable dtAC = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_ActualCost", strCancel.ToString());

                                    if (ShipQuantity == 0 && DoubleUtil.CompareFloat(">", OrderQuantity, 0, FieldUsage.Quantity))
                                    {
                                        if (dtAC != null && dtAC.Rows.Count > 0 && !dtAC.Rows[0].IsNull("MonthlyCost"))
                                        {
                                            decimal monthlyCostCancel, monthlyForexCostCancelEUR, monthlyForexCostCancelUSD = 0;
                                            decimal.TryParse(dtAC.Rows[0]["MonthlyCost"].ToString(), out monthlyCostCancel);
                                            decimal.TryParse(dtAC.Rows[0]["MonthlyForexCost"].ToString(), out monthlyForexCostCancelEUR);
                                            decimal.TryParse(dtAC.Rows[0]["MonthlyForex2Cost"].ToString(), out monthlyForexCostCancelUSD);

                                            if (TotalCost - monthlyCostCancel <= 3 && TotalCost - monthlyCostCancel >= -3)
                                            {
                                                //Hiç bir şey yapma
                                            }
                                            else
                                            {
                                                newRowC = TempCostTable.NewRow();
                                                SetDefaultColumns(ActualCostCalculate, newRowC, drCancel, coRow);
                                                if (newRowC != null)
                                                {
                                                    //İçerde Yapılan İşçiliklerin Maliyet Sütunları
                                                    SetImportDepartmentCost(newRowC, drCancel["WorkOrderNo"], drCancel["StyleCode"]/*, QualityCode*/);
                                                }
                                                CalDiffCost(monthlyCostCancel, monthlyForexCostCancelEUR, monthlyForexCostCancelUSD);

                                                //Dahili Maliyet Dönem Sütunları
                                                foreach (DataColumn dtcol in tempCostTable.Columns)
                                                {
                                                    if (totalCostColList.Contains(dtcol.ColumnName))
                                                    {
                                                        decimal importTotalCost = 0M, beforeMonthlyCost = 0;
                                                        decimal.TryParse(newRowC[dtcol.ColumnName].ToString(), out importTotalCost);
                                                        decimal.TryParse(dtAC.Rows[0][$"Monthly{dtcol.ColumnName}"].ToString(), out beforeMonthlyCost);
                                                        newRowC[$"Monthly{dtcol.ColumnName}"] = importTotalCost - beforeMonthlyCost;
                                                    }
                                                }
                                                foreach (DataColumn dtcol in tempCostTable.Columns)
                                                {
                                                    if (totalCostColEuroList.Contains(dtcol.ColumnName))
                                                    {
                                                        decimal importTotalCostEUR = 0M, beforeMonthlyCostEUR = 0;
                                                        decimal.TryParse(newRowC[dtcol.ColumnName].ToString(), out importTotalCostEUR);
                                                        decimal.TryParse(dtAC.Rows[0][$"Monthly{dtcol.ColumnName}"].ToString(), out beforeMonthlyCostEUR);
                                                        newRowC[$"Monthly{dtcol.ColumnName}"] = importTotalCostEUR - beforeMonthlyCostEUR;
                                                    }
                                                }
                                                foreach (DataColumn dtcol in tempCostTable.Columns)
                                                {
                                                    if (totalCostColUSDList.Contains(dtcol.ColumnName))
                                                    {
                                                        decimal importTotalCostUSD = 0M, beforeMonthlyCostUSD = 0;
                                                        decimal.TryParse(newRowC[dtcol.ColumnName].ToString(), out importTotalCostUSD);
                                                        decimal.TryParse(dtAC.Rows[0][$"Monthly{dtcol.ColumnName}"].ToString(), out beforeMonthlyCostUSD);
                                                        newRowC[$"Monthly{dtcol.ColumnName}"] = importTotalCostUSD - beforeMonthlyCostUSD;
                                                    }
                                                }
                                                newRowC["IsClosed"] = "İptal-Maliyet Farkı";
                                                newRowC["ForexCode"] = drCancel["ForexCode"];
                                                newRowC["TotalCost"] = TotalCost;
                                                newRowC["TotalForexCostEUR"] = TotalForexCostEUR;
                                                newRowC["TotalForexCostUSD"] = TotalForexCostUSD;
                                                newRowC["MonthlyCost"] = MonthlyCost;
                                                newRowC["MonthlyProfit"] = MonthlyProfit;
                                                newRowC["MonthlyForexCostEUR"] = MonthlyForexCostEUR;
                                                newRowC["MonthlyForexCostUSD"] = MonthlyForexCostUSD;
                                                newRowC["MonthlyForexProfitEUR"] = MonthlyForexProfitEUR;
                                                newRowC["MonthlyForexProfitUSD"] = MonthlyForexProfitUSD;
                                                TempCostTable.Rows.Add(newRowC);
                                            }
                                        }
                                        else
                                        {
                                            newRowC = TempCostTable.NewRow();
                                            SetDefaultColumns(ActualCostCalculate, newRowC, drCancel, coRow);
                                            if (newRowC != null)
                                            {
                                                //İçerde Yapılan İşçiliklerin Maliyet Sütunları
                                                SetImportDepartmentCost(newRowC, drCancel["WorkOrderNo"], drCancel["StyleCode"]/*, QualityCode*/);
                                            }
                                            MonthlyUnitPriceCost = TotalCost / OrderQuantity;
                                            MonthlyUnitPriceForexCostEUR = TotalForexCostEUR / OrderQuantity;
                                            MonthlyUnitPriceForexCostUSD = TotalForexCostUSD / OrderQuantity;
                                            MonthlyUnitPriceCost = Math.Round(MonthlyUnitPriceCost, amountDec);
                                            MonthlyUnitPriceForexCostEUR = Math.Round(MonthlyUnitPriceForexCostEUR, amountDec);
                                            MonthlyUnitPriceForexCostUSD = Math.Round(MonthlyUnitPriceForexCostUSD, amountDec);
                                            MonthlyCost = TotalCost;
                                            MonthlyForexCostEUR = TotalForexCostEUR;
                                            MonthlyForexCostUSD = TotalForexCostUSD;
                                            MonthlyProfit = MonthlyCost * (-1);
                                            MonthlyForexProfitEUR = MonthlyForexCostEUR * (-1);
                                            MonthlyForexProfitUSD = MonthlyForexCostUSD * (-1);
                                            //Dahili Maliyet Dönem Sütunları
                                            foreach (DataColumn dtcol in tempCostTable.Columns)
                                            {
                                                if (totalCostColList.Contains(dtcol.ColumnName))
                                                    newRowC[$"Monthly{dtcol.ColumnName}"] = newRowC[dtcol.ColumnName];
                                                if (totalCostColEuroList.Contains(dtcol.ColumnName))
                                                    newRowC[$"Monthly{dtcol.ColumnName}"] = newRowC[dtcol.ColumnName];
                                                if (totalCostColUSDList.Contains(dtcol.ColumnName))
                                                    newRowC[$"Monthly{dtcol.ColumnName}"] = newRowC[dtcol.ColumnName];
                                            }
                                            newRowC["IsClosed"] = "İptal Order";
                                            newRowC["ForexCode"] = drCancel["ForexCode"];
                                            newRowC["TotalCost"] = TotalCost;
                                            newRowC["TotalForexCostEUR"] = TotalForexCostEUR;
                                            newRowC["TotalForexCostUSD"] = TotalForexCostUSD;
                                            newRowC["MonthlyUnitPriceCost"] = MonthlyUnitPriceCost;
                                            newRowC["MonthlyCost"] = MonthlyCost;
                                            newRowC["MonthlyProfit"] = MonthlyProfit;
                                            newRowC["MonthlyUnitPriceForexCostEUR"] = MonthlyUnitPriceForexCostEUR;
                                            newRowC["MonthlyUnitPriceForexCostUSD"] = MonthlyUnitPriceForexCostUSD;
                                            newRowC["MonthlyForexCostEUR"] = MonthlyForexCostEUR;
                                            newRowC["MonthlyForexCostUSD"] = MonthlyForexCostUSD;
                                            newRowC["MonthlyForexProfitEUR"] = MonthlyForexProfitEUR;
                                            newRowC["MonthlyForexProfitUSD"] = MonthlyForexProfitUSD;
                                            TempCostTable.Rows.Add(newRowC);
                                        }
                                    }
                                }
                                if (newRowC != null)
                                {
                                    SetExportDepartmentCost(newRowC, coRow);
                                    oldWorkOrderNo = coRow["WorkOrderNo"].ToString();
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Maliyet Farkı (Faturası yok ama ay fark etmeksizin son kayıtlı maliyetten farklı bir maliyeti olan Orderlar)
            CostDifferenceTable = new DataTable("CostDifferenceTable");
            CostDifferenceTable = GetCostDifference();
            if (CostDifferenceTable != null && CostDifferenceTable.Rows.Count > 0)
            {
                DataSet rstCost = null;
                DataRow newRowDC = null;
                foreach (DataRow drDC in CostDifferenceTable.Rows)
                {
                    if (!(TempCostTable.Select($"WorkOrderId = {drDC["WorkOrderId"]}").Length > 0))
                    {
                        using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
                        {
                            QualityCode = "1";
                            rstCost = (DataSet)ActualCostCalculate.Execute(drDC["WorkOrderNo"], drDC["StyleCode"], WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId);
                            if (rstCost is DataSet && rstCost != null && rstCost.Tables.Contains("CostItemTable") && rstCost.Tables["CostItemTable"].Rows.Count > 0)
                            {
                                DataTable dcTable = (rstCost as DataSet).Tables["CostItemTable"];
                                string oldWorkOrderNo = string.Empty;

                                foreach (DataRow dcRow in dcTable.Select("ParentId = 0", "WorkOrderNo"))
                                {
                                    if (dcRow.IsNull("WorkOrderNo")) continue;

                                    if (!dcRow["WorkOrderNo"].ToString().Equals(oldWorkOrderNo) && !(TempCostTable.Select($"WorkOrderId = {drDC["WorkOrderId"]}").Length > 0))
                                    {
                                        StringBuilder sbDC = new StringBuilder();
                                        sbDC.AppendLine($"select sum(isnull(MonthlyCost,0)) MonthlyCost,sum(isnull(MonthlyForexCost,0)) MonthlyForexCost,sum(isnull(MonthlyForex2Cost,0)) MonthlyForex2Cost " +
                                            @",sum(isnull(eac.MonthlyYarnCostAmount2,0)) MonthlyYarnCostAmount2      ,sum(isnull(eac.MonthlyYarnDyeCostAmount2,0)) MonthlyYarnDyeCostAmount2
                                         ,sum(isnull(eac.MonthlyFabricCostAmount2,0)) MonthlyFabricCostAmount2      ,sum(isnull(eac.MonthlyFabricDyeCostAmount2,0)) MonthlyFabricDyeCostAmount2
                                         ,sum(isnull(eac.MonthlyFabricPrintCostAmount2,0)) MonthlyFabricPrintCostAmount2      ,sum(isnull(eac.MonthlyFabricOther1CostAmount2,0)) MonthlyFabricOther1CostAmount2
                                         ,sum(isnull(eac.MonthlyFabricOther2CostAmount2,0)) MonthlyFabricOther2CostAmount2      ,sum(isnull(eac.MonthlyTrimCostAmount2,0)) MonthlyTrimCostAmount2
                                         ,sum(isnull(eac.MonthlyReturnCostAmount2,0)) MonthlyReturnCostAmount2      ,sum(isnull(eac.MonthlyCutCostAmount2,0)) MonthlyCutCostAmount2
                                         ,sum(isnull(eac.MonthlyPieceWorkCostAmount2,0)) MonthlyPieceWorkCostAmount2      ,sum(isnull(eac.MonthlyChemicalCostAmount2,0)) MonthlyChemicalCostAmount2
                                         ,sum(isnull(eac.MonthlyYarnCostAmount2EUR,0)) MonthlyYarnCostAmount2EUR      ,sum(isnull(eac.MonthlyYarnDyeCostAmount2EUR,0)) MonthlyYarnDyeCostAmount2EUR
                                         ,sum(isnull(eac.MonthlyFabricCostAmount2EUR,0)) MonthlyFabricCostAmount2EUR      ,sum(isnull(eac.MonthlyFabricDyeCostAmount2EUR,0)) MonthlyFabricDyeCostAmount2EUR
                                         ,sum(isnull(eac.MonthlyFabricPrintCostAmount2EUR,0)) MonthlyFabricPrintCostAmount2EUR      ,sum(isnull(eac.MonthlyFabricOther1CostAmount2EUR,0)) MonthlyFabricOther1CostAmount2EUR
                                         ,sum(isnull(eac.MonthlyFabricOther2CostAmount2EUR,0)) MonthlyFabricOther2CostAmount2EUR      ,sum(isnull(eac.MonthlyTrimCostAmount2EUR,0)) MonthlyTrimCostAmount2EUR
                                         ,sum(isnull(eac.MonthlyReturnCostAmount2EUR,0)) MonthlyReturnCostAmount2EUR      ,sum(isnull(eac.MonthlyCutCostAmount2EUR,0)) MonthlyCutCostAmount2EUR
                                         ,sum(isnull(eac.MonthlyPieceWorkCostAmount2EUR,0)) MonthlyPieceWorkCostAmount2EUR      ,sum(isnull(eac.MonthlyChemicalCostAmount2EUR,0)) MonthlyChemicalCostAmount2EUR
                                         ,sum(isnull(eac.MonthlyYarnCostAmount2USD,0)) MonthlyYarnCostAmount2USD      ,sum(isnull(eac.MonthlyYarnDyeCostAmount2USD,0)) MonthlyYarnDyeCostAmount2USD
                                         ,sum(isnull(eac.MonthlyFabricCostAmount2USD,0)) MonthlyFabricCostAmount2USD      ,sum(isnull(eac.MonthlyFabricDyeCostAmount2USD,0)) MonthlyFabricDyeCostAmount2USD
                                         ,sum(isnull(eac.MonthlyFabricPrintCostAmount2USD,0)) MonthlyFabricPrintCostAmount2USD      ,sum(isnull(eac.MonthlyFabricOther1CostAmount2USD,0)) MonthlyFabricOther1CostAmount2USD
                                         ,sum(isnull(eac.MonthlyFabricOther2CostAmount2USD,0)) MonthlyFabricOther2CostAmount2USD      ,sum(isnull(eac.MonthlyTrimCostAmount2USD,0)) MonthlyTrimCostAmount2USD
                                         ,sum(isnull(eac.MonthlyReturnCostAmount2USD,0)) MonthlyReturnCostAmount2USD      ,sum(isnull(eac.MonthlyCutCostAmount2USD,0)) MonthlyCutCostAmount2USD
                                         ,sum(isnull(eac.MonthlyPieceWorkCostAmount2USD,0)) MonthlyPieceWorkCostAmount2USD      ,sum(isnull(eac.MonthlyChemicalCostAmount2USD ,0)) MonthlyChemicalCostAmount2USD" +
                                            $" from Erp_ActualCost eac where eac.CompanyId = {activeSession.ActiveCompany.RecId} and eac.Year = {CostDate.Year} and eac.Month < {CostDate.Month} and WorkOrderId in (Select RecId from Erp_WorkOrder where WorkOrderNo= '{drDC["WorkOrderNo"]}')");
                                        DataTable dtDC = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_ActualCost", sbDC.ToString());
                                        if (dtDC == null || dtDC.Rows.Count <= 0 || dtDC.Rows[0].IsNull("MonthlyCost")) continue;
                                        if (dtDC != null && dtDC.Rows.Count > 0 && !dtDC.Rows[0].IsNull("MonthlyCost"))
                                        {
                                            newRowDC = TempCostTable.NewRow();
                                            SetDefaultColumns(ActualCostCalculate, newRowDC, drDC, dcRow);
                                            decimal beforeMonthlyCost = 0, beforeMonthlyForexCostEUR = 0, beforeMonthlyForexCostUSD;

                                            CalcTotalandMonthlyValues(drDC);
                                            decimal.TryParse(dtDC.Rows[0]["MonthlyCost"].ToString(), out beforeMonthlyCost);
                                            decimal.TryParse(dtDC.Rows[0]["MonthlyForexCost"].ToString(), out beforeMonthlyForexCostEUR);
                                            decimal.TryParse(dtDC.Rows[0]["MonthlyForex2Cost"].ToString(), out beforeMonthlyForexCostUSD);
                                            if (TotalCost - MonthlyCost <= 3 && TotalCost - MonthlyCost >= -3)
                                            {
                                                //Hiç bir şey yapma
                                            }
                                            else
                                            {
                                                string query = @"select ei.RecId from Erp_Invoice ei left join Erp_InventoryReceipt ir with(nolock) on ei.RecId = ir.InvoiceId
                                                                left join Erp_InventoryReceiptItem iri with(nolock) on iri.InventoryReceiptId = ir.RecId";
                                                query += $" where iri.WorkOrderReceiptItemId in (Select RecId from Erp_WorkOrderItem with(nolock) where WorkOrderId in (Select RecId from Erp_WorkOrder where WorkOrderNo = '{dcRow["WorkOrderNo"]}'))";
                                                query += $" and Year(ei.DischargeDate) = {CostDate.Year} and MONTH(ei.DischargeDate) > {CostDate.Month}";
                                                DataTable dtInv = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_ActualCost", query.ToString());
                                                if (dtInv != null && dtInv.Rows.Count > 0)
                                                {
                                                    //Hiç bir şey yapma
                                                }
                                                else
                                                {
                                                    CalDiffCost(beforeMonthlyCost, beforeMonthlyForexCostEUR, beforeMonthlyForexCostUSD);
                                                    if (newRowDC != null)
                                                    {
                                                        //İçerde Yapılan İşçiliklerin Maliyet Sütunları
                                                        SetImportDepartmentCost(newRowDC, drDC["WorkOrderNo"], drDC["StyleCode"]/*, QualityCode*/);
                                                    }
                                                    // Dahili Maliyet Dönem Sütunları
                                                    foreach (DataColumn dtcol in tempCostTable.Columns)
                                                    {
                                                        if (totalCostColList.Contains(dtcol.ColumnName)) //totallistesinde olanlara göre eski ile yeniyi kıyaslıyor
                                                        {
                                                            decimal importTotalCost = 0M, beforeImportMonthlyCost = 0, calcAmount = 0;
                                                            decimal.TryParse(newRowDC[dtcol.ColumnName].ToString(), out importTotalCost);
                                                            decimal.TryParse(dtDC.Rows[0][$"Monthly{dtcol.ColumnName}"].ToString(), out beforeImportMonthlyCost);
                                                            calcAmount = importTotalCost - beforeImportMonthlyCost;
                                                            newRowDC[$"Monthly{dtcol.ColumnName}"] = calcAmount;
                                                        }
                                                    }
                                                    foreach (DataColumn dtcol in tempCostTable.Columns)
                                                    {
                                                        if (totalCostColEuroList.Contains(dtcol.ColumnName))
                                                        {
                                                            decimal importTotalCostEUR = 0M, beforeImportMonthlyCostEUR = 0;
                                                            decimal.TryParse(newRowDC[dtcol.ColumnName].ToString(), out importTotalCostEUR);
                                                            decimal.TryParse(dtDC.Rows[0][$"Monthly{dtcol.ColumnName}"].ToString(), out beforeImportMonthlyCostEUR);
                                                            newRowDC[$"Monthly{dtcol.ColumnName}"] = importTotalCostEUR - beforeImportMonthlyCostEUR;
                                                        }
                                                    }
                                                    foreach (DataColumn dtcol in tempCostTable.Columns)
                                                    {
                                                        if (totalCostColUSDList.Contains(dtcol.ColumnName))
                                                        {
                                                            decimal importTotalCostUSD = 0M, beforeImportMonthlyCostUSD = 0;
                                                            decimal.TryParse(newRowDC[dtcol.ColumnName].ToString(), out importTotalCostUSD);
                                                            decimal.TryParse(dtDC.Rows[0][$"Monthly{dtcol.ColumnName}"].ToString(), out beforeImportMonthlyCostUSD);
                                                            newRowDC[$"Monthly{dtcol.ColumnName}"] = importTotalCostUSD - beforeImportMonthlyCostUSD;
                                                        }
                                                    }
                                                    newRowDC["TotalForexCostEUR"] = TotalForexCostEUR;
                                                    newRowDC["TotalForexCostUSD"] = TotalForexCostUSD;
                                                    newRowDC["IsClosed"] = "Maliyet Farkı";
                                                    newRowDC["MonthlyCost"] = MonthlyCost;
                                                    newRowDC["MonthlyProfit"] = MonthlyProfit;
                                                    newRowDC["MonthlyForexCostEUR"] = MonthlyForexCostEUR;
                                                    newRowDC["MonthlyForexCostUSD"] = MonthlyForexCostUSD;
                                                    newRowDC["MonthlyForexProfitEUR"] = MonthlyForexProfitEUR;
                                                    newRowDC["MonthlyForexProfitUSD"] = MonthlyForexProfitUSD;
                                                    TempCostTable.Rows.Add(newRowDC);

                                                    oldWorkOrderNo = dcRow["WorkOrderNo"].ToString();

                                                }
                                            }
                                        }
                                    }
                                    if (newRowDC != null)
                                    {
                                        SetExportDepartmentCost(newRowDC, dcRow);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 2. Kalite ve Muhtelif Satışlar
            Muh2KTable = new DataTable("Muh2KTable");
            Muh2KTable = GetMUHand2KOrder();
            if (Muh2KTable?.Rows?.Count > 0) //%100 Kar
            {
                foreach (DataRow Muh2KRow in Muh2KTable.Select(""))
                {
                    long workOrderId = -1;
                    DataRow newRowMUHand2K = null;
                    newRowMUHand2K = TempCostTable.NewRow();
                    long.TryParse(Muh2KRow["WorkOrderId"].ToString(), out workOrderId);
                    if (workOrderId == 0)
                    {
                        newRowMUHand2K["IsClosed"] = "KZ(%100 Kar)-MUH";
                    }
                    else if (workOrderId > 0)
                    {
                        newRowMUHand2K["IsClosed"] = "KZ(%100 Kar)-2K";

                        DataSet result2K = null;
                        using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
                        {
                            QualityCode = "2";
                            result2K = (DataSet)ActualCostCalculate.Execute(Muh2KRow["WorkOrderNo"], Muh2KRow["StyleCode"], WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 1, DateTime.MinValue, QualityCode);
                            if (result2K is DataSet && result2K != null && result2K.Tables.Contains("CostItemTable") && result2K.Tables["CostItemTable"].Rows.Count > 0)
                            {
                                DataTable KTable = (result2K as DataSet).Tables["CostItemTable"];
                                OrderQuantity = ActualCostCalculate.OrderQuantity;
                                CutQuantity = ActualCostCalculate.CutQuantity;

                                foreach (DataRow inKRow in KTable.Select("ParentId = 0", "WorkOrderNo"))
                                {
                                    CalcTotalandMonthlyValues(Muh2KRow);
                                    newRowMUHand2K["DeliveryDate"] = ActualCostCalculate.OrderDeliveryDate;
                                    newRowMUHand2K["Quantity"] = OrderQuantity;
                                    newRowMUHand2K["CuttingQuantity"] = CutQuantity;
                                    newRowMUHand2K["TotalCost"] = 0;
                                    newRowMUHand2K["TotalForexCostEUR"] = 0;
                                    newRowMUHand2K["TotalForexCostUSD"] = 0;
                                    if (newRowMUHand2K != null)
                                    {
                                        SetExportDepartmentCost(newRowMUHand2K, inKRow);
                                    }
                                    if (newRowMUHand2K != null)
                                    {
                                        //İçerde Yapılan İşçiliklerin Maliyet Sütunları
                                        SetImportDepartmentCost(newRowMUHand2K, Muh2KRow["WorkOrderNo"], Muh2KRow["StyleCode"]/*, QualityCode*/);
                                    }
                                }
                            }
                        }
                    }
                    decimal.TryParse(Muh2KRow["SumQuantity"].ToString(), out MonthlyShipQuantity);
                    newRowMUHand2K["MonthlyShipmentQuantity"] = MonthlyShipQuantity;
                    decimal.TryParse(Muh2KRow["SumItemTotal"].ToString(), out MonthlySalesTotal);
                    decimal.TryParse(Muh2KRow["ItemTotalForexEUR"].ToString(), out MonthlySalesTotalForexEUR);
                    decimal.TryParse(Muh2KRow["ItemTotalForexUSD"].ToString(), out MonthlySalesTotalForexUSD);
                    newRowMUHand2K["CurrentAccountCode"] = Muh2KRow["CurrentAccountCode"];
                    newRowMUHand2K["CurrentAccountName"] = Muh2KRow["CurrentAccountName"];
                    newRowMUHand2K["WorkOrderId"] = Muh2KRow["WorkOrderId"];
                    newRowMUHand2K["WorkOrderItemId"] = Muh2KRow["WorkOrderItemId"];
                    newRowMUHand2K["CurrentAccountId"] = Muh2KRow["CurrentAccountId"];
                    newRowMUHand2K["WorkOrderNo"] = Muh2KRow["WorkOrderNo"];
                    newRowMUHand2K["StyleCode"] = Muh2KRow["StyleCode"];
                    newRowMUHand2K["ShipmentDate"] = Muh2KRow["ReceiptDate"];
                    newRowMUHand2K["ForexCode"] = Muh2KRow["ForexCode"];
                    newRowMUHand2K["Year"] = CostDate.Year;
                    newRowMUHand2K["Month"] = CostDate.Month;
                    newRowMUHand2K["MonthlyUnitPriceCost"] = 0;
                    newRowMUHand2K["MonthlyUnitPriceForexCostEUR"] = 0;
                    newRowMUHand2K["MonthlyUnitPriceForexCostUSD"] = 0;
                    newRowMUHand2K["MonthlyCost"] = 0;
                    newRowMUHand2K["MonthlyForexCostEUR"] = 0;
                    newRowMUHand2K["MonthlyForexCostUSD"] = 0;
                    //Dahili Maliyet Dönem Sütunları
                    foreach (var item in ImportCostNames)
                    {
                        if (item.Value.Contains("MonthlyCost"))
                            newRowMUHand2K[item.Key] = 0;
                    }
                    foreach (var item in ImportCostNames)
                    {
                        if (item.Value.Contains("MonthlyEUR"))
                            newRowMUHand2K[item.Key] = 0;
                    }
                    foreach (var item in ImportCostNames)
                    {
                        if (item.Value.Contains("MonthlyUSD"))
                            newRowMUHand2K[item.Key] = 0;
                    }
                    newRowMUHand2K["MonthlySalesCost"] = MonthlySalesTotal;
                    newRowMUHand2K["MonthlySalesForexCostEUR"] = MonthlySalesTotalForexEUR;
                    newRowMUHand2K["MonthlySalesForexCostUSD"] = MonthlySalesTotalForexUSD;
                    newRowMUHand2K["MonthlyProfit"] = MonthlySalesTotal;
                    newRowMUHand2K["MonthlyForexProfitEUR"] = MonthlySalesTotalForexEUR;
                    newRowMUHand2K["MonthlyForexProfitUSD"] = MonthlySalesTotalForexUSD;
                    TempCostTable.Rows.Add(newRowMUHand2K);
                }
            }
            #endregion

            //Parça İşçilik Detay Tablosu(İç İşçilikler)
            GetPieceWorkDetailTable();
        }

        private void SetDefaultColumns(ActualCostModelService ActualCostCalculate, DataRow newRow, DataRow orderRow, DataRow inRow)
        {
            OrderQuantity = ActualCostCalculate.OrderQuantity;
            CutQuantity = ActualCostCalculate.CutQuantity;
            TotalCost = ActualCostCalculate.GrandTotalCost;
            newRow["CurrentAccountCode"] = ActualCostCalculate.OrderCustomerCode;
            newRow["CurrentAccountName"] = ActualCostCalculate.OrderCustomerName;
            newRow["WorkOrderId"] = orderRow["WorkOrderId"];
            newRow["CurrentAccountId"] = orderRow["CurrentAccountId"];
            newRow["WorkOrderNo"] = inRow["WorkOrderNo"];
            newRow["StyleCode"] = inRow["StyleCode"];
            newRow["ShipmentDate"] = orderRow["ShipmentDate"];
            newRow["DeliveryDate"] = ActualCostCalculate.OrderDeliveryDate;
            newRow["ForexCode"] = ActualCostCalculate.OrderForexCode;
            newRow["Quantity"] = OrderQuantity;
            newRow["CuttingQuantity"] = CutQuantity;
            newRow["TotalCost"] = TotalCost;
            newRow["Year"] = CostDate.Year;
            newRow["Month"] = CostDate.Month;
        }
        private void CalcTotalandMonthlyValues(DataRow orderRow)
        {
            DataSet resultTotal = null;
            DataSet resultMonthly = null;
            QualityCode = "1";
            using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
            {
                TotalSalesCost = 0; ShipQuantity = 0;
                resultTotal = (DataSet)ActualCostCalculate.Execute(orderRow["WorkOrderNo"], orderRow["StyleCode"], WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 1, DateTime.MinValue, QualityCode);
                if (resultTotal is DataSet && resultTotal != null && resultTotal.Tables.Contains("IncomeItemTable") && resultTotal.Tables["IncomeItemTable"].Rows.Count > 0)
                {

                    DataTable incomeTable = (resultTotal as DataSet).Tables["IncomeItemTable"];
                    foreach (DataRow item in incomeTable.Select("ParentId = 0 and GroupName = 'GELİR FATURALARI'", "WorkOrderNo"))
                    {
                        decimal.TryParse(item["Amount"].ToString(), out TotalSalesCost);
                        decimal.TryParse(item["Quantity"].ToString(), out ShipQuantity);
                    }
                }
            }
            using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
            {
                MonthlySalesTotal = 0; MonthlyShipQuantity = 0;
                resultMonthly = (DataSet)ActualCostCalculate.Execute(orderRow["WorkOrderNo"], orderRow["StyleCode"], WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 1, CostDate, QualityCode);
                if (resultMonthly is DataSet && resultMonthly != null && resultMonthly.Tables.Contains("IncomeItemTable") && resultMonthly.Tables["IncomeItemTable"].Rows.Count > 0)
                {
                    DataTable incomeTable = (resultMonthly as DataSet).Tables["IncomeItemTable"];
                    foreach (DataRow item in incomeTable.Select("ParentId = 0 and GroupName = 'GELİR FATURALARI'", "WorkOrderNo"))
                    {
                        decimal.TryParse(item["Amount"].ToString(), out MonthlySalesTotal);
                        decimal.TryParse(item["Quantity"].ToString(), out MonthlyShipQuantity);
                    }
                }
            }
            using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
            {
                TotalSalesForexCostEUR = 0;
                ActualCostCalculate.ChosenForexCode = SLanguage.GetString("EURO");

                resultTotal = (DataSet)ActualCostCalculate.Execute(orderRow["WorkOrderNo"], orderRow["StyleCode"], WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 1, DateTime.MinValue, QualityCode);
                if (resultTotal is DataSet && resultTotal != null && resultTotal.Tables.Contains("IncomeItemTable") && resultTotal.Tables["IncomeItemTable"].Rows.Count > 0)
                {
                    TotalForexCostEUR = ActualCostCalculate.TotalForexCost;
                    DataTable incomeTable = (resultTotal as DataSet).Tables["IncomeItemTable"];
                    foreach (DataRow item in incomeTable.Select("ParentId = 0 and GroupName = 'GELİR FATURALARI'", "WorkOrderNo"))
                        decimal.TryParse(item["ForexAmount"].ToString(), out TotalSalesForexCostEUR);
                }
            }
            using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
            {
                TotalSalesForexCostUSD = 0;
                ActualCostCalculate.ChosenForexCode = SLanguage.GetString("$");

                resultTotal = (DataSet)ActualCostCalculate.Execute(orderRow["WorkOrderNo"], orderRow["StyleCode"], WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 1, DateTime.MinValue, QualityCode);
                if (resultTotal is DataSet && resultTotal != null && resultTotal.Tables.Contains("IncomeItemTable") && resultTotal.Tables["IncomeItemTable"].Rows.Count > 0)
                {
                    TotalForexCostUSD = ActualCostCalculate.TotalForexCost;
                    DataTable incomeTable = (resultTotal as DataSet).Tables["IncomeItemTable"];
                    foreach (DataRow item in incomeTable.Select("ParentId = 0 and GroupName = 'GELİR FATURALARI'", "WorkOrderNo"))
                        decimal.TryParse(item["ForexAmount"].ToString(), out TotalSalesForexCostUSD);
                }
            }
            using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
            {
                MonthlySalesTotalForexEUR = 0;
                ActualCostCalculate.ChosenForexCode = SLanguage.GetString("EURO");

                resultMonthly = (DataSet)ActualCostCalculate.Execute(orderRow["WorkOrderNo"], orderRow["StyleCode"], WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 1, CostDate, QualityCode);
                if (resultMonthly is DataSet && resultMonthly != null && resultMonthly.Tables.Contains("IncomeItemTable") && resultMonthly.Tables["IncomeItemTable"].Rows.Count > 0)
                {
                    DataTable incomeTable = (resultMonthly as DataSet).Tables["IncomeItemTable"];
                    foreach (DataRow item in incomeTable.Select("ParentId = 0 and GroupName = 'GELİR FATURALARI'", "WorkOrderNo"))
                        decimal.TryParse(item["ForexAmount"].ToString(), out MonthlySalesTotalForexEUR);
                }
            }
            using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
            {
                MonthlySalesTotalForexUSD = 0;
                ActualCostCalculate.ChosenForexCode = SLanguage.GetString("$");

                resultMonthly = (DataSet)ActualCostCalculate.Execute(orderRow["WorkOrderNo"], orderRow["StyleCode"], WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 1, CostDate, QualityCode);
                if (resultMonthly is DataSet && resultMonthly != null && resultMonthly.Tables.Contains("IncomeItemTable") && resultMonthly.Tables["IncomeItemTable"].Rows.Count > 0)
                {
                    DataTable incomeTable = (resultMonthly as DataSet).Tables["IncomeItemTable"];
                    foreach (DataRow item in incomeTable.Select("ParentId = 0 and GroupName = 'GELİR FATURALARI'", "WorkOrderNo"))
                        decimal.TryParse(item["ForexAmount"].ToString(), out MonthlySalesTotalForexUSD);
                }
            }
        }

        /// <summary>
        /// Dönem satış tutarı, Dönem satış miktarı
        /// </summary>
        /// <param name="WorkOrderNo"></param>

        private void CalDiffCost(decimal beforeMonthlyCost, decimal beforeMonthlyForexCosEur, decimal beforeMonthlyForexCostUSD)
        {
            MonthlyCost = TotalCost - beforeMonthlyCost;
            MonthlyForexCostEUR = TotalForexCostEUR - beforeMonthlyForexCosEur;
            MonthlyForexCostUSD = TotalForexCostUSD - beforeMonthlyForexCostUSD;
            MonthlyProfit = MonthlyCost * (-1);
            MonthlyForexProfitEUR = MonthlyForexCostEUR * (-1);
            MonthlyForexProfitUSD = MonthlyForexCostUSD * (-1);
        }

        /// <summary>
        /// İçerdeki Atölyelerde Yapılan Maliyet Sütunları
        /// </summary>
        /// <param name="newRow"></param>
        /// <param name="WorkOrderNo"></param>
        /// <param name="StyleCode"></param>
        private void SetImportDepartmentCost(DataRow newRow, object WorkOrderNo, object StyleCode/*, string qualityCode*/)
        {
            DataSet resultIn = null;
            using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
            {
                resultIn = (DataSet)ActualCostCalculate.Execute(WorkOrderNo, StyleCode, WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 2/*, DateTime.MinValue, qualityCode*/);
                if (resultIn is DataSet && resultIn != null && resultIn.Tables.Contains("CostItemTable") && resultIn.Tables["CostItemTable"].Rows.Count > 0)
                {
                    DataTable costTable = (resultIn as DataSet).Tables["CostItemTable"];
                    foreach (DataRow dRow in costTable.Select("ParentId = 0", "WorkOrderNo"))
                    {
                        byte actualCostTyp = 0;
                        byte.TryParse(dRow["ActualType"].ToString(), out actualCostTyp);
                        if (actualCostTyp == (byte)OrderCostType.Yarn) newRow["YarnCostAmount2"] = dRow["Amount"];
                        else if (actualCostTyp == (byte)OrderCostType.YarnDye) newRow["YarnDyeCostAmount2"] = dRow["Amount"];
                        else if (actualCostTyp == (byte)OrderCostType.Fabric) newRow["FabricCostAmount2"] = dRow["Amount"];
                        else if (actualCostTyp == (byte)OrderCostType.FabricDye) newRow["FabricDyeCostAmount2"] = dRow["Amount"];
                        else if (actualCostTyp == (byte)OrderCostType.FabricPrint) newRow["FabricPrintCostAmount2"] = dRow["Amount"];
                        else if (actualCostTyp == (byte)OrderCostType.FabricOther1) newRow["FabricOther1CostAmount2"] = dRow["Amount"];
                        else if (actualCostTyp == (byte)OrderCostType.FabricOther2) newRow["FabricOther2CostAmount2"] = dRow["Amount"];
                        else if (actualCostTyp == (byte)OrderCostType.ReturnRawMaterial) newRow["ReturnCostAmount2"] = dRow["Amount"];
                        else if (actualCostTyp == (byte)OrderCostType.Trim) newRow["TrimCostAmount2"] = dRow["Amount"];
                        else if (actualCostTyp == (byte)OrderCostType.Cutting) newRow["CutCostAmount2"] = dRow["Amount"];
                        else if (actualCostTyp == (byte)OrderCostType.PieceWork) newRow["PieceWorkCostAmount2"] = dRow["Amount"];
                        else if (actualCostTyp == (byte)OrderCostType.Cutting) newRow["CutCostAmount2"] = dRow["Amount"];
                        else if (actualCostTyp == (byte)OrderCostType.Chemical) newRow["ChemicalCostAmount2"] = dRow["Amount"];
                    }
                }
            }
            using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
            {
                ActualCostCalculate.ChosenForexCode = SLanguage.GetString("EURO");
                resultIn = (DataSet)ActualCostCalculate.Execute(WorkOrderNo, StyleCode, WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 2/*, DateTime.MinValue, qualityCode*/);
                if (resultIn is DataSet && resultIn != null && resultIn.Tables.Contains("CostItemTable") && resultIn.Tables["CostItemTable"].Rows.Count > 0)
                {
                    DataTable costTable = (resultIn as DataSet).Tables["CostItemTable"];
                    foreach (DataRow dRow in costTable.Select("ParentId = 0", "WorkOrderNo"))
                    {
                        byte actualCostTyp = 0;
                        byte.TryParse(dRow["ActualType"].ToString(), out actualCostTyp);
                        if (actualCostTyp == (byte)OrderCostType.Yarn) newRow["YarnCostAmount2EUR"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.YarnDye) newRow["YarnDyeCostAmount2EUR"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.Fabric) newRow["FabricCostAmount2EUR"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.FabricDye) newRow["FabricDyeCostAmount2EUR"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.FabricPrint) newRow["FabricPrintCostAmount2EUR"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.FabricOther1) newRow["FabricOther1CostAmount2EUR"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.FabricOther2) newRow["FabricOther2CostAmount2EUR"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.ReturnRawMaterial) newRow["ReturnCostAmount2EUR"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.Trim) newRow["TrimCostAmount2EUR"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.Cutting) newRow["CutCostAmount2EUR"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.PieceWork) newRow["PieceWorkCostAmount2EUR"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.Cutting) newRow["CutCostAmount2EUR"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.Chemical) newRow["ChemicalCostAmount2EUR"] = dRow["ForexAmount"];
                    }
                }
            }
            using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
            {
                ActualCostCalculate.ChosenForexCode = SLanguage.GetString("$");
                resultIn = (DataSet)ActualCostCalculate.Execute(WorkOrderNo, StyleCode, WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 2/*, DateTime.MinValue, qualityCode*/);
                if (resultIn is DataSet && resultIn != null && resultIn.Tables.Contains("CostItemTable") && resultIn.Tables["CostItemTable"].Rows.Count > 0)
                {
                    DataTable costTable = (resultIn as DataSet).Tables["CostItemTable"];
                    foreach (DataRow dRow in costTable.Select("ParentId = 0", "WorkOrderNo"))
                    {
                        byte actualCostTyp = 0;
                        byte.TryParse(dRow["ActualType"].ToString(), out actualCostTyp);
                        if (actualCostTyp == (byte)OrderCostType.Yarn) newRow["YarnCostAmount2USD"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.YarnDye) newRow["YarnDyeCostAmount2USD"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.Fabric) newRow["FabricCostAmount2USD"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.FabricDye) newRow["FabricDyeCostAmount2USD"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.FabricPrint) newRow["FabricPrintCostAmount2USD"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.FabricOther1) newRow["FabricOther1CostAmount2USD"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.FabricOther2) newRow["FabricOther2CostAmount2USD"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.ReturnRawMaterial) newRow["ReturnCostAmount2USD"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.Trim) newRow["TrimCostAmount2USD"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.Cutting) newRow["CutCostAmount2USD"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.PieceWork) newRow["PieceWorkCostAmount2USD"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.Cutting) newRow["CutCostAmount2USD"] = dRow["ForexAmount"];
                        else if (actualCostTyp == (byte)OrderCostType.Chemical) newRow["ChemicalCostAmount2USD"] = dRow["ForexAmount"];
                    }
                }
            }
        }
        /// <summary>
        /// Satırda gelen maliyet tutarlarının kolon şeklinde yazılması
        /// </summary>
        /// <param name="newRow"></param>
        /// <param name="inRow"></param>
        private void SetExportDepartmentCost(DataRow newRow, DataRow inRow)
        {
            byte actualCostType = 0;
            byte.TryParse(inRow["ActualType"].ToString(), out actualCostType);
            if (actualCostType == (byte)OrderCostType.Yarn) newRow["YarnCostAmount"] = inRow["Amount"];
            else if (actualCostType == (byte)OrderCostType.YarnDye) newRow["YarnDyeCostAmount"] = inRow["Amount"];
            else if (actualCostType == (byte)OrderCostType.Fabric) newRow["FabricCostAmount"] = inRow["Amount"];
            else if (actualCostType == (byte)OrderCostType.FabricDye) newRow["FabricDyeCostAmount"] = inRow["Amount"];
            else if (actualCostType == (byte)OrderCostType.FabricPrint) newRow["FabricPrintCostAmount"] = inRow["Amount"];
            else if (actualCostType == (byte)OrderCostType.FabricOther1) newRow["FabricOther1CostAmount"] = inRow["Amount"];
            else if (actualCostType == (byte)OrderCostType.FabricOther2) newRow["FabricOther2CostAmount"] = inRow["Amount"];
            else if (actualCostType == (byte)OrderCostType.ReturnRawMaterial) newRow["ReturnCostAmount"] = inRow["Amount"];
            else if (actualCostType == (byte)OrderCostType.Trim) newRow["TrimCostAmount"] = inRow["Amount"];
            else if (actualCostType == (byte)OrderCostType.Cutting) newRow["CutCostAmount"] = inRow["Amount"];
            else if (actualCostType == (byte)OrderCostType.PieceWork) newRow["PieceWorkCostAmount"] = inRow["Amount"];
            else if (actualCostType == (byte)OrderCostType.Cutting) newRow["CutCostAmount"] = inRow["Amount"];
            else if (actualCostType == (byte)OrderCostType.Chemical) newRow["ChemicalCostAmount"] = inRow["Amount"];
        }
        private void GrdActualCost_CurrentItemChanged(object sender, DevExpress.Xpf.Grid.CurrentItemChangedEventArgs e)
        {
            var view = OrderSelectedItem as DataRowView;
            if (view != null)
            {
                string WorkOrderNo = view.Row["WorkOrderNo"].ToString();
                int? nextRowHandle = grdPieceWorkDetail?.FindRowByValue("WorkOrderNo", (string)WorkOrderNo);
                if (nextRowHandle.HasValue) PieceWorkDetailSelectedItem = grdPieceWorkDetail.GetRow(nextRowHandle.Value);
            }
        }
        private void GetPieceWorkDetailTable()
        {
            PieceWorkDetailTable = new DataTable();
            InitializePieceWorkDataColumns();
            DataTable TempTable = new DataTable();
            if (TempCostTable != null)
                TempTable.Merge(TempCostTable);
            var resultOrderItem = from DataRow item in TempTable.Select("isnull(TotalCost,0) >0")
                                  group item by new
                                  {
                                      WorkOrderNo = item.Field<string>("WorkOrderNo"),
                                      StyleCode = !item.IsNull("StyleCode") ? item.Field<string>("StyleCode") : "",
                                      OrderStatus = item.Field<string>("IsClosed"),
                                      OrderQuantity = !item.IsNull("Quantity") ? item.Field<decimal>("Quantity") : 0,
                                      ShipQuantity = !item.IsNull("ShipmentQuantity") ? item.Field<decimal>("ShipmentQuantity") : 0,
                                      MonthlyShipmentQuantity = !item.IsNull("ShipmentQuantity") ? item.Field<decimal>("MonthlyShipmentQuantity") : 0
                                  } into grp
                                  orderby grp.Key.WorkOrderNo
                                  select new
                                  {
                                      WorkOrderNo = grp.Key.WorkOrderNo,
                                      StyleCode = grp.Key.StyleCode,
                                      OrderStatus = grp.Key.OrderStatus,
                                      OrderQuantity = grp.Key.OrderQuantity,
                                      ShipQuantity = grp.Key.ShipQuantity,
                                      MonthlyShipmentQuantity = grp.Key.MonthlyShipmentQuantity,
                                  };
            DataSet resultItem = null;
            if (resultOrderItem != null && resultOrderItem.Any())
            {
                foreach (var orderRow in resultOrderItem)
                {
                    DataRow newRow = null;
                    using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
                    {
                        //Dahili İşçilikler İçin Gerçek Maliyet Servisine 2 Parametresi Gönderiliyor.
                        resultItem = (DataSet)ActualCostCalculate.Execute(orderRow.WorkOrderNo, orderRow.StyleCode, WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 2, DateTime.MinValue, "1");
                        if (resultItem is DataSet && resultItem != null && resultItem.Tables.Contains("CostItemTable") && resultItem.Tables["CostItemTable"].Rows.Count > 0)
                        {
                            DataTable pwTable = (resultItem as DataSet).Tables["CostItemTable"];

                            foreach (DataRow dRow in pwTable.Select($"ParentId <> 0"))
                            {
                                byte actualCostType = 0; decimal amount = 0;
                                decimal.TryParse(dRow["Amount"].ToString(), out amount);
                                byte.TryParse(dRow["ActualType"].ToString(), out actualCostType);
                                if (actualCostType == (byte)OrderCostType.PieceWork && amount > 0)
                                {
                                    newRow = PieceWorkDetailTable.NewRow();
                                    PieceWorkDetailTable.Rows.Add(newRow);
                                    newRow["WorkOrderNo"] = dRow["WorkOrderNo"];
                                    newRow["OrderStatus"] = orderRow.OrderStatus;
                                    newRow["ProcessName"] = dRow["Name"];
                                    newRow["TotalAmount"] = dRow["Amount"];
                                    decimal importTotalCost = 0M;
                                    decimal.TryParse(newRow["TotalAmount"].ToString(), out importTotalCost);

                                    decimal beforeImportMonthlyCost = 0;
                                    StringBuilder query = new StringBuilder();
                                    query.AppendLine($"SELECT MonthlyAmount FROM Erp_ActualCostProcessDetail WHERE ActualCostId IN (SELECT eac.RecId FROM Erp_ActualCost eac WHERE eac.Year = {CostDate.Year} AND eac.Month < {CostDate.Month} AND eac.WorkOrderId IN");
                                    query.AppendLine($"(SELECT RecId FROM Erp_WorkOrder ewo WHERE ewo.WorkOrderNo = '{dRow["WorkOrderNo"]}')) AND ForexId IS NULL AND Type=2");
                                    DataTable dtBefore = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_ActualCostProcessDetail", query.ToString());

                                    switch (orderRow.OrderStatus)
                                    {
                                        case "Açık-YT Boş":
                                            newRow["OrderStatus"] = "Açık-YT Boş";
                                            newRow["MonthlyAmount"] = CalcOpenOrderForPieceWorkTL(importTotalCost, orderRow.OrderQuantity, orderRow.ShipQuantity, orderRow.MonthlyShipmentQuantity);
                                            break;
                                        case "KZ(%100 Kar)":
                                            newRow["OrderStatus"] = "KZ(%100 Kar)";
                                            newRow["MonthlyAmount"] = 0;
                                            break;
                                        case "Açık-Kapalı ama ÇT<YT":
                                            newRow["OrderStatus"] = "Açık-Kapalı ama ÇT<YT";
                                            newRow["MonthlyAmount"] = CalcOpenOrderForPieceWorkTL(importTotalCost, orderRow.OrderQuantity, orderRow.ShipQuantity, orderRow.MonthlyShipmentQuantity);
                                            break;
                                        case "Kapalı-Tek Yükleme":
                                            newRow["OrderStatus"] = "Kapalı-Tek Yükleme";
                                            newRow["MonthlyAmount"] = importTotalCost;
                                            break;
                                        case "Açık-Kapalı ama İleri Yüklemesi Var":
                                            newRow["OrderStatus"] = "Açık-Kapalı ama İleri Yüklemesi Var";
                                            newRow["MonthlyAmount"] = CalcOpenOrderForPieceWorkTL(importTotalCost, orderRow.OrderQuantity, orderRow.ShipQuantity, orderRow.MonthlyShipmentQuantity);
                                            break;
                                        case "Açık-Kapalı ama Hem Eski Hem İleri Yüklemesi Var":
                                            newRow["OrderStatus"] = "Açık-Kapalı ama Hem Eski Hem İleri Yüklemesi Var";
                                            newRow["MonthlyAmount"] = CalcOpenOrderForPieceWorkTL(importTotalCost, orderRow.OrderQuantity, orderRow.ShipQuantity, orderRow.MonthlyShipmentQuantity);
                                            break;
                                        case "İptal Order":
                                            newRow["OrderStatus"] = "İptal Order";
                                            newRow["MonthlyAmount"] = importTotalCost;
                                            break;
                                        case "KZ(%100 Kar)-2K":
                                            newRow["OrderStatus"] = "KZ(%100 Kar)-2K";
                                            newRow["MonthlyAmount"] = 0;
                                            break;
                                        case "Kapalı-Bu Ay Son Yükleme":
                                            newRow["OrderStatus"] = "Kapalı-Bu Ay Son Yükleme";
                                            beforeImportMonthlyCost = 0;
                                            if (dtBefore != null && dtBefore.Rows.Count > 0)
                                                beforeImportMonthlyCost = (from DataRow rRow in dtBefore.Rows where !rRow.IsNull("MonthlyAmount") select Convert.ToDecimal(rRow["MonthlyAmount"])).Sum();
                                            newRow["MonthlyAmount"] = importTotalCost - beforeImportMonthlyCost;
                                            break;
                                        case "Maliyet Farkı":
                                            newRow["OrderStatus"] = "Maliyet Farkı";
                                            beforeImportMonthlyCost = 0;
                                            if (dtBefore != null && dtBefore.Rows.Count > 0)
                                                beforeImportMonthlyCost = (from DataRow rRow in dtBefore.Rows where !rRow.IsNull("MonthlyAmount") select Convert.ToDecimal(rRow["MonthlyAmount"])).Sum();
                                            newRow["MonthlyAmount"] = importTotalCost - beforeImportMonthlyCost;
                                            break;
                                        case "İptal-Maliyet Farkı":
                                            newRow["OrderStatus"] = "İptal-Maliyet Farkı";
                                            beforeImportMonthlyCost = 0;
                                            if (dtBefore != null && dtBefore.Rows.Count > 0)
                                                beforeImportMonthlyCost = (from DataRow rRow in dtBefore.Rows where !rRow.IsNull("MonthlyAmount") select Convert.ToDecimal(rRow["MonthlyAmount"])).Sum();
                                            newRow["MonthlyAmount"] = importTotalCost - beforeImportMonthlyCost;
                                            break;
                                    }
                                    using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
                                    {
                                        ActualCostCalculate.ChosenForexCode = SLanguage.GetString("EURO");
                                        resultItem = (DataSet)ActualCostCalculate.Execute(orderRow.WorkOrderNo, orderRow.StyleCode, WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 2, DateTime.MinValue, "1");
                                        if (resultItem is DataSet && resultItem != null && resultItem.Tables.Contains("CostItemTable") && resultItem.Tables["CostItemTable"].Rows.Count > 0)
                                        {
                                            pwTable = (resultItem as DataSet).Tables["CostItemTable"];
                                            //DataRow newRow = null;
                                            foreach (DataRow dRow1 in pwTable.Select($"ParentId <> 0"))
                                            {
                                                decimal.TryParse(dRow1["Amount"].ToString(), out amount);
                                                byte.TryParse(dRow1["ActualType"].ToString(), out actualCostType);
                                                if (actualCostType == (byte)OrderCostType.PieceWork && amount > 0 && dRow1["Name"].ToString().Contains(newRow["ProcessName"].ToString()))
                                                {
                                                    newRow["TotalForexAmountEUR"] = dRow1["ForexAmount"];
                                                    decimal importTotalCostEUR = 0M;
                                                    decimal.TryParse(newRow["TotalForexAmountEUR"].ToString(), out importTotalCostEUR);

                                                    beforeImportMonthlyCost = 0;
                                                    query = new StringBuilder();
                                                    query.AppendLine($"SELECT MonthlyForexAmountEUR FROM Erp_ActualCostProcessDetail WHERE ActualCostId IN (SELECT eac.RecId FROM Erp_ActualCost eac WHERE eac.Year = {CostDate.Year} AND eac.Month < {CostDate.Month} AND eac.WorkOrderId IN");
                                                    query.AppendLine($"(SELECT RecId FROM Erp_WorkOrder ewo WHERE ewo.WorkOrderNo = '{dRow1["WorkOrderNo"]}')) AND ForexId IN (Select mf.RecId FROM Meta_Forex mf WHERE mf.ForexCode = 'EUR') AND Type=2");
                                                    dtBefore = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_ActualCostProcessDetail", query.ToString());

                                                    switch (orderRow.OrderStatus)
                                                    {
                                                        case "Açık-YT Boş":
                                                            newRow["MonthlyForexAmountEUR"] = CalcOpenOrderForPieceWorkEUR(importTotalCostEUR, orderRow.OrderQuantity, orderRow.ShipQuantity, orderRow.MonthlyShipmentQuantity);
                                                            break;
                                                        case "KZ(%100 Kar)":
                                                            newRow["MonthlyForexAmountEUR"] = 0;
                                                            break;
                                                        case "Açık-Kapalı ama ÇT<YT":
                                                            newRow["MonthlyForexAmountEUR"] = CalcOpenOrderForPieceWorkEUR(importTotalCostEUR, orderRow.OrderQuantity, orderRow.ShipQuantity, orderRow.MonthlyShipmentQuantity);
                                                            break;
                                                        case "Kapalı-Tek Yükleme":
                                                            newRow["MonthlyForexAmountEUR"] = importTotalCostEUR;
                                                            break;
                                                        case "Açık-Kapalı ama İleri Yüklemesi Var":
                                                            newRow["MonthlyForexAmountEUR"] = CalcOpenOrderForPieceWorkEUR(importTotalCostEUR, orderRow.OrderQuantity, orderRow.ShipQuantity, orderRow.MonthlyShipmentQuantity);
                                                            break;
                                                        case "Açık-Kapalı ama Hem Eski Hem İleri Yüklemesi Var":
                                                            newRow["MonthlyForexAmountEUR"] = CalcOpenOrderForPieceWorkEUR(importTotalCostEUR, orderRow.OrderQuantity, orderRow.ShipQuantity, orderRow.MonthlyShipmentQuantity);
                                                            break;
                                                        case "İptal Order":
                                                            newRow["MonthlyForexAmountEUR"] = importTotalCostEUR;
                                                            break;
                                                        case "KZ(%100 Kar)-MUH":
                                                            newRow["MonthlyForexAmountEUR"] = 0;
                                                            break;
                                                        case "Kapalı-Bu Ay Son Yükleme":
                                                            if (dtBefore != null && dtBefore.Rows.Count > 0)
                                                                beforeImportMonthlyCost = (from DataRow rRow in dtBefore.Rows where !rRow.IsNull("MonthlyForexAmountEUR") select Convert.ToDecimal(rRow["MonthlyForexAmountEUR"])).Sum();
                                                            newRow["MonthlyForexAmountEUR"] = importTotalCostEUR - beforeImportMonthlyCost;
                                                            break;
                                                        case "Maliyet Farkı":
                                                            beforeImportMonthlyCost = 0;
                                                            if (dtBefore != null && dtBefore.Rows.Count > 0)
                                                                beforeImportMonthlyCost = (from DataRow rRow in dtBefore.Rows where !rRow.IsNull("MonthlyForexAmountEUR") select Convert.ToDecimal(rRow["MonthlyForexAmountEUR"])).Sum();
                                                            newRow["MonthlyForexAmountEUR"] = importTotalCostEUR - beforeImportMonthlyCost;
                                                            break;
                                                        case "İptal-Maliyet Farkı":
                                                            if (dtBefore != null && dtBefore.Rows.Count > 0)
                                                                beforeImportMonthlyCost = (from DataRow rRow in dtBefore.Rows where !rRow.IsNull("MonthlyForexAmountEUR") select Convert.ToDecimal(rRow["MonthlyForexAmountEUR"])).Sum();
                                                            newRow["MonthlyForexAmountEUR"] = importTotalCostEUR - beforeImportMonthlyCost;
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    using (ActualCostCalculate = _container.Resolve<ActualCostModelService>())
                                    {
                                        ActualCostCalculate.ChosenForexCode = SLanguage.GetString("$");
                                        resultItem = (DataSet)ActualCostCalculate.Execute(orderRow.WorkOrderNo, orderRow.StyleCode, WOStartDate, WOFinishDate, CalculateTerm, activeSession.ActiveCompany.RecId, 2, DateTime.MinValue, "1");
                                        if (resultItem is DataSet && resultItem != null && resultItem.Tables.Contains("CostItemTable") && resultItem.Tables["CostItemTable"].Rows.Count > 0)
                                        {
                                            pwTable = (resultItem as DataSet).Tables["CostItemTable"];
                                            //DataRow newRow = null;
                                            foreach (DataRow dRow2 in pwTable.Select($"ParentId <> 0"))
                                            {
                                                decimal.TryParse(dRow2["Amount"].ToString(), out amount);
                                                byte.TryParse(dRow2["ActualType"].ToString(), out actualCostType);
                                                if (actualCostType == (byte)OrderCostType.PieceWork && amount > 0 && dRow2["Name"].ToString().Contains(newRow["ProcessName"].ToString()))
                                                {
                                                    newRow["TotalForexAmountUSD"] = dRow2["ForexAmount"];
                                                    decimal importTotalCostUSD = 0M;
                                                    decimal.TryParse(newRow["TotalForexAmountUSD"].ToString(), out importTotalCostUSD);

                                                    beforeImportMonthlyCost = 0;
                                                    query = new StringBuilder();
                                                    query.AppendLine($"SELECT MonthlyForexAmountUSD FROM Erp_ActualCostProcessDetail WHERE ActualCostId IN (SELECT eac.RecId FROM Erp_ActualCost eac WHERE eac.Year = {CostDate.Year} AND eac.Month < {CostDate.Month} AND eac.WorkOrderId IN");
                                                    query.AppendLine($"(SELECT RecId FROM Erp_WorkOrder ewo WHERE ewo.WorkOrderNo = '{dRow2["WorkOrderNo"]}')) AND ForexId IN (Select mf.RecId FROM Meta_Forex mf WHERE mf.ForexCode = '$') AND Type=2");
                                                    dtBefore = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_ActualCostProcessDetail", query.ToString());

                                                    switch (orderRow.OrderStatus)
                                                    {
                                                        case "Açık-YT Boş":
                                                            newRow["MonthlyForexAmountUSD"] = CalcOpenOrderForPieceWorkUSD(importTotalCostUSD, orderRow.OrderQuantity, orderRow.ShipQuantity, orderRow.MonthlyShipmentQuantity);
                                                            break;
                                                        case "KZ(%100 Kar)":
                                                            newRow["MonthlyForexAmountUSD"] = 0;
                                                            break;
                                                        case "Açık-Kapalı ama ÇT<YT":
                                                            newRow["MonthlyForexAmountUSD"] = CalcOpenOrderForPieceWorkUSD(importTotalCostUSD, orderRow.OrderQuantity, orderRow.ShipQuantity, orderRow.MonthlyShipmentQuantity);
                                                            break;
                                                        case "Kapalı-Tek Yükleme":
                                                            newRow["MonthlyForexAmountUSD"] = importTotalCostUSD;
                                                            break;
                                                        case "Açık-Kapalı ama İleri Yüklemesi Var":
                                                            newRow["MonthlyForexAmountUSD"] = CalcOpenOrderForPieceWorkUSD(importTotalCostUSD, orderRow.OrderQuantity, orderRow.ShipQuantity, orderRow.MonthlyShipmentQuantity);
                                                            break;
                                                        case "Açık-Kapalı ama Hem Eski Hem İleri Yüklemesi Var":
                                                            newRow["MonthlyForexAmountUSD"] = CalcOpenOrderForPieceWorkUSD(importTotalCostUSD, orderRow.OrderQuantity, orderRow.ShipQuantity, orderRow.MonthlyShipmentQuantity);
                                                            break;
                                                        case "İptal Order":
                                                            newRow["MonthlyForexAmountUSD"] = importTotalCostUSD;
                                                            break;
                                                        case "KZ(%100 Kar)-MUH":
                                                            newRow["MonthlyForexAmountUSD"] = 0;
                                                            break;
                                                        case "Kapalı-Bu Ay Son Yükleme":
                                                            if (dtBefore != null && dtBefore.Rows.Count > 0)
                                                                beforeImportMonthlyCost = (from DataRow rRow in dtBefore.Rows where !rRow.IsNull("MonthlyForexAmountUSD") select Convert.ToDecimal(rRow["MonthlyForexAmountUSD"])).Sum();
                                                            newRow["MonthlyForexAmountUSD"] = importTotalCostUSD - beforeImportMonthlyCost;
                                                            break;
                                                        case "Maliyet Farkı":
                                                            beforeImportMonthlyCost = 0;
                                                            if (dtBefore != null && dtBefore.Rows.Count > 0)
                                                                beforeImportMonthlyCost = (from DataRow rRow in dtBefore.Rows where !rRow.IsNull("MonthlyForexAmountUSD") select Convert.ToDecimal(rRow["MonthlyForexAmountUSD"])).Sum();
                                                            newRow["MonthlyForexAmountUSD"] = importTotalCostUSD - beforeImportMonthlyCost;
                                                            break;
                                                        case "İptal-Maliyet Farkı":
                                                            if (dtBefore != null && dtBefore.Rows.Count > 0)
                                                                beforeImportMonthlyCost = (from DataRow rRow in dtBefore.Rows where !rRow.IsNull("MonthlyForexAmountUSD") select Convert.ToDecimal(rRow["MonthlyForexAmountUSD"])).Sum();
                                                            newRow["MonthlyForexAmountUSD"] = importTotalCostUSD - beforeImportMonthlyCost;
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            grdPieceWorkDetail.ItemsSource = PieceWorkDetailTable;
        }

        private decimal CalcOpenOrderForPieceWorkTL(decimal importTotalCost, decimal orderQuantity, decimal shipQuantity, decimal monthlyShipmentQuantity)
        {
            decimal importMonthlyUnitPriceCost = 0M;
            importMonthlyUnitPriceCost = 0M;
            if (DoubleUtil.CompareFloat(">", orderQuantity, 0, FieldUsage.Quantity) && shipQuantity < orderQuantity)
            {
                importMonthlyUnitPriceCost = importTotalCost / orderQuantity;
            }
            //Yüklenen >= Sipariş
            if (DoubleUtil.CompareFloat(">", shipQuantity, 0, FieldUsage.Quantity) && orderQuantity > 0 && shipQuantity >= orderQuantity)
            {
                importMonthlyUnitPriceCost = importTotalCost / shipQuantity;
            }
            //Dönem Maliyeti
            return Math.Round(importMonthlyUnitPriceCost * monthlyShipmentQuantity, amountDec);
        }
        private decimal CalcOpenOrderForPieceWorkEUR(decimal importTotalCostEUR, decimal orderQuantity, decimal shipQuantity, decimal monthlyShipmentQuantity)
        {
            decimal importMonthlyUnitPriceCost = 0M;
            importMonthlyUnitPriceCost = 0M;
            if (DoubleUtil.CompareFloat(">", orderQuantity, 0, FieldUsage.Quantity) && shipQuantity < orderQuantity)
            {
                importMonthlyUnitPriceCost = importTotalCostEUR / orderQuantity;
            }
            //Yüklenen >= Sipariş
            if (DoubleUtil.CompareFloat(">", shipQuantity, 0, FieldUsage.Quantity) && orderQuantity > 0 && shipQuantity >= orderQuantity)
            {
                importMonthlyUnitPriceCost = importTotalCostEUR / shipQuantity;
            }
            //Dönem Maliyeti
            return Math.Round(importMonthlyUnitPriceCost * monthlyShipmentQuantity, amountDec);
        }
        private decimal CalcOpenOrderForPieceWorkUSD(decimal importTotalCostUSD, decimal orderQuantity, decimal shipQuantity, decimal monthlyShipmentQuantity)
        {
            decimal importMonthlyUnitPriceCost = 0M;
            importMonthlyUnitPriceCost = 0M;
            if (DoubleUtil.CompareFloat(">", orderQuantity, 0, FieldUsage.Quantity) && shipQuantity < orderQuantity)
            {
                importMonthlyUnitPriceCost = importTotalCostUSD / orderQuantity;
            }
            //Yüklenen >= Sipariş
            if (DoubleUtil.CompareFloat(">", shipQuantity, 0, FieldUsage.Quantity) && orderQuantity > 0 && shipQuantity >= orderQuantity)
            {
                importMonthlyUnitPriceCost = importTotalCostUSD / shipQuantity;
            }
            //Dönem Maliyeti
            return Math.Round(importMonthlyUnitPriceCost * monthlyShipmentQuantity, amountDec);
        }

        private void InitializePieceWorkGrid()
        {
            if (grdPieceWorkDetail == null) return;
            PieceWorkDetailColumnCollection = new ReceiptColumnCollection
            {
                new ReceiptColumn { ColumnName = "WorkOrderNo", IsFixedColumn = true, Caption = SLanguage.GetString("Order No"), Width = 100, DataType = UdtTypes.GetUdtSystemType("UdtCode") },
                new ReceiptColumn { ColumnName = "OrderStatus", IsFixedColumn = true, Caption = SLanguage.GetString("Order Durumu"), Width = 100, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn { ColumnName = "ProcessName", IsFixedColumn = true, Caption = SLanguage.GetString("Proses Adı"), Width = 100, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn { ColumnName = "Type", IsFixedColumn = true, Caption = SLanguage.GetString("Maliyet Tipi"), Width = 100, DataType = UdtTypes.GetUdtSystemType("UdtCode"), IsVisible = false },
                new ReceiptColumn { ColumnName = "ForexCode", IsFixedColumn = true, Caption = SLanguage.GetString("Döviz"), Width = 100, DataType = UdtTypes.GetUdtSystemType("UdtCode"), IsVisible = false },
                new ReceiptColumn { ColumnName = "MonthlyAmount", Caption = SLanguage.GetString("Dönem Maliyet"), Width = 90, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyForexAmountEUR", Caption = SLanguage.GetString("Dönem EURO Maliyeti"), Width = 90, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyForexAmountUSD", Caption = SLanguage.GetString("Dönem USD Maliyeti"), Width = 90, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TotalAmount", Caption = SLanguage.GetString("Toplam Maliyet"), Width = 90, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TotalForexAmountEUR", Caption = SLanguage.GetString("Toplam EURO Maliyeti"), Width = 90, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TotalForexAmountUSD", Caption = SLanguage.GetString("Toplam USD Maliyeti"), Width = 90, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
            };
            grdPieceWorkDetail.ColumnDefinitions = PieceWorkDetailColumnCollection;
        }
        private void InitializePieceWorkDataColumns()
        {
            if (PieceWorkDetailTable == null) return;
            PieceWorkDetailTable.Columns.Add(new DataColumn { ColumnName = "WorkOrderNo", DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            PieceWorkDetailTable.Columns.Add(new DataColumn { ColumnName = "ProcessName", DataType = UdtTypes.GetUdtSystemType("UdtName") });
            PieceWorkDetailTable.Columns.Add(new DataColumn { ColumnName = "OrderStatus", DataType = UdtTypes.GetUdtSystemType("UdtName") });
            PieceWorkDetailTable.Columns.Add(new DataColumn { ColumnName = "ForexCode", DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            PieceWorkDetailTable.Columns.Add(new DataColumn { ColumnName = "Type", DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            PieceWorkDetailTable.Columns.Add(new DataColumn { ColumnName = "TotalAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            PieceWorkDetailTable.Columns.Add(new DataColumn { ColumnName = "TotalForexAmountEUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            PieceWorkDetailTable.Columns.Add(new DataColumn { ColumnName = "TotalForexAmountUSD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            PieceWorkDetailTable.Columns.Add(new DataColumn { ColumnName = "MonthlyAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            PieceWorkDetailTable.Columns.Add(new DataColumn { ColumnName = "MonthlyForexAmountEUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            PieceWorkDetailTable.Columns.Add(new DataColumn { ColumnName = "MonthlyForexAmountUSD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
        }
        private void CalcOpenOrder()
        {
            //Yüklenen < Sipariş                                        
            if (DoubleUtil.CompareFloat(">", OrderQuantity, 0, FieldUsage.Quantity) && ShipQuantity < OrderQuantity)
            {
                MonthlyUnitPriceCost = TotalCost / OrderQuantity;
                MonthlyUnitPriceForexCostEUR = TotalForexCostEUR / OrderQuantity;
                MonthlyUnitPriceForexCostUSD = TotalForexCostUSD / OrderQuantity;
            }
            //Yüklenen >= Sipariş
            if (DoubleUtil.CompareFloat(">", ShipQuantity, 0, FieldUsage.Quantity) && OrderQuantity > 0 && ShipQuantity >= OrderQuantity)
            {
                MonthlyUnitPriceCost = TotalCost / ShipQuantity;
                MonthlyUnitPriceForexCostEUR = TotalForexCostEUR / ShipQuantity;
                MonthlyUnitPriceForexCostUSD = TotalForexCostUSD / ShipQuantity;
            }
            MonthlyUnitPriceCost = Math.Round(MonthlyUnitPriceCost, amountDec);
            MonthlyUnitPriceForexCostEUR = Math.Round(MonthlyUnitPriceForexCostEUR, amountDec);
            MonthlyUnitPriceForexCostUSD = Math.Round(MonthlyUnitPriceForexCostUSD, amountDec);
            //Dönem Maliyeti - Kar/Zarar
            MonthlyCost = MonthlyUnitPriceCost * MonthlyShipQuantity;
            MonthlyForexCostEUR = MonthlyUnitPriceForexCostEUR * MonthlyShipQuantity;
            MonthlyForexCostUSD = MonthlyUnitPriceForexCostUSD * MonthlyShipQuantity;
            MonthlyProfit = MonthlySalesTotal - MonthlyCost;
            MonthlyForexProfitEUR = MonthlySalesTotalForexEUR - MonthlyForexCostEUR;
            MonthlyForexProfitUSD = MonthlySalesTotalForexUSD - MonthlyForexCostUSD;
        }
        private void CalcImportDepartmentCostsForOpenOrder(DataRow newRow)
        {
            foreach (DataColumn dtcol in tempCostTable.Columns)
            {
                if (totalCostColList.Contains(dtcol.ColumnName))
                {
                    decimal importTotalCost = 0M, importMonthlyUnitPriceCost = 0M;
                    decimal.TryParse(newRow[dtcol.ColumnName].ToString(), out importTotalCost);
                    if (DoubleUtil.CompareFloat(">", OrderQuantity, 0, FieldUsage.Quantity) && ShipQuantity < OrderQuantity)
                    {
                        importMonthlyUnitPriceCost = importTotalCost / OrderQuantity;
                    }
                    //Yüklenen >= Sipariş
                    if (DoubleUtil.CompareFloat(">", ShipQuantity, 0, FieldUsage.Quantity) && OrderQuantity > 0 && ShipQuantity >= OrderQuantity)
                    {
                        importMonthlyUnitPriceCost = importTotalCost / ShipQuantity;
                    }
                    //Dönem Maliyeti
                    newRow[$"Monthly{dtcol.ColumnName}"] = Math.Round(importMonthlyUnitPriceCost * MonthlyShipQuantity, amountDec);
                }
                if (totalCostColEuroList.Contains(dtcol.ColumnName))
                {
                    decimal importTotalEURCost = 0M, importMonthlyUnitPriceForexCostEUR = 0M;
                    decimal.TryParse(newRow[dtcol.ColumnName].ToString(), out importTotalEURCost);
                    if (DoubleUtil.CompareFloat(">", OrderQuantity, 0, FieldUsage.Quantity) && ShipQuantity < OrderQuantity)
                    {
                        importMonthlyUnitPriceForexCostEUR = importTotalEURCost / OrderQuantity;
                    }
                    //Yüklenen >= Sipariş
                    if (DoubleUtil.CompareFloat(">", ShipQuantity, 0, FieldUsage.Quantity) && OrderQuantity > 0 && ShipQuantity >= OrderQuantity)
                    {
                        importMonthlyUnitPriceForexCostEUR = importTotalEURCost / ShipQuantity;
                    }
                    //Dönem Maliyeti
                    newRow[$"Monthly{dtcol.ColumnName}"] = Math.Round(importMonthlyUnitPriceForexCostEUR * MonthlyShipQuantity, amountDec);
                }
                if (totalCostColUSDList.Contains(dtcol.ColumnName))
                {
                    decimal importTotalUSDCost = 0M, importMonthlyUnitPriceForexCostUSD = 0M;
                    decimal.TryParse(newRow[dtcol.ColumnName].ToString(), out importTotalUSDCost);
                    if (DoubleUtil.CompareFloat(">", OrderQuantity, 0, FieldUsage.Quantity) && ShipQuantity < OrderQuantity)
                    {
                        importMonthlyUnitPriceForexCostUSD = importTotalUSDCost / OrderQuantity;
                    }
                    //Yüklenen >= Sipariş
                    if (DoubleUtil.CompareFloat(">", ShipQuantity, 0, FieldUsage.Quantity) && OrderQuantity > 0 && ShipQuantity >= OrderQuantity)
                    {
                        importMonthlyUnitPriceForexCostUSD = importTotalUSDCost / ShipQuantity;
                    }
                    //Dönem Maliyeti
                    newRow[$"Monthly{dtcol.ColumnName}"] = Math.Round(importMonthlyUnitPriceForexCostUSD * MonthlyShipQuantity, amountDec);
                }
            }
        }
        private void FillImportDepertmantNames()
        {
            if (ImportCostNames != null && ImportCostNames.Count == 0)
            {
                ImportCostNames.Add("YarnCostAmount2", "TotalCost");
                ImportCostNames.Add("MonthlyYarnCostAmount2", "MonthlyCost");
                ImportCostNames.Add("YarnDyeCostAmount2", "TotalCost");
                ImportCostNames.Add("MonthlyYarnDyeCostAmount2", "MonthlyCost");
                ImportCostNames.Add("FabricCostAmount2", "TotalCost");
                ImportCostNames.Add("MonthlyFabricCostAmount2", "MonthlyCost");
                ImportCostNames.Add("FabricDyeCostAmount2", "TotalCost");
                ImportCostNames.Add("MonthlyFabricDyeCostAmount2", "MonthlyCost");
                ImportCostNames.Add("FabricPrintCostAmount2", "TotalCost");
                ImportCostNames.Add("MonthlyFabricPrintCostAmount2", "MonthlyCost");
                ImportCostNames.Add("FabricOther1CostAmount2", "TotalCost");
                ImportCostNames.Add("MonthlyFabricOther1CostAmount2", "MonthlyCost");
                ImportCostNames.Add("FabricOther2CostAmount2", "TotalCost");
                ImportCostNames.Add("MonthlyFabricOther2CostAmount2", "MonthlyCost");
                ImportCostNames.Add("TrimCostAmount2", "TotalCost");
                ImportCostNames.Add("MonthlyTrimCostAmount2", "MonthlyCost");
                ImportCostNames.Add("ReturnCostAmount2", "TotalCost");
                ImportCostNames.Add("MonthlyReturnCostAmount2", "MonthlyCost");
                ImportCostNames.Add("CutCostAmount2", "TotalCost");
                ImportCostNames.Add("MonthlyCutCostAmount2", "MonthlyCost");
                ImportCostNames.Add("PieceWorkCostAmount2", "TotalCost");
                ImportCostNames.Add("MonthlyPieceWorkCostAmount2", "MonthlyCost");
                ImportCostNames.Add("ChemicalCostAmount2", "TotalCost");
                ImportCostNames.Add("MonthlyChemicalCostAmount2", "MonthlyCost");
                ImportCostNames.Add("YarnCostAmount2EUR", "TotalEUR");
                ImportCostNames.Add("YarnCostAmount2USD", "TotalUSD");
                ImportCostNames.Add("MonthlyYarnCostAmount2EUR", "MonthlyEUR");
                ImportCostNames.Add("MonthlyYarnCostAmount2USD", "MonthlyUSD");
                ImportCostNames.Add("YarnDyeCostAmount2EUR", "TotalEUR");
                ImportCostNames.Add("YarnDyeCostAmount2USD", "TotalUSD");
                ImportCostNames.Add("MonthlyYarnDyeCostAmount2EUR", "MonthlyEUR");
                ImportCostNames.Add("MonthlyYarnDyeCostAmount2USD", "MonthlyUSD");
                ImportCostNames.Add("FabricCostAmount2EUR", "TotalEUR");
                ImportCostNames.Add("FabricCostAmount2USD", "TotalUSD");
                ImportCostNames.Add("MonthlyFabricCostAmount2EUR", "MonthlyEUR");
                ImportCostNames.Add("MonthlyFabricCostAmount2USD", "MonthlyUSD");
                ImportCostNames.Add("FabricDyeCostAmount2EUR", "TotalEUR");
                ImportCostNames.Add("FabricDyeCostAmount2USD", "TotalUSD");
                ImportCostNames.Add("MonthlyFabricDyeCostAmount2EUR", "MonthlyEUR");
                ImportCostNames.Add("MonthlyFabricDyeCostAmount2USD", "MonthlyUSD");
                ImportCostNames.Add("FabricPrintCostAmount2EUR", "TotalEUR");
                ImportCostNames.Add("FabricPrintCostAmount2USD", "TotalUSD");
                ImportCostNames.Add("MonthlyFabricPrintCostAmount2EUR", "MonthlyEUR");
                ImportCostNames.Add("MonthlyFabricPrintCostAmount2USD", "MonthlyUSD");
                ImportCostNames.Add("FabricOther1CostAmount2EUR", "TotalEUR");
                ImportCostNames.Add("FabricOther1CostAmount2USD", "TotalUSD");
                ImportCostNames.Add("MonthlyFabricOther1CostAmount2EUR", "MonthlyEUR");
                ImportCostNames.Add("MonthlyFabricOther1CostAmount2USD", "MonthlyUSD");
                ImportCostNames.Add("FabricOther2CostAmount2EUR", "TotalEUR");
                ImportCostNames.Add("FabricOther2CostAmount2USD", "TotalUSD");
                ImportCostNames.Add("MonthlyFabricOther2CostAmount2EUR", "MonthlyEUR");
                ImportCostNames.Add("MonthlyFabricOther2CostAmount2USD", "MonthlyUSD");
                ImportCostNames.Add("TrimCostAmount2EUR", "TotalEUR");
                ImportCostNames.Add("TrimCostAmount2USD", "TotalUSD");
                ImportCostNames.Add("MonthlyTrimCostAmount2EUR", "MonthlyEUR");
                ImportCostNames.Add("MonthlyTrimCostAmount2USD", "MonthlyUSD");
                ImportCostNames.Add("ReturnCostAmount2EUR", "TotalEUR");
                ImportCostNames.Add("ReturnCostAmount2USD", "TotalUSD");
                ImportCostNames.Add("MonthlyReturnCostAmount2EUR", "MonthlyEUR");
                ImportCostNames.Add("MonthlyReturnCostAmount2USD", "MonthlyUSD");
                ImportCostNames.Add("CutCostAmount2EUR", "TotalEUR");
                ImportCostNames.Add("CutCostAmount2USD", "TotalUSD");
                ImportCostNames.Add("MonthlyCutCostAmount2EUR", "MonthlyEUR");
                ImportCostNames.Add("MonthlyCutCostAmount2USD", "MonthlyUSD");
                ImportCostNames.Add("PieceWorkCostAmount2EUR", "TotalEUR");
                ImportCostNames.Add("PieceWorkCostAmount2USD", "TotalUSD");
                ImportCostNames.Add("MonthlyPieceWorkCostAmount2EUR", "MonthlyEUR");
                ImportCostNames.Add("MonthlyPieceWorkCostAmount2USD", "MonthlyUSD");
                ImportCostNames.Add("ChemicalCostAmount2EUR", "TotalEUR");
                ImportCostNames.Add("ChemicalCostAmount2USD", "TotalUSD");
                ImportCostNames.Add("MonthlyChemicalCostAmount2EUR", "MonthlyEUR");
                ImportCostNames.Add("MonthlyChemicalCostAmount2USD", "MonthlyUSD");
            }
            foreach (var item in ImportCostNames)
            {
                if (item.Value.Contains("TotalCost"))
                {
                    totalCostColList.Add(item.Key);
                }
            }
            foreach (var item in ImportCostNames)
            {
                if (item.Value.Contains("TotalEUR"))
                {
                    totalCostColEuroList.Add(item.Key);
                }
            }
            foreach (var item in ImportCostNames)
            {
                if (item.Value.Contains("TotalUSD"))
                {
                    totalCostColUSDList.Add(item.Key);
                }
            }
        }
        private void OnSaveActualCostCommand(ISysCommandParam obj)
        {
            if (TempCostTable != null && TempCostTable.Rows.Count > 0)
            {
                DataTable actualDt = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_ActualCost", $"Select RecId from Erp_ActualCost where CompanyId = {activeSession.ActiveCompany.RecId} and Year = {CostDate.Year} and Month > {CostDate.Month}");
                if (actualDt != null && actualDt.Rows.Count > 0)
                {
                    SysMng.ActWndMng.ShowMsg(SLanguage.GetString("İleri Aya Ait Kayıt Olduğu Bu Ay İçin Yeni Kayıt Yapılamaz."), ConstantStr.Information);
                    return;
                }
                DataTable actDt = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_ActualCost", $"Select RecId from Erp_ActualCost where CompanyId = {activeSession.ActiveCompany.RecId} and Year = {CostDate.Year} and Month = {CostDate.Month}");
                if (actDt != null && actDt.Rows.Count > 0)
                {
                    if (SysMng.Instance.ActWndMng.ShowMsg(SLanguage.GetString("Bu Aya Ait Kayıtlı Order Maliyeti Mevcut. Devam Edilsin Mi ?"), ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.YesNo, Common.InformationMessages.MessageBoxImage.Warning) == Common.InformationMessages.MessageBoxResult.Yes)
                    {
                        UtilityFunctions.SqlCustomNonQuery(ActiveBO.Connection, ActiveBO.Transaction, $"Delete From Erp_ActualCost where CompanyId = {activeSession.ActiveCompany.RecId} and Year = {CostDate.Year} and Month = {CostDate.Month}");
                        SaveActualCost();
                    }
                }
                else
                    SaveActualCost();
            }
        }
        private void SaveActualCost()
        {
            DataTable actualDt = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_ActualCost", $"Select RecId from Erp_ActualCost where CompanyId = {activeSession.ActiveCompany.RecId} and Year = {CostDate.Year} and Month = {CostDate.Month}");
            if (actualDt != null && actualDt.Rows.Count > 0)
            {
                SysMng.ActWndMng.ShowMsg(SLanguage.GetString("Bu Aya Ait Kayıt Olduğu Bu Ay İçin Yeni Kayıt Yapılamaz."), ConstantStr.Information);
                return;
            }
            foreach (DataRow tempRow in TempCostTable.Rows)
            {
                #region Erp_ActualCost
                DataTable dtForex = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Meta_Forex", $"Select RecId from Meta_Forex where ForexCode = '{tempRow["ForexCode"]}'");
                actualCostBO.NewRecord();
                actualCostBO.CurrentRow["CurrentAccountId"] = tempRow["CurrentAccountId"];
                actualCostBO.CurrentRow["ApprovedExplanation"] = tempRow["IsClosed"];
                long workOrderId = 0;
                long.TryParse(tempRow["WorkOrderId"].ToString(), out workOrderId);
                if (workOrderId == 0)
                {
                    actualCostBO.CurrentRow["WorkOrderId"] = DBNull.Value;
                    actualCostBO.CurrentRow["WorkOrderItemId"] = DBNull.Value;
                    actualCostBO.CurrentRow["Explanation"] = "Muhtelif Satışlar";
                }
                else
                {
                    actualCostBO.CurrentRow["WorkOrderId"] = tempRow["WorkOrderId"];
                    actualCostBO.CurrentRow["WorkOrderItemId"] = tempRow["WorkOrderItemId"];
                }
                actualCostBO.CurrentRow["Year"] = CostDate.Year;
                actualCostBO.CurrentRow["Month"] = CostDate.Month;
                if (dtForex.Rows.Count > 0)
                    actualCostBO.CurrentRow["ForexId"] = dtForex.Rows[0]["RecId"];
                actualCostBO.CurrentRow["ForexUnitPrice"] = tempRow["ForexUnitPrice"];
                actualCostBO.CurrentRow["ForexAmount"] = tempRow["ForexCostAmount"];

                actualCostBO.CurrentRow["Quantity"] = tempRow["Quantity"];
                actualCostBO.CurrentRow["CuttingQuantity"] = tempRow["CuttingQuantity"];
                actualCostBO.CurrentRow["ShipmentQuantity"] = tempRow["ShipmentQuantity"];
                actualCostBO.CurrentRow["RemainingQuantity"] = tempRow["RemainingQuantity"];
                if (tempRow["ShipmentDate"] != DBNull.Value)
                    actualCostBO.CurrentRow["ShipmentDate"] = tempRow["ShipmentDate"];

                actualCostBO.CurrentRow["YarnCostAmount"] = tempRow["YarnCostAmount"];
                actualCostBO.CurrentRow["YarnDyeCostAmount"] = tempRow["YarnDyeCostAmount"];
                actualCostBO.CurrentRow["FabricCostAmount"] = tempRow["FabricCostAmount"];
                actualCostBO.CurrentRow["FabricDyeCostAmount"] = tempRow["FabricDyeCostAmount"];
                actualCostBO.CurrentRow["FabricPrintCostAmount"] = tempRow["FabricPrintCostAmount"];
                actualCostBO.CurrentRow["FabricOther1CostAmount"] = tempRow["FabricOther1CostAmount"];
                actualCostBO.CurrentRow["FabricOther2CostAmount"] = tempRow["FabricOther2CostAmount"];
                actualCostBO.CurrentRow["TrimCostAmount"] = tempRow["TrimCostAmount"];
                actualCostBO.CurrentRow["ReturnCostAmount"] = tempRow["ReturnCostAmount"];
                actualCostBO.CurrentRow["CutCostAmount"] = tempRow["CutCostAmount"];
                actualCostBO.CurrentRow["PieceWorkCostAmount"] = tempRow["PieceWorkCostAmount"];
                actualCostBO.CurrentRow["ChemicalCostAmount"] = tempRow["ChemicalCostAmount"];

                actualCostBO.CurrentRow["YarnCostAmount2"] = tempRow["YarnCostAmount2"];
                actualCostBO.CurrentRow["YarnDyeCostAmount2"] = tempRow["YarnDyeCostAmount2"];
                actualCostBO.CurrentRow["FabricCostAmount2"] = tempRow["FabricCostAmount2"];
                actualCostBO.CurrentRow["FabricDyeCostAmount2"] = tempRow["FabricDyeCostAmount2"];
                actualCostBO.CurrentRow["FabricPrintCostAmount2"] = tempRow["FabricPrintCostAmount2"];
                actualCostBO.CurrentRow["FabricOther1CostAmount2"] = tempRow["FabricOther1CostAmount2"];
                actualCostBO.CurrentRow["FabricOther2CostAmount2"] = tempRow["FabricOther2CostAmount2"];
                actualCostBO.CurrentRow["TrimCostAmount2"] = tempRow["TrimCostAmount2"];
                actualCostBO.CurrentRow["ReturnCostAmount2"] = tempRow["ReturnCostAmount2"];
                actualCostBO.CurrentRow["CutCostAmount2"] = tempRow["CutCostAmount2"];
                actualCostBO.CurrentRow["PieceWorkCostAmount2"] = tempRow["PieceWorkCostAmount2"];
                actualCostBO.CurrentRow["ChemicalCostAmount2"] = tempRow["ChemicalCostAmount2"];

                actualCostBO.CurrentRow["YarnCostAmount2EUR"] = tempRow["YarnCostAmount2EUR"];
                actualCostBO.CurrentRow["YarnDyeCostAmount2EUR"] = tempRow["YarnDyeCostAmount2EUR"];
                actualCostBO.CurrentRow["FabricCostAmount2EUR"] = tempRow["FabricCostAmount2EUR"];
                actualCostBO.CurrentRow["FabricDyeCostAmount2EUR"] = tempRow["FabricDyeCostAmount2EUR"];
                actualCostBO.CurrentRow["FabricPrintCostAmount2EUR"] = tempRow["FabricPrintCostAmount2EUR"];
                actualCostBO.CurrentRow["FabricOther1CostAmount2EUR"] = tempRow["FabricOther1CostAmount2EUR"];
                actualCostBO.CurrentRow["FabricOther2CostAmount2EUR"] = tempRow["FabricOther2CostAmount2EUR"];
                actualCostBO.CurrentRow["TrimCostAmount2EUR"] = tempRow["TrimCostAmount2EUR"];
                actualCostBO.CurrentRow["ReturnCostAmount2EUR"] = tempRow["ReturnCostAmount2EUR"];
                actualCostBO.CurrentRow["CutCostAmount2EUR"] = tempRow["CutCostAmount2EUR"];
                actualCostBO.CurrentRow["PieceWorkCostAmount2EUR"] = tempRow["PieceWorkCostAmount2EUR"];
                actualCostBO.CurrentRow["ChemicalCostAmount2EUR"] = tempRow["ChemicalCostAmount2EUR"];

                actualCostBO.CurrentRow["YarnCostAmount2USD"] = tempRow["YarnCostAmount2USD"];
                actualCostBO.CurrentRow["YarnDyeCostAmount2USD"] = tempRow["YarnDyeCostAmount2USD"];
                actualCostBO.CurrentRow["FabricCostAmount2USD"] = tempRow["FabricCostAmount2USD"];
                actualCostBO.CurrentRow["FabricDyeCostAmount2USD"] = tempRow["FabricDyeCostAmount2USD"];
                actualCostBO.CurrentRow["FabricPrintCostAmount2USD"] = tempRow["FabricPrintCostAmount2USD"];
                actualCostBO.CurrentRow["FabricOther1CostAmount2USD"] = tempRow["FabricOther1CostAmount2USD"];
                actualCostBO.CurrentRow["FabricOther2CostAmount2USD"] = tempRow["FabricOther2CostAmount2USD"];
                actualCostBO.CurrentRow["TrimCostAmount2USD"] = tempRow["TrimCostAmount2USD"];
                actualCostBO.CurrentRow["ReturnCostAmount2USD"] = tempRow["ReturnCostAmount2USD"];
                actualCostBO.CurrentRow["CutCostAmount2USD"] = tempRow["CutCostAmount2USD"];
                actualCostBO.CurrentRow["PieceWorkCostAmount2USD"] = tempRow["PieceWorkCostAmount2USD"];
                actualCostBO.CurrentRow["ChemicalCostAmount2USD"] = tempRow["ChemicalCostAmount2USD"];

                actualCostBO.CurrentRow["MonthlyYarnCostAmount2"] = tempRow["MonthlyYarnCostAmount2"];
                actualCostBO.CurrentRow["MonthlyYarnDyeCostAmount2"] = tempRow["MonthlyYarnDyeCostAmount2"];
                actualCostBO.CurrentRow["MonthlyFabricCostAmount2"] = tempRow["MonthlyFabricCostAmount2"];
                actualCostBO.CurrentRow["MonthlyFabricDyeCostAmount2"] = tempRow["MonthlyFabricDyeCostAmount2"];
                actualCostBO.CurrentRow["MonthlyFabricPrintCostAmount2"] = tempRow["MonthlyFabricPrintCostAmount2"];
                actualCostBO.CurrentRow["MonthlyFabricOther1CostAmount2"] = tempRow["MonthlyFabricOther1CostAmount2"];
                actualCostBO.CurrentRow["MonthlyFabricOther2CostAmount2"] = tempRow["MonthlyFabricOther2CostAmount2"];
                actualCostBO.CurrentRow["MonthlyTrimCostAmount2"] = tempRow["MonthlyTrimCostAmount2"];
                actualCostBO.CurrentRow["MonthlyReturnCostAmount2"] = tempRow["MonthlyReturnCostAmount2"];
                actualCostBO.CurrentRow["MonthlyCutCostAmount2"] = tempRow["MonthlyCutCostAmount2"];
                actualCostBO.CurrentRow["MonthlyPieceWorkCostAmount2"] = tempRow["MonthlyPieceWorkCostAmount2"];
                actualCostBO.CurrentRow["MonthlyChemicalCostAmount2"] = tempRow["MonthlyChemicalCostAmount2"];

                actualCostBO.CurrentRow["MonthlyYarnCostAmount2EUR"] = tempRow["MonthlyYarnCostAmount2EUR"];
                actualCostBO.CurrentRow["MonthlyYarnDyeCostAmount2EUR"] = tempRow["MonthlyYarnDyeCostAmount2EUR"];
                actualCostBO.CurrentRow["MonthlyFabricCostAmount2EUR"] = tempRow["MonthlyFabricCostAmount2EUR"];
                actualCostBO.CurrentRow["MonthlyFabricDyeCostAmount2EUR"] = tempRow["MonthlyFabricDyeCostAmount2EUR"];
                actualCostBO.CurrentRow["MonthlyFabricPrintCostAmount2EUR"] = tempRow["MonthlyFabricPrintCostAmount2EUR"];
                actualCostBO.CurrentRow["MonthlyFabricOther1CostAmount2EUR"] = tempRow["MonthlyFabricOther1CostAmount2EUR"];
                actualCostBO.CurrentRow["MonthlyFabricOther2CostAmount2EUR"] = tempRow["MonthlyFabricOther2CostAmount2EUR"];
                actualCostBO.CurrentRow["MonthlyTrimCostAmount2EUR"] = tempRow["MonthlyTrimCostAmount2EUR"];
                actualCostBO.CurrentRow["MonthlyReturnCostAmount2EUR"] = tempRow["MonthlyReturnCostAmount2EUR"];
                actualCostBO.CurrentRow["MonthlyCutCostAmount2EUR"] = tempRow["MonthlyCutCostAmount2EUR"];
                actualCostBO.CurrentRow["MonthlyPieceWorkCostAmount2EUR"] = tempRow["MonthlyPieceWorkCostAmount2EUR"];
                actualCostBO.CurrentRow["MonthlyChemicalCostAmount2EUR"] = tempRow["MonthlyChemicalCostAmount2EUR"];

                actualCostBO.CurrentRow["MonthlyYarnCostAmount2USD"] = tempRow["MonthlyYarnCostAmount2USD"];
                actualCostBO.CurrentRow["MonthlyYarnDyeCostAmount2USD"] = tempRow["MonthlyYarnDyeCostAmount2USD"];
                actualCostBO.CurrentRow["MonthlyFabricCostAmount2USD"] = tempRow["MonthlyFabricCostAmount2USD"];
                actualCostBO.CurrentRow["MonthlyFabricDyeCostAmount2USD"] = tempRow["MonthlyFabricDyeCostAmount2USD"];
                actualCostBO.CurrentRow["MonthlyFabricPrintCostAmount2USD"] = tempRow["MonthlyFabricPrintCostAmount2USD"];
                actualCostBO.CurrentRow["MonthlyFabricOther1CostAmount2USD"] = tempRow["MonthlyFabricOther1CostAmount2USD"];
                actualCostBO.CurrentRow["MonthlyFabricOther2CostAmount2USD"] = tempRow["MonthlyFabricOther2CostAmount2USD"];
                actualCostBO.CurrentRow["MonthlyTrimCostAmount2USD"] = tempRow["MonthlyTrimCostAmount2USD"];
                actualCostBO.CurrentRow["MonthlyReturnCostAmount2USD"] = tempRow["MonthlyReturnCostAmount2USD"];
                actualCostBO.CurrentRow["MonthlyCutCostAmount2USD"] = tempRow["MonthlyCutCostAmount2USD"];
                actualCostBO.CurrentRow["MonthlyPieceWorkCostAmount2USD"] = tempRow["MonthlyPieceWorkCostAmount2USD"];
                actualCostBO.CurrentRow["MonthlyChemicalCostAmount2USD"] = tempRow["MonthlyChemicalCostAmount2USD"];

                actualCostBO.CurrentRow["MonthlyShipmentQuantity"] = tempRow["MonthlyShipmentQuantity"];
                actualCostBO.CurrentRow["MonthlySalesCost"] = tempRow["MonthlySalesCost"];
                actualCostBO.CurrentRow["MonthlySalesForexCost"] = tempRow["MonthlySalesForexCostEUR"];
                actualCostBO.CurrentRow["MonthlySalesForex2Cost"] = tempRow["MonthlySalesForexCostUSD"];
                actualCostBO.CurrentRow["MonthlyUnitPriceCost"] = tempRow["MonthlyUnitPriceCost"];
                actualCostBO.CurrentRow["MonthlyUnitPriceForexCost"] = tempRow["MonthlyUnitPriceForexCostEUR"];
                actualCostBO.CurrentRow["MonthlyUnitPriceForex2Cost"] = tempRow["MonthlyUnitPriceForexCostUSD"];
                actualCostBO.CurrentRow["MonthlyCost"] = tempRow["MonthlyCost"];
                actualCostBO.CurrentRow["MonthlyForexCost"] = tempRow["MonthlyForexCostEUR"];
                actualCostBO.CurrentRow["MonthlyForex2Cost"] = tempRow["MonthlyForexCostUSD"];
                actualCostBO.CurrentRow["MonthlyProfit"] = tempRow["MonthlyProfit"];
                actualCostBO.CurrentRow["MonthlyForexProfit"] = tempRow["MonthlyForexProfitEUR"];
                actualCostBO.CurrentRow["MonthlyForex2Profit"] = tempRow["MonthlyForexProfitUSD"];

                actualCostBO.CurrentRow["TotalSalesCost"] = tempRow["TotalSalesCost"];
                actualCostBO.CurrentRow["TotalSalesForexCost"] = tempRow["TotalSalesForexCostEUR"];
                actualCostBO.CurrentRow["TotalSalesForex2Cost"] = tempRow["TotalSalesForexCostUSD"];
                actualCostBO.CurrentRow["TotalCost"] = tempRow["TotalCost"];
                actualCostBO.CurrentRow["TotalForexCost"] = tempRow["TotalForexCostEUR"];
                actualCostBO.CurrentRow["TotalForex2Cost"] = tempRow["TotalForexCostUSD"];
                #endregion

                if (PieceWorkDetailTable != null && PieceWorkDetailTable.Rows.Count > 0)
                {
                    //var pieceWorkRows = PieceWorkDetailTable.Select($"WorkOrderNo = '{tempRow["WorkOrderNo"]} and ProcessName = '{tempRow["ProcessName"]}'", "ProcessName").First();
                    var pieceWorkRows = (from DataRow p in PieceWorkDetailTable.Rows
                                         where
                                                !p.IsNull("WorkOrderNo") && p.Field<string>("WorkOrderNo") == tempRow["WorkOrderNo"].ToString()
                                                //&& !p.IsNull("ProcessName") && p.Field<string>("ProcessName") == tempRow["ProcessName"].ToString()
                                                && !p.IsNull("OrderStatus") && p.Field<string>("OrderStatus") == tempRow["IsClosed"].ToString()
                                         select p).ToArray();
                    foreach (var drPieceWork in pieceWorkRows)
                    {
                        DataTable dtProcess = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_Process", $"Select RecId from Erp_Process where ProcessName = '{drPieceWork["ProcessName"].ToString().Trim()}'");
                        DataRow newPWork = actualCostBO.Data.Tables["Erp_ActualCostProcessDetail"].NewRow();
                        actualCostBO.Data.Tables["Erp_ActualCostProcessDetail"].Rows.Add(newPWork);
                        newPWork["ActualCostId"] = actualCostBO.CurrentRow["RecId"];
                        if (dtProcess != null && dtProcess.Rows.Count > 0)
                            newPWork["ProcessId"] = dtProcess.Rows[0]["RecId"];
                        newPWork["TotalAmount"] = drPieceWork["TotalAmount"];
                        newPWork["TotalForexAmountEUR"] = drPieceWork["TotalForexAmountEUR"];
                        newPWork["TotalForexAmountUSD"] = drPieceWork["TotalForexAmountUSD"];
                        newPWork["MonthlyAmount"] = drPieceWork["MonthlyAmount"];
                        newPWork["MonthlyForexAmountEUR"] = drPieceWork["MonthlyForexAmountEUR"];
                        newPWork["MonthlyForexAmountUSD"] = drPieceWork["MonthlyForexAmountUSD"];
                    }
                }
                if (actualCostBO != null)
                {
                    PostResult result = actualCostBO.PostData();
                    if (result != PostResult.Succeed)
                        sysMng.ActWndMng.ShowMsg(actualCostBO.ErrorMessage, ConstantStr.InfoPostError, Common.InformationMessages.MessageBoxButton.OK, Common.InformationMessages.MessageBoxImage.Error);
                }
            }
            SysMng.ActWndMng.ShowMsg(SLanguage.GetString("Kaydedildi."), ConstantStr.InfoPostSucceed);
        }
        private DataTable CreateTempCostTable()
        {
            DataTable tempTable = new DataTable();

            tempTable.Columns.Add(new DataColumn { ColumnName = "WorkOrderId", DataType = UdtTypes.GetUdtSystemType("UdtInt64") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "WorkOrderItemId", DataType = UdtTypes.GetUdtSystemType("UdtInt64") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "CurrentAccountId", DataType = UdtTypes.GetUdtSystemType("UdtInt64") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "WorkOrderNo", DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "StyleCode", DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "IsClosed", DataType = UdtTypes.GetUdtSystemType("UdtName") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "Year", DataType = UdtTypes.GetUdtSystemType("UdtName") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "Month", DataType = UdtTypes.GetUdtSystemType("UdtName") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "CurrentAccountCode", DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "CurrentAccountName", DataType = UdtTypes.GetUdtSystemType("UdtName") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ForexCode", DataType = UdtTypes.GetUdtSystemType("UdtCode") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ShipmentDate", DataType = UdtTypes.GetUdtSystemType("UdtDateTime") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "DeliveryDate", DataType = UdtTypes.GetUdtSystemType("UdtDateTime") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ForexUnitPrice", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ForexCostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });

            tempTable.Columns.Add(new DataColumn { ColumnName = "YarnCostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "YarnDyeCostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricCostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricDyeCostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricPrintCostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricOther1CostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricOther2CostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "TrimCostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ReturnCostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "CutCostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "PieceWorkCostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ChemicalCostAmount", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });

            tempTable.Columns.Add(new DataColumn { ColumnName = "YarnCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "YarnDyeCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricDyeCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricPrintCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricOther1CostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricOther2CostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "TrimCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ReturnCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "CutCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "PieceWorkCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ChemicalCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });

            tempTable.Columns.Add(new DataColumn { ColumnName = "Quantity", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "CuttingQuantity", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ShipmentQuantity", DataType = UdtTypes.GetUdtSystemType("UdtQuantity") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "RemainingQuantity", DataType = UdtTypes.GetUdtSystemType("UdtQuantity") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyCost", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyForexCostEUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyForexCostUSD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyUnitPriceCost", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyUnitPriceForexCostEUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyUnitPriceForexCostUSD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlySalesCost", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlySalesForexCostEUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlySalesForexCostUSD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "TotalSalesCost", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "TotalSalesForexCostEUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "TotalSalesForexCostUSD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyShipmentQuantity", DataType = UdtTypes.GetUdtSystemType("UdtQuantity") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "TotalCost", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "TotalForexCostEUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "TotalForexCostUSD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyProfit", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyForexProfitEUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyForexProfitUSD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });

            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyYarnCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyYarnCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyYarnCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "YarnCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "YarnCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyYarnDyeCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyYarnDyeCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyYarnDyeCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "YarnDyeCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "YarnDyeCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricDyeCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricDyeCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricDyeCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricDyeCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricDyeCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricPrintCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricPrintCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricPrintCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricPrintCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricPrintCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricOther1CostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricOther1CostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricOther1CostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricOther1CostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricOther1CostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricOther2CostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricOther2CostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyFabricOther2CostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricOther2CostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "FabricOther2CostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyTrimCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyTrimCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyTrimCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "TrimCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "TrimCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyReturnCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyReturnCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyReturnCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ReturnCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ReturnCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyCutCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyCutCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyCutCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "CutCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "CutCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyPieceWorkCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyPieceWorkCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyPieceWorkCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "PieceWorkCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "PieceWorkCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyChemicalCostAmount2", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyChemicalCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "MonthlyChemicalCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ChemicalCostAmount2EUR", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });
            tempTable.Columns.Add(new DataColumn { ColumnName = "ChemicalCostAmount2USD", DataType = UdtTypes.GetUdtSystemType("UdtAmount") });

            return tempTable;
        }

        /// <summary>
        /// Hesaplama için seçilmiş ay içerinde hareket görmüş Order listesi
        /// </summary>
        private DataTable GetWorkOrderList()
        {
            StringBuilder sb = new StringBuilder();
            //Fatura-İrsaliye Hareketi
            sb.AppendLine(" Select distinct a.* from (");
            sb.AppendLine(" select distinct wo.RecId WorkOrderId, woi.RecId WorkOrderItemId,isnull(wo.WorkOrderNo, '') WorkOrderNo, isnull(wo.CurrentAccountId, '') CurrentAccountId, isnull((Select InventoryCode from Erp_Inventory with (nolock) where RecId = woi.InventoryId), '') StyleCode, convert(nvarchar(10),wo.ShipmentDate,104) ShipmentDate");
            sb.AppendLine(" from Erp_WorkOrder wo with (nolock) left join Erp_WorkOrderItem woi with (nolock) on woi.WorkOrderId = wo.RecId");
            sb.AppendLine(" left join [Erp_InventoryReceiptItem] iri with (nolock)  on (woi.RecId = iri.WorkOrderReceiptItemId) left join [Erp_InventoryReceipt] ir with (nolock) on ir.RecId = iri.InventoryReceiptId");
            sb.AppendLine(" left join [Erp_Invoice] inv with (nolock)  on (ir.[InvoiceId] = inv.[RecId]) left join [Erp_PackingListItem] [erp_packinglistitem] with (nolock)  on (iri.[WorkOrderReceiptItemId] = [erp_packinglistitem].[WorkOrderItemId])");
            sb.AppendLine($"where inv.CompanyId = {activeSession.ActiveCompany.RecId} and isnull(iri.QualityTypeId,0) <> 2 and isnull(wo.UD_PMaliyet,0) = 0 and inv.ReceiptType in (120,121) and Month(inv.DischargeDate) = {CostDate.Month} )a");

            DataTable dtorder = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_WorkOrder", sb.ToString());
            return dtorder;
        }
        private DataTable GetCostDifference()
        {
            /*Burası Maliyet Farkını yakalayacak bir sorgu olmalı
            Koşullar
            1. Kapalı Tüm Orderlar
            2. Çalıştığım ayda Satış faturası da hiç olmayacak
            2. Anlık Toplam maliyet servisten gelecek ve bunlarla Erp_ActualCostda yer alan bulunduğum aydan önceki dönem maliyetlerinin Toplamı elimizde olacak
            3. ATM <> DM ise (+-3 TL kadar) farklı ise bu bir maliyet farkı orderıdır.      
             */
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("select wo.RecId WorkOrderId,isnull(wo.WorkOrderNo, '') WorkOrderNo,isnull(wo.CurrentAccountId, '') CurrentAccountId, ");
            sb.AppendLine("isnull((Select InventoryCode from Erp_Inventory with (nolock) where RecId in (Select InventoryId from Erp_WorkOrderItem woi where woi.WorkOrderId = wo.RecId)), '') StyleCode, convert(nvarchar(10),wo.ShipmentDate,104) ShipmentDate");
            sb.AppendLine("from Erp_WorkOrder wo with(nolock) where wo.ShipmentDate is not null and isnull(wo.UD_PMaliyet,0) = 0 and");
            sb.AppendLine("(select COUNT(RecId) from Erp_InventoryReceiptItem iri where iri.WorkOrderReceiptItemId in (Select woi.RecId from Erp_WorkOrderItem woi where woi.WorkOrderId=wo.RecId) and ");
            sb.AppendLine($"iri.InventoryReceiptId in (Select RecId from Erp_InventoryReceipt ir where ir.InvoiceId in (Select inv.RecId from Erp_Invoice inv where Month(inv.DischargeDate) = {CostDate.Month} and inv.ReceiptType in (120,121)))) <= 0");

            DataTable dtCancelWithoutInvOrder = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_WorkOrder", sb.ToString());
            return dtCancelWithoutInvOrder;
        }
        private DataTable GetMUHand2KOrder()
        {
            RateId = SysMng.Instance.getSession().ParamService.GetParameterClass<GeneralParameters>().FGeneral;
            if (RateId == 0) RateId = 1;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($" Select Sum(a.Quantity) SumQuantity,Sum(a.ItemTotal) SumItemTotal,Sum(a.ItemTotalForexEUR) ItemTotalForexEUR,Sum(a.ItemTotalForexUSD) ItemTotalForexUSD,Sum(a.ItemTotalForex) SumItemTotalForex,a.ForexId,a.ForexCode, a.WorkOrderId , a.WorkOrderItemId, a.WorkOrderNo,a.CurrentAccountId ,a.CurrentAccountCode,a.CurrentAccountName,a.StyleCode,a.ReceiptDate from (");
            sb.AppendLine($"select inv.ReceiptType , iri.Quantity,iri.NetUnitPrice ,iri.ForexRate ,iri.NetUnitPriceForex ,iri.ItemTotal,iri.ItemTotalForex");
            sb.AppendLine($",isnull((iri.NetQuantity * iri.NetUnitPrice),0) as Amount");
            sb.AppendLine($"  ,isnull((iri.NetQuantity * iri.NetUnitPrice),0) /  case when iri.ForexId is null  then dbo.fnGetForexRate(inv.ReceiptDate,(Select RecId from Meta_Forex where ForexCode = 'EURO'),{RateId}) ");
            sb.AppendLine($"  when (iri.ForexId is not null) and iri.ForexId !=  (Select RecId from Meta_Forex where ForexCode = 'EURO')  then dbo.fnGetForexRate(inv.ReceiptDate,(Select RecId from Meta_Forex where ForexCode = 'EURO'),{RateId}) else iri.ForexRate end ItemTotalForexEUR");
            sb.AppendLine($"  ,isnull((iri.NetQuantity * iri.NetUnitPrice),0) /  case when iri.ForexId is null  then dbo.fnGetForexRate(inv.ReceiptDate,(Select RecId from Meta_Forex where ForexCode = '$'),{RateId}) ");
            sb.AppendLine($"  when (iri.ForexId is not null) and iri.ForexId !=  (Select RecId from Meta_Forex where ForexCode = '$')  then dbo.fnGetForexRate(inv.ReceiptDate,(Select RecId from Meta_Forex where ForexCode = '$'),{RateId}) else iri.ForexRate end ItemTotalForexUSD");
            sb.AppendLine($",wo.ForexId,(Select ForexCode from Meta_Forex where RecId in (wo.ForexId)) ForexCode");
            sb.AppendLine($",wo.RecId WorkOrderId, woi.RecId WorkOrderItemId,isnull(wo.WorkOrderNo, '') WorkOrderNo, isnull(wo.CurrentAccountId, '') CurrentAccountId");
            sb.AppendLine($", isnull((Select CurrentAccountCode from Erp_CurrentAccount where RecId in (wo.CurrentAccountId)), '') CurrentAccountCode ");
            sb.AppendLine($", isnull((Select CurrentAccountName from Erp_CurrentAccount where RecId in (wo.CurrentAccountId)), '') CurrentAccountName");
            sb.AppendLine($", isnull((Select InventoryCode from Erp_Inventory with (nolock) where RecId = woi.InventoryId), '') StyleCode, convert(nvarchar(10),wo.ShipmentDate,104) ReceiptDate");
            sb.AppendLine($" from [Erp_Invoice] inv with (nolock)  ");
            sb.AppendLine($" left join [Erp_InventoryReceipt] ir with (nolock) on ir.InvoiceId = inv.RecId");
            sb.AppendLine($" left join [Erp_InventoryReceiptItem] iri with (nolock)  on (iri.InventoryReceiptId = ir.RecId) ");
            sb.AppendLine($" left join Erp_Inventory einv with (nolock) on einv.RecId = iri.InventoryId  ");
            sb.AppendLine($" left join Erp_WorkOrderItem woi with (nolock) on woi.RecId = iri.WorkOrderReceiptItemId");
            sb.AppendLine($" left join Erp_WorkOrder wo with (nolock)  on (wo.RecId = woi.WorkOrderId)");
            sb.AppendLine($"where inv.CompanyId = {activeSession.ActiveCompany.RecId} and isnull(wo.UD_PMaliyet,0) = 0 and iri.QualityTypeId = 2 and inv.ReceiptType in (120,121) and Month(inv.DischargeDate) = {CostDate.Month} and UPPER(einv.InventoryCode) not like 'MUH%' )a");
            sb.AppendLine($"group by a.WorkOrderId,a.ForexId,a.ForexCode, a.WorkOrderItemId, a.WorkOrderNo,a.CurrentAccountId ,a.CurrentAccountCode,a.CurrentAccountName,a.StyleCode,a.ReceiptDate ");
            sb.AppendLine($" union all Select Sum(a.Quantity) SumQuantity,Sum(a.ItemTotal) SumItemTotal,Sum(a.ItemTotalForexEUR) ItemTotalForexEUR,Sum(a.ItemTotalForexUSD) ItemTotalForexUSD,Sum(a.ItemTotalForex) SumItemTotalForex,a.ForexId,a.ForexCode, a.WorkOrderId , a.WorkOrderItemId, a.WorkOrderNo,a.CurrentAccountId ,a.CurrentAccountCode,a.CurrentAccountName,a.StyleCode,a.ReceiptDate from (select");
            sb.AppendLine($"inv.ReceiptType,iri.Quantity,iri.NetUnitPrice ,iri.ForexRate ,iri.NetUnitPriceForex,iri.ItemTotal,iri.ItemTotalForex");
            sb.AppendLine($",isnull((iri.NetQuantity * iri.NetUnitPrice),0) as Amount");
            sb.AppendLine($"  ,isnull((iri.NetQuantity * iri.NetUnitPrice),0) /  case when iri.ForexId is null  then dbo.fnGetForexRate(inv.ReceiptDate,(Select RecId from Meta_Forex where ForexCode = 'EURO'),{RateId}) ");
            sb.AppendLine($"  when (iri.ForexId is not null) and iri.ForexId !=  (Select RecId from Meta_Forex where ForexCode = 'EURO')  then dbo.fnGetForexRate(inv.ReceiptDate,(Select RecId from Meta_Forex where ForexCode = 'EURO'),{RateId}) else iri.ForexRate end ItemTotalForexEUR");
            sb.AppendLine($"  ,isnull((iri.NetQuantity * iri.NetUnitPrice),0) /  case when iri.ForexId is null  then dbo.fnGetForexRate(inv.ReceiptDate,(Select RecId from Meta_Forex where ForexCode = '$'),{RateId}) ");
            sb.AppendLine($"  when (iri.ForexId is not null) and iri.ForexId !=  (Select RecId from Meta_Forex where ForexCode = '$')  then dbo.fnGetForexRate(inv.ReceiptDate,(Select RecId from Meta_Forex where ForexCode = '$'),{RateId}) else iri.ForexRate end ItemTotalForexUSD");
            sb.AppendLine($" ,iri.ForexId,(Select ForexCode from Meta_Forex where RecId in (iri.ForexId)) ForexCode");
            sb.AppendLine($",0 WorkOrderId, 0 WorkOrderItemId,'Muhtelif Satışlar' WorkOrderNo, isnull(inv.CurrentAccountId, '') CurrentAccountId");
            sb.AppendLine($",isnull((Select CurrentAccountCode from Erp_CurrentAccount where RecId in (inv.CurrentAccountId)), '') CurrentAccountCode");
            sb.AppendLine($", isnull((Select CurrentAccountName from Erp_CurrentAccount where RecId in (inv.CurrentAccountId)), '') CurrentAccountName");
            sb.AppendLine($",isnull(einv.InventoryCode, '') StyleCode, convert(nvarchar(10),inv.ReceiptDate,104) ReceiptDate");
            sb.AppendLine($"from [Erp_Invoice] inv with (nolock) left join [Erp_InventoryReceipt] ir with (nolock) on (ir.[InvoiceId] = inv.[RecId]) ");
            sb.AppendLine($"left join [Erp_InventoryReceiptItem] iri with (nolock)  on (ir.RecId = iri.InventoryReceiptId) left join Erp_Inventory einv with (nolock) on einv.RecId = iri.InventoryId ");
            sb.AppendLine($"where inv.CompanyId = {activeSession.ActiveCompany.RecId} and inv.ReceiptType in (120,121) and Month(inv.DischargeDate) = {CostDate.Month} and UPPER(einv.InventoryCode) like 'MUH%' )a");
            sb.AppendLine($"group by a.WorkOrderId,a.ForexId,a.ForexCode, a.WorkOrderItemId, a.WorkOrderNo,a.CurrentAccountId ,a.CurrentAccountCode,a.CurrentAccountName,a.StyleCode,a.ReceiptDate");

            DataTable dtOrder = UtilityFunctions.GetDataTableList(SysMng.getSession().dbInfo.DBProvider, SysMng.getSession().dbInfo.Connection, null, "Erp_WorkOrder", sb.ToString());
            return dtOrder;
        }
        private void InitializeActualCostGrid()
        {
            if (grdActualCost == null) return;
            ActualCostColumnCollection = new ReceiptColumnCollection
            {
                new ReceiptColumn { ColumnName = "CurrentAccountCode", IsFixedColumn = true, Caption = SLanguage.GetString("Müşteri Kodu"), Width = 100, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn { ColumnName = "CurrentAccountName", IsFixedColumn = true, Caption = SLanguage.GetString("Müşteri Adı"), Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtCode") },
                new ReceiptColumn { ColumnName = "WorkOrderNo", IsFixedColumn = true, Caption = SLanguage.GetString("Order No"), Width = 100, DataType = UdtTypes.GetUdtSystemType("UdtCode") },
                new ReceiptColumn { ColumnName = "StyleCode", IsFixedColumn = true, Caption = SLanguage.GetString("Model No"), Width = 100, DataType = UdtTypes.GetUdtSystemType("UdtCode") },
                new ReceiptColumn { ColumnName = "IsClosed", IsFixedColumn = true, Caption = SLanguage.GetString("Order Durumu"), Width = 50, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn { ColumnName = "ShipmentDate", IsFixedColumn = true, Caption = SLanguage.GetString("Yükleme Tarihi"), Width = 80, EditorType = EditorType.DateEditor, UsageType = FieldUsage.Date, DataType = UdtTypes.GetUdtSystemType("UdtDate") },
                new ReceiptColumn { ColumnName = "Year", IsFixedColumn = true, Caption = SLanguage.GetString("Maliyet Yılı"), Width = 80, DataType = UdtTypes.GetUdtSystemType("UdtCode") },
                new ReceiptColumn { ColumnName = "Month", IsFixedColumn = true, Caption = SLanguage.GetString("Maliyet Ayı"), Width = 80, DataType = UdtTypes.GetUdtSystemType("UdtCode") },
                new ReceiptColumn { ColumnName = "ForexCode", IsFixedColumn = true, Caption = SLanguage.GetString("Döviz Kodu"), Width = 50, DataType = UdtTypes.GetUdtSystemType("UdtCode") },
                new ReceiptColumn { ColumnName = "Quantity", IsFixedColumn = true, Caption = SLanguage.GetString("Order Adet"), Width = 80, IsCalculateTotalColumn = true, DataType = UdtTypes.GetUdtSystemType("UdtQuantity") },
                new ReceiptColumn { ColumnName = "CuttingQuantity", IsFixedColumn = true, Caption = SLanguage.GetString("Kesim Adet"), Width = 80, IsCalculateTotalColumn = true, DataType = UdtTypes.GetUdtSystemType("UdtQuantity") },
                new ReceiptColumn { ColumnName = "RemainingQuantity", IsFixedColumn = true, Caption = SLanguage.GetString("Kalan Adet"), Width = 80, IsCalculateTotalColumn = true, DataType = UdtTypes.GetUdtSystemType("UdtQuantity") },

                new ReceiptColumn { ColumnName = "YarnCostAmount", Caption = SLanguage.GetString("İplik Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "YarnDyeCostAmount", Caption = SLanguage.GetString("İplik Boya Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricCostAmount", Caption = SLanguage.GetString("Kumaş Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricDyeCostAmount", Caption = SLanguage.GetString("Kumaş Boya Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricPrintCostAmount", Caption = SLanguage.GetString("Kumaş Baskı Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricOther1CostAmount", Caption = SLanguage.GetString("Diğer Fason İşçilik-1"), Width = 110, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricOther2CostAmount", Caption = SLanguage.GetString("Diğer Fason İşçilik-2"), Width = 110, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TrimCostAmount", Caption = SLanguage.GetString("Aksesuar Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "ReturnCostAmount", Caption = SLanguage.GetString("Artık Hammadde Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "CutCostAmount", Caption = SLanguage.GetString("Kesim Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "PieceWorkCostAmount", Caption = SLanguage.GetString("Parça İşçilik Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "ChemicalCostAmount", Caption = SLanguage.GetString("Kimyasal Maliyet"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },

                new ReceiptColumn { ColumnName = "MonthlyYarnCostAmount2", Caption = SLanguage.GetString("Dönem PRM İplik Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyYarnCostAmount2EUR", Caption = SLanguage.GetString("Dönem EURO PRM İplik Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyYarnCostAmount2USD", Caption = SLanguage.GetString("Dönem USD PRM İplik Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "YarnCostAmount2", Caption = SLanguage.GetString("Toplam PRM İplik Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "YarnCostAmount2EUR", Caption = SLanguage.GetString("Toplam EURO PRM İplik Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "YarnCostAmount2USD", Caption = SLanguage.GetString("Toplam USD PRM İplik Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyYarnDyeCostAmount2", Caption = SLanguage.GetString("Dönem PRM İplik Boya Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyYarnDyeCostAmount2EUR", Caption = SLanguage.GetString("Dönem EURO PRM İplik Boya Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyYarnDyeCostAmount2USD", Caption = SLanguage.GetString("Dönem USD PRM İplik Boya Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "YarnDyeCostAmount2", Caption = SLanguage.GetString("Toplam PRM İplik Boya Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "YarnDyeCostAmount2EUR", Caption = SLanguage.GetString("Toplam EURO PRM İplik Boya Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "YarnDyeCostAmount2USD", Caption = SLanguage.GetString("Toplam USD PRM İplik Boya Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricCostAmount2", Caption = SLanguage.GetString("Dönem PRM Kumaş Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricCostAmount2EUR", Caption = SLanguage.GetString("Dönem EURO PRM Kumaş Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricCostAmount2USD", Caption = SLanguage.GetString("Dönem USD PRM Kumaş Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricCostAmount2", Caption = SLanguage.GetString("Toplam PRM Kumaş Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricCostAmount2EUR", Caption = SLanguage.GetString("Toplam EURO PRM Kumaş Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricCostAmount2USD", Caption = SLanguage.GetString("Toplam USD PRM Kumaş Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricDyeCostAmount2", Caption = SLanguage.GetString("Dönem PRM Kumaş Boya Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricDyeCostAmount2EUR", Caption = SLanguage.GetString("Dönem EURO PRM Kumaş Boya Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricDyeCostAmount2USD", Caption = SLanguage.GetString("Dönem USD PRM Kumaş Boya Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricDyeCostAmount2", Caption = SLanguage.GetString("Toplam PRM Kumaş Boya Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricDyeCostAmount2EUR", Caption = SLanguage.GetString("Toplam EURO PRM Kumaş Boya Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricDyeCostAmount2USD", Caption = SLanguage.GetString("Toplam USD PRM Kumaş Boya Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricPrintCostAmount2", Caption = SLanguage.GetString("Dönem PRM Kumaş Baskı Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricPrintCostAmount2EUR", Caption = SLanguage.GetString("Dönem EURO PRM Kumaş Baskı Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricPrintCostAmount2USD", Caption = SLanguage.GetString("Dönem USD PRM Kumaş Baskı Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricPrintCostAmount2", Caption = SLanguage.GetString("Toplam PRM Kumaş Baskı Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricPrintCostAmount2EUR", Caption = SLanguage.GetString("Toplam EURO PRM Kumaş Baskı Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricPrintCostAmount2USD", Caption = SLanguage.GetString("Toplam USD PRM Kumaş Baskı Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricOther1CostAmount2", Caption = SLanguage.GetString("Dönem PRM Diğer Fason İşçilik-1"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricOther1CostAmount2EUR", Caption = SLanguage.GetString("Dönem EURO PRM Diğer Fason İşçilik-1"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricOther1CostAmount2USD", Caption = SLanguage.GetString("Dönem USD PRM Diğer Fason İşçilik-1"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricOther1CostAmount2", Caption = SLanguage.GetString("Toplam PRM Diğer Fason İşçilik-1"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricOther1CostAmount2EUR", Caption = SLanguage.GetString("Toplam EURO PRM Diğer Fason İşçilik-1"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricOther1CostAmount2USD", Caption = SLanguage.GetString("Toplam USD PRM Diğer Fason İşçilik-1"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricOther2CostAmount2", Caption = SLanguage.GetString("Dönem PRM Diğer Fason İşçilik-2"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricOther2CostAmount2EUR", Caption = SLanguage.GetString("Dönem EURO PRM Diğer Fason İşçilik-2"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyFabricOther2CostAmount2USD", Caption = SLanguage.GetString("Dönem USD PRM Diğer Fason İşçilik-2"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricOther2CostAmount2", Caption = SLanguage.GetString("Toplam PRM Diğer Fason İşçilik-2"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricOther2CostAmount2EUR", Caption = SLanguage.GetString("Toplam EURO PRM Diğer Fason İşçilik-2"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "FabricOther2CostAmount2USD", Caption = SLanguage.GetString("Toplam USD PRM Diğer Fason İşçilik-2"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyTrimCostAmount2", Caption = SLanguage.GetString("Dönem PRM Aksesuar Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyTrimCostAmount2EUR", Caption = SLanguage.GetString("Dönem EURO PRM Aksesuar Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyTrimCostAmount2USD", Caption = SLanguage.GetString("Dönem USD PRM Aksesuar Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TrimCostAmount2", Caption = SLanguage.GetString("Toplam PRM Aksesuar Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TrimCostAmount2EUR", Caption = SLanguage.GetString("Toplam EURO PRM Aksesuar Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TrimCostAmount2USD", Caption = SLanguage.GetString("Toplam USD PRM Aksesuar Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyReturnCostAmount2", Caption = SLanguage.GetString("Dönem PRM Artık Hammadde Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyReturnCostAmount2EUR", Caption = SLanguage.GetString("Dönem EURO PRM Artık Hammadde Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyReturnCostAmount2USD", Caption = SLanguage.GetString("Dönem USD PRM Artık Hammadde Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "ReturnCostAmount2", Caption = SLanguage.GetString("Toplam PRM Artık Hammadde Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "ReturnCostAmount2EUR", Caption = SLanguage.GetString("Toplam EURO PRM Artık Hammadde Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "ReturnCostAmount2USD", Caption = SLanguage.GetString("Toplam USD PRM Artık Hammadde Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyCutCostAmount2", Caption = SLanguage.GetString("Dönem PRM Kesim Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyCutCostAmount2EUR", Caption = SLanguage.GetString("Dönem EURO PRM Kesim Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyCutCostAmount2USD", Caption = SLanguage.GetString("Dönem USD PRM Kesim Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "CutCostAmount2", Caption = SLanguage.GetString("Toplam PRM Kesim Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "CutCostAmount2EUR", Caption = SLanguage.GetString("Toplam EURO PRM Kesim Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "CutCostAmount2USD", Caption = SLanguage.GetString("Toplam USD PRM Kesim Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyPieceWorkCostAmount2", Caption = SLanguage.GetString("Dönem PRM Parça İşçilik Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyPieceWorkCostAmount2EUR", Caption = SLanguage.GetString("Dönem EURO PRM Parça İşçilik Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyPieceWorkCostAmount2USD", Caption = SLanguage.GetString("Dönem USD PRM Parça İşçilik Maliyeti"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "PieceWorkCostAmount2", Caption = SLanguage.GetString("Toplam PRM Parça İşçilik Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "PieceWorkCostAmount2EUR", Caption = SLanguage.GetString("Toplam EURO PRM Parça İşçilik Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "PieceWorkCostAmount2USD", Caption = SLanguage.GetString("Toplam USD PRM Parça İşçilik Maliyeti"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyChemicalCostAmount2", Caption = SLanguage.GetString("Dönem PRM Kimyasal Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyChemicalCostAmount2EUR", Caption = SLanguage.GetString("Dönem EURO PRM Kimyasal Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyChemicalCostAmount2USD", Caption = SLanguage.GetString("Dönem USD PRM Kimyasal Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "ChemicalCostAmount2", Caption = SLanguage.GetString("Toplam PRM Kimyasal Maliyet"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "ChemicalCostAmount2EUR", Caption = SLanguage.GetString("Toplam EURO PRM Kimyasal Maliyet"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "ChemicalCostAmount2USD", Caption = SLanguage.GetString("Toplam USD PRM Kimyasal Maliyet"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },

                new ReceiptColumn { ColumnName = "ShipmentQuantity", Caption = SLanguage.GetString("Toplam F.Satış Adeti"), Width = 80, IsCalculateTotalColumn = true, DataType = UdtTypes.GetUdtSystemType("UdtQuantity") },
                new ReceiptColumn { ColumnName = "MonthlyShipmentQuantity", Caption = SLanguage.GetString("Dönem F.Satış Adeti"), Width = 80, IsCalculateTotalColumn = true, DataType = UdtTypes.GetUdtSystemType("UdtQuantity") },
                new ReceiptColumn { ColumnName = "MonthlySalesCost", Caption = SLanguage.GetString("Dönem Satış Tutarı"), Width = 120, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlySalesForexCostEUR", Caption = SLanguage.GetString("Dönem Satış EURO Tutarı"), Width = 120, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlySalesForexCostUSD", Caption = SLanguage.GetString("Dönem Satış USD Tutarı"), Width = 120, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyUnitPriceCost", Caption = SLanguage.GetString("Birim Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.ForexUnitPrice, Width = 90, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyUnitPriceForexCostEUR", Caption = SLanguage.GetString("Birim EURO Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.ForexUnitPrice, Width = 90, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyUnitPriceForexCostUSD", Caption = SLanguage.GetString("Birim USD Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.ForexUnitPrice, Width = 90, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyCost", Caption = SLanguage.GetString("Dönem Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyForexCostEUR", Caption = SLanguage.GetString("Dönem EURO Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyForexCostUSD", Caption = SLanguage.GetString("Dönem USD Maliyet"), IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, Width = 120, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TotalSalesCost", Caption = SLanguage.GetString("Toplam Satış Tutarı"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TotalSalesForexCostEUR", Caption = SLanguage.GetString("Toplam EURO Satış Tutarı"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TotalSalesForexCostUSD", Caption = SLanguage.GetString("Toplam USD Satış Tutarı"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TotalCost", Caption = SLanguage.GetString("Toplam Maliyet"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TotalForexCostEUR", Caption = SLanguage.GetString("Toplam EURO Maliyet"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "TotalForexCostUSD", Caption = SLanguage.GetString("Toplam USD Maliyet"), Width = 90, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyProfit", Caption = SLanguage.GetString("Dönem Kar/Zarar"), Width = 120, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyForexProfitEUR", Caption = SLanguage.GetString("Dönem EURO Kar/Zarar"), Width = 120, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, UdtType = UdtType.UdtAmount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") },
                new ReceiptColumn { ColumnName = "MonthlyForexProfitUSD", Caption = SLanguage.GetString("Dönem USD Kar/Zarar"), Width = 120, IsCalculateTotalColumn = true, EditorType = EditorType.MaskEditor, UsageType = FieldUsage.Amount, UdtType = UdtType.UdtAmount, DataType = UdtTypes.GetUdtSystemType("UdtAmount") }
            };

            grdActualCost.ColumnDefinitions = ActualCostColumnCollection;
        }
        public override void Dispose()
        {
            if (disposed)
                return;
            if (grdActualCost != null)
            {
                grdActualCost.MouseDoubleClick -= GrdActualCost_MouseDoubleClick;
                grdActualCost.CurrentItemChanged -= GrdActualCost_CurrentItemChanged;
            }
            actualCostBO?.Dispose();
            ActualCostCalculate?.Dispose();
            base.Dispose();
        }
    }
}
