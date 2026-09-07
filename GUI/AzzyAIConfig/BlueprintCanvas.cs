using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace AzzyAIConfig
{
    public class BlueprintCanvas : Panel
    {
        private List<ComboNode> nodes = new List<ComboNode>();
        private List<NodeConnection> connections = new List<NodeConnection>();
        
        // Interaction state
        private ComboNode draggedNode = null;
        private Point dragOffset;
        private NodePin draggedPin = null;
        private Point tempConnectionEnd;
        private bool isPanning = false;
        private Point panStart;
        private Point gridOffset = new Point(0, 0);
        private float zoomLevel = 1.0f;
        private ContextMenuStrip nodeContextMenu;
        private ToolStripMenuItem deleteSelectedMenuItem;
        private ContextMenuStrip connectionContextMenu;
        private NodeConnection contextConnection;
        private Stack<CanvasState> undoStack = new Stack<CanvasState>();
        private Stack<CanvasState> redoStack = new Stack<CanvasState>();
        private ComboNode copyBuffer = null;
        private Point? dragStartPos = null;
        private List<ComboNode> selectedNodes = new List<ComboNode>();
        private bool isSelecting = false;
        private Point selectionStart;
        private Rectangle selectionRect = Rectangle.Empty;
        private Point groupDragStart;
        private Dictionary<ComboNode, Point> groupStartPositions = new Dictionary<ComboNode, Point>();
        private bool rightButtonDown = false;
        private Point rightDownPos;
        private bool rightDragged = false;
        
        // Visual settings
        private const int GridSize = 20;
        private Color gridColor = Color.FromArgb(30, 30, 35);
        private Color gridLineColor = Color.FromArgb(50, 50, 55);
        private Color selectionColor = Color.FromArgb(0, 122, 204);

        public ComboNode SelectedNode { get; private set; }
        public event EventHandler<NodeEventArgs> NodeSelected;
        public event EventHandler<NodeEventArgs> NodeAdded;
        public event EventHandler<ConnectionEventArgs> ConnectionCreated;

        // Helper: convert screen coordinates to world coordinates (accounts for pan + zoom)
        private Point ScreenToWorld(Point screenPos)
        {
            float invZoom = 1f / zoomLevel;
            int worldX = (int)Math.Round((screenPos.X - gridOffset.X) * invZoom);
            int worldY = (int)Math.Round((screenPos.Y - gridOffset.Y) * invZoom);
            return new Point(worldX, worldY);
        }

        public BlueprintCanvas()
        {
            DoubleBuffered = true;
            BackColor = gridColor;
            AllowDrop = true;
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
            
            MouseDown += OnCanvasMouseDown;
            MouseMove += OnCanvasMouseMove;
            MouseUp += OnCanvasMouseUp;
            Paint += OnCanvasPaint;
            DragEnter += OnCanvasDragEnter;
            DragDrop += OnCanvasDragDrop;
            MouseWheel += OnCanvasMouseWheel;
            KeyDown += OnCanvasKeyDown;

            BuildContextMenu();
            BuildConnectionMenu();
        }

        public void AddNode(ComboNode node)
        {
            SaveStateForUndo();
            nodes.Add(node);
            NodeAdded?.Invoke(this, new NodeEventArgs(node));
            Invalidate();
        }

        public void RemoveNode(ComboNode node)
        {
            SaveStateForUndo();
            // Remove connections
            connections.RemoveAll(c => c.SourceNodeId == node.Id || c.TargetNodeId == node.Id);
            nodes.Remove(node);
            if (selectedNodes.Contains(node))
            {
                selectedNodes.Remove(node);
                if (SelectedNode == node)
                    SelectedNode = selectedNodes.LastOrDefault();
            }
            Invalidate();
        }

        public void ClearAll()
        {
            SaveStateForUndo();
            nodes.Clear();
            connections.Clear();
            ClearSelection();
            Invalidate();
        }

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            Focus();
            Point worldPos = ScreenToWorld(e.Location);

            if (e.Button == MouseButtons.Middle)
            {
                isPanning = true;
                panStart = e.Location;
                Cursor = Cursors.SizeAll;
                return;
            }

            if (e.Button == MouseButtons.Left && (Control.ModifierKeys & Keys.Space) == Keys.Space)
            {
                isPanning = true;
                panStart = e.Location;
                Cursor = Cursors.SizeAll;
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                // Delay context menu handling to MouseUp to allow right-drag panning
                rightButtonDown = true;
                rightDownPos = e.Location;
                rightDragged = false;
                return;
            }

            // Check for pin drag (connection creation)
            foreach (var node in nodes.AsEnumerable().Reverse())
            {
                var pin = node.GetPinAt(worldPos);
                if (pin != null)
                {
                    draggedPin = pin;
                    draggedNode = node;
                    tempConnectionEnd = worldPos;
                    return;
                }
            }

            // Check for node selection and drag
            foreach (var node in nodes.AsEnumerable().Reverse())
            {
                if (node.GetBounds().Contains(worldPos))
                {
                    bool additive = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                    bool alreadySelected = selectedNodes.Contains(node);
                    if (additive)
                    {
                        ToggleSelection(node);
                        NodeSelected?.Invoke(this, new NodeEventArgs(node));
                    }
                    else
                    {
                        if (!alreadySelected)
                        {
                            SelectNode(node);
                        }
                        else
                        {
                            if (SelectedNode != node)
                            {
                                SelectedNode = node;
                                NodeSelected?.Invoke(this, new NodeEventArgs(node));
                            }
                        }
                    }

                    if (node.GetHeaderBounds().Contains(worldPos))
                    {
                        draggedNode = node;
                        dragOffset = new Point(worldPos.X - node.Position.X, worldPos.Y - node.Position.Y);
                        dragStartPos = node.Position;
                        groupDragStart = worldPos;
                        groupStartPositions = selectedNodes.ToDictionary(n => n, n => n.Position);
                        // Move to front
                        nodes.Remove(node);
                        nodes.Add(node);
                    }
                    Invalidate();
                    return;
                }
            }

            // Clicked on empty space - deselect
            if (selectedNodes.Count > 0)
            {
                ClearSelection();
                NodeSelected?.Invoke(this, new NodeEventArgs(null));
                Invalidate();
            }

            // Begin marquee selection
            if (e.Button == MouseButtons.Left)
            {
                isSelecting = true;
                selectionStart = worldPos;
                selectionRect = Rectangle.Empty;
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            Point worldPos = ScreenToWorld(e.Location);

            // If right mouse is pressed, detect drag and start panning
            if (rightButtonDown && !isPanning)
            {
                int dx = Math.Abs(e.X - rightDownPos.X);
                int dy = Math.Abs(e.Y - rightDownPos.Y);
                if (dx > 2 || dy > 2)
                {
                    isPanning = true;
                    panStart = e.Location;
                    rightDragged = true;
                    Cursor = Cursors.SizeAll;
                }
            }

            if (isPanning)
            {
                gridOffset.X += e.X - panStart.X;
                gridOffset.Y += e.Y - panStart.Y;
                panStart = e.Location;
                Invalidate();
                return;
            }

            if (isSelecting)
            {
                int x1 = Math.Min(selectionStart.X, worldPos.X);
                int y1 = Math.Min(selectionStart.Y, worldPos.Y);
                int x2 = Math.Max(selectionStart.X, worldPos.X);
                int y2 = Math.Max(selectionStart.Y, worldPos.Y);
                selectionRect = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                Invalidate();
                return;
            }

            if (draggedPin != null)
            {
                tempConnectionEnd = worldPos;
                Invalidate();
                return;
            }

            if (draggedNode != null && e.Button == MouseButtons.Left)
            {
                if (groupStartPositions.Count > 0)
                {
                    var delta = new Point(worldPos.X - groupDragStart.X, worldPos.Y - groupDragStart.Y);
                    foreach (var kvp in groupStartPositions)
                    {
                        kvp.Key.Position = new Point(kvp.Value.X + delta.X, kvp.Value.Y + delta.Y);
                    }
                }
                else
                {
                    draggedNode.Position = new Point(
                        worldPos.X - dragOffset.X,
                        worldPos.Y - dragOffset.Y
                    );
                }
                Invalidate();
                return;
            }

            // Hover detection
            bool needsRedraw = false;
            foreach (var node in nodes)
            {
                bool wasHovered = node.IsHovered;
                node.IsHovered = node.GetBounds().Contains(e.Location);
                if (wasHovered != node.IsHovered)
                    needsRedraw = true;
            }
            if (needsRedraw)
                Invalidate();
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Stop panning if it was a drag
                if (isPanning)
                {
                    isPanning = false;
                    Cursor = Cursors.Default;
                }
                // If it was just a click (no drag), handle context menus
                if (!rightDragged)
                {
                    HandleRightClick(e.Location);
                }
                rightButtonDown = false;
                rightDragged = false;
                return;
            }

            if (isPanning && (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Left))
            {
                isPanning = false;
                Cursor = Cursors.Default;
                return;
            }

            if (isSelecting && e.Button == MouseButtons.Left)
            {
                isSelecting = false;
                if (selectionRect.Width > 0 && selectionRect.Height > 0)
                {
                    ClearSelection();
                    foreach (var node in nodes)
                    {
                        if (selectionRect.IntersectsWith(node.GetBounds()))
                            AddToSelection(node);
                    }
                    SelectedNode = selectedNodes.FirstOrDefault();
                    NodeSelected?.Invoke(this, new NodeEventArgs(SelectedNode));
                }
                selectionRect = Rectangle.Empty;
                Invalidate();
                return;
            }

            if (draggedPin != null)
            {
                Point worldPos = ScreenToWorld(e.Location);
                bool connected = false;
                // Try to create connection
                foreach (var targetNode in nodes)
                {
                    var targetPin = targetNode.GetPinAt(worldPos);
                    if (targetPin != null && targetNode != draggedNode && 
                        targetPin.IsInput != draggedPin.IsInput)
                    {
                        CreateConnection(draggedNode, draggedPin, targetNode, targetPin);
                        connected = true;
                        break;
                    }
                }
                
                // If not connected, show compatible node menu
                if (!connected)
                {
                    ShowPinSuggestionMenu(draggedPin, draggedNode, worldPos);
                }
                
                draggedPin = null;
                draggedNode = null;
                Invalidate();
                return;
            }

            if (draggedNode != null && dragStartPos.HasValue && draggedNode.Position != dragStartPos.Value)
            {
                SaveStateForUndo();
            }
            draggedNode = null;
            dragStartPos = null;
            groupStartPositions.Clear();
        }

        private void HandleRightClick(Point screenLocation)
        {
            Point worldPos = ScreenToWorld(screenLocation);
            // First, try connection hit-test
            var hitConn = HitTestConnection(worldPos, 6);
            if (hitConn != null)
            {
                contextConnection = hitConn;
                connectionContextMenu.Show(this, screenLocation);
                return;
            }

            // Check for pin right-click
            foreach (var node in nodes.AsEnumerable().Reverse())
            {
                var pin = node.GetPinAt(worldPos);
                if (pin != null)
                {
                    ShowPinSuggestionMenu(pin, node, screenLocation);
                    return;
                }
            }

            foreach (var node in nodes.AsEnumerable().Reverse())
            {
                if (node.GetBounds().Contains(worldPos))
                {
                    bool alreadySelected = selectedNodes.Contains(node);
                    if (!alreadySelected)
                    {
                        SelectNode(node);
                    }
                    else
                    {
                        SelectedNode = node;
                        NodeSelected?.Invoke(this, new NodeEventArgs(node));
                    }

                    nodeContextMenu.Tag = node;
                    nodeContextMenu.Show(this, screenLocation);
                    return;
                }
            }

            // Empty space right-click: deselect
            if (selectedNodes.Count > 0)
            {
                ClearSelection();
                NodeSelected?.Invoke(this, new NodeEventArgs(null));
                Invalidate();
            }
        }

        private void OnCanvasKeyDown(object sender, KeyEventArgs e)
        {
            // Arrow keys pan the canvas
            if (!e.Control && !e.Alt)
            {
                int step = e.Shift ? 40 : 20;
                bool moved = false;
                if (e.KeyCode == Keys.Left) { gridOffset.X += step; moved = true; }
                if (e.KeyCode == Keys.Right) { gridOffset.X -= step; moved = true; }
                if (e.KeyCode == Keys.Up) { gridOffset.Y += step; moved = true; }
                if (e.KeyCode == Keys.Down) { gridOffset.Y -= step; moved = true; }
                if (moved)
                {
                    Invalidate();
                    e.Handled = true;
                    return;
                }
            }

            // Select All
            if (e.Control && e.KeyCode == Keys.A)
            {
                ClearSelection();
                foreach (var n in nodes)
                {
                    n.IsSelected = true;
                    selectedNodes.Add(n);
                }
                SelectedNode = selectedNodes.FirstOrDefault();
                NodeSelected?.Invoke(this, new NodeEventArgs(SelectedNode));
                Invalidate();
                e.Handled = true;
                return;
            }

            // Clear selection
            if (e.KeyCode == Keys.Escape)
            {
                if (selectedNodes.Count > 0)
                {
                    ClearSelection();
                    NodeSelected?.Invoke(this, new NodeEventArgs(null));
                    Invalidate();
                }
                e.Handled = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.Z)
            {
                Undo();
                e.Handled = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.Y)
            {
                Redo();
                e.Handled = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.C)
            {
                if (SelectedNode != null)
                {
                    copyBuffer = CloneNode(SelectedNode);
                }
                e.Handled = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.V)
            {
                if (copyBuffer != null)
                {
                    SaveStateForUndo();
                    var pasted = CloneNode(copyBuffer);
                    pasted.Position = SelectedNode != null ? new Point(SelectedNode.Position.X + 20, SelectedNode.Position.Y + 20) : new Point(30, 30);
                    AddNode(pasted);
                    SelectNode(pasted);
                }
                e.Handled = true;
                return;
            }
            if (e.KeyCode == Keys.Delete && selectedNodes.Count > 0)
            {
                RemoveNodes(selectedNodes.ToList());
                ClearSelection();
                NodeSelected?.Invoke(this, new NodeEventArgs(null));
                e.Handled = true;
            }
        }

        private void CreateConnection(ComboNode sourceNode, NodePin sourcePin, 
                                     ComboNode targetNode, NodePin targetPin)
        {
            // Ensure proper direction (output -> input)
            if (sourcePin.IsInput)
            {
                var temp = sourceNode;
                sourceNode = targetNode;
                targetNode = temp;
                
                var tempPin = sourcePin;
                sourcePin = targetPin;
                targetPin = tempPin;
            }

            // Check if connection already exists
            if (connections.Any(c => c.SourceNodeId == sourceNode.Id && 
                                    c.SourcePinId == sourcePin.Id &&
                                    c.TargetNodeId == targetNode.Id && 
                                    c.TargetPinId == targetPin.Id))
                return;

            // For execution pins, only one connection allowed
            if (sourcePin.Type == PinType.Execution || targetPin.Type == PinType.Execution)
            {
                connections.RemoveAll(c => c.SourcePinId == sourcePin.Id || 
                                          c.TargetPinId == targetPin.Id);
            }

            var connection = new NodeConnection
            {
                SourceNodeId = sourceNode.Id,
                SourcePinId = sourcePin.Id,
                TargetNodeId = targetNode.Id,
                TargetPinId = targetPin.Id,
                LineColor = sourcePin.Type == PinType.Execution ? 
                           Color.White : Color.FromArgb(100, 180, 255)
            };

            SaveStateForUndo();
            connections.Add(connection);
            sourcePin.Connections.Add(targetPin.Id);
            targetPin.Connections.Add(sourcePin.Id);
            
            ConnectionCreated?.Invoke(this, new ConnectionEventArgs(connection));
        }

        private void RemoveConnectionsForPin(Guid pinId)
        {
            var toRemove = connections.Where(c => c.SourcePinId == pinId || c.TargetPinId == pinId).ToList();
            foreach (var conn in toRemove)
            {
                RemoveConnection(conn);
            }
        }

        private void RemoveConnection(NodeConnection conn)
        {
            SaveStateForUndo();
            connections.Remove(conn);

            var sourceNode = nodes.FirstOrDefault(n => n.Id == conn.SourceNodeId);
            var targetNode = nodes.FirstOrDefault(n => n.Id == conn.TargetNodeId);
            if (sourceNode != null)
            {
                var sp = sourceNode.OutputPins.FirstOrDefault(p => p.Id == conn.SourcePinId) ??
                         sourceNode.InputPins.FirstOrDefault(p => p.Id == conn.SourcePinId);
                sp?.Connections.Remove(conn.TargetPinId);
            }
            if (targetNode != null)
            {
                var tp = targetNode.InputPins.FirstOrDefault(p => p.Id == conn.TargetPinId) ??
                         targetNode.OutputPins.FirstOrDefault(p => p.Id == conn.TargetPinId);
                tp?.Connections.Remove(conn.SourcePinId);
            }

            Invalidate();
        }

        private void OnCanvasDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ComboNode)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void OnCanvasDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ComboNode)))
            {
                var template = (ComboNode)e.Data.GetData(typeof(ComboNode));
                var newNode = CloneNode(template);
                
                Point dropPoint = PointToClient(new Point(e.X, e.Y));
                newNode.Position = new Point(dropPoint.X - newNode.Size.Width / 2, 
                                            dropPoint.Y - 15);
                
                AddNode(newNode);
            }
        }

        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            float oldZoom = zoomLevel;
            zoomLevel += e.Delta > 0 ? 0.1f : -0.1f;
            zoomLevel = Math.Max(0.5f, Math.Min(2.0f, zoomLevel));
            
            // TODO: Implement zoom transform
            Invalidate();
        }

        private ComboNode CloneNode(ComboNode template)
        {
            var node = new ComboNode
            {
                Type = template.Type,
                Category = template.Category,
                Title = template.Title,
                Description = template.Description,
                HeaderColor = template.HeaderColor,
                BodyColor = template.BodyColor,
                Size = template.Size
            };

            foreach (var pin in template.InputPins)
            {
                node.InputPins.Add(new NodePin
                {
                    Name = pin.Name,
                    Type = pin.Type,
                    IsInput = true,
                    Index = pin.Index,
                    PinColor = pin.PinColor
                });
            }

            foreach (var pin in template.OutputPins)
            {
                node.OutputPins.Add(new NodePin
                {
                    Name = pin.Name,
                    Type = pin.Type,
                    IsInput = false,
                    Index = pin.Index,
                    PinColor = pin.PinColor
                });
            }

            foreach (var prop in template.Properties)
            {
                node.Properties[prop.Key] = prop.Value;
            }

            return node;
        }

        private void BuildContextMenu()
        {
            nodeContextMenu = new ContextMenuStrip();
            // Style with dark theme
            nodeContextMenu.BackColor = Color.FromArgb(40, 40, 40);
            nodeContextMenu.ForeColor = Color.White;
            nodeContextMenu.Renderer = new DarkMenuRenderer();
            nodeContextMenu.AutoClose = true;
            nodeContextMenu.ShowImageMargin = false;
            nodeContextMenu.ShowCheckMargin = false;
            
            nodeContextMenu.Items.Add("Duplicate Node", null, (s, e) =>
            {
                if (nodeContextMenu.Tag is ComboNode n)
                {
                    var clone = CloneNode(n);
                    clone.Position = new Point(n.Position.X + 20, n.Position.Y + 20);
                    AddNode(clone);
                }
            });

            nodeContextMenu.Items.Add("Delete Node", null, (s, e) =>
            {
                if (nodeContextMenu.Tag is ComboNode n)
                {
                    RemoveNodes(new List<ComboNode> { n });
                    NodeSelected?.Invoke(this, new NodeEventArgs(null));
                }
            });

            deleteSelectedMenuItem = new ToolStripMenuItem("Delete Selected Nodes", null, (s, e) =>
            {
                if (selectedNodes.Count > 1)
                {
                    RemoveNodes(selectedNodes.ToList());
                    ClearSelection();
                    NodeSelected?.Invoke(this, new NodeEventArgs(null));
                }
            });
            nodeContextMenu.Items.Add(deleteSelectedMenuItem);

            nodeContextMenu.Opening += (s, e) =>
            {
                deleteSelectedMenuItem.Enabled = selectedNodes.Count > 1;
            };
        }

        private void RemoveNodes(IEnumerable<ComboNode> nodesToRemove)
        {
            var list = nodesToRemove?.ToList();
            if (list == null || list.Count == 0) return;

            SaveStateForUndo();

            foreach (var node in list)
            {
                connections.RemoveAll(c => c.SourceNodeId == node.Id || c.TargetNodeId == node.Id);
            }

            foreach (var node in list)
            {
                nodes.Remove(node);
                if (selectedNodes.Contains(node))
                {
                    selectedNodes.Remove(node);
                }
            }

            SelectedNode = selectedNodes.LastOrDefault();
            Invalidate();
        }

        private void BuildConnectionMenu()
        {
            connectionContextMenu = new ContextMenuStrip();
            // Style with dark theme
            connectionContextMenu.BackColor = Color.FromArgb(40, 40, 40);
            connectionContextMenu.ForeColor = Color.White;
            connectionContextMenu.Renderer = new DarkMenuRenderer();
            
            connectionContextMenu.Items.Add("Delete Connection", null, (s, e) =>
            {
                if (contextConnection != null)
                {
                    RemoveConnection(contextConnection);
                    contextConnection = null;
                }
            });
        }

        private void SelectNode(ComboNode node)
        {
            ClearSelection();
            AddToSelection(node);
            SelectedNode = node;
            NodeSelected?.Invoke(this, new NodeEventArgs(node));
        }

        private void AddToSelection(ComboNode node)
        {
            if (node == null) return;
            if (!selectedNodes.Contains(node))
            {
                selectedNodes.Add(node);
                node.IsSelected = true;
            }
            SelectedNode = node;
        }

        private void ToggleSelection(ComboNode node)
        {
            if (node == null) return;
            if (selectedNodes.Contains(node))
            {
                selectedNodes.Remove(node);
                node.IsSelected = false;
            }
            else
            {
                selectedNodes.Add(node);
                node.IsSelected = true;
            }
            SelectedNode = selectedNodes.LastOrDefault();
        }

        private void ClearSelection()
        {
            foreach (var n in selectedNodes)
            {
                n.IsSelected = false;
            }
            selectedNodes.Clear();
            SelectedNode = null;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            var key = keyData & Keys.KeyCode;
            if (key == Keys.Delete || key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down)
                return true;
            return base.IsInputKey(keyData);
        }

        private NodeConnection HitTestConnection(Point p, int tolerance)
        {
            foreach (var conn in connections)
            {
                var sourceNode = nodes.FirstOrDefault(n => n.Id == conn.SourceNodeId);
                var targetNode = nodes.FirstOrDefault(n => n.Id == conn.TargetNodeId);
                if (sourceNode == null || targetNode == null) continue;

                var sourcePin = sourceNode.OutputPins.FirstOrDefault(x => x.Id == conn.SourcePinId) ??
                                sourceNode.InputPins.FirstOrDefault(x => x.Id == conn.SourcePinId);
                var targetPin = targetNode.InputPins.FirstOrDefault(x => x.Id == conn.TargetPinId) ??
                                targetNode.OutputPins.FirstOrDefault(x => x.Id == conn.TargetPinId);
                if (sourcePin == null || targetPin == null) continue;

                var start = sourcePin.GetCenter(sourceNode);
                var end = targetPin.GetCenter(targetNode);
                if (DistancePointToSegment(p, start, end) <= tolerance)
                    return conn;
            }
            return null;
        }

        private double DistancePointToSegment(Point p, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            if (dx == 0 && dy == 0)
            {
                dx = p.X - a.X;
                dy = p.Y - a.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));
            double projX = a.X + t * dx;
            double projY = a.Y + t * dy;
            double distX = p.X - projX;
            double distY = p.Y - projY;
            return Math.Sqrt(distX * distX + distY * distY);
        }

        private void SaveStateForUndo()
        {
            var state = new CanvasState
            {
                Nodes = DeepCloneNodes(nodes),
                Connections = DeepCloneConnections(connections)
            };
            undoStack.Push(state);
            redoStack.Clear();
        }

        private void Undo()
        {
            if (undoStack.Count == 0) return;
            var current = new CanvasState { Nodes = DeepCloneNodes(nodes), Connections = DeepCloneConnections(connections) };
            var prev = undoStack.Pop();
            redoStack.Push(current);
            RestoreState(prev);
        }

        private void Redo()
        {
            if (redoStack.Count == 0) return;
            var current = new CanvasState { Nodes = DeepCloneNodes(nodes), Connections = DeepCloneConnections(connections) };
            var next = redoStack.Pop();
            undoStack.Push(current);
            RestoreState(next);
        }

        private void RestoreState(CanvasState state)
        {
            nodes = DeepCloneNodes(state.Nodes);
            connections = DeepCloneConnections(state.Connections);
            ClearSelection();
            NodeSelected?.Invoke(this, new NodeEventArgs(null));
            Invalidate();
        }

        private List<ComboNode> DeepCloneNodes(List<ComboNode> source)
        {
            var list = new List<ComboNode>();
            foreach (var n in source)
            {
                var clone = new ComboNode
                {
                    Id = n.Id,
                    Type = n.Type,
                    Category = n.Category,
                    Title = n.Title,
                    Description = n.Description,
                    HeaderColor = n.HeaderColor,
                    BodyColor = n.BodyColor,
                    Size = n.Size,
                    Position = n.Position,
                    IsSelected = false,
                    IsHovered = false
                };

                foreach (var pin in n.InputPins)
                {
                    clone.InputPins.Add(new NodePin
                    {
                        Id = pin.Id,
                        Name = pin.Name,
                        Type = pin.Type,
                        IsInput = pin.IsInput,
                        Index = pin.Index,
                        PinColor = pin.PinColor,
                        Connections = new List<Guid>(pin.Connections)
                    });
                }
                foreach (var pin in n.OutputPins)
                {
                    clone.OutputPins.Add(new NodePin
                    {
                        Id = pin.Id,
                        Name = pin.Name,
                        Type = pin.Type,
                        IsInput = pin.IsInput,
                        Index = pin.Index,
                        PinColor = pin.PinColor,
                        Connections = new List<Guid>(pin.Connections)
                    });
                }
                foreach (var kv in n.Properties)
                {
                    clone.Properties[kv.Key] = kv.Value;
                }
                list.Add(clone);
            }
            return list;
        }

        private List<NodeConnection> DeepCloneConnections(List<NodeConnection> source)
        {
            var list = new List<NodeConnection>();
            foreach (var c in source)
            {
                list.Add(new NodeConnection
                {
                    SourceNodeId = c.SourceNodeId,
                    SourcePinId = c.SourcePinId,
                    TargetNodeId = c.TargetNodeId,
                    TargetPinId = c.TargetPinId,
                    LineColor = c.LineColor
                });
            }
            return list;
        }

        private class CanvasState
        {
            public List<ComboNode> Nodes { get; set; }
            public List<NodeConnection> Connections { get; set; }
        }

        private void OnCanvasPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Apply camera transform (zoom then pan so offsets are not scaled)
            g.ScaleTransform(zoomLevel, zoomLevel);
            g.TranslateTransform(gridOffset.X, gridOffset.Y, MatrixOrder.Append);

            // Draw grid
            DrawGrid(g);

            // Draw connections
            foreach (var conn in connections)
            {
                DrawConnection(g, conn);
            }

            // Draw temp connection while dragging
            if (draggedPin != null && draggedNode != null)
            {
                Point start = draggedPin.GetCenter(draggedNode);
                DrawBezierConnection(g, start, tempConnectionEnd, 
                                   draggedPin.Type == PinType.Execution ? Color.White : 
                                   Color.FromArgb(100, 180, 255), 2);
            }

            // Draw nodes
            foreach (var node in nodes)
            {
                DrawNode(g, node);
            }

            // Selection marquee
            if (isSelecting && selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                using (var brush = new SolidBrush(Color.FromArgb(40, selectionColor)))
                using (var pen = new Pen(selectionColor, 1) { DashStyle = DashStyle.Dash })
                {
                    g.FillRectangle(brush, selectionRect);
                    g.DrawRectangle(pen, selectionRect);
                }
            }
        }

        private void DrawGrid(Graphics g)
        {
            using (var pen = new Pen(gridLineColor, 1))
            {
                float invZoom = 1f / zoomLevel;

                // Determine the visible world-space bounds based on zoom and pan
                float worldLeft = -gridOffset.X * invZoom;
                float worldTop = -gridOffset.Y * invZoom;
                float worldRight = (Width - gridOffset.X) * invZoom;
                float worldBottom = (Height - gridOffset.Y) * invZoom;

                int firstGridX = (int)Math.Floor(worldLeft / GridSize) * GridSize;
                int firstGridY = (int)Math.Floor(worldTop / GridSize) * GridSize;

                // Vertical lines
                for (int worldX = firstGridX; worldX <= worldRight + GridSize; worldX += GridSize)
                {
                    g.DrawLine(pen, worldX, worldTop - GridSize, worldX, worldBottom + GridSize);
                }

                // Horizontal lines
                for (int worldY = firstGridY; worldY <= worldBottom + GridSize; worldY += GridSize)
                {
                    g.DrawLine(pen, worldLeft - GridSize, worldY, worldRight + GridSize, worldY);
                }
            }
        }

        private void DrawNode(Graphics g, ComboNode node)
        {
            var bounds = node.GetBounds();
            var headerBounds = node.GetHeaderBounds();

            // Shadow
            using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            {
                g.FillRoundedRectangle(shadowBrush, bounds.X + 3, bounds.Y + 3, 
                                      bounds.Width, bounds.Height, 8);
            }

            // Body
            using (var bodyBrush = new SolidBrush(node.BodyColor))
            {
                g.FillRoundedRectangle(bodyBrush, bounds, 8);
            }

            // Header
            using (var headerBrush = new SolidBrush(node.HeaderColor))
            {
                g.FillRoundedRectangle(headerBrush, headerBounds, 8, true, false);
            }

            // Selection/Hover outline
            if (node.IsSelected || node.IsHovered)
            {
                using (var pen = new Pen(node.IsSelected ? selectionColor : 
                                        Color.FromArgb(100, 100, 105), 2))
                {
                    g.DrawRoundedRectangle(pen, bounds, 8);
                }
            }

            // Title
            using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                var titleFormat = new StringFormat 
                { 
                    Alignment = StringAlignment.Center, 
                    LineAlignment = StringAlignment.Center 
                };
                g.DrawString(node.Title, font, brush, headerBounds, titleFormat);
            }

            // Draw pins
            foreach (var pin in node.InputPins)
            {
                DrawPin(g, pin, node);
            }
            foreach (var pin in node.OutputPins)
            {
                DrawPin(g, pin, node);
            }

            // Description/properties
            if (!string.IsNullOrEmpty(node.Description))
            {
                using (var font = new Font("Segoe UI", 8))
                using (var brush = new SolidBrush(Color.FromArgb(180, 180, 180)))
                {
                    var textRect = new Rectangle(bounds.X + 40, bounds.Y + 50, 
                                                 bounds.Width - 80, bounds.Height - 60);
                    g.DrawString(node.Description, font, brush, textRect);
                }
            }
        }

        private void DrawPin(Graphics g, NodePin pin, ComboNode parent)
        {
            var bounds = pin.GetBounds(parent);
            
            // Pin circle
            using (var brush = new SolidBrush(pin.PinColor))
            using (var pen = new Pen(Color.Black, 1))
            {
                if (pin.Type == PinType.Execution)
                {
                    // Triangle for execution pins
                    Point[] points = pin.IsInput ? 
                        new Point[] {
                            new Point(bounds.Right, bounds.Y + bounds.Height / 2),
                            new Point(bounds.X, bounds.Y),
                            new Point(bounds.X, bounds.Bottom)
                        } :
                        new Point[] {
                            new Point(bounds.X, bounds.Y + bounds.Height / 2),
                            new Point(bounds.Right, bounds.Y),
                            new Point(bounds.Right, bounds.Bottom)
                        };
                    g.FillPolygon(brush, points);
                    g.DrawPolygon(pen, points);
                }
                else
                {
                    // Circle for data pins
                    g.FillEllipse(brush, bounds);
                    g.DrawEllipse(pen, bounds);
                }
            }

            // Pin label
            using (var font = new Font("Segoe UI", 7.5f))
            using (var brush = new SolidBrush(Color.FromArgb(200, 200, 200)))
            {
                var format = new StringFormat 
                { 
                    LineAlignment = StringAlignment.Center 
                };
                
                int labelX = pin.IsInput ? bounds.Right + 5 : bounds.X - 5;
                format.Alignment = pin.IsInput ? StringAlignment.Near : StringAlignment.Far;
                
                g.DrawString(pin.Name, font, brush, labelX, 
                           bounds.Y + bounds.Height / 2, format);
            }
        }

        private void DrawConnection(Graphics g, NodeConnection conn)
        {
            var sourceNode = nodes.FirstOrDefault(n => n.Id == conn.SourceNodeId);
            var targetNode = nodes.FirstOrDefault(n => n.Id == conn.TargetNodeId);
            
            if (sourceNode == null || targetNode == null) return;

            var sourcePin = sourceNode.OutputPins.FirstOrDefault(p => p.Id == conn.SourcePinId);
            var targetPin = targetNode.InputPins.FirstOrDefault(p => p.Id == conn.TargetPinId);
            
            if (sourcePin == null || targetPin == null) return;

            Point start = sourcePin.GetCenter(sourceNode);
            Point end = targetPin.GetCenter(targetNode);

            DrawBezierConnection(g, start, end, conn.LineColor, 2.5f);
        }

        private void DrawBezierConnection(Graphics g, Point start, Point end, 
                                         Color color, float width)
        {
            int distance = Math.Abs(end.X - start.X);
            int controlOffset = Math.Min(distance / 2, 100);

            Point cp1 = new Point(start.X + controlOffset, start.Y);
            Point cp2 = new Point(end.X - controlOffset, end.Y);

            using (var pen = new Pen(color, width))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.ArrowAnchor;
                g.DrawBezier(pen, start, cp1, cp2, end);
            }
        }

        public List<ComboNode> GetNodes()
        {
            return new List<ComboNode>(nodes);
        }

        public List<NodeConnection> GetConnections()
        {
            return new List<NodeConnection>(connections);
        }

        public void RestoreConnections(List<NodeConnection> conns)
        {
            connections.Clear();
            foreach (var conn in conns)
            {
                connections.Add(conn);
                
                // Reconnect pin references
                var sourceNode = nodes.FirstOrDefault(n => n.Id == conn.SourceNodeId);
                var targetNode = nodes.FirstOrDefault(n => n.Id == conn.TargetNodeId);
                if (sourceNode != null && targetNode != null)
                {
                    var sourcePin = sourceNode.OutputPins.FirstOrDefault(p => p.Id == conn.SourcePinId) ??
                                   sourceNode.InputPins.FirstOrDefault(p => p.Id == conn.SourcePinId);
                    var targetPin = targetNode.InputPins.FirstOrDefault(p => p.Id == conn.TargetPinId) ??
                                   targetNode.OutputPins.FirstOrDefault(p => p.Id == conn.TargetPinId);
                    
                    if (sourcePin != null && targetPin != null)
                    {
                        if (!sourcePin.Connections.Contains(targetPin.Id))
                            sourcePin.Connections.Add(targetPin.Id);
                        if (!targetPin.Connections.Contains(sourcePin.Id))
                            targetPin.Connections.Add(sourcePin.Id);
                    }
                }
            }
            Invalidate();
        }

        private void ShowPinSuggestionMenu(NodePin sourcePin, ComboNode sourceNode, Point location)
        {
            var menu = new ContextMenuStrip();
            menu.BackColor = Color.FromArgb(45, 45, 48);
            menu.ForeColor = Color.White;
               menu.Renderer = new DarkMenuRenderer();
               menu.ShowImageMargin = false;
               menu.ShowCheckMargin = false;

               // Get ONLY compatible nodes based on current node type and pin
               var compatibleNodes = GetValidNextNodes(sourceNode, sourcePin)
                   .OrderBy(t => t.Title)
                .ToList();

            if (compatibleNodes.Count() == 0)
            {
                   var noMatch = new ToolStripMenuItem("No compatible nodes available");
                noMatch.Enabled = false;
                menu.Items.Add(noMatch);
            }
            else
            {
                foreach (var template in compatibleNodes)
                {
                    var item = new ToolStripMenuItem(template.Title);
                    item.Tag = template;
                    item.Click += (s, e) =>
                    {
                        var selectedTemplate = (s as ToolStripMenuItem)?.Tag as ComboNode;
                        if (selectedTemplate != null)
                        {
                            // Clone and add node
                            var newNode = CloneNode(selectedTemplate);
                            newNode.Position = new Point(location.X - 75, location.Y - 20);
                            AddNode(newNode);

                            // Auto-connect to compatible pin
                            var compatiblePin = GetFirstCompatiblePin(newNode, sourcePin);
                            if (compatiblePin != null)
                            {
                                CreateConnection(sourceNode, sourcePin, newNode, compatiblePin);
                            }

                            Invalidate();
                        }
                    };
                    menu.Items.Add(item);
                }
            }

               // Show at mouse cursor (use Control.MousePosition for screen coordinates)
               menu.Show(Control.MousePosition);
        }

           /// <summary>
           /// Returns ONLY the valid next node types based on current node and pin type
           /// Implements true RO AI combo flow logic with proper pin-type awareness
           /// Flow: Trigger → Condition(s) → AND/OR → Target → Skill → Delay/Loop/Condition
           /// </summary>
           private List<ComboNode> GetValidNextNodes(ComboNode sourceNode, NodePin sourcePin)
           {
               var allTemplates = NodeLibrary.GetAllNodeTemplates();
               var validNodes = new List<ComboNode>();

               // Pin type matters: Execution (white) connects to Execution inputs
               // Data (colored) connects to Data inputs (Target, Value, etc.)
               bool isExecutionPin = sourcePin.Type == PinType.Execution || sourcePin.Type == PinType.Trigger;
               bool isDataPin = sourcePin.Type == PinType.Data;

               // Determine what nodes are compatible based on source node type and pin
               switch (sourceNode.Type)
               {
                   case NodeType.Trigger:
                       // Trigger "Execute" output (Execution) → Condition first, then skills/targets
                       validNodes = allTemplates.Where(t => 
                           t.Type == NodeType.Condition ||
                           t.Type == NodeType.Skill ||
                           t.Type == NodeType.Target ||
                           (t.Type == NodeType.Logic && IsLogicNode(t, "AND", "OR", "Sequence"))).ToList();
                       break;

                   case NodeType.Condition:
                       if (isExecutionPin) // True/False execution branches
                       {
                           // Condition branching → Target, Skill, or more Conditions
                           validNodes = allTemplates.Where(t =>
                               t.Type == NodeType.Target ||
                               t.Type == NodeType.Skill ||
                               t.Type == NodeType.Condition ||
                               (t.Type == NodeType.Logic && IsLogicNode(t, "Delay", "Loop", "Sequence"))).ToList();
                       }
                       else if (isDataPin) // Value pin going to AND/OR
                       {
                           // Condition Value → AND/OR nodes (logic gates)
                           validNodes = allTemplates.Where(t =>
                               t.Type == NodeType.Logic && IsLogicNode(t, "AND", "OR")).ToList();
                       }
                       break;

                   case NodeType.Target:
                       // Target "Then" or "Target" pins
                       if (isExecutionPin)
                       {
                           // Then execution → Skill immediately
                           validNodes = allTemplates.Where(t =>
                               t.Type == NodeType.Skill ||
                               (t.Type == NodeType.Logic && IsLogicNode(t, "Delay"))).ToList();
                       }
                       else if (isDataPin)
                       {
                           // Target data pin → Skill's Target input
                           validNodes = allTemplates.Where(t =>
                               t.Type == NodeType.Skill).ToList();
                       }
                       break;

                   case NodeType.Skill:
                       // Skill "Then"/"On Success"/"On Fail" → Logic, Condition, or another Skill
                       if (isExecutionPin) // Execute, Then, On Success, On Fail
                       {
                           // Skill execution pins → Logic, Condition, or another Skill
                           validNodes = allTemplates.Where(t =>
                               t.Type == NodeType.Logic ||
                               t.Type == NodeType.Condition ||
                               t.Type == NodeType.Skill).ToList();
                       }
                       else if (isDataPin && sourcePin.Name == "Target") // Target data pin specifically
                       {
                           // Skill "Target" data pin → ONLY Set Target node's input
                           validNodes = allTemplates.Where(t =>
                               t.Type == NodeType.Target).ToList();
                       }
                       break;

                   case NodeType.Logic:
                       // Different logic types allow different next nodes
                       string logicType = sourceNode.Properties.ContainsKey("LogicType") 
                           ? sourceNode.Properties["LogicType"].ToString() 
                           : "Delay";
                   
                       if (logicType == "Loop")
                       {
                           // Loop body can contain skills, conditions, targets
                           validNodes = allTemplates.Where(t =>
                               t.Type == NodeType.Skill ||
                               t.Type == NodeType.Condition ||
                               t.Type == NodeType.Target).ToList();
                       }
                       else if (logicType == "AND" || logicType == "OR")
                       {
                           // AND/OR output (Execution) → Skill, Condition, or another Logic
                           validNodes = allTemplates.Where(t =>
                               t.Type == NodeType.Skill ||
                               t.Type == NodeType.Condition ||
                               t.Type == NodeType.Logic ||
                               t.Type == NodeType.Target).ToList();
                       }
                       else if (logicType == "Sequence")
                       {
                           // Sequence → similar to AND/OR
                           validNodes = allTemplates.Where(t =>
                               t.Type == NodeType.Skill ||
                               t.Type == NodeType.Condition ||
                               t.Type == NodeType.Logic).ToList();
                       }
                       else // Delay
                       {
                           // Delay → Skill, Condition, or another Logic node
                           validNodes = allTemplates.Where(t =>
                               t.Type == NodeType.Skill ||
                               t.Type == NodeType.Condition ||
                               t.Type == NodeType.Logic).ToList();
                       }
                       break;
               }

               // Final filter: ensure target node has at least one compatible input pin
               return validNodes.Where(t => HasCompatiblePin(t, sourcePin)).Distinct().ToList();
           }

           private bool IsLogicNode(ComboNode node, params string[] logicTypes)
           {
               if (node.Type != NodeType.Logic) return false;
               if (!node.Properties.ContainsKey("LogicType")) return false;
               string nodeLogicType = node.Properties["LogicType"].ToString();
               return logicTypes.Contains(nodeLogicType);
           }

        private bool HasCompatiblePin(ComboNode node, NodePin sourcePin)
        {
            var pins = sourcePin.IsInput ? node.OutputPins : node.InputPins;
            return pins.Any(p => ArePinTypesCompatible(sourcePin.Type, p.Type));
        }

        private NodePin GetFirstCompatiblePin(ComboNode node, NodePin sourcePin)
        {
            var pins = sourcePin.IsInput ? node.OutputPins : node.InputPins;
            return pins.FirstOrDefault(p => ArePinTypesCompatible(sourcePin.Type, p.Type));
        }

        private bool ArePinTypesCompatible(PinType a, PinType b)
        {
            if (a == b) return true;
            // Allow Trigger to connect to Execution pins
            if ((a == PinType.Trigger && b == PinType.Execution) ||
                (a == PinType.Execution && b == PinType.Trigger))
                return true;
            return false;
        }
    }

    // Extension methods for rounded rectangles
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, 
                                               Rectangle bounds, int radius, 
                                               bool topOnly = false, bool bottomOnly = false)
        {
            using (var path = CreateRoundedRectangle(bounds, radius, topOnly, bottomOnly))
            {
                g.FillPath(brush, path);
            }
        }

        public static void FillRoundedRectangle(this Graphics g, Brush brush, 
                                               int x, int y, int width, int height, int radius)
        {
            FillRoundedRectangle(g, brush, new Rectangle(x, y, width, height), radius);
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle bounds, int radius)
        {
            using (var path = CreateRoundedRectangle(bounds, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius, 
                                                          bool topOnly = false, bool bottomOnly = false)
        {
            var path = new GraphicsPath();
            
            if (topOnly)
            {
                path.AddArc(bounds.X, bounds.Y, radius * 2, radius * 2, 180, 90);
                path.AddArc(bounds.Right - radius * 2, bounds.Y, radius * 2, radius * 2, 270, 90);
                path.AddLine(bounds.Right, bounds.Y + radius, bounds.Right, bounds.Bottom);
                path.AddLine(bounds.Right, bounds.Bottom, bounds.X, bounds.Bottom);
                path.AddLine(bounds.X, bounds.Bottom, bounds.X, bounds.Y + radius);
            }
            else if (bottomOnly)
            {
                path.AddLine(bounds.X, bounds.Y, bounds.Right, bounds.Y);
                path.AddLine(bounds.Right, bounds.Y, bounds.Right, bounds.Bottom - radius);
                path.AddArc(bounds.Right - radius * 2, bounds.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                path.AddLine(bounds.X, bounds.Bottom - radius, bounds.X, bounds.Y);
            }
            else
            {
                path.AddArc(bounds.X, bounds.Y, radius * 2, radius * 2, 180, 90);
                path.AddArc(bounds.Right - radius * 2, bounds.Y, radius * 2, radius * 2, 270, 90);
                path.AddArc(bounds.Right - radius * 2, bounds.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            }
            
            path.CloseFigure();
            return path;
        }
    }

    // Dark theme renderer for context menus
    public class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkMenuColors()) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                using (var brush = new SolidBrush(Color.FromArgb(60, 60, 65)))
                {
                    e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
                }
            }
            else
            {
                using (var brush = new SolidBrush(Color.FromArgb(40, 40, 40)))
                {
                    e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
                }
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = Color.White;
            base.OnRenderItemText(e);
        }
    }

    public class DarkMenuColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Color.FromArgb(60, 60, 65);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(60, 60, 65);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(60, 60, 65);
        public override Color MenuItemBorder => Color.FromArgb(80, 80, 85);
        public override Color MenuBorder => Color.FromArgb(40, 40, 40);
        public override Color ImageMarginGradientBegin => Color.FromArgb(40, 40, 40);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(40, 40, 40);
        public override Color ImageMarginGradientEnd => Color.FromArgb(40, 40, 40);
        public override Color SeparatorDark => Color.FromArgb(40, 40, 40);
        public override Color SeparatorLight => Color.FromArgb(40, 40, 40);
    }
}
