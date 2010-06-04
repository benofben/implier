namespace Implied
{
    partial class FormImplied
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
            this.labelSecurity = new System.Windows.Forms.Label();
            this.comboBoxSecurity = new System.Windows.Forms.ComboBox();
            this.groupBoxConfiguration = new System.Windows.Forms.GroupBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageSpreadMatrix = new System.Windows.Forms.TabPage();
            this.textBoxConsole = new System.Windows.Forms.TextBox();
            this.tabPageConsole = new System.Windows.Forms.TabPage();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.groupBoxConfiguration.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageSpreadMatrix.SuspendLayout();
            this.tabPageConsole.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            this.SuspendLayout();
            // 
            // labelSecurity
            // 
            this.labelSecurity.AutoSize = true;
            this.labelSecurity.Location = new System.Drawing.Point(6, 32);
            this.labelSecurity.Name = "labelSecurity";
            this.labelSecurity.Size = new System.Drawing.Size(48, 13);
            this.labelSecurity.TabIndex = 0;
            this.labelSecurity.Text = "Security:";
            // 
            // comboBoxSecurity
            // 
            this.comboBoxSecurity.FormattingEnabled = true;
            this.comboBoxSecurity.Items.AddRange(new object[] {
            "CME-CL",
            "CME-GC"});
            this.comboBoxSecurity.Location = new System.Drawing.Point(60, 29);
            this.comboBoxSecurity.Name = "comboBoxSecurity";
            this.comboBoxSecurity.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSecurity.TabIndex = 1;
            // 
            // groupBoxConfiguration
            // 
            this.groupBoxConfiguration.Controls.Add(this.buttonApply);
            this.groupBoxConfiguration.Controls.Add(this.labelSecurity);
            this.groupBoxConfiguration.Controls.Add(this.comboBoxSecurity);
            this.groupBoxConfiguration.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxConfiguration.Location = new System.Drawing.Point(0, 0);
            this.groupBoxConfiguration.Name = "groupBoxConfiguration";
            this.groupBoxConfiguration.Size = new System.Drawing.Size(871, 100);
            this.groupBoxConfiguration.TabIndex = 5;
            this.groupBoxConfiguration.TabStop = false;
            this.groupBoxConfiguration.Text = "Configuration";
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(187, 27);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 23);
            this.buttonApply.TabIndex = 2;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageConsole);
            this.tabControl1.Controls.Add(this.tabPageSpreadMatrix);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 100);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(871, 515);
            this.tabControl1.TabIndex = 9;
            // 
            // tabPageSpreadMatrix
            // 
            this.tabPageSpreadMatrix.Controls.Add(this.dataGridView2);
            this.tabPageSpreadMatrix.Location = new System.Drawing.Point(4, 22);
            this.tabPageSpreadMatrix.Name = "tabPageSpreadMatrix";
            this.tabPageSpreadMatrix.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSpreadMatrix.Size = new System.Drawing.Size(863, 489);
            this.tabPageSpreadMatrix.TabIndex = 1;
            this.tabPageSpreadMatrix.Text = "Spread Matrix";
            this.tabPageSpreadMatrix.UseVisualStyleBackColor = true;
            // 
            // textBoxConsole
            // 
            this.textBoxConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxConsole.Location = new System.Drawing.Point(3, 3);
            this.textBoxConsole.Multiline = true;
            this.textBoxConsole.Name = "textBoxConsole";
            this.textBoxConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxConsole.Size = new System.Drawing.Size(857, 483);
            this.textBoxConsole.TabIndex = 1;
            this.textBoxConsole.WordWrap = false;
            // 
            // tabPageConsole
            // 
            this.tabPageConsole.Controls.Add(this.textBoxConsole);
            this.tabPageConsole.Location = new System.Drawing.Point(4, 22);
            this.tabPageConsole.Name = "tabPageConsole";
            this.tabPageConsole.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConsole.Size = new System.Drawing.Size(863, 489);
            this.tabPageConsole.TabIndex = 0;
            this.tabPageConsole.Text = "Console";
            this.tabPageConsole.UseVisualStyleBackColor = true;
            // 
            // dataGridView2
            // 
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView2.Location = new System.Drawing.Point(3, 3);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.Size = new System.Drawing.Size(857, 483);
            this.dataGridView2.TabIndex = 1;
            // 
            // FormImplied
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(871, 615);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.groupBoxConfiguration);
            this.Name = "FormImplied";
            this.Text = "Implied";
            this.groupBoxConfiguration.ResumeLayout(false);
            this.groupBoxConfiguration.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPageSpreadMatrix.ResumeLayout(false);
            this.tabPageConsole.ResumeLayout(false);
            this.tabPageConsole.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelSecurity;
        private System.Windows.Forms.ComboBox comboBoxSecurity;
        private System.Windows.Forms.GroupBox groupBoxConfiguration;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageConsole;
        private System.Windows.Forms.TextBox textBoxConsole;
        private System.Windows.Forms.TabPage tabPageSpreadMatrix;
        private System.Windows.Forms.DataGridView dataGridView2;
    }
}

