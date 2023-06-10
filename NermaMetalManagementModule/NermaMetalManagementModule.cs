using LiveCore.Desktop.SBase.MenuManager;
using Sentez.Common;
using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.Report;
using Sentez.Common.ResourceManager;
using Sentez.Common.SystemServices;
using Sentez.Data.BusinessObjects;
using Sentez.Data.MetaData.DatabaseControl;
//using Sentez.NermaMetalManagementModule.PresentationModels;
using Sentez.NermaMetalManagementModule.Services;
using System;
using System.IO;
using System.Reflection;
using NermaMetalManagementModule.Services;
using Sentez.Common.PresentationModels;
using Sentez.NermaMetalManagementModule.Models;
using Sentez.Common.Utilities;
using Sentez.NermaMetalManagementModule.WorkList;
using Sentez.Data.MetaData;
using Sentez.Data.Tools;
using Sentez.Localization;
using System.Windows;
using LiveCore.Desktop.UI.Controls;
using System.Windows.Input;
using System.Data;
using LiveCore.Desktop.Common;
using Prism.Ioc;
using System.Collections.Generic;

namespace Sentez.NermaMetalManagementModule
{
    public partial class NermaMetalManagementModule : LiveModule
    {
        //Deneme değişiklik
        IContainerExtension _container;
        SysMng _sysMng;
        LiveSession ActiveSession
        {
            get
            {
                return SysMng.Instance.getSession();
            }
        }

        public Stream _MenuDefination = null;
        public override Stream MenuDefination
        {
            get
            {
                return _MenuDefination;
            }
        }

        public override short moduleID { get { return (short)Modules.ExternalModule15; } }

        public NermaMetalManagementModule(IContainerExtension container)
        {
            _container = container;
            _sysMng = _container.Resolve<SysMng>();
            if (_sysMng != null)
            {
                _sysMng.AfterDesktopLogin += _sysMng_AfterDesktopLogin;
            }
        }
        public override void OnRegister(IContainerRegistry containerRegistry)
        {
            RegisterCoreDocuments();
            RegisterBO();
            RegisterViews();
            RegisterRes();
            RegisterRpr();
            RegisterPM();
            RegisterModuleCommands();
            RegisterServices();
            RegisterList();
            NermaMetalManagementModuleSecurity.RegisterSecurityDefinitions();

            MenuManager.Instance.RegisterMenu("NermaMetalManagementModule", "NermaMetalManagementModuleMenu", moduleID, true);
        }

        public override void OnInitialize(IContainerProvider containerProvider)
        {
            _sysMng.AddApplication("NermaMetalManagementModule");
        }

        public override void RegisterModuleCommands()
        {
        }


        private void _sysMng_AfterDesktopLogin(object sender, EventArgs e)
        {
            if (!Schema.Tables["Erp_DemandReceiptItem"].Fields.Contains("UD_SizeDetailCode"))
                CreatMetaDataFieldsService.CreatMetaDataFields("Erp_DemandReceiptItem", "UD_SizeDetailCode", SLanguage.GetString("Ölçü Kodu"), (byte)UdtType.UdtCode, (byte)FieldUsage.Code, (byte)EditorType.ListSelector, (byte)ValueInputMethod.FreeType, 0);
        }

        public void Initialize()
        {
        }

        private void RegisterBO()
        {
            _container.Register<IBusinessObject, UnitItemSizeSetDetailsBO>("UnitItemSizeSetDetailsBO");
            _container.Register<IBusinessObject, AttributeSetDetailsBO>("AttributeSetDetailsBO");
        }

