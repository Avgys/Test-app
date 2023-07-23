using System.Collections;
using HelpScripts;
using SimUnity.Management;
using UnityEngine;
using UnityEngine.EventSystems;

public class Interactable : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    
    [SerializeField]
    // public AvailableAction[] Actions;

    public delegate void MovedHandler(Interactable invoker, Vector3 pos);

    public MovedHandler MovedEvent;
    public GameManager GameManager { get; set; }

    void Start()
    {
        // _floorLayer = LayerMask.NameToLayer("Floor");
    }

    private Coroutine _followMouseCoroutine;
    private int _floorLayer;

    public void FollowTheMouse()
    {
        _followMouseCoroutine = StartCoroutine(FollowTheMouseIterate());
    }

    IEnumerator FollowTheMouseIterate()
    {
        var y = GetComponent<MeshRenderer>().bounds.size.y / 2;
        while(true)
        {
            // if (MousePosition.TryGet(Camera.main, _floorLayer, out var pos))
            // {
            //     MovedEvent?.Invoke(this, pos);
            //     var newPosition = pos;
            //     newPosition.y += y;
            //     transform.position = newPosition;
            // }

            yield return new WaitForSeconds(0.01f);
        }
    }

    public void StopFollowTheMouse()
    {
        StopCoroutine(_followMouseCoroutine); 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // if (_schemeEditor.Mode != EditMode.Move && Cursor.lockState != CursorLockMode.Locked)
        //     RadialMenuSpawner.Instance.SpawnMenu(this);
    }
}
