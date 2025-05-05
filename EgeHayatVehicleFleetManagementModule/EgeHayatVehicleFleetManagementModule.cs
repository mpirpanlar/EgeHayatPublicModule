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
using LiveCore.Desktop.Common;
using Prism.Ioc;
using Sentez.Common.SBase;
using Sentez.Data.BusinessObjects;
using Sentez.Common.PresentationModels;
using Sentez.Common.Report;
using EgeHayatVehicleFleetManagementModule.Services;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;
using Sentez.Core.ParameterClasses;
using System.Windows.Threading;
using System.Windows;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Sentez.EgeHayatVehicleFleetManagementModule.PresentationModels;
using Sentez.Parameters;
using Sentez.EgeHayatVehicleFleetManagementModule.Parameters;
using Sentez.Data.Tools;
using Sentez.Data.MetaData;
using Sentez.Localization;
using Sentez.EgeHayatVehicleFleetManagementModule.Models;
using Sentez.Common.Utilities;

namespace Sentez.EgeHayatVehicleFleetManagementModule
{
    public partial class EgeHayatVehicleFleetManagementModule : LiveModule
    {
        SysMng _sysMng;
        LiveSession liveSession = null;
        EgeHayatVehicleFleetManagementModuleParameters EgeHayatVehicleFleetManagementModuleParameters;
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

        public override short moduleID { get { return (short)Modules.ExternalModule20; } }

