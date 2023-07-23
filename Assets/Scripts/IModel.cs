using UnityEngine;

namespace SimUnity.Models
{
    public interface IModel 
    {
        public string Name { get; }
        public int Id { get; }
        public Transform Transform { get; }
        public MeshFilter Mesh { get; }
        public MeshRenderer Renderer { get; }
        
        public Color Color { get; set; }
    }
}