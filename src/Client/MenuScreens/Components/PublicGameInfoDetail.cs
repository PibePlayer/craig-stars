using System;
using System.Collections.Generic;
using CraigStars.Client;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars
{
    public class PublicGameInfoDetail : VBoxContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(PublicGameInfoDetail));

        /// <summary>
        /// event triggered when game is deleted
        /// </summary>
        public event Action<PublicGameInfoDetail> OnDeleted;

        /// <summary>
        /// The PlayerSaves for this view. A single game could have multiple player saves.
        /// </summary>
        /// <value></value>
        public List<PlayerSave> PlayerSaves { get; set; }

        Label detailNameLabel;
        Label modeValueLabel;
        Label sizeValueLabel;
        Label densityValueLabel;
        Label playersValueLabel;

        GridContainer playersGridContainer;
        Continuer continuer;
        TextureRect textureRect;
        CSButton deleteButton;

        public override void _Ready()
        {
            detailNameLabel = GetNode<Label>("DetailNameLabel");
            sizeValueLabel = GetNode<Label>("InfoGridContainer/SizeValueLabel");
            modeValueLabel = GetNode<Label>("InfoGridContainer/ModeValueLabel");
            densityValueLabel = GetNode<Label>("InfoGridContainer/DensityValueLabel");
            playersValueLabel = GetNode<Label>("InfoGridContainer/PlayersValueLabel");
            playersGridContainer = GetNode<GridContainer>("ScrollContainer/VBoxContainer/PlayersGridContainer");
            textureRect = GetNode<TextureRect>("ScrollContainer/VBoxContainer/ScreenshotHBoxContainer/TextureRect");
            deleteButton = GetNode<CSButton>("ScrollContainer/VBoxContainer/ButtonsHBoxContainer/DeleteButton");

            // we'll use this later if a game is loaded
            continuer = new Continuer();
            AddChild(continuer);

            deleteButton.OnPressed(OnDeleteButtonPressed);
        }

        public void UpdateControls()
        {
            if (PlayerSaves != null && PlayerSaves.Count > 0)
            {
                var gameInfo = PlayerSaves[0].GameInfo;
                detailNameLabel.Text = $"{gameInfo.Name}: Year {gameInfo.Year}";
                modeValueLabel.Text = EnumUtils.GetLabelForGameMode(gameInfo.Mode);
                sizeValueLabel.Text = EnumUtils.GetLabelForSize(gameInfo.Size);
                densityValueLabel.Text = gameInfo.Density.ToString();
                playersValueLabel.Text = $"{gameInfo.Players.Count}";

                var screenshot = GamesManager.Instance.GetPlayerScreenshot(gameInfo.Name, gameInfo.Year, PlayerSaves[0].PlayerNum);
                textureRect.Texture = screenshot;

                playersGridContainer.ClearChildren();

                foreach (var playerSave in PlayerSaves)
                {
                    playersGridContainer.AddChild(new Label()
                    {
                        Text = playerSave.GameInfo.Players[playerSave.PlayerNum].Name,
                        Align = Label.AlignEnum.Right,
                        SizeFlagsHorizontal = (int)SizeFlags.ExpandFill
                    });
                    HBoxContainer buttonContainer = new HBoxContainer() { SizeFlagsHorizontal = (int)SizeFlags.ExpandFill, Alignment = AlignMode.End };
                    var loadButton = new CSButton()
                    {
                        Text = "Load",
                        RectMinSize = new Vector2(80, 0)
                    };
                    loadButton.OnPressed((button) => OnLoadButtonPressed(gameInfo, playerSave.PlayerNum));
                    buttonContainer.AddChild(loadButton);

                    playersGridContainer.AddChild(buttonContainer);
                }
            }
        }

        void OnLoadButtonPressed(PublicGameInfo gameInfo, int playerNum)
        {
            try
            {
                continuer.Continue(gameInfo.Name, gameInfo.Year, playerNum);
                Settings.Instance.ContinueGame = gameInfo.Name;
                Settings.Instance.ContinueYear = gameInfo.Year;
                Settings.Instance.ContinuePlayerNum = playerNum;
            }
            catch (Exception e)
            {
                log.Error($"Failed to continue game {gameInfo.Name}: {gameInfo.Year}", e);
                CSConfirmDialog.Show($"Failed to load game {gameInfo.Name}: {gameInfo.Year}");
            }
        }

        void OnDeleteButtonPressed(CSButton button)
        {
            if (PlayerSaves != null && PlayerSaves.Count > 0)
            {
                var gameName = PlayerSaves[0].GameInfo.Name;
                GamesManager.Instance.DeleteGame(gameName);
                CSConfirmDialog.Show($"Are you sure you want to delete the game {gameName}?",
                () =>
                {
                    GamesManager.Instance.DeleteGame(gameName);
                    OnDeleted?.Invoke(this);
                });
            }
        }

    }
}