        public EgeHayatVehicleFleetManagementModule(IContainerExtension container)
        {
            _container = container;
            _sysMng = _container.Resolve<SysMng>();
            if (_sysMng != null)
            {
                _sysMng.AfterDesktopLogin += _sysMng_AfterDesktopLogin;
                _sysMng.BeforeLogout += _sysMng_BeforeLogout;
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
            EgeHayatVehicleFleetManagementModuleSecurity.RegisterSecurityDefinitions();

            MenuManager.Instance.RegisterMenu("EgeHayatVehicleFleetManagementModule", "EgeHayatVehicleFleetManagementModuleMenu", moduleID, true);
        }

        public override void OnInitialize(IContainerProvider containerProvider)
        {
            _sysMng.AddApplication("EgeHayatVehicleFleetManagementModule");
        }

        public override void RegisterModuleCommands()
        {
        }

        public void Initialize()
        {
        }

        private void RegisterBO()
        {
            _container.Register<IBusinessObject, VehicleAssignmentBO>("VehicleAssignmentBO");
            _container.Register<IBusinessObject, VehicleInspectionBO>("VehicleInspectionBO");
            _container.Register<IBusinessObject, VehicleMaintenanceBO>("VehicleMaintenanceBO");
        }

        private void RegisterServices()
        {
            ParameterFactory.StaticFactory.RegisterParameterClass(typeof(EgeHayatVehicleFleetManagementModuleParameters), (int)Modules.ExternalModule20);
            _container.Register<ISystemService, CreatMetaDataFieldsService>("CreatMetaDataFieldsService");
            //BusinessObjectBase.AddCustomExtension("OrderReceiptBO", typeof(OrderReceiptControlExtension));

            //BusinessObjectBase.AddCustomConstruction("CurrentAccountBO", CurrentAccountBoCustomCons);
            //BusinessObjectBase.AddCustomInit("CurrentAccountBO", CurrentAccountBo_Init);

            //BusinessObjectBase.AddCustomConstruction("CRMCustomerTransactionBO", CrmCustomerTransactionBoCustomCons);
            //BusinessObjectBase.AddCustomInit("CRMCustomerTransactionBO", CrmCustomerTransactionBo_Init);

            //PMBase.AddCustomInit("CurrentAccountPM", CurrentAccountPm_Init);
            //PMBase.AddCustomInit("CRMCustomerTransactionPM", CrmCustomerTransactionPm_Init);
        }

        private void RegisterRes()
        {
            ResMng.AddRes("EgeHayatVehicleFleetManagementModuleMenu", "EgeHayatVehicleFleetManagementModule;component/ModuleMenu.xml", ResSource.Resource, ResourceType.MenuXml, Modules.ExternalModule20, 0, 0);
        }

        private void RegisterList()
        {
            //_container.Register<IReport, CrmActivityTypeList>("Crm_ActivityTypeTypeNameList");
            //_container.Register<IReport, CrmActivityChecklistItemList>("Crm_ActivityChecklistItemChecklistTitleList");
        }

        private void RegisterViews()
        {
            ResMng.AddRes("EgeHayatVehicleFleetManagementModuleParametersView", "EgeHayatVehicleFleetManagementModule;component/Views/EgeHayatVehicleFleetManagementModuleParameters.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule20, 0, 0);
            ResMng.AddRes("VehicleAssignmentView", "EgeHayatVehicleFleetManagementModule;component/Views/VehicleAssignment.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule20, 0, 0);
            ResMng.AddRes("VehicleInspectionView", "EgeHayatVehicleFleetManagementModule;component/Views/VehicleInspection.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule20, 0, 0);
            ResMng.AddRes("VehicleMaintenanceView", "EgeHayatVehicleFleetManagementModule;component/Views/VehicleMaintenance.xaml", ResSource.Resource, ResourceType.View, Modules.ExternalModule20, 0, 0);
        }

        private void RegisterPM()
        {
            _container.Register<IPMBase, EgeHayatVehicleFleetManagementModuleParametersPM>("EgeHayatVehicleFleetManagementModuleParametersPM");
        }

        private void RegisterRpr()
        {
            //_container.Register<IReport, SalesShipmentComparePolicy>("SalesShipmentComparePolicy");
        }

        public void RegisterCoreDocuments()
        {
            Schema.ReadXml(Assembly.GetAssembly(typeof(EgeHayatVehicleFleetManagementModule)).GetManifestResourceStream("EgeHayatVehicleFleetManagementModule.EgeHayatVehicleFleetManagementModuleDataSchema.xml"));
            DbCreator.AddRegistration(3014, EgeHayatVehicleFleetManagementModuleDbUpdateScript);
        }

        DbScripts EgeHayatVehicleFleetManagementModuleDbUpdateScript(DbCreator instance)
        {
            return DbScripts.LoadFromAssembly(Assembly.GetAssembly(typeof(EgeHayatVehicleFleetManagementModule)), "EgeHayatVehicleFleetManagementModule.EgeHayatVehicleFleetManagementModuleDbUpdateScripts.xml");
        }

        private CancellationTokenSource bilgeceBoomerangCts;
        private static readonly object bilgeceBoomerangLockKey = new object();

        private void _sysMng_AfterDesktopLogin(object sender, EventArgs e)
        {
            liveSession = _sysMng.getSession();
            EgeHayatVehicleFleetManagementModuleParameters = liveSession.ParamService.GetParameterClass<EgeHayatVehicleFleetManagementModuleParameters>();

            LookupList.Instance.AddLookupList("MaintenanceTypeList", "TypeName", typeof(string), new object[] { SLanguage.GetString("Periyodik Bakım")
                                                                                                , SLanguage.GetString("Yağ ve Filtre Değişimi")
                                                                                                , SLanguage.GetString("Fren Sistemi Kontrolü")
                                                                                                , SLanguage.GetString("Lastik Değişimi veya Rotasyonu")
                                                                                                , SLanguage.GetString("Akü Kontrolü")
                                                                                                , SLanguage.GetString("Klima ve Havalandırma Bakımı")
                                                                                                , SLanguage.GetString("Şanzıman Yağı Kontrolü")
                                                                                                , SLanguage.GetString("Motor Performans Testi")
                                                                                                , SLanguage.GetString("Far ve Aydınlatma Kontrolü")
                                                                                                , SLanguage.GetString("Egzoz Emisyon Ölçümü")
                                                                                                , SLanguage.GetString("TÜVTÜRK Muayene")
                                                                                                , SLanguage.GetString("Diğer")
                                                                                              }
                                                                                              , "Type",
                                                                                              typeof(byte), new object[] { (byte)0, (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10, (byte)99 });
        }

        private void _sysMng_BeforeLogout(object sender, EventArgs e)
        {
        }
    }
}
