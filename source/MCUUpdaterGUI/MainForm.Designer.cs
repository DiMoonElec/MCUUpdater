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
      this.button1 = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
      this.progressBarProgress = new System.Windows.Forms.ToolStripProgressBar();
      this.textBoxLog = new System.Windows.Forms.TextBox();
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
      this.groupboxConnection.Location = new System.Drawing.Point(12, 71);
      this.groupboxConnection.Name = "groupboxConnection";
      this.groupboxConnection.Size = new System.Drawing.Size(523, 218);
      this.groupboxConnection.TabIndex = 2;
      this.groupboxConnection.TabStop = false;
      this.groupboxConnection.Text = "Connection";
      // 
      // buttonAbort
      // 
      this.buttonAbort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
      this.buttonAbort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.buttonAbort.Location = new System.Drawing.Point(387, 82);
      this.buttonAbort.Name = "buttonAbort";
      this.buttonAbort.Size = new System.Drawing.Size(130, 60);
      this.buttonAbort.TabIndex = 5;
      this.buttonAbort.Text = "ABORT";
      this.buttonAbort.UseVisualStyleBackColor = false;
      this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
      // 
      // buttonStartUpdate
      // 
      this.buttonStartUpdate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
      this.buttonStartUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.buttonStartUpdate.Location = new System.Drawing.Point(387, 16);
      this.buttonStartUpdate.Name = "buttonStartUpdate";
      this.buttonStartUpdate.Size = new System.Drawing.Size(130, 60);
      this.buttonStartUpdate.TabIndex = 4;
      this.buttonStartUpdate.Text = "START\r\nUPDATE";
      this.buttonStartUpdate.UseVisualStyleBackColor = false;
      this.buttonStartUpdate.Click += new System.EventHandler(this.buttonStartUpdate_Click);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.button1);
      this.groupBox2.Controls.Add(this.textBox1);
      this.groupBox2.Location = new System.Drawing.Point(12, 12);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(523, 53);
      this.groupBox2.TabIndex = 3;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Update file";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(442, 17);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "button1";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // textBox1
      // 
      this.textBox1.BackColor = System.Drawing.Color.White;
      this.textBox1.Location = new System.Drawing.Point(6, 19);
      this.textBox1.Name = "textBox1";
      this.textBox1.ReadOnly = true;
      this.textBox1.Size = new System.Drawing.Size(430, 20);
      this.textBox1.TabIndex = 0;
      // 
      // statusStrip1
      // 
      this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.progressBarProgress});
      this.statusStrip1.Location = new System.Drawing.Point(0, 470);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(547, 24);
      this.statusStrip1.SizingGrip = false;
      this.statusStrip1.TabIndex = 4;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // toolStripStatusLabel1
      // 
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 19);
      this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
      // 
      // toolStripStatusLabel2
      // 
      this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
      this.toolStripStatusLabel2.Size = new System.Drawing.Size(118, 19);
      this.toolStripStatusLabel2.Text = "toolStripStatusLabel2";
      // 
      // progressBarProgress
      // 
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(200, 18);
      // 
      // textBoxLog
      // 
      this.textBoxLog.BackColor = System.Drawing.Color.White;
      this.textBoxLog.Location = new System.Drawing.Point(12, 295);
      this.textBoxLog.Multiline = true;
      this.textBoxLog.Name = "textBoxLog";
      this.textBoxLog.ReadOnly = true;
      this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.textBoxLog.Size = new System.Drawing.Size(523, 172);
      this.textBoxLog.TabIndex = 5;
      // 
      // transportUI1
      // 
      this.transportUI1.Location = new System.Drawing.Point(6, 16);
      this.transportUI1.Margin = new System.Windows.Forms.Padding(6);
      this.transportUI1.Name = "transportUI1";
      this.transportUI1.Size = new System.Drawing.Size(375, 193);
      this.transportUI1.TabIndex = 1;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(547, 494);
      this.Controls.Add(this.textBoxLog);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupboxConnection);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
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
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button buttonStartUpdate;
    private System.Windows.Forms.Button buttonAbort;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
    private System.Windows.Forms.ToolStripProgressBar progressBarProgress;
    private System.Windows.Forms.TextBox textBoxLog;
  }
}

