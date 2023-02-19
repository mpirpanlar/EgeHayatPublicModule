using Microsoft.Practices.Unity;
using Sentez.Common;
using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.PresentationModels;
using Sentez.Common.Report;
using Sentez.Common.Security;
using Sentez.Common.SystemServices;
using Sentez.Common.Utilities;
using Sentez.Data.BusinessObjects;
using Sentez.Localization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NermaMetalManagementModule.Views;
using Sentez.Common.SqlBuilder;
using LiveCore.Desktop.UI.Controls;
using Sentez.Data.Tools;

namespace Sentez.NermaMetalManagementModule.PresentationModels
{
    public partial class FaultTaskControlPM : ReportPM
    {
        #region Properties
        IBusinessObject customerTransactionBO = null;
        List<long> workOrderIdList;
        string workOrderIds;
        private ISystemService _faultQueryService;
        public LookupList Lists { get; set; }
        private ReportBase _pPolicy = null;
        public ReportBase PPolicy { get { return _pPolicy; } set { _pPolicy = value; OnPropertyChanged("PPolicy"); } }

        object faultSelectedItem;
        public object FaultSelectedItem
        {
            get { return faultSelectedItem; }
            set
            {
                faultSelectedItem = value;
                OnPropertyChanged("FaultSelectedItem");
            }
        }
        private DataTable faultTable;
        public DataTable FaultTable
        {
            get { return faultTable; }
            set { faultTable = value; }
        }
        private DataTable planningTable;
        public DataTable PlanningTable
        {
            get { return planningTable; }
            set { planningTable = value; }
        }
        #endregion
        public FaultTaskControlPM(IUnityContainer container_)
            : base(container_)
        {

        }
        public override void LoadCommands()
        {
            base.LoadCommands();
            CmdList.AddCmd(501, "RefreshCommand", SLanguage.GetString("Yenile"), OnRefreshCommand, null);
            CmdList.AddCmd(502, "IsApprovedOkCommand", SLanguage.GetString("Seçilenleri Onayla"), OnIsApprovedOkCommand, null);
            CmdList.AddCmd(301, "DeleteCommand", SLanguage.GetString("Sil"), OnDeleteCommand, null);
            CmdList.AddCmd(302, "GoRequirementCommand", SLanguage.GetString("İlgili Tedariğe Git"), OnGoRequirementCommand, null);
        }
        public override void Init()
        {
            base.Init();
            InitPolicy();
            _faultQueryService = SysMng.Instance.getSession().Container.Resolve<ISystemService>("FaultQueryService");
            customerTransactionBO = SysMng.Instance._container.Resolve<IBusinessObject>("CustomerTransactionBO");
            if (dbGrid != null)
            {
                dbGrid.MouseDoubleClick += dbGrid_MouseDoubleClick;
                dbGrid.PreviewKeyDown += DbGrid_PreviewKeyDown;
            }
            InsertContextMenu(AddToMenu(new MenuItemPM(SLanguage.GetString("Sil"), "DeleteCommand") { ShortcutKey = Key.F6, ShortcutKeyModifier = ModifierKeys.None }, null));
            InsertContextMenu(AddToMenu(new MenuItemPM(SLanguage.GetString("İlgili Tedariğe Git"), "GoRequirementCommand") { ShortcutKey = Key.F9, ShortcutKeyModifier = ModifierKeys.Control }, null));
        }

