﻿namespace KinectServer
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
            this.OpenGLWorker = new System.ComponentModel.BackgroundWorker();
            this.savingWorker = new System.ComponentModel.BackgroundWorker();
            this.updateWorker = new System.ComponentModel.BackgroundWorker();
            this.refineWorker = new System.ComponentModel.BackgroundWorker();
            this.statsWorker = new System.ComponentModel.BackgroundWorker();
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txBandwidthLabel = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.rxBandwidthLabel = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.txBufferLabel = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.liveBufferLabel = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.liveFrequencyLabel = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.rxFrequencyLabel = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.sourceListLabel = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.sourceTotalLabel = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.ueConnectedLabel = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ueTCPPicker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TCPPicker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rxBufferHoldPicker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.liveLatencyPicker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.requestLatencyPicker)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
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
            this.btShowLive.Enabled = false;
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
            this.statusStrip1.Location = new System.Drawing.Point(0, 372);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(603, 22);
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
            this.lClientListBox.Size = new System.Drawing.Size(219, 134);
            this.lClientListBox.TabIndex = 7;
            // 
            // lbSeqName
            // 
            this.lbSeqName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbSeqName.AutoSize = true;
            this.lbSeqName.Location = new System.Drawing.Point(381, 145);
            this.lbSeqName.Name = "lbSeqName";
            this.lbSeqName.Size = new System.Drawing.Size(85, 13);
            this.lbSeqName.TabIndex = 8;
            this.lbSeqName.Text = "Sequence name";
            // 
            // txtSeqName
            // 
            this.txtSeqName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSeqName.Location = new System.Drawing.Point(472, 142);
            this.txtSeqName.MaxLength = 40;
            this.txtSeqName.Name = "txtSeqName";
            this.txtSeqName.Size = new System.Drawing.Size(119, 20);
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
            // statsWorker
            // 
            this.statsWorker.WorkerSupportsCancellation = true;
            this.statsWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bufferStats_DoWork);
            // 
            // ueTCPPicker
            // 
            this.ueTCPPicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
            this.ueTCPPicker.Size = new System.Drawing.Size(119, 20);
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
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(358, 118);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "UE TCP Connections";
            // 
            // TCPPicker
            // 
            this.TCPPicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
            this.TCPPicker.Size = new System.Drawing.Size(119, 20);
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
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(376, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "TCP Connections";
            // 
            // rxBufferHoldPicker
            // 
            this.rxBufferHoldPicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
            this.rxBufferHoldPicker.Size = new System.Drawing.Size(119, 20);
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
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(361, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 13);
            this.label3.TabIndex = 26;
            this.label3.Text = "Rx Buffer Hold [pkts]";
            // 
            // liveLatencyPicker
            // 
            this.liveLatencyPicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
            this.liveLatencyPicker.Size = new System.Drawing.Size(119, 20);
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
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(341, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 13);
            this.label4.TabIndex = 28;
            this.label4.Text = "Live Update Interval [ms]";
            // 
            // requestLatencyPicker
            // 
            this.requestLatencyPicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.requestLatencyPicker.Location = new System.Drawing.Point(472, 12);
            this.requestLatencyPicker.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.requestLatencyPicker.Name = "requestLatencyPicker";
            this.requestLatencyPicker.Size = new System.Drawing.Size(119, 20);
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
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(343, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(123, 13);
            this.label5.TabIndex = 30;
            this.label5.Text = "Rx Request Interval [ms]";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(12, 183);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(186, 85);
            this.groupBox1.TabIndex = 31;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Network";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.Controls.Add(this.txBandwidthLabel, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.rxBandwidthLabel, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(174, 60);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // txBandwidthLabel
            // 
            this.txBandwidthLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txBandwidthLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txBandwidthLabel.Location = new System.Drawing.Point(55, 30);
            this.txBandwidthLabel.Name = "txBandwidthLabel";
            this.txBandwidthLabel.Size = new System.Drawing.Size(116, 30);
            this.txBandwidthLabel.TabIndex = 3;
            this.txBandwidthLabel.Text = "0 Mbps";
            this.txBandwidthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.Location = new System.Drawing.Point(3, 30);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 30);
            this.label7.TabIndex = 2;
            this.label7.Text = "Tx";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.Location = new System.Drawing.Point(3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(46, 30);
            this.label6.TabIndex = 1;
            this.label6.Text = "Rx";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rxBandwidthLabel
            // 
            this.rxBandwidthLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rxBandwidthLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rxBandwidthLabel.Location = new System.Drawing.Point(55, 0);
            this.rxBandwidthLabel.Name = "rxBandwidthLabel";
            this.rxBandwidthLabel.Size = new System.Drawing.Size(116, 30);
            this.rxBandwidthLabel.TabIndex = 0;
            this.rxBandwidthLabel.Text = "0 Mbps";
            this.rxBandwidthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel2);
            this.groupBox2.Location = new System.Drawing.Point(204, 183);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(186, 85);
            this.groupBox2.TabIndex = 32;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Buffer";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel2.Controls.Add(this.txBufferLabel, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label9, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label10, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.liveBufferLabel, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(174, 60);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // txBufferLabel
            // 
            this.txBufferLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txBufferLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txBufferLabel.Location = new System.Drawing.Point(55, 30);
            this.txBufferLabel.Name = "txBufferLabel";
            this.txBufferLabel.Size = new System.Drawing.Size(116, 30);
            this.txBufferLabel.TabIndex = 3;
            this.txBufferLabel.Text = "0";
            this.txBufferLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.Location = new System.Drawing.Point(3, 30);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(46, 30);
            this.label9.TabIndex = 2;
            this.label9.Text = "Tx";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.Location = new System.Drawing.Point(3, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(46, 30);
            this.label10.TabIndex = 1;
            this.label10.Text = "Live";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // liveBufferLabel
            // 
            this.liveBufferLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.liveBufferLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.liveBufferLabel.Location = new System.Drawing.Point(55, 0);
            this.liveBufferLabel.Name = "liveBufferLabel";
            this.liveBufferLabel.Size = new System.Drawing.Size(116, 30);
            this.liveBufferLabel.TabIndex = 0;
            this.liveBufferLabel.Text = "0";
            this.liveBufferLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel3);
            this.groupBox3.Location = new System.Drawing.Point(396, 183);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(186, 85);
            this.groupBox3.TabIndex = 33;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Operating Frequencies";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel3.Controls.Add(this.liveFrequencyLabel, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.label11, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label12, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.rxFrequencyLabel, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(174, 60);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // liveFrequencyLabel
            // 
            this.liveFrequencyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.liveFrequencyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.liveFrequencyLabel.Location = new System.Drawing.Point(55, 30);
            this.liveFrequencyLabel.Name = "liveFrequencyLabel";
            this.liveFrequencyLabel.Size = new System.Drawing.Size(116, 30);
            this.liveFrequencyLabel.TabIndex = 3;
            this.liveFrequencyLabel.Text = "0 Hz";
            this.liveFrequencyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.Location = new System.Drawing.Point(3, 30);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(46, 30);
            this.label11.TabIndex = 2;
            this.label11.Text = "Live";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.Location = new System.Drawing.Point(3, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(46, 30);
            this.label12.TabIndex = 1;
            this.label12.Text = "Rx";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rxFrequencyLabel
            // 
            this.rxFrequencyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rxFrequencyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rxFrequencyLabel.Location = new System.Drawing.Point(55, 0);
            this.rxFrequencyLabel.Name = "rxFrequencyLabel";
            this.rxFrequencyLabel.Size = new System.Drawing.Size(116, 30);
            this.rxFrequencyLabel.TabIndex = 0;
            this.rxFrequencyLabel.Text = "0 Hz";
            this.rxFrequencyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.tableLayoutPanel4);
            this.groupBox4.Location = new System.Drawing.Point(12, 274);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(186, 85);
            this.groupBox4.TabIndex = 32;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Source";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.sourceListLabel, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.label14, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.sourceTotalLabel, 1, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(174, 60);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // sourceListLabel
            // 
            this.sourceListLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel4.SetColumnSpan(this.sourceListLabel, 2);
            this.sourceListLabel.Location = new System.Drawing.Point(3, 30);
            this.sourceListLabel.Name = "sourceListLabel";
            this.sourceListLabel.Size = new System.Drawing.Size(168, 30);
            this.sourceListLabel.TabIndex = 2;
            this.sourceListLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label14.Location = new System.Drawing.Point(3, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(81, 30);
            this.label14.TabIndex = 1;
            this.label14.Text = "Currently Connected";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sourceTotalLabel
            // 
            this.sourceTotalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceTotalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sourceTotalLabel.Location = new System.Drawing.Point(90, 0);
            this.sourceTotalLabel.Name = "sourceTotalLabel";
            this.sourceTotalLabel.Size = new System.Drawing.Size(81, 30);
            this.sourceTotalLabel.TabIndex = 0;
            this.sourceTotalLabel.Text = "0";
            this.sourceTotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.tableLayoutPanel5);
            this.groupBox5.Location = new System.Drawing.Point(204, 274);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(186, 85);
            this.groupBox5.TabIndex = 33;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "User Experience";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.label8, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.ueConnectedLabel, 1, 0);
            this.tableLayoutPanel5.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(174, 60);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.Location = new System.Drawing.Point(3, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 30);
            this.label8.TabIndex = 2;
            this.label8.Text = "Currently Connected";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ueConnectedLabel
            // 
            this.ueConnectedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ueConnectedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ueConnectedLabel.Location = new System.Drawing.Point(90, 0);
            this.ueConnectedLabel.Name = "ueConnectedLabel";
            this.ueConnectedLabel.Size = new System.Drawing.Size(81, 30);
            this.ueConnectedLabel.TabIndex = 3;
            this.ueConnectedLabel.Text = "False";
            this.ueConnectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainWindowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 394);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
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
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
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
        private System.ComponentModel.BackgroundWorker statsWorker;
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label rxBandwidthLabel;
        private System.Windows.Forms.Label txBandwidthLabel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label txBufferLabel;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label liveBufferLabel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label liveFrequencyLabel;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label rxFrequencyLabel;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label sourceListLabel;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label sourceTotalLabel;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label ueConnectedLabel;
    }
}

