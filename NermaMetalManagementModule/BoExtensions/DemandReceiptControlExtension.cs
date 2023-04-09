using Sentez.Data.BusinessObjects;

using System;
using System.Collections.Generic;
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
    }
}
