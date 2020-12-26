using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Singletons;
using Godot;
using Stateless;

namespace CraigStars
{
    public class MapObject : Area2D
    {
        public enum OwnerAlly
        {
            Unknown,
            Known,
            Owned,
            Friend,
            Enemy
        }

        public enum States
        {
            None,
            Selected,
            Active,
        }

        public enum Triggers
        {
            Select, // this node is clicked on
            Activate, // this node is clicked on
            Deselect, // some other node is clicked on
        }

        [Export]
        public States State { get; set; } = States.None;

        [Export]
        public OwnerAlly OwnerAllyState { get; set; } = OwnerAlly.Unknown;

        public String ObjectName { get; set; } = "";

        StateMachine<States, Triggers> selectedMachine;

        public override void _Ready()
        {
            // hook up mouse events to our area
            Connect("input_event", this, nameof(OnInputEvent));

            selectedMachine = new StateMachine<States, Triggers>(() => State, s => State = s);

            // we can transition into the None state from Selected or Active, and we deselect
            selectedMachine.Configure(States.None)
                .OnEntry(() => OnDeselected())
                .Permit(Triggers.Select, States.Selected)
                .Permit(Triggers.Activate, States.Active)
                .PermitReentry(Triggers.Deselect);

            selectedMachine.Configure(States.Selected)
                .OnEntry(() => OnSelected())
                .Permit(Triggers.Select, States.Active)
                .Permit(Triggers.Activate, States.Active)
                .Permit(Triggers.Deselect, States.None);

            selectedMachine.Configure(States.Active)
                .OnEntry(() => OnActivated())
                .PermitReentry(Triggers.Activate)
                .Permit(Triggers.Select, States.Selected)
                .Permit(Triggers.Deselect, States.None);

            selectedMachine.OnTransitioned(t => GD.Print($"OnTransitioned: {ObjectName} {t.Source} -> {t.Destination} via {t.Trigger}({string.Join(", ", t.Parameters)})"));

            // wire up signals
            Signals.MapObjectSelectedEvent += OnMapObjectSelected;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectSelectedEvent -= OnMapObjectSelected;
        }

        /// <summary>
        /// We listen for the MapObjectSelectedEvent so we can deselect ourselves
        /// </summary>
        /// <param name="mapObject"></param>
        internal virtual void OnMapObjectSelected(MapObject mapObject)
        {
            // if a different map object is selected, deselect us
            if (mapObject != this && !GetPeers().Contains(mapObject))
            {
                if (State != States.None)
                {
                    Deselect();
                }
                GetPeers().ForEach(mo =>
                {
                    if (mo.State != States.None)
                    {
                        mo.Deselect();
                        Deselect();
                    }
                });
            }
        }

        protected void ActivateNextPeer(List<MapObject> peers)
        {
            // activate our first peer
            peers[0].Activate();

            // deselect us
            Deselect();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                selectedMachine.Fire(Triggers.Deselect);
            }
        }

        void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Shift)
                {
                    // this was shift+clicked, so let the viewport know it's supposed to be added as a waypoint
                    Signals.PublishMapObjectWaypointAddedEvent(this);
                }
                else
                {
                    // if we have an active peer, activate the next peer in the list
                    var peers = GetPeers();
                    var activePeer = peers.Find(p => p.State == States.Active);
                    if (activePeer != null)
                    {
                        GD.Print($"{ObjectName} clicked, but has active peer. Activating next peer: of {activePeer.ObjectName} is selected");
                        activePeer.ActivateNextPeer(activePeer.GetPeers());
                    }
                    else if (State == States.Active && peers.Count > 0)
                    {
                        // we are already active and have peers, activate the next one
                        ActivateNextPeer(peers);
                    }
                    else
                    {
                        GD.Print($"{ObjectName} is selected");
                        selectedMachine.Fire(Triggers.Select);
                    }

                }
            }
        }

        /// <summary>
        /// True if we have an active peer
        /// </summary>
        /// <param name="peers"></param>
        /// <returns></returns>
        internal bool HasActivePeer(List<MapObject> peers = null)
        {
            if (peers == null)
            {
                peers = GetPeers();
            }
            return peers.Find(p => p.State == States.Active) != null;
        }

        #region Virtuals

        internal virtual List<MapObject> GetPeers() { return Enumerable.Empty<MapObject>().ToList(); }

        protected virtual void OnDeselected() { }
        protected virtual void OnSelected()
        {
            Signals.PublishMapObjectSelectedEvent(this);
        }
        protected virtual void OnActivated()
        {
            // we became active, so publish an activated event for the UI
            Signals.PublishMapObjectActivatedEvent(this);
        }

        public virtual void Activate()
        {
            selectedMachine.Fire(Triggers.Activate);
        }

        public virtual void Deselect()
        {
            selectedMachine.Fire(Triggers.Deselect);
        }

        #endregion

    }
}