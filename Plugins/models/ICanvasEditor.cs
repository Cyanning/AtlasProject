using UnityEngine;

namespace Plugins.models
{
    public interface ICanvasEditor
    {
        int ModelGender { get; }
        string[] ModelDisplayed { get; }
        void ClickRespond(Transform clickedModel);
    }
}
