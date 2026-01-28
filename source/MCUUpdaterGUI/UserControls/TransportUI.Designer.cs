namespace MCUUpdaterGUI
{
  partial class TransportUI
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

    #region Код, автоматически созданный конструктором компонентов

    /// <summary> 
    /// Требуемый метод для поддержки конструктора — не изменяйте 
    /// содержимое этого метода с помощью редактора кода.
    /// </summary>
    private void InitializeComponent()
    {
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.label4 = new System.Windows.Forms.Label();
      this.comboBoxTransport = new System.Windows.Forms.ComboBox();
      this.textBoxConnectTimeout = new System.Windows.Forms.TextBox();
      this.textBoxRespTimeout = new System.Windows.Forms.TextBox();
      this.textBoxBaud_NetPort = new System.Windows.Forms.TextBox();
      this.comboBoxPort_Host = new System.Windows.Forms.ComboBox();
      this.labelConnectTimeout = new System.Windows.Forms.Label();
      this.labelRespTimeout = new System.Windows.Forms.Label();
      this.labelBaud_NetPort = new System.Windows.Forms.Label();
      this.textBoxDeviceWait = new System.Windows.Forms.TextBox();
      this.labelPort_Host = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 192F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Controls.Add(this.label4, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.comboBoxTransport, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.textBoxConnectTimeout, 1, 5);
      this.tableLayoutPanel1.Controls.Add(this.textBoxRespTimeout, 1, 4);
      this.tableLayoutPanel1.Controls.Add(this.textBoxBaud_NetPort, 1, 3);
      this.tableLayoutPanel1.Controls.Add(this.comboBoxPort_Host, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.labelConnectTimeout, 0, 5);
      this.tableLayoutPanel1.Controls.Add(this.labelRespTimeout, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.labelBaud_NetPort, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.textBoxDeviceWait, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.labelPort_Host, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 7;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(496, 245);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(4, 0);
      this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(184, 32);
      this.label4.TabIndex = 6;
      this.label4.Text = "Transport";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // comboBoxTransport
      // 
      this.comboBoxTransport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxTransport.FormattingEnabled = true;
      this.comboBoxTransport.Location = new System.Drawing.Point(196, 4);
      this.comboBoxTransport.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.comboBoxTransport.Name = "comboBoxTransport";
      this.comboBoxTransport.Size = new System.Drawing.Size(253, 24);
      this.comboBoxTransport.TabIndex = 0;
      this.comboBoxTransport.SelectedIndexChanged += new System.EventHandler(this.comboBoxTransport_SelectedIndexChanged);
      // 
      // textBoxConnectTimeout
      // 
      this.textBoxConnectTimeout.Location = new System.Drawing.Point(196, 158);
      this.textBoxConnectTimeout.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.textBoxConnectTimeout.Name = "textBoxConnectTimeout";
      this.textBoxConnectTimeout.Size = new System.Drawing.Size(253, 22);
      this.textBoxConnectTimeout.TabIndex = 5;
      // 
      // textBoxRespTimeout
      // 
      this.textBoxRespTimeout.Location = new System.Drawing.Point(196, 128);
      this.textBoxRespTimeout.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.textBoxRespTimeout.Name = "textBoxRespTimeout";
      this.textBoxRespTimeout.Size = new System.Drawing.Size(253, 22);
      this.textBoxRespTimeout.TabIndex = 4;
      // 
      // textBoxBaud_NetPort
      // 
      this.textBoxBaud_NetPort.Location = new System.Drawing.Point(196, 98);
      this.textBoxBaud_NetPort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.textBoxBaud_NetPort.Name = "textBoxBaud_NetPort";
      this.textBoxBaud_NetPort.Size = new System.Drawing.Size(253, 22);
      this.textBoxBaud_NetPort.TabIndex = 3;
      // 
      // comboBoxPort_Host
      // 
      this.comboBoxPort_Host.FormattingEnabled = true;
      this.comboBoxPort_Host.Location = new System.Drawing.Point(196, 66);
      this.comboBoxPort_Host.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.comboBoxPort_Host.Name = "comboBoxPort_Host";
      this.comboBoxPort_Host.Size = new System.Drawing.Size(253, 24);
      this.comboBoxPort_Host.TabIndex = 2;
      this.comboBoxPort_Host.DropDown += new System.EventHandler(this.comboBoxPort_Host_DropDown);
      // 
      // labelConnectTimeout
      // 
      this.labelConnectTimeout.AutoSize = true;
      this.labelConnectTimeout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelConnectTimeout.Location = new System.Drawing.Point(4, 154);
      this.labelConnectTimeout.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.labelConnectTimeout.Name = "labelConnectTimeout";
      this.labelConnectTimeout.Size = new System.Drawing.Size(184, 30);
      this.labelConnectTimeout.TabIndex = 7;
      this.labelConnectTimeout.Text = "Connection timeout, ms";
      this.labelConnectTimeout.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // labelRespTimeout
      // 
      this.labelRespTimeout.AutoSize = true;
      this.labelRespTimeout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelRespTimeout.Location = new System.Drawing.Point(4, 124);
      this.labelRespTimeout.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.labelRespTimeout.Name = "labelRespTimeout";
      this.labelRespTimeout.Size = new System.Drawing.Size(184, 30);
      this.labelRespTimeout.TabIndex = 4;
      this.labelRespTimeout.Text = "Response timeout, ms";
      this.labelRespTimeout.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // labelBaud_NetPort
      // 
      this.labelBaud_NetPort.AutoSize = true;
      this.labelBaud_NetPort.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelBaud_NetPort.Location = new System.Drawing.Point(4, 94);
      this.labelBaud_NetPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.labelBaud_NetPort.Name = "labelBaud_NetPort";
      this.labelBaud_NetPort.Size = new System.Drawing.Size(184, 30);
      this.labelBaud_NetPort.TabIndex = 2;
      this.labelBaud_NetPort.Text = "Baud";
      this.labelBaud_NetPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // textBoxDeviceWait
      // 
      this.textBoxDeviceWait.Location = new System.Drawing.Point(196, 36);
      this.textBoxDeviceWait.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.textBoxDeviceWait.Name = "textBoxDeviceWait";
      this.textBoxDeviceWait.Size = new System.Drawing.Size(253, 22);
      this.textBoxDeviceWait.TabIndex = 1;
      // 
      // labelPort_Host
      // 
      this.labelPort_Host.AutoSize = true;
      this.labelPort_Host.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelPort_Host.Location = new System.Drawing.Point(4, 62);
      this.labelPort_Host.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.labelPort_Host.Name = "labelPort_Host";
      this.labelPort_Host.Size = new System.Drawing.Size(184, 32);
      this.labelPort_Host.TabIndex = 0;
      this.labelPort_Host.Text = "Port";
      this.labelPort_Host.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(4, 32);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(184, 30);
      this.label1.TabIndex = 9;
      this.label1.Text = "Device Wait, sec";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // TransportUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "TransportUI";
      this.Size = new System.Drawing.Size(496, 245);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Label labelPort_Host;
    private System.Windows.Forms.ComboBox comboBoxPort_Host;
    private System.Windows.Forms.Label labelBaud_NetPort;
    private System.Windows.Forms.TextBox textBoxBaud_NetPort;
    private System.Windows.Forms.Label labelRespTimeout;
    private System.Windows.Forms.TextBox textBoxRespTimeout;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox comboBoxTransport;
    private System.Windows.Forms.Label labelConnectTimeout;
    private System.Windows.Forms.TextBox textBoxConnectTimeout;
    private System.Windows.Forms.TextBox textBoxDeviceWait;
    private System.Windows.Forms.Label label1;
  }
}
