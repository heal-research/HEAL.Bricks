#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks.UI.WindowsForms {
  partial class SettingsForm {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.pathsGroupBox = new System.Windows.Forms.GroupBox();
      this.selectPackagesCachePathButton = new System.Windows.Forms.Button();
      this.selectPackagesPathButton = new System.Windows.Forms.Button();
      this.packagesCachePathTextBox = new System.Windows.Forms.TextBox();
      this.packagesPathTextBox = new System.Windows.Forms.TextBox();
      this.packagesCachePathLabel = new System.Windows.Forms.Label();
      this.appPathLabel = new System.Windows.Forms.Label();
      this.appPathTextBox = new System.Windows.Forms.TextBox();
      this.packagesPathLabel = new System.Windows.Forms.Label();
      this.repositoriesGroupBox = new System.Windows.Forms.GroupBox();
      this.editRepositoryButton = new System.Windows.Forms.Button();
      this.moveRepositoryDownButton = new System.Windows.Forms.Button();
      this.moveRepositoryUpButton = new System.Windows.Forms.Button();
      this.removeRepositoryButton = new System.Windows.Forms.Button();
      this.addRepositoryButton = new System.Windows.Forms.Button();
      this.repositoriesListBox = new System.Windows.Forms.ListBox();
      this.isolationGroupBox = new System.Windows.Forms.GroupBox();
      this.selectStarterAssemblyButton = new System.Windows.Forms.Button();
      this.useWindowsContainerLabel = new System.Windows.Forms.Label();
      this.dockerImageLabel = new System.Windows.Forms.Label();
      this.dotnetCommandLabel = new System.Windows.Forms.Label();
      this.starterAssemblyLabel = new System.Windows.Forms.Label();
      this.dockerCommandLabel = new System.Windows.Forms.Label();
      this.useWindowsContainerCheckBox = new System.Windows.Forms.CheckBox();
      this.dockerImageTextBox = new System.Windows.Forms.TextBox();
      this.starterAssemblyTextBox = new System.Windows.Forms.TextBox();
      this.dockerCommandTextBox = new System.Windows.Forms.TextBox();
      this.dotnetCommandTextBox = new System.Windows.Forms.TextBox();
      this.isolationComboBox = new System.Windows.Forms.ComboBox();
      this.isolationLabel = new System.Windows.Forms.Label();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.pathsGroupBox.SuspendLayout();
      this.repositoriesGroupBox.SuspendLayout();
      this.isolationGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Image = ((System.Drawing.Image)(resources.GetObject("okButton.Image")));
      this.okButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.okButton.Location = new System.Drawing.Point(735, 526);
      this.okButton.Name = "okButton";
      this.okButton.Padding = new System.Windows.Forms.Padding(0, 0, 12, 0);
      this.okButton.Size = new System.Drawing.Size(66, 23);
      this.okButton.TabIndex = 3;
      this.okButton.Text = "&OK";
      this.okButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.okButton.UseVisualStyleBackColor = true;
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton.Image")));
      this.cancelButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.cancelButton.Location = new System.Drawing.Point(807, 526);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(66, 23);
      this.cancelButton.TabIndex = 4;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // pathsGroupBox
      // 
      this.pathsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pathsGroupBox.Controls.Add(this.selectPackagesCachePathButton);
      this.pathsGroupBox.Controls.Add(this.selectPackagesPathButton);
      this.pathsGroupBox.Controls.Add(this.packagesCachePathTextBox);
      this.pathsGroupBox.Controls.Add(this.packagesPathTextBox);
      this.pathsGroupBox.Controls.Add(this.packagesCachePathLabel);
      this.pathsGroupBox.Controls.Add(this.appPathLabel);
      this.pathsGroupBox.Controls.Add(this.appPathTextBox);
      this.pathsGroupBox.Controls.Add(this.packagesPathLabel);
      this.pathsGroupBox.Location = new System.Drawing.Point(12, 12);
      this.pathsGroupBox.Name = "pathsGroupBox";
      this.pathsGroupBox.Size = new System.Drawing.Size(861, 106);
      this.pathsGroupBox.TabIndex = 0;
      this.pathsGroupBox.TabStop = false;
      this.pathsGroupBox.Text = "&Paths";
      // 
      // selectPackagesCachePathButton
      // 
      this.selectPackagesCachePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.selectPackagesCachePathButton.Image = ((System.Drawing.Image)(resources.GetObject("selectPackagesCachePathButton.Image")));
      this.selectPackagesCachePathButton.Location = new System.Drawing.Point(830, 73);
      this.selectPackagesCachePathButton.Name = "selectPackagesCachePathButton";
      this.selectPackagesCachePathButton.Size = new System.Drawing.Size(25, 25);
      this.selectPackagesCachePathButton.TabIndex = 7;
      this.selectPackagesCachePathButton.UseVisualStyleBackColor = true;
      this.selectPackagesCachePathButton.Click += new System.EventHandler(this.SelectFolderPathOnClick);
      // 
      // selectPackagesPathButton
      // 
      this.selectPackagesPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.selectPackagesPathButton.Image = ((System.Drawing.Image)(resources.GetObject("selectPackagesPathButton.Image")));
      this.selectPackagesPathButton.Location = new System.Drawing.Point(830, 44);
      this.selectPackagesPathButton.Name = "selectPackagesPathButton";
      this.selectPackagesPathButton.Size = new System.Drawing.Size(25, 25);
      this.selectPackagesPathButton.TabIndex = 4;
      this.selectPackagesPathButton.UseVisualStyleBackColor = true;
      this.selectPackagesPathButton.Click += new System.EventHandler(this.SelectFolderPathOnClick);
      // 
      // packagesCachePathTextBox
      // 
      this.packagesCachePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.packagesCachePathTextBox.Location = new System.Drawing.Point(143, 74);
      this.packagesCachePathTextBox.Name = "packagesCachePathTextBox";
      this.packagesCachePathTextBox.ReadOnly = true;
      this.packagesCachePathTextBox.Size = new System.Drawing.Size(681, 23);
      this.packagesCachePathTextBox.TabIndex = 6;
      // 
      // packagesPathTextBox
      // 
      this.packagesPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.packagesPathTextBox.Location = new System.Drawing.Point(143, 45);
      this.packagesPathTextBox.Name = "packagesPathTextBox";
      this.packagesPathTextBox.ReadOnly = true;
      this.packagesPathTextBox.Size = new System.Drawing.Size(681, 23);
      this.packagesPathTextBox.TabIndex = 3;
      // 
      // packagesCachePathLabel
      // 
      this.packagesCachePathLabel.AutoSize = true;
      this.packagesCachePathLabel.Location = new System.Drawing.Point(6, 77);
      this.packagesCachePathLabel.Name = "packagesCachePathLabel";
      this.packagesCachePathLabel.Size = new System.Drawing.Size(131, 15);
      this.packagesCachePathLabel.TabIndex = 5;
      this.packagesCachePathLabel.Text = "Packages &Cache Folder:";
      // 
      // appPathLabel
      // 
      this.appPathLabel.AutoSize = true;
      this.appPathLabel.Location = new System.Drawing.Point(6, 19);
      this.appPathLabel.Name = "appPathLabel";
      this.appPathLabel.Size = new System.Drawing.Size(98, 15);
      this.appPathLabel.TabIndex = 0;
      this.appPathLabel.Text = "Application Path:";
      // 
      // appPathTextBox
      // 
      this.appPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.appPathTextBox.Location = new System.Drawing.Point(143, 16);
      this.appPathTextBox.Name = "appPathTextBox";
      this.appPathTextBox.ReadOnly = true;
      this.appPathTextBox.Size = new System.Drawing.Size(681, 23);
      this.appPathTextBox.TabIndex = 1;
      // 
      // packagesPathLabel
      // 
      this.packagesPathLabel.AutoSize = true;
      this.packagesPathLabel.Location = new System.Drawing.Point(6, 48);
      this.packagesPathLabel.Name = "packagesPathLabel";
      this.packagesPathLabel.Size = new System.Drawing.Size(95, 15);
      this.packagesPathLabel.TabIndex = 2;
      this.packagesPathLabel.Text = "&Packages Folder:";
      // 
      // repositoriesGroupBox
      // 
      this.repositoriesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.repositoriesGroupBox.Controls.Add(this.editRepositoryButton);
      this.repositoriesGroupBox.Controls.Add(this.moveRepositoryDownButton);
      this.repositoriesGroupBox.Controls.Add(this.moveRepositoryUpButton);
      this.repositoriesGroupBox.Controls.Add(this.removeRepositoryButton);
      this.repositoriesGroupBox.Controls.Add(this.addRepositoryButton);
      this.repositoriesGroupBox.Controls.Add(this.repositoriesListBox);
      this.repositoriesGroupBox.Location = new System.Drawing.Point(12, 124);
      this.repositoriesGroupBox.Name = "repositoriesGroupBox";
      this.repositoriesGroupBox.Size = new System.Drawing.Size(861, 201);
      this.repositoriesGroupBox.TabIndex = 1;
      this.repositoriesGroupBox.TabStop = false;
      this.repositoriesGroupBox.Text = "&NuGet Package Sources";
      // 
      // editRepositoryButton
      // 
      this.editRepositoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.editRepositoryButton.Image = ((System.Drawing.Image)(resources.GetObject("editRepositoryButton.Image")));
      this.editRepositoryButton.Location = new System.Drawing.Point(830, 83);
      this.editRepositoryButton.Name = "editRepositoryButton";
      this.editRepositoryButton.Size = new System.Drawing.Size(25, 25);
      this.editRepositoryButton.TabIndex = 3;
      this.editRepositoryButton.UseVisualStyleBackColor = true;
      this.editRepositoryButton.Click += new System.EventHandler(this.EditRepositoryOnClick);
      // 
      // moveRepositoryDownButton
      // 
      this.moveRepositoryDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.moveRepositoryDownButton.Image = ((System.Drawing.Image)(resources.GetObject("moveRepositoryDownButton.Image")));
      this.moveRepositoryDownButton.Location = new System.Drawing.Point(830, 145);
      this.moveRepositoryDownButton.Name = "moveRepositoryDownButton";
      this.moveRepositoryDownButton.Size = new System.Drawing.Size(25, 25);
      this.moveRepositoryDownButton.TabIndex = 5;
      this.moveRepositoryDownButton.UseVisualStyleBackColor = true;
      this.moveRepositoryDownButton.Click += new System.EventHandler(this.MoveRepositoryDownOnClick);
      // 
      // moveRepositoryUpButton
      // 
      this.moveRepositoryUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.moveRepositoryUpButton.Image = ((System.Drawing.Image)(resources.GetObject("moveRepositoryUpButton.Image")));
      this.moveRepositoryUpButton.Location = new System.Drawing.Point(830, 114);
      this.moveRepositoryUpButton.Name = "moveRepositoryUpButton";
      this.moveRepositoryUpButton.Size = new System.Drawing.Size(25, 25);
      this.moveRepositoryUpButton.TabIndex = 4;
      this.moveRepositoryUpButton.UseVisualStyleBackColor = true;
      this.moveRepositoryUpButton.Click += new System.EventHandler(this.MoveRepositoryUpOnClick);
      // 
      // removeRepositoryButton
      // 
      this.removeRepositoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.removeRepositoryButton.Image = ((System.Drawing.Image)(resources.GetObject("removeRepositoryButton.Image")));
      this.removeRepositoryButton.Location = new System.Drawing.Point(830, 52);
      this.removeRepositoryButton.Name = "removeRepositoryButton";
      this.removeRepositoryButton.Size = new System.Drawing.Size(25, 25);
      this.removeRepositoryButton.TabIndex = 2;
      this.removeRepositoryButton.UseVisualStyleBackColor = true;
      this.removeRepositoryButton.Click += new System.EventHandler(this.RemoveRepositoryOnClick);
      // 
      // addRepositoryButton
      // 
      this.addRepositoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.addRepositoryButton.Image = ((System.Drawing.Image)(resources.GetObject("addRepositoryButton.Image")));
      this.addRepositoryButton.Location = new System.Drawing.Point(830, 21);
      this.addRepositoryButton.Name = "addRepositoryButton";
      this.addRepositoryButton.Size = new System.Drawing.Size(25, 25);
      this.addRepositoryButton.TabIndex = 1;
      this.addRepositoryButton.UseVisualStyleBackColor = true;
      this.addRepositoryButton.Click += new System.EventHandler(this.AddRepositoryOnClick);
      // 
      // repositoriesListBox
      // 
      this.repositoriesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.repositoriesListBox.FormattingEnabled = true;
      this.repositoriesListBox.ItemHeight = 15;
      this.repositoriesListBox.Location = new System.Drawing.Point(6, 22);
      this.repositoriesListBox.Name = "repositoriesListBox";
      this.repositoriesListBox.Size = new System.Drawing.Size(818, 169);
      this.repositoriesListBox.TabIndex = 0;
      this.repositoriesListBox.SelectedIndexChanged += new System.EventHandler(this.EnableDisableControlsOnSelectedIndexChanged);
      // 
      // isolationGroupBox
      // 
      this.isolationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.isolationGroupBox.Controls.Add(this.selectStarterAssemblyButton);
      this.isolationGroupBox.Controls.Add(this.useWindowsContainerLabel);
      this.isolationGroupBox.Controls.Add(this.dockerImageLabel);
      this.isolationGroupBox.Controls.Add(this.dotnetCommandLabel);
      this.isolationGroupBox.Controls.Add(this.starterAssemblyLabel);
      this.isolationGroupBox.Controls.Add(this.dockerCommandLabel);
      this.isolationGroupBox.Controls.Add(this.useWindowsContainerCheckBox);
      this.isolationGroupBox.Controls.Add(this.dockerImageTextBox);
      this.isolationGroupBox.Controls.Add(this.starterAssemblyTextBox);
      this.isolationGroupBox.Controls.Add(this.dockerCommandTextBox);
      this.isolationGroupBox.Controls.Add(this.dotnetCommandTextBox);
      this.isolationGroupBox.Controls.Add(this.isolationComboBox);
      this.isolationGroupBox.Controls.Add(this.isolationLabel);
      this.isolationGroupBox.Location = new System.Drawing.Point(12, 331);
      this.isolationGroupBox.Name = "isolationGroupBox";
      this.isolationGroupBox.Size = new System.Drawing.Size(861, 189);
      this.isolationGroupBox.TabIndex = 2;
      this.isolationGroupBox.TabStop = false;
      this.isolationGroupBox.Text = "&Isolation";
      // 
      // selectStarterAssemblyButton
      // 
      this.selectStarterAssemblyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.selectStarterAssemblyButton.Image = ((System.Drawing.Image)(resources.GetObject("selectStarterAssemblyButton.Image")));
      this.selectStarterAssemblyButton.Location = new System.Drawing.Point(830, 79);
      this.selectStarterAssemblyButton.Name = "selectStarterAssemblyButton";
      this.selectStarterAssemblyButton.Size = new System.Drawing.Size(25, 25);
      this.selectStarterAssemblyButton.TabIndex = 6;
      this.selectStarterAssemblyButton.UseVisualStyleBackColor = true;
      this.selectStarterAssemblyButton.Click += new System.EventHandler(this.SelectAssemblyPathOnClick);
      // 
      // useWindowsContainerLabel
      // 
      this.useWindowsContainerLabel.AutoSize = true;
      this.useWindowsContainerLabel.Location = new System.Drawing.Point(6, 166);
      this.useWindowsContainerLabel.Name = "useWindowsContainerLabel";
      this.useWindowsContainerLabel.Size = new System.Drawing.Size(114, 15);
      this.useWindowsContainerLabel.TabIndex = 11;
      this.useWindowsContainerLabel.Text = "&Windows Container:";
      // 
      // dockerImageLabel
      // 
      this.dockerImageLabel.AutoSize = true;
      this.dockerImageLabel.Location = new System.Drawing.Point(6, 141);
      this.dockerImageLabel.Name = "dockerImageLabel";
      this.dockerImageLabel.Size = new System.Drawing.Size(83, 15);
      this.dockerImageLabel.TabIndex = 9;
      this.dockerImageLabel.Text = "Docker &Image:";
      // 
      // dotnetCommandLabel
      // 
      this.dotnetCommandLabel.AutoSize = true;
      this.dotnetCommandLabel.Location = new System.Drawing.Point(6, 54);
      this.dotnetCommandLabel.Name = "dotnetCommandLabel";
      this.dotnetCommandLabel.Size = new System.Drawing.Size(106, 15);
      this.dotnetCommandLabel.TabIndex = 2;
      this.dotnetCommandLabel.Text = "Dot&net Command:";
      // 
      // starterAssemblyLabel
      // 
      this.starterAssemblyLabel.AutoSize = true;
      this.starterAssemblyLabel.Location = new System.Drawing.Point(6, 83);
      this.starterAssemblyLabel.Name = "starterAssemblyLabel";
      this.starterAssemblyLabel.Size = new System.Drawing.Size(98, 15);
      this.starterAssemblyLabel.TabIndex = 4;
      this.starterAssemblyLabel.Text = "&Starter Assembly:";
      // 
      // dockerCommandLabel
      // 
      this.dockerCommandLabel.AutoSize = true;
      this.dockerCommandLabel.Location = new System.Drawing.Point(6, 112);
      this.dockerCommandLabel.Name = "dockerCommandLabel";
      this.dockerCommandLabel.Size = new System.Drawing.Size(107, 15);
      this.dockerCommandLabel.TabIndex = 7;
      this.dockerCommandLabel.Text = "&Docker Command:";
      // 
      // useWindowsContainerCheckBox
      // 
      this.useWindowsContainerCheckBox.AutoSize = true;
      this.useWindowsContainerCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.useWindowsContainerCheckBox.Location = new System.Drawing.Point(143, 167);
      this.useWindowsContainerCheckBox.Name = "useWindowsContainerCheckBox";
      this.useWindowsContainerCheckBox.Size = new System.Drawing.Size(15, 14);
      this.useWindowsContainerCheckBox.TabIndex = 12;
      this.useWindowsContainerCheckBox.UseVisualStyleBackColor = true;
      // 
      // dockerImageTextBox
      // 
      this.dockerImageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.errorProvider.SetIconAlignment(this.dockerImageTextBox, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
      this.errorProvider.SetIconPadding(this.dockerImageTextBox, 3);
      this.dockerImageTextBox.Location = new System.Drawing.Point(143, 138);
      this.dockerImageTextBox.Name = "dockerImageTextBox";
      this.dockerImageTextBox.Size = new System.Drawing.Size(681, 23);
      this.dockerImageTextBox.TabIndex = 10;
      this.dockerImageTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxNotNullOrEmptyOrWhiteSpaceOnValidating);
      this.dockerImageTextBox.Validated += new System.EventHandler(this.RemoveErrorOnValidated);
      // 
      // starterAssemblyTextBox
      // 
      this.starterAssemblyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.starterAssemblyTextBox.Location = new System.Drawing.Point(143, 80);
      this.starterAssemblyTextBox.Name = "starterAssemblyTextBox";
      this.starterAssemblyTextBox.ReadOnly = true;
      this.starterAssemblyTextBox.Size = new System.Drawing.Size(681, 23);
      this.starterAssemblyTextBox.TabIndex = 5;
      // 
      // dockerCommandTextBox
      // 
      this.dockerCommandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.errorProvider.SetIconAlignment(this.dockerCommandTextBox, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
      this.errorProvider.SetIconPadding(this.dockerCommandTextBox, 3);
      this.dockerCommandTextBox.Location = new System.Drawing.Point(143, 109);
      this.dockerCommandTextBox.Name = "dockerCommandTextBox";
      this.dockerCommandTextBox.Size = new System.Drawing.Size(681, 23);
      this.dockerCommandTextBox.TabIndex = 8;
      this.dockerCommandTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxNotNullOrEmptyOrWhiteSpaceOnValidating);
      this.dockerCommandTextBox.Validated += new System.EventHandler(this.RemoveErrorOnValidated);
      // 
      // dotnetCommandTextBox
      // 
      this.dotnetCommandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.errorProvider.SetIconAlignment(this.dotnetCommandTextBox, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
      this.errorProvider.SetIconPadding(this.dotnetCommandTextBox, 3);
      this.dotnetCommandTextBox.Location = new System.Drawing.Point(143, 51);
      this.dotnetCommandTextBox.Name = "dotnetCommandTextBox";
      this.dotnetCommandTextBox.Size = new System.Drawing.Size(681, 23);
      this.dotnetCommandTextBox.TabIndex = 3;
      this.dotnetCommandTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxNotNullOrEmptyOrWhiteSpaceOnValidating);
      this.dotnetCommandTextBox.Validated += new System.EventHandler(this.RemoveErrorOnValidated);
      // 
      // isolationComboBox
      // 
      this.isolationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.isolationComboBox.FormattingEnabled = true;
      this.isolationComboBox.Location = new System.Drawing.Point(143, 22);
      this.isolationComboBox.Name = "isolationComboBox";
      this.isolationComboBox.Size = new System.Drawing.Size(176, 23);
      this.isolationComboBox.TabIndex = 1;
      // 
      // isolationLabel
      // 
      this.isolationLabel.AutoSize = true;
      this.isolationLabel.Location = new System.Drawing.Point(6, 25);
      this.isolationLabel.Name = "isolationLabel";
      this.isolationLabel.Size = new System.Drawing.Size(89, 15);
      this.isolationLabel.TabIndex = 0;
      this.isolationLabel.Text = "&Isolation Mode:";
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // openFileDialog
      // 
      this.openFileDialog.DefaultExt = "dll";
      this.openFileDialog.FileName = "assembly";
      this.openFileDialog.Filter = "Assemblies (*.dll;*.exe)|*.dll;*.exe|All files (*.*)|*.*";
      this.openFileDialog.Title = "Select Starter Assembly";
      // 
      // SettingsForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(885, 561);
      this.Controls.Add(this.isolationGroupBox);
      this.Controls.Add(this.repositoriesGroupBox);
      this.Controls.Add(this.pathsGroupBox);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SettingsForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Settings";
      this.pathsGroupBox.ResumeLayout(false);
      this.pathsGroupBox.PerformLayout();
      this.repositoriesGroupBox.ResumeLayout(false);
      this.isolationGroupBox.ResumeLayout(false);
      this.isolationGroupBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.GroupBox pathsGroupBox;
    private System.Windows.Forms.Button selectPackagesCachePathButton;
    private System.Windows.Forms.Button selectPackagesPathButton;
    private System.Windows.Forms.TextBox packagesCachePathTextBox;
    private System.Windows.Forms.TextBox packagesPathTextBox;
    private System.Windows.Forms.Label packagesCachePathLabel;
    private System.Windows.Forms.Label appPathLabel;
    private System.Windows.Forms.TextBox appPathTextBox;
    private System.Windows.Forms.Label packagesPathLabel;
    private System.Windows.Forms.GroupBox repositoriesGroupBox;
    private System.Windows.Forms.GroupBox isolationGroupBox;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private System.Windows.Forms.Button moveRepositoryDownButton;
    private System.Windows.Forms.Button moveRepositoryUpButton;
    private System.Windows.Forms.Button removeRepositoryButton;
    private System.Windows.Forms.Button addRepositoryButton;
    private System.Windows.Forms.ListBox repositoriesListBox;
    private System.Windows.Forms.Label dockerImageLabel;
    private System.Windows.Forms.Label dotnetCommandLabel;
    private System.Windows.Forms.Label starterAssemblyLabel;
    private System.Windows.Forms.Label dockerCommandLabel;
    private System.Windows.Forms.CheckBox useWindowsContainerCheckBox;
    private System.Windows.Forms.TextBox dockerImageTextBox;
    private System.Windows.Forms.TextBox starterAssemblyTextBox;
    private System.Windows.Forms.TextBox dockerCommandTextBox;
    private System.Windows.Forms.TextBox dotnetCommandTextBox;
    private System.Windows.Forms.ComboBox isolationComboBox;
    private System.Windows.Forms.Label isolationLabel;
    private System.Windows.Forms.Button selectStarterAssemblyButton;
    private System.Windows.Forms.Label useWindowsContainerLabel;
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.Button editRepositoryButton;
  }
}