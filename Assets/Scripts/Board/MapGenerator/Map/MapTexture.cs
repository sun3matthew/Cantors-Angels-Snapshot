using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Map
{
    public class MapTexture
    {
        int _textureScale;
        public MapTexture(int textureScale)
        {
            _textureScale = textureScale;
        }

        public void AttachTexture(GameObject plane, Map map)
        {

            int _textureWidth = (int)Map.Width * _textureScale;
            int _textureHeight = (int)Map.Height * _textureScale;

            Texture2D texture = new(_textureWidth, _textureHeight);
            texture.SetPixels(Enumerable.Repeat(Color.magenta, _textureWidth * _textureHeight).ToArray());

            // // var lines = map.Graph.edges.Where(p => p.v0 != null).Select(p => new[] 
            // // { 
            // //     p.v0.point.x, p.v0.point.y,
            // //     p.v1.point.x, p.v1.point.y
            // // }).ToArray();

            foreach (var c in map.Graph.centers){
                Vector2 center = new(c.point.x * _textureScale, c.point.y * _textureScale);
                // Debug.Log(c.biome);
                texture.FillPolygon(c.corners.Select(p => new Vector2(p.point.x * _textureScale, p.point.y * _textureScale)).ToArray(), BiomeProperties.Colors[c.biome]);
            }

            // // for all pixels, if the pixel is magenta, set it to the last color
            Color lastColor = BiomeProperties.Colors[Biome.Ocean];
            for(int x = 0; x < _textureWidth; x++){
                for(int y = 0; y < _textureHeight; y++){
                    Color pixel = texture.GetPixel(x, y);
                    if(pixel == Color.magenta){
                        texture.SetPixel(x, y, lastColor);
                    }else{
                        lastColor = pixel;
                    }
                }
            }



            // for each center, redraw the polygon with a opacity of 0.5, and slightly rotate it randomly
            // for(int i = 0; i < 1; i++){
            //     List<Center> centers = map.Graph.centers;
            //     while(centers.Count > 0){
            //         int index = UnityEngine.Random.Range(0, centers.Count);
            //         Center c = centers[index];
            //         Vector2 center = new Vector2(c.point.x * _textureScale, c.point.y * _textureScale);
            //         Vector2[] corners = c.corners.Select(p => new Vector2(p.point.x * _textureScale, p.point.y * _textureScale)).ToArray();
            //         Vector2[] rotatedCorners = new Vector2[corners.Length];
            //         for(int j = 0; j < corners.Length; j++){
            //             rotatedCorners[j] = RotateAround(center, corners[j], UnityEngine.Random.Range(0, 360));
            //         }
            //         Color color = BiomeProperties.Colors[c.biome];
            //         texture.FillPolygon(rotatedCorners, color);
            //         centers.RemoveAt(index);
            //     }
            // }


            // foreach (var line in lines)
            //     DrawLine(texture, line[0], line[1], line[2], line[3], Color.black);

            // foreach (var line in map.Graph.edges.Where(p => p.river > 0 && !p.d0.water && !p.d1.water))
            //     DrawLine(texture, line.v0.point.x, line.v0.point.y, line.v1.point.x, line.v1.point.y, Color.blue);

            texture.Apply();

            plane.GetComponent<UnityEngine.UI.RawImage>().texture = texture;
        }

        private void DrawLine(Texture2D texture, float x0, float y0, float x1, float y1, Color color)
        {
            texture.DrawLine((int)(x0 * _textureScale), (int)(y0 * _textureScale), (int)(x1 * _textureScale), (int)(y1 * _textureScale), color);
        }

        private Vector2 RotateAround(Vector2 center, Vector2 point, float angle){
            angle = 0;
            float radians = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            float nx = (cos * (point.x - center.x)) + (sin * (point.y - center.y)) + center.x;
            float ny = (cos * (point.y - center.y)) - (sin * (point.x - center.x)) + center.y;
            return new Vector2(nx, ny);
        }

    }
}
