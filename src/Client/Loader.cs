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
        Tween tween;

        public override void _Ready()
        {
            Visible = false;
            progress = GetNode<TextureProgress>("TextureProgress");
            tween = new Tween();

        }

        public void LoadScene(string filePath)
        {
            Visible = true;
            loader = ResourceLoader.LoadInteractive(filePath);
        }

        public override void _Process(float delta)
        {
            if (loader == null)
            {
                return;
            }

            Error err = loader.Poll();

            if (err == Error.FileEof)
            {
                Resource resource = loader.GetResource();
                loader = null;
                // fire this off, don't wait
                var _ = SetNewScene((PackedScene)resource);
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

        async public Task SetNewScene(PackedScene resource)
        {
            // await SceneChanger.Instance.Fade();

            GetTree().ChangeSceneTo(resource);

            // await SceneChanger.Instance.ReverseFade();
        }

    }
}