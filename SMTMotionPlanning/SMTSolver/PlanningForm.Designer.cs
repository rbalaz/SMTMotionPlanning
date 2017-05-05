namespace SMTMotionPlanning
{
    partial class PlanningForm
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
        public System.Windows.Forms.Label TestLabel { get; }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.confirmButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.executeButton = new System.Windows.Forms.Button();
            this.runButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.widthEntryBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lengthEntryBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.startXBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.startYBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.goalXBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.goalYBox = new System.Windows.Forms.TextBox();
            this.obstacleLoader = new System.Windows.Forms.Button();
            this.curvedCheckBox = new System.Windows.Forms.CheckBox();
            this.worldLoader = new System.Windows.Forms.Button();
            this.progressLabel = new System.Windows.Forms.Label();
            this.captureButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.distanceBox = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.commandsCheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.flowLayoutPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel5, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(640, 480);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.confirmButton);
            this.flowLayoutPanel1.Controls.Add(this.resetButton);
            this.flowLayoutPanel1.Controls.Add(this.executeButton);
            this.flowLayoutPanel1.Controls.Add(this.runButton);
            this.flowLayoutPanel1.Controls.Add(this.closeButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(163, 438);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(474, 39);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // confirmButton
            // 
            this.confirmButton.AutoSize = true;
            this.confirmButton.Location = new System.Drawing.Point(3, 3);
            this.confirmButton.Name = "confirmButton";
            this.confirmButton.Size = new System.Drawing.Size(75, 36);
            this.confirmButton.TabIndex = 0;
            this.confirmButton.Text = "Confirm";
            this.confirmButton.UseVisualStyleBackColor = true;
            this.confirmButton.Click += new System.EventHandler(this.confirmButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.AutoSize = true;
            this.resetButton.Location = new System.Drawing.Point(84, 3);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(75, 36);
            this.resetButton.TabIndex = 1;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // executeButton
            // 
            this.executeButton.AutoSize = true;
            this.executeButton.Location = new System.Drawing.Point(165, 3);
            this.executeButton.Name = "executeButton";
            this.executeButton.Size = new System.Drawing.Size(112, 36);
            this.executeButton.TabIndex = 2;
            this.executeButton.Text = "Find path";
            this.executeButton.UseVisualStyleBackColor = true;
            this.executeButton.Click += new System.EventHandler(this.executeButton_Click);
            // 
            // runButton
            // 
            this.runButton.AutoSize = true;
            this.runButton.Location = new System.Drawing.Point(283, 3);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(76, 36);
            this.runButton.TabIndex = 8;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.AutoSize = true;
            this.closeButton.Location = new System.Drawing.Point(365, 3);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(88, 36);
            this.closeButton.TabIndex = 7;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.label1);
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel3);
            this.flowLayoutPanel2.Controls.Add(this.label4);
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel4);
            this.flowLayoutPanel2.Controls.Add(this.obstacleLoader);
            this.flowLayoutPanel2.Controls.Add(this.curvedCheckBox);
            this.flowLayoutPanel2.Controls.Add(this.worldLoader);
            this.flowLayoutPanel2.Controls.Add(this.progressLabel);
            this.flowLayoutPanel2.Controls.Add(this.captureButton);
            this.flowLayoutPanel2.Controls.Add(this.commandsCheckBox);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(154, 429);
            this.flowLayoutPanel2.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 36);
            this.label1.TabIndex = 1;
            this.label1.Text = "World parameters:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.label2);
            this.flowLayoutPanel3.Controls.Add(this.widthEntryBox);
            this.flowLayoutPanel3.Controls.Add(this.label3);
            this.flowLayoutPanel3.Controls.Add(this.lengthEntryBox);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 39);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(150, 69);
            this.flowLayoutPanel3.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 29);
            this.label2.TabIndex = 0;
            this.label2.Text = "Width:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // widthEntryBox
            // 
            this.widthEntryBox.Location = new System.Drawing.Point(59, 3);
            this.widthEntryBox.Name = "widthEntryBox";
            this.widthEntryBox.Size = new System.Drawing.Size(83, 24);
            this.widthEntryBox.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(3, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 29);
            this.label3.TabIndex = 4;
            this.label3.Text = "Length:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lengthEntryBox
            // 
            this.lengthEntryBox.Location = new System.Drawing.Point(74, 33);
            this.lengthEntryBox.Name = "lengthEntryBox";
            this.lengthEntryBox.Size = new System.Drawing.Size(68, 24);
            this.lengthEntryBox.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(3, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(150, 37);
            this.label4.TabIndex = 4;
            this.label4.Text = "Agent start and goal location:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.Controls.Add(this.label5);
            this.flowLayoutPanel4.Controls.Add(this.startXBox);
            this.flowLayoutPanel4.Controls.Add(this.label6);
            this.flowLayoutPanel4.Controls.Add(this.startYBox);
            this.flowLayoutPanel4.Controls.Add(this.label9);
            this.flowLayoutPanel4.Controls.Add(this.goalXBox);
            this.flowLayoutPanel4.Controls.Add(this.label10);
            this.flowLayoutPanel4.Controls.Add(this.goalYBox);
            this.flowLayoutPanel4.Location = new System.Drawing.Point(3, 151);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(150, 64);
            this.flowLayoutPanel4.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(22, 29);
            this.label5.TabIndex = 1;
            this.label5.Text = "X:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // startXBox
            // 
            this.startXBox.Location = new System.Drawing.Point(31, 3);
            this.startXBox.Name = "startXBox";
            this.startXBox.Size = new System.Drawing.Size(37, 24);
            this.startXBox.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(74, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(22, 29);
            this.label6.TabIndex = 5;
            this.label6.Text = "Y:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // startYBox
            // 
            this.startYBox.Location = new System.Drawing.Point(102, 3);
            this.startYBox.Name = "startYBox";
            this.startYBox.Size = new System.Drawing.Size(40, 24);
            this.startYBox.TabIndex = 6;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(3, 30);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(22, 29);
            this.label9.TabIndex = 7;
            this.label9.Text = "X:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // goalXBox
            // 
            this.goalXBox.Location = new System.Drawing.Point(31, 33);
            this.goalXBox.Name = "goalXBox";
            this.goalXBox.Size = new System.Drawing.Size(37, 24);
            this.goalXBox.TabIndex = 8;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(74, 30);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(22, 29);
            this.label10.TabIndex = 9;
            this.label10.Text = "Y:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // goalYBox
            // 
            this.goalYBox.Location = new System.Drawing.Point(102, 33);
            this.goalYBox.Name = "goalYBox";
            this.goalYBox.Size = new System.Drawing.Size(40, 24);
            this.goalYBox.TabIndex = 10;
            // 
            // obstacleLoader
            // 
            this.obstacleLoader.Location = new System.Drawing.Point(3, 221);
            this.obstacleLoader.Name = "obstacleLoader";
            this.obstacleLoader.Size = new System.Drawing.Size(150, 44);
            this.obstacleLoader.TabIndex = 3;
            this.obstacleLoader.Text = "Load obstacles";
            this.obstacleLoader.UseVisualStyleBackColor = true;
            this.obstacleLoader.Click += new System.EventHandler(this.obstacleLoader_Click);
            // 
            // curvedCheckBox
            // 
            this.curvedCheckBox.AutoSize = true;
            this.curvedCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.curvedCheckBox.Location = new System.Drawing.Point(3, 271);
            this.curvedCheckBox.Name = "curvedCheckBox";
            this.curvedCheckBox.Size = new System.Drawing.Size(150, 22);
            this.curvedCheckBox.TabIndex = 1;
            this.curvedCheckBox.Text = "Curved path";
            this.curvedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.curvedCheckBox.UseVisualStyleBackColor = true;
            // 
            // worldLoader
            // 
            this.worldLoader.Location = new System.Drawing.Point(3, 299);
            this.worldLoader.Name = "worldLoader";
            this.worldLoader.Size = new System.Drawing.Size(150, 44);
            this.worldLoader.TabIndex = 3;
            this.worldLoader.Text = "Load world from file";
            this.worldLoader.UseVisualStyleBackColor = true;
            this.worldLoader.Click += new System.EventHandler(this.worldLoader_Click);
            // 
            // progressLabel
            // 
            this.progressLabel.AutoSize = true;
            this.progressLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressLabel.Location = new System.Drawing.Point(3, 346);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(150, 18);
            this.progressLabel.TabIndex = 4;
            this.progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // captureButton
            // 
            this.captureButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.captureButton.Location = new System.Drawing.Point(3, 367);
            this.captureButton.Name = "captureButton";
            this.captureButton.Size = new System.Drawing.Size(150, 21);
            this.captureButton.TabIndex = 5;
            this.captureButton.Text = "Capture";
            this.captureButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.captureButton.UseVisualStyleBackColor = true;
            this.captureButton.Click += new System.EventHandler(this.captureButton_Click);
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.AutoSize = true;
            this.flowLayoutPanel5.Controls.Add(this.label8);
            this.flowLayoutPanel5.Controls.Add(this.distanceBox);
            this.flowLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel5.Location = new System.Drawing.Point(3, 438);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(154, 39);
            this.flowLayoutPanel5.TabIndex = 3;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(3, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(76, 27);
            this.label8.TabIndex = 0;
            this.label8.Text = "Distance:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // distanceBox
            // 
            this.distanceBox.Location = new System.Drawing.Point(85, 3);
            this.distanceBox.Name = "distanceBox";
            this.distanceBox.Size = new System.Drawing.Size(60, 24);
            this.distanceBox.TabIndex = 1;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Text files|*.txt|All files|*.*";
            // 
            // commandsCheckBox
            // 
            this.commandsCheckBox.AutoSize = true;
            this.commandsCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commandsCheckBox.Location = new System.Drawing.Point(3, 394);
            this.commandsCheckBox.Name = "commandsCheckBox";
            this.commandsCheckBox.Size = new System.Drawing.Size(150, 22);
            this.commandsCheckBox.TabIndex = 6;
            this.commandsCheckBox.Text = "Commands file";
            this.commandsCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.commandsCheckBox.UseVisualStyleBackColor = true;
            // 
            // PlanningForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "PlanningForm";
            this.Text = "Form1";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel4.PerformLayout();
            this.flowLayoutPanel5.ResumeLayout(false);
            this.flowLayoutPanel5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button confirmButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button executeButton;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox widthEntryBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox lengthEntryBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox startXBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox startYBox;
        private System.Windows.Forms.Button obstacleLoader;
        private System.Windows.Forms.Button worldLoader;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
        private System.Windows.Forms.TextBox distanceBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox goalXBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox goalYBox;
        private System.Windows.Forms.CheckBox curvedCheckBox;
        private System.Windows.Forms.Label progressLabel;
        private System.Windows.Forms.Button captureButton;
        private System.Windows.Forms.CheckBox commandsCheckBox;
    }
}

