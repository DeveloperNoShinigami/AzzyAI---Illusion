// MainForm.cs
//
// Programmed by Machiavellian of iRO Chaos
//
// Description:
// This file contains partial code for the MainForm class object, which is
// used as the graphical user interface for the AzzyAI configuration
// utility.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
 

namespace AzzyAIConfig
{
    public partial class MainForm : Form
    {
        // Custom control for Kimi configuration (H_Config)
        // Instantiated in MainForm_Load to ensure it loads from file
        KimiConf _kconf;

        public MainForm()
        {
            // Pregenerated designer code
            InitializeComponent();
        }

        void SaveChanges()
        {
            // Save the Kimi configurations
            _kconf.Save();
            kimiTactControl1.Save();
            comboTactControl1.SaveSettings();
            extraControl1.Save();
            pvpTactControl1.Save();
            
            // Also generate H_SkillList.lua (Kimi-only) next to the EXE
            SkillList.Save(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "H_SkillList.lua"));
            // Files are saved to the current working directory (config tool folder).
            // Copying to USER_AI is manual per workflow.
        }
        

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Force create a fresh KimiConf instance that loads from file
            _kconf = new KimiConf();
            
            // Set the propertyGridKimi selected object to the Kimi configurations
            propertyGridKimi.SelectedObject = _kconf;
            
            // Load combo tactics settings
            comboTactControl1.LoadSettings();
            
            // Refresh the grid to display loaded values
            propertyGridKimi.Refresh();
        }

        private void ConfigChanged(object sender, EventArgs e)
        {
            // Check if buttonApply is not enabled
            if (!buttonApply.Enabled)
            {
                // Enable buttonApply
                buttonApply.Enabled = true;
            }

            // Check if the applySettingsToolStripMenuItem is not enabled
            if (!applySettingsToolStripMenuItem.Enabled)
            {
                // Enable applySettingsToolStripMenuItem
                applySettingsToolStripMenuItem.Enabled = true;
            }

            // Check if the revertToolStripMenuItem is not enabled
            if (!revertToolStripMenuItem.Enabled)
            {
                // Enable reverToolStripMenuItem
                revertToolStripMenuItem.Enabled = true;
            }

            // Check if the resetToDefaultsToolStripMenuItem is not enabled
            if (!resetToDefaultsToolStripMenuItem.Enabled)
            {
                // Enable resetToDefaultsToolStripMenuItem
                resetToDefaultsToolStripMenuItem.Enabled = true;
            }
        }

        private void buttonQuit_Click(object sender, EventArgs e)
        {
            // Check if buttonApply is enabled
            if (buttonApply.Enabled)
            {
                // Popup a message to check if the user would like to save the configurations
                System.Windows.Forms.DialogResult result = MessageBox.Show("Would you like to save these configuration settings?", "Message", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                // Check if the result from the message box is yes
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // Save the configuration changes
                    SaveChanges();

                    // Close the window
                    Close();
                }

                // If the result from the message box is no
                else if (result == System.Windows.Forms.DialogResult.No)
                {
                    // Close the window
                    Close();
                }
            }

            // Close the window
            Close();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            // Save the configuration changes
            SaveChanges();

            // Disable the buttonApply
            buttonApply.Enabled = false;

            // Disable the applySettingsToolStripMenuItem
            applySettingsToolStripMenuItem.Enabled = false;

            // Disable the reverToolStripMenuItem
            revertToolStripMenuItem.Enabled = false;
        }

        private void revertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Revert the Kimi configurations
            _kconf.Revert();
        }

        private void resetToDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Reset the Kimi configurations to defaults
            _kconf.SetDefaults();
        }

        private void documentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the documentation file exists
            if (System.IO.File.Exists("Documentation.pdf"))
            {
                // Start a new process to open the documentation file
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "Documentation.pdf";
                p.Start();
            }
            // If the documentation file does not exist
            else
            {
                // Popup an error message for the documentation file
                MessageBox.Show("Documentation file could not be found.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Kimi AI Config v2.0\n" +
                          "Based on Azzy AI by Dr. Azzy\n" +
                          "GUI by Machiavellian, Kimi conversion (2026)\n" +
                          "(C) 2009-2026\n\n" +
                          "Exclusive support for Kimi puppets\n" +
                          "Echoes of Morroc Server",
                          "Kimi AI Config 2.0", 
                          MessageBoxButtons.OK);
        }

        private void kimiSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create a new open file dialog box
            OpenFileDialog ofd = new OpenFileDialog();

            // Turn on validate names
            ofd.ValidateNames = true;

            // Add a filter for lua files
            ofd.Filter = "(*.lua)|*.lua";

            // Set the open file dialog box title to "Import Kimi settings."
            ofd.Title = "Import Kimi settings.";

            // Show the dialog and check if the result is OK
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Open the file
                _kconf.Open(ofd.FileName);

                // Update the propertyGridKimi values
                propertyGridKimi.Update();
            }
        }

        private void kimiTacticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create a new open file dialog box
            OpenFileDialog ofd = new OpenFileDialog();

            // Turn on validate names
            ofd.ValidateNames = true;

            // Add a filter for lua files
            ofd.Filter = "(*.lua)|*.lua";

            // Set the open file dialog box title to "Import Kimi tactics."
            ofd.Title = "Import Kimi tactics.";

            // Show the dialog and check if the result is OK
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Open the file
                kimiTactControl1.Open(ofd.FileName);
            }
        }

        private void kimiSettingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Create a new save file dialog box
            SaveFileDialog sfd = new SaveFileDialog();

            // Turn on validate names
            sfd.ValidateNames = true;

            // Set the open file dialog box title to "Export Kimi settings."
            sfd.Title = "Export Kimi settings.";

            // Add a filter for lua files
            sfd.Filter = "(*.lua)|*.lua";

            // Show the dialog and check if the result is OK
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Save the file
                _kconf.Save(sfd.FileName);
            }
        }

        private void kimiTacticsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Create a new save file dialog box
            SaveFileDialog sfd = new SaveFileDialog();

            // Turn on validate names
            sfd.ValidateNames = true;

            // Set the open file dialog box title to "Export Kimi tactics."
            sfd.Title = "Export Kimi tactics.";

            // Add a filter for lua files
            sfd.Filter = "(*.lua)|*.lua";

            // Show the dialog and check if the result is OK
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Save the file
                kimiTactControl1.Save(sfd.FileName);
            }
        }

        private void propertyGridKimi_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ConfigChanged(this, EventArgs.Empty);
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {
            // No action needed
        }

        private void kimiTactControl1_Load(object sender, EventArgs e)
        {
            // No action needed
        }

    }
}