        private void DbGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.O)
            {
                DoOkeyApproved();
                ((sender as LiveGridControl).View as ListSelectionView).FocusedRowHandle = 0;
                ((sender as LiveGridControl).View as ListSelectionView).Focus();
                e.Handled = true;
            }
            else if (e.Key == Key.A)
            {
                GoFaultEntry();
            }
            else
                e.Handled = true;
        }

        private void dbGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GoFaultEntry();
        }
        private void GoFaultEntry()
        {
            if (dbGrid?.View != null && FaultSelectedItem != null && FaultSelectedItem is DataRowView)
            {
                FaultExplanationEntry faultExplanationEntryView = new FaultExplanationEntry
                {
                    FaultSelectedItemV = FaultSelectedItem
                };
                SysMng.Instance.ActWndMng.ShowWnd(faultExplanationEntryView, true, SLanguage.GetString("Hata Açıklaması Girişi"), Common.InformationMessages.WindowStyle.SingleBorderWindow, 400, 380, Common.InformationMessages.ResizeMode.CanResize, 9999, 9999, false, SizeToContent.Manual);
                OnRun(null);
            }
        }

        private void OnGoRequirementCommand(ISysCommandParam obj)
        {
            if (dbGrid?.View != null && FaultSelectedItem != null && FaultSelectedItem is DataRowView)
            {
                int orderId = 0;
                int.TryParse((FaultSelectedItem as DataRowView).Row["WorkOrderId"].ToString(), out orderId);
                if ((FaultSelectedItem as DataRowView).Row[SLanguage.GetString("Hata Tipi")].ToString() == "IPH")
                {
                    SysMng.Instance.GetCmd("CmdGeneralOpen").Execute(new SysCommandParam("Requirement", "RequirementPM", new PmParam("RequirementPM", "BOCardContext") { Name = "Yarn", Tag = orderId, Tag2 = orderId, SecId = (short)SecurityHelper.GetSecId("VModule", "VogueSecurityItems", "RequirementYarn"), SubSecId = (short)SecurityHelper.GetSecId("VModule", "RequirementSecuritySubItems", "YarnRequirement") }, "RequirementBO", new BoParam { Tag = "Yarn", Type = 15, ActiveRecordId = orderId }, "", "") { logicalModuleID = (short)Modules.VModule, moduleID = (short)Modules.VModule, secID = (short)SecurityHelper.GetSecId("VModule", "VogueSecurityItems", "RequirementYarn"), subsecID = (short)SecurityHelper.GetSecId("VModule", "RequirementSecuritySubItems", "YarnRequirement") });
                }
                else if ((FaultSelectedItem as DataRowView).Row[SLanguage.GetString("Hata Tipi")].ToString() == "KUH")
                {
                    SysMng.Instance.GetCmd("CmdGeneralOpen").Execute(new SysCommandParam("Requirement", "RequirementPM", new PmParam("RequirementPM", "BOCardContext") { Name = "Fabric", Tag = orderId, Tag2 = orderId, SecId = (short)SecurityHelper.GetSecId("VModule", "VogueSecurityItems", "RequirementFabric"), SubSecId = (short)SecurityHelper.GetSecId("VModule", "RequirementSecuritySubItems", "FabricRequirement") }, "RequirementBO", new BoParam() { Tag = "Fabric", Type = 15, ActiveRecordId = orderId }, "", "") { logicalModuleID = (short)Modules.VModule, moduleID = (short)Modules.VModule, secID = (short)SecurityHelper.GetSecId("VModule", "VogueSecurityItems", "RequirementFabric"), subsecID = (short)SecurityHelper.GetSecId("VModule", "RequirementSecuritySubItems", "FabricRequirement") });
                }
                else if ((FaultSelectedItem as DataRowView).Row[SLanguage.GetString("Hata Tipi")].ToString() == "AKH")
                {
                    SysMng.Instance.GetCmd("CmdGeneralOpen").Execute(new SysCommandParam("Requirement", "RequirementPM", new PmParam("RequirementPM", "BOCardContext") { Name = "Trim", Tag = orderId, Tag2 = orderId, SecId = (short)SecurityHelper.GetSecId("VModule", "VogueSecurityItems", "RequirementTrim"), SubSecId = (short)SecurityHelper.GetSecId("VModule", "RequirementSecuritySubItems", "TrimRequirement") }, "RequirementBO", new BoParam() { Tag = "Trim", Type = 15, ActiveRecordId = orderId }, "", "") { logicalModuleID = (short)Modules.VModule, moduleID = (short)Modules.VModule, secID = (short)SecurityHelper.GetSecId("VModule", "VogueSecurityItems", "RequirementTrim"), subsecID = (short)SecurityHelper.GetSecId("VModule", "RequirementSecuritySubItems", "TrimRequirement") });
                }
                else
                    SysMng.Instance.ActWndMng.ShowMsg(SLanguage.GetString("Seçili hata tedarik hatası değildir."), ConstantStr.Warning);
            }
        }
        private void OnDeleteCommand(ISysCommandParam obj)
        {
            try
            {
                sysMng.ShowWaitCursor();
                if (!SysMng.Instance.CheckRights(OperationType.Delete, (short)Modules.ExternalModule15, (short)Modules.ExternalModule15, (short)VogueCostModuleSecurityItems.FaultTaskControl, (short)VogueCostModuleSecuritySubItems.None))
                {
                    SysMng.Instance.ActWndMng.ShowMsg(SLanguage.GetString("Silme yetkiniz bulunmamaktadır."), ConstantStr.Warning);
                    return;
                }
                if (SysMng.Instance.ActWndMng.ShowMsgYesNo(ConstantStr.ConfirmDelete, ConstantStr.Warning) != Common.InformationMessages.MessageBoxResult.Yes) return;
                if (Selection != null && Selection.Count > 0)
                {
                    foreach (var item in Selection)
                    {
                        DataRow drSelected = (item as DataRowView).Row;
                        DataTable dtEc = UtilityFunctions.GetDataTableList(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, null, "Erp_CustomerTransaction", $"Select RecId from Erp_CustomerTransaction with(nolock) where SourceId = {drSelected["SourceId"]}");
                        if (dtEc != null && dtEc.Rows.Count > 0)
                        {
                            customerTransactionBO.Get(Convert.ToInt64(dtEc.Rows[0]["RecId"]));
                            customerTransactionBO.Delete();
                        }
                    }
                }
                OnRun(null);
            }
            finally
            {
                sysMng.ShowArrowCursor();
            }

        }

        /// <summary>
        /// Sistemdeki varsa düzeltilmiş olan hataların tespitiyle IsCompleted=1 yaparak ekranı yeniliyor.
        /// </summary>
        /// <param name="obj"></param>
        private void OnRefreshCommand(ISysCommandParam obj)
        {
            if (ActivePolicy.Data?.Tables["Erp_CustomerTransaction"] != null && ActivePolicy.Data?.Tables["Erp_CustomerTransaction"].Rows.Count > 0)
            {
                PlanningTable = ActivePolicy.Data?.Tables["Erp_CustomerTransaction"].Copy();
            }
            workOrderIdList = new List<long>();
            int CompareDateMonth = 0; int CompareDateYear = 0;
            if (PlanningTable != null && PlanningTable.Rows.Count > 0)
            {
                foreach (DataRow planningRow in PlanningTable.Rows)
                {
                    if (!string.IsNullOrEmpty(planningRow["WorkOrderId"].ToString()))
                        workOrderIdList.Add(Convert.ToInt64(planningRow["WorkOrderId"]));
                    CompareDateMonth = ((DateTime)planningRow["Başlangıç"]).Month;
                    CompareDateYear = ((DateTime)planningRow["Başlangıç"]).Year;
                }
                workOrderIds = string.Join(",", workOrderIdList);
            }
            FaultTable = (DataTable)_faultQueryService.Execute(workOrderIds, CompareDateMonth, CompareDateYear);
            List<string> faultExplanationsList;
            faultExplanationsList = new List<string>();
            if (FaultTable != null && FaultTable.Rows.Count > 0)
            {
                foreach (DataRow faultRow in FaultTable.Rows)
                {
                    faultExplanationsList.Add(faultRow["Explanation"].ToString());
                }
                if(PlanningTable != null && PlanningTable.Rows.Count>0)
                {
                    foreach (DataRow planningRow in PlanningTable.Rows)
                    {
                        if (!faultExplanationsList.Contains(planningRow["Hata Sebebi"].ToString()))
                        {
                            DataTable dtEc = UtilityFunctions.GetDataTableList(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, null, "Erp_CustomerTransaction", $"Select RecId from Erp_CustomerTransaction with(nolock) where SourceId = {planningRow["SourceId"]}");
                            if (dtEc != null && dtEc.Rows.Count > 0)
                            {
                                customerTransactionBO.Get(Convert.ToInt64(dtEc.Rows[0]["RecId"]));
                                customerTransactionBO.CurrentRow["IsApproved"] = 1;
                                if (customerTransactionBO.PostData(customerTransactionBO.Transaction) == PostResult.Succeed)
                                    continue;
                                else if (!string.IsNullOrEmpty(customerTransactionBO.ErrorMessage))
                                    customerTransactionBO.ShowMessage(customerTransactionBO.ErrorMessage);
                            }
                        }
                    }
                }
            }
            OnRun(null);
        }

        /// <summary>
        /// Seçili olan hatanın onay işlemleri
        /// </summary>
        /// <param name="obj"></param>
        private void OnIsApprovedOkCommand(ISysCommandParam obj)
        {
            if (!SysMng.Instance.CheckRights(OperationType.Update, (short)Modules.ExternalModule15, (short)Modules.ExternalModule15, (short)VogueCostModuleSecurityItems.FaultTaskControl, (short)VogueCostModuleSecuritySubItems.None))
            {
                SysMng.Instance.ActWndMng.ShowMsg(SLanguage.GetString("Onaylama yetkiniz bulunmamaktadır."), ConstantStr.Warning);
                return;
            }
            DoOkeyApproved();
        }

        private void DoOkeyApproved()
        {
            if (Selection != null && Selection.Count > 0)
            {
                foreach (var item in Selection)
                {
                    DataRow drSelected = (item as DataRowView).Row;
                    if (!string.IsNullOrEmpty(drSelected[SLanguage.GetString("Hata Açıklaması")].ToString()))
                    {
                        DataTable dtEc = UtilityFunctions.GetDataTableList(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, null, "Erp_CustomerTransaction", $"Select RecId from Erp_CustomerTransaction with(nolock) where SourceId = {drSelected["SourceId"]}");
                        if (dtEc != null && dtEc.Rows.Count > 0)
                        {
                            customerTransactionBO.Get(Convert.ToInt32(dtEc.Rows[0]["RecId"]));
                            customerTransactionBO.CurrentRow["IsApproved"] = 1;
                            if (customerTransactionBO.PostData(customerTransactionBO.Transaction) == PostResult.Succeed)
                                continue;
                            else if (!string.IsNullOrEmpty(customerTransactionBO.ErrorMessage))
                                customerTransactionBO.ShowMessage(customerTransactionBO.ErrorMessage);
                        }
                    }
                    else
                    {
                        SysMng.ActWndMng.ShowMsg(SLanguage.GetString($"Onaylamak istediğiniz {drSelected[SLanguage.GetString("Order Numarası")]} nolu Order için hata açıklaması bulunmamaktadır!."), ConstantStr.Warning);
                        return;
                    }
                }
                OnRun(null);
            }
            else { SysMng.ActWndMng.ShowMsg(SLanguage.GetString("Onaylama işlemi için seçim yapınız."), ConstantStr.Warning); }
        }

        private void InitPolicy()
        {
            _pPolicy = _container.Resolve<IReport>("FaultTaskControlPolicy") as ReportBase;
            if (_pPolicy != null && _pPolicy.PolicyParam == null)
                _pPolicy.PolicyParam = new PolicyParams();
            if (_pPolicy == null) return;
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
            var sel = (from FilterItem c in ActivePolicy.statementList[0].filterList where (c.filterTable1Alias == "erp_planning") && c.field1Name == "IsCompleted" select c).FirstOrDefault();
            if (sel != null && sel.valueList.Count > 1)
            {
                if (sel.valueList.Any(x => x.ToString() == "1"))
                {
                    sel.valueList.Remove(sel.valueList.FirstOrDefault(x => x.ToString() == "1"));
                }
            }
        }
        public override void Dispose()
        {
            if (disposed)
                return;

            if (Lists != null) Lists.Dispose();
            Lists = null;
            if (dbGrid != null)
            {
                dbGrid.MouseDoubleClick -= dbGrid_MouseDoubleClick;
                dbGrid.PreviewKeyDown -= DbGrid_PreviewKeyDown;
            }
            base.Dispose();
        }
    }
}
