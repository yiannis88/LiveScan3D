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
            this.lClientListBox = new System.Windows.Forms.ListBox();
            this.lbSeqName = new System.Windows.Forms.Label();
            this.txtSeqName = new System.Windows.Forms.TextBox();
            this.btDebug = new System.Windows.Forms.Button();
            this.recordingWorker = new System.ComponentModel.BackgroundWorker();
            this.reqLatency = new System.Windows.Forms.TextBox();
            this.btLatencyBtn = new System.Windows.Forms.Button();
            this.showLiveLatency = new System.Windows.Forms.TextBox();
            this.btshowLiveLatencyBtn = new System.Windows.Forms.Button();
            this.holdRxFrames = new System.Windows.Forms.TextBox();
            this.btHoldRxFrames = new System.Windows.Forms.Button();
            this.tcpConnections = new System.Windows.Forms.TextBox();
            this.btTcpConnections = new System.Windows.Forms.Button();
            this.dropFramesDevLatency = new System.Windows.Forms.TextBox();
            this.dropFramesDevStep = new System.Windows.Forms.TextBox();
            this.dropFramesDevFps = new System.Windows.Forms.TextBox();
            this.btDropFramesDev = new System.Windows.Forms.Button();
            this.tcpConnectionsUe = new System.Windows.Forms.TextBox();
            this.btTcpConnectionsUe = new System.Windows.Forms.Button();
            this.OpenGLWorker = new System.ComponentModel.BackgroundWorker();
            this.savingWorker = new System.ComponentModel.BackgroundWorker();
            this.updateWorker = new System.ComponentModel.BackgroundWorker();
            this.refineWorker = new System.ComponentModel.BackgroundWorker();
            this.statusStrip1.SuspendLayout();
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
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 187);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(600, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // lClientListBox
            // 
            this.lClientListBox.FormattingEnabled = true;
            this.lClientListBox.HorizontalScrollbar = true;
            this.lClientListBox.Location = new System.Drawing.Point(117, 12);
            this.lClientListBox.Name = "lClientListBox";
            this.lClientListBox.Size = new System.Drawing.Size(219, 108);
            this.lClientListBox.TabIndex = 7;
            // 
            // lbSeqName
            // 
            this.lbSeqName.AutoSize = true;
            this.lbSeqName.Location = new System.Drawing.Point(123, 157);
            this.lbSeqName.Name = "lbSeqName";
            this.lbSeqName.Size = new System.Drawing.Size(88, 13);
            this.lbSeqName.TabIndex = 8;
            this.lbSeqName.Text = "Sequence name:";
            this.lbSeqName.Click += new System.EventHandler(this.LbSeqName_Click);
            // 
            // txtSeqName
            // 
            this.txtSeqName.Location = new System.Drawing.Point(233, 155);
            this.txtSeqName.MaxLength = 40;
            this.txtSeqName.Name = "txtSeqName";
            this.txtSeqName.Size = new System.Drawing.Size(106, 20);
            this.txtSeqName.TabIndex = 9;
            this.txtSeqName.Text = "noname";
            // 
            // btDebug
            // 
            this.btDebug.Location = new System.Drawing.Point(233, 123);
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
            // reqLatency
            // 
            this.reqLatency.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.reqLatency.Location = new System.Drawing.Point(346, 12);
            this.reqLatency.MaxLength = 10;
            this.reqLatency.Name = "reqLatency";
            this.reqLatency.Size = new System.Drawing.Size(116, 20);
            this.reqLatency.TabIndex = 11;
            this.reqLatency.Text = "Request Latency [ms]";
            this.reqLatency.Enter += new System.EventHandler(this.reqLatency_enter);
            this.reqLatency.Leave += new System.EventHandler(this.reqLatency_leave);
            // 
            // btLatencyBtn
            // 
            this.btLatencyBtn.Location = new System.Drawing.Point(472, 12);
            this.btLatencyBtn.Name = "btLatencyBtn";
            this.btLatencyBtn.Size = new System.Drawing.Size(116, 23);
            this.btLatencyBtn.TabIndex = 12;
            this.btLatencyBtn.Text = "Request Latency";
            this.btLatencyBtn.UseVisualStyleBackColor = true;
            this.btLatencyBtn.Click += new System.EventHandler(this.btLatencyBtn_Click);
            // 
            // showLiveLatency
            // 
            this.showLiveLatency.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.showLiveLatency.Location = new System.Drawing.Point(346, 41);
            this.showLiveLatency.MaxLength = 10;
            this.showLiveLatency.Name = "showLiveLatency";
            this.showLiveLatency.Size = new System.Drawing.Size(116, 20);
            this.showLiveLatency.TabIndex = 13;
            this.showLiveLatency.Text = "ShowLive Latency [ms]";
            this.showLiveLatency.Enter += new System.EventHandler(this.showLiveLatency_enter);
            this.showLiveLatency.Leave += new System.EventHandler(this.showLiveLatency_leave);
            // 
            // btshowLiveLatencyBtn
            // 
            this.btshowLiveLatencyBtn.Location = new System.Drawing.Point(472, 41);
            this.btshowLiveLatencyBtn.Name = "btshowLiveLatencyBtn";
            this.btshowLiveLatencyBtn.Size = new System.Drawing.Size(116, 23);
            this.btshowLiveLatencyBtn.TabIndex = 14;
            this.btshowLiveLatencyBtn.Text = "ShowLive Latency";
            this.btshowLiveLatencyBtn.UseVisualStyleBackColor = true;
            this.btshowLiveLatencyBtn.Click += new System.EventHandler(this.btshowLiveLatencyBtn_Click);
            // 
            // holdRxFrames
            // 
            this.holdRxFrames.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.holdRxFrames.Location = new System.Drawing.Point(346, 70);
            this.holdRxFrames.MaxLength = 10;
            this.holdRxFrames.Name = "holdRxFrames";
            this.holdRxFrames.Size = new System.Drawing.Size(116, 20);
            this.holdRxFrames.TabIndex = 15;
            this.holdRxFrames.Text = "Rx Buffer Hold [pkts]";
            this.holdRxFrames.Enter += new System.EventHandler(this.holdRxFrames_enter);
            this.holdRxFrames.Leave += new System.EventHandler(this.holdRxFrames_leave);
            // 
            // btHoldRxFrames
            // 
            this.btHoldRxFrames.Location = new System.Drawing.Point(472, 70);
            this.btHoldRxFrames.Name = "btHoldRxFrames";
            this.btHoldRxFrames.Size = new System.Drawing.Size(116, 23);
            this.btHoldRxFrames.TabIndex = 16;
            this.btHoldRxFrames.Text = "Buffer Hold";
            this.btHoldRxFrames.UseVisualStyleBackColor = true;
            this.btHoldRxFrames.Click += new System.EventHandler(this.btHoldRxFrames_Click);
            // 
            // tcpConnections
            // 
            this.tcpConnections.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.tcpConnections.Location = new System.Drawing.Point(346, 99);
            this.tcpConnections.MaxLength = 10;
            this.tcpConnections.Name = "tcpConnections";
            this.tcpConnections.Size = new System.Drawing.Size(116, 20);
            this.tcpConnections.TabIndex = 17;
            this.tcpConnections.Text = "TCP connections";
            this.tcpConnections.TextChanged += new System.EventHandler(this.tcpConnections_TextChanged);
            this.tcpConnections.Enter += new System.EventHandler(this.tcpConnections_enter);
            this.tcpConnections.Leave += new System.EventHandler(this.tcpConnections_leave);
            // 
            // btTcpConnections
            // 
            this.btTcpConnections.Location = new System.Drawing.Point(472, 99);
            this.btTcpConnections.Name = "btTcpConnections";
            this.btTcpConnections.Size = new System.Drawing.Size(116, 23);
            this.btTcpConnections.TabIndex = 18;
            this.btTcpConnections.Text = "TCP connections";
            this.btTcpConnections.UseVisualStyleBackColor = true;
            this.btTcpConnections.Click += new System.EventHandler(this.btTcpConnections_Click);
            // 
            // DropFramesDev
            // 
            this.dropFramesDevLatency.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dropFramesDevLatency.Location = new System.Drawing.Point(346, 129);
            this.dropFramesDevLatency.MaxLength = 4;
            this.dropFramesDevLatency.Name = "dropFramesDevLatency";
            this.dropFramesDevLatency.Size = new System.Drawing.Size(35, 20);
            this.dropFramesDevLatency.TabIndex = 19;
            this.dropFramesDevLatency.Text = "Lat";
            this.dropFramesDevLatency.TextChanged += new System.EventHandler(this.dropFramesDevLatency_TextChanged);
            this.dropFramesDevLatency.Enter += new System.EventHandler(this.dropFramesDevLatency_enter);
            this.dropFramesDevLatency.Leave += new System.EventHandler(this.dropFramesDevLatency_leave);
            ///////////// 2nd requirement /////////////
            this.dropFramesDevFps.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dropFramesDevFps.Location = new System.Drawing.Point(386, 129);
            this.dropFramesDevFps.MaxLength = 4;
            this.dropFramesDevFps.Name = "dropFramesDevFps";
            this.dropFramesDevFps.Size = new System.Drawing.Size(35, 20);
            this.dropFramesDevFps.TabIndex = 20;
            this.dropFramesDevFps.Text = "FPS";
            this.dropFramesDevFps.TextChanged += new System.EventHandler(this.dropFramesDevFps_TextChanged);
            this.dropFramesDevFps.Enter += new System.EventHandler(this.dropFramesDevFps_enter);
            this.dropFramesDevFps.Leave += new System.EventHandler(this.dropFramesDevFps_leave);
            ///////////// 3rd parameter algorithm step /////////////
            this.dropFramesDevStep.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dropFramesDevStep.Location = new System.Drawing.Point(426, 129);
            this.dropFramesDevStep.MaxLength = 4;
            this.dropFramesDevStep.Name = "dropFramesDevStep";
            this.dropFramesDevStep.Size = new System.Drawing.Size(35, 20);
            this.dropFramesDevStep.TabIndex = 21;
            this.dropFramesDevStep.Text = "Step";
            this.dropFramesDevStep.TextChanged += new System.EventHandler(this.dropFramesDevStep_TextChanged);
            this.dropFramesDevStep.Enter += new System.EventHandler(this.dropFramesDevStep_enter);
            this.dropFramesDevStep.Leave += new System.EventHandler(this.dropFramesDevStep_leave);
            // 
            // btTcpConnections
            // 
            this.btDropFramesDev.Location = new System.Drawing.Point(472, 129);
            this.btDropFramesDev.Name = "btDropFramesDev";
            this.btDropFramesDev.Size = new System.Drawing.Size(116, 23);
            this.btDropFramesDev.TabIndex = 22;
            this.btDropFramesDev.Text = "Requirements";
            this.btDropFramesDev.UseVisualStyleBackColor = true;
            this.btDropFramesDev.Click += new System.EventHandler(this.btDropFramesDev_Click);
            // 
            // tcpConnectionsUe
            // 
            this.tcpConnectionsUe.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.tcpConnectionsUe.Location = new System.Drawing.Point(346, 157);
            this.tcpConnectionsUe.MaxLength = 10;
            this.tcpConnectionsUe.Name = "tcpConnectionsUe";
            this.tcpConnectionsUe.Size = new System.Drawing.Size(116, 20);
            this.tcpConnectionsUe.TabIndex = 23;
            this.tcpConnectionsUe.Text = "TCP connections UE";
            this.tcpConnectionsUe.Enter += new System.EventHandler(this.tcpConnectionsUe_enter);
            this.tcpConnectionsUe.Leave += new System.EventHandler(this.tcpConnectionsUe_leave);
            // 
            // btTcpConnectionsUe
            // 
            this.btTcpConnectionsUe.Location = new System.Drawing.Point(472, 157);
            this.btTcpConnectionsUe.Name = "btTcpConnectionsUe";
            this.btTcpConnectionsUe.Size = new System.Drawing.Size(116, 23);
            this.btTcpConnectionsUe.TabIndex = 24;
            this.btTcpConnectionsUe.Text = "TCP connections UE";
            this.btTcpConnectionsUe.UseVisualStyleBackColor = true;
            this.btTcpConnectionsUe.Click += new System.EventHandler(this.btTcpConnectionsUe_Click);
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
            // MainWindowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 209);
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
            this.Controls.Add(this.reqLatency);
            this.Controls.Add(this.btLatencyBtn);
            this.Controls.Add(this.showLiveLatency);
            this.Controls.Add(this.btshowLiveLatencyBtn);
            this.Controls.Add(this.holdRxFrames);
            this.Controls.Add(this.btHoldRxFrames);
            this.Controls.Add(this.tcpConnections);
            this.Controls.Add(this.btTcpConnections);
            this.Controls.Add(this.dropFramesDevLatency);
            this.Controls.Add(this.dropFramesDevFps);
            this.Controls.Add(this.dropFramesDevStep);
            this.Controls.Add(this.btDropFramesDev);
            this.Controls.Add(this.tcpConnectionsUe);
            this.Controls.Add(this.btTcpConnectionsUe);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainWindowForm";
            this.Text = "LiveScanServer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.MainWindowForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
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
        private System.Windows.Forms.TextBox reqLatency;
        private System.Windows.Forms.Button btLatencyBtn;
        private System.Windows.Forms.TextBox showLiveLatency;
        private System.Windows.Forms.Button btshowLiveLatencyBtn;
        private System.Windows.Forms.TextBox holdRxFrames;
        private System.Windows.Forms.Button btHoldRxFrames;
        private System.Windows.Forms.TextBox tcpConnections;
        private System.Windows.Forms.Button btTcpConnections;
        private System.Windows.Forms.TextBox dropFramesDevLatency;        
        private System.Windows.Forms.TextBox dropFramesDevFps;
        private System.Windows.Forms.TextBox dropFramesDevStep;
        private System.Windows.Forms.Button btDropFramesDev;
        private System.Windows.Forms.TextBox tcpConnectionsUe;
        private System.Windows.Forms.Button btTcpConnectionsUe;
        private System.ComponentModel.BackgroundWorker recordingWorker;                 
        private System.ComponentModel.BackgroundWorker OpenGLWorker;
        private System.ComponentModel.BackgroundWorker savingWorker;
        private System.ComponentModel.BackgroundWorker updateWorker;                
        private System.ComponentModel.BackgroundWorker refineWorker;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;        
    }
}

