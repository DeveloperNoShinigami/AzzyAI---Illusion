using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace AzzyAIConfig
{
    /// <summary>
    /// Map names are used by the Lua runtime as data keys. Keep the value
    /// deliberately narrow so a hand-edited or imported settings file cannot
    /// turn it into a path or Lua expression.
    /// </summary>
    internal static class NavigationMapNames
    {
        internal const string ResourceName = "AzzyAIConfig.USER_AI.NavigationMaps.txt";

        static readonly object Sync = new object();
        static string[] _values;

        internal static bool IsValid(string value)
        {
            if (string.IsNullOrEmpty(value)) return true;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (!((c >= 'a' && c <= 'z') ||
                      (c >= 'A' && c <= 'Z') ||
                      (c >= '0' && c <= '9') ||
                      c == '_' || c == '@' || c == '-'))
                    return false;
            }
            return true;
        }

        internal static string Normalize(string value)
        {
            return IsValid(value) ? (value ?? string.Empty) : string.Empty;
        }

        internal static string[] GetValues()
        {
            lock (Sync)
            {
                if (_values == null)
                    _values = LoadValues();

                return (string[])_values.Clone();
            }
        }

        static string[] LoadValues()
        {
            var values = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            // The empty entry gives the user an explicit way to disable
            // navigation while retaining an empty Lua string on disk.
            values.Add(string.Empty);
            seen.Add(string.Empty);

            Assembly assembly = typeof(NavigationMapNames).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(ResourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                        ReadValues(reader, values, seen);
                    return values.ToArray();
                }
            }

            // This fallback keeps the designer and an unpacked development
            // build usable when the project has not yet produced an assembly.
            string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NavigationMaps.txt");
            if (File.Exists(fileName))
            {
                using (var reader = new StreamReader(fileName))
                    ReadValues(reader, values, seen);
            }
            return values.ToArray();
        }

        static void ReadValues(TextReader reader, List<string> values, HashSet<string> seen)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string mapName = line.Trim();
                // The runtime accepts this same safe subset. Ignore comments,
                // blank lines, and source entries that cannot be runtime keys.
                if (mapName.Length == 0 || mapName[0] == '#') continue;
                if (!IsValid(mapName) || !seen.Add(mapName)) continue;
                values.Add(mapName);
            }
        }
    }

    /// <summary>
    /// TypeConverter used by PropertyGrid. Standard values are suggestions,
    /// not an exclusive enum, so the user can type a map name that is present
    /// in a newer runtime data file.
    /// </summary>
    internal sealed class NavigationMapConverter : StringConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string name=value as string;
            if (name!=null && NavigationMapNames.IsValid(name)) return name;
            return base.ConvertFrom(context,culture,value);
        }
    }

    // Suggestions belong to the PropertyGrid's existing text editor.
    internal sealed class NavigationMapSuggestions
    {
        readonly PropertyGrid grid;
        readonly Action<string> select;
        readonly SuggestionWindow popup = new SuggestionWindow();
        readonly ListBox matches = new ListBox { Dock=DockStyle.Fill, IntegralHeight=false };
        TextBox editor;
        bool choosing;
        internal NavigationMapSuggestions(PropertyGrid grid, Action<string> select)
        {
            this.grid=grid; this.select=select;
            popup.Controls.Add(matches);
            Hook(grid);
            grid.SelectedGridItemChanged += delegate { popup.Hide(); };
            grid.VisibleChanged += delegate { if(!grid.Visible) popup.Hide(); };
            grid.Disposed += delegate { popup.Dispose(); };
            matches.MouseDown += delegate(object sender, MouseEventArgs e) {
                int index=matches.IndexFromPoint(e.Location);
                if(index>=0) { matches.SelectedIndex=index; Accept(); }
            };
            var owner=grid.FindForm();
            if(owner!=null) { owner.Move += delegate { popup.Hide(); }; owner.Deactivate += delegate { popup.Hide(); }; }
        }
        void Hook(Control control)
        {
            var text=control as TextBox;
            if(text!=null) {
                text.TextChanged += delegate { Update(text); };
                text.KeyDown += EditorKeyDown;
                text.LostFocus += delegate { if(!popup.IsDisposed) popup.Hide(); };
            }
            control.ControlAdded += delegate(object sender, ControlEventArgs e) { Hook(e.Control); };
            foreach(Control child in control.Controls) Hook(child);
        }
        bool IsMapField()
        {
            var item=grid.SelectedGridItem;
            return item!=null && item.PropertyDescriptor!=null && item.PropertyDescriptor.Name=="NavigationMap";
        }
        internal static string[] Filter(string query)
        {
            var found=new List<string>();
            foreach(string name in NavigationMapNames.GetValues())
                if(name.Length>0 && name.IndexOf(query,StringComparison.OrdinalIgnoreCase)>=0) found.Add(name);
            return found.ToArray();
        }
        void Update(TextBox text)
        {
            if(choosing || !text.Focused || !IsMapField()) return;
            editor=text;
            if(text.Text.Length==0) { popup.Hide(); return; }
            matches.BeginUpdate(); matches.Items.Clear(); matches.Items.AddRange(Filter(text.Text)); matches.EndUpdate();
            if(matches.Items.Count==0) { popup.Hide(); return; }
            matches.SelectedIndex=0;
            popup.Size=new Size(Math.Max(240,text.Width),Math.Min(220,matches.ItemHeight*Math.Min(matches.Items.Count,12)+4));
            var location=text.PointToScreen(new Point(0,text.Height));
            var area=Screen.FromControl(text).WorkingArea;
            location.X=Math.Min(location.X,area.Right-popup.Width);
            if(location.Y+popup.Height>area.Bottom) location.Y=text.PointToScreen(Point.Empty).Y-popup.Height;
            popup.Location=location;
            if(!popup.Visible) popup.Show(grid.FindForm());
        }
        void EditorKeyDown(object sender, KeyEventArgs e)
        {
            if(!popup.Visible || !IsMapField()) return;
            if(e.KeyCode==Keys.Escape) popup.Hide();
            else if(e.KeyCode==Keys.Enter) Accept();
            else if(e.KeyCode==Keys.Down) matches.SelectedIndex=Math.Min(matches.Items.Count-1,matches.SelectedIndex+1);
            else if(e.KeyCode==Keys.Up) matches.SelectedIndex=Math.Max(0,matches.SelectedIndex-1);
            else return;
            e.Handled=true; e.SuppressKeyPress=true;
        }
        void Accept()
        {
            if(editor==null || matches.SelectedItem==null) return;
            string name=(string)matches.SelectedItem;
            choosing=true;
            try { editor.Text=name; select(name); popup.Hide(); editor.Focus(); editor.SelectionStart=name.Length; }
            finally { choosing=false; }
        }
        sealed class SuggestionWindow : Form
        {
            internal SuggestionWindow() { FormBorderStyle=FormBorderStyle.None; ShowInTaskbar=false; StartPosition=FormStartPosition.Manual; }
            protected override bool ShowWithoutActivation { get { return true; } }
            protected override CreateParams CreateParams { get { var p=base.CreateParams; p.ExStyle|=0x08000000; return p; } }
        }
    }
}
