using Godot;
using log4net;
using System.Threading.Tasks;

namespace CraigStars
{
    public class Loader : Control
    {
        static CSLog log = LogProvider.GetLogger(typeof(Loader));
        ResourceInteractiveLoader loader;
        TextureProgress progress;

        public override void _Ready()
        {
            Visible = false;
            progress = GetNode<TextureProgress>("TextureProgress");
            SetProcess(false);
        }

        public void LoadScene(string filePath)
        {
            Visible = true;
            loader = ResourceLoader.LoadInteractive(filePath);
            SetProcess(true);
        }

        public override void _Process(float delta)
        {
            if (loader == null)
            {
                SetProcess(false);
                return;
            }

            Error err = loader.Poll();

            if (err == Error.FileEof)
            {
                Resource resource = loader.GetResource();
                loader = null;
                // fire this off, don't wait
                SetNewScene((PackedScene)resource);
                return;
            }
            else if (err == Error.Ok)
            {
                UpdateProgress();
                return;
            }
        }

        public void UpdateProgress()
        {
            var loadProgress = ((float)loader.GetStage()) / loader.GetStageCount();

            progress.Value = loadProgress * 100;
        }

        public void SetNewScene(PackedScene resource)
        {
            // await SceneChanger.Instance.Fade();

            GetTree().ChangeSceneTo(resource);

            // await SceneChanger.Instance.ReverseFade();
        }

    }
}