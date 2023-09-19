using LiveCore.Desktop.SBase.MenuManager;
using Sentez.Common;
using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.ResourceManager;
using Sentez.Common.SystemServices;
using Sentez.Data.MetaData.DatabaseControl;
using System;
using System.IO;
using System.Reflection;
using NermaReservationManagementModule.Services;
using Sentez.Data.MetaData;
using Sentez.Data.Tools;
using Sentez.Localization;
using LiveCore.Desktop.Common;
using Prism.Ioc;

namespace Sentez.NermaReservationManagementModule
{
    public partial class NermaReservationManagementModule : LiveModule
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

        public override short moduleID { get { return (short)Modules.ExternalModule17; } }

        public NermaReservationManagementModule(IContainerExtension container)
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
            NermaReservationManagementModuleSecurity.RegisterSecurityDefinitions();

            MenuManager.Instance.RegisterMenu("NermaReservationManagementModule", "NermaReservationManagementModuleMenu", moduleID, true);
        }

        public override void OnInitialize(IContainerProvider containerProvider)
        {
            _sysMng.AddApplication("NermaReservationManagementModule");
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


            if (!Schema.Tables["Erp_InventoryPriceList"].Fields.Contains("UD_Variant1PriceEfective"))
                CreatMetaDataFieldsService.CreatMetaDataFields("Erp_InventoryPriceList", "UD_Variant1PriceEfective", SLanguage.GetString("Varyant-1 Fiyatta Etkili"), (byte)UdtType.UdtBool, (byte)FieldUsage.Bool, (byte)EditorType.CheckBox, (byte)ValueInputMethod.FreeType, 0);
            if (!Schema.Tables["Erp_InventoryPriceList"].Fields.Contains("UD_Variant2PriceEfective"))
                CreatMetaDataFieldsService.CreatMetaDataFields("Erp_InventoryPriceList", "UD_Variant2PriceEfective", SLanguage.GetString("Varyant-2 Fiyatta Etkili"), (byte)UdtType.UdtBool, (byte)FieldUsage.Bool, (byte)EditorType.CheckBox, (byte)ValueInputMethod.FreeType, 0);
            if (!Schema.Tables["Erp_InventoryPriceList"].Fields.Contains("UD_Variant3PriceEfective"))
                CreatMetaDataFieldsService.CreatMetaDataFields("Erp_InventoryPriceList", "UD_Variant3PriceEfective", SLanguage.GetString("Varyant-3 Fiyatta Etkili"), (byte)UdtType.UdtBool, (byte)FieldUsage.Bool, (byte)EditorType.CheckBox, (byte)ValueInputMethod.FreeType, 0);
            if (!Schema.Tables["Erp_InventoryPriceList"].Fields.Contains("UD_Variant4PriceEfective"))
                CreatMetaDataFieldsService.CreatMetaDataFields("Erp_InventoryPriceList", "UD_Variant4PriceEfective", SLanguage.GetString("Varyant-4 Fiyatta Etkili"), (byte)UdtType.UdtBool, (byte)FieldUsage.Bool, (byte)EditorType.CheckBox, (byte)ValueInputMethod.FreeType, 0);
            if (!Schema.Tables["Erp_InventoryPriceList"].Fields.Contains("UD_Variant5PriceEfective"))
                CreatMetaDataFieldsService.CreatMetaDataFields("Erp_InventoryPriceList", "UD_Variant5PriceEfective", SLanguage.GetString("Varyant-5 Fiyatta Etkili"), (byte)UdtType.UdtBool, (byte)FieldUsage.Bool, (byte)EditorType.CheckBox, (byte)ValueInputMethod.FreeType, 0);
        }

        public void Initialize()
        {
        }

        private void RegisterBO()
        {
            //_container.Register<IBusinessObject, UnitItemSizeSetDetailsBO>("UnitItemSizeSetDetailsBO");
        }

        private void RegisterServices()
        {
            _container.Register<ISystemService, CreatMetaDataFieldsService>("CreatMetaDataFieldsService");
            //BusinessObjectBase.AddCustomConstruction("InventoryBO", InventoryBoCustomCons);
            //BusinessObjectBase.AddCustomInit("InventoryBO", InventoryBo_Init_InventoryUnitItemSizeSetDetails);
            //PMBase.AddCustomInit("InventoryPM", InventoryPm_Init_InventoryUnitItemSizeSetDetails);
            //PMBase.AddCustomDispose("InventoryPM", InventoryPm_Dispose_InventoryUnitItemSizeSetDetails);

            //BusinessObjectBase.AddCustomConstruction("QuotationReceiptBO", QuotationReceiptBoCustomCons);

        }

        //private void QuotationReceiptBoCustomCons(ref short itemId, ref string keyColumn, ref string typeField, ref string[] Tables)
        //{
        //    List<string> tableList = new List<string>();
        //    tableList.AddRange(Tables);

        //    tableList.Add("Erp_QuotationReceiptRecipeItem");
        //    Tables = tableList.ToArray();
        //}

        private void RegisterRes()
        {
            ResMng.AddRes("NermaReservationManagementModuleMenu", "NermaReservationManagementModule;component/ModuleMenu.xml", ResSource.Resource, ResourceType.MenuXml, Modules.ExternalModule15, 0, 0);
        }

        private void RegisterList()
        {
            //_container.Register<IReport, UnitItemSizeSetDetailsList>("Erp_UnitItemSizeSetDetailsSizeDetailCodeList");
            //_container.Register<IReport, InventoryUnitItemSizeSetDetails>("Erp_InventoryUnitItemSizeSetDetailsSizeDetailCodeList");

            //_container.Register<IReport, AttributeSetDetailsList>("Erp_AttributeSetDetailsAttributeSetCodeList");
            //_container.Register<IReport, CategoryAttributeSetDetails>("Erp_CategoryAttributeSetDetailsAttributeSetCodeList");

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
            //ResMng.AddRes("SalesShipmentCompare", "NermaReservationManagementModule;component/Views/SalesShipmentCompare.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("SalesShipmentDetails", "NermaReservationManagementModule;component/Views/SalesShipmentDetails.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("FaultControlMechanism", "NermaReservationManagementModule;component/Views/FaultControlMechanism.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("FaultTaskControl", "NermaReservationManagementModule;component/Views/FaultTaskControl.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("FaultExplanationEntry", "NermaReservationManagementModule;component/Views/FaultExplanationEntry.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("VCMMonthlyActualCost", "NermaReservationManagementModule;component/Views/VCMMonthlyActualCost.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("CollectiveActualCost", "NermaReservationManagementModule;component/Views/CollectiveActualCost.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
            //ResMng.AddRes("OrderAllHistory", "NermaReservationManagementModule;component/Views/OrderAllHistory.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);

            ResMng.AddRes("UnitItemSizeSetDetailsView", "NermaReservationManagementModule;component/Views/UnitItemSizeSetDetails.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule15, 0, 0);
        }

        private void RegisterPM()
        {
            //_container.Register<IPMBase, SalesShipmentComparePM>("SalesShipmentComparePM");
        }

        private void RegisterRpr()
        {
            //_container.Register<IReport, SalesShipmentComparePolicy>("SalesShipmentComparePolicy");
            //_container.Register<IReport, FaultTaskControlPolicy>("FaultTaskControlPolicy");
            //_container.Register<ISystemService, FaultQueryService>("FaultQueryService");
        }

        public void RegisterCoreDocuments()
        {
            Data.MetaData.Schema.ReadXml(Assembly.GetAssembly(typeof(NermaReservationManagementModule)).GetManifestResourceStream("NermaReservationManagementModule.NermaReservationManagementModuleDataSchema.xml"));
            DbCreator.AddRegistration(3014, NermaReservationManagementModuleDbUpdateScript);
        }

        DbScripts NermaReservationManagementModuleDbUpdateScript(DbCreator instance)
        {
            return DbScripts.LoadFromAssembly(Assembly.GetAssembly(typeof(NermaReservationManagementModule)), "NermaReservationManagementModule.NermaReservationManagementModuleDbUpdateScripts.xml");
        }
    }
}
