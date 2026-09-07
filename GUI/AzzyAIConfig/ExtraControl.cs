// ExtraControl.cs
//
// Programmed by Machiavellian of iRO Chaos
// Modified for Kimi-Only (2026)
//
// Description:
// This file contains partial class code for the ExtraControl class object.
// Now exclusively handles H_Extra.lua for Kimi configuration.

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace AzzyAIConfig
{
    public partial class ExtraControl : UserControl
    {
        // Storage for H_Extra.lua content
        string _hextra = "";

        // An event handler for when the contents have been changed
        public event EventHandler ExtraChanged;

        string DefaultExtraPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "H_Extra.lua");

        public ExtraControl()
        {
            // Check if the file H_Extra.lua exists
            if (File.Exists(DefaultExtraPath))
            {
                // Read the contents of H_Extra.lua
                _hextra = File.ReadAllText(DefaultExtraPath);
            }

            InitializeComponent();
        }

        public void Save()
        {
            // Kimi-only: Do NOT overwrite H_Extra.lua from GUI to avoid clobbering user customizations
            // No-op: users should manage H_Extra.lua manually
            return;
        }

        public void Save(string hfile)
        {
            // Deprecated: prevent GUI from modifying H_Extra.lua
            return;
        }

        private void ExtraControl_Load(object sender, EventArgs e)
        {
            // Check if the file H_Extra.lua exists
            if (File.Exists(DefaultExtraPath))
            {
                // Read the contents of H_Extra.lua
                _hextra = File.ReadAllText(DefaultExtraPath);
                textBox1.Text = _hextra;
            }

            // Remove the event handler from the comboBox1 SelectedIndexChanged event
            comboBox1.SelectedIndexChanged -= new EventHandler(comboBox1_SelectedIndexChanged);

            // Set the comboBox1 selected item to the first item in the list
            comboBox1.SelectedIndex = 0;

            // Add an event handler to the comboBox1 SelectedIndexChanged event
            comboBox1.SelectedIndexChanged += new EventHandler(comboBox1_SelectedIndexChanged);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Remove the event handler from the textBox1 TextChanged event
            textBox1.TextChanged -= new EventHandler(textBox1_TextChanged);

            // Check which item in comboBox1 is selected
            switch (comboBox1.SelectedIndex)
            {
                    // If the item is the first in the item list
                case 0:
                    {
                        // Kimi-only: only H_Extra.lua is supported
                        textBox1.Text = _hextra;
                    } break;

                    // if the item is the second in the item list
                case 1:
                    {
                        // Kimi-only: only H_Extra.lua is supported
                        textBox1.Text = _hextra;
                    } break;
            }

            // Add an event handler to the textBox1 TextChanged event
            textBox1.TextChanged += new EventHandler(textBox1_TextChanged);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Check if the event handler ExtraChanged is not null
            if (ExtraChanged != null)
            {
                // Fire the ExtraChanged event
                ExtraChanged(this, new EventArgs());
            }
        }
    }
}
