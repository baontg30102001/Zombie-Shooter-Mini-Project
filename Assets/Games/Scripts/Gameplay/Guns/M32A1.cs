using UnityEngine;
using Zenject;

public class M32A1 : Gun
{
    [SerializeField] private LineRenderer _lineRenderer;
    
    public LineRenderer LineRenderer => _lineRenderer;
    
    public class Factory : PlaceholderFactory<M32A1>
    {
        
    }
}
