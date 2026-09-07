using System;
using System.Drawing;
using System.Collections.Generic;
namespace AzzyAIConfig
{
    public enum NodeType
    {
        Skill,
        Condition,
        Logic,
        Trigger,
        Output
    }

    public enum NodeCategory
    {
        OffensiveSkill,
        SupportSkill,
        HPCondition,
        SPCondition,
        MobCondition,
        LogicGate,
        Sequence,
        StateTransition
    }

    public class ComboNode
    {
        public Guid Id { get; set; }
        public NodeType Type { get; set; }
        public NodeCategory Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Point Position { get; set; }
        public Size Size { get; set; }
        public Color HeaderColor { get; set; }
        public Color BodyColor { get; set; }
        
        // Node data
        public Dictionary<string, object> Properties { get; set; }
        
        // Connection points
        public List<NodePin> InputPins { get; set; }
        public List<NodePin> OutputPins { get; set; }
        
        // Visual state
        public bool IsSelected { get; set; }
        public bool IsHovered { get; set; }
        
        public ComboNode()
        {
            Id = Guid.NewGuid();
            Properties = new Dictionary<string, object>();
            InputPins = new List<NodePin>();
            OutputPins = new List<NodePin>();
            Size = new Size(180, 100);
            BodyColor = Color.FromArgb(40, 40, 45);
            HeaderColor = Color.FromArgb(60, 60, 65);
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(Position, Size);
        }

        public Rectangle GetHeaderBounds()
        {
            return new Rectangle(Position.X, Position.Y, Size.Width, 30);
        }

        public NodePin GetPinAt(Point point)
        {
            foreach (var pin in InputPins)
            {
                if (pin.GetBounds(this).Contains(point))
                    return pin;
            }
            foreach (var pin in OutputPins)
            {
                if (pin.GetBounds(this).Contains(point))
                    return pin;
            }
            return null;
        }
    }

    public enum PinType
    {
        Execution,  // Flow control
        Data,       // Data passing
        Trigger     // Event trigger
    }

    public class NodePin
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public PinType Type { get; set; }
        public bool IsInput { get; set; }
        public int Index { get; set; }
        public Color PinColor { get; set; }
        
        public List<Guid> Connections { get; set; }

        public NodePin()
        {
            Id = Guid.NewGuid();
            Connections = new List<Guid>();
            PinColor = Color.White;
        }

        public Rectangle GetBounds(ComboNode parent)
        {
            int pinSize = 12;
            int x = IsInput ? parent.Position.X - pinSize / 2 : 
                              parent.Position.X + parent.Size.Width - pinSize / 2;
            int y = parent.Position.Y + 35 + (Index * 25);
            return new Rectangle(x, y, pinSize, pinSize);
        }

        public Point GetCenter(ComboNode parent)
        {
            var bounds = GetBounds(parent);
            return new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
        }
    }

    public class NodeConnection
    {
        public Guid Id { get; set; }
        public Guid SourceNodeId { get; set; }
        public Guid SourcePinId { get; set; }
        public Guid TargetNodeId { get; set; }
        public Guid TargetPinId { get; set; }
        public Color LineColor { get; set; }

        public NodeConnection()
        {
            Id = Guid.NewGuid();
            LineColor = Color.FromArgb(180, 180, 180);

            // Event args for node and connection events  
            public class NodeEventArgs : EventArgs
            {
                public ComboNode Node { get; set; }
                public NodeEventArgs(ComboNode node) { Node = node; }
            }

            public class ConnectionEventArgs : EventArgs
            {
                public NodeConnection Connection { get; set; }
                public ConnectionEventArgs(NodeConnection connection) { Connection = connection; }
            }
        }
    }
}
