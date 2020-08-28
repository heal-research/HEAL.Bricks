namespace WinFormsTestApp {
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
      this.startButton = new System.Windows.Forms.Button();
      this.appsListView = new System.Windows.Forms.ListView();
      this.SuspendLayout();
      // 
      // startButton
      // 
      this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.startButton.Location = new System.Drawing.Point(713, 415);
      this.startButton.Name = "startButton";
      this.startButton.Size = new System.Drawing.Size(75, 23);
      this.startButton.TabIndex = 0;
      this.startButton.Text = "&Start";
      this.startButton.UseVisualStyleBackColor = true;
      this.startButton.Click += new System.EventHandler(this.startButton_Click);
      // 
      // appsListView
      // 
      this.appsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.appsListView.HideSelection = false;
      this.appsListView.Location = new System.Drawing.Point(12, 12);
      this.appsListView.MultiSelect = false;
      this.appsListView.Name = "appsListView";
      this.appsListView.Size = new System.Drawing.Size(776, 397);
      this.appsListView.TabIndex = 1;
      this.appsListView.UseCompatibleStateImageBehavior = false;
      // 
      // StarterForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.appsListView);
      this.Controls.Add(this.startButton);
      this.Name = "StarterForm";
      this.Text = "Starter";
      this.Load += new System.EventHandler(this.StarterForm_Load);
      this.ResumeLayout(false);

    }

        #endregion

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.ListView appsListView;
    }
}

