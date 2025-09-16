using UnityEngine;

namespace Plugins.C_
{
    public class ModelTranslucent : MonoBehaviour
    {
        public bool isTranslucnet;

        public void SetTranslucentState(bool state)
        {
            isTranslucnet = state;
            if (transform.childCount == 0) return;
            for (var i = 0; i < transform.childCount; i++)
            {
                var obj = transform.GetChild(i);
                obj.GetComponent<ModelTranslucent>().SetTranslucentState(state);
            }
        }
    }
}
