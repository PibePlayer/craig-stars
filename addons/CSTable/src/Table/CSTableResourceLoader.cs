using Godot;

namespace CraigStarsTable
{
    /// <summary>
    /// A singleton node for creating new Server scene trees when multiplayer games are hosted
    /// </summary>
    public class CSTableResourceLoader : Node
    {

        public const string DefaultCellControlScript = "res://addons/CSTable/src/Table/CSLabelCell.cs";
        public const string DefaultColumnHeaderScene = "res://addons/CSTable/src/Table/ColumnHeader.tscn";

        public CSharpScript DefaultCellControl { get; set; }
        public PackedScene DefaultColumnHeader { get; set; }

        /// <summary>
        /// Server is a singleton
        /// </summary>
        private static CSTableResourceLoader instance;
        public static CSTableResourceLoader Instance
        {
            get
            {
                return instance;
            }
        }

        CSTableResourceLoader()
        {
            instance = this;
        }

        public override void _Ready()
        {
            base._Ready();
            instance = this;

            DefaultColumnHeader = ResourceLoader.Load<PackedScene>(DefaultColumnHeaderScene);
            DefaultCellControl = ResourceLoader.Load<CSharpScript>(DefaultCellControlScript);
        }
    }
}
