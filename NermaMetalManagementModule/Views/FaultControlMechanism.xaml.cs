using Sentez.Common.Commands;
using Sentez.Common.SystemServices;
using Sentez.Common.Utilities;
using Sentez.Data.BusinessObjects;
using Sentez.Data.MetaData;
using Sentez.Data.Tools;
using Sentez.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace NermaMetalManagementModule.Views
{
    public partial class FaultControlMechanism : UserControl, INotifyPropertyChanged, IDisposable
    {
        #region Properties
        private ISystemService _faultQueryService;
        List<long> workOrderIdList;
        string workOrderIds;
        DataView faultDataView { get; set; }
        public LookupList Lists { get; set; }
        public class UsersModel
        {
            public long EmployeeId { get; set; }
            public List<string> FaultList { get; set; }
        }
        private DataTable salesShipmentCompTable;
        public DataTable SalesShipmentCompTable
        {
            get { return salesShipmentCompTable; }
            set { salesShipmentCompTable = value; }
        }
        private DataTable faultTable;
        public DataTable FaultTable
        {
            get { return faultTable; }
            set
            {
                faultTable = value;
                OnPropertyChanged("FaultTable");
            }
        }
        public DateTime CompareDateV;
        #endregion
        public FaultControlMechanism()
        {
            InitializeComponent();
            _faultQueryService = SysMng.Instance.getSession().Container.Resolve<ISystemService>("FaultQueryService");
            Loaded += FaultControlMechanism_Loaded;
        }
        private void FaultControlMechanism_Loaded(object sender, RoutedEventArgs e)
        {
            if (salesShipmentCompTable != null && salesShipmentCompTable.Rows.Count > 0)
            {
                workOrderIdList = new List<long>();
                foreach (DataRow salesRow in salesShipmentCompTable.Rows)
                {
                    if (!string.IsNullOrEmpty(salesRow["Order Kayıt No"].ToString()))
                        workOrderIdList.Add(Convert.ToInt64(salesRow["Order Kayıt No"]));
                }
                workOrderIds = string.Join(",", workOrderIdList);
                getFaultData(workOrderIds);
                InitializeFaultControlGrid();
            }
        }

        public void getFaultData(string workOrderIds)
        {
            #region Daha Önce Erp_CustomerTransaction hata kaydı atılmış bir order ise o Order için aynı hatanın yeniden sorgulanmaması için düzenleme
            string sourceIds = null;
            DataTable planningCheck = UtilityFunctions.GetDataTableList(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, null, "PlanningCheck", $"select SourceId from Erp_CustomerTransaction with(nolock)");
            if (planningCheck != null && planningCheck.Rows.Count > 0)
            {
                List<long> faultList = new List<long>();
                foreach (DataRow planningRow in planningCheck.Rows)
                    faultList.Add(Convert.ToInt64(planningRow["SourceId"]));
                sourceIds = string.Join(",", faultList);
            }
            #endregion
            FaultTable = (DataTable)_faultQueryService.Execute(workOrderIds, CompareDateV.Month, CompareDateV.Year);
            if (FaultTable != null && FaultTable.Rows.Count > 0 && sourceIds != null)
            {
                foreach (DataRow drFault in FaultTable.Select($"RecId in ({sourceIds})"))
                {
                    FaultTable.Rows.Remove(drFault);
                }
                FaultTable.AcceptChanges();
            }
            grdFaultControl.ItemsSource = FaultTable;
        }

        public void InitializeFaultControlGrid()
        {
            ReceiptColumnCollection columnsCollection = new ReceiptColumnCollection
            {
                new ReceiptColumn {ColumnName = "FaultType", Caption = SLanguage.GetString("Hata Tipi"), Width = 65, IsReadOnly=true, UsageType = FieldUsage.Integer, DataType = UdtTypes.GetUdtSystemType("UdtInt32") },
                new ReceiptColumn {ColumnName = "CurrentAccountName", Caption = SLanguage.GetString("Müşteri"), Width = 120, IsReadOnly=true, UsageType = FieldUsage.Integer, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn {ColumnName = "WorkOrderNo", Caption = SLanguage.GetString("Order No"), Width = 120, IsReadOnly=true, UsageType = FieldUsage.Integer, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn {ColumnName = "RecId", Caption = SLanguage.GetString("RecId"), Width = 50, IsReadOnly=true, IsVisible = false, UsageType = FieldUsage.Integer, DataType = UdtTypes.GetUdtSystemType("UdtInt32") },
                new ReceiptColumn {ColumnName = "EmployeeId", Caption = SLanguage.GetString("Çalışan Kayıt No"), Width = 50, IsReadOnly=true, UsageType = FieldUsage.Integer, DataType = UdtTypes.GetUdtSystemType("UdtInt32") },
                new ReceiptColumn {ColumnName = "EmployeeName", Caption = SLanguage.GetString("Çalışan Adı"), Width = 50, IsReadOnly=true, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn {ColumnName = "ReceiptTypeName", Caption = SLanguage.GetString("Fiş Tipi"), Width = 75, IsReadOnly=true, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn {ColumnName = "ReceiptUpTypeName", Caption = SLanguage.GetString("Fiş Üst Tipi"), Width = 75, IsReadOnly=true, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn {ColumnName = "ReceiptSubTypeName", Caption = SLanguage.GetString("Fiş Alt Tipi"), Width = 75, IsReadOnly=true, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType("UdtName") },
                new ReceiptColumn {ColumnName = "ReceiptId", Caption = SLanguage.GetString("ReceiptId"), Width = 50, IsReadOnly=true, IsVisible = false, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType("UdtInt32") },
                new ReceiptColumn {ColumnName = "Explanation", Caption = SLanguage.GetString("Açıklama"), Width = 460, IsReadOnly=true, UsageType = FieldUsage.Name, DataType = UdtTypes.GetUdtSystemType("UdtExpHuge") },
                new ReceiptColumn {ColumnName = "SentQuantity", Caption = SLanguage.GetString("Çıkış"), Width = 50, IsReadOnly=true, UsageType = FieldUsage.Quantity, DataType =UdtTypes.GetUdtSystemType("UdtQuantity")},
                new ReceiptColumn {ColumnName = "ReceivedQuantity", Caption = SLanguage.GetString("Giriş"), IsReadOnly=true, Width = 50, UsageType = FieldUsage.Quantity, DataType =UdtTypes.GetUdtSystemType("UdtQuantity")},
            };
            grdFaultControl.ColumnDefinitions = columnsCollection;
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            getFaultData(workOrderIds);
        }

        /// <summary>
        /// Hataların ilgili personellere görev olarak atanma işlemi (Erp_Planninge kaydı)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SysMng.Instance.ShowWaitCursor();
                if (FaultTable != null && FaultTable.Rows.Count > 0)
                {
                    IBusinessObject customerTransactionBO = SysMng.Instance._container.Resolve<IBusinessObject>("CustomerTransactionBO");

                    List<UsersModel> UserList = new List<UsersModel>();

                    DataTable dtMetaUser = UtilityFunctions.GetDataTableList(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, null, "Meta_User", $"Select EmployeeId,UD_HataYetkisi from Meta_User with(nolock) where UD_HataYetkisi is not null");
                    if (dtMetaUser != null && dtMetaUser.Rows.Count > 0)
                    {
                        foreach (DataRow drUser in dtMetaUser.Rows)
                        {
                            long employeeId = 0;
                            long.TryParse(drUser["EmployeeId"].ToString(), out employeeId);
                            if (employeeId > 0)
                            {
                                UsersModel user = new UsersModel();
                                string faults = drUser["UD_HataYetkisi"].ToString();
                                List<string> faultList = faults.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                                user.EmployeeId = employeeId;
                                user.FaultList = faultList;
                                UserList.Add(user);
                            }
                        }
                    }
                    int faultType = 0;
                    foreach (DataRow faultRow in FaultTable.Rows)
                    {
                        int.TryParse(faultRow["FaultType"].ToString(), out faultType);
                        List<long> employeeIdList = GetEmployeeId(UserList, faultType.ToString());
                        if (employeeIdList != null && employeeIdList.Count > 0)
                        {
                            foreach (var empId in employeeIdList)
                            {
                                customerTransactionBO.NewRecord();
                                CodeGenerator ctCodeGenerator = new CodeGenerator(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, SysMng.Instance.getSession().ActiveCompany.RecId, 0, 20, 0, "Erp_CustomerTransaction", "DocumentNo", "", false, false, true, true)
                                {
                                    CodeField = "DocumentNo",
                                    CodeTable = "Erp_CustomerTransaction",
                                    TypeField = "",
                                    TemplateString = "########"
                                };
                                DataTable dtEmployee = UtilityFunctions.GetDataTableList(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, null, "Meta_User", $"Select DepartmentId from Erp_Employee with(nolock) where CompanyId = {SysMng.Instance.getSession().ActiveCompany.RecId} and RecId = {empId}");
                                if (dtEmployee != null && dtEmployee.Rows.Count > 0)
                                    customerTransactionBO.CurrentRow["DepartmentId"] = dtEmployee.Rows[0]["DepartmentId"];
                                customerTransactionBO.CurrentRow["CurrentAccountId"] = faultRow["CurrentAccountId"];
                                customerTransactionBO.CurrentRow["SourceId"] = faultRow["RecId"];
                                customerTransactionBO.CurrentRow["DocumentNo"] = ctCodeGenerator.GenerateCode();
                                customerTransactionBO.CurrentRow["Explanation"] = faultRow["Explanation"];
                                customerTransactionBO.CurrentRow["SourceModule"] = 1102;
                                customerTransactionBO.CurrentRow["SourceDocumentNo"] = faultRow["WorkOrderId"];

                                DataRow newTaskItem = customerTransactionBO.Data.Tables["Erp_CustomerTransactionActivity"].NewRow();

                                newTaskItem["CustomerTransactionId"] = customerTransactionBO.CurrentRow["RecId"];
                                newTaskItem["EmployeeId"] = empId;
                                newTaskItem["StartDate"] = DateTime.Today;
                                newTaskItem["Explanation"] = faultRow["WorkOrderNo"];
                                newTaskItem["Quantity"] = faultRow["ReceivedQuantity"];
                                newTaskItem["UnitPrice"] = faultRow["SentQuantity"];

                                customerTransactionBO.Data.Tables["Erp_CustomerTransactionActivity"].Rows.Add(newTaskItem);

                                if (customerTransactionBO.PostData(customerTransactionBO.Transaction) == PostResult.Succeed)
                                    continue;
                                if (customerTransactionBO.ErrorMessages.Count > 0 || !string.IsNullOrEmpty(customerTransactionBO.ErrorMessage))
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine(!string.IsNullOrEmpty(customerTransactionBO.ErrorMessage) ? $"Veri üzerinde hatalar var.- {customerTransactionBO.ErrorMessage} - Kayıt yapılamaz." : "Veri üzerinde hatalar var.");
                                    foreach (var msg in customerTransactionBO.ErrorMessages)
                                        sb.AppendFormat("Hata Tablo:{0}, Kolon:{1}, Mesaj:{2}", msg.TableName, msg.ColumnName, msg.ErrorMessage).AppendLine();
                                    customerTransactionBO.ShowMessage(sb.ToString());
                                }
                            }
                        }
                    }
                    customerTransactionBO.ShowMessage("İşlem tamamlandı.");
                    getFaultData(workOrderIds);
                }
            }
            finally
            {
                SysMng.Instance.ShowArrowCursor();
            }
        }
        private List<long> GetEmployeeId(List<UsersModel> users, string faultType)
        {
            List<long> empIds = new List<long>();
            foreach (var user in users)
            {
                foreach (var fault in user.FaultList)
                {
                    if (fault == faultType)
                    {
                        empIds.Add(user.EmployeeId);
                        return empIds;
                    }

                }
            }
            return null;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var window = Parent as Window;
            window?.Close();
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
        #region IDisposable Support
        private bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                //Loaded -= FaultControlMechanism_Loaded;

                if (disposing)
                {

                }
                disposed = true;
            }
        }
        ~FaultControlMechanism()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
