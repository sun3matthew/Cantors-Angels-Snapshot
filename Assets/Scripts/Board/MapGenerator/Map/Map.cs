using Delaunay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Map
{
    public class Map
    {
        private int _pointCount = 2000;
        // private int _pointCount = 1000;
        // float _lakeThreshold = 0.03f;
        float _lakeThreshold = 0.75f;
        public const float Width = 50;
        public const float Height = 50;
        
        public Graph Graph { get; private set; }
        public Map(int seed)
        {
            List<uint> colors = new();
            var points = new List<Vector2>();

            CoreRandom coreRandom = new(seed);
            for (int i = 0; i < _pointCount; i++)
            {
                colors.Add(0);
                points.Add(new Vector2(
                        coreRandom.Next(Width),
                        coreRandom.Next(Height))
                );
            }


            var voronoi = new Voronoi(points, colors, new Rect(0, 0, Width, Height), coreRandom);
            
            Graph = new Graph(points, voronoi, (int)Width, (int)Height, _lakeThreshold, coreRandom);
        }
    }
}
