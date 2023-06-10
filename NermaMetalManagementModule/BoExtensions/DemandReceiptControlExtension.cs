using Sentez.Data.BusinessObjects;
using Sentez.Localization;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace NermaMetalManagementModule.BoExtensions
{
    public class DemandReceiptControlExtension : BoExtensionBase
    {
        public DemandReceiptControlExtension(BusinessObjectBase bo)
            : base(bo)
        {
        }

        protected override void SetBusinessObject(BusinessObjectBase businessObject)
        {
            base.SetBusinessObject(businessObject);
            if (BusinessObject == null)
                return;
        }

        protected override void OnColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            base.OnColumnChanged(sender, e);
            if (e.Column.ColumnName == "ItemVariant2Id")
            {
                if (!e.Row.IsNull("MarkId"))
                {

                }
            }
        }

        protected override void OnBeforePost(object sender, CancelEventArgs e)
        {
            if (!Enabled || _suppressEvents)
                return;
            base.OnBeforePost(sender, e);
            if (!e.Cancel)
            {
                foreach (DataRow itemRow in BusinessObject.Data.Tables["Erp_QuotationReceiptItem"].Select("", "", DataViewRowState.CurrentRows))
                {
                    bool attributeItemIsSelect;
                    bool.TryParse(itemRow["AttributeItemIsSelect"].ToString(), out attributeItemIsSelect);
                    if (attributeItemIsSelect && itemRow.IsNull("CategoryAttributeSetDetails_AttributeSetCode"))
                    {
                        BusinessObject.ErrorMessage = SLanguage.GetString("Lütfen özellik set kodu seçimi yapınız");
                        BusinessObject.ErrorMessage += $"\n{SLanguage.GetString("Malzeme Kodu")}: {itemRow["ItemCode"]}\n{SLanguage.GetString("Malzeme Adı")}: {itemRow["ItemName"]}";
                        e.Cancel = true;
                        break;
                    }
                    if (!e.Cancel && !attributeItemIsSelect && !itemRow.IsNull("CategoryAttributeSetDetails_AttributeSetCode"))
                    {
                        BusinessObject.ErrorMessage = SLanguage.GetString("Lütfen özellik set kodu seçimi *** yapmayınız ***");
                        BusinessObject.ErrorMessage += $"\n{SLanguage.GetString("Malzeme Kodu")}: {itemRow["ItemCode"]}\n{SLanguage.GetString("Malzeme Adı")}: {itemRow["ItemName"]}";
                        e.Cancel = true;
                        break;
                    }
                }
            }
        }
    }
}
