using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class ShipDesignerDialog : WindowDialog
    {

        Button okButton;
        Player me;

        DesignTree shipDesignTree;
        HullSummary shipHullSummary;

        DesignTree starbaseDesignTree;
        HullSummary starbaseHullSummary;

        TechTree hullsTechTree;
        HullSummary hullHullSummary;

        TechTree hullComponentsTechTree;
        HullSummary designerHullSummary;

        public override void _Ready()
        {
            me = PlayersManager.Instance.Me;
            okButton = FindNode("OKButton") as Button;
            shipDesignTree = FindNode("ShipDesignTree") as DesignTree;
            shipHullSummary = FindNode("ShipHullSummary") as HullSummary;

            starbaseDesignTree = FindNode("StarbaseDesignTree") as DesignTree;
            starbaseHullSummary = FindNode("StarbaseHullSummary") as HullSummary;

            hullsTechTree = FindNode("HullsTechTree") as TechTree;
            hullHullSummary = FindNode("HullHullSummary") as HullSummary;

            hullComponentsTechTree = FindNode("HullComponentsTechTree") as TechTree;
            designerHullSummary = FindNode("DesignerHullSummary") as HullSummary;

            shipDesignTree.DesignSelectedEvent += OnShipDesignSelectedEvent;
            starbaseDesignTree.DesignSelectedEvent += OnStarbaseDesignSelectedEvent;
            hullsTechTree.TechSelectedEvent += OnHullSelectedEvent;
            hullComponentsTechTree.TechSelectedEvent += OnHullComponentSelectedEvent;

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));
            okButton.Connect("pressed", this, nameof(OnOk));
        }

        public override void _ExitTree()
        {
            shipDesignTree.DesignSelectedEvent -= OnShipDesignSelectedEvent;
            starbaseDesignTree.DesignSelectedEvent -= OnStarbaseDesignSelectedEvent;
        }

        void OnShipDesignSelectedEvent(ShipDesign design)
        {
            shipHullSummary.ShipDesign = design;
            shipHullSummary.Hull = design.Hull;
        }

        void OnStarbaseDesignSelectedEvent(ShipDesign design)
        {
            starbaseHullSummary.ShipDesign = design;
            starbaseHullSummary.Hull = design.Hull;
        }

        void OnHullSelectedEvent(Tech tech)
        {
            if (tech is TechHull hull)
            {
                hullHullSummary.Hull = hull;
            }
        }

        void OnHullComponentSelectedEvent(Tech tech)
        {
            if (tech is TechHullComponent hullComponent)
            {
                // todo do something with the designer
            }
        }

        /// <summary>
        /// Hide show
        /// </summary>
        void OnAboutToShow()
        {
            designerHullSummary.Hull = Techs.Scout;
            designerHullSummary.ShipDesign = new ShipDesign()
            {
                Name = "New Scout",
                Hull = Techs.Scout,
                
            };
        }

        /// <summary>
        /// Called when the popup hides
        /// </summary>
        void OnPopupHide()
        {
        }

        /// <summary>
        /// Just hide the dialog on ok
        /// </summary>
        void OnOk()
        {
            Hide();
        }

    }
}