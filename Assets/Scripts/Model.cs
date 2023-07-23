using SimUnity.Models;
using UnityEngine;

public class Model : MonoBehaviour, IModel
{
    public int Id { get; set; }
    public string Name => gameObject.name;
    public Transform Transform => transform;

    public MeshFilter Mesh => GetComponent<MeshFilter>();
    public MeshRenderer Renderer => GetComponent<MeshRenderer>();
    public Color Color
    {
        get => Renderer.material.color;
        set
        {
            var newMat = new Material(Renderer.material)
            {
                color = value
            };
            
            Renderer.material = newMat;
        }
    }

    public Vector3 Scale => transform.localScale;
}