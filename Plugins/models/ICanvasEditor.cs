using UnityEngine;

namespace Plugins.models
{
    public interface ICanvasEditor
    {
        int ModelGender { get; }
        string[] ModelDisplayed { get; }
        string[] ForamensDisplayed { get; }
        void ClickRespond(Transform clickedModel);
    }
}
