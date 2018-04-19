using ECC_IFields_WindowsServices.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace ECC_IFields_WindowsServices.Installers
{
    [RunInstaller(Constants.DeployPackages.ECCPITagCreator)]
    public partial class TagCreatorInstaller : System.Configuration.Install.Installer
    {
        public TagCreatorInstaller()
        {
            InitializeComponent();
        }

        private void ECCPITagCreator_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void serviceProcessInstaller2_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
