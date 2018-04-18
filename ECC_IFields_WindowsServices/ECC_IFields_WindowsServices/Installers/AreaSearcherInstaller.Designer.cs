namespace ECC_IFields_WindowsServices.Installers
{
    partial class AreaSearcherInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.areaSearcherServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller1
            // 
            this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceProcessInstaller1.Password = null;
            this.serviceProcessInstaller1.Username = null;
            // 
            // areaSearcherServiceInstaller
            // 
            this.areaSearcherServiceInstaller.Description = "Gets the new created tags in each area server and insert the result in the orcl d" +
    "atabase.";
            this.areaSearcherServiceInstaller.DisplayName = "ECCPIAreaSearcher";
            this.areaSearcherServiceInstaller.ServiceName = "ECCPIAreaSearcher";
            this.areaSearcherServiceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.areaSearcherServiceInstaller_AfterInstall);
            // 
            // AreaSearcherInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller1,
            this.areaSearcherServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller areaSearcherServiceInstaller;
    }
}