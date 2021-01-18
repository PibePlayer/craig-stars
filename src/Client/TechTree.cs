using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Utils;
using CraigStars.Singletons;

namespace CraigStars
{
    public class TechTree : VBoxContainer
    {
        /// <summary>
        /// Fired when the selected tech changes
        /// </summary>
        public event Action<Tech> TechSelectedEvent;

        [Export]
        public bool HullComponentsOnly { get; set; }

        [Export]
        public bool ShipHullsOnly { get; set; }

        [Export]
        public bool StarbaseHullsOnly { get; set; }

        Tree techTree;
        TreeItem root;

        LineEdit searchLineEdit;
        CheckButton onlyAvailableCheckButton;

        List<Tech> techs = new List<Tech>();
        Dictionary<TechCategory, TreeItem> categoryTreeItemByCategory = new Dictionary<TechCategory, TreeItem>();
        Dictionary<Tech, TreeItem> techTreeItemByTech = new Dictionary<Tech, TreeItem>();

        public override void _Ready()
        {
            techTree = FindNode("Tree") as Tree;
            searchLineEdit = FindNode("SearchLineEdit") as LineEdit;
            onlyAvailableCheckButton = FindNode("OnlyAvailableCheckButton") as CheckButton;

            techTree.Connect("item_selected", this, nameof(OnTechSelected));
            onlyAvailableCheckButton.Connect("toggled", this, nameof(OnOnlyAvailableCheckButtonToggled));
            searchLineEdit.Connect("text_changed", this, nameof(OnSearchLineEditTextChanged));

            // populate the tree
            ClearTree();
            AddCategoriesToTree(TechStore.Instance.Techs);
            AddTechsToTree(TechStore.Instance.Techs);
        }

        public void SelectFirstTech()
        {
            // select the first item in the first category
            var firstCategoryTreeItem = categoryTreeItemByCategory[TechStore.Instance.Categories[0]] as TreeItem;
            firstCategoryTreeItem.GetChildren()?.Select(0);
        }

        /// <summary>
        /// You can't hide items, so we clear the tree when we change items
        /// </summary>
        void ClearTree()
        {
            techTree.Clear();
            root = techTree.CreateItem();
        }

        void UpdateTreeItems()
        {
            List<Tech> techsToShow;
            if (HullComponentsOnly)
            {
                techsToShow = TechStore.Instance.HullComponents.Cast<Tech>().ToList();
            }
            else if (ShipHullsOnly)
            {
                techsToShow = TechStore.Instance.ShipHulls.Cast<Tech>().ToList();
            }
            else if (StarbaseHullsOnly)
            {
                techsToShow = TechStore.Instance.StarbaseHulls.Cast<Tech>().ToList();
            }
            else
            {
                techsToShow = TechStore.Instance.Techs;
            }

            // filter techs based on search result
            if (searchLineEdit.Text.Trim() != "")
            {
                techsToShow = techsToShow.Where(tech => tech.Name.ToLower().Contains(searchLineEdit.Text.Trim().ToLower())).ToList();
            }

            if (onlyAvailableCheckButton.Pressed)
            {
                techsToShow = techsToShow.Where(tech => PlayersManager.Instance.Me.HasTech(tech)).ToList();
            }

            ClearTree();
            AddCategoriesToTree(techsToShow);
            AddTechsToTree(techsToShow);
        }

        /// <summary>
        /// Add categories for our techs
        /// </summary>
        /// <param name="techs"></param>
        void AddCategoriesToTree(List<Tech> techs)
        {
            // get a list of categories, sorted.
            foreach (var category in TechStore.Instance.GetCategoriesForTechs(techs))
            {
                var categoryTreeItem = techTree.CreateItem(root);
                categoryTreeItem.SetText(0, category.ToString());
                categoryTreeItemByCategory[category] = categoryTreeItem;
            }

        }

        /// <summary>
        /// Add techs to the tree, sorted by category
        /// </summary>
        void AddTechsToTree(List<Tech> techsToAdd)
        {
            techs = new List<Tech>(techsToAdd);
            techs.Sort((t1, t2) => t1.Ranking.CompareTo(t2.Ranking));

            techs.Each((tech, index) =>
            {
                var categoryRoot = categoryTreeItemByCategory[tech.Category];
                var item = techTree.CreateItem(categoryRoot);
                techTreeItemByTech[tech] = item;
                item.SetMetadata(0, index);
                item.SetText(0, tech.Name);
                if (tech is TechHull)
                {
                    item.SetIcon(0, TextureLoader.Instance.FindTexture(tech, 0));
                }
                else
                {
                    item.SetIcon(0, TextureLoader.Instance.FindTexture(tech));
                }
            });
        }

        /// <summary>
        /// Change the active tech
        /// </summary>
        void OnTechSelected()
        {
            var selected = techTree.GetSelected();
            if (selected.GetMetadata(0) is int index)
            {
                var tech = techs[index];
                TechSelectedEvent?.Invoke(tech);
            }
        }

        void OnSearchLineEditTextChanged(string newText)
        {
            UpdateTreeItems();
        }

        /// <summary>
        /// Only show available tech
        /// </summary>
        /// <param name="buttonPressed"></param>
        void OnOnlyAvailableCheckButtonToggled(bool buttonPressed)
        {
            UpdateTreeItems();
            SelectFirstTech();
        }
    }
}