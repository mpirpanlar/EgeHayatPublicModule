using LiveCore.Desktop.UI.Controls;
using Microsoft.Practices.Unity;
using Sentez.Common;
using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.PresentationModels;
using Sentez.Common.Report;
using Sentez.Common.SystemServices;
using Sentez.Common.Utilities;
using Sentez.Data.BusinessObjects;
using Sentez.Data.Tools;
using Sentez.Localization;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using NermaMetalManagementModule.Views;

namespace Sentez.NermaMetalManagementModule.PresentationModels
{
    public partial class SalesShipmentComparePM : ReportPM
    {
        #region Properties
        public LookupList Lists { get; set; }
        LiveGridControl gridSales;
        private ReportBase _pPolicy = null;
        string OrderNo;
        string QualityCode , ForexCode ;
        DataTable salesShipmentCompTable;
        public ReportBase PPolicy { get { return _pPolicy; } set { _pPolicy = value; OnPropertyChanged("PPolicy"); } }

        private DateTime compareDate = DateTime.Today;
        public DateTime CompareDate
        {
            get { return compareDate; }
            set { compareDate = value; OnPropertyChanged("CompareDate"); }
        }

        object salesShipmentSelectedItem;
        public object SalesShipmentSelectedItem
        {
            get { return salesShipmentSelectedItem; }
            set
            {
                salesShipmentSelectedItem = value;
                OnPropertyChanged("SalesShipmentSelectedItem");
            }
        }
        #endregion

        public SalesShipmentComparePM(IUnityContainer container_)
            : base(container_)
        {

        }
        public override void LoadCommands()
        {
            base.LoadCommands();
            CmdList.AddCmd(501, "RefreshCommand", SLanguage.GetString("Yenile"), OnRefreshCommand, null);
            CmdList.AddCmd(502, "FaultControlMechanismCommand", SLanguage.GetString("Hata Kontrol Mekanizması"), OnFaultControlMechanismCommand, null);
        }

        private void OnFaultControlMechanismCommand(ISysCommandParam obj)
        {
            try
            {
                sysMng.ShowWaitCursor();
                if (!SysMng.Instance.CheckRights(OperationType.Select, (short)Modules.ExternalModule15, (short)Modules.ExternalModule15, (short)VogueCostModuleSecurityItems.FaultControlMechanism, (short)VogueCostModuleSecuritySubItems.None))
                {
                    SysMng.Instance.ActWndMng.ShowMsg(SLanguage.GetString("Hata kontrol yetkiniz bulunmamaktadır."), ConstantStr.Warning);
                    return;
                }
                #region Proses Tablosuna Hata Grup Kodlarının Eklenmesi 
                DataTable dtTable = UtilityFunctions.GetDataTableList(ActiveSession.dbInfo.DBProvider, ActiveSession.dbInfo.Connection, null, "Erp_Process", string.Format("select RecId from Erp_Process with (nolock) where ProcessCode in ('IPH','KUH','AKH','STH','GMH') and CompanyId={0}", ActiveSession.ActiveCompany.RecId.Value));
                if (dtTable == null || dtTable.Rows.Count < 1)
                {
                    IBusinessObject processBO = _container.Resolve<IBusinessObject>("ProcessBO");
                    processBO.NewRecord();
                    processBO.CurrentRow.Row["CompanyId"] = ActiveSession.ActiveCompany.RecId.Value; processBO.CurrentRow.Row["ProcessCode"] = "IPH"; processBO.CurrentRow.Row["ProcessName"] = SLanguage.GetString("İplik Hatası"); processBO.CurrentRow.Row["UseManufacturing"] = 0;
                    processBO.CurrentRow.Row["UsePlanning"] = 1; processBO.CurrentRow.Row["ActualType"] = 1; processBO.CurrentRow.Row["InUse"] = 1;
                    processBO.ClearAllExtensions();
                    processBO.PostData();
                    processBO.NewRecord();
                    processBO.CurrentRow.Row["CompanyId"] = ActiveSession.ActiveCompany.RecId.Value; processBO.CurrentRow.Row["ProcessCode"] = "KUH"; processBO.CurrentRow.Row["ProcessName"] = SLanguage.GetString("Kumaş Hatası"); processBO.CurrentRow.Row["UseManufacturing"] = 0;
                    processBO.CurrentRow.Row["UsePlanning"] = 1; processBO.CurrentRow.Row["ActualType"] = 1; processBO.CurrentRow.Row["InUse"] = 1;
                    processBO.ClearAllExtensions();
                    processBO.PostData();
                    processBO.NewRecord();
                    processBO.CurrentRow.Row["CompanyId"] = ActiveSession.ActiveCompany.RecId.Value; processBO.CurrentRow.Row["ProcessCode"] = "AKH"; processBO.CurrentRow.Row["ProcessName"] = SLanguage.GetString("Aksesuar Hatası"); processBO.CurrentRow.Row["UseManufacturing"] = 0;
                    processBO.CurrentRow.Row["UsePlanning"] = 1; processBO.CurrentRow.Row["ActualType"] = 1; processBO.CurrentRow.Row["InUse"] = 1;
                    processBO.ClearAllExtensions();
                    processBO.PostData();
                    processBO.NewRecord();
                    processBO.CurrentRow.Row["CompanyId"] = ActiveSession.ActiveCompany.RecId.Value; processBO.CurrentRow.Row["ProcessCode"] = "STH"; processBO.CurrentRow.Row["ProcessName"] = SLanguage.GetString("Satış Hatası"); processBO.CurrentRow.Row["UseManufacturing"] = 0;
                    processBO.CurrentRow.Row["UsePlanning"] = 1; processBO.CurrentRow.Row["ActualType"] = 1; processBO.CurrentRow.Row["InUse"] = 1;
                    processBO.ClearAllExtensions();
                    processBO.PostData();
                    processBO.NewRecord();
                    processBO.CurrentRow.Row["CompanyId"] = ActiveSession.ActiveCompany.RecId.Value; processBO.CurrentRow.Row["ProcessCode"] = "GMH"; processBO.CurrentRow.Row["ProcessName"] = SLanguage.GetString("Genel Mantık Hatası"); processBO.CurrentRow.Row["UseManufacturing"] = 0;
                    processBO.CurrentRow.Row["UsePlanning"] = 1; processBO.CurrentRow.Row["ActualType"] = 1; processBO.CurrentRow.Row["InUse"] = 1;
                    processBO.ClearAllExtensions();
                    processBO.PostData();
                }
                #endregion

                CopyDataOrders();
                FaultControlMechanism faultControlMechView = new FaultControlMechanism
                {
                    SalesShipmentCompTable = salesShipmentCompTable,
                    CompareDateV = CompareDate
                };
                SysMng.Instance.ActWndMng.ShowWnd(faultControlMechView, true, SLanguage.GetString("Hata Kontrol Mekanizması"), Common.InformationMessages.WindowStyle.SingleBorderWindow, 1000, 650, Common.InformationMessages.ResizeMode.CanResize, 9999, 9999, false, SizeToContent.Manual);
            }
            finally
            {
                sysMng.ShowArrowCursor();
            }
        }

