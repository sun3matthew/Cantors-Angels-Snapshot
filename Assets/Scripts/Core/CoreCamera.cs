using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreCamera : MonoBehaviour
{
    private class KinematicObject
    {
        private float Acceleration;
        private float Friction;
        private float MaxSpeed;
        public float[] Velocity { get; private set; }
        public float[] Position { get; private set; }
        public KinematicObject(float acceleration, float friction, float maxSpeed, Vector2 position)
        {
            Acceleration = acceleration;
            Friction = friction;
            MaxSpeed = maxSpeed;
            Velocity = new float[2];
            Position = new float[2] { position.x, position.y };
        }
        public void SetPosition(Vector2 position) => Position = new float[2] { position.x, position.y };
        public void ChangeMaxSpeed(float maxSpeed) => MaxSpeed = maxSpeed;
        public void Update()
        {
            float[] oldVelocity = new float[2] { Velocity[0], Velocity[1] };

            Vector2 DeltaXYFromCenter = new((Input.mousePosition.x - Screen.width / 2) / (Screen.width / 2), (Input.mousePosition.y - Screen.height / 2) / (Screen.height / 2));
            float threshold = 0.9f;
            if (Mathf.Abs(DeltaXYFromCenter.x) > threshold && Mathf.Abs(DeltaXYFromCenter.x) <= 1)
                IncreaseVelocity(0, Acceleration * DeltaXYFromCenter.x);
            if (Mathf.Abs(DeltaXYFromCenter.y) > threshold && Mathf.Abs(DeltaXYFromCenter.y) <= 1)
                IncreaseVelocity(1, Acceleration * DeltaXYFromCenter.y);

            if (Input.GetKey(KeyCode.W))
                IncreaseVelocity(1, Acceleration);
            if (Input.GetKey(KeyCode.S))
                IncreaseVelocity(1, -Acceleration);
            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
                Velocity[1] = Mathf.Abs(Velocity[1]) <= Friction * Time.deltaTime ? 0 : Velocity[1] - Friction * Mathf.Sign(Velocity[1]) * Time.deltaTime;

            if (Input.GetKey(KeyCode.A))
                IncreaseVelocity(0, -Acceleration);
            if (Input.GetKey(KeyCode.D))
                IncreaseVelocity(0, Acceleration);
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                Velocity[0] = Mathf.Abs(Velocity[0]) <= Friction * Time.deltaTime ? 0 : Velocity[0] - Friction * Mathf.Sign(Velocity[0]) * Time.deltaTime;

            for (int i = 0; i < 2; i++)
                Position[i] += Velocity[i] * Time.deltaTime;
        }
        public void ClampPosition(Vector2 min, Vector2 max)
        {
            for (int i = 0; i < 2; i++){
                if (Position[i] < min[i]){
                    Position[i] = min[i];
                    Velocity[i] = 0;
                }
                if (Position[i] > max[i]){
                    Position[i] = max[i];
                    Velocity[i] = 0;
                }
            }
        }
        private void IncreaseVelocity(int idx, float acceleration)
        {
            float delta = acceleration * Time.deltaTime;
            if (Mathf.Abs(Velocity[idx] + delta) < MaxSpeed)
                Velocity[idx] += delta;
            else
                Velocity[idx] = Mathf.Sign(delta) * MaxSpeed;
        }
    }
    public const float maxZoom = 14f;
    public const float minZoom = 2f;
    public const float zoomSpeed = 25;


    public const float maxSpeed = 25;
    // public const float accSpeed = 120;
    public const float accSpeed = 220;
    public static Camera Camera { get; private set; }
    public static CoreCamera Instance { get; private set; }

    private KinematicObject KinematicObjectRef;

    [SerializeField]
    private Vector2Int GoTo;
    public static void Initialize(){
        Camera = Camera.main;
        if (Camera.gameObject.GetComponent<CoreCamera>() != null)
            return;

        CoreCamera coreCamera = Camera.gameObject.AddComponent<CoreCamera>();
        Camera.orthographicSize = (maxZoom + minZoom) / 2;
        
        int boardSize = Board.Instance.BoardSize;
        Vector2 position = (Vector2)(HexVector)new GridVector(boardSize / 2, boardSize / 2);
        Camera.transform.position = new Vector3(position.x, position.y, -10);
        coreCamera.KinematicObjectRef = new KinematicObject(accSpeed, accSpeed * 1.5f, maxSpeed, position);

        Instance = coreCamera;
    }
    public static void Deserialize(byte[] bytes, ref int index){
        float x = SaveFile.ReadFloat(bytes, ref index);
        float y = SaveFile.ReadFloat(bytes, ref index);
        float size = SaveFile.ReadFloat(bytes, ref index);
        if (x == -1)
            return;
        Initialize();
        Camera.transform.position = new Vector3(x, y, Camera.transform.position.z);
        Camera.orthographicSize = size;

        CoreCamera coreCamera = Camera.gameObject.GetComponent<CoreCamera>();
        coreCamera.KinematicObjectRef.SetPosition(Camera.transform.position);

        Instance = coreCamera;
    }

    public void CameraUpdate()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            KinematicObjectRef.ChangeMaxSpeed(maxSpeed * 2);
        if(Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            KinematicObjectRef.ChangeMaxSpeed(maxSpeed);
            

        if(Input.mouseScrollDelta.y > 0){
            Camera.orthographicSize -= zoomSpeed * Time.deltaTime;
            if(Camera.orthographicSize < minZoom)
                Camera.orthographicSize = minZoom;
        }
            
        if(Input.mouseScrollDelta.y < 0){
            Camera.orthographicSize += zoomSpeed * Time.deltaTime;
            if(Camera.orthographicSize > maxZoom)
                Camera.orthographicSize = maxZoom;
        }

        KinematicObjectRef.Update();        

        if (KinematicObjectRef.Velocity[0] != 0 || KinematicObjectRef.Velocity[1] != 0)
            UpdateCameraPosition();

        Vector2 min = new();
        Vector2 max = new(BoardRender.Instance.BoardBounds[1], BoardRender.Instance.BoardBounds[3]);
        Vector3 cameraBottomLeft = Camera.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 cameraTopRight = Camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        float cameraWidth = cameraTopRight.x - cameraBottomLeft.x;
        float cameraHeight = cameraTopRight.y - cameraBottomLeft.y;
        min.x += cameraWidth / 2;
        min.y += cameraHeight / 2;
        max.x -= cameraWidth / 2;
        max.y -= cameraHeight / 2;
        min.y += UniversalRenderer.max * 0.5f;
        max.y -= UniversalRenderer.max * 0.5f;
        KinematicObjectRef.ClampPosition(min, max);

        ParticleSystemComputeCollection.Update(Time.deltaTime);
        ParticleSystemCollection.Update(Time.deltaTime);

    }
    public static GridVector MouseGridPos() => (GridVector)(HexVector)(Vector2)Camera.transform.position;



    public static void LookAt(Entity entity){
        Vector2 position = (Vector2)entity.Position;
        Camera.transform.position = new Vector3(position.x, position.y, Camera.transform.position.z);
    }

    private void UpdateCameraPosition() => Camera.transform.position = new Vector3(KinematicObjectRef.Position[0], KinematicObjectRef.Position[1], 0);
    public Vector2 GetCameraPosition() => new(KinematicObjectRef.Position[0], KinematicObjectRef.Position[1]);
    public static void Serialize(List<byte> bytes){
        if (Camera == null){
            SaveFile.WriteFloat(bytes, -1);
            SaveFile.WriteFloat(bytes, -1);
            SaveFile.WriteFloat(bytes, -1);
        }else{
            SaveFile.WriteFloat(bytes, Camera.transform.position.x);
            SaveFile.WriteFloat(bytes, Camera.transform.position.y);
            SaveFile.WriteFloat(bytes, Camera.orthographicSize);
        }
    }


    [ContextMenu("GoTo")]
    public void GoToPosition(){
        KinematicObjectRef.SetPosition((Vector2)new HexVector(GoTo.x, GoTo.y));   
        UpdateCameraPosition();
    }
}
