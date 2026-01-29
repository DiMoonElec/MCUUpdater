namespace MCUUpdaterGUI
{
  partial class MainForm
  {
    /// <summary>
    /// Обязательная переменная конструктора.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Освободить все используемые ресурсы.
    /// </summary>
    /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Код, автоматически созданный конструктором форм Windows

    /// <summary>
    /// Требуемый метод для поддержки конструктора — не изменяйте 
    /// содержимое этого метода с помощью редактора кода.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupboxConnection = new System.Windows.Forms.GroupBox();
      this.buttonAbort = new System.Windows.Forms.Button();
      this.buttonStartUpdate = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.buttonSelectFile = new System.Windows.Forms.Button();
      this.textBoxSelectedFile = new System.Windows.Forms.TextBox();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.labelProgressBar = new System.Windows.Forms.ToolStripStatusLabel();
      this.progressBarProgress = new System.Windows.Forms.ToolStripProgressBar();
      this.logRichTextBox = new MCUUpdaterGUI.UserControls.LogRichTextBox();
      this.transportUI1 = new MCUUpdaterGUI.TransportUI();
      this.groupboxConnection.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupboxConnection
      // 
      this.groupboxConnection.Controls.Add(this.buttonAbort);
      this.groupboxConnection.Controls.Add(this.buttonStartUpdate);
      this.groupboxConnection.Controls.Add(this.transportUI1);
      this.groupboxConnection.Location = new System.Drawing.Point(16, 87);
      this.groupboxConnection.Margin = new System.Windows.Forms.Padding(4);
      this.groupboxConnection.Name = "groupboxConnection";
      this.groupboxConnection.Padding = new System.Windows.Forms.Padding(4);
      this.groupboxConnection.Size = new System.Drawing.Size(697, 268);
      this.groupboxConnection.TabIndex = 2;
      this.groupboxConnection.TabStop = false;
      this.groupboxConnection.Text = "Connection";
      // 
      // buttonAbort
      // 
      this.buttonAbort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
      this.buttonAbort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.buttonAbort.Location = new System.Drawing.Point(516, 101);
      this.buttonAbort.Margin = new System.Windows.Forms.Padding(4);
      this.buttonAbort.Name = "buttonAbort";
      this.buttonAbort.Size = new System.Drawing.Size(173, 74);
      this.buttonAbort.TabIndex = 5;
      this.buttonAbort.Text = "ABORT";
      this.buttonAbort.UseVisualStyleBackColor = false;
      this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
      // 
      // buttonStartUpdate
      // 
      this.buttonStartUpdate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
      this.buttonStartUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.buttonStartUpdate.Location = new System.Drawing.Point(516, 20);
      this.buttonStartUpdate.Margin = new System.Windows.Forms.Padding(4);
      this.buttonStartUpdate.Name = "buttonStartUpdate";
      this.buttonStartUpdate.Size = new System.Drawing.Size(173, 74);
      this.buttonStartUpdate.TabIndex = 4;
      this.buttonStartUpdate.Text = "START\r\nUPDATE";
      this.buttonStartUpdate.UseVisualStyleBackColor = false;
      this.buttonStartUpdate.Click += new System.EventHandler(this.buttonStartUpdate_Click);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.buttonSelectFile);
      this.groupBox2.Controls.Add(this.textBoxSelectedFile);
      this.groupBox2.Location = new System.Drawing.Point(16, 15);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
      this.groupBox2.Size = new System.Drawing.Size(697, 65);
      this.groupBox2.TabIndex = 3;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Update file";
      // 
      // buttonSelectFile
      // 
      this.buttonSelectFile.Location = new System.Drawing.Point(589, 21);
      this.buttonSelectFile.Margin = new System.Windows.Forms.Padding(4);
      this.buttonSelectFile.Name = "buttonSelectFile";
      this.buttonSelectFile.Size = new System.Drawing.Size(100, 28);
      this.buttonSelectFile.TabIndex = 1;
      this.buttonSelectFile.Text = "Select";
      this.buttonSelectFile.UseVisualStyleBackColor = true;
      this.buttonSelectFile.Click += new System.EventHandler(this.buttonSelectFile_Click);
      // 
      // textBoxSelectedFile
      // 
      this.textBoxSelectedFile.BackColor = System.Drawing.Color.White;
      this.textBoxSelectedFile.Location = new System.Drawing.Point(8, 23);
      this.textBoxSelectedFile.Margin = new System.Windows.Forms.Padding(4);
      this.textBoxSelectedFile.Name = "textBoxSelectedFile";
      this.textBoxSelectedFile.ReadOnly = true;
      this.textBoxSelectedFile.Size = new System.Drawing.Size(572, 22);
      this.textBoxSelectedFile.TabIndex = 0;
      // 
      // statusStrip1
      // 
      this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelProgressBar,
            this.progressBarProgress});
      this.statusStrip1.Location = new System.Drawing.Point(0, 580);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
      this.statusStrip1.Size = new System.Drawing.Size(729, 28);
      this.statusStrip1.SizingGrip = false;
      this.statusStrip1.TabIndex = 4;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // labelProgressBar
      // 
      this.labelProgressBar.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.labelProgressBar.Name = "labelProgressBar";
      this.labelProgressBar.Size = new System.Drawing.Size(48, 22);
      this.labelProgressBar.Text = "Ready";
      this.labelProgressBar.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // progressBarProgress
      // 
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(267, 20);
      // 
      // logRichTextBox
      // 
      this.logRichTextBox.BackColor = System.Drawing.SystemColors.Window;
      this.logRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.logRichTextBox.DebugColor = System.Drawing.Color.Gray;
      this.logRichTextBox.ErrorColor = System.Drawing.Color.DarkRed;
      this.logRichTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.logRichTextBox.InfoColor = System.Drawing.Color.SteelBlue;
      this.logRichTextBox.Location = new System.Drawing.Point(16, 363);
      this.logRichTextBox.Margin = new System.Windows.Forms.Padding(4);
      this.logRichTextBox.MaxLines = 100;
      this.logRichTextBox.Name = "logRichTextBox";
      this.logRichTextBox.ReadOnly = true;
      this.logRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.logRichTextBox.Size = new System.Drawing.Size(696, 211);
      this.logRichTextBox.TabIndex = 5;
      this.logRichTextBox.Text = "";
      this.logRichTextBox.TimestampColor = System.Drawing.Color.DarkGray;
      this.logRichTextBox.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
      this.logRichTextBox.WarningColor = System.Drawing.Color.DarkOrange;
      // 
      // transportUI1
      // 
      this.transportUI1.Location = new System.Drawing.Point(8, 20);
      this.transportUI1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
      this.transportUI1.Name = "transportUI1";
      this.transportUI1.Size = new System.Drawing.Size(500, 238);
      this.transportUI1.TabIndex = 1;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(729, 608);
      this.Controls.Add(this.logRichTextBox);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupboxConnection);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Margin = new System.Windows.Forms.Padding(4);
      this.MaximizeBox = false;
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Form1";
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.groupboxConnection.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private TransportUI transportUI1;
    private System.Windows.Forms.GroupBox groupboxConnection;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.TextBox textBoxSelectedFile;
    private System.Windows.Forms.Button buttonSelectFile;
    private System.Windows.Forms.Button buttonStartUpdate;
    private System.Windows.Forms.Button buttonAbort;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel labelProgressBar;
    private System.Windows.Forms.ToolStripProgressBar progressBarProgress;
    private UserControls.LogRichTextBox logRichTextBox;
  }
}