        private void OnRefreshCommand(ISysCommandParam obj)
        {
            InitPolicy();
            OnRun(null);
        }

        public override void Init()
        {
            base.Init();
            InitPolicy();
            gridSales = FCtrl<LiveGridControl>("dbGrid");
            if (gridSales != null)
                gridSales.MouseDoubleClick += SalesShipmentGrid_MouseDoubleClick;
        }

        private void SalesShipmentGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (gridSales?.View != null && SalesShipmentSelectedItem != null && SalesShipmentSelectedItem is DataRowView)
            {
                OrderNo = (SalesShipmentSelectedItem as DataRowView).Row[SLanguage.GetString("Order Numarası")].ToString();
                QualityCode = (SalesShipmentSelectedItem as DataRowView).Row[SLanguage.GetString("Kalite Kodu")].ToString();
                ForexCode = (SalesShipmentSelectedItem as DataRowView).Row[SLanguage.GetString("Fatura Döviz Cinsi")].ToString();
                PmParam pmparam = new PmParam("SalesShipmentDetailsPM", "BOCardContext");
                pmparam.Tag = OrderNo;
                pmparam.Tag2 = CompareDate;
                pmparam.Tag3 = QualityCode;
                pmparam.TagStr = ForexCode;
                BoParam boparam2 = new BoParam();
                SysCommandParam prm = new SysCommandParam("SalesShipmentDetails", "SalesShipmentDetailsPM", pmparam, "", boparam2, SLanguage.GetString("Satış-Sevkiyat Detayları"), "") { isModal = true };
                SysMng.Instance.GetCmd("CmdGeneralOpen").Execute(prm);
            }
        }
        private void CopyDataOrders()
        {
            if (ActivePolicy.Data?.Tables["Erp_Invoice"] != null && ActivePolicy.Data?.Tables["Erp_Invoice"].Rows.Count > 0)
            {
                salesShipmentCompTable = ActivePolicy.Data?.Tables["Erp_Invoice"].Copy();
            }
        }
        private void InitPolicy()
        {
            _pPolicy = _container.Resolve<IReport>("SalesShipmentComparePolicy") as ReportBase;
            if (_pPolicy != null && _pPolicy.PolicyParam == null)
                _pPolicy.PolicyParam = new PolicyParams();
            if (_pPolicy == null) return; 
            _pPolicy.Parameteres.Add("CompareDate", CompareDate);
            AddPolicy(_pPolicy);
            _pPolicy.startWhenWorklistLoaded = true;
            _pPolicy.Init();
            base.Init();
            if (ActivePolicy.statementList.Count > 0 && ActivePolicy?.statementList != null)
            {
                if (_pPolicy != ActivePolicy)
                {
                    ActivePolicy.statementList.Clear();
                    foreach (var statement in _pPolicy.statementList)
                    {
                        ActivePolicy.statementList.Add(statement);
                    }
                }                
            }
        }
        public override void Dispose()
        {
            if (disposed)
                return;

            if (Lists != null) Lists.Dispose();
            Lists = null;
            if (gridSales != null)
                gridSales.MouseDoubleClick -= SalesShipmentGrid_MouseDoubleClick;
            base.Dispose();
        }
    }
}
