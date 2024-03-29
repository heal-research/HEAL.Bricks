﻿#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks.UI.WindowsForms {
  partial class EditRepositoryDialog {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditRepositoryDialog));
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.repositoryLabel = new System.Windows.Forms.Label();
      this.repositoryTextBox = new System.Windows.Forms.TextBox();
      this.usernameTextBox = new System.Windows.Forms.TextBox();
      this.passwordTextBox = new System.Windows.Forms.TextBox();
      this.usernameLabel = new System.Windows.Forms.Label();
      this.passwordLabel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Image = ((System.Drawing.Image)(resources.GetObject("okButton.Image")));
      this.okButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.okButton.Location = new System.Drawing.Point(476, 104);
      this.okButton.Name = "okButton";
      this.okButton.Padding = new System.Windows.Forms.Padding(0, 0, 12, 0);
      this.okButton.Size = new System.Drawing.Size(66, 23);
      this.okButton.TabIndex = 6;
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
      this.cancelButton.Location = new System.Drawing.Point(548, 104);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(66, 23);
      this.cancelButton.TabIndex = 7;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // repositoryLabel
      // 
      this.repositoryLabel.AutoSize = true;
      this.repositoryLabel.Location = new System.Drawing.Point(12, 15);
      this.repositoryLabel.Name = "repositoryLabel";
      this.repositoryLabel.Size = new System.Drawing.Size(93, 15);
      this.repositoryLabel.TabIndex = 0;
      this.repositoryLabel.Text = "Package &Source:";
      // 
      // repositoryTextBox
      // 
      this.repositoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.repositoryTextBox.Location = new System.Drawing.Point(126, 12);
      this.repositoryTextBox.Name = "repositoryTextBox";
      this.repositoryTextBox.Size = new System.Drawing.Size(488, 23);
      this.repositoryTextBox.TabIndex = 1;
      // 
      // usernameTextBox
      // 
      this.usernameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.usernameTextBox.Location = new System.Drawing.Point(126, 41);
      this.usernameTextBox.Name = "usernameTextBox";
      this.usernameTextBox.Size = new System.Drawing.Size(488, 23);
      this.usernameTextBox.TabIndex = 3;
      // 
      // passwordTextBox
      // 
      this.passwordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.passwordTextBox.Location = new System.Drawing.Point(126, 70);
      this.passwordTextBox.Name = "passwordTextBox";
      this.passwordTextBox.PasswordChar = '*';
      this.passwordTextBox.Size = new System.Drawing.Size(488, 23);
      this.passwordTextBox.TabIndex = 5;
      // 
      // usernameLabel
      // 
      this.usernameLabel.AutoSize = true;
      this.usernameLabel.Location = new System.Drawing.Point(12, 44);
      this.usernameLabel.Name = "usernameLabel";
      this.usernameLabel.Size = new System.Drawing.Size(63, 15);
      this.usernameLabel.TabIndex = 2;
      this.usernameLabel.Text = "&Username:";
      // 
      // passwordLabel
      // 
      this.passwordLabel.AutoSize = true;
      this.passwordLabel.Location = new System.Drawing.Point(12, 73);
      this.passwordLabel.Name = "passwordLabel";
      this.passwordLabel.Size = new System.Drawing.Size(60, 15);
      this.passwordLabel.TabIndex = 4;
      this.passwordLabel.Text = "&Password:";
      // 
      // EditRepositoryDialog
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(626, 139);
      this.Controls.Add(this.passwordLabel);
      this.Controls.Add(this.usernameLabel);
      this.Controls.Add(this.passwordTextBox);
      this.Controls.Add(this.usernameTextBox);
      this.Controls.Add(this.repositoryTextBox);
      this.Controls.Add(this.repositoryLabel);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "EditRepositoryDialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Add Package Source";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Label repositoryLabel;
    private System.Windows.Forms.TextBox repositoryTextBox;
    private System.Windows.Forms.TextBox usernameTextBox;
    private System.Windows.Forms.TextBox passwordTextBox;
    private System.Windows.Forms.Label usernameLabel;
    private System.Windows.Forms.Label passwordLabel;
  }
}