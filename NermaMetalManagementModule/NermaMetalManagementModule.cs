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
using Sentez.Core.ParameterClasses;
using Sentez.InventoryModule.PresentationModels;
using Sentez.NermaMetalManagementModule.PresentationModels;
using Sentez.Common.SBase;

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
            if (!Schema.Tables["Erp_OrderReceiptItem"].Fields.Contains("UD_SizeDetailCode"))
                CreatMetaDataFieldsService.CreatMetaDataFields("Erp_OrderReceiptItem", "UD_SizeDetailCode", SLanguage.GetString("Ölçü Kodu"), (byte)UdtType.UdtCode, (byte)FieldUsage.Code, (byte)EditorType.ListSelector, (byte)ValueInputMethod.FreeType, 0);
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

            #region Teklif fiş giriş ekran menüleri
            PMBase.AddCustomInit("QuotationReceiptPM", QuotationReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("QuotationReceiptPM", QuotationReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("QuotationReceiptPM", QuotationReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("QuotationReceiptPM", QuotationReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Urun", QuotationReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Urun", QuotationReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Urun", QuotationReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Urun", QuotationReceiptPm_OnListCommand);

            PMBase.AddCustomInit("AluminyumProfil", QuotationReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("AluminyumProfil", QuotationReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("AluminyumProfil", QuotationReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("AluminyumProfil", QuotationReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Kompozit", QuotationReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Kompozit", QuotationReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Kompozit", QuotationReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Kompozit", QuotationReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Cam", QuotationReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Cam", QuotationReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Cam", QuotationReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Cam", QuotationReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Donanım", QuotationReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Donanım", QuotationReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Donanım", QuotationReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Donanım", QuotationReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Aksesuar", QuotationReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Aksesuar", QuotationReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Aksesuar", QuotationReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Aksesuar", QuotationReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Diger", QuotationReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Diger", QuotationReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Diger", QuotationReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Diger", QuotationReceiptPm_OnListCommand);
            #endregion

            BusinessObjectBase.AddCustomInit("OrderReceiptBO", OrderReceiptBo_Init_InventoryUnitItemSizeSetDetails);
            BusinessObjectBase.AddCustomConstruction("OrderReceiptBO", OrderReceiptBoCustomCons);

            #region Sipariş fişi giriş ekran menüleri
            PMBase.AddCustomInit("OrderReceiptPM2", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("OrderReceiptPM2", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("OrderReceiptPM2", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("OrderReceiptPM2", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("OrderReceiptPM2_2_0", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("OrderReceiptPM2_2_0", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("OrderReceiptPM2_2_0", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("OrderReceiptPM2_2_0", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Urun2", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Urun2", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Urun2", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Urun2", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Urun2_2_0", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Urun2_2_0", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Urun2_2_0", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Urun2_2_0", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("AluminyumProfil2", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("AluminyumProfil2", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("AluminyumProfil2", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("AluminyumProfil2", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("AluminyumProfil2_2_0", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("AluminyumProfil2_2_0", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("AluminyumProfil2_2_0", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("AluminyumProfil2_2_0", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Kompozit2", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Kompozit2", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Kompozit2", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Kompozit2", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Kompozit2_2_0", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Kompozit2_2_0", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Kompozit2_2_0", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Kompozit2_2_0", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Cam2_2_0", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Cam2_2_0", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Cam2_2_0", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Cam2_2_0", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Cam2", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Cam2", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Cam2", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Cam2", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Donanım2_2_0", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Donanım2_2_0", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Donanım2_2_0", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Donanım2_2_0", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Donanım2", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Donanım2", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Donanım2", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Donanım2", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Aksesuar2_2_0", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Aksesuar2_2_0", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Aksesuar2_2_0", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Aksesuar2_2_0", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Aksesuar2", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Aksesuar2", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Aksesuar2", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Aksesuar2", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Diger2_2_0", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Diger2_2_0", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Diger2_2_0", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Diger2_2_0", OrderReceiptPm_OnListCommand);

            PMBase.AddCustomInit("Diger2", OrderReceiptPm_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomViewLoaded("Diger2", OrderReceiptPm_ViewLoaded_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomDispose("Diger2", OrderReceiptPm_Dispose_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomCommandExecutes("Diger2", OrderReceiptPm_OnListCommand);
            #endregion

            BusinessObjectBase.AddCustomConstruction("VariantTypeBO", VariantTypeBoCustomCons);
            BusinessObjectBase.AddCustomInit("VariantTypeBO", VariantTypeBo_Init_VariantItemMark);
            PMBase.AddCustomInit("VariantType", VariantTypePm_Init_VariantItemMark);
            PMBase.AddCustomViewLoaded("VariantType", VariantTypePm_ViewLoaded_VariantItemMark);
            PMBase.AddCustomDispose("VariantType", VariantTypePm_Dispose_VariantItemMark);

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
            ResMng.AddRes("InventoryPriceListDetailsView", "NermaMetalManagementModule;component/Views/InventoryPriceListDetails.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);

            ResMng.AddRes("CategoryUnitItemSizeSetDetailsView", "NermaMetalManagementModule;component/Views/CategoryUnitItemSizeSetDetails.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);

            ResMng.AddRes("InventoryMarksView", "NermaMetalManagementModule;component/Views/InventoryMarks.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            ResMng.AddRes("VariantItemMarksView", "NermaMetalManagementModule;component/Views/VariantItemMarks.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);

            ResMng.AddRes("AttributeSetDetailsView", "NermaMetalManagementModule;component/Views/AttributeSetDetails.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            ResMng.AddRes("CategoryAttributeSetDetailsView", "NermaMetalManagementModule;component/Views/CategoryAttributeSetDetails.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);

            ResMng.AddRes("QuotationRecipeItemView", "NermaMetalManagementModule;component/Views/QuotationRecipeItem.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            ResMng.AddRes("OrderRecipeItemView", "NermaMetalManagementModule;component/Views/OrderRecipeItem.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
        }

        private void RegisterPM()
        {
            //_container.Register<IPMBase, SalesShipmentComparePM>("SalesShipmentComparePM");
            //_container.Register<IPMBase, FaultTaskControlPM>("FaultTaskControlPM");
            //_container.Register<IPMBase, VCMMonthlyActualCostPM>("VCMMonthlyActualCostPM");
            //_container.Register<IPMBase, SalesShipmentDetailsPM>("SalesShipmentDetailsPM");
            //_container.Register<IPMBase, CollectiveActualCostPM>("CollectiveActualCostPM");
            _container.Register<IPMBase, InventoryPriceListDetailsPM>("InventoryPriceListDetailsPM");
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
