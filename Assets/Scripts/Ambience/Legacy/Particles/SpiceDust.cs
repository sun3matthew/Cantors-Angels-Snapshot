using System.Collections.Generic;
using UnityEngine;

public class SpiceDust : Particle
{
    public static Sprite sprite;
    private const float DustLife = 8f;
    public static List<Tile>[] SpiceFields;
    public static Board board;
    public SpiceDust(float x, float y) : base(x, y, DustLife){
        Velocity = new Vector2(CoreRandom.GlobalRange(-0.4f, 0.4f), CoreRandom.GlobalRange(0f, 0.1f));
    }
    public static List<Particle> Emit(float counter){
        if (board != Board.Instance){ // Check if different board
            board = Board.Instance;
            SpiceFields = new List<Tile>[4];
            for (int i = 0; i < 4; i++)
                SpiceFields[i] = new List<Tile>();
            Tile[,] tiles = board.Current.TileBoard;
            for (int i = 0; i < tiles.GetLength(0); i++)
                for (int j = 0; j < tiles.GetLength(1); j++)
                    if (tiles[i, j].SpiceLevel() > 0)
                        SpiceFields[tiles[i, j].SpiceLevel() - 1].Add(tiles[i, j]);
        }
        if (counter > 0.2f){
            List<Particle> particles = new();
            for (int i = 0; i < SpiceFields.Length; i++)
                    for (int j = 0; j < SpiceFields[i].Count; j++)
                        for (int k = 0; k <= i; k++)
                            if (CoreRandom.GlobalRange(0, 1.0f) < 0.01f){
                                Vector2 position = UniversalRenderer.CalculatePosition(SpiceFields[i][j], 0);
                                particles.Add(new SpiceDust(position.x + CoreRandom.GlobalRange(-0.7f, 0.7f), position.y + CoreRandom.GlobalRange(-0.4f, 0.4f)));
                            }
            return particles;
        }
        return null;
    }
    public override void Update(float dt)
    {
        base.Update(dt);
        // if (Life < 4){ //! idk if you should do this, artistic vision vs realism/worldBuilding
        //     Velocity = new Vector2(0, 0);
        //     Acceleration = new Vector2(0, 0);
        // }
        // else
            Acceleration = new Vector2(CoreRandom.GlobalRange(-0.8f, 0.8f), CoreRandom.GlobalRange(0f, 0.2f));
    }

    static SpiceDust()
    {
        sprite = Resources.Load<Sprite>("Sprites/Particles/Spice");
    }
    public override Sprite GetSprite() => sprite;
    // public override Color GetColor() => new (0.87f, 0.42f, 0.24f, (0.5f - Mathf.Abs((Life / DustLife) - 0.5f)) * 1.6f);
    public override Color GetColor() => new (0.97f, 0.32f, 0.24f, (0.5f - Mathf.Abs((Life / DustLife) - 0.5f)) * 1.6f);
    public override int GetSortingOrder() => 2;
}