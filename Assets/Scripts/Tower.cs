using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom
{
    public delegate void NewActiveAgent(Agent agent);

    public class Tower : MonoBehaviour
    {

        public static event NewActiveAgent OnNewActiveAgent;

        static Tower _instance;

        List<RoomData> roomHistory = new List<RoomData>();
        List<Agent> agents = new List<Agent>();
        int activeAgent = 0;
        int activeLevel = -1;

        Room room;
        Player player;
        Projectile activeShot;
        bool switchAgent = false;
        bool queueRoom = false;

        public static Room ActiveRoom
        {
            get
            {
                if (_instance.room == null)
                    _instance.room = FindObjectOfType<Room>();
                return _instance.room;
            }
        }

        public static void Spawn()
        {
            if (_instance == null)
            {
                var tower = FindObjectOfType<Tower>();
                if (tower)
                    tower.enabled = true;
            }
            else
                _instance.enabled = true;

        }

        void Awake() {
            if (_instance == null)
            {
                _instance = this;                
                agents.AddRange(FindObjectsOfType<Agent>());
                for (int i=0;i<agents.Count;i++)
                {
                    if (agents[i] is Player)
                        player = agents[i] as Player;
                }
                
            } else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public static int StairsDownNextToGenerate(int roomWidth, int roomHeight)
        {
            if (_instance.roomHistory.Count == 0)
                return -1;

            
            var refRoom = _instance.roomHistory[_instance.roomHistory.Count - 1];
            var pos = RoomMath.GetCorrespondingPosition(refRoom, TileType.StairsUp, roomWidth, roomHeight, true);
            var coord = Coordinate.FromPosition(pos, roomWidth);
            
            if (RoomMath.CoordinateOnCorner(coord, roomWidth, roomHeight))
            {
                if (coord.x == 0)
                    coord.x = 1;
                else if (coord.y == 0)
                    coord.y = 1;
                else
                    coord.x -= 1;
            }
            return coord.ToPosition(roomWidth, roomHeight);
        }

        void OnEnable() {
            Room.OnRoomGeneration += HandleRoomGenerated;
            Player.OnPlayerEnterNewPosition += HandleNewPlayerPosition;
            Projectile.OnProjectileHit += HandleProjectileHit;
            Projectile.OnProjectileLaunch += HandleProjectileLaunch;
        }

        void OnDisable()
        {
            Room.OnRoomGeneration -= HandleRoomGenerated;
            Player.OnPlayerEnterNewPosition -= HandleNewPlayerPosition;
            Projectile.OnProjectileHit -= HandleProjectileHit;
            Projectile.OnProjectileLaunch -= HandleProjectileLaunch;
        }

        private void HandleProjectileHit(Projectile projectile, Coordinate position)
        {
            activeShot = null;
            if (switchAgent)
                NextAgent();
        }

        private void HandleProjectileLaunch(Projectile projectile)
        {
            activeShot = projectile;
        }

        private void HandleRoomGenerated(Room room, RoomData data)
        {
            this.room = room;
            roomHistory.Add(data);

        }

        private void HandleNewPlayerPosition(Player player, Coordinate position, TileType tileType)
        {
            if (tileType == TileType.StairsUp)
                room.Generate();
        }

        public static void RoomDone()
        {
            _instance.queueRoom = false;
            _instance.NextAgent();
        }

        public static void AgentDone(Agent agent)
        {
            _instance.queueRoom = agent == _instance.player;
            _instance.switchAgent = true;
            if (_instance.activeShot == null)
                _instance.NextAgent();
        }

        void NextAgent()
        {
            switchAgent = false;
            if (queueRoom)
                animateRoom();
            else
            {
                //TODO: This is a bit dangerous and should prob test if anyone is still alive.
                do
                {
                    activeAgent++;
                    if (activeAgent >= agents.Count)
                        activeAgent = 0;
                } while (!agents[activeAgent].alive);
                if (OnNewActiveAgent != null)
                    OnNewActiveAgent(agents[activeAgent]);
                agents[activeAgent].Enact();

            }
        }

        public static int Agents
        {
            get { return _instance.agents.Count; }
        }

        bool AllAgentsReady
        {
            get
            {
                for (int i=0, l=agents.Count;i< l;i++)
                {
                    if (!agents[i].isActiveAndEnabled)
                        return false;
                }
                return true;
            }
        }
        public static Coordinate GetAgentPosition(int agentIndex)
        {
            var agent = _instance.agents[agentIndex];

            if (agent.alive)
                return agent.position;
            else
                return Coordinate.InvalidPlacement;
        }

        public static void Reset()
        {
            _instance.roomHistory.Clear();
            _instance.room.Generate();
            _instance.player.NewGame();
        }

        public void animateRoom()
        {
            StartCoroutine(_instance.room.Enact());
        }

        void Update()
        {
            if (activeLevel < 0 && AllAgentsReady && Time.timeSinceLevelLoad > 1f)
            {
                activeLevel = 0;
                ActiveRoom.Generate();
            }
        }

#if UNITY_EDITOR

        void OnGUI()
        {
            GUI.TextArea(new Rect(260, 2, 100, 50), string.Format("Level:\t{0}\nAgents:\t{1}\nActive:\t{2}", roomHistory.Count, agents.Count, activeAgent));
        }
#endif
    }
}
