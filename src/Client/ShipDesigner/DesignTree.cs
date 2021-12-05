using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class DesignTree : Control
    {
        public enum DesignsToShow { All, Ships, Starbases }

        [Export]
        public DesignsToShow ShowDesigns { get; set; } = DesignsToShow.All;

        /// <summary>
        /// Fired when the selected tech changes
        /// </summary>
        public event Action<ShipDesign> DesignSelectedEvent;

        LineEdit searchLineEdit;
        Tree tree;
        TreeItem root;

        Player Me { get => PlayersManager.Me; }

        List<ShipDesign> designs = new List<ShipDesign>();
        Dictionary<ShipDesign, TreeItem> techTreeItemByTech = new Dictionary<ShipDesign, TreeItem>();


        public override void _Ready()
        {
            tree = FindNode("Tree") as Tree;
            searchLineEdit = FindNode("SearchLineEdit") as LineEdit;

            tree.Connect("item_selected", this, nameof(OnDesignSelected));

            Connect("visibility_changed", this, nameof(OnVisible));
        }

        public void SelectFirstItem()
        {
            // select the first item in the first category
            root.GetChildren()?.Select(0);
        }

        /// <summary>
        /// Wire up events
        /// </summary>
        void OnVisible()
        {
            if (IsVisibleInTree())
            {
                UpdateTreeItems();
                SelectFirstItem();
            }
        }

        /// <summary>
        /// You can't hide items, so we clear the tree when we change items
        /// </summary>
        void ClearTree()
        {
            tree.Clear();
            root = tree.CreateItem();
        }

        public void UpdateTreeItems()
        {
            List<ShipDesign> designsToShow = Me.Designs;
            switch (ShowDesigns)
            {
                case DesignsToShow.Ships:
                    designsToShow = Me.Designs.Where(design => !design.Hull.Starbase).ToList();
                    break;
                case DesignsToShow.Starbases:
                    designsToShow = Me.Designs.Where(design => design.Hull.Starbase).ToList();
                    break;
                default:
                    designsToShow = Me.Designs;
                    break;
            }


            // filter designs based on search result
            if (searchLineEdit.Text.Trim() != "")
            {
                designsToShow = designsToShow.Where(tech => tech.Name.ToLower().Contains(searchLineEdit.Text.Trim().ToLower())).ToList();
            }

            ClearTree();
            AddDesignsToTree(designsToShow);
        }

        /// <summary>
        /// Add designs to the tree, sorted by category
        /// </summary>
        void AddDesignsToTree(List<ShipDesign> designsToAdd)
        {
            designs = new List<ShipDesign>(designsToAdd);
            // TODO: sort by power
            designs.Sort((t1, t2) =>
            {
                var compare = t1.Hull.Ranking.CompareTo(t2.Hull.Ranking);
                if (compare == 0)
                {
                    compare = t1.Hull.Name.CompareTo(t2.Hull.Name);
                    if (compare == 0)
                    {
                        compare = t1.Version.CompareTo(t2.Version);
                    }
                }
                return compare;
            });

            designs.Each((design, index) =>
            {
                var item = tree.CreateItem(root);
                techTreeItemByTech[design] = item;
                item.SetMetadata(0, index);
                item.SetText(0, $"{design.Name} v{design.Version}");
                item.SetIcon(0, TextureLoader.Instance.FindTexture(design.Hull, design.HullSetNumber));
            });
        }

        /// <summary>
        /// Change the active tech
        /// </summary>
        void OnDesignSelected()
        {
            var selected = tree.GetSelected();
            if (selected.GetMetadata(0) is int index)
            {
                var design = designs[index];
                DesignSelectedEvent?.Invoke(design);
            }
        }

    }
}