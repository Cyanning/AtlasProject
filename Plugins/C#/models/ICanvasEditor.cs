using UnityEngine;

namespace Plugins.C_.models
{
    public interface ICanvasEditor
    {
        int ModelGender { get; }
        string[] ModelDisplayed { get; }
        void ClickRespond(Transform clickedModel);
    }
}
