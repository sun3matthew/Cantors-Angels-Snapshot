using System.Collections.Generic;
using UnityEngine;


public class CoreLoop : MonoBehaviour
{
    public static CoreLoop Instance { get; private set; }    

    private static Board Board; // get only //! Make sure this is updated if you ever reload the board
    private static BoardRender BoardRender; // get only

    private UserDeltaEntity SelectedDeltaEntity;
    private MonoDelta CurrentMonoDelta;

    private UICollection UICollection;
    private CommitNetwork CommitNetwork => UICollection.CommitGraphManager.CommitNetwork;
    private CommitGraph CommitGraph => UICollection.CommitGraphManager.CommitGraph;

    public void Initialize(){
        Instance = this;

        DebugToFile.Initialize();
        if(!DummyTesting.Test())
            return;
        Application.targetFrameRate = -1;

        DebugPerformance.AddLayer("Init");
        Entity.Initialize();
        SpriteManager.Initialize();
        AnimationSync.Initialize();
        gameObject.AddComponent<DoubleClick>();
        DebugPerformance.CreateSegment("Sprite Init");
        BoardHistory.Initialize();
        DebugPerformance.CreateSegment("History Init");
        
        UICollection = new UICollection(GameObject.Find("Canvas").GetComponent<Canvas>());
        DebugPerformance.CreateSegment("UI Init");

        UniversalRenderer.InitializePool();
        DebugPerformance.CreateSegment("Entity Pool Init");

        DebugPerformance.AddLayer("Board Init");
        int loadSave = SaveUtility.CurrentSafeFileIdx(SaveUtility.SaveDir);
        int seed = 4123346;
        if (loadSave == 0){
            Board = BoardCreator.CreateBoardFromSeed(seed);
            Board.RegenerateDeltas(2);
            PathFinding.Initialize(Board.Current);

            CoreCamera.Initialize();
        }else{
            SaveFile.Load(loadSave);
            Board = Board.Instance;
        }
        CommitNetwork.ForceAddBranch(BoardHistory.Root);
        CommitNetwork.ForceUpdateLoadedNode(BoardHistory.Current);
        UICollection.CommitGraphManager.CommitMapArrows.SyncToBoard();

        SaveUtility.CreateDirectory(SaveUtility.NetworkGraphCache(seed));
        DebugPerformance.EndLayer();

        BoardRender = new BoardRender(Board);
        BoardRender.ReRender();
        DebugPerformance.CreateSegment("Board Render");

        ParticleSystemCollection.Initialize();
        ParticleSystemComputeCollection.Initialize();
        DebugPerformance.CreateSegment("Particle System");


        UniversalRenderer.Pool.PreWarm(2000);
        UICollection.Update();
        DebugPerformance.CreateSegment("PreWarm");

        DebugPerformance.EndLayer();
        DebugPerformance.PrintAndClearBuffer();

    }
    void Awake()
    {
        Initialize();
        DebugPerformance.AddLayer("Other");
    }

