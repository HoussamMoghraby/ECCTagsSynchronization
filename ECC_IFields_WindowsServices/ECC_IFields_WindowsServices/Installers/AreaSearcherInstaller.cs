using ECC_IFields_WindowsServices.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace ECC_IFields_WindowsServices.Installers
{
    [RunInstaller(Constants.DeployPackages.ECCPIAreaSearcher)]
    public partial class AreaSearcherInstaller : System.Configuration.Install.Installer
    {
        public AreaSearcherInstaller()
        {
            var cc = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("aa"));
            InitializeComponent();
        }

        private void areaSearcherServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
