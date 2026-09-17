using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace AzzyAIConfig
{
    public class NodeLibrary
    {
        public static List<ComboNode> GetAllNodeTemplates()
        {
            var templates = new List<ComboNode>();
            
            // Skill nodes
            templates.AddRange(GetSkillNodes());
            
            // Condition nodes
            templates.AddRange(GetConditionNodes());
            
            // Logic nodes
            templates.AddRange(GetLogicNodes());
            
            // Trigger nodes
            templates.AddRange(GetTriggerNodes());
            
            // Target nodes
            templates.AddRange(GetTargetNodes());
            
            // End node (combo terminator)
            templates.AddRange(GetEndNodes());
            
            return templates;
        }

        public static ComboNode GetTemplateByTitle(string title)
        {
            var all = GetAllNodeTemplates();
            foreach (var t in all)
            {
                if (string.Equals(t.Title, title, StringComparison.OrdinalIgnoreCase))
                    return t;
            }

            // Existing blueprints used a few pre-catalog titles (for example
            // "Warm Def" and "Illusion of Claws"). Resolve those by skill ID
            // so loading a blueprint also upgrades it to the current name and
            // metadata.
            int skillId = KimiSkills.GetSkillId(title);
            if (skillId != 0 || string.Equals(title, "(None)", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var t in all)
                {
                    if (t.Type == NodeType.Skill && t.Properties.ContainsKey("SkillID") &&
                        Convert.ToInt32(t.Properties["SkillID"]) == skillId)
                        return t;
                }
            }
            return null;
        }

        private static List<ComboNode> GetSkillNodes()
        {
            var nodes = new List<ComboNode>();
            // Combat/Offensive skills - RED
            Color combatColor = Color.FromArgb(60, 20, 20);
            Color combatHeader = Color.FromArgb(150, 40, 40);
            // Support skills - GREEN
            Color supportColor = Color.FromArgb(20, 60, 20);
            Color supportHeader = Color.FromArgb(30, 100, 30);

            // Auto-attack is a pseudo skill and remains available to every
            // Kimi. Real cast nodes come exclusively from active catalog
            // entries, which keeps passive skills out of the toolbox.
            nodes.Add(CreateAutoAttackNode(combatHeader, combatColor));
            foreach (var skill in KimiSkills.GetActiveDefinitions())
            {
                nodes.Add(CreateSkillNode(skill,
                    skill.IsSupport ? supportHeader : combatHeader,
                    skill.IsSupport ? supportColor : combatColor));
            }

            return nodes;
        }

        private static ComboNode CreateAutoAttackNode(Color header, Color body)
        {
            return CreateSkillNode("Auto-Attack", -1, 1, "Enemy", false, header, body,
                "Basic attack with no skill level or SP cost.");
        }

        private static ComboNode CreateSkillNode(KimiSkillDefinition skill, Color header, Color body)
        {
            return CreateSkillNode(skill.Name, skill.Id, skill.MaxLevel, skill.DefaultTarget,
                skill.IsSupport, header, body, GetSkillDescription(skill));
        }

        private static ComboNode CreateSkillNode(string name, int skillId, int maxLevel,
                                                 string defaultTarget, bool isSupport,
                                                 Color header, Color body, string description)
        {
            var node = new ComboNode
            {
                Type = NodeType.Skill,
                Category = isSupport ? NodeCategory.SupportSkill : NodeCategory.OffensiveSkill,
                Title = name,
                Description = description,
                HeaderColor = header,
                BodyColor = body,
                Size = new Size(650, 90)
            };

            // ONLY skill-specific properties
            node.Properties["SkillID"] = skillId;
            if (skillId != -1)  // Regular skills get SkillLevel
            {
                node.Properties["SkillLevel"] = 1;
                node.Properties["SkillMaxLevel"] = maxLevel;
                node.Properties["RepeatCount"] = 1;
            }
            else  // Auto-Attack only gets RepeatCount
            {
                node.Properties["RepeatCount"] = 1;
            }
            node.Properties["TargetMode"] = string.IsNullOrEmpty(defaultTarget) ? "Enemy" : defaultTarget;

            // Input pins
            node.InputPins.Add(new NodePin 
            { 
                Name = "Execute", 
                Type = PinType.Execution, 
                IsInput = true, 
                Index = 0,
                PinColor = Color.White 
            });
            node.InputPins.Add(new NodePin 
            { 
                Name = "Target", 
                Type = PinType.Data, 
                IsInput = true, 
                Index = 1,
                PinColor = Color.FromArgb(255, 100, 100)
            });

            // Output pins
            node.OutputPins.Add(new NodePin 
            { 
                Name = "Then", 
                Type = PinType.Execution, 
                IsInput = false, 
                Index = 0,
                PinColor = Color.White 
            });
            node.OutputPins.Add(new NodePin 
            { 
                Name = "On Success", 
                Type = PinType.Execution, 
                IsInput = false, 
                Index = 1,
                PinColor = Color.FromArgb(100, 255, 100)
            });
            node.OutputPins.Add(new NodePin 
            { 
                Name = "On Fail", 
                Type = PinType.Execution, 
                IsInput = false, 
                Index = 2,
                PinColor = Color.FromArgb(255, 100, 100)
            });

            return node;
        }

        private static string GetSkillDescription(KimiSkillDefinition skill)
        {
            return skill.Name + " (" + skill.Id + ") - " + skill.Type +
                   ". Default target: " + skill.DefaultTarget +
                   ". Valid levels: 1-" + skill.MaxLevel + ".";
        }

        private static List<ComboNode> GetConditionNodes()
        {
            var nodes = new List<ComboNode>();
            Color condColor = Color.FromArgb(30, 60, 80);
            Color condHeader = Color.FromArgb(50, 90, 120);

            nodes.Add(CreateConditionNode("Kimi HP %", "KimiHP", condHeader, condColor));
            nodes.Add(CreateConditionNode("Owner HP %", "OwnerHP", condHeader, condColor));
            nodes.Add(CreateConditionNode("Kimi SP %", "KimiSP", condHeader, condColor));
            nodes.Add(CreateConditionNode("Owner SP %", "OwnerSP", condHeader, condColor));
            nodes.Add(CreateConditionNode("Mob Count", "MobCount", condHeader, condColor));
            nodes.Add(CreateConditionNode("Distance to Target", "Distance", condHeader, condColor));
            nodes.Add(CreateConditionNode("Skill on Cooldown", "Cooldown", condHeader, condColor));
            nodes.Add(CreateConditionNode("In Combat", "InCombat", condHeader, condColor));

            // Kimi type gate (Ward=1, Occult=2, Agile=3, Raging=4)
            var kimiType = CreateConditionNode("Kimi Type", "HomuType", condHeader, condColor);
            kimiType.Properties["Operator"] = "==";
            kimiType.Properties["Value"] = 1; // Default to Ward, editable in PropertyGrid
            nodes.Add(kimiType);

            return nodes;
        }

        private static ComboNode CreateConditionNode(string name, string condType, Color header, Color body)
        {
            var node = new ComboNode
            {
                Type = NodeType.Condition,
                Title = name,
                Description = GetConditionDescription(name, condType),
                HeaderColor = header,
                BodyColor = body,
                Size = new Size(380, 220)
            };

            // ONLY condition-specific properties
            node.Properties["ConditionType"] = condType;
            node.Properties["Operator"] = ">=";
            node.Properties["Value"] = 50;

            // Input pins
            node.InputPins.Add(new NodePin 
            { 
                Name = "Check", 
                Type = PinType.Execution, 
                IsInput = true, 
                Index = 0,
                PinColor = Color.White 
            });

            // Output pins - True/False branches + Value data (for AND/OR logic gates)
            node.OutputPins.Add(new NodePin 
            { 
                Name = "True", 
                Type = PinType.Execution, 
                IsInput = false, 
                Index = 0,
                PinColor = Color.FromArgb(100, 255, 100)
            });
            node.OutputPins.Add(new NodePin 
            { 
                Name = "False", 
                Type = PinType.Execution, 
                IsInput = false, 
                Index = 1,
                PinColor = Color.FromArgb(255, 100, 100)
            });
            node.OutputPins.Add(new NodePin 
            { 
                Name = "Value", 
                Type = PinType.Data, 
                IsInput = false, 
                Index = 2,
                PinColor = Color.FromArgb(100, 180, 255)
            });

            return node;
        }

        private static string GetConditionDescription(string name, string condType)
        {
            if (name == "Kimi HP %")
                return "NEXT: Use >= to trigger healing (e.g., >= 50% is low threshold). Chain to Chaotic Heal or Warm Def nodes. Common gate: <= 30% for emergency skills.";
            else if (name == "Owner HP %")
                return "NEXT: Gate support skills when master is in danger. Use >= 50% to assist owner, <= 30% for emergency saves. Pair with Body Double as fallback.";
            else if (name == "Kimi SP %")
                return "NEXT: Prevent skill spam when SP is low. Use >= 60% to ensure enough SP for costly skills. Pair with Loop nodes to control casting frequency.";
            else if (name == "Owner SP %")
                return "NEXT: Synchronize with master's rotation. Gate AoE skills to when master has sufficient SP. Useful for coordinated builds.";
            else if (name == "Mob Count")
                return "NEXT: Gate AoE skills (Illusion of Light) to trigger only with multiple enemies >= 2. Use <= 1 for solo-target only skills.";
            else if (name == "Distance to Target")
                return "NEXT: Gate melee skills (<= 3) vs ranged (>= 5). Prevent Crusher spam at distance. Create range-based skill chains.";
            else if (name == "Skill on Cooldown")
                return "NEXT: Prevent skill spam and cooldown issues. Pair with Delay nodes to add safety margins (check before next skill cast).";
            else if (name == "In Combat")
                return "NEXT: Gate aggressive combos to active combat only. Use FALSE path for idle/buff skills. Best practice: aggressive skills only when In Combat = TRUE.";
            else if (name == "Kimi Type")
                return "NEXT: Create type-specific combos. Ward combo separate from Occult. Use == operator. Select type in properties (1=Ward, 2=Occult, 3=Agile, 4=Raging).";
            else
                return "NEXT: Gate actions with this condition. Connect True path to skill, False to fallback or delay.";
        }

        private static List<ComboNode> GetLogicNodes()
        {
            var nodes = new List<ComboNode>();
            Color logicColor = Color.FromArgb(40, 40, 60);
            Color logicHeader = Color.FromArgb(60, 60, 90);

            nodes.Add(CreateLogicNode("AND", "AND", logicHeader, logicColor));
            nodes.Add(CreateLogicNode("OR", "OR", logicHeader, logicColor));
            nodes.Add(CreateLogicNode("XOR", "XOR", logicHeader, logicColor));
            nodes.Add(CreateLogicNode("NAND", "NAND", logicHeader, logicColor));
            nodes.Add(CreateLogicNode("NOR", "NOR", logicHeader, logicColor));
            nodes.Add(CreateLogicNode("NOT", "NOT", logicHeader, logicColor));
            nodes.Add(CreateLogicNode("Sequence", "Sequence", logicHeader, logicColor));
            nodes.Add(CreateLogicNode("Loop N Times", "Loop", logicHeader, logicColor));
            nodes.Add(CreateLogicNode("Wait Delay", "Delay", logicHeader, logicColor));
            
            // Combo Delay removed (already covered by Wait Delay)
            // Target Selector removed (replaced by Set Target node in GetTargetNodes)

            return nodes;
        }

        private static ComboNode CreateLogicNode(string name, string logicType, Color header, Color body)
        {
            var node = new ComboNode
            {
                Type = NodeType.Logic,
                Category = NodeCategory.LogicGate,
                Title = name,
                Description = GetLogicDescription(name, logicType),
                HeaderColor = header,
                BodyColor = body,
                Size = new Size(450, 200)
            };

            // ONLY logic-specific properties (conditionally added)
            node.Properties["LogicType"] = logicType;
            
            if (logicType == "Loop")
            {
                node.Properties["LoopCount"] = 3;
            }
            else if (logicType == "Delay")
            {
                node.Properties["DelayMs"] = 500;
            }
            // AND/OR/Sequence have NO additional properties beyond LogicType

            // Input pins
            node.InputPins.Add(new NodePin 
            { 
                Name = "In", 
                Type = PinType.Execution, 
                IsInput = true, 
                Index = 0,
                PinColor = Color.White 
            });

            if (logicType == "AND" || logicType == "OR" || logicType == "XOR" || logicType == "NAND" || logicType == "NOR")
            {
                node.InputPins.Add(new NodePin 
                { 
                    Name = "A", 
                    Type = PinType.Data, 
                    IsInput = true, 
                    Index = 1,
                    PinColor = Color.FromArgb(100, 180, 255)
                });
                node.InputPins.Add(new NodePin 
                { 
                    Name = "B", 
                    Type = PinType.Data, 
                    IsInput = true, 
                    Index = 2,
                    PinColor = Color.FromArgb(100, 180, 255)
                });
            }
            else if (logicType == "NOT")
            {
                // NOT only needs input A
                node.InputPins.Add(new NodePin 
                { 
                    Name = "A", 
                    Type = PinType.Data, 
                    IsInput = true, 
                    Index = 1,
                    PinColor = Color.FromArgb(100, 180, 255)
                });
            }

            // Output pins
            node.OutputPins.Add(new NodePin 
            { 
                Name = "Out", 
                Type = PinType.Execution, 
                IsInput = false, 
                Index = 0,
                PinColor = Color.White 
            });

            if (logicType == "Loop")
            {
                node.OutputPins.Add(new NodePin 
                { 
                    Name = "Loop Body", 
                    Type = PinType.Execution, 
                    IsInput = false, 
                    Index = 1,
                    PinColor = Color.FromArgb(180, 180, 100)
                });
                node.OutputPins.Add(new NodePin 
                { 
                    Name = "Completed", 
                    Type = PinType.Execution, 
                    IsInput = false, 
                    Index = 2,
                    PinColor = Color.FromArgb(100, 255, 100)
                });
            }

            return node;
        }

        private static string GetLogicDescription(string name, string logicType)
        {
            if (name == "AND")
                return "NEXT: Both inputs must be TRUE. Drag Condition 'Value' pins to inputs A & B. Outputs Execution to next node only if both TRUE.";
            else if (name == "OR")
                return "NEXT: Either input is TRUE. Drag Condition 'Value' pins to inputs A & B. Outputs Execution to next node if either TRUE.";
            else if (name == "XOR")
                return "NEXT: Exclusive OR - exactly one input TRUE (not both). Drag Condition 'Value' pins to inputs A & B. True if A!=B.";
            else if (name == "NAND")
                return "NEXT: NOT AND - false only if both TRUE. Drag Condition 'Value' pins to inputs A & B. Negates AND logic.";
            else if (name == "NOR")
                return "NEXT: NOT OR - true only if both FALSE. Drag Condition 'Value' pins to inputs A & B. Negates OR logic.";
            else if (name == "NOT")
                return "NEXT: Negate input A. Drag Condition 'Value' pin to input A only. Outputs TRUE if A is FALSE, vice versa.";
            else if (name == "Sequence")
                return "NEXT: Execute inputs in strict order. Use for time-sensitive chains: Crusher -> Claws -> Heal. Each step must complete before next.";
            else if (name == "Loop N Times")
                return "NEXT: Repeat execution N times. Set LoopCount 2-5 in properties. Use for spam chains (Claws x3). Caution: values > 10 can cause lag.";
            else if (name == "Wait Delay")
                return "NEXT: Add pause before next skill. Set DelayMs: 100-300ms for spam, 500-1000ms for spacing, 2000+ for long buffs. Prevents action overlap.";
            else
                return "NEXT: Connect this logic node to control skill execution flow. Use AND/OR/XOR/NAND/NOR for conditions, NOT for negation, Sequence for order, Delay for timing.";
        }

        private static List<ComboNode> GetTriggerNodes()
        {
            var nodes = new List<ComboNode>();
            Color triggerColor = Color.FromArgb(60, 40, 20);
            Color triggerHeader = Color.FromArgb(100, 70, 40);

            nodes.Add(CreateTriggerNode("On Attack State", "OnAttack", triggerHeader, triggerColor));
            nodes.Add(CreateTriggerNode("On Chase State", "OnChase", triggerHeader, triggerColor));
            nodes.Add(CreateTriggerNode("On Idle State", "OnIdle", triggerHeader, triggerColor));
            nodes.Add(CreateTriggerNode("On Target Change", "OnTargetChange", triggerHeader, triggerColor));
            nodes.Add(CreateTriggerNode("On Owner Damage", "OnOwnerDamage", triggerHeader, triggerColor));

            return nodes;
        }

        private static ComboNode CreateTriggerNode(string name, string triggerType, Color header, Color body)
        {
            var node = new ComboNode
            {
                Type = NodeType.Trigger,
                Title = name,
                Description = GetTriggerDescription(name, triggerType),
                HeaderColor = header,
                BodyColor = body,
                Size = new Size(220, 200)
            };

            // ONLY trigger-specific property
            node.Properties["TriggerType"] = triggerType;

            // Output pin only: Trigger starts execution flow
            node.OutputPins.Add(new NodePin 
            { 
                Name = "Execute", 
                Type = PinType.Execution, 
                IsInput = false, 
                Index = 0,
                PinColor = Color.White
            });

            return node;
        }

        private static string GetTriggerDescription(string name, string triggerType)
        {
            if (name == "On Attack State")
                return "NEXT: Branch offensive skill chains here. Common path: Attack -> Condition checks -> Skill nodes. Use for primary damage rotation.";
            else if (name == "On Chase State")
                return "NEXT: Gate AoE skills and mobility here. Typical chain: Chase -> Mob Count >= 2 -> Illusion of Light. Different from idle behavior.";
            else if (name == "On Idle State")
                return "NEXT: Create buff refresh loops here. Pattern: Idle -> Warm Def -> Delay 5000ms -> Loop back. Good for passive maintenance.";
            else if (name == "On Target Change")
                return "NEXT: Reset or adapt combo for new enemy. Use to switch from single-target to AoE strategies. Prevents locked-in tactics on new mobs.";
            else if (name == "On Owner Damage")
                return "NEXT: Create emergency protection here. Chain: Owner Damage -> Body Double OR Chaotic Heal. Priority: save owner before continuing offense.";
            else
                return "NEXT: This is a trigger. Connect its 'Execute' output to skill/condition nodes to start combo chains.";
        }

        // NEW NODE TYPE: Range Check (validates target distance)
        private static ComboNode CreateRangeCheckNode(string name, Color header, Color body)
        {
            var node = new ComboNode
            {
                Type = NodeType.Condition,
                Category = NodeCategory.MobCondition,
                Title = name,
                Description = "Check if target is within range",
                HeaderColor = header,
                BodyColor = body,
                Size = new Size(380, 220)
            };

            // ONLY range-specific properties
            node.Properties["ConditionType"] = "Distance";  // For export compatibility
            node.Properties["MinRange"] = 0;
            node.Properties["MaxRange"] = 5;

            node.InputPins.Add(new NodePin { Name = "Check", Type = PinType.Execution, IsInput = true, Index = 0, PinColor = Color.White });
            node.InputPins.Add(new NodePin { Name = "Target", Type = PinType.Data, IsInput = true, Index = 1, PinColor = Color.FromArgb(255, 100, 100) });
            node.OutputPins.Add(new NodePin { Name = "In Range", Type = PinType.Execution, IsInput = false, Index = 0, PinColor = Color.FromArgb(100, 255, 100) });
            node.OutputPins.Add(new NodePin { Name = "Out of Range", Type = PinType.Execution, IsInput = false, Index = 1, PinColor = Color.FromArgb(255, 100, 100) });

            return node;
        }

        private static List<ComboNode> GetTargetNodes()
        {
            var nodes = new List<ComboNode>();
            // Target nodes - PURPLE color for clarity
            Color targetHeader = Color.FromArgb(100, 60, 120);
            Color targetBody = Color.FromArgb(60, 30, 80);

            // Single flexible target node with mode selector
            nodes.Add(CreateSetTargetNode(targetHeader, targetBody));

            return nodes;
        }

        private static List<ComboNode> GetEndNodes()
        {
            var nodes = new List<ComboNode>();
            // End nodes - YELLOW/GOLD color to indicate termination point
            Color endHeader = Color.FromArgb(150, 120, 40);
            Color endBody = Color.FromArgb(80, 70, 20);

            nodes.Add(CreateEndNode(endHeader, endBody));

            return nodes;
        }

        private static ComboNode CreateEndNode(Color header, Color body)
        {
            var node = new ComboNode
            {
                Type = NodeType.Output,  // Reuse Output type for End nodes
                Category = NodeCategory.Sequence,
                Title = "End",
                Description = "Combo terminator - Controls how often combo re-executes",
                HeaderColor = header,
                BodyColor = body,
                Size = new Size(500, 150)
            };

            // End node specific properties
            node.Properties["RepeatMode"] = "Once";  // Once, Always, OncePerTarget, XTimes
            node.Properties["RepeatCount"] = 1;      // For XTimes mode

            // Input pin only (execution flow)
            node.InputPins.Add(new NodePin 
            { 
                Name = "In", 
                Type = PinType.Execution, 
                IsInput = true, 
                Index = 0,
                PinColor = Color.White 
            });

            // No output pins - this is a terminal node

            return node;
        }

        private static ComboNode CreateSetTargetNode(Color header, Color body)
        {
            var node = new ComboNode
            {
                Type = NodeType.Target,
                Category = NodeCategory.Sequence,
                Title = "Set Target",
                Description = "Configure target mode for role-specific smart targeting",
                HeaderColor = header,
                BodyColor = body,
                Size = new Size(550, 120)
            };

            // ONLY target-specific property (role-specific modes)
            node.Properties["TargetMode"] = "Enemy";  // Enemy, Owner, Self, Ally, NearestEnemy, FarthestEnemy, NearestAlly, FarthestAlly, StrongestEnemy, WeakestEnemy, Focus

            // Input pins
            node.InputPins.Add(new NodePin 
            { 
                Name = "Set", 
                Type = PinType.Execution, 
                IsInput = true, 
                Index = 0,
                PinColor = Color.White 
            });

            // Output pins
            node.OutputPins.Add(new NodePin 
            { 
                Name = "Then", 
                Type = PinType.Execution, 
                IsInput = false, 
                Index = 0,
                PinColor = Color.White 
            });
            node.OutputPins.Add(new NodePin 
            { 
                Name = "Target", 
                Type = PinType.Data, 
                IsInput = false, 
                Index = 1,
                PinColor = Color.FromArgb(255, 100, 100) 
            });

            return node;
        }

        private static string GetTargetDescription(string targetType)
        {
            // Kept for reference
            return "Set combo target context.";
        }

        // DEPRECATED: Target Selector (replaced by Set Target node with TargetMode)
        // Keeping for backward compatibility but hidden from toolbox
        private static ComboNode CreateTargetSelectorNode(string name, Color header, Color body)
        {
            var node = new ComboNode
            {
                Type = NodeType.Target,  // Changed from Logic to Target
                Category = NodeCategory.Sequence,
                Title = name,
                Description = "DEPRECATED: Use Set Target node instead",
                HeaderColor = header,
                BodyColor = body,
                Size = new Size(450, 200)
            };

            // Map to TargetMode for consistency
            node.Properties["TargetMode"] = "NearestEnemy"; // Will be replaced by Set Target node

            node.InputPins.Add(new NodePin { Name = "Select", Type = PinType.Execution, IsInput = true, Index = 0, PinColor = Color.White });
            node.OutputPins.Add(new NodePin { Name = "Then", Type = PinType.Execution, IsInput = false, Index = 0, PinColor = Color.White });
            node.OutputPins.Add(new NodePin { Name = "Target", Type = PinType.Data, IsInput = false, Index = 1, PinColor = Color.FromArgb(255, 100, 100) });

            return node;
        }
    }
}
