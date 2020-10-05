#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks.UI.WindowsForms {
  partial class PackageManagerForm {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackageManagerForm));
      this.tabControl = new System.Windows.Forms.TabControl();
      this.browseTabPage = new System.Windows.Forms.TabPage();
      this.showMoreButton = new System.Windows.Forms.Button();
      this.installButton = new System.Windows.Forms.Button();
      this.refreshSearchButton = new System.Windows.Forms.Button();
      this.includePreReleases = new System.Windows.Forms.CheckBox();
      this.searchLabel = new System.Windows.Forms.Label();
      this.searchTextBox = new System.Windows.Forms.TextBox();
      this.browsePackagesListView = new System.Windows.Forms.ListView();
      this.browsePackagesNameColumn = new System.Windows.Forms.ColumnHeader();
      this.browsePackagesVersionColumn = new System.Windows.Forms.ColumnHeader();
      this.browsePackagesRepositoryColumn = new System.Windows.Forms.ColumnHeader();
      this.installedTabPage = new System.Windows.Forms.TabPage();
      this.removeButton = new System.Windows.Forms.Button();
      this.installedPackagesListView = new System.Windows.Forms.ListView();
      this.installedPackagesNameColumn = new System.Windows.Forms.ColumnHeader();
      this.installedPackagesVersionColumn = new System.Windows.Forms.ColumnHeader();
      this.installedPackagesStatusColumn = new System.Windows.Forms.ColumnHeader();
      this.updatesTabPage = new System.Windows.Forms.TabPage();
      this.updateButton = new System.Windows.Forms.Button();
      this.updatePackagesListView = new System.Windows.Forms.ListView();
      this.updatePackagesNameColumn = new System.Windows.Forms.ColumnHeader();
      this.updatePackagesInstalledVersionColumn = new System.Windows.Forms.ColumnHeader();
      this.updatePackagesNewVersionColumn = new System.Windows.Forms.ColumnHeader();
      this.splitContainer = new System.Windows.Forms.SplitContainer();
      this.detailsTextBox = new System.Windows.Forms.RichTextBox();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.tabControl.SuspendLayout();
      this.browseTabPage.SuspendLayout();
      this.installedTabPage.SuspendLayout();
      this.updatesTabPage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
      this.splitContainer.Panel1.SuspendLayout();
      this.splitContainer.Panel2.SuspendLayout();
      this.splitContainer.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl
      // 
      this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl.Controls.Add(this.browseTabPage);
      this.tabControl.Controls.Add(this.installedTabPage);
      this.tabControl.Controls.Add(this.updatesTabPage);
      this.tabControl.Location = new System.Drawing.Point(3, 3);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 1;
      this.tabControl.Size = new System.Drawing.Size(494, 420);
      this.tabControl.TabIndex = 0;
      this.tabControl.SelectedIndexChanged += new System.EventHandler(this.UpdateControlsOnSelectedIndexChanged);
      // 
      // browseTabPage
      // 
      this.browseTabPage.Controls.Add(this.showMoreButton);
      this.browseTabPage.Controls.Add(this.installButton);
      this.browseTabPage.Controls.Add(this.refreshSearchButton);
      this.browseTabPage.Controls.Add(this.includePreReleases);
      this.browseTabPage.Controls.Add(this.searchLabel);
      this.browseTabPage.Controls.Add(this.searchTextBox);
      this.browseTabPage.Controls.Add(this.browsePackagesListView);
      this.browseTabPage.Location = new System.Drawing.Point(4, 24);
      this.browseTabPage.Name = "browseTabPage";
      this.browseTabPage.Padding = new System.Windows.Forms.Padding(3);
      this.browseTabPage.Size = new System.Drawing.Size(486, 392);
      this.browseTabPage.TabIndex = 1;
      this.browseTabPage.Text = "Browse";
      this.browseTabPage.UseVisualStyleBackColor = true;
      // 
      // showMoreButton
      // 
      this.showMoreButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.showMoreButton.Image = ((System.Drawing.Image)(resources.GetObject("showMoreButton.Image")));
      this.showMoreButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.showMoreButton.Location = new System.Drawing.Point(390, 366);
      this.showMoreButton.Name = "showMoreButton";
      this.showMoreButton.Size = new System.Drawing.Size(93, 23);
      this.showMoreButton.TabIndex = 6;
      this.showMoreButton.Text = "Show &More";
      this.showMoreButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.showMoreButton.UseVisualStyleBackColor = true;
      this.showMoreButton.Click += new System.EventHandler(this.LoadMoreRemotePackages);
      // 
      // installButton
      // 
      this.installButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.installButton.Image = ((System.Drawing.Image)(resources.GetObject("installButton.Image")));
      this.installButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.installButton.Location = new System.Drawing.Point(3, 366);
      this.installButton.Name = "installButton";
      this.installButton.Padding = new System.Windows.Forms.Padding(0, 0, 12, 0);
      this.installButton.Size = new System.Drawing.Size(85, 23);
      this.installButton.TabIndex = 5;
      this.installButton.Text = "&Install";
      this.installButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.installButton.UseVisualStyleBackColor = true;
      this.installButton.Click += new System.EventHandler(this.InstallPackagesOnClick);
      // 
      // refreshSearchButton
      // 
      this.refreshSearchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.refreshSearchButton.Image = ((System.Drawing.Image)(resources.GetObject("refreshSearchButton.Image")));
      this.refreshSearchButton.Location = new System.Drawing.Point(328, 3);
      this.refreshSearchButton.Name = "refreshSearchButton";
      this.refreshSearchButton.Size = new System.Drawing.Size(23, 23);
      this.refreshSearchButton.TabIndex = 2;
      this.toolTip.SetToolTip(this.refreshSearchButton, "Refresh search results");
      this.refreshSearchButton.UseVisualStyleBackColor = true;
      this.refreshSearchButton.Click += new System.EventHandler(this.ReloadRemotePackages);
      // 
      // includePreReleases
      // 
      this.includePreReleases.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.includePreReleases.AutoSize = true;
      this.includePreReleases.Location = new System.Drawing.Point(357, 5);
      this.includePreReleases.Name = "includePreReleases";
      this.includePreReleases.Size = new System.Drawing.Size(126, 19);
      this.includePreReleases.TabIndex = 3;
      this.includePreReleases.Text = "Include &Prereleases";
      this.toolTip.SetToolTip(this.includePreReleases, "Include prerelease versions in search results");
      this.includePreReleases.UseVisualStyleBackColor = true;
      // 
      // searchLabel
      // 
      this.searchLabel.Image = ((System.Drawing.Image)(resources.GetObject("searchLabel.Image")));
      this.searchLabel.Location = new System.Drawing.Point(3, 3);
      this.searchLabel.Name = "searchLabel";
      this.searchLabel.Size = new System.Drawing.Size(23, 23);
      this.searchLabel.TabIndex = 0;
      // 
      // searchTextBox
      // 
      this.searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.searchTextBox.Location = new System.Drawing.Point(32, 3);
      this.searchTextBox.Name = "searchTextBox";
      this.searchTextBox.Size = new System.Drawing.Size(290, 23);
      this.searchTextBox.TabIndex = 1;
      this.toolTip.SetToolTip(this.searchTextBox, "Search available packages");
      this.searchTextBox.TextChanged += new System.EventHandler(this.ReloadRemotePackages);
      // 
      // browsePackagesListView
      // 
      this.browsePackagesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.browsePackagesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.browsePackagesNameColumn,
            this.browsePackagesVersionColumn,
            this.browsePackagesRepositoryColumn});
      this.browsePackagesListView.FullRowSelect = true;
      this.browsePackagesListView.HideSelection = false;
      this.browsePackagesListView.Location = new System.Drawing.Point(3, 32);
      this.browsePackagesListView.Name = "browsePackagesListView";
      this.browsePackagesListView.ShowItemToolTips = true;
      this.browsePackagesListView.Size = new System.Drawing.Size(480, 328);
      this.browsePackagesListView.TabIndex = 4;
      this.browsePackagesListView.UseCompatibleStateImageBehavior = false;
      this.browsePackagesListView.View = System.Windows.Forms.View.Details;
      this.browsePackagesListView.SelectedIndexChanged += new System.EventHandler(this.UpdateControlsOnSelectedIndexChanged);
      // 
      // browsePackagesNameColumn
      // 
      this.browsePackagesNameColumn.Text = "Name";
      this.browsePackagesNameColumn.Width = 180;
      // 
      // browsePackagesVersionColumn
      // 
      this.browsePackagesVersionColumn.Text = "Version";
      this.browsePackagesVersionColumn.Width = 80;
      // 
      // browsePackagesRepositoryColumn
      // 
      this.browsePackagesRepositoryColumn.Text = "Repository";
      this.browsePackagesRepositoryColumn.Width = 250;
      // 
      // installedTabPage
      // 
      this.installedTabPage.Controls.Add(this.removeButton);
      this.installedTabPage.Controls.Add(this.installedPackagesListView);
      this.installedTabPage.Location = new System.Drawing.Point(4, 24);
      this.installedTabPage.Name = "installedTabPage";
      this.installedTabPage.Padding = new System.Windows.Forms.Padding(3);
      this.installedTabPage.Size = new System.Drawing.Size(486, 392);
      this.installedTabPage.TabIndex = 0;
      this.installedTabPage.Text = "Installed";
      this.installedTabPage.UseVisualStyleBackColor = true;
      // 
      // removeButton
      // 
      this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.removeButton.Image = ((System.Drawing.Image)(resources.GetObject("removeButton.Image")));
      this.removeButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.removeButton.Location = new System.Drawing.Point(3, 366);
      this.removeButton.Name = "removeButton";
      this.removeButton.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
      this.removeButton.Size = new System.Drawing.Size(85, 23);
      this.removeButton.TabIndex = 1;
      this.removeButton.Text = "&Remove";
      this.removeButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.removeButton.UseVisualStyleBackColor = true;
      this.removeButton.Click += new System.EventHandler(this.RemovePackagesOnClick);
      // 
      // installedPackagesListView
      // 
      this.installedPackagesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.installedPackagesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.installedPackagesNameColumn,
            this.installedPackagesVersionColumn,
            this.installedPackagesStatusColumn});
      this.installedPackagesListView.FullRowSelect = true;
      this.installedPackagesListView.HideSelection = false;
      this.installedPackagesListView.Location = new System.Drawing.Point(3, 3);
      this.installedPackagesListView.Name = "installedPackagesListView";
      this.installedPackagesListView.ShowItemToolTips = true;
      this.installedPackagesListView.Size = new System.Drawing.Size(480, 357);
      this.installedPackagesListView.TabIndex = 0;
      this.installedPackagesListView.UseCompatibleStateImageBehavior = false;
      this.installedPackagesListView.View = System.Windows.Forms.View.Details;
      this.installedPackagesListView.SelectedIndexChanged += new System.EventHandler(this.UpdateControlsOnSelectedIndexChanged);
      // 
      // installedPackagesNameColumn
      // 
      this.installedPackagesNameColumn.Text = "Name";
      this.installedPackagesNameColumn.Width = 180;
      // 
      // installedPackagesVersionColumn
      // 
      this.installedPackagesVersionColumn.Text = "Version";
      this.installedPackagesVersionColumn.Width = 80;
      // 
      // installedPackagesStatusColumn
      // 
      this.installedPackagesStatusColumn.Text = "Status";
      this.installedPackagesStatusColumn.Width = 120;
      // 
      // updatesTabPage
      // 
      this.updatesTabPage.Controls.Add(this.updateButton);
      this.updatesTabPage.Controls.Add(this.updatePackagesListView);
      this.updatesTabPage.Location = new System.Drawing.Point(4, 24);
      this.updatesTabPage.Name = "updatesTabPage";
      this.updatesTabPage.Padding = new System.Windows.Forms.Padding(3);
      this.updatesTabPage.Size = new System.Drawing.Size(486, 392);
      this.updatesTabPage.TabIndex = 2;
      this.updatesTabPage.Text = "Updates";
      this.updatesTabPage.UseVisualStyleBackColor = true;
      // 
      // updateButton
      // 
      this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.updateButton.Image = ((System.Drawing.Image)(resources.GetObject("updateButton.Image")));
      this.updateButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.updateButton.Location = new System.Drawing.Point(3, 366);
      this.updateButton.Name = "updateButton";
      this.updateButton.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
      this.updateButton.Size = new System.Drawing.Size(85, 23);
      this.updateButton.TabIndex = 2;
      this.updateButton.Text = "&Update";
      this.updateButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.updateButton.UseVisualStyleBackColor = true;
      this.updateButton.Click += new System.EventHandler(this.UpdatePackagesOnClick);
      // 
      // updatePackagesListView
      // 
      this.updatePackagesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.updatePackagesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.updatePackagesNameColumn,
            this.updatePackagesInstalledVersionColumn,
            this.updatePackagesNewVersionColumn});
      this.updatePackagesListView.FullRowSelect = true;
      this.updatePackagesListView.HideSelection = false;
      this.updatePackagesListView.Location = new System.Drawing.Point(3, 3);
      this.updatePackagesListView.Name = "updatePackagesListView";
      this.updatePackagesListView.ShowItemToolTips = true;
      this.updatePackagesListView.Size = new System.Drawing.Size(480, 357);
      this.updatePackagesListView.TabIndex = 0;
      this.updatePackagesListView.UseCompatibleStateImageBehavior = false;
      this.updatePackagesListView.View = System.Windows.Forms.View.Details;
      this.updatePackagesListView.SelectedIndexChanged += new System.EventHandler(this.UpdateControlsOnSelectedIndexChanged);
      // 
      // updatePackagesNameColumn
      // 
      this.updatePackagesNameColumn.Text = "Name";
      this.updatePackagesNameColumn.Width = 180;
      // 
      // updatePackagesInstalledVersionColumn
      // 
      this.updatePackagesInstalledVersionColumn.Text = "Installed";
      this.updatePackagesInstalledVersionColumn.Width = 80;
      // 
      // updatePackagesNewVersionColumn
      // 
      this.updatePackagesNewVersionColumn.Text = "New";
      this.updatePackagesNewVersionColumn.Width = 80;
      // 
      // splitContainer
      // 
      this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.splitContainer.Location = new System.Drawing.Point(12, 12);
      this.splitContainer.Name = "splitContainer";
      // 
      // splitContainer.Panel1
      // 
      this.splitContainer.Panel1.Controls.Add(this.tabControl);
      // 
      // splitContainer.Panel2
      // 
      this.splitContainer.Panel2.Controls.Add(this.detailsTextBox);
      this.splitContainer.Size = new System.Drawing.Size(776, 426);
      this.splitContainer.SplitterDistance = 500;
      this.splitContainer.TabIndex = 0;
      this.splitContainer.Text = "splitContainer1";
      // 
      // detailsTextBox
      // 
      this.detailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.detailsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.detailsTextBox.Cursor = System.Windows.Forms.Cursors.Default;
      this.detailsTextBox.Location = new System.Drawing.Point(3, 27);
      this.detailsTextBox.Name = "detailsTextBox";
      this.detailsTextBox.ReadOnly = true;
      this.detailsTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.detailsTextBox.Size = new System.Drawing.Size(266, 396);
      this.detailsTextBox.TabIndex = 0;
      this.detailsTextBox.Text = "";
      // 
      // PackageManagerForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.splitContainer);
      this.Name = "PackageManagerForm";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Package Manager";
      this.Load += new System.EventHandler(this.PrepareForm);
      this.tabControl.ResumeLayout(false);
      this.browseTabPage.ResumeLayout(false);
      this.browseTabPage.PerformLayout();
      this.installedTabPage.ResumeLayout(false);
      this.updatesTabPage.ResumeLayout(false);
      this.splitContainer.Panel1.ResumeLayout(false);
      this.splitContainer.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
      this.splitContainer.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TabPage installedTabPage;
    private System.Windows.Forms.TabPage browseTabPage;
    private System.Windows.Forms.SplitContainer splitContainer;
    private System.Windows.Forms.ListView installedPackagesListView;
    private System.Windows.Forms.TextBox searchTextBox;
    private System.Windows.Forms.ListView browsePackagesListView;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.Label searchLabel;
    private System.Windows.Forms.Button installButton;
    private System.Windows.Forms.Button updateButton;
    private System.Windows.Forms.Button removeButton;
    private System.Windows.Forms.Button refreshSearchButton;
    private System.Windows.Forms.CheckBox includePreReleases;
    private System.Windows.Forms.RichTextBox detailsTextBox;
    private System.Windows.Forms.Button showMoreButton;
    private System.Windows.Forms.ColumnHeader installedPackagesNameColumn;
    private System.Windows.Forms.ColumnHeader installedPackagesVersionColumn;
    private System.Windows.Forms.ColumnHeader installedPackagesStatusColumn;
    private System.Windows.Forms.ColumnHeader browsePackagesNameColumn;
    private System.Windows.Forms.ColumnHeader browsePackagesVersionColumn;
    private System.Windows.Forms.ColumnHeader browsePackagesRepositoryColumn;
    private System.Windows.Forms.TabPage updatesTabPage;
    private System.Windows.Forms.ListView updatePackagesListView;
    private System.Windows.Forms.ColumnHeader updatePackagesNameColumn;
    private System.Windows.Forms.ColumnHeader updatePackagesInstalledVersionColumn;
    private System.Windows.Forms.ColumnHeader updatePackagesNewVersionColumn;
  }
}