    void Update()
    {
        UICollection.Update();

        BoardRender.BounceGrid.UpdateBounce();
        BoardRender.UpdateTileChunks();
        BoardRender.RenderTemporalStates();


        int stepDeltaMakerIdx = SelectedDeltaEntity != null ? SelectedDeltaEntity.StepDeltaMaker() : 0;
        if (!UICollection.CommitGraphManager.IsActive){
            CoreCamera.Instance.CameraUpdate();
            if(SelectedDeltaEntity != null){
                if(stepDeltaMakerIdx == 0 && CurrentMonoDelta != null){ 
                    Board.Current.InjectMonoDelta(CurrentMonoDelta, SelectedDeltaEntity);
                    (SelectedDeltaEntity as IManualDeltaEntity).ResolveManualDelta();

                    RegenerateDeltas();
                }
            }

            if(Input.GetMouseButtonUp(0)){
                if (stepDeltaMakerIdx != 2){
                    DeselectEntity();

                    SelectedDeltaEntity = Board.Current.GetEntity<UserDeltaEntity>(MouseHexPos());
                    if(SelectedDeltaEntity != null){
                        CoreAnimator ca = BoardRender.GetUniversalRenderer(SelectedDeltaEntity).GetCoreAnimator();
                        ca.SetOutlineColor(Color.green);
                        ca.SetSpriteOutline(true);

                        CurrentMonoDelta = SelectedDeltaEntity.InitActionTree();
                        stepDeltaMakerIdx = SelectedDeltaEntity.StepDeltaMaker();
                    }

                }

                GridVector MousePos = (GridVector)MouseHexPos();
                Tile SelectedTile = Board.Current.TileBoard[MousePos.x, MousePos.y];
                if(SelectedTile != null)
                    BoardRender.BounceGrid.AddBounceForce(MousePos, 0.024f, 0, 2, 0.03f);
            }
        }

        // Forwards and backwards movements graph node
        if (Input.GetKeyDown(KeyCode.Q)){
            Board.SetWorkingBoard(Board.WorkingBoard - 2);
            // Board.RegenerateHashes();
            CleanUp();
        }else if (Input.GetKeyDown(KeyCode.E)){
            Board.SetWorkingBoard(Board.WorkingBoard + 2);
            // Board.RegenerateHashes();
            CleanUp();
        }

        if(Input.GetMouseButtonDown(1))
            DeselectEntity();

        if(SelectedDeltaEntity != null && Input.GetKeyDown(KeyCode.U)){ //undo
            Board.UndoMonoDeltaAssociatedWith(SelectedDeltaEntity);
            CleanUp();
        }

        if(Input.GetKey(KeyCode.C)){
            UserSpawner userSpawner = Board.Current.GetEntity<UserSpawner>(UniversalDeltaEntity.UserSpawner);
            EntityEnum entityE = EntityEnum.Null;
            if(Input.GetKeyDown(KeyCode.Alpha1))
                entityE = EntityEnum.Village;
            if(Input.GetKeyDown(KeyCode.Alpha2))
                entityE = EntityEnum.Church;
            if(Input.GetKeyDown(KeyCode.Alpha3))
                entityE = EntityEnum.BasicMech;
            if(Input.GetKeyDown(KeyCode.Alpha4))
                entityE = EntityEnum.ArtilleryMech;
            if(Input.GetKeyDown(KeyCode.Alpha5))
                entityE = EntityEnum.Bunker;
            if(Input.GetKeyDown(KeyCode.Alpha6))
                entityE = EntityEnum.ScoutMech;
            
            if(entityE != EntityEnum.Null && userSpawner.CanCreateEntity(entityE, MouseHexPos())){
                userSpawner.CreateEntity(entityE, MouseHexPos());
                (userSpawner as IManualDeltaEntity).ResolveManualDelta();
                RegenerateDeltas();
            }
        }



        // * Debugging
        if(Input.GetKeyDown(KeyCode.Equals))
            QualitySettings.vSyncCount = QualitySettings.vSyncCount == 0 ? 1 : 0;
        if(Input.GetKeyDown(KeyCode.Equals) && Input.GetKey(KeyCode.LeftShift))
            SaveFile.Save();
        if(Input.GetKeyDown(KeyCode.Minus))
            Board.DumpBoard();
    }
    public void CleanUp(){
        BoardRender.ReRender();
        DeselectEntity();
    }
    private void DeselectEntity(){
        if (SelectedDeltaEntity != null){
            SelectedDeltaEntity.CleanUpSelected();
            BoardRender.GetUniversalRenderer(SelectedDeltaEntity)?.GetCoreAnimator().SetSpriteOutline(false);
        }
        SelectedDeltaEntity = null;
    }
    private void RegenerateDeltas(){
        Board.RegenerateDeltas(Board.WorkingBoard + 1);
        CleanUp();
    }
    public static HexVector MouseHexPos(){
        Vector2 OriginalPixelPosition = CoreCamera.Camera.ScreenToWorldPoint(Input.mousePosition);
        for(float currentY = OriginalPixelPosition.y; currentY < OriginalPixelPosition.y + 10; currentY += 0.1f){
            Vector2 PixelPos = new(OriginalPixelPosition.x, currentY);
            HexVector hexPos = (HexVector)PixelPos;
            // get the tiles above and below check the hex tiles above and below after applying the offset and see if the mouse is within the hex
            // do this by the offseting mouse and using the pixel to hex function
            List<HexVector> hexes = HexVector.HexRadius(hexPos, 4);
            List<UniversalRenderer> universalRenderers = new();
            for (int i = 0; i < hexes.Count; i++){
                if (!Board.IsHexInBounds(hexes[i]))
                    continue;
                UniversalRenderer universalRenderer = BoardRender.GetUniversalRenderer(hexes[i], 0, Board.WorkingBoard);
                if (universalRenderer != null)
                    universalRenderers.Add(universalRenderer);
            }

            // sort universal renders by ZOffset
            universalRenderers.Sort((a, b) => b.GetZOffset().CompareTo(a.GetZOffset()));
            for (int i = 0; i < universalRenderers.Count; i++){
                HexVector testHex = (HexVector)(PixelPos - new Vector2(0, universalRenderers[i].GetZOffset()));
                HexVector currentHex = universalRenderers[i].DataObject.Position;
                if (testHex == currentHex)
                    return currentHex;
            }
        }
        return new HexVector(int.MaxValue, int.MaxValue);
    }
    public void OnDestroy(){
        ParticleSystemComputeCollection.Release();
        UICollection.Destroy();
    }
}