using System;
using UnityEngine;


namespace Plugins.C_
{
    public class BodyResponder : MonoBehaviour
    {
        private int _value;
        private readonly int _shaderIDColor = Shader.PropertyToID("_Color");

        public void OnClickCubeItem(UnityEngine.EventSystems.BaseEventData data = null)
        {
            _value = Convert.ToInt32(gameObject.name[^7..]);
            if (_value < 1000000) return;

            SetSelectedState();
        }

        private void SetSelectedState()
        {
            Color colorSelected;
            var meshRenderer = transform.gameObject.GetComponent<MeshRenderer>();

            switch (_value / 100000 - 10)
            {
                case 6:
                    ColorUtility.TryParseHtmlString("#FF0000", out colorSelected);
                    break;
                default:
                    ColorUtility.TryParseHtmlString("#7eb678", out colorSelected);
                    break;
            }

            meshRenderer.material.SetColor(_shaderIDColor, colorSelected);

        }
    }
}
