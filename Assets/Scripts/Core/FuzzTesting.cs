// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// // Do a f*ck ton of random moves to try to find bugs
// public class FuzzTesting : MonoBehaviour
// {
//     private CoreRandom Random;
//     private bool Terminated;
//     private int Iterations;

//     [SerializeField]
//     private Text IterationText;

//     public void Start(){
//         // Random = new CoreRandom(314159265);
//         Random = new CoreRandom(100430);

//         DebugToFile.Initialize();
//         Application.targetFrameRate = -1;
//         Entity.Initialize();
//         BoardHistory.Initialize();

//         ResetBoard();
//     }
//     public void Update(){
//         if (Input.GetKeyDown(KeyCode.Space)){
//             ResetBoard();
//         }
//         if (Input.GetKeyDown(KeyCode.F))
//             Debug.Log(Board.Instance);

//         if (Terminated){
//             string currText = IterationText.text;
//             if (currText.Contains("Terminated"))
//                 return;
//             IterationText.text = currText + "\n<color=red>Terminated</color>";
//             return;
//         }

//         bool success = true;
//         Iterations++;
//         List<(float, DeltaEntity, MonoDelta)> possibleDeltas = new();
//         for(int failSafe = 0; failSafe < 100; failSafe++){
//             if(success){
//                 success = false;
//                 List<DeltaEntity> deltaEntities = Board.Instance.Current.GetEntities<DeltaEntity>();
//                 List<IManualDeltaEntity> manualDeltaEntities = new();
//                 foreach (DeltaEntity deltaEntity in deltaEntities)
//                     if (deltaEntity is IManualDeltaEntity manualDeltaEntity)
//                         manualDeltaEntities.Add(manualDeltaEntity);

//                 possibleDeltas = new();
//                 foreach (IManualDeltaEntity manualDeltaEntity in manualDeltaEntities){
//                     List<(float, MonoDelta)> deltas = manualDeltaEntity.AllPossibleDeltas();
//                     foreach ((float, MonoDelta) delta in deltas)
//                         possibleDeltas.Add((delta.Item1, manualDeltaEntity as DeltaEntity, delta.Item2));
//                 }

//                 if (possibleDeltas.Count == 0){
//                     if(Board.Instance.Current.GetEntities<Church>().Count == 0){
//                         Debug.Log("Terminated");
//                         Terminated = true;
//                         return;
//                     }
//                     break;
//                 }            
//             }
//             int idx = Random.Next(possibleDeltas.Count);
//             float weight = possibleDeltas[idx].Item1;
//             weight = Mathf.Pow(weight, 4);
//             if (Random.NextFloat() < weight){
//                 // try{
//                     MonoDelta delta = possibleDeltas[idx].Item3;
//                     DeltaEntity SelectedDeltaEntity = possibleDeltas[idx].Item2;
//                     Board.Instance.Current.InjectMonoDelta(delta, SelectedDeltaEntity);
//                     (SelectedDeltaEntity as IManualDeltaEntity).ResolveManualDelta();
//                     Board.Instance.RegenerateDeltas(Board.Instance.WorkingBoard + 1);
//                     success = true;
//                 // } catch (System.Exception e){
//                 //     Terminated = true;
//                 //     Debug.LogError(e);
//                 //     return;
//                 // }
//             }
//         }

//         Board.Instance.CommitTurn(false);

//         BoardState boardState = Board.Instance.Current;
//         string text = "Iterations: " + Iterations;
//         long hash = boardState.BruteHash();
//         text += "\n" + SaveUtility.ToHexString(hash);
//         Economy economy = boardState.GetEntity<Economy>(UniversalDeltaEntity.Economy);
//         text += "\nSpice: " + economy.GetResource(ResourceType.Spice) + " | Faith: " + economy.GetResource(ResourceType.Faith);
//         int HadicCount = boardState.GetEntities<Hadic>().Count;
//         text += "\nHadic: " + HadicCount;
//         int UnitCount = boardState.GetEntities<Mech>().Count;
//         text += "\nUnit: " + UnitCount;
//         int ChurchCount = boardState.GetEntities<Church>().Count;
//         text += "\nChurch: " + ChurchCount;
//         int VillageCount = boardState.GetEntities<Village>().Count;
//         text += "\nVillage: " + VillageCount;
//         IterationText.text = text;


//         DebugPerformance.AddLayer("SaveTest");
//         byte[] saveData = SaveFile.GenerateSaveFile();
//         SaveFile.LoadSaveFile(saveData);
//         DebugPerformance.EndLayer();
//         DebugPerformance.ClearBuffer();
//     }
//     private void ResetBoard(){
//         Iterations = 0;
//         Terminated = false;

//         UserSpawner.CachedDeltaEntity = null;

//         DebugPerformance.AddLayer("Init");
//         BoardCreator.CreateBoardFromSeed(Random.Next(int.MaxValue));
//         Board.Instance.RegenerateDeltas(2);
//         PathFinding.Initialize(Board.Instance.Current);
//         DebugPerformance.EndLayer();
//         DebugPerformance.PrintAndClearBuffer();
//     }
// }
