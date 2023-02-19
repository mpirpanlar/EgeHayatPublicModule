using Sentez.Common.Commands;
using Sentez.Data.BusinessObjects;
using Sentez.Data.Tools;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NermaMetalManagementModule.Views
{
    public partial class FaultExplanationEntry : UserControl
    {
        IBusinessObject customerTransactionBO = null;
        string txtExplanation; 
        object faultSelectedItemV;
        public object FaultSelectedItemV
        {
            get { return faultSelectedItemV; }
            set
            {
                faultSelectedItemV = value;
            }
        }
        
        public FaultExplanationEntry()
        {
            InitializeComponent();
            customerTransactionBO = SysMng.Instance._container.Resolve<IBusinessObject>("CustomerTransactionBO");
            //planningBO.Init(new BoParam(15));
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            txtExplanation = txtFEText.Text;

            DataTable dtEc = UtilityFunctions.GetDataTableList(SysMng.Instance.getSession().dbInfo.DBProvider, SysMng.Instance.getSession().dbInfo.Connection, null, "Erp_CustomerTransaction", $"Select RecId from Erp_CustomerTransaction with(nolock) where SourceId = {(FaultSelectedItemV as DataRowView).Row["SourceId"]}");
            if (dtEc != null && dtEc.Rows.Count > 0)
            {
                customerTransactionBO.Get(Convert.ToInt64(dtEc.Rows[0]["RecId"]));
                
                if (string.IsNullOrEmpty(customerTransactionBO.CurrentRow["ApprovedExplanation"].ToString()))
                    customerTransactionBO.CurrentRow["ApprovedExplanation"] = txtExplanation;
                else
                {
                    string currentText = customerTransactionBO.CurrentRow["ApprovedExplanation"].ToString();
                    customerTransactionBO.CurrentRow["ApprovedExplanation"] = currentText + ' ' + txtExplanation;
                }
                if (customerTransactionBO.PostData(customerTransactionBO.Transaction) == PostResult.Succeed) { }
                else if (!string.IsNullOrEmpty(customerTransactionBO.ErrorMessage))
                    customerTransactionBO.ShowMessage(customerTransactionBO.ErrorMessage);
            }
            (Parent as Window).Close();
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtFEText.Text = null;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var window = Parent as Window;
            window?.Close();
        }
    }
}
