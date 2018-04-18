namespace ECC_IFields_WindowsServices.Installers
{
    partial class TagCreatorInstaller
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
            this.ECCPITagCreator = new System.ServiceProcess.ServiceInstaller();
            this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            // 
            // ECCPITagCreator
            // 
            this.ECCPITagCreator.Description = "Creates the fields tags into central ECC PI Server";
            this.ECCPITagCreator.DisplayName = "ECCPITagCreator";
            this.ECCPITagCreator.ServiceName = "ECCPITagCreator";
            this.ECCPITagCreator.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.ECCPITagCreator_AfterInstall);
            // 
            // serviceProcessInstaller1
            // 
            this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceProcessInstaller1.Password = null;
            this.serviceProcessInstaller1.Username = null;
            this.serviceProcessInstaller1.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller2_AfterInstall);
            // 
            // TagCreatorInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ECCPITagCreator,
            this.serviceProcessInstaller1});

        }

        #endregion

        private System.ServiceProcess.ServiceInstaller ECCPITagCreator;
        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
    }
}