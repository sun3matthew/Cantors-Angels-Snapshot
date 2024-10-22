using System.Collections;
using System.Collections.Generic;
using Assets.Map;
using UnityEngine;
using System.Linq;
public class IslandGenerator
{
    private const float Scale = 0.1f;

    public static int[,] GenerateIslandTiles(int boardSize, int seed){
        IslandShape.PERLIN_CHECK_VALUE = 0.3f;
        Map map = new(seed);
        // between 0 and 100

        float _textureScale = boardSize/50;


        int[,] tile = new int[boardSize, boardSize];
        for(int x = 0; x < boardSize; x++){
            for(int y = 0; y < boardSize; y++){
                tile[x, y] = -1;
            }
        }

        foreach (Center c in map.Graph.centers){
            Vector2 center = new(c.point.x * _textureScale, c.point.y * _textureScale);
            tile[(int)center.x, (int)center.y] = (int)c.biome;
            tile = FillPolygon(tile, c.corners.Select(p => new Vector2(p.point.x * _textureScale, p.point.y * _textureScale)).ToArray(), (int)c.biome);
        }


        int lastBiome = 0;
        for(int x = 0; x < boardSize; x++){
            for(int y = 0; y < boardSize; y++){
                if(tile[x, y] == -1){
                    tile[x, y] = lastBiome;
                }else{
                    lastBiome = tile[x, y];
                }
            }
        }
        return tile;

}

    public static int[,] FillPolygon(int[,] tiles, Vector2[] points, int biome){
        // http://alienryderflex.com/polygon_fill/

        var IMAGE_BOT = (int)points.Max(p => p.y);
        var IMAGE_TOP = (int)points.Min(p => p.y);
        var IMAGE_LEFT = (int)points.Min(p => p.x);
        var IMAGE_RIGHT = (int)points.Max(p => p.x);
        var MAX_POLY_CORNERS = points.Count();
        var polyCorners = MAX_POLY_CORNERS;
        var polyY = points.Select(p => p.y).ToArray();
        var polyX = points.Select(p => p.x).ToArray();
        int[] nodeX = new int[MAX_POLY_CORNERS];
        //int nodes, pixelX, i, j, swap;
        int nodes, i, j, swap;

        //  Loop through the rows of the image.
        for (int pixelY = IMAGE_TOP; pixelY <= IMAGE_BOT; pixelY++)
        {

            //  Build a list of nodes.
            nodes = 0;
            j = polyCorners - 1;
            for (i = 0; i < polyCorners; i++)
            {
                if (polyY[i] < (float)pixelY && polyY[j] >= (float)pixelY || polyY[j] < (float)pixelY && polyY[i] >= (float)pixelY)
                {
                    nodeX[nodes++] = (int)(polyX[i] + (pixelY - polyY[i]) / (polyY[j] - polyY[i]) * (polyX[j] - polyX[i]));
                }
                j = i;
            }

            //  Sort the nodes, via a simple “Bubble” sort.
            i = 0;
            while (i < nodes - 1)
            {
                if (nodeX[i] > nodeX[i + 1])
                {
                    swap = nodeX[i]; nodeX[i] = nodeX[i + 1]; nodeX[i + 1] = swap; if (i != 0) i--;
                }
                else
                {
                    i++;
                }
            }
            //  Fill the pixels between node pairs.
            for (i = 0; i < nodes; i += 2)
            {
                if (nodeX[i] >= IMAGE_RIGHT) 
                    break;
                if (nodeX[i + 1] > IMAGE_LEFT)
                {
                    if (nodeX[i] < IMAGE_LEFT) 
                        nodeX[i] = IMAGE_LEFT;
                    if (nodeX[i + 1] > IMAGE_RIGHT) 
                        nodeX[i + 1] = IMAGE_RIGHT;
                    for (j = nodeX[i]; j < nodeX[i + 1]; j++)
                        if(j >= 0 && j < tiles.GetLength(0) && pixelY >= 0 && pixelY < tiles.GetLength(1))
                            tiles[j, pixelY] = biome;
                }
            }
        }
        return tiles;
    }
}
