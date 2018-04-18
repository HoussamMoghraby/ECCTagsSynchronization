using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace ECC_IFields_WindowsServices.Installers
{
    [RunInstaller(true)]
    public partial class AreaSearcherInstaller : System.Configuration.Install.Installer
    {
        public AreaSearcherInstaller()
        {
            InitializeComponent();
        }

        private void areaSearcherServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
