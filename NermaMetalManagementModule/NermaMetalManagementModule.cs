using LiveCore.Desktop.SBase.MenuManager;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Unity;
using Sentez.Common;
using Sentez.Common.Commands;
using Sentez.Common.ModuleBase;
using Sentez.Common.Report;
using Sentez.Common.ResourceManager;
using Sentez.Common.SBase;
using Sentez.Common.SystemServices;
using Sentez.Data.BusinessObjects;
using Sentez.Data.MetaData.DatabaseControl;
//using Sentez.NermaMetalManagementModule.PresentationModels;
using Sentez.NermaMetalManagementModule.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NermaMetalManagementModule.Services;
using Sentez.Common.PresentationModels;
using Sentez.NermaMetalManagementModule.Models;
using Sentez.Common.Utilities;
using Sentez.NermaMetalManagementModule.WorkList;

namespace Sentez.NermaMetalManagementModule
{
    public partial class NermaMetalManagementModule : IModule, ISentezModule
    {
        //Deneme değişiklik
        IUnityContainer _container;
        SysMng _sysMng;
        LiveSession ActiveSession
        {
            get
            {
                return SysMng.Instance.getSession();
            }
        }

        public Stream _MenuDefination = null;
        public Stream MenuDefination
        {
            get
            {
                return _MenuDefination;
            }
        }

        public short moduleID { get { return (short)Modules.ExternalModule15; } }

        public NermaMetalManagementModule(IUnityContainer container)
        {
            _container = container;
            _sysMng = _container.Resolve<SysMng>();
        }

        public void Initialize()
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
            VogueCostModuleSecurity.RegisterSecurityDefinitions();

            MenuManager.Instance.RegisterMenu("NermaMetalManagementModule", "NermaMetalManagementModuleMenu", moduleID, true);
            _sysMng.AddApplication("NermaMetalManagementModule");
        }

        private void RegisterBO()
        {
            _container.RegisterType<IBusinessObject, UnitItemSizeSetDetailsBO>("UnitItemSizeSetDetailsBO");
        }

        private void RegisterServices()
        {
            BusinessObjectBase.AddCustomConstruction("InventoryBO", InventoryBoCustomCons);
            BusinessObjectBase.AddCustomInit("InventoryBO", InventoryBo_Init_InventoryUnitItemSizeSetDetails);
            PMBase.AddCustomInit("InventoryPM", InventoryPm_Init_InventoryUnitItemSizeSetDetails);
        }

        private void InventoryBoCustomCons(ref short itemId, ref string keyColumn, ref string typeField, ref string[] Tables)
        {
            List<string> tableList = new List<string>();
            tableList.AddRange(Tables);

            tableList.Add("Erp_InventoryUnitItemSizeSetDetails");
            Tables = tableList.ToArray();
        }

        private void RegisterRes()
        {
            ResMng.AddRes("NermaMetalManagementModuleMenu", "NermaMetalManagementModule;component/ModuleMenu.xml", ResSource.Resource, ResourceType.MenuXml, Modules.ExternalModule15, 0, 0);
        }

        private void RegisterList()
        {
            _container.RegisterType<IReport, UnitItemSizeSetDetailsList>("Erp_UnitItemSizeSetDetailsSizeDetailCodeList");
            //_container.RegisterType<IReport, MetaCurrentAccountAnalysisSubjectList>("Meta_CurrentAccountAnalysisSubjectAnalysisSubjectCodeList");
            //_container.RegisterType<IReport, MetaCurrentAccountAnalysisElementList>("Meta_CurrentAccountAnalysisElementAnalysisElementCodeList");

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
        }

        private void RegisterPM()
        {
            //_container.RegisterType<IPMBase, SalesShipmentComparePM>("SalesShipmentComparePM");
            //_container.RegisterType<IPMBase, FaultTaskControlPM>("FaultTaskControlPM");
            //_container.RegisterType<IPMBase, VCMMonthlyActualCostPM>("VCMMonthlyActualCostPM");
            //_container.RegisterType<IPMBase, SalesShipmentDetailsPM>("SalesShipmentDetailsPM");
            //_container.RegisterType<IPMBase, CollectiveActualCostPM>("CollectiveActualCostPM");
            //_container.RegisterType<IPMBase, OrderAllHistoryPM>("OrderAllHistoryPM");
        }

        private void RegisterRpr()
        {
            _container.RegisterType<IReport, SalesShipmentComparePolicy>("SalesShipmentComparePolicy");
            _container.RegisterType<IReport, FaultTaskControlPolicy>("FaultTaskControlPolicy");
            _container.RegisterType<ISystemService, FaultQueryService>("FaultQueryService");
        }

        public void RegisterModuleCommands()
        {
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
