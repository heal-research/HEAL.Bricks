#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks.UI.WindowsForms {
  partial class StarterForm {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StarterForm));
      this.settingsButton = new System.Windows.Forms.Button();
      this.packageManagerButton = new System.Windows.Forms.Button();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.startStopButton = new System.Windows.Forms.Button();
      this.smallImageList = new System.Windows.Forms.ImageList(this.components);
      this.reloadButton = new System.Windows.Forms.Button();
      this.applicationsListView = new System.Windows.Forms.ListView();
      this.largeImageList = new System.Windows.Forms.ImageList(this.components);
      this.applicationsLabel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // settingsButton
      // 
      this.settingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.settingsButton.Image = ((System.Drawing.Image)(resources.GetObject("settingsButton.Image")));
      this.settingsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.settingsButton.Location = new System.Drawing.Point(12, 395);
      this.settingsButton.Name = "settingsButton";
      this.settingsButton.Size = new System.Drawing.Size(85, 23);
      this.settingsButton.TabIndex = 2;
      this.settingsButton.Text = "Se&ttings...";
      this.settingsButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip.SetToolTip(this.settingsButton, "Open settings dialog");
      this.settingsButton.UseVisualStyleBackColor = true;
      this.settingsButton.Click += new System.EventHandler(this.ShowOptionsDialogOnClick);
      // 
      // packageManagerButton
      // 
      this.packageManagerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.packageManagerButton.Image = ((System.Drawing.Image)(resources.GetObject("packageManagerButton.Image")));
      this.packageManagerButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.packageManagerButton.Location = new System.Drawing.Point(103, 395);
      this.packageManagerButton.Name = "packageManagerButton";
      this.packageManagerButton.Size = new System.Drawing.Size(137, 23);
      this.packageManagerButton.TabIndex = 3;
      this.packageManagerButton.Text = "&Package Manager...";
      this.packageManagerButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip.SetToolTip(this.packageManagerButton, "Open package manager");
      this.packageManagerButton.UseVisualStyleBackColor = true;
      this.packageManagerButton.Click += new System.EventHandler(this.ShowPackageManagerOnClick);
      // 
      // startStopButton
      // 
      this.startStopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.startStopButton.Enabled = false;
      this.startStopButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.startStopButton.ImageKey = "Play";
      this.startStopButton.ImageList = this.smallImageList;
      this.startStopButton.Location = new System.Drawing.Point(616, 395);
      this.startStopButton.Name = "startStopButton";
      this.startStopButton.Size = new System.Drawing.Size(58, 23);
      this.startStopButton.TabIndex = 5;
      this.startStopButton.Text = "&Start";
      this.startStopButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip.SetToolTip(this.startStopButton, "Start application");
      this.startStopButton.UseVisualStyleBackColor = true;
      this.startStopButton.Click += new System.EventHandler(this.StartStopApplicationOnClick);
      // 
      // smallImageList
      // 
      this.smallImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
      this.smallImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("smallImageList.ImageStream")));
      this.smallImageList.TransparentColor = System.Drawing.Color.Transparent;
      this.smallImageList.Images.SetKeyName(0, "Package");
      this.smallImageList.Images.SetKeyName(1, "Play");
      this.smallImageList.Images.SetKeyName(2, "Pause");
      this.smallImageList.Images.SetKeyName(3, "Stop");
      // 
      // reloadButton
      // 
      this.reloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.reloadButton.Image = ((System.Drawing.Image)(resources.GetObject("reloadButton.Image")));
      this.reloadButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.reloadButton.Location = new System.Drawing.Point(539, 395);
      this.reloadButton.Name = "reloadButton";
      this.reloadButton.Size = new System.Drawing.Size(71, 23);
      this.reloadButton.TabIndex = 4;
      this.reloadButton.Text = "&Reload";
      this.reloadButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip.SetToolTip(this.reloadButton, "Reload applications");
      this.reloadButton.UseVisualStyleBackColor = true;
      this.reloadButton.Click += new System.EventHandler(this.LoadApplicationsOnClick);
      // 
      // applicationsListView
      // 
      this.applicationsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.applicationsListView.HideSelection = false;
      this.applicationsListView.LargeImageList = this.largeImageList;
      this.applicationsListView.Location = new System.Drawing.Point(12, 27);
      this.applicationsListView.MultiSelect = false;
      this.applicationsListView.Name = "applicationsListView";
      this.applicationsListView.ShowItemToolTips = true;
      this.applicationsListView.Size = new System.Drawing.Size(662, 362);
      this.applicationsListView.TabIndex = 1;
      this.applicationsListView.UseCompatibleStateImageBehavior = false;
      this.applicationsListView.ItemActivate += new System.EventHandler(this.StartApplicationOnItemActivate);
      this.applicationsListView.SelectedIndexChanged += new System.EventHandler(this.EnableDisableControlsOnSelectedIndexChanged);
      // 
      // largeImageList
      // 
      this.largeImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
      this.largeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("largeImageList.ImageStream")));
      this.largeImageList.TransparentColor = System.Drawing.Color.Transparent;
      this.largeImageList.Images.SetKeyName(0, "Package");
      this.largeImageList.Images.SetKeyName(1, "Play");
      this.largeImageList.Images.SetKeyName(2, "Pause");
      this.largeImageList.Images.SetKeyName(3, "Stop");
      // 
      // applicationsLabel
      // 
      this.applicationsLabel.AutoSize = true;
      this.applicationsLabel.Location = new System.Drawing.Point(12, 9);
      this.applicationsLabel.Name = "applicationsLabel";
      this.applicationsLabel.Size = new System.Drawing.Size(76, 15);
      this.applicationsLabel.TabIndex = 0;
      this.applicationsLabel.Text = "&Applications:";
      // 
      // StarterForm
      // 
      this.AcceptButton = this.startStopButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(686, 430);
      this.Controls.Add(this.applicationsLabel);
      this.Controls.Add(this.reloadButton);
      this.Controls.Add(this.startStopButton);
      this.Controls.Add(this.applicationsListView);
      this.Controls.Add(this.packageManagerButton);
      this.Controls.Add(this.settingsButton);
      this.Name = "StarterForm";
      this.Text = "Application Selector";
      this.Load += new System.EventHandler(this.LoadApplicationsOnLoad);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.Button settingsButton;
    private System.Windows.Forms.Button packageManagerButton;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.ListView applicationsListView;
    private System.Windows.Forms.Button startStopButton;
    private System.Windows.Forms.Button reloadButton;
    private System.Windows.Forms.ImageList smallImageList;
    private System.Windows.Forms.Label applicationsLabel;
    private System.Windows.Forms.ImageList largeImageList;
  }
}