        private void RegisterServices()
        {
            _container.Register<ISystemService, CreatMetaDataFieldsService>("CreatMetaDataFieldsService");
            BusinessObjectBase.AddCustomConstruction("InventoryBO", InventoryBoCustomCons);
            BusinessObjectBase.AddCustomInit("InventoryBO", InventoryBo_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomInit("InventoryPM", InventoryPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("InventoryPM", InventoryPm_Dispose_InventoryUnitItemSizeSetDetails);

            BusinessObjectBase.AddCustomInit("QuotationReceiptBO", QuotationReceiptBo_Init_InventoryUnitItemSizeSetDetails);
            BusinessObjectBase.AddCustomConstruction("QuotationReceiptBO", QuotationReceiptBoCustomCons);
            PMBase.AddCustomInit("QuotationReceiptPM", QuotationReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("QuotationReceiptPM", QuotationReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("QuotationReceiptPM", QuotationReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);

            BusinessObjectBase.AddCustomConstruction("VariantTypeBO", VariantTypeBoCustomCons);
            BusinessObjectBase.AddCustomInit("VariantTypeBO", VariantTypeBo_Init_VariantItemMark);
            PMBase.AddCustomInit("VariantType", VariantTypePm_Init_VariantItemMark);
            PMBase.AddCustomViewLoaded("VariantType", VariantTypePm_ViewLoaded_VariantItemMark);
            PMBase.AddCustomDispose("VariantType", VariantTypePm_Dispose_VariantItemMark);
            PMBase.AddCustomCommandExecutes("QuotationReceiptPM", QuotationReceiptPm_OnListCommand);

            BusinessObjectBase.AddCustomConstruction("CategoryBO", CategoryBoCustomCons);
            BusinessObjectBase.AddCustomInit("CategoryBO", CategoryBo_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomInit("Category", CategoryPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomInit("InventoryAttributeSet", CardPm_Init_InventoryAttributeSetItem);
            PMBase.AddCustomDispose("Category", CategoryPm_Dispose_InventoryUnitItemSizeSetDetails);

        }

        private void QuotationReceiptBoCustomCons(ref short itemId, ref string keyColumn, ref string typeField, ref string[] Tables)
        {
            List<string> tableList = new List<string>();
            tableList.AddRange(Tables);

            tableList.Add("Erp_QuotationReceiptRecipeItem");
            Tables = tableList.ToArray();
        }

        private void CardPm_Init_InventoryAttributeSetItem(PMBase pm, PmParam parameter)
        {
            inventoryAttributeSetPm = pm as CardPM;
            if (inventoryAttributeSetPm == null)
            {
                return;
            }
            LiveGridControl gridDetail = inventoryAttributeSetPm.FCtrl("gridDetail") as LiveGridControl;
            if (gridDetail != null)
            {
                if (!gridDetail.ColumnDefinitions.Contains("IsSelect"))
                    gridDetail.ColumnDefinitions.Add(new ReceiptColumn() { ColumnName = "IsSelect", Caption = "Seçim", EditorType = EditorType.CheckBox, Width = 80 });
            }
        }

        private bool QuotationReceiptPm_OnListCommand(PMBase pm, PmParam parameter, ISysCommandParam commandParam)
        {
            //throw new NotImplementedException();
            if (commandParam?.cmdName == "ListCommand" && quotationReceiptPm != null)
            {
                var focusScope = FocusManager.GetFocusScope(quotationReceiptPm.ActiveViewControl);
                var element = FocusManager.GetFocusedElement(focusScope) as FrameworkElement;
                var visualelement = FrameworkTreeHelper.FindVisualParent<LiveGridControl>(element);
                string name = quotationReceiptPm.GetFocusedField();
                if (visualelement is LiveGridControl && (visualelement?.Name == "GridDetail"))
                {
                    if (visualelement.CurrentColumn?.FieldName == "ItemVariant2Code")
                    {
                        (visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = "";
                        if (visualelement.CurrentItem is DataRowView)
                        {
                            bool inventoryIsSurfaceTreatment;
                            bool.TryParse((visualelement.CurrentItem as DataRowView).Row["InventoryIsSurfaceTreatment"].ToString(), out inventoryIsSurfaceTreatment);
                            if (!inventoryIsSurfaceTreatment)
                            {
                                quotationReceiptPm.sysMng.ActWndMng.ShowMsg($"{(visualelement.CurrentItem as DataRowView).Row["ItemCode"]}-{(visualelement.CurrentItem as DataRowView).Row["ItemName"]} {SLanguage.GetString("malzemesi için yüzey işlem yapılacak özelliği aktif edilmemiş!")}", ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK, Common.InformationMessages.MessageBoxImage.Warning);
                                return true;
                            }
                            else
                            {
                                if (!(visualelement.CurrentItem as DataRowView).Row.IsNull("MarkId"))
                                {
                                    //(visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = $" [erp_variantitem].[RecId] in (select VariantItemId from Erp_VariantItemMark with (nolock) where MarkId in (select MarkId from Erp_VariantItemMark with (nolock) where MarkId={(visualelement.CurrentItem as DataRowView).Row["MarkId"]}))";
                                    //(visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = $" [erp_variantitem].[RecId] in (select VariantItemId from Erp_VariantItemMark with (nolock) where MarkId in (select MarkId from Erp_VariantItemMark where MarkId={(visualelement.CurrentItem as DataRowView).Row["MarkId"]} and CardId in (select RecId from Erp_VariantCard where TypeId={(visualelement.CurrentItem as DataRowView).Row["Variant2TypeId"]})) and RecId in (select VariantItemId from Erp_VariantItemMark where MarkId={(visualelement.CurrentItem as DataRowView).Row["MarkId"]}))";
                                    (visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = $" [erp_variantitem].[RecId] IN (SELECT RecId FROM Erp_VariantItem EVI WITH (NOLOCK) WHERE EVI.RecId IN(SELECT EVIM.VariantItemId FROM Erp_VariantItemMark EVIM WITH (NOLOCK) WHERE EVIM.MarkId = {(visualelement.CurrentItem as DataRowView).Row["MarkId"]}) AND EVI.CardId IN (SELECT EVC.RecId FROM Erp_VariantCard EVC WITH (NOLOCK) WHERE EVC.TypeId={(visualelement.CurrentItem as DataRowView).Row["Variant2TypeId"]}))";

                                }
                                return false;
                            }
                        }
                    }
                    else if (visualelement.CurrentColumn?.FieldName == "MarkName")
                    {
                        (visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = "";
                        if (visualelement.CurrentItem is DataRowView)
                        {
                            bool inventoryIsSurfaceTreatment;
                            bool.TryParse((visualelement.CurrentItem as DataRowView).Row["InventoryIsSurfaceTreatment"].ToString(), out inventoryIsSurfaceTreatment);
                            if (!inventoryIsSurfaceTreatment)
                            {
                                quotationReceiptPm.sysMng.ActWndMng.ShowMsg($"{(visualelement.CurrentItem as DataRowView).Row["ItemCode"]}-{(visualelement.CurrentItem as DataRowView).Row["ItemName"]} {SLanguage.GetString("malzemesi için yüzey işlem yapılacak özelliği aktif edilmemiş!")}", ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK, Common.InformationMessages.MessageBoxImage.Warning);
                                return true;
                            }
                            else
                            {
                                if (!(visualelement.CurrentItem as DataRowView).Row.IsNull("ItemVariant2Id"))
                                {
                                    //(visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = $" and [erp_mark].[RecId] in (select RecId from Erp_InventoryMark with (nolock) where InventoryId={(visualelement.CurrentItem as DataRowView).Row["InventoryId"]})";
                                    (visualelement.CurrentColumn.Tag as ReceiptColumn).ListWhereStr = $" and [erp_mark].[RecId] in (select MarkId from Erp_VariantItemMark with (nolock) where VariantItemId={(visualelement.CurrentItem as DataRowView).Row["ItemVariant2Id"]})";
                                }
                                return false;
                            }
                        }
                    }
                    else if (visualelement.CurrentColumn?.FieldName == "NER_SurfaceType")
                    {
                        if (visualelement.CurrentItem is DataRowView)
                        {
                            bool inventoryIsSurfaceTreatment;
                            bool.TryParse((visualelement.CurrentItem as DataRowView).Row["InventoryIsSurfaceTreatment"].ToString(), out inventoryIsSurfaceTreatment);
                            if (!inventoryIsSurfaceTreatment)
                            {
                                quotationReceiptPm.sysMng.ActWndMng.ShowMsg($"{(visualelement.CurrentItem as DataRowView).Row["ItemCode"]}-{(visualelement.CurrentItem as DataRowView).Row["ItemName"]} {SLanguage.GetString("malzemesi için yüzey işlem yapılacak özelliği aktif edilmemiş!")}", ConstantStr.Warning, Common.InformationMessages.MessageBoxButton.OK, Common.InformationMessages.MessageBoxImage.Warning);
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            return true;
        }

        private void RegisterRes()
        {
            ResMng.AddRes("NermaMetalManagementModuleMenu", "NermaMetalManagementModule;component/ModuleMenu.xml", ResSource.Resource, ResourceType.MenuXml, Modules.ExternalModule15, 0, 0);
        }

        private void RegisterList()
        {
            _container.Register<IReport, UnitItemSizeSetDetailsList>("Erp_UnitItemSizeSetDetailsSizeDetailCodeList");
            _container.Register<IReport, InventoryUnitItemSizeSetDetails>("Erp_InventoryUnitItemSizeSetDetailsSizeDetailCodeList");

            _container.Register<IReport, AttributeSetDetailsList>("Erp_AttributeSetDetailsAttributeSetCodeList");
            _container.Register<IReport, CategoryAttributeSetDetails>("Erp_CategoryAttributeSetDetailsAttributeSetCodeList");
            //_container.Register<IReport, MetaCurrentAccountAnalysisSubjectList>("Meta_CurrentAccountAnalysisSubjectAnalysisSubjectCodeList");
            //_container.Register<IReport, MetaCurrentAccountAnalysisElementList>("Meta_CurrentAccountAnalysisElementAnalysisElementCodeList");

            //LookupList.Instance.AddLookupList("CekCalismasiList", "Display", typeof(string), new object[] {
            //    "Sorunlu","Sorunsuz"
            //}, "Value", typeof(byte), new object[] { (byte)0, (byte)1 });

            //LookupList.Instance.AddLookupList("MoralitesiList", "Display", typeof(string), new object[] {
            //    "Sorunlu","Sorunsuz"
            //}, "Value", typeof(byte), new object[] { (byte)0, (byte)1 });

            //LookupList.Instance.AddLookupList("StatuYilList", "Display", typeof(string), new object[] {
            //    "411","412","413","414","415","416","417","418","419","420","421","422","423","424","425","426","427","428","429","430"
            //}, "Value", typeof(int), new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 });
        }

        private void RegisterViews()
        {
            //ResMng.AddRes("SalesShipmentCompare", "NermaMetalManagementModule;component/Views/SalesShipmentCompare.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("SalesShipmentDetails", "NermaMetalManagementModule;component/Views/SalesShipmentDetails.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("FaultControlMechanism", "NermaMetalManagementModule;component/Views/FaultControlMechanism.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("FaultTaskControl", "NermaMetalManagementModule;component/Views/FaultTaskControl.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("FaultExplanationEntry", "NermaMetalManagementModule;component/Views/FaultExplanationEntry.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("VCMMonthlyActualCost", "NermaMetalManagementModule;component/Views/VCMMonthlyActualCost.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("CollectiveActualCost", "NermaMetalManagementModule;component/Views/CollectiveActualCost.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("OrderAllHistory", "NermaMetalManagementModule;component/Views/OrderAllHistory.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);

            ResMng.AddRes("InventoryUnitItemSizeSetDetailsView", "NermaMetalManagementModule;component/Views/InventoryUnitItemSizeSetDetails.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            ResMng.AddRes("UnitItemSizeSetDetailsView", "NermaMetalManagementModule;component/Views/UnitItemSizeSetDetails.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);

            ResMng.AddRes("CategoryUnitItemSizeSetDetailsView", "NermaMetalManagementModule;component/Views/CategoryUnitItemSizeSetDetails.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);

            ResMng.AddRes("InventoryMarksView", "NermaMetalManagementModule;component/Views/InventoryMarks.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            ResMng.AddRes("VariantItemMarksView", "NermaMetalManagementModule;component/Views/VariantItemMarks.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);

            ResMng.AddRes("AttributeSetDetailsView", "NermaMetalManagementModule;component/Views/AttributeSetDetails.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            ResMng.AddRes("CategoryAttributeSetDetailsView", "NermaMetalManagementModule;component/Views/CategoryAttributeSetDetails.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);

            ResMng.AddRes("QuotationRecipeItemView", "NermaMetalManagementModule;component/Views/QuotationRecipeItem.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
        }

        private void RegisterPM()
        {
            //_container.Register<IPMBase, SalesShipmentComparePM>("SalesShipmentComparePM");
            //_container.Register<IPMBase, FaultTaskControlPM>("FaultTaskControlPM");
            //_container.Register<IPMBase, VCMMonthlyActualCostPM>("VCMMonthlyActualCostPM");
            //_container.Register<IPMBase, SalesShipmentDetailsPM>("SalesShipmentDetailsPM");
            //_container.Register<IPMBase, CollectiveActualCostPM>("CollectiveActualCostPM");
            //_container.Register<IPMBase, OrderAllHistoryPM>("OrderAllHistoryPM");
        }

        private void RegisterRpr()
        {
            _container.Register<IReport, SalesShipmentComparePolicy>("SalesShipmentComparePolicy");
            _container.Register<IReport, FaultTaskControlPolicy>("FaultTaskControlPolicy");
            _container.Register<ISystemService, FaultQueryService>("FaultQueryService");
        }

        public void RegisterCoreDocuments()
        {
            Data.MetaData.Schema.ReadXml(Assembly.GetAssembly(typeof(NermaMetalManagementModule)).GetManifestResourceStream("NermaMetalManagementModule.NermaMetalManagementModuleDataSchema.xml"));
            DbCreator.AddRegistration(3014, NermaMetalManagementModuleDbUpdateScript);
        }

        DbScripts NermaMetalManagementModuleDbUpdateScript(DbCreator instance)
        {
            return DbScripts.LoadFromAssembly(Assembly.GetAssembly(typeof(NermaMetalManagementModule)), "NermaMetalManagementModule.NermaMetalManagementModuleDbUpdateScripts.xml");
        }
    }
}
