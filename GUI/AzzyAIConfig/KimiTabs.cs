using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace AzzyAIConfig
{
    internal sealed class KimiSettingsView : CustomTypeDescriptor
    {
        readonly KimiConf config;
        readonly int type;
        internal KimiSettingsView(KimiConf config, int type) { this.config = config; this.type = type; }
        static bool Specific(PropertyDescriptor p)
        {
            return p.Category.StartsWith("Kimi Skills") || p.Category == "Autobuff Options";
        }
        public override PropertyDescriptorCollection GetProperties() { return GetProperties(null); }
        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var list = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(config))
            {
                if (!p.IsBrowsable || p.Name.StartsWith("Combo") || KimiComboProfiles.IsAutoOption(p)) continue;
                if (type == 0) { if (!Specific(p)) list.Add(p); continue; }
                if (!Specific(p)) continue;
                string name = ((KimiTypeOptions)type).ToString();
                if (p.Category.StartsWith("Kimi Skills - ") && p.Category != "Kimi Skills - Common" && p.Category != "Kimi Skills - " + name) continue;
                if (p.Name == "OnlyAOE" && type != 2) continue;
                if (p.Name.Contains("MasterSwap") && type == 4) continue;
                if ((p.Name.Contains("WarmDef") || p.Name.Contains("Bastion")) && type != 1) continue;
                if ((p.Name.Contains("Heal")) && type != 2) continue;
                list.Add(p);
            }
            return new PropertyDescriptorCollection(list.ToArray());
        }
        public override object GetPropertyOwner(PropertyDescriptor pd) { return config; }
    }

    public partial class MainForm
    {
        KimiComboProfiles _comboProfiles;
        readonly List<PropertyGrid> kimiGrids = new List<PropertyGrid>();
        TabControl kimiTabs;
        static readonly Color[] KimiColors = { Color.Goldenrod, Color.DarkGoldenrod, Color.Teal, Color.DarkMagenta };

        static TabControl FindTabControl(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is TabControl) return (TabControl)control;
                var found = FindTabControl(control);
                if (found != null) return found;
            }
            return null;
        }

        void InitializeKimiTabs()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "H_Config.lua");
            _comboProfiles = new KimiComboProfiles(_kconf, path);
            propertyGridKimi.SelectedObject = new KimiSettingsView(_kconf, 0);
            new NavigationMapSuggestions(propertyGridKimi, delegate(string map) {
                _kconf.NavigationMap=map;
                ConfigChanged(this, EventArgs.Empty);
            });
            kimiTabs = FindTabControl(this);
            if (kimiTabs == null) return;
            var tabHandle = kimiTabs.Handle;
            foreach (TabPage page in kimiTabs.TabPages)
                if (page.Text == "Combo Tactics") page.Text = "Blueprint (Shared)";
            kimiTabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            kimiTabs.DrawItem += DrawKimiTab;
            kimiTabs.SelectedIndexChanged += delegate { RefreshKimiGrids(); };
            for (int type = 1; type <= 4; type++)
            {
                string name = ((KimiTypeOptions)type).ToString();
                Color accent = KimiColors[type - 1];
                var page = new TabPage(name) { Tag = type, BackColor = Color.White, Padding = new Padding(8) };
                var header = new Panel { Dock = DockStyle.Top, Height = 170, BackColor = Color.FromArgb(248, 247, 245) };
                header.Controls.Add(new Label { Text = name + " Kimi", ForeColor = accent, Font = new Font(Font.FontFamily, 18, FontStyle.Bold), Location = new Point(14, 15), AutoSize = true });
                header.Controls.Add(new Label { Text = "Skills and behavior for " + name + ".\nStandard combos are saved separately for this Kimi.\nShared skills use the same values across applicable tabs.\nViewing this tab does not change the selected Kimi.", Location = new Point(16, 55), AutoSize = true });
                var portrait = new KimiPortrait(name.ToLowerInvariant()) { Dock = DockStyle.Right, Width = 155 };
                header.Controls.Add(portrait);
                var sections = new TabControl { Dock = DockStyle.Fill };
                var skills = new TabPage("Skills & Behavior");
                var combo = new TabPage("Combo");
                var auto = new TabPage("Automatic Skills");
                auto.Controls.Add(CreateKimiGrid(_comboProfiles.ForAutoSkills(type)));
                var skillsGrid = CreateKimiGrid(new KimiSettingsView(_kconf, type));
                var comboGrid = CreateKimiGrid(_comboProfiles.ForType(type));
                skills.Controls.Add(skillsGrid);
                combo.Controls.Add(comboGrid);
                sections.TabPages.Add(skills);
                sections.TabPages.Add(auto);
                sections.TabPages.Add(combo);
                page.Controls.Add(sections);
                page.Controls.Add(header);
                kimiTabs.TabPages.Insert(type, page);
            }
            MinimumSize = new Size(1000, 700);
            Size = new Size(Math.Max(Width, 1100), Math.Max(Height, 780));
        }

        PropertyGrid CreateKimiGrid(object model)
        {
            var grid = new PropertyGrid { Dock = DockStyle.Fill, SelectedObject = model, PropertySort = PropertySort.Categorized };
            grid.PropertyValueChanged += delegate { ConfigChanged(this, EventArgs.Empty); };
            kimiGrids.Add(grid);
            return grid;
        }

        void RefreshKimiGrids()
        {
            propertyGridKimi.Refresh();
            foreach (var grid in kimiGrids) grid.Refresh();
        }

        void DrawKimiTab(object sender, DrawItemEventArgs e)
        {
            var page = kimiTabs.TabPages[e.Index];
            Color accent = page.Tag is int ? KimiColors[(int)page.Tag - 1] : SystemColors.ControlText;
            bool selected = e.Index == kimiTabs.SelectedIndex;
            using (var brush = new SolidBrush(selected && page.Tag is int ? accent : SystemColors.Control)) e.Graphics.FillRectangle(brush, e.Bounds);
            using (var brush = new SolidBrush(accent)) e.Graphics.FillRectangle(brush, e.Bounds.X, e.Bounds.Bottom - 3, e.Bounds.Width, 3);
            TextRenderer.DrawText(e.Graphics, page.Text, Font, e.Bounds, selected && page.Tag is int ? Color.White : accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        sealed class KimiPortrait : Control
        {
            readonly Image picture;
            readonly int cropHeight;
            public KimiPortrait(string name)
            {
                DoubleBuffered = true;
                var stream = typeof(MainForm).Assembly.GetManifestResourceStream("AzzyAIConfig.KimiArt." + name + ".png");
                if (stream != null) { using (stream) using (var source = Image.FromStream(stream)) picture = new Bitmap(source); }
                cropHeight = name == "ward" ? 343 : name == "agile" ? 217 : name == "occult" ? 277 : 169;
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if (picture == null) { TextRenderer.DrawText(e.Graphics, "Ward portrait\npending", Font, ClientRectangle, Color.DimGray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); return; }
                int height = Math.Min(cropHeight, picture.Height);
                float scale = Math.Min((Width - 12f) / picture.Width, (Height - 12f) / height);
                var dest = new Rectangle((Width - (int)(picture.Width * scale)) / 2, 6, (int)(picture.Width * scale), (int)(height * scale));
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                e.Graphics.DrawImage(picture, dest, new Rectangle(0, 0, picture.Width, height), GraphicsUnit.Pixel);
            }
            protected override void Dispose(bool disposing) { if (disposing && picture != null) picture.Dispose(); base.Dispose(disposing); }
        }
    }
}
