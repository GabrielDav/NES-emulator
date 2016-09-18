namespace NesEmulator
{
    partial class Main
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.InfoRegisters = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.InfoRegPC = new System.Windows.Forms.TextBox();
            this.InfoRegY = new System.Windows.Forms.TextBox();
            this.InfoRegX = new System.Windows.Forms.TextBox();
            this.InfoRegS = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.InfoRegA = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.InfoFlagZ = new System.Windows.Forms.TextBox();
            this.InfoFV = new System.Windows.Forms.TextBox();
            this.InfoFN = new System.Windows.Forms.TextBox();
            this.InfoFI = new System.Windows.Forms.TextBox();
            this.InfoFD = new System.Windows.Forms.TextBox();
            this.InfoFC = new System.Windows.Forms.TextBox();
            this.InfoFB = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            this.InfoRegisters.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(245, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(532, 324);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.InfoRegisters);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Location = new System.Drawing.Point(12, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(227, 205);
            this.panel2.TabIndex = 1;
            // 
            // InfoRegisters
            // 
            this.InfoRegisters.Controls.Add(this.label11);
            this.InfoRegisters.Controls.Add(this.InfoRegPC);
            this.InfoRegisters.Controls.Add(this.InfoRegY);
            this.InfoRegisters.Controls.Add(this.InfoRegX);
            this.InfoRegisters.Controls.Add(this.InfoRegS);
            this.InfoRegisters.Controls.Add(this.label12);
            this.InfoRegisters.Controls.Add(this.label10);
            this.InfoRegisters.Controls.Add(this.label9);
            this.InfoRegisters.Controls.Add(this.label8);
            this.InfoRegisters.Controls.Add(this.InfoRegA);
            this.InfoRegisters.Location = new System.Drawing.Point(89, 3);
            this.InfoRegisters.Name = "InfoRegisters";
            this.InfoRegisters.Size = new System.Drawing.Size(135, 154);
            this.InfoRegisters.TabIndex = 2;
            this.InfoRegisters.TabStop = false;
            this.InfoRegisters.Text = "Registers";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 100);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(40, 13);
            this.label11.TabIndex = 15;
            this.label11.Text = "Reg Y:";
            // 
            // InfoRegPC
            // 
            this.InfoRegPC.Location = new System.Drawing.Point(53, 123);
            this.InfoRegPC.Name = "InfoRegPC";
            this.InfoRegPC.ReadOnly = true;
            this.InfoRegPC.Size = new System.Drawing.Size(23, 20);
            this.InfoRegPC.TabIndex = 19;
            this.InfoRegPC.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // InfoRegY
            // 
            this.InfoRegY.Location = new System.Drawing.Point(53, 97);
            this.InfoRegY.Name = "InfoRegY";
            this.InfoRegY.ReadOnly = true;
            this.InfoRegY.Size = new System.Drawing.Size(23, 20);
            this.InfoRegY.TabIndex = 18;
            this.InfoRegY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // InfoRegX
            // 
            this.InfoRegX.Location = new System.Drawing.Point(53, 71);
            this.InfoRegX.Name = "InfoRegX";
            this.InfoRegX.ReadOnly = true;
            this.InfoRegX.Size = new System.Drawing.Size(23, 20);
            this.InfoRegX.TabIndex = 17;
            this.InfoRegX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // InfoRegS
            // 
            this.InfoRegS.Location = new System.Drawing.Point(53, 45);
            this.InfoRegS.Name = "InfoRegS";
            this.InfoRegS.ReadOnly = true;
            this.InfoRegS.Size = new System.Drawing.Size(23, 20);
            this.InfoRegS.TabIndex = 16;
            this.InfoRegS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 126);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(47, 13);
            this.label12.TabIndex = 15;
            this.label12.Text = "Reg PC:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 74);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(40, 13);
            this.label10.TabIndex = 15;
            this.label10.Text = "Reg X:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 48);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(40, 13);
            this.label9.TabIndex = 15;
            this.label9.Text = "Reg S:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(40, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Reg A:";
            // 
            // InfoRegA
            // 
            this.InfoRegA.Location = new System.Drawing.Point(53, 19);
            this.InfoRegA.Name = "InfoRegA";
            this.InfoRegA.ReadOnly = true;
            this.InfoRegA.Size = new System.Drawing.Size(23, 20);
            this.InfoRegA.TabIndex = 14;
            this.InfoRegA.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.InfoFlagZ);
            this.groupBox1.Controls.Add(this.InfoFV);
            this.groupBox1.Controls.Add(this.InfoFN);
            this.groupBox1.Controls.Add(this.InfoFI);
            this.groupBox1.Controls.Add(this.InfoFD);
            this.groupBox1.Controls.Add(this.InfoFC);
            this.groupBox1.Controls.Add(this.InfoFB);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(80, 199);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Flags";
            // 
            // InfoFlagZ
            // 
            this.InfoFlagZ.Location = new System.Drawing.Point(47, 175);
            this.InfoFlagZ.Name = "InfoFlagZ";
            this.InfoFlagZ.ReadOnly = true;
            this.InfoFlagZ.Size = new System.Drawing.Size(23, 20);
            this.InfoFlagZ.TabIndex = 13;
            this.InfoFlagZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // InfoFV
            // 
            this.InfoFV.Location = new System.Drawing.Point(47, 149);
            this.InfoFV.Name = "InfoFV";
            this.InfoFV.ReadOnly = true;
            this.InfoFV.Size = new System.Drawing.Size(23, 20);
            this.InfoFV.TabIndex = 12;
            this.InfoFV.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // InfoFN
            // 
            this.InfoFN.Location = new System.Drawing.Point(47, 123);
            this.InfoFN.Name = "InfoFN";
            this.InfoFN.ReadOnly = true;
            this.InfoFN.Size = new System.Drawing.Size(23, 20);
            this.InfoFN.TabIndex = 11;
            this.InfoFN.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // InfoFI
            // 
            this.InfoFI.Location = new System.Drawing.Point(47, 97);
            this.InfoFI.Name = "InfoFI";
            this.InfoFI.ReadOnly = true;
            this.InfoFI.Size = new System.Drawing.Size(23, 20);
            this.InfoFI.TabIndex = 10;
            this.InfoFI.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // InfoFD
            // 
            this.InfoFD.Location = new System.Drawing.Point(47, 71);
            this.InfoFD.Name = "InfoFD";
            this.InfoFD.ReadOnly = true;
            this.InfoFD.Size = new System.Drawing.Size(23, 20);
            this.InfoFD.TabIndex = 9;
            this.InfoFD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // InfoFC
            // 
            this.InfoFC.Location = new System.Drawing.Point(47, 45);
            this.InfoFC.Name = "InfoFC";
            this.InfoFC.ReadOnly = true;
            this.InfoFC.Size = new System.Drawing.Size(23, 20);
            this.InfoFC.TabIndex = 8;
            this.InfoFC.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // InfoFB
            // 
            this.InfoFB.Location = new System.Drawing.Point(47, 19);
            this.InfoFB.Name = "InfoFB";
            this.InfoFB.ReadOnly = true;
            this.InfoFB.Size = new System.Drawing.Size(23, 20);
            this.InfoFB.TabIndex = 7;
            this.InfoFB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 178);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(34, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "flagZ:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 152);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "flagV:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 126);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "flagN:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "flagI:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "flagD:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "flagC:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "flagB:";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(798, 352);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "Main";
            this.Text = "Main";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.Shown += new System.EventHandler(this.Main_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Main_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Main_KeyUp);
            this.panel2.ResumeLayout(false);
            this.InfoRegisters.ResumeLayout(false);
            this.InfoRegisters.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox InfoRegisters;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.TextBox InfoRegPC;
        public System.Windows.Forms.TextBox InfoRegY;
        public System.Windows.Forms.TextBox InfoRegX;
        public System.Windows.Forms.TextBox InfoRegS;
        public System.Windows.Forms.TextBox InfoRegA;
        public System.Windows.Forms.TextBox InfoFlagZ;
        public System.Windows.Forms.TextBox InfoFV;
        public System.Windows.Forms.TextBox InfoFN;
        public System.Windows.Forms.TextBox InfoFI;
        public System.Windows.Forms.TextBox InfoFD;
        public System.Windows.Forms.TextBox InfoFC;
        public System.Windows.Forms.TextBox InfoFB;
    }
}