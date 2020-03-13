namespace KinectServer
{
    partial class MainWindowForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btStart = new System.Windows.Forms.Button();
            this.btSettings = new System.Windows.Forms.Button();
            this.btCalibrate = new System.Windows.Forms.Button();
            this.btRefineCalib = new System.Windows.Forms.Button();
            this.btRecord = new System.Windows.Forms.Button();
            this.btShowLive = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.lrxFreqToolstrip = new System.Windows.Forms.ToolStripStatusLabel();
            this.lliveFreqToolstrip = new System.Windows.Forms.ToolStripStatusLabel();
            this.lBufferToolstrip = new System.Windows.Forms.ToolStripStatusLabel();
            this.lClientListBox = new System.Windows.Forms.ListBox();
            this.lbSeqName = new System.Windows.Forms.Label();
            this.txtSeqName = new System.Windows.Forms.TextBox();
            this.btDebug = new System.Windows.Forms.Button();
            this.recordingWorker = new System.ComponentModel.BackgroundWorker();
            this.OpenGLWorker = new System.ComponentModel.BackgroundWorker();
            this.savingWorker = new System.ComponentModel.BackgroundWorker();
            this.updateWorker = new System.ComponentModel.BackgroundWorker();
            this.refineWorker = new System.ComponentModel.BackgroundWorker();
            this.bufferStats = new System.ComponentModel.BackgroundWorker();
            this.ueTCPPicker = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.TCPPicker = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.rxBufferHoldPicker = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.liveLatencyPicker = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.requestLatencyPicker = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ueTCPPicker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TCPPicker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rxBufferHoldPicker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.liveLatencyPicker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.requestLatencyPicker)).BeginInit();
            this.SuspendLayout();
            // 
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(12, 12);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(95, 23);
            this.btStart.TabIndex = 0;
            this.btStart.Text = "Start server";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.Click += new System.EventHandler(this.btStart_Click);
            // 
            // btSettings
            // 
            this.btSettings.Location = new System.Drawing.Point(12, 41);
            this.btSettings.Name = "btSettings";
            this.btSettings.Size = new System.Drawing.Size(95, 23);
            this.btSettings.TabIndex = 1;
            this.btSettings.Text = "Settings";
            this.btSettings.UseVisualStyleBackColor = true;
            this.btSettings.Click += new System.EventHandler(this.btSettings_Click);
            // 
            // btCalibrate
            // 
            this.btCalibrate.Location = new System.Drawing.Point(12, 70);
            this.btCalibrate.Name = "btCalibrate";
            this.btCalibrate.Size = new System.Drawing.Size(95, 23);
            this.btCalibrate.TabIndex = 2;
            this.btCalibrate.Text = "Calibrate";
            this.btCalibrate.UseVisualStyleBackColor = true;
            this.btCalibrate.Click += new System.EventHandler(this.btCalibrate_Click);
            // 
            // btRefineCalib
            // 
            this.btRefineCalib.Location = new System.Drawing.Point(12, 97);
            this.btRefineCalib.Name = "btRefineCalib";
            this.btRefineCalib.Size = new System.Drawing.Size(95, 23);
            this.btRefineCalib.TabIndex = 3;
            this.btRefineCalib.Text = "Refine calib";
            this.btRefineCalib.UseVisualStyleBackColor = true;
            this.btRefineCalib.Click += new System.EventHandler(this.btRefineCalib_Click);
            // 
            // btRecord
            // 
            this.btRecord.Location = new System.Drawing.Point(12, 126);
            this.btRecord.Name = "btRecord";
            this.btRecord.Size = new System.Drawing.Size(95, 23);
            this.btRecord.TabIndex = 4;
            this.btRecord.Text = "Start recording";
            this.btRecord.UseVisualStyleBackColor = true;
            this.btRecord.Click += new System.EventHandler(this.btRecord_Click);
            // 
            // btShowLive
            // 
            this.btShowLive.Location = new System.Drawing.Point(12, 154);
            this.btShowLive.Name = "btShowLive";
            this.btShowLive.Size = new System.Drawing.Size(95, 23);
            this.btShowLive.TabIndex = 5;
            this.btShowLive.Text = "Show live";
            this.btShowLive.UseVisualStyleBackColor = true;
            this.btShowLive.Click += new System.EventHandler(this.btShowLive_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.lrxFreqToolstrip,
            this.lliveFreqToolstrip,
            this.lBufferToolstrip});
            this.statusStrip1.Location = new System.Drawing.Point(0, 191);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(600, 24);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 19);
            // 
            // lrxFreqToolstrip
            // 
            this.lrxFreqToolstrip.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lrxFreqToolstrip.BorderStyle = System.Windows.Forms.Border3DStyle.Bump;
            this.lrxFreqToolstrip.Name = "lrxFreqToolstrip";
            this.lrxFreqToolstrip.Size = new System.Drawing.Size(44, 19);
            this.lrxFreqToolstrip.Text = "rxFreq";
            // 
            // lliveFreqToolstrip
            // 
            this.lliveFreqToolstrip.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.lliveFreqToolstrip.Name = "lliveFreqToolstrip";
            this.lliveFreqToolstrip.Size = new System.Drawing.Size(48, 19);
            this.lliveFreqToolstrip.Text = "liveFreq";
            // 
            // lBufferToolstrip
            // 
            this.lBufferToolstrip.Name = "lBufferToolstrip";
            this.lBufferToolstrip.Size = new System.Drawing.Size(85, 19);
            this.lBufferToolstrip.Text = "Buffer Statuses";
            this.lBufferToolstrip.Visible = false;
            // 
            // lClientListBox
            // 
            this.lClientListBox.FormattingEnabled = true;
            this.lClientListBox.HorizontalScrollbar = true;
            this.lClientListBox.Location = new System.Drawing.Point(117, 12);
            this.lClientListBox.Name = "lClientListBox";
            this.lClientListBox.Size = new System.Drawing.Size(219, 134);
            this.lClientListBox.TabIndex = 7;
            // 
            // lbSeqName
            // 
            this.lbSeqName.AutoSize = true;
            this.lbSeqName.Location = new System.Drawing.Point(381, 145);
            this.lbSeqName.Name = "lbSeqName";
            this.lbSeqName.Size = new System.Drawing.Size(85, 13);
            this.lbSeqName.TabIndex = 8;
            this.lbSeqName.Text = "Sequence name";
            // 
            // txtSeqName
            // 
            this.txtSeqName.Location = new System.Drawing.Point(472, 142);
            this.txtSeqName.MaxLength = 40;
            this.txtSeqName.Name = "txtSeqName";
            this.txtSeqName.Size = new System.Drawing.Size(116, 20);
            this.txtSeqName.TabIndex = 9;
            this.txtSeqName.Text = "noname";
            // 
            // btDebug
            // 
            this.btDebug.Location = new System.Drawing.Point(117, 154);
            this.btDebug.Name = "btDebug";
            this.btDebug.Size = new System.Drawing.Size(105, 23);
            this.btDebug.TabIndex = 10;
            this.btDebug.Text = "Start Log";
            this.btDebug.UseVisualStyleBackColor = true;
            this.btDebug.Click += new System.EventHandler(this.btDebug_Click);
            // 
            // recordingWorker
            // 
            this.recordingWorker.WorkerSupportsCancellation = true;
            this.recordingWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.recordingWorker_DoWork);
            this.recordingWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.recordingWorker_RunWorkerCompleted);
            // 
            // OpenGLWorker
            // 
            this.OpenGLWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.OpenGLWorker_DoWork);
            this.OpenGLWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.OpenGLWorker_RunWorkerCompleted);
            // 
            // savingWorker
            // 
            this.savingWorker.WorkerSupportsCancellation = true;
            this.savingWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.savingWorker_DoWork);
            this.savingWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.savingWorker_RunWorkerCompleted);
            // 
            // updateWorker
            // 
            this.updateWorker.WorkerSupportsCancellation = true;
            this.updateWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.updateWorker_DoWork);
            // 
            // refineWorker
            // 
            this.refineWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.refineWorker_DoWork);
            this.refineWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.refineWorker_RunWorkerCompleted);
            // 
            // bufferStats
            // 
            this.bufferStats.WorkerSupportsCancellation = true;
            this.bufferStats.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bufferStats_DoWork);
            // 
            // ueTCPPicker
            // 
            this.ueTCPPicker.Location = new System.Drawing.Point(472, 116);
            this.ueTCPPicker.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.ueTCPPicker.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ueTCPPicker.Name = "ueTCPPicker";
            this.ueTCPPicker.Size = new System.Drawing.Size(116, 20);
            this.ueTCPPicker.TabIndex = 21;
            this.ueTCPPicker.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ueTCPPicker.ValueChanged += new System.EventHandler(this.ueTCPPicker_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(358, 118);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "UE TCP Connections";
            // 
            // TCPPicker
            // 
            this.TCPPicker.Location = new System.Drawing.Point(472, 90);
            this.TCPPicker.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.TCPPicker.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.TCPPicker.Name = "TCPPicker";
            this.TCPPicker.Size = new System.Drawing.Size(116, 20);
            this.TCPPicker.TabIndex = 23;
            this.TCPPicker.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.TCPPicker.ValueChanged += new System.EventHandler(this.TCPPicker_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(376, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "TCP Connections";
            // 
            // rxBufferHoldPicker
            // 
            this.rxBufferHoldPicker.Location = new System.Drawing.Point(472, 64);
            this.rxBufferHoldPicker.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.rxBufferHoldPicker.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.rxBufferHoldPicker.Name = "rxBufferHoldPicker";
            this.rxBufferHoldPicker.Size = new System.Drawing.Size(116, 20);
            this.rxBufferHoldPicker.TabIndex = 25;
            this.rxBufferHoldPicker.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.rxBufferHoldPicker.ValueChanged += new System.EventHandler(this.rxBufferHoldPicker_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(361, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 13);
            this.label3.TabIndex = 26;
            this.label3.Text = "Rx Buffer Hold [pkts]";
            // 
            // liveLatencyPicker
            // 
            this.liveLatencyPicker.Location = new System.Drawing.Point(472, 38);
            this.liveLatencyPicker.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.liveLatencyPicker.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.liveLatencyPicker.Name = "liveLatencyPicker";
            this.liveLatencyPicker.Size = new System.Drawing.Size(116, 20);
            this.liveLatencyPicker.TabIndex = 27;
            this.liveLatencyPicker.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.liveLatencyPicker.ValueChanged += new System.EventHandler(this.liveLatencyPicker_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(341, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 13);
            this.label4.TabIndex = 28;
            this.label4.Text = "Live Update Interval [ms]";
            // 
            // requestLatencyPicker
            // 
            this.requestLatencyPicker.Location = new System.Drawing.Point(472, 12);
            this.requestLatencyPicker.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.requestLatencyPicker.Name = "requestLatencyPicker";
            this.requestLatencyPicker.Size = new System.Drawing.Size(116, 20);
            this.requestLatencyPicker.TabIndex = 29;
            this.requestLatencyPicker.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.requestLatencyPicker.ValueChanged += new System.EventHandler(this.requestLatencyPicker_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(343, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(123, 13);
            this.label5.TabIndex = 30;
            this.label5.Text = "Rx Request Interval [ms]";
            // 
            // MainWindowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 215);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.requestLatencyPicker);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.liveLatencyPicker);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.rxBufferHoldPicker);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TCPPicker);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ueTCPPicker);
            this.Controls.Add(this.btStart);
            this.Controls.Add(this.btSettings);
            this.Controls.Add(this.btCalibrate);
            this.Controls.Add(this.btRefineCalib);
            this.Controls.Add(this.btRecord);
            this.Controls.Add(this.btShowLive);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.lClientListBox);
            this.Controls.Add(this.lbSeqName);
            this.Controls.Add(this.txtSeqName);
            this.Controls.Add(this.btDebug);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainWindowForm";
            this.Text = "LiveScanServer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.MainWindowForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ueTCPPicker)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TCPPicker)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rxBufferHoldPicker)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.liveLatencyPicker)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.requestLatencyPicker)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.Button btSettings;
        private System.Windows.Forms.Button btCalibrate;
        private System.Windows.Forms.Button btRefineCalib;
        private System.Windows.Forms.Button btRecord;
        private System.Windows.Forms.Button btShowLive;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ListBox lClientListBox;
        private System.Windows.Forms.Label lbSeqName;
        private System.Windows.Forms.TextBox txtSeqName;
        private System.Windows.Forms.Button btDebug;
        private System.ComponentModel.BackgroundWorker recordingWorker;                 
        private System.ComponentModel.BackgroundWorker OpenGLWorker;
        private System.ComponentModel.BackgroundWorker savingWorker;
        private System.ComponentModel.BackgroundWorker updateWorker;                
        private System.ComponentModel.BackgroundWorker refineWorker;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel lBufferToolstrip;
        private System.ComponentModel.BackgroundWorker bufferStats;
        private System.Windows.Forms.NumericUpDown ueTCPPicker;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown TCPPicker;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown rxBufferHoldPicker;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown liveLatencyPicker;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown requestLatencyPicker;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripStatusLabel lrxFreqToolstrip;
        private System.Windows.Forms.ToolStripStatusLabel lliveFreqToolstrip;
    }
}

