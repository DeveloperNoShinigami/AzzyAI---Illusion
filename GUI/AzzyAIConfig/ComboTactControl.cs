using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;

namespace AzzyAIConfig
{
    public partial class ComboTactControl : UserControl
    {
        private Dictionary<ListViewItem, ComboNode> toolboxNodeMap = new Dictionary<ListViewItem, ComboNode>();
        private readonly string defaultBlueprintPath;
        private string lastBlueprintPath;
        private string lastExportPath;  // Remember last export folder
        public event EventHandler ComboSettingsChanged;

        public ComboTactControl()
        {
            defaultBlueprintPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "combo_tactics.cbp");
            lastBlueprintPath = defaultBlueprintPath;
            
            // Initialize last export path to default USER_AI/data folder
            string defaultExportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "USER_AI", "data");
            lastExportPath = Directory.Exists(defaultExportDir) ? defaultExportDir : AppDomain.CurrentDomain.BaseDirectory;

            InitializeComponent();
            InitializeToolbox();
            SetupEventHandlers();
            ConfigurePropertyGrid();
        }

        public enum TargetModeOption
        {
            Enemy,
            Owner,
            Self,
            Ground,
            Ally,
            NearestEnemy,
            FarthestEnemy,
            StrongestEnemy,
            WeakestEnemy,
            NearestAlly,
            FarthestAlly,
            Focus,
            MobID
        }

        public enum RepeatModeOption
        {
            Once,
            Always,
            OncePerTarget,
            XTimes
        }

        public enum OperatorOption
        {
            GreaterEqual,   // >=
            LessEqual,      // <=
            Equal,          // ==
            GreaterThan,    // >
            LessThan        // <
        }

        public enum LogicTypeOption
        {
            Delay,
            Loop,
            AND,
            OR,
            Sequence
        }

        private void ConfigurePropertyGrid()
        {
            // Adjust PropertyGrid column widths for better text wrapping
            // PropertyGrid doesn't expose column widths directly in WinForms,
            // but we can control the overall width and let it auto-adjust
            propertyGrid.HelpVisible = true;
            propertyGrid.PropertySort = PropertySort.Categorized;
            
            // Use reflection to set internal column ratios (60% label, 40% value)
            try
            {
                var gridView = propertyGrid.GetType().GetField("gridView", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.NonPublic)?.GetValue(propertyGrid);
                
                if (gridView != null)
                {
                    var labelRatioProperty = gridView.GetType().GetProperty("InternalLabelWidth",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Public);
                    
                    if (labelRatioProperty != null && labelRatioProperty.CanWrite)
                    {
                        // Set label width to 55% of total width
                        int labelWidth = (int)(propertyGrid.Width * 0.55);
                        labelRatioProperty.SetValue(gridView, labelWidth, null);
                    }
                }
            }
            catch
            {
                // Fallback: PropertyGrid will use default column widths
            }
        }

        private void NotifyComboChanged()
        {
            ComboSettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void InitializeToolbox()
        {
            toolboxListView.SmallImageList = new ImageList { ImageSize = new Size(16, 16) };
            toolboxListView.OwnerDraw = true;
            toolboxListView.BackColor = Color.FromArgb(30, 30, 30);
            toolboxListView.ForeColor = Color.White;
            
            var templates = NodeLibrary.GetAllNodeTemplates();
            var groups = new Dictionary<string, ListViewGroup>();

            // Create groups ordered by importance/color
            groups["1 - Triggers"] = new ListViewGroup("1 - Triggers", HorizontalAlignment.Left);
            groups["2 - Conditions"] = new ListViewGroup("2 - Conditions", HorizontalAlignment.Left);
            groups["3 - Targets"] = new ListViewGroup("3 - Targets", HorizontalAlignment.Left);
            groups["4 - Skills (Combat)"] = new ListViewGroup("4 - Skills (Combat)", HorizontalAlignment.Left);
            groups["5 - Skills (Support)"] = new ListViewGroup("5 - Skills (Support)", HorizontalAlignment.Left);
            groups["6 - Logic/Control"] = new ListViewGroup("6 - Logic/Control", HorizontalAlignment.Left);

            foreach (var group in groups.Values)
            {
                toolboxListView.Groups.Add(group);
            }

            // Add templates to toolbox with priority grouping
            foreach (var template in templates)
            {
                string groupKey = "6 - Logic/Control";
                if (template.Type == NodeType.Trigger) groupKey = "1 - Triggers";
                else if (template.Type == NodeType.Condition) groupKey = "2 - Conditions";
                else if (template.Type == NodeType.Target) groupKey = "3 - Targets";
                else if (template.Type == NodeType.Skill)
                {
                    groupKey = template.Category == NodeCategory.SupportSkill ? "5 - Skills (Support)" : "4 - Skills (Combat)";
                }

                var item = new ListViewItem(template.Title, groups[groupKey]);
                item.Tag = template;
                // Use the actual node's header color instead of generic type color
                item.BackColor = template.HeaderColor;
                item.ForeColor = Color.White;
                toolboxListView.Items.Add(item);
                toolboxNodeMap[item] = template;
            }

            // Handle draw events for proper coloring and selection highlight
            toolboxListView.DrawItem += (s, e) =>
            {
                if (e.Item == null) return;
                
                var node = toolboxNodeMap.ContainsKey(e.Item) ? toolboxNodeMap[e.Item] : null;
                // Use the actual node's header color
                Color bgColor = node != null ? node.HeaderColor : Color.FromArgb(40, 40, 40);
                
                // Highlight selected item with brighter color
                if (e.Item.Selected)
                {
                    // Brighten the color for selection
                    bgColor = Color.FromArgb(
                        Math.Min(255, bgColor.R + 80),
                        Math.Min(255, bgColor.G + 80),
                        Math.Min(255, bgColor.B + 80)
                    );
                }
                
                e.Graphics.FillRectangle(new SolidBrush(bgColor), e.Bounds);
                
                // Draw selection border
                if (e.Item.Selected)
                {
                    using (var pen = new Pen(Color.Yellow, 2))
                    {
                        e.Graphics.DrawRectangle(pen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                    }
                }
                
                e.Graphics.DrawString(e.Item.Text, toolboxListView.Font, new SolidBrush(Color.White), e.Bounds.Left + 5, e.Bounds.Top + 3);
            };

            toolboxListView.DrawSubItem += (s, e) =>
            {
                e.DrawDefault = true;
            };
        }

        /// <summary>
        /// Get valid next node types based on current node type and output pin name.
        /// This enables SMART AUTO-SUGGESTIONS when user connects pins.
        /// </summary>
        private List<NodeType> GetValidNextNodeTypes(NodeType currentType, string outputPinName)
        {
            var valid = new List<NodeType>();

            if (currentType == NodeType.Trigger)
            {
                // After trigger's "Execute", typically goes to Condition or Skill
                valid.Add(NodeType.Condition);
                valid.Add(NodeType.Target);
                valid.Add(NodeType.Skill);
            }
            else if (currentType == NodeType.Condition)
            {
                // After condition's "True" or "False", goes to Target/Skill/Condition/Logic
                if (outputPinName == "True" || outputPinName == "False")
                {
                    valid.Add(NodeType.Target);
                    valid.Add(NodeType.Skill);
                    valid.Add(NodeType.Condition);
                    valid.Add(NodeType.Logic);
                }
            }
            else if (currentType == NodeType.Target)
            {
                // After target's "Then", goes to Skill or more Targets
                if (outputPinName == "Then")
                {
                    valid.Add(NodeType.Skill);
                    valid.Add(NodeType.Target);
                    valid.Add(NodeType.Logic);
                }
            }
            else if (currentType == NodeType.Skill)
            {
                // After skill's "Then" or "On Success", goes to Delay/Skill/Target/Condition/Logic
                if (outputPinName == "Then" || outputPinName == "On Success")
                {
                    valid.Add(NodeType.Logic);
                    valid.Add(NodeType.Skill);
                    valid.Add(NodeType.Target);
                    valid.Add(NodeType.Condition);
                }
            }
            else if (currentType == NodeType.Logic)
            {
                // After logic's "Out" or "After Delay", goes to Skill/Target/Condition/Logic
                if (outputPinName == "Out" || outputPinName == "After Delay")
                {
                    valid.Add(NodeType.Skill);
                    valid.Add(NodeType.Target);
                    valid.Add(NodeType.Logic);
                    valid.Add(NodeType.Condition);
                }
            }

            return valid;
        }

        /// <summary>
        /// Filter toolbox to show ONLY valid next nodes based on current context.
        /// Called when user starts dragging from a pin.
        /// </summary>
        public void FilterToolboxForNextNodes(ComboNode fromNode, NodePin fromPin)
        {
            if (fromNode == null || fromPin == null)
            {
                // Reset toolbox to show all nodes
                ShowAllToolboxNodes();
                return;
            }

            // Get valid next node types
            var validTypes = GetValidNextNodeTypes(fromNode.Type, fromPin.Name);
            
            // Filter toolbox
            foreach (ListViewItem item in toolboxListView.Items)
            {
                var template = toolboxNodeMap.ContainsKey(item) ? toolboxNodeMap[item] : null;
                if (template != null)
                {
                    item.ForeColor = validTypes.Contains(template.Type) ? Color.White : Color.Gray;
                }
            }

            // Show hint label (optional - can add later)
            if (validTypes.Count == 0)
            {
                MessageBox.Show("No valid next nodes from this pin!", "End of chain");
            }
        }

        /// <summary>
        /// Show all toolbox nodes (reset filter)
        /// </summary>
        public void ShowAllToolboxNodes()
        {
            foreach (ListViewItem item in toolboxListView.Items)
            {
                item.ForeColor = Color.White;
            }
        }

        private Color GetColorForNodeType(NodeType type)
        {
            switch (type)
            {
                case NodeType.Skill:
                    return Color.FromArgb(80, 20, 20);  // Dark red for skills (matching combat skills)
                case NodeType.Condition:
                    return Color.FromArgb(40, 80, 120);  // Blue for conditions
                case NodeType.Logic:
                    return Color.FromArgb(80, 60, 100);  // Purple for logic
                case NodeType.Trigger:
                    return Color.FromArgb(120, 80, 40);  // Brown/Orange for triggers
                default:
                    return Color.FromArgb(50, 50, 50);
            }
        }

        private void SetupEventHandlers()
        {
            blueprintCanvas.NodeSelected += (s, e) =>
            {
                if (e.Node != null)
                {
                    // Use type-specific wrappers to show only relevant properties
                    propertyGrid.SelectedObject = CreatePropertyWrapperForNode(e.Node);
                }
                else
                {
                    propertyGrid.SelectedObject = null;
                }
                NotifyComboChanged();
            };

            blueprintCanvas.NodeAdded += (s, e) =>
            {
                NotifyComboChanged();
            };

            blueprintCanvas.ConnectionCreated += (s, conn) =>
            {
                NotifyComboChanged();
            };

            propertyGrid.PropertyValueChanged += (s, e) => NotifyComboChanged();
        }

        private void toolboxListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is ListViewItem item && item.Tag is ComboNode template)
            {
                DoDragDrop(template, DragDropEffects.Copy);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear all nodes and connections?", "Clear Canvas", 
                              MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                blueprintCanvas.ClearAll();
                propertyGrid.SelectedObject = null;
                NotifyComboChanged();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog 
            { 
                Filter = "Combo Blueprint|*.cbp|All Files|*.*",
                DefaultExt = "cbp",
                FileName = "combo_tactics.cbp",
                InitialDirectory = Path.GetDirectoryName(defaultBlueprintPath)
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    SaveToFile(dlg.FileName);
                    MessageBox.Show("Blueprint saved successfully!", "Save", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog 
            { 
                Filter = "Combo Blueprint|*.cbp|All Files|*.*", 
                InitialDirectory = Path.GetDirectoryName(lastBlueprintPath ?? defaultBlueprintPath)
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadFromFile(dlg.FileName);
                    MessageBox.Show("Blueprint loaded successfully!", "Load", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            var lua = ExportToLua();
            
            // Log all node properties for debugging
            var nodes = blueprintCanvas.GetNodes();
            StringBuilder debugLog = new StringBuilder();
            debugLog.AppendLine("=== NODE PROPERTIES DEBUG ===");
            foreach (var node in nodes)
            {
                debugLog.AppendLine($"\n[{node.Type}] {node.Title}:");
                foreach (var prop in node.Properties)
                {
                    debugLog.AppendLine($"  {prop.Key} = {prop.Value}");
                }
            }
            System.Diagnostics.Debug.WriteLine(debugLog.ToString());

            using (var dlg = new SaveFileDialog 
            { 
                Filter = "Lua Files|*.lua|All Files|*.*",
                DefaultExt = "lua",
                FileName = "ComboTactics.lua",
                InitialDirectory = lastExportPath
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dlg.FileName, lua);
                    // Remember this folder for next export
                    lastExportPath = Path.GetDirectoryName(dlg.FileName);
                    MessageBox.Show("Exported to Lua successfully!\n\nFile: " + dlg.FileName, 
                                  "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public void SaveSettings()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "combo_tactics.cbp");
            SaveToFile(path);
        }

        public void LoadSettings()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "combo_tactics.cbp");
            if (File.Exists(path))
            {
                LoadFromFile(path);
            }
        }

        private void SaveToFile(string path)
        {
            var nodes = blueprintCanvas.GetNodes();
            
            // Convert to serializable DTO with hex colors instead of Color objects
            var serializableNodes = new List<SerializableNode>();
            foreach (var node in nodes)
            {
                var dto = new SerializableNode
                {
                    Id = node.Id,
                    Type = node.Type,
                    Category = node.Category,
                    Title = node.Title,
                    Description = "",  // Don't serialize - will regenerate on load
                    Position = node.Position,
                    Size = node.Size,
                    HeaderColorHex = node.GetHeaderColorHex(),
                    BodyColorHex = node.GetBodyColorHex(),
                    Properties = node.Properties
                };
                
                // Convert input pins to serializable format with hex colors
                foreach (var pin in node.InputPins)
                {
                    dto.InputPins.Add(new SerializablePin
                    {
                        Id = pin.Id,
                        Name = pin.Name,
                        Type = pin.Type,
                        IsInput = pin.IsInput,
                        Index = pin.Index,
                        PinColorHex = pin.GetPinColorHex(),
                        Connections = new List<Guid>(pin.Connections)
                    });
                }
                
                // Convert output pins to serializable format with hex colors
                foreach (var pin in node.OutputPins)
                {
                    dto.OutputPins.Add(new SerializablePin
                    {
                        Id = pin.Id,
                        Name = pin.Name,
                        Type = pin.Type,
                        IsInput = pin.IsInput,
                        Index = pin.Index,
                        PinColorHex = pin.GetPinColorHex(),
                        Connections = new List<Guid>(pin.Connections)
                    });
                }
                
                serializableNodes.Add(dto);
            }
            
            // Create wrapper data structure with nodes and connections (with hex colors)
            var conns = blueprintCanvas.GetConnections();
            var serializableConnections = new List<SerializableNodeConnection>();
            foreach (var conn in conns)
            {
                serializableConnections.Add(new SerializableNodeConnection
                {
                    Id = conn.Id,
                    SourceNodeId = conn.SourceNodeId,
                    SourcePinId = conn.SourcePinId,
                    TargetNodeId = conn.TargetNodeId,
                    TargetPinId = conn.TargetPinId,
                    LineColorHex = conn.GetLineColorHex()
                });
            }
            
            var data = new SerializableBlueprintData
            {
                Nodes = serializableNodes,
                SerializableConnections = serializableConnections
            };
            
            var json = new System.Web.Script.Serialization.JavaScriptSerializer();
            string jsonString = json.Serialize(data);
            
            // Format JSON for readability
            jsonString = FormatJson(jsonString);
            File.WriteAllText(path, jsonString);

            lastBlueprintPath = path;
            EnsureDefaultBlueprintCopy(path, jsonString);
        }

        private void LoadFromFile(string path)
        {
            var json = new System.Web.Script.Serialization.JavaScriptSerializer();
            var jsonText = File.ReadAllText(path);
            var data = json.Deserialize<SerializableBlueprintData>(jsonText);
            
            blueprintCanvas.ClearAll();
            
            // Dictionary to map old pin IDs to new pin IDs during migration
            var pinIdMap = new Dictionary<Guid, Guid>(); // oldPinId -> newPinId
            
            foreach (var nodeData in data.Nodes)
            {
                // Reconstruct ComboNode from serializable data
                var node = new ComboNode
                {
                    Id = nodeData.Id,
                    Type = nodeData.Type,
                    Category = nodeData.Category,
                    Title = nodeData.Title,
                    Description = "",  // Will be regenerated below based on node type
                    Position = nodeData.Position,
                    Size = nodeData.Size
                };
                
                // Regenerate description from current node library definitions
                // This ensures descriptions always match the latest program version
                node.Description = GetNodeDescriptionFromLibrary(nodeData.Type, nodeData.Title, nodeData.Properties);
                
                // Restore node colors from hex
                if (!string.IsNullOrEmpty(nodeData.HeaderColorHex))
                    node.SetHeaderColorFromHex(nodeData.HeaderColorHex);
                if (!string.IsNullOrEmpty(nodeData.BodyColorHex))
                    node.SetBodyColorFromHex(nodeData.BodyColorHex);

                // Backfill colors from template when older files had defaults
                var template = NodeLibrary.GetTemplateByTitle(node.Title);
                if (template != null)
                {
                    var defaultHeader = Color.FromArgb(60, 60, 65);
                    var defaultBody = Color.FromArgb(40, 40, 45);
                    if (string.IsNullOrEmpty(nodeData.HeaderColorHex) || node.HeaderColor.ToArgb() == defaultHeader.ToArgb())
                        node.HeaderColor = template.HeaderColor;
                    if (string.IsNullOrEmpty(nodeData.BodyColorHex) || node.BodyColor.ToArgb() == defaultBody.ToArgb())
                        node.BodyColor = template.BodyColor;
                }
                
                // Restore properties
                if (nodeData.Properties != null)
                {
                    foreach (var kvp in nodeData.Properties)
                        node.Properties[kvp.Key] = kvp.Value;
                }

                // Upgrade older skill nodes with catalog limits and target
                // defaults while preserving an explicit user override.
                if (node.Type == NodeType.Skill && node.Properties.ContainsKey("SkillID"))
                {
                    int skillId = Convert.ToInt32(node.Properties["SkillID"]);
                    if (skillId != -1)
                    {
                        int oldLevel = node.Properties.ContainsKey("SkillLevel")
                            ? Convert.ToInt32(node.Properties["SkillLevel"]) : 1;
                        node.Properties["SkillLevel"] = KimiSkills.ClampLevel(skillId, oldLevel);
                        node.Properties["SkillMaxLevel"] = KimiSkills.GetMaxLevel(skillId);
                    }
                    if (!node.Properties.ContainsKey("TargetMode"))
                        node.Properties["TargetMode"] = KimiSkills.GetDefaultTarget(skillId);
                }
                
                // MIGRATION FIX: Use current template pins instead of saved pins
                // This ensures old saved files get updated pin definitions automatically
                if (template != null)
                {
                    // Clear any stale pins and use template pins
                    node.InputPins.Clear();
                    node.OutputPins.Clear();
                    
                    // Deep copy template pins and record ID mapping
                    foreach (var templatePin in template.InputPins)
                    {
                        var newPin = new NodePin
                        {
                            Id = Guid.NewGuid(), // New ID for this instance
                            Name = templatePin.Name,
                            Type = templatePin.Type,
                            IsInput = templatePin.IsInput,
                            Index = templatePin.Index,
                            PinColor = templatePin.PinColor,
                            Connections = new List<Guid>()
                        };
                        node.InputPins.Add(newPin);
                        
                        // Map old pin IDs to new pin IDs by matching on (Name, Type, IsInput)
                        var oldPin = nodeData.InputPins.FirstOrDefault(p => 
                            p.Name == templatePin.Name && p.Type == templatePin.Type);
                        if (oldPin != null)
                        {
                            pinIdMap[oldPin.Id] = newPin.Id;
                        }
                    }
                    
                    foreach (var templatePin in template.OutputPins)
                    {
                        var newPin = new NodePin
                        {
                            Id = Guid.NewGuid(), // New ID for this instance
                            Name = templatePin.Name,
                            Type = templatePin.Type,
                            IsInput = templatePin.IsInput,
                            Index = templatePin.Index,
                            PinColor = templatePin.PinColor,
                            Connections = new List<Guid>()
                        };
                        node.OutputPins.Add(newPin);
                        
                        // Map old pin IDs to new pin IDs
                        var oldPin = nodeData.OutputPins.FirstOrDefault(p => 
                            p.Name == templatePin.Name && p.Type == templatePin.Type);
                        if (oldPin != null)
                        {
                            pinIdMap[oldPin.Id] = newPin.Id;
                        }
                    }
                }
                else
                {
                    // Fallback: No template found, use saved pins (legacy behavior)
                    foreach (var pinData in nodeData.InputPins)
                    {
                        var pin = new NodePin
                        {
                            Id = pinData.Id,
                            Name = pinData.Name,
                            Type = pinData.Type,
                            IsInput = pinData.IsInput,
                            Index = pinData.Index,
                            Connections = new List<Guid>(pinData.Connections)
                        };
                        if (!string.IsNullOrEmpty(pinData.PinColorHex))
                            pin.SetPinColorFromHex(pinData.PinColorHex);
                        node.InputPins.Add(pin);
                    }
                    
                    foreach (var pinData in nodeData.OutputPins)
                    {
                        var pin = new NodePin
                        {
                            Id = pinData.Id,
                            Name = pinData.Name,
                            Type = pinData.Type,
                            IsInput = pinData.IsInput,
                            Index = pinData.Index,
                            Connections = new List<Guid>(pinData.Connections)
                        };
                        if (!string.IsNullOrEmpty(pinData.PinColorHex))
                            pin.SetPinColorFromHex(pinData.PinColorHex);
                        node.OutputPins.Add(pin);
                    }
                }
                
                blueprintCanvas.AddNode(node);
            }
            
            // Restore connections and their colors, remapping pin IDs if necessary
            if (data.SerializableConnections != null && data.SerializableConnections.Count > 0)
            {
                var connections = new List<NodeConnection>();
                foreach (var connData in data.SerializableConnections)
                {
                    var conn = new NodeConnection
                    {
                        Id = connData.Id,
                        SourceNodeId = connData.SourceNodeId,
                        // Remap source pin ID if it was migrated, otherwise use as-is
                        SourcePinId = pinIdMap.ContainsKey(connData.SourcePinId) 
                            ? pinIdMap[connData.SourcePinId] 
                            : connData.SourcePinId,
                        TargetNodeId = connData.TargetNodeId,
                        // Remap target pin ID if it was migrated, otherwise use as-is
                        TargetPinId = pinIdMap.ContainsKey(connData.TargetPinId) 
                            ? pinIdMap[connData.TargetPinId] 
                            : connData.TargetPinId
                    };
                    if (!string.IsNullOrEmpty(connData.LineColorHex))
                        conn.SetLineColorFromHex(connData.LineColorHex);
                    connections.Add(conn);
                }
                blueprintCanvas.RestoreConnections(connections);
            }
            
            NotifyComboChanged();

            lastBlueprintPath = path;
            EnsureDefaultBlueprintCopy(path);
        }

        private void EnsureDefaultBlueprintCopy(string sourcePath, string content = null)
        {
            try
            {
                if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(defaultBlueprintPath))
                    return;

                if (string.Equals(sourcePath, defaultBlueprintPath, StringComparison.OrdinalIgnoreCase))
                    return;

                if (content != null)
                {
                    File.WriteAllText(defaultBlueprintPath, content);
                }
                else
                {
                    File.Copy(sourcePath, defaultBlueprintPath, true);
                }
            }
            catch
            {
                // Non-fatal: keep working even if we can't sync the default file
            }
        }

        private string FormatJson(string json)
        {
            int indent = 0;
            var sb = new StringBuilder();
            bool inString = false;
            bool escape = false;

            foreach (char c in json)
            {
                if (escape)
                {
                    sb.Append(c);
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    sb.Append(c);
                    escape = true;
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                    sb.Append(c);
                    continue;
                }

                if (inString)
                {
                    sb.Append(c);
                    continue;
                }

                if (c == '{' || c == '[')
                {
                    sb.Append(c);
                    sb.AppendLine();
                    indent++;
                    sb.Append(new string(' ', indent * 2));
                }
                else if (c == '}' || c == ']')
                {
                    indent--;
                    sb.AppendLine();
                    sb.Append(new string(' ', indent * 2));
                    sb.Append(c);
                }
                else if (c == ',')
                {
                    sb.Append(c);
                    sb.AppendLine();
                    sb.Append(new string(' ', indent * 2));
                }
                else if (c == ':' && !inString)
                {
                    sb.Append(c);
                    sb.Append(' ');
                }
                else if (c != ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private string ExportToLua()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- Auto-generated Combo Tactics from Blueprint Editor");
            sb.AppendLine("-- Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine();

            var nodes = blueprintCanvas.GetNodes();
            var connections = blueprintCanvas.GetConnections();

            // Find all trigger nodes to produce multiple combos in one file
            var triggerNodes = nodes.Where(n => n.Type == NodeType.Trigger).ToList();

            // Fallback: if no triggers, export a single ALWAYS combo with all nodes
            if (triggerNodes.Count == 0)
            {
                triggerNodes.Add(new ComboNode
                {
                    Id = Guid.NewGuid(),
                    Type = NodeType.Trigger,
                    Title = "Always",
                    Properties = new Dictionary<string, object> { { "TriggerType", "ALWAYS" } }
                });
            }

            int comboCounter = 0;
            foreach (var trigger in triggerNodes)
            {
                comboCounter++;
                // Compute undirected connected component containing this trigger
                var included = new HashSet<Guid>();
                included.Add(trigger.Id);
                bool changed = true;
                while (changed)
                {
                    changed = false;
                    foreach (var conn in connections)
                    {
                        bool srcIn = included.Contains(conn.SourceNodeId);
                        bool dstIn = included.Contains(conn.TargetNodeId);
                        if (srcIn && !dstIn)
                        {
                            included.Add(conn.TargetNodeId);
                            changed = true;
                        }
                        else if (dstIn && !srcIn)
                        {
                            included.Add(conn.SourceNodeId);
                            changed = true;
                        }
                    }
                }

                var comboNodes = nodes.Where(n => included.Contains(n.Id)).ToList();
                var comboConns = connections.Where(c => included.Contains(c.SourceNodeId) && included.Contains(c.TargetNodeId)).ToList();

                string baseName = !string.IsNullOrEmpty(trigger.Title) ? SanitizeName(trigger.Title) : "Combo";
                string comboGlobal = $"ComboTactics{comboCounter}";
                string comboName = $"ComboTactics_{baseName}";
                string triggerType = trigger.Properties != null && trigger.Properties.ContainsKey("TriggerType")
                    ? trigger.Properties["TriggerType"].ToString()
                    : "ALWAYS";

                sb.AppendLine($"{comboGlobal} = {{");
                sb.AppendLine($"    name = \"{comboName}\",");
                sb.AppendLine("    enabled = true,");
                sb.AppendLine($"    trigger = \"{triggerType}\",");
                sb.AppendLine("    priority = 1,");
                sb.AppendLine();
                sb.AppendLine("    nodes = {");

                // Build stable numeric ids for export (avoid GUIDs in Lua)
                var nodeIndexMap = new Dictionary<Guid, int>();
                for (int i = 0; i < comboNodes.Count; i++) nodeIndexMap[comboNodes[i].Id] = i + 1;

                // Serialize nodes for this combo
                foreach (var node in comboNodes)
                {
                    var typeStr = NodeTypeToString(node.Type);
                    int idx = nodeIndexMap[node.Id];
                    sb.Append($"        {{id = {idx}, type = \"{typeStr}\"");

                    // Add type-specific fields
                    if (node.Type == NodeType.Skill)
                    {
                        if (node.Properties.ContainsKey("SkillID")) sb.Append($", skill = \"{node.Properties["SkillID"]}\"");
                        if (node.Properties.ContainsKey("SkillLevel")) sb.Append($", level = {node.Properties["SkillLevel"]}");
                        if (node.Properties.ContainsKey("RepeatCount")) sb.Append($", repeatCount = {node.Properties["RepeatCount"]}");
                        if (node.Properties.ContainsKey("TargetMode")) sb.Append($", targetMode = \"{node.Properties["TargetMode"]}\"");
                        if (node.Properties.ContainsKey("SelectionMode")) sb.Append($", target = \"{node.Properties["SelectionMode"]}\"");
                    }
                    else if (node.Type == NodeType.Condition)
                    {
                        if (node.Properties.ContainsKey("ConditionType")) sb.Append($", condition = \"{node.Properties["ConditionType"]}\"");
                        if (node.Properties.ContainsKey("Value")) sb.Append($", value = {node.Properties["Value"]}");
                        if (node.Properties.ContainsKey("Operator")) sb.Append($", op = \"{node.Properties["Operator"]}\"");
                    }
                    else if (node.Type == NodeType.Logic)
                    {
                        string logicOp = "Delay";
                        if (node.Properties.ContainsKey("LogicType") && node.Properties["LogicType"] != null)
                            logicOp = node.Properties["LogicType"].ToString();
                        else if (node.Properties.ContainsKey("LoopCount"))
                            logicOp = "Loop";
                        sb.Append($", op = \"{logicOp}\"");

                        if (node.Properties.ContainsKey("LoopCount") && node.Properties["LoopCount"] != null)
                            sb.Append($", loopCount = {node.Properties["LoopCount"]}");
                        if (node.Properties.ContainsKey("DelayMs") && node.Properties["DelayMs"] != null)
                            sb.Append($", duration = {node.Properties["DelayMs"]}");
                    }
                    else if (node.Type == NodeType.Trigger)
                    {
                        if (node.Properties.ContainsKey("TriggerType")) sb.Append($", trigger = \"{node.Properties["TriggerType"]}\"");
                        if (node.Properties.ContainsKey("Value")) sb.Append($", value = {node.Properties["Value"]}");
                    }
                    else if (node.Type == NodeType.Target)
                    {
                        if (node.Properties.ContainsKey("TargetMode")) sb.Append($", targetMode = \"{node.Properties["TargetMode"]}\"");
                        if (node.Properties.ContainsKey("MobID") && (int)node.Properties["MobID"] > 0) sb.Append($", mobID = {node.Properties["MobID"]}");
                    }
                    else if (node.Type == NodeType.Output)  // End nodes
                    {
                        if (node.Properties.ContainsKey("RepeatMode")) sb.Append($", repeatMode = \"{node.Properties["RepeatMode"]}\"");
                        if (node.Properties.ContainsKey("RepeatCount")) sb.Append($", repeatCount = {node.Properties["RepeatCount"]}");
                    }

                    sb.AppendLine("},");
                }

                sb.AppendLine("    },");
                sb.AppendLine();
                sb.AppendLine("    connections = {");

                // Export ALL connections without deduplication
                // Allow multiple connections between same node pair (e.g., execution AND data pins)
                foreach (var conn in comboConns)
                {
                    if (!nodeIndexMap.ContainsKey(conn.SourceNodeId) || !nodeIndexMap.ContainsKey(conn.TargetNodeId))
                        continue;

                    int fromId = nodeIndexMap[conn.SourceNodeId];
                    int toId = nodeIndexMap[conn.TargetNodeId];
                    string fromPin = GetPinName(comboNodes, conn.SourceNodeId, conn.SourcePinId);
                    string toPin = GetPinName(comboNodes, conn.TargetNodeId, conn.TargetPinId);

                    sb.Append("        {from = " + fromId + ", to = " + toId);
                    if (!string.IsNullOrEmpty(fromPin)) sb.Append(", fromPin = \"" + fromPin + "\"");
                    if (!string.IsNullOrEmpty(toPin)) sb.Append(", toPin = \"" + toPin + "\"");
                    sb.AppendLine("},");
                }

                sb.AppendLine("    }");
                sb.AppendLine("}");
                sb.AppendLine();
            }

            // No return value needed; the runtime will read ComboTactics1/2/... globals
            return sb.ToString();
        }

        private string SanitizeName(string name)
        {
            var sb = new StringBuilder();
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-') sb.Append(c);
                else sb.Append('_');
            }
            return sb.ToString();
        }

        private string NodeTypeToString(NodeType t)
        {
            switch (t)
            {
                case NodeType.Skill: return "skill";
                case NodeType.Condition: return "condition";
                case NodeType.Logic: return "logic";
                case NodeType.Trigger: return "trigger";
                case NodeType.Target: return "target";
                case NodeType.Output: return "end";
                default: return "unknown";
            }
        }

        private string GetPinName(List<ComboNode> nodes, Guid nodeId, Guid pinId)
        {
            var node = nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null) return string.Empty;
            var pin = node.InputPins.Concat(node.OutputPins).FirstOrDefault(p => p.Id == pinId);
            return pin != null ? pin.Name : string.Empty;
        }

        private List<string> TraverseExecutionGraph(ComboNode startNode, 
                                                    List<ComboNode> allNodes, 
                                                    List<NodeConnection> allConnections)
        {
            var code = new List<string>();
            
            // Find outgoing execution connections
            var execPin = startNode.OutputPins.FirstOrDefault(p => p.Type == PinType.Execution || 
                                                                   p.Type == PinType.Trigger);
            if (execPin == null) return code;

            var outgoing = allConnections.Where(c => c.SourcePinId == execPin.Id).ToList();
            
            foreach (var conn in outgoing)
            {
                var targetNode = allNodes.FirstOrDefault(n => n.Id == conn.TargetNodeId);
                if (targetNode == null) continue;

                // Generate code based on node type
                switch (targetNode.Type)
                {
                    case NodeType.Skill:
                        int skillId = (int)targetNode.Properties["SkillID"];
                        int skillLevel = (int)targetNode.Properties["SkillLevel"];
                        int repeatCount = (int)targetNode.Properties["RepeatCount"];
                        
                        for (int i = 0; i < repeatCount; i++)
                        {
                            code.Add(GenerateSkillCall(skillId, skillLevel));
                            code.Add($"-- Wait for skill animation");
                        }
                        break;

                    case NodeType.Condition:
                        string condType = (string)targetNode.Properties["ConditionType"];
                        string op = (string)targetNode.Properties["Operator"];
                        int value = (int)targetNode.Properties["Value"];
                        
                        // Special handling for range check
                        if (targetNode.Title == "Check Range")
                        {
                            int minRange = (int)targetNode.Properties["MinRange"];
                            int maxRange = (int)targetNode.Properties["MaxRange"];
                            code.Add($"if {GenerateRangeCheck(minRange, maxRange)} then");
                        }
                        else
                        {
                            code.Add($"if {GenerateConditionCode(condType, op, value)} then");
                        }
                        // Recursively process "True" branch
                        var truePin = targetNode.OutputPins.FirstOrDefault(p => p.Name == "True");
                        if (truePin != null)
                        {
                            var trueBranch = TraverseExecutionFromPin(truePin, allNodes, allConnections);
                            foreach (var line in trueBranch)
                            {
                                code.Add("    " + line);
                            }
                        }
                        code.Add("else");
                        // Process "False" branch
                        var falsePin = targetNode.OutputPins.FirstOrDefault(p => p.Name == "False");
                        if (falsePin != null)
                        {
                            var falseBranch = TraverseExecutionFromPin(falsePin, allNodes, allConnections);
                            foreach (var line in falseBranch)
                            {
                                code.Add("    " + line);
                            }
                        }
                        code.Add("end");
                        break;

                    case NodeType.Logic:
                        string logicType = (string)targetNode.Properties["LogicType"];
                        
                        // Check if this is a Combo Delay node
                        if (targetNode.Properties.ContainsKey("UseComboDelay") && 
                            (bool)targetNode.Properties["UseComboDelay"])
                        {
                            int delayMs = (int)targetNode.Properties["DelayMs"];
                            code.Add(GenerateDelayCode(delayMs));
                        }
                        else if (logicType == "Loop")
                        {
                            int loopCount = (int)targetNode.Properties["LoopCount"];
                            code.Add($"for i = 1, {loopCount} do");
                            
                            var loopPin = targetNode.OutputPins.FirstOrDefault(p => p.Name == "Loop Body");
                            if (loopPin != null)
                            {
                                var loopBody = TraverseExecutionFromPin(loopPin, allNodes, allConnections);
                                foreach (var line in loopBody)
                                {
                                    code.Add("    " + line);
                                }
                            }
                            code.Add("end");
                        }
                        else if (logicType == "Delay")
                        {
                            int delayMs = (int)targetNode.Properties["DelayMs"];
                            code.Add($"-- Wait {delayMs}ms");
                        }
                        break;
                }

                // Continue to next node in execution chain
                var nextPin = targetNode.OutputPins.FirstOrDefault(p => p.Type == PinType.Execution && 
                                                                        (p.Name == "Then" || p.Name == "Out"));
                if (nextPin != null)
                {
                    var nextCode = TraverseExecutionFromPin(nextPin, allNodes, allConnections);
                    code.AddRange(nextCode);
                }
            }

            return code;
        }

        private List<string> TraverseExecutionFromPin(NodePin pin, 
                                                      List<ComboNode> allNodes, 
                                                      List<NodeConnection> allConnections)
        {
            var code = new List<string>();
            var connections = allConnections.Where(c => c.SourcePinId == pin.Id).ToList();
            
            foreach (var conn in connections)
            {
                var targetNode = allNodes.FirstOrDefault(n => n.Id == conn.TargetNodeId);
                if (targetNode != null)
                {
                    // Recursively process this node
                    // (simplified - would need full traversal logic)
                    code.Add($"-- Execute node: {targetNode.Title}");
                }
            }

            return code;
        }

        private string GenerateConditionCode(string condType, string op, int value)
        {
            // Generate Lua condition expressions using RO AI API
            switch (condType)
            {
                case "KimiHP":
                    return $"(GetV(V_HP, myid) * 100 / GetV(V_MAXHP, myid)) {op} {value}";
                case "OwnerHP":
                    var ownerExpr = "GetV(V_OWNER, myid)";
                    return $"(GetV(V_HP, {ownerExpr}) * 100 / GetV(V_MAXHP, {ownerExpr})) {op} {value}";
                case "KimiSP":
                    return $"(GetV(V_SP, myid) * 100 / GetV(V_MAXSP, myid)) {op} {value}";
                case "OwnerSP":
                    var ownerSP = "GetV(V_OWNER, myid)";
                    return $"(GetV(V_SP, {ownerSP}) * 100 / GetV(V_MAXSP, {ownerSP})) {op} {value}";
                case "HomuType":
                    return $"GetV(V_HOMUNTYPE, myid) {op} {value}";
                case "MobCount":
                    return $"GetNearbyMobCount(myid, 7) {op} {value}";
                case "Distance":
                    return $"GetDistance2(myid, target) {op} {value}";
                case "InCombat":
                    return "(MyState == CHASE_ST or MyState == ATTACK_ST)";
                case "Cooldown":
                    return $"(AutoSkillCooldown[{value}] > GetTick())";
                default:
                    return "true";
            }
        }

        private string GenerateSkillCall(int skillId, int skillLevel, string targetVar = "target")
        {
            // Use the runtime dispatcher so self and ground skills use their catalog target mode.
            if (skillId == -1)
            {
                return $"Attack(myid, {targetVar})";
            }

            return $"DoSkill({skillId}, {skillLevel}, {targetVar})";
        }

        private string GenerateDelayCode(int delayMs)
        {
            // Use GetTick() delay pattern from AI_main.lua
            return $"local waitUntil = GetTick() + {delayMs}; while GetTick() < waitUntil do end";
        }

        private string GenerateRangeCheck(int minRange, int maxRange, string targetVar = "target")
        {
            return $"local dist = GetDistance2(myid, {targetVar}); (dist >= {minRange} and dist <= {maxRange})";
        }

        private string GetNodeDescriptionFromLibrary(NodeType nodeType, string nodeTitle, Dictionary<string, object> properties)
        {
            // Regenerate description based on current node library definitions
            // This ensures loaded combos always have up-to-date descriptions
            if (nodeType == NodeType.Skill)
            {
                if (properties != null && properties.ContainsKey("SkillID"))
                {
                    int skillId = Convert.ToInt32(properties["SkillID"]);
                    var skill = KimiSkills.GetDefinition(skillId);
                    if (skill != null)
                        return skill.Name + " (" + skill.Id + ") - " + skill.Type +
                               ". Default target: " + skill.DefaultTarget +
                               ". Valid levels: 1-" + skill.MaxLevel + ".";
                }
                // Use NodeLibrary skill descriptions
                if (nodeTitle == "Illusion of Claws")
                    return "NEXT: Set RepeatCount 2-3 for auto-attack chains. Connect to Delay node for spam timing (200-300ms). Best for Agile/Raging Kimi.";
                else if (nodeTitle == "Illusion of Breath")
                    return "NEXT: Combo with Claws for mixed damage. Use after Crusher to finish low-HP targets. Occult Kimi preferred.";
                else if (nodeTitle == "Illusion Crusher")
                    return "NEXT: Lead with this for burst, follow with Claws chain. Great in Chase phase. Requires Cordial intimacy.";
                else if (nodeTitle == "Illusion of Light")
                    return "NEXT: Gate with 'Mob Count >= 2' condition to trigger only on multiple enemies. Add Delay 1000-2000ms to prevent overkill. Requires Cordial intimacy.";
                else if (nodeTitle == "Chaotic Heal")
                    return "NEXT: Gate with 'Kimi HP <= 50%' to trigger emergency healing. Connect output to another skill for recovery rotation.";
                else if (nodeTitle == "Warm Def")
                    return "NEXT: Pair with healing node. Use in sequences after damage phases. Good for tank builds (Ward Kimi).";
                else if (nodeTitle == "Body Double")
                    return "NEXT: Gate with 'Owner HP <= 30%' for emergency saves. Requires Loyal intimacy. Use as fallback, not primary.";
                else if (nodeTitle == "Master Swap")
                    return "NEXT: Use in Chase phase combos for tactical repositioning. Not required for most builds.";
                else if (nodeTitle == "Auto-Attack")
                    return "NEXT: Use as filler between skill casts or delay nodes. No SP cost. Good for sustained offense chains.";
            }
            else if (nodeType == NodeType.Condition)
            {
                if (nodeTitle == "Kimi HP %")
                    return "NEXT: Use >= to trigger healing (e.g., >= 50% is low threshold). Chain to Chaotic Heal or Warm Def nodes. Common gate: <= 30% for emergency skills.";
                else if (nodeTitle == "Owner HP %")
                    return "NEXT: Gate support skills when master is in danger. Use >= 50% to assist owner, <= 30% for emergency saves. Pair with Body Double as fallback.";
                else if (nodeTitle == "Kimi SP %")
                    return "NEXT: Prevent skill spam when SP is low. Use >= 60% to ensure enough SP for costly skills. Pair with Loop nodes to control casting frequency.";
                else if (nodeTitle == "Owner SP %")
                    return "NEXT: Synchronize with master's rotation. Gate AoE skills to when master has sufficient SP. Useful for coordinated builds.";
                else if (nodeTitle == "Mob Count")
                    return "NEXT: Gate AoE skills (Illusion of Light) to trigger only with multiple enemies >= 2. Use <= 1 for solo-target only skills.";
                else if (nodeTitle == "Distance to Target")
                    return "NEXT: Gate melee skills (<= 3) vs ranged (>= 5). Prevent Crusher spam at distance. Create range-based skill chains.";
                else if (nodeTitle == "Skill on Cooldown")
                    return "NEXT: Prevent skill spam and cooldown issues. Pair with Delay nodes to add safety margins (check before next skill cast).";
                else if (nodeTitle == "In Combat")
                    return "NEXT: Gate aggressive combos to active combat only. Use FALSE path for idle/buff skills. Best practice: aggressive skills only when In Combat = TRUE.";
                else if (nodeTitle == "Kimi Type")
                    return "NEXT: Create type-specific combos. Ward combo separate from Occult. Use == operator. Select type in properties (1=Ward, 2=Occult, 3=Agile, 4=Raging).";
            }
            else if (nodeType == NodeType.Logic)
            {
                if (nodeTitle == "AND")
                    return "NEXT: Both inputs must be TRUE. Use to combine conditions: Low HP AND In Combat = heal during fights only. Drag condition pins to both inputs.";
                else if (nodeTitle == "OR")
                    return "NEXT: Either input is TRUE. Use for fallbacks: Low HP OR Owner Hit = execute protection. Great for multi-trigger combos.";
                else if (nodeTitle == "Sequence")
                    return "NEXT: Execute inputs in strict order. Use for time-sensitive chains: Crusher -> Claws -> Heal. Each step must complete before next.";
                else if (nodeTitle == "Loop N Times")
                    return "NEXT: Repeat execution N times. Set LoopCount 2-5 in properties. Use for spam chains (Claws x3). Caution: values > 10 can cause lag.";
                else if (nodeTitle == "Wait Delay")
                    return "NEXT: Add pause before next skill. Set DelayMs: 100-300ms for spam, 500-1000ms for spacing, 2000+ for long buffs. Prevents action overlap.";
            }
            else if (nodeType == NodeType.Trigger)
            {
                if (nodeTitle == "On Attack State")
                    return "NEXT: Branch offensive skill chains here. Common path: Attack -> Condition checks -> Skill nodes. Use for primary damage rotation.";
                else if (nodeTitle == "On Chase State")
                    return "NEXT: Gate AoE skills and mobility here. Typical chain: Chase -> Mob Count >= 2 -> Illusion of Light. Different from idle behavior.";
                else if (nodeTitle == "On Idle State")
                    return "NEXT: Create buff refresh loops here. Pattern: Idle -> Warm Def -> Delay 5000ms -> Loop back. Good for passive maintenance.";
                else if (nodeTitle == "On Target Change")
                    return "NEXT: Reset or adapt combo for new enemy. Use to switch from single-target to AoE strategies. Prevents locked-in tactics on new mobs.";
                else if (nodeTitle == "On Owner Damage")
                    return "NEXT: Create emergency protection here. Chain: Owner Damage -> Body Double OR Chaotic Heal. Priority: save owner before continuing offense.";
            }
            
            return "Node description will appear here.";
        }

        public enum KimiType
        {
            Ward = 1,
            Occult = 2,
            Agile = 3,
            Raging = 4
        }

        // Specialized wrapper only for Kimi Type condition nodes
        private class KimiNodePropertyWrapper : NodePropertyWrapper
        {
            public KimiNodePropertyWrapper(ComboNode n) : base(n) { }

            [System.ComponentModel.Category("Condition")]
            [System.ComponentModel.Description("Select Kimi variant to gate this combo: Ward (1) = tank build, Occult (2) = magic/healing, Agile (3) = ASPD physical, Raging (4) = balanced offense. Use with '==' operator.")]
            public KimiType Kimi
            {
                get { return (KimiType)GetInt("Value", 1); }
                set { node.Properties["Value"] = (int)value; }
            }
        }

        // End Node Property Wrapper - Shows: RepeatMode (dropdown), RepeatCount
        private class EndNodePropertyWrapper : NodePropertyWrapper
        {
            public EndNodePropertyWrapper(ComboNode n) : base(n) { }

            [System.ComponentModel.Category("Combo Execution")]
            [System.ComponentModel.Description("How often this combo re-executes: Once (run once per trigger), Always (unlimited repeats), OncePerTarget (run once per new target), XTimes (run X times then stop).")]
            [System.ComponentModel.Browsable(true)]
            public RepeatModeOption RepeatMode
            {
                get
                {
                    var raw = GetString("RepeatMode", "Once");
                    RepeatModeOption val;
                    return Enum.TryParse(raw, true, out val) ? val : RepeatModeOption.Once;
                }
                set { node.Properties["RepeatMode"] = value.ToString(); }
            }

            [System.ComponentModel.Category("Combo Execution")]
            [System.ComponentModel.Description("Number of times to execute combo when RepeatMode=XTimes. Examples: 1=once, 2=twice, 5=five times. Ignored for other repeat modes.")]
            [System.ComponentModel.Browsable(true)]
            public int RepeatCount
            {
                get { return GetInt("RepeatCount", 1); }
                set { node.Properties["RepeatCount"] = value; }
            }
        }

        // Skill Node Property Wrapper - Shows the catalog level limit and
        // target default for each active Kimi skill.
        private class SkillNodePropertyWrapper : NodePropertyWrapper
        {
            public SkillNodePropertyWrapper(ComboNode n) : base(n) { }

            [System.ComponentModel.Category("Skill")]
            [System.ComponentModel.Description("Skill level. The valid range is determined by this skill's KIMI_SKILLS maximum; level 0 is disabled through the Kimi settings enabled flag.")]
            [System.ComponentModel.Browsable(true)]
            public new int SkillLevel
            {
                get
                {
                    // Hide for Auto-Attack (SkillID = -1)
                    int skillId = GetInt("SkillID", -1);
                    return skillId != -1 ? KimiSkills.ClampLevel(skillId,
                        GetInt("SkillLevel", 1)) : 0;
                }
                set
                {
                    int skillId = GetInt("SkillID", -1);
                    if (skillId != -1) node.Properties["SkillLevel"] = KimiSkills.ClampLevel(skillId, value);
                }
            }

            [System.ComponentModel.Category("Skill")]
            [System.ComponentModel.Description("Maximum level from KIMI_SKILLS.txt. Read-only metadata for this skill node.")]
            [System.ComponentModel.Browsable(true)]
            [System.ComponentModel.ReadOnly(true)]
            public int MaxLevel
            {
                get { return KimiSkills.GetMaxLevel(GetInt("SkillID", -1)); }
            }

            [System.ComponentModel.Category("Targeting")]
            [System.ComponentModel.Description("Default target for this active skill: Enemy, Owner, Self, or Ground. Change only when the combo intentionally overrides the catalog default.")]
            [System.ComponentModel.Browsable(true)]
            public new TargetModeOption TargetMode
            {
                get
                {
                    string fallback = KimiSkills.GetDefaultTarget(GetInt("SkillID", -1));
                    var raw = GetString("TargetMode", fallback);
                    TargetModeOption val;
                    return Enum.TryParse(raw, true, out val) ? val : ParseTargetMode(fallback);
                }
                set { node.Properties["TargetMode"] = value.ToString(); }
            }

            static TargetModeOption ParseTargetMode(string target)
            {
                TargetModeOption result;
                return Enum.TryParse(target, true, out result) ? result : TargetModeOption.Enemy;
            }

            [System.ComponentModel.Category("Skill")]
            [System.ComponentModel.Description("Times to execute this skill/attack in sequence. Use 1 for single cast, 2-5 for spam chains (e.g., Claws x3).")]
            [System.ComponentModel.Browsable(true)]
            public new int RepeatCount
            {
                get { return GetInt("RepeatCount", 1); }
                set { node.Properties["RepeatCount"] = value; }
            }
        }

        // Condition Node Property Wrapper - Shows: ConditionType, Operator, Value
        private class ConditionNodePropertyWrapper : NodePropertyWrapper
        {
            public ConditionNodePropertyWrapper(ComboNode n) : base(n) { }

            [System.ComponentModel.Category("Condition")]
            [System.ComponentModel.Description("What condition to check: KimiHP, OwnerHP, KimiSP, OwnerSP, MobCount, Distance, etc.")]
            [System.ComponentModel.Browsable(true)]
            public new string ConditionType
            {
                get { return GetString("ConditionType", "KimiHP"); }
                set { node.Properties["ConditionType"] = value; }
            }

            [System.ComponentModel.Category("Condition")]
            [System.ComponentModel.Description("Comparison operator: >= (at least), <= (at most), == (exact), > (more than), < (less than).")]
            [System.ComponentModel.Browsable(true)]
            public OperatorOption Operator
            {
                get
                {
                    var raw = GetString("Operator", ">=");
                    switch (raw)
                    {
                        case ">=": return OperatorOption.GreaterEqual;
                        case "<=": return OperatorOption.LessEqual;
                        case "==": return OperatorOption.Equal;
                        case ">": return OperatorOption.GreaterThan;
                        case "<": return OperatorOption.LessThan;
                        default: return OperatorOption.GreaterEqual;
                    }
                }
                set
                {
                    string str = "";
                    switch (value)
                    {
                        case OperatorOption.GreaterEqual: str = ">="; break;
                        case OperatorOption.LessEqual: str = "<="; break;
                        case OperatorOption.Equal: str = "=="; break;
                        case OperatorOption.GreaterThan: str = ">"; break;
                        case OperatorOption.LessThan: str = "<"; break;
                    }
                    node.Properties["Operator"] = str;
                }
            }

            [System.ComponentModel.Category("Condition")]
            [System.ComponentModel.Description("Threshold value for condition. Examples: HP%=30 (heal when low), MobCount=3 (AoE trigger).")]
            [System.ComponentModel.Browsable(true)]
            public new int Value
            {
                get { return GetInt("Value", 50); }
                set { node.Properties["Value"] = value; }
            }
        }

        // Logic Node Property Wrapper - Shows: LogicType, DelayMs (if Delay), LoopCount (if Loop)
        private class LogicNodePropertyWrapper : NodePropertyWrapper
        {
            public LogicNodePropertyWrapper(ComboNode n) : base(n) { }

            [System.ComponentModel.Category("Logic")]
            [System.ComponentModel.Description("Logic operation: Delay (wait time), Loop (repeat count), AND (both true), OR (either true), Sequence (execute in order).")]
            [System.ComponentModel.Browsable(true)]
            public LogicTypeOption LogicType
            {
                get
                {
                    var raw = GetString("LogicType", "Delay");
                    LogicTypeOption val;
                    return Enum.TryParse(raw, true, out val) ? val : LogicTypeOption.Delay;
                }
                set { node.Properties["LogicType"] = value.ToString(); }
            }

            [System.ComponentModel.Category("Logic")]
            [System.ComponentModel.Description("Milliseconds to wait. 100-500ms = skill spam timing, 1000ms = 1 second for buffs, 2000-5000ms = long cooldowns.")]
            [System.ComponentModel.Browsable(true)]
            public new int DelayMs
            {
                get
                {
                    var logicType = LogicType;
                    return logicType == LogicTypeOption.Delay ? GetInt("DelayMs", 500) : 0;
                }
                set
                {
                    var logicType = LogicType;
                    if (logicType == LogicTypeOption.Delay) node.Properties["DelayMs"] = value;
                }
            }

            [System.ComponentModel.Category("Logic")]
            [System.ComponentModel.Description("How many times to repeat the loop. Use 2-5 for skill chains, 10+ for sustained barrage.")]
            [System.ComponentModel.Browsable(true)]
            public new int LoopCount
            {
                get
                {
                    var logicType = LogicType;
                    return logicType == LogicTypeOption.Loop ? GetInt("LoopCount", 3) : 0;
                }
                set
                {
                    var logicType = LogicType;
                    if (logicType == LogicTypeOption.Loop) node.Properties["LoopCount"] = value;
                }
            }
        }

        // Trigger Node Property Wrapper - Shows: TriggerType only
        private class TriggerNodePropertyWrapper : NodePropertyWrapper
        {
            public TriggerNodePropertyWrapper(ComboNode n) : base(n) { }

            [System.ComponentModel.Category("Trigger")]
            [System.ComponentModel.Description("Type of trigger event that starts this combo: OnAttack, OnChase, OnIdle, OnOwnerDamage, OnTargetChange.")]
            [System.ComponentModel.Browsable(true)]
            public new string TriggerType
            {
                get { return GetString("TriggerType", "OnAttack"); }
                set { node.Properties["TriggerType"] = value; }
            }
        }

        // Target Node Property Wrapper - Shows: TargetMode, MobID (only when mode=MobID)
        private class TargetNodePropertyWrapper : NodePropertyWrapper
        {
            public TargetNodePropertyWrapper(ComboNode n) : base(n) { }

            [System.ComponentModel.Category("Target")]
            [System.ComponentModel.Description("Targeting mode used by runtime smart selectors. Choose among: Enemy, Owner, Self, Ally, NearestEnemy, FarthestEnemy, StrongestEnemy, WeakestEnemy, NearestAlly, FarthestAlly, Focus, MobID.")]
            [System.ComponentModel.Browsable(true)]
            public TargetModeOption TargetMode
            {
                get
                {
                    var raw = GetString("TargetMode", "Enemy");
                    TargetModeOption val;
                    return Enum.TryParse(raw, true, out val) ? val : TargetModeOption.Enemy;
                }
                set { node.Properties["TargetMode"] = value.ToString(); }
            }

            [System.ComponentModel.Category("Target")]
            [System.ComponentModel.Description("Specific mob ID to target (for boss-specific tactics). Only used when TargetMode=MobID. Example: 1189 for Phreeoni, 1095 for Andre. Leave at 0 if not using MobID mode.")]
            [System.ComponentModel.Browsable(true)]
            public new int MobID
            {
                get { return GetInt("MobID", 0); }
                set { node.Properties["MobID"] = value; }
            }
        }

        // Factory method to create type-specific property wrappers
        private object CreatePropertyWrapperForNode(ComboNode node)
        {
            switch (node.Type)
            {
                case NodeType.Skill:
                    return new SkillNodePropertyWrapper(node);
                case NodeType.Condition:
                    // Special case for Kimi Type condition
                    bool isKimiType = node.Properties.ContainsKey("ConditionType") &&
                                      node.Properties["ConditionType"] != null &&
                                      node.Properties["ConditionType"].ToString() == "HomuType";
                    if (isKimiType)
                        return new KimiNodePropertyWrapper(node);
                    else
                        return new ConditionNodePropertyWrapper(node);
                case NodeType.Logic:
                    return new LogicNodePropertyWrapper(node);
                case NodeType.Trigger:
                    return new TriggerNodePropertyWrapper(node);
                case NodeType.Target:
                    return new TargetNodePropertyWrapper(node);
                case NodeType.Output:  // End nodes use Output type
                    return new EndNodePropertyWrapper(node);
                default:
                    return new NodePropertyWrapper(node);
            }
        }

        // Base Property wrapper for PropertyGrid
        private class NodePropertyWrapper
        {
            protected ComboNode node;

            public NodePropertyWrapper(ComboNode n)
            {
                node = n;
            }

            [System.ComponentModel.Category("General")]
            [System.ComponentModel.Description("Node display name shown in the blueprint canvas. Change to organize complex combos (e.g., 'Low HP Heal', 'AoE Burst Combo').")]
            public string Title
            {
                get { return node.Title; }
                set { node.Title = value; }
            }

            [System.ComponentModel.Category("General")]
            [System.ComponentModel.Description("Detailed description of what this node does. Auto-populated based on node type. Used as reference when designing combos.")]
            public string Description
            {
                get { return node.Description; }
                set { node.Description = value; }
            }

            // Common strongly-typed properties (hidden by default, shown in type-specific wrappers)

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Skill")]
            [System.ComponentModel.Description("Skill level (1-10). Higher levels = more damage/effect. 0 = disabled. Use level 5+ for core skills.")]
            public int SkillLevel
            {
                get { return GetInt("SkillLevel", 1); }
                set { node.Properties["SkillLevel"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Skill")]
            [System.ComponentModel.Description("Times to execute this skill/attack in sequence. Use 1 for single cast, 2-5 for spam chains (e.g., Claws x3).")]
            public int RepeatCount
            {
                get { return GetInt("RepeatCount", 1); }
                set { node.Properties["RepeatCount"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Condition")]
            [System.ComponentModel.Description("Comparison operator: >= (at least), <= (at most), == (exact), > (more than), < (less than). Use >= for HP/SP thresholds, == for type checks.")]
            public string Operator
            {
                get { return GetString("Operator", ">="); }
                set { node.Properties["Operator"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Condition")]
            [System.ComponentModel.Description("Threshold value for condition. Examples: HP%=30 (heal when low), SP%=60 (enough SP to cast), MobCount=3 (AoE trigger), Distance<=5 (melee range).")]
            public int Value
            {
                get { return GetInt("Value", 0); }
                set { node.Properties["Value"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Range")]
            [System.ComponentModel.Description("Minimum distance in cells. Use 0 for melee-only skills, 1-3 for short-range checks. Combined with MaxRange for range gates.")]
            public int MinRange
            {
                get { return GetInt("MinRange", 0); }
                set { node.Properties["MinRange"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Range")]
            [System.ComponentModel.Description("Maximum distance in cells. Use 1-3 for melee, 5-10 for ranged checks. Prevents casting skills when target is too far.")]
            public int MaxRange
            {
                get { return GetInt("MaxRange", 0); }
                set { node.Properties["MaxRange"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Logic")]
            [System.ComponentModel.Description("How many times to repeat the loop. Use 2-5 for skill chains, 10+ for sustained barrage. Caution: high values can cause spam/lag.")]
            public int LoopCount
            {
                get { return GetInt("LoopCount", 1); }
                set { node.Properties["LoopCount"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Logic")]
            [System.ComponentModel.Description("Milliseconds to wait. 100-500ms = skill spam timing, 1000ms = 1 second for buffs, 2000-5000ms = long cooldowns. Prevents skill overlap.")]
            public int DelayMs
            {
                get { return GetInt("DelayMs", 0); }
                set { node.Properties["DelayMs"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Logic")]
            [System.ComponentModel.Description("Internal flag for Combo Delay nodes. Leave as default unless customizing Lua export behavior.")]
            public bool UseComboDelay
            {
                get { return GetBool("UseComboDelay", false); }
                set { node.Properties["UseComboDelay"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Targeting")]
            [System.ComponentModel.Description("Target selection: Current (last target), Nearest (closest mob), Weakest (lowest HP), Strongest (highest HP). Use Nearest for AoE, Weakest to finish low HP enemies.")]
            public string SelectionMode
            {
                get { return GetString("SelectionMode", "Current"); }
                set { node.Properties["SelectionMode"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Target")]
            [System.ComponentModel.Description("How to select target: Current (active target), ByMobID (specific mob), Owner (master), Self (Kimi), Nearest (closest), Weakest (lowest HP), Strongest (highest HP).")]
            public string TargetMode
            {
                get { return GetString("TargetMode", "Current"); }
                set { node.Properties["TargetMode"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Target")]
            [System.ComponentModel.Description("Specific mob ID to target (when TargetMode=ByMobID). Example: 1189 for Phreeoni, 1095 for Andre. Check Mob_ID.lua for full list.")]
            public int MobID
            {
                get { return GetInt("MobID", 0); }
                set { node.Properties["MobID"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Skill")]
            [System.ComponentModel.Description("Type of trigger event that starts this combo. Examples: OnAttack, OnChase, OnIdle, OnOwnerDamage, OnTargetChange.")]
            public string TriggerType
            {
                get { return GetString("TriggerType", "OnAttack"); }
                set { node.Properties["TriggerType"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Condition")]
            [System.ComponentModel.Description("What condition to check: HomuType (Kimi type), HP (health %), SP (mana %), MobCount (enemy count), Distance (range), etc.")]
            public string ConditionType
            {
                get { return GetString("ConditionType", "HomuType"); }
                set { node.Properties["ConditionType"] = value; }
            }

            [System.ComponentModel.Browsable(false)]
            [System.ComponentModel.Category("Logic")]
            [System.ComponentModel.Description("Logic operation: Delay (wait time), Loop (repeat count), AND (both true), OR (either true), Sequence (execute in order).")]
            public string LogicType
            {
                get { return GetString("LogicType", "Delay"); }
                set { node.Properties["LogicType"] = value; }
            }

            // Helper accessors
            private bool IsKimiTypeNode()
            {
                return node.Properties.ContainsKey("ConditionType") &&
                       node.Properties["ConditionType"] != null &&
                       node.Properties["ConditionType"].ToString() == "HomuType";
            }

            protected int GetInt(string key, int defaultValue)
            {
                if (node.Properties.ContainsKey(key) && int.TryParse(node.Properties[key].ToString(), out int result))
                    return result;
                return defaultValue;
            }

            protected bool GetBool(string key, bool defaultValue)
            {
                if (node.Properties.ContainsKey(key))
                {
                    bool result;
                    if (bool.TryParse(node.Properties[key].ToString(), out result))
                        return result;
                    if (node.Properties[key] is bool b) return b;
                }
                return defaultValue;
            }

            protected string GetString(string key, string defaultValue)
            {
                if (node.Properties.ContainsKey(key))
                    return node.Properties[key].ToString();
                return defaultValue;
            }
        }

        [Serializable]
        private class SerializableBlueprintData
        {
            public List<SerializableNode> Nodes { get; set; } = new List<SerializableNode>();
            public List<SerializableNodeConnection> SerializableConnections { get; set; } = new List<SerializableNodeConnection>();
        }
        
        private class SerializableNodeConnection
        {
            public Guid Id { get; set; }
            public Guid SourceNodeId { get; set; }
            public Guid SourcePinId { get; set; }
            public Guid TargetNodeId { get; set; }
            public Guid TargetPinId { get; set; }
            public string LineColorHex { get; set; }
        }
        
        private class SerializableNode
        {
            public Guid Id { get; set; }
            public NodeType Type { get; set; }
            public NodeCategory Category { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public Point Position { get; set; }
            public Size Size { get; set; }
            public string HeaderColorHex { get; set; }
            public string BodyColorHex { get; set; }
            public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
            public List<SerializablePin> InputPins { get; set; } = new List<SerializablePin>();
            public List<SerializablePin> OutputPins { get; set; } = new List<SerializablePin>();
        }
        
        private class SerializablePin
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public PinType Type { get; set; }
            public bool IsInput { get; set; }
            public int Index { get; set; }
            public string PinColorHex { get; set; }
            public List<Guid> Connections { get; set; } = new List<Guid>();
        }
    }
}
