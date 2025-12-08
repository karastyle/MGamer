
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailMesh : MonoBehaviour
{
    //The number of vertices to create per frame
    private const int NUM_VERTICES = 12;
     
    [SerializeField]
    [Tooltip("The empty game object located at the tip of the blade")]
    private GameObject _tip = null;

    [SerializeField]
    [Tooltip("The empty game object located at the base of the blade")]
    private GameObject _base = null;

    [SerializeField]
    [Tooltip("The mesh object with the mesh filter and mesh renderer")]
    private GameObject _meshParent = null;

    [SerializeField]
    [Tooltip("The number of frame that the trail should be rendered for")]
    private int _trailFrameLength = 3;

    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;
    private Vector3 _previousTipPosition;
    private Vector3 _previousBasePosition;

    private bool _showTrail = false;
    private Vector2[] _uvs;

    private int _frameCount;   
    
    void Start()
    {
        //Init mesh and triangles
        _meshParent.transform.position = Vector3.zero;
        _mesh = new Mesh();
        _meshParent.GetComponent<MeshFilter>().mesh = _mesh;
        
        int vertCount = _trailFrameLength * NUM_VERTICES;
        _vertices = new Vector3[vertCount];
        _triangles = new int[vertCount];
        _uvs = new Vector2[vertCount];

        //Set starting position for tip and base
        _previousTipPosition = _tip.transform.position;
        _previousBasePosition = _base.transform.position;
    }
    
    //是否展示拖尾
    public void ShowTrail(bool show)
    {
        _showTrail = show;
        _mesh.Clear();
        _frameCount = 0;
        
        if (show)
        {
            // 设置起始点（从当前位置开始记录）
            _previousBasePosition = _base.transform.position;
            _previousTipPosition = _tip.transform.position;
        }
    }
    
    void LateUpdate()
    {
        if (!_showTrail)
        {
            return;
        }
        
        //让mesh parent的世界坐标和旋转保持不变， 因为tip base 拿的都是世界坐标。
        _meshParent.transform.position = Vector3.zero;
        this._meshParent.transform.rotation = Quaternion.identity;
        
        //Reset the frame count one we reach the frame length
        if(_frameCount == (_trailFrameLength * NUM_VERTICES))
        {
            //从最前面写入， 每帧生成的矩形放在数组哪里都可以，只要坐标和三角形索引对应上就行
            _frameCount = 0;
        }
        
        int totalVerts = _trailFrameLength * NUM_VERTICES;
        int totalSegments = totalVerts / NUM_VERTICES;
        
        //Draw first triangle vertices for back and front
        _vertices[_frameCount] = _base.transform.position;
        _vertices[_frameCount + 1] = _tip.transform.position;
        _vertices[_frameCount + 2] = _previousTipPosition;
        _vertices[_frameCount + 3] = _base.transform.position;
        _vertices[_frameCount + 4] = _previousTipPosition;
        _vertices[_frameCount + 5] = _tip.transform.position;

        //Draw fill in triangle vertices
        _vertices[_frameCount + 6] = _previousTipPosition;
        _vertices[_frameCount + 7] = _base.transform.position;
        _vertices[_frameCount + 8] = _previousBasePosition;
        _vertices[_frameCount + 9] = _previousTipPosition;
        _vertices[_frameCount + 10] = _previousBasePosition;
        _vertices[_frameCount + 11] = _base.transform.position;

        //Set triangles
        // _triangles[_frameCount] = _frameCount;
        // _triangles[_frameCount + 1] = _frameCount + 1;
        // _triangles[_frameCount + 2] = _frameCount + 2;
        // _triangles[_frameCount + 3] = _frameCount + 3;
        // _triangles[_frameCount + 4] = _frameCount + 4;
        // _triangles[_frameCount + 5] = _frameCount + 5;
        // _triangles[_frameCount + 6] = _frameCount + 6;
        // _triangles[_frameCount + 7] = _frameCount + 7;
        // _triangles[_frameCount + 8] = _frameCount + 8;
        // _triangles[_frameCount + 9] = _frameCount + 9;
        // _triangles[_frameCount + 10] = _frameCount + 10;
        // _triangles[_frameCount + 11] = _frameCount + 11;
        
        
        // ---- 计算 UV ----
        // 最新段的 v = 1，最旧段的 v = 0
        for (int i = 0; i < totalSegments; i++)
        {
            //这里每次都是一样的结果，   最新的段是1  ->  最旧的是0
            float normalized = Mathf.Clamp01(1f - (float)i / (totalSegments - 1)); 
            //由于_frameCount会循环覆盖， 所以这里要计算出当前段在数组中的起始位置， 从起始位置往左边循环，左边的一定是次新， 一共循环totalSegments次就能遍历所有段
            int segBase = ((_frameCount / NUM_VERTICES + totalSegments - i) % totalSegments) * NUM_VERTICES;

            for (int v = 0; v < NUM_VERTICES; v++)
            {
                float u = (v % 2 == 0) ? 0f : 1f;
                _uvs[segBase + v] = new Vector2(u, normalized);
            }
        }
        
        for (int i = 0; i < NUM_VERTICES; i++)
        {
            _triangles[_frameCount + i] = _frameCount + i;
        }

        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uvs;
        _mesh.RecalculateBounds();

        //Track the previous base and tip positions for the next frame
        _previousTipPosition = _tip.transform.position;
        _previousBasePosition = _base.transform.position;
        _frameCount += NUM_VERTICES;
    }


    
}
