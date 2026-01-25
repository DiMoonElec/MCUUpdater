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
      this.transportUI1 = new MCUUpdaterGUI.TransportUI();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.button1 = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
      this.progressBarProgress = new System.Windows.Forms.ToolStripProgressBar();
      this.textBoxLog = new System.Windows.Forms.TextBox();
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
      this.groupboxConnection.Location = new System.Drawing.Point(22, 131);
      this.groupboxConnection.Margin = new System.Windows.Forms.Padding(6);
      this.groupboxConnection.Name = "groupboxConnection";
      this.groupboxConnection.Padding = new System.Windows.Forms.Padding(6);
      this.groupboxConnection.Size = new System.Drawing.Size(959, 284);
      this.groupboxConnection.TabIndex = 2;
      this.groupboxConnection.TabStop = false;
      this.groupboxConnection.Text = "Connection";
      // 
      // buttonAbort
      // 
      this.buttonAbort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
      this.buttonAbort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.buttonAbort.Location = new System.Drawing.Point(710, 35);
      this.buttonAbort.Margin = new System.Windows.Forms.Padding(6);
      this.buttonAbort.Name = "buttonAbort";
      this.buttonAbort.Size = new System.Drawing.Size(238, 111);
      this.buttonAbort.TabIndex = 5;
      this.buttonAbort.Text = "ABORT";
      this.buttonAbort.UseVisualStyleBackColor = false;
      this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
      // 
      // buttonStartUpdate
      // 
      this.buttonStartUpdate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
      this.buttonStartUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.buttonStartUpdate.Location = new System.Drawing.Point(460, 35);
      this.buttonStartUpdate.Margin = new System.Windows.Forms.Padding(6);
      this.buttonStartUpdate.Name = "buttonStartUpdate";
      this.buttonStartUpdate.Size = new System.Drawing.Size(238, 111);
      this.buttonStartUpdate.TabIndex = 4;
      this.buttonStartUpdate.Text = "START\r\nUPDATE";
      this.buttonStartUpdate.UseVisualStyleBackColor = false;
      this.buttonStartUpdate.Click += new System.EventHandler(this.buttonStartUpdate_Click);
      // 
      // transportUI1
      // 
      this.transportUI1.Location = new System.Drawing.Point(6, 30);
      this.transportUI1.Margin = new System.Windows.Forms.Padding(11);
      this.transportUI1.Name = "transportUI1";
      this.transportUI1.Size = new System.Drawing.Size(411, 244);
      this.transportUI1.TabIndex = 1;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.button1);
      this.groupBox2.Controls.Add(this.textBox1);
      this.groupBox2.Location = new System.Drawing.Point(22, 22);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(6);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(6);
      this.groupBox2.Size = new System.Drawing.Size(959, 98);
      this.groupBox2.TabIndex = 3;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Update file";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(810, 31);
      this.button1.Margin = new System.Windows.Forms.Padding(6);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(138, 42);
      this.button1.TabIndex = 1;
      this.button1.Text = "button1";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // textBox1
      // 
      this.textBox1.BackColor = System.Drawing.Color.White;
      this.textBox1.Location = new System.Drawing.Point(11, 35);
      this.textBox1.Margin = new System.Windows.Forms.Padding(6);
      this.textBox1.Name = "textBox1";
      this.textBox1.ReadOnly = true;
      this.textBox1.Size = new System.Drawing.Size(785, 29);
      this.textBox1.TabIndex = 0;
      // 
      // statusStrip1
      // 
      this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.progressBarProgress});
      this.statusStrip1.Location = new System.Drawing.Point(0, 775);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 26, 0);
      this.statusStrip1.Size = new System.Drawing.Size(1003, 39);
      this.statusStrip1.TabIndex = 4;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // toolStripStatusLabel1
      // 
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new System.Drawing.Size(206, 30);
      this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
      // 
      // toolStripStatusLabel2
      // 
      this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
      this.toolStripStatusLabel2.Size = new System.Drawing.Size(206, 30);
      this.toolStripStatusLabel2.Text = "toolStripStatusLabel2";
      // 
      // progressBarProgress
      // 
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(367, 29);
      // 
      // textBoxLog
      // 
      this.textBoxLog.BackColor = System.Drawing.Color.White;
      this.textBoxLog.Location = new System.Drawing.Point(22, 426);
      this.textBoxLog.Margin = new System.Windows.Forms.Padding(6);
      this.textBoxLog.Multiline = true;
      this.textBoxLog.Name = "textBoxLog";
      this.textBoxLog.ReadOnly = true;
      this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.textBoxLog.Size = new System.Drawing.Size(956, 338);
      this.textBoxLog.TabIndex = 5;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1003, 814);
      this.Controls.Add(this.textBoxLog);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupboxConnection);
      this.Margin = new System.Windows.Forms.Padding(6);
      this.Name = "MainForm";
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

