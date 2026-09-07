namespace AzzyAIConfig
{
    partial class MainForm
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
            System.Windows.Forms.MenuStrip menuStrip1;
            System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem homunculusSettingsToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem homunculusTacticsToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem homunculusSettingsToolStripMenuItem1;
            System.Windows.Forms.ToolStripMenuItem homunculusTacticsToolStripMenuItem1;
            System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem documentationToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            System.Windows.Forms.TabControl tabControl1;
            System.Windows.Forms.TabPage tabPage1;
            System.Windows.Forms.TabPage tabPage3;
            System.Windows.Forms.TabPage tabPage4;
            System.Windows.Forms.TabPage tabPage5;
            System.Windows.Forms.TabPage tabPage6;
            System.Windows.Forms.Label label1;
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.applySettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.revertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToDefaultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.propertyGridKimi = new System.Windows.Forms.PropertyGrid();
            this.extraControl1 = new AzzyAIConfig.ExtraControl();
            this.kimiTactControl1 = new KimiTactControl();
            this.comboTactControl1 = new AzzyAIConfig.ComboTactControl();
            this.pvpTactControl1 = new PvpTactControl();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonQuit = new System.Windows.Forms.Button();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            homunculusSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            homunculusTacticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            homunculusSettingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            homunculusTacticsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            documentationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();

            tabPage3 = new System.Windows.Forms.TabPage();
            tabPage4 = new System.Windows.Forms.TabPage();
            tabPage5 = new System.Windows.Forms.TabPage();
            tabPage6 = new System.Windows.Forms.TabPage();
            label1 = new System.Windows.Forms.Label();
            menuStrip1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            tabPage5.SuspendLayout();
            tabPage6.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            fileToolStripMenuItem,
            editToolStripMenuItem,
            helpToolStripMenuItem});
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(624, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.BackColor = System.Drawing.SystemColors.Control;
            menuStrip1.ForeColor = System.Drawing.SystemColors.ControlText;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            importToolStripMenuItem,
            exportToolStripMenuItem,
            this.toolStripMenuItem1,
            exitToolStripMenuItem});
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F)));
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            homunculusSettingsToolStripMenuItem,
            homunculusTacticsToolStripMenuItem});
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            importToolStripMenuItem.Text = "Import";
            // 
            // homunculusSettingsToolStripMenuItem
            // 
            homunculusSettingsToolStripMenuItem.Name = "kimiSettingsToolStripMenuItem";
            homunculusSettingsToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            homunculusSettingsToolStripMenuItem.Text = "Kimi Settings...";
            homunculusSettingsToolStripMenuItem.Click += new System.EventHandler(this.kimiSettingsToolStripMenuItem_Click);
            // 
            // homunculusTacticsToolStripMenuItem
            // 
            homunculusTacticsToolStripMenuItem.Name = "kimiTacticsToolStripMenuItem";
            homunculusTacticsToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            homunculusTacticsToolStripMenuItem.Text = "Kimi Tactics...";
            homunculusTacticsToolStripMenuItem.Click += new System.EventHandler(this.kimiTacticsToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            homunculusSettingsToolStripMenuItem1,
            homunculusTacticsToolStripMenuItem1});
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            exportToolStripMenuItem.Text = "Export";
            // 
            // homunculusSettingsToolStripMenuItem1
            // 
            homunculusSettingsToolStripMenuItem1.Name = "kimiSettingsToolStripMenuItem1";
            homunculusSettingsToolStripMenuItem1.Size = new System.Drawing.Size(197, 22);
            homunculusSettingsToolStripMenuItem1.Text = "Kimi Settings...";
            homunculusSettingsToolStripMenuItem1.Click += new System.EventHandler(this.kimiSettingsToolStripMenuItem1_Click);
            // 
            // homunculusTacticsToolStripMenuItem1
            // 
            homunculusTacticsToolStripMenuItem1.Name = "kimiTacticsToolStripMenuItem1";
            homunculusTacticsToolStripMenuItem1.Size = new System.Drawing.Size(197, 22);
            homunculusTacticsToolStripMenuItem1.Text = "Kimi Tactics...";
            homunculusTacticsToolStripMenuItem1.Click += new System.EventHandler(this.kimiTacticsToolStripMenuItem1_Click);
            // 

            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(107, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += new System.EventHandler(this.buttonQuit_Click);
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applySettingsToolStripMenuItem,
            this.toolStripMenuItem3,
            this.revertToolStripMenuItem,
            this.resetToDefaultsToolStripMenuItem});
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.E)));
            editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            editToolStripMenuItem.Text = "Edit";
            // 
            // applySettingsToolStripMenuItem
            // 
            this.applySettingsToolStripMenuItem.Enabled = false;
            this.applySettingsToolStripMenuItem.Name = "applySettingsToolStripMenuItem";
            this.applySettingsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.applySettingsToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.applySettingsToolStripMenuItem.Text = "Apply Settings";
            this.applySettingsToolStripMenuItem.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(201, 6);
            // 
            // revertToolStripMenuItem
            // 
            this.revertToolStripMenuItem.Enabled = false;
            this.revertToolStripMenuItem.Name = "revertToolStripMenuItem";
            this.revertToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.revertToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.revertToolStripMenuItem.Text = "Revert";
            this.revertToolStripMenuItem.Click += new System.EventHandler(this.revertToolStripMenuItem_Click);
            // 
            // resetToDefaultsToolStripMenuItem
            // 
            this.resetToDefaultsToolStripMenuItem.Name = "resetToDefaultsToolStripMenuItem";
            this.resetToDefaultsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.resetToDefaultsToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.resetToDefaultsToolStripMenuItem.Text = "Reset to Defaults";
            this.resetToDefaultsToolStripMenuItem.Click += new System.EventHandler(this.resetToDefaultsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            documentationToolStripMenuItem,
            this.toolStripMenuItem2,
            aboutToolStripMenuItem});
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.H)));
            helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // documentationToolStripMenuItem
            // 
            documentationToolStripMenuItem.Name = "documentationToolStripMenuItem";
            documentationToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            documentationToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            documentationToolStripMenuItem.Text = "Documentation";
            documentationToolStripMenuItem.Click += new System.EventHandler(this.documentationToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(173, 6);
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            aboutToolStripMenuItem.Text = "About...";
            aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.Controls.Add(tabControl1, 0, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 1);
            tableLayoutPanel1.Controls.Add(this.buttonApply, 1, 1);
            tableLayoutPanel1.Controls.Add(this.buttonQuit, 2, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(624, 418);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // tabControl1
            // 
            tableLayoutPanel1.SetColumnSpan(tabControl1, 3);
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Controls.Add(tabPage6);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(3, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(618, 383);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(this.propertyGridKimi);
            tabPage1.Location = new System.Drawing.Point(4, 22);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            tabPage1.Size = new System.Drawing.Size(610, 357);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Kimi Settings";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // propertyGridKimi
            // 
            this.propertyGridKimi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridKimi.Location = new System.Drawing.Point(3, 3);
            this.propertyGridKimi.Name = "propertyGridKimi";
            this.propertyGridKimi.Size = new System.Drawing.Size(604, 351);
            this.propertyGridKimi.TabIndex = 0;
            this.propertyGridKimi.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridKimi_PropertyValueChanged);
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(this.kimiTactControl1);
            tabPage3.Location = new System.Drawing.Point(4, 22);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new System.Drawing.Size(610, 358);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Kimi Tactics";
            tabPage3.UseVisualStyleBackColor = true;
            tabPage3.Click += new System.EventHandler(this.tabPage3_Click);
            // 
            // kimiTactControl1
            // 
            this.kimiTactControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kimiTactControl1.Location = new System.Drawing.Point(0, 0);
            this.kimiTactControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.kimiTactControl1.MinimumSize = new System.Drawing.Size(400, 230);
            this.kimiTactControl1.Name = "kimiTactControl1";
            this.kimiTactControl1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.kimiTactControl1.Size = new System.Drawing.Size(610, 358);
            this.kimiTactControl1.TabIndex = 0;
            this.kimiTactControl1.TacticsChanged += new System.EventHandler(this.ConfigChanged);
            this.kimiTactControl1.Load += new System.EventHandler(this.kimiTactControl1_Load);
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(this.comboTactControl1);
            tabPage4.Location = new System.Drawing.Point(4, 22);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new System.Drawing.Size(610, 358);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Combo Tactics";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // comboTactControl1
            // 
            this.comboTactControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboTactControl1.Location = new System.Drawing.Point(0, 0);
            this.comboTactControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboTactControl1.Name = "comboTactControl1";
            this.comboTactControl1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.comboTactControl1.Size = new System.Drawing.Size(610, 358);
            this.comboTactControl1.TabIndex = 0;
            this.comboTactControl1.ComboSettingsChanged += new System.EventHandler(this.ConfigChanged);
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(this.pvpTactControl1);
            tabPage5.Location = new System.Drawing.Point(4, 22);
            tabPage5.Name = "tabPage5";
            tabPage5.Size = new System.Drawing.Size(610, 358);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "PVP Tactics";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // pvpTactControl1
            // 
            this.pvpTactControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pvpTactControl1.Location = new System.Drawing.Point(0, 0);
            this.pvpTactControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pvpTactControl1.MinimumSize = new System.Drawing.Size(400, 260);
            this.pvpTactControl1.Name = "pvpTactControl1";
            this.pvpTactControl1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.pvpTactControl1.Size = new System.Drawing.Size(610, 358);
            this.pvpTactControl1.TabIndex = 0;
            this.pvpTactControl1.PvpTacticsChanged += new System.EventHandler(this.ConfigChanged);
            // 
            // tabPage6
            // 
            tabPage6.Controls.Add(this.extraControl1);
            tabPage6.Location = new System.Drawing.Point(4, 22);
            tabPage6.Name = "tabPage6";
            tabPage6.Size = new System.Drawing.Size(610, 358);
            tabPage6.TabIndex = 5;
            tabPage6.Text = "Extra Options";
            tabPage6.UseVisualStyleBackColor = true;
            // 
            // extraControl1
            // 
            this.extraControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.extraControl1.Location = new System.Drawing.Point(0, 0);
            this.extraControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.extraControl1.MinimumSize = new System.Drawing.Size(312, 72);
            this.extraControl1.Name = "extraControl1";
            this.extraControl1.Size = new System.Drawing.Size(610, 358);
            this.extraControl1.TabIndex = 0;
            this.extraControl1.ExtraChanged += new System.EventHandler(this.ConfigChanged);
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(282, 397);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(162, 13);
            label1.TabIndex = 1;
            label1.Text = "Kimi AI Configuration Utility v2.0";
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonApply.Enabled = false;
            this.buttonApply.Location = new System.Drawing.Point(450, 392);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(90, 23);
            this.buttonApply.TabIndex = 2;
            this.buttonApply.Text = "Apply Settings";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // buttonQuit
            // 
            this.buttonQuit.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonQuit.Location = new System.Drawing.Point(546, 392);
            this.buttonQuit.Name = "buttonQuit";
            this.buttonQuit.Size = new System.Drawing.Size(75, 23);
            this.buttonQuit.TabIndex = 3;
            this.buttonQuit.Text = "Quit";
            this.buttonQuit.UseVisualStyleBackColor = true;
            this.buttonQuit.Click += new System.EventHandler(this.buttonQuit_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 442);
            this.Controls.Add(tableLayoutPanel1);
            this.Controls.Add(menuStrip1);
            this.MainMenuStrip = menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KimiAI - Illusion v1 (Refactor of AzzyAI)";
            this.Load += new System.EventHandler(this.MainForm_Load);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            tabPage5.ResumeLayout(false);
            tabPage6.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem applySettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem revertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToDefaultsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.PropertyGrid propertyGridKimi;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonQuit;
        private ComboTactControl comboTactControl1;
        private KimiTactControl kimiTactControl1;
        private PvpTactControl pvpTactControl1;
        private ExtraControl extraControl1;
    }
}