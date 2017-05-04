namespace SMTMotionPlanning
{
    partial class SegmentationForm
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
            this.executeButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.loadButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.captureButton = new System.Windows.Forms.Button();
            this.stitchButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.greenLowerLabel = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.greenUpperLabel = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.redLowerLabel = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.redUpperLabel = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.blueLowerLabel = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.blueUpperLabel = new System.Windows.Forms.Label();
            this.checkBox = new System.Windows.Forms.TextBox();
            this.checkButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.greenLowerBar = new System.Windows.Forms.HScrollBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.greenUpperBar = new System.Windows.Forms.HScrollBar();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.redLowerBar = new System.Windows.Forms.HScrollBar();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.redUpperBar = new System.Windows.Forms.HScrollBar();
            this.label12 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.blueLowerBar = new System.Windows.Forms.HScrollBar();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.blueUpperBar = new System.Windows.Forms.HScrollBar();
            this.label16 = new System.Windows.Forms.Label();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.label19 = new System.Windows.Forms.Label();
            this.cameraBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.flowLayoutPanel5.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // executeButton
            // 
            this.executeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.executeButton.Location = new System.Drawing.Point(3, 55);
            this.executeButton.Name = "executeButton";
            this.executeButton.Size = new System.Drawing.Size(111, 46);
            this.executeButton.TabIndex = 0;
            this.executeButton.Text = "Execute";
            this.executeButton.UseVisualStyleBackColor = true;
            this.executeButton.Click += new System.EventHandler(this.executeButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 63.63636F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36.36364F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(784, 562);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.flowLayoutPanel4);
            this.flowLayoutPanel1.Controls.Add(this.flowLayoutPanel5);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(501, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(280, 273);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.Controls.Add(this.loadButton);
            this.flowLayoutPanel4.Controls.Add(this.executeButton);
            this.flowLayoutPanel4.Controls.Add(this.closeButton);
            this.flowLayoutPanel4.Controls.Add(this.captureButton);
            this.flowLayoutPanel4.Controls.Add(this.stitchButton);
            this.flowLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(117, 270);
            this.flowLayoutPanel4.TabIndex = 2;
            // 
            // loadButton
            // 
            this.loadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.loadButton.Location = new System.Drawing.Point(3, 3);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(111, 46);
            this.loadButton.TabIndex = 1;
            this.loadButton.Text = "Load";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.closeButton.Location = new System.Drawing.Point(3, 107);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(111, 46);
            this.closeButton.TabIndex = 2;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // captureButton
            // 
            this.captureButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.captureButton.Location = new System.Drawing.Point(3, 159);
            this.captureButton.Name = "captureButton";
            this.captureButton.Size = new System.Drawing.Size(111, 46);
            this.captureButton.TabIndex = 3;
            this.captureButton.Text = "Capture";
            this.captureButton.UseVisualStyleBackColor = true;
            this.captureButton.Click += new System.EventHandler(this.captureButton_Click);
            // 
            // stitchButton
            // 
            this.stitchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.stitchButton.Location = new System.Drawing.Point(3, 211);
            this.stitchButton.Name = "stitchButton";
            this.stitchButton.Size = new System.Drawing.Size(111, 46);
            this.stitchButton.TabIndex = 4;
            this.stitchButton.Text = "Stitch";
            this.stitchButton.UseVisualStyleBackColor = true;
            this.stitchButton.Click += new System.EventHandler(this.stitchButton_Click);
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.Controls.Add(this.label17);
            this.flowLayoutPanel5.Controls.Add(this.label18);
            this.flowLayoutPanel5.Controls.Add(this.greenLowerLabel);
            this.flowLayoutPanel5.Controls.Add(this.label20);
            this.flowLayoutPanel5.Controls.Add(this.greenUpperLabel);
            this.flowLayoutPanel5.Controls.Add(this.label22);
            this.flowLayoutPanel5.Controls.Add(this.label23);
            this.flowLayoutPanel5.Controls.Add(this.redLowerLabel);
            this.flowLayoutPanel5.Controls.Add(this.label25);
            this.flowLayoutPanel5.Controls.Add(this.redUpperLabel);
            this.flowLayoutPanel5.Controls.Add(this.label27);
            this.flowLayoutPanel5.Controls.Add(this.label28);
            this.flowLayoutPanel5.Controls.Add(this.blueLowerLabel);
            this.flowLayoutPanel5.Controls.Add(this.label30);
            this.flowLayoutPanel5.Controls.Add(this.blueUpperLabel);
            this.flowLayoutPanel5.Controls.Add(this.checkBox);
            this.flowLayoutPanel5.Controls.Add(this.checkButton);
            this.flowLayoutPanel5.Controls.Add(this.label19);
            this.flowLayoutPanel5.Controls.Add(this.cameraBox);
            this.flowLayoutPanel5.Location = new System.Drawing.Point(126, 3);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(145, 270);
            this.flowLayoutPanel5.TabIndex = 2;
            // 
            // label17
            // 
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label17.ForeColor = System.Drawing.Color.Lime;
            this.label17.Location = new System.Drawing.Point(3, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(142, 23);
            this.label17.TabIndex = 0;
            this.label17.Text = "      Green:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label18.Location = new System.Drawing.Point(0, 23);
            this.label18.Margin = new System.Windows.Forms.Padding(0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(88, 16);
            this.label18.TabIndex = 1;
            this.label18.Text = "Lower bound:";
            // 
            // greenLowerLabel
            // 
            this.greenLowerLabel.AutoSize = true;
            this.greenLowerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold);
            this.greenLowerLabel.Location = new System.Drawing.Point(91, 23);
            this.greenLowerLabel.Name = "greenLowerLabel";
            this.greenLowerLabel.Size = new System.Drawing.Size(32, 16);
            this.greenLowerLabel.TabIndex = 2;
            this.greenLowerLabel.Text = "255";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label20.Location = new System.Drawing.Point(0, 39);
            this.label20.Margin = new System.Windows.Forms.Padding(0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(90, 16);
            this.label20.TabIndex = 3;
            this.label20.Text = "Upper bound:";
            // 
            // greenUpperLabel
            // 
            this.greenUpperLabel.AutoSize = true;
            this.greenUpperLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold);
            this.greenUpperLabel.Location = new System.Drawing.Point(93, 39);
            this.greenUpperLabel.Name = "greenUpperLabel";
            this.greenUpperLabel.Size = new System.Drawing.Size(32, 16);
            this.greenUpperLabel.TabIndex = 4;
            this.greenUpperLabel.Text = "255";
            // 
            // label22
            // 
            this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label22.ForeColor = System.Drawing.Color.Red;
            this.label22.Location = new System.Drawing.Point(3, 55);
            this.label22.Name = "label22";
            this.label22.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.label22.Size = new System.Drawing.Size(142, 26);
            this.label22.TabIndex = 5;
            this.label22.Text = "        Red:";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label23.Location = new System.Drawing.Point(0, 81);
            this.label23.Margin = new System.Windows.Forms.Padding(0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(88, 16);
            this.label23.TabIndex = 6;
            this.label23.Text = "Lower bound:";
            // 
            // redLowerLabel
            // 
            this.redLowerLabel.AutoSize = true;
            this.redLowerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold);
            this.redLowerLabel.Location = new System.Drawing.Point(91, 81);
            this.redLowerLabel.Name = "redLowerLabel";
            this.redLowerLabel.Size = new System.Drawing.Size(32, 16);
            this.redLowerLabel.TabIndex = 7;
            this.redLowerLabel.Text = "255";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label25.Location = new System.Drawing.Point(0, 97);
            this.label25.Margin = new System.Windows.Forms.Padding(0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(90, 16);
            this.label25.TabIndex = 8;
            this.label25.Text = "Upper bound:";
            // 
            // redUpperLabel
            // 
            this.redUpperLabel.AutoSize = true;
            this.redUpperLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold);
            this.redUpperLabel.Location = new System.Drawing.Point(93, 97);
            this.redUpperLabel.Name = "redUpperLabel";
            this.redUpperLabel.Size = new System.Drawing.Size(32, 16);
            this.redUpperLabel.TabIndex = 9;
            this.redUpperLabel.Text = "255";
            // 
            // label27
            // 
            this.label27.BackColor = System.Drawing.SystemColors.Control;
            this.label27.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label27.ForeColor = System.Drawing.Color.Blue;
            this.label27.Location = new System.Drawing.Point(3, 113);
            this.label27.Name = "label27";
            this.label27.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.label27.Size = new System.Drawing.Size(142, 26);
            this.label27.TabIndex = 10;
            this.label27.Text = "        Blue:";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label28.Location = new System.Drawing.Point(0, 139);
            this.label28.Margin = new System.Windows.Forms.Padding(0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(88, 16);
            this.label28.TabIndex = 11;
            this.label28.Text = "Lower bound:";
            // 
            // blueLowerLabel
            // 
            this.blueLowerLabel.AutoSize = true;
            this.blueLowerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold);
            this.blueLowerLabel.Location = new System.Drawing.Point(91, 139);
            this.blueLowerLabel.Name = "blueLowerLabel";
            this.blueLowerLabel.Size = new System.Drawing.Size(32, 16);
            this.blueLowerLabel.TabIndex = 12;
            this.blueLowerLabel.Text = "255";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label30.Location = new System.Drawing.Point(0, 155);
            this.label30.Margin = new System.Windows.Forms.Padding(0);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(90, 16);
            this.label30.TabIndex = 13;
            this.label30.Text = "Upper bound:";
            // 
            // blueUpperLabel
            // 
            this.blueUpperLabel.AutoSize = true;
            this.blueUpperLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold);
            this.blueUpperLabel.Location = new System.Drawing.Point(93, 155);
            this.blueUpperLabel.Name = "blueUpperLabel";
            this.blueUpperLabel.Size = new System.Drawing.Size(32, 16);
            this.blueUpperLabel.TabIndex = 14;
            this.blueUpperLabel.Text = "255";
            // 
            // checkBox
            // 
            this.checkBox.Location = new System.Drawing.Point(3, 183);
            this.checkBox.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            this.checkBox.Name = "checkBox";
            this.checkBox.Size = new System.Drawing.Size(45, 20);
            this.checkBox.TabIndex = 18;
            // 
            // checkButton
            // 
            this.checkButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.checkButton.Location = new System.Drawing.Point(54, 174);
            this.checkButton.Name = "checkButton";
            this.checkButton.Size = new System.Drawing.Size(82, 41);
            this.checkButton.TabIndex = 15;
            this.checkButton.Text = "Check";
            this.checkButton.UseVisualStyleBackColor = true;
            this.checkButton.Click += new System.EventHandler(this.checkButton_Click);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.label1);
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel3);
            this.flowLayoutPanel2.Controls.Add(this.label3);
            this.flowLayoutPanel2.Controls.Add(this.greenLowerBar);
            this.flowLayoutPanel2.Controls.Add(this.label4);
            this.flowLayoutPanel2.Controls.Add(this.label6);
            this.flowLayoutPanel2.Controls.Add(this.greenUpperBar);
            this.flowLayoutPanel2.Controls.Add(this.label7);
            this.flowLayoutPanel2.Controls.Add(this.label5);
            this.flowLayoutPanel2.Controls.Add(this.label9);
            this.flowLayoutPanel2.Controls.Add(this.redLowerBar);
            this.flowLayoutPanel2.Controls.Add(this.label10);
            this.flowLayoutPanel2.Controls.Add(this.label11);
            this.flowLayoutPanel2.Controls.Add(this.redUpperBar);
            this.flowLayoutPanel2.Controls.Add(this.label12);
            this.flowLayoutPanel2.Controls.Add(this.label8);
            this.flowLayoutPanel2.Controls.Add(this.label13);
            this.flowLayoutPanel2.Controls.Add(this.blueLowerBar);
            this.flowLayoutPanel2.Controls.Add(this.label14);
            this.flowLayoutPanel2.Controls.Add(this.label15);
            this.flowLayoutPanel2.Controls.Add(this.blueUpperBar);
            this.flowLayoutPanel2.Controls.Add(this.label16);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(501, 284);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(262, 275);
            this.flowLayoutPanel2.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(254, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Setting background threshold";
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.label2);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 23);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(254, 23);
            this.flowLayoutPanel3.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.ForeColor = System.Drawing.Color.Lime;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(251, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "               Green:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(0, 50);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 1, 0, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(19, 20);
            this.label3.TabIndex = 3;
            this.label3.Text = "0";
            // 
            // greenLowerBar
            // 
            this.greenLowerBar.Location = new System.Drawing.Point(19, 49);
            this.greenLowerBar.Maximum = 264;
            this.greenLowerBar.Name = "greenLowerBar";
            this.greenLowerBar.Size = new System.Drawing.Size(202, 22);
            this.greenLowerBar.TabIndex = 4;
            this.greenLowerBar.ValueChanged += new System.EventHandler(this.greenLowerBar_ValueChanged);
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(221, 50);
            this.label4.Margin = new System.Windows.Forms.Padding(0, 1, 0, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 20);
            this.label4.TabIndex = 5;
            this.label4.Text = "255";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(0, 83);
            this.label6.Margin = new System.Windows.Forms.Padding(0, 1, 0, 12);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(19, 20);
            this.label6.TabIndex = 7;
            this.label6.Text = "0";
            // 
            // greenUpperBar
            // 
            this.greenUpperBar.Location = new System.Drawing.Point(19, 82);
            this.greenUpperBar.Maximum = 264;
            this.greenUpperBar.Name = "greenUpperBar";
            this.greenUpperBar.Size = new System.Drawing.Size(202, 22);
            this.greenUpperBar.TabIndex = 8;
            this.greenUpperBar.ValueChanged += new System.EventHandler(this.greenUpperBar_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label7.Location = new System.Drawing.Point(221, 83);
            this.label7.Margin = new System.Windows.Forms.Padding(0, 1, 0, 12);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 20);
            this.label7.TabIndex = 9;
            this.label7.Text = "255";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label5.ForeColor = System.Drawing.Color.Red;
            this.label5.Location = new System.Drawing.Point(3, 115);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(251, 20);
            this.label5.TabIndex = 1;
            this.label5.Text = "                 Red:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label9.Location = new System.Drawing.Point(0, 136);
            this.label9.Margin = new System.Windows.Forms.Padding(0, 1, 0, 12);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(19, 20);
            this.label9.TabIndex = 10;
            this.label9.Text = "0";
            // 
            // redLowerBar
            // 
            this.redLowerBar.Location = new System.Drawing.Point(19, 135);
            this.redLowerBar.Maximum = 264;
            this.redLowerBar.Name = "redLowerBar";
            this.redLowerBar.Size = new System.Drawing.Size(202, 22);
            this.redLowerBar.TabIndex = 11;
            this.redLowerBar.ValueChanged += new System.EventHandler(this.redLowerBar_ValueChanged);
            // 
            // label10
            // 
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label10.Location = new System.Drawing.Point(221, 136);
            this.label10.Margin = new System.Windows.Forms.Padding(0, 1, 0, 12);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(39, 20);
            this.label10.TabIndex = 12;
            this.label10.Text = "255";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label11.Location = new System.Drawing.Point(0, 169);
            this.label11.Margin = new System.Windows.Forms.Padding(0, 1, 0, 12);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(19, 20);
            this.label11.TabIndex = 13;
            this.label11.Text = "0";
            // 
            // redUpperBar
            // 
            this.redUpperBar.Location = new System.Drawing.Point(19, 168);
            this.redUpperBar.Maximum = 264;
            this.redUpperBar.Name = "redUpperBar";
            this.redUpperBar.Size = new System.Drawing.Size(202, 22);
            this.redUpperBar.TabIndex = 14;
            this.redUpperBar.ValueChanged += new System.EventHandler(this.redUpperBar_ValueChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label12.Location = new System.Drawing.Point(221, 169);
            this.label12.Margin = new System.Windows.Forms.Padding(0, 1, 0, 12);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(39, 20);
            this.label12.TabIndex = 15;
            this.label12.Text = "255";
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label8.ForeColor = System.Drawing.Color.Blue;
            this.label8.Location = new System.Drawing.Point(3, 201);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(251, 20);
            this.label8.TabIndex = 1;
            this.label8.Text = "                 Blue:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label13.Location = new System.Drawing.Point(0, 222);
            this.label13.Margin = new System.Windows.Forms.Padding(0, 1, 0, 12);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(19, 20);
            this.label13.TabIndex = 16;
            this.label13.Text = "0";
            // 
            // blueLowerBar
            // 
            this.blueLowerBar.Location = new System.Drawing.Point(19, 221);
            this.blueLowerBar.Maximum = 264;
            this.blueLowerBar.Name = "blueLowerBar";
            this.blueLowerBar.Size = new System.Drawing.Size(202, 22);
            this.blueLowerBar.TabIndex = 17;
            this.blueLowerBar.ValueChanged += new System.EventHandler(this.blueLowerBar_ValueChanged);
            // 
            // label14
            // 
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label14.Location = new System.Drawing.Point(221, 222);
            this.label14.Margin = new System.Windows.Forms.Padding(0, 1, 0, 12);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(39, 20);
            this.label14.TabIndex = 18;
            this.label14.Text = "255";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label15.Location = new System.Drawing.Point(0, 255);
            this.label15.Margin = new System.Windows.Forms.Padding(0, 1, 0, 12);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(19, 20);
            this.label15.TabIndex = 19;
            this.label15.Text = "0";
            // 
            // blueUpperBar
            // 
            this.blueUpperBar.Location = new System.Drawing.Point(19, 254);
            this.blueUpperBar.Maximum = 264;
            this.blueUpperBar.Name = "blueUpperBar";
            this.blueUpperBar.Size = new System.Drawing.Size(202, 22);
            this.blueUpperBar.TabIndex = 20;
            this.blueUpperBar.ValueChanged += new System.EventHandler(this.blueUpperBar_ValueChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.label16.Location = new System.Drawing.Point(221, 255);
            this.label16.Margin = new System.Windows.Forms.Padding(0, 1, 0, 12);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(39, 20);
            this.label16.TabIndex = 21;
            this.label16.Text = "255";
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "openFileDialog2";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label19.Location = new System.Drawing.Point(3, 218);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(134, 20);
            this.label19.TabIndex = 19;
            this.label19.Text = "Camera number:";
            // 
            // cameraBox
            // 
            this.cameraBox.Location = new System.Drawing.Point(45, 241);
            this.cameraBox.Margin = new System.Windows.Forms.Padding(45, 3, 3, 3);
            this.cameraBox.Name = "cameraBox";
            this.cameraBox.Size = new System.Drawing.Size(45, 20);
            this.cameraBox.TabIndex = 20;
            // 
            // SegmentationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SegmentationForm";
            this.Text = "Image segmentation for motion planning";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel5.ResumeLayout(false);
            this.flowLayoutPanel5.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button executeButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.HScrollBar greenLowerBar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.HScrollBar greenUpperBar;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.HScrollBar redLowerBar;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.HScrollBar redUpperBar;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.HScrollBar blueLowerBar;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.HScrollBar blueUpperBar;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label greenLowerLabel;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label greenUpperLabel;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label redLowerLabel;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label redUpperLabel;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label blueLowerLabel;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label blueUpperLabel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button captureButton;
        private System.Windows.Forms.Button stitchButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.Button checkButton;
        private System.Windows.Forms.TextBox checkBox;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox cameraBox;
    }
}

