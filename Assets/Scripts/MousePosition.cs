using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HelpScripts
{
    public static class MousePosition
    {
        public static LayerMask IuLayer = LayerMask.GetMask("UI");

        public static bool TryGet(Camera cam, LayerMask layerMask, out Vector3 position, out IEnumerable<GameObject> objects)
        {
            if (EventSystem.current == null)
            {
                position = Vector3.zero;
                objects = null;
                return false;
            }

            var pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;

            var lookingLayer = layerMask;
            
            List<RaycastResult> hits = new();
            EventSystem.current.RaycastAll(pointerEventData, hits);

            if (!hits.Any(x => x.gameObject.layer == IuLayer)
                && hits.Any(x => x.gameObject.layer == lookingLayer))
            {
                position = hits.First(x => x.gameObject.layer == lookingLayer).worldPosition;
                objects = hits.Select(x => x.gameObject);
                return true;
            }

            position = Vector3.zero;
            objects = null;
            return false;
        }

        //public static int LayerToInt(LayerMask layer) => Mathf.RoundToInt(Mathf.Log(layer) / Mathf.Log(2));
    }
}