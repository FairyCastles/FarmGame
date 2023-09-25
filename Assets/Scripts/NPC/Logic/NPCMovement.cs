using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Farm.Save;
using Farm.GameTime;

namespace Farm.NPC
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(DataGUID))]
    public class NPCMovement : MonoBehaviour, ISaveable
    {
        [Header("Schdule")]
        public ScheduleDataList_SO scheduleData;
        private SortedSet<ScheduleDetails> scheduleSet;
        private ScheduleDetails currentSchedule;

        // 临时存储信息
        [SerializeField] 
        private string currentScene;
        private string targetScene;
        private Vector3Int currentGridPosition;
        private Vector3Int targetGridPosition;
        private Vector3Int nextGridPosition;
        private Vector3 nextWorldPosition;

        public string StartScene { set => currentScene = value; }

        [Header("Move")]
        public float normalSpeed = 2f;
        private float minSpeed = 1;
        private float maxSpeed = 3;
        private Vector2 dir;
        [HideInInspector]
        public bool isMoving;

        // Components
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D coll;
        private Animator anim;

        private Stack<MovementStep> movementStepsStack;
        private Coroutine npcMoveRoutine;
        private Grid grid;

        private bool isInitialised;
        private bool npcMove;
        private bool sceneLoaded;

        public bool interactable;
        private bool isFirstLoad;
        private Season currentSeason;
        // Animation Counter
        private float animationBreakTime;
        private bool canPlayStopAnimaiton;
        private AnimationClip stopAnimationClip;
        public AnimationClip blankAnimationClip;
        private AnimatorOverrideController animOverride;

        private TimeSpan GameTime => TimeManager.Instance.GameTime;

        public string GUID => GetComponent<DataGUID>().guid;

        #region Life Function

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            coll = GetComponent<BoxCollider2D>();
            anim = GetComponent<Animator>();
            movementStepsStack = new Stack<MovementStep>();

            animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
            anim.runtimeAnimatorController = animOverride;
            scheduleSet = new SortedSet<ScheduleDetails>();

            foreach (var schedule in scheduleData.scheduleList)
            {
                scheduleSet.Add(schedule);
            }
        }

        private void OnEnable()
        {
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnload;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
            EventHandler.GameMinuteEvent += OnGameMinute;
            EventHandler.StartNewGameEvent += OnStartNewGame;
            EventHandler.EndGameEvent += OnEndGame;
        }

        private void OnDisable()
        {
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnload;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
            EventHandler.GameMinuteEvent -= OnGameMinute;
            EventHandler.StartNewGameEvent -= OnStartNewGame;
            EventHandler.EndGameEvent -= OnEndGame;
        }

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        private void FixedUpdate() 
        {
            if(sceneLoaded)
            {
                Movement();
            }
        }

        private void Update()
        {
            if (sceneLoaded)
            {
                SwitchAnimation();
            }

            // 计时器
            animationBreakTime -= Time.deltaTime;
            canPlayStopAnimaiton = animationBreakTime <= 0;
        }

        #endregion

        #region Event Function

        private void OnBeforeSceneUnload()
        {
            sceneLoaded = false;
        }

        private void OnAfterSceneLoaded()
        {
            grid = FindObjectOfType<Grid>();
            CheckVisiable();
            if(!isInitialised)
            {
                InitNPC();
                isInitialised = true;
            }
            sceneLoaded = true;

            // 从已有的存档读取数据，新生成 Schedule
            if (!isFirstLoad)
            {
                currentGridPosition = grid.WorldToCell(transform.position);
                // 立刻执行 Schedule
                var schedule = new ScheduleDetails(0, 0, 0, 0, currentSeason, targetScene, (Vector2Int)targetGridPosition, stopAnimationClip, interactable);
                BuildPath(schedule);
                isFirstLoad = true;
            }
        }

        private void OnGameMinute(int minute, int hour, int day, Season season)
        {
            int time = (hour * 100) + minute;
            currentSeason = season;

            ScheduleDetails matchSchedule = null;

            foreach (var schedule in scheduleSet)
            {
                if (schedule.Time == time)
                {
                    if (schedule.day != day && schedule.day != 0) continue;
                    if (schedule.season != season) continue;
                    matchSchedule = schedule;
                }
                else if (schedule.Time > time) break;
            }
            if (matchSchedule != null)
            {
                BuildPath(matchSchedule);
            }
        }

        private void OnStartNewGame(int index)
        {
            isInitialised = false;
            isFirstLoad = true;
        }

        private void OnEndGame()
        {
            sceneLoaded = false;
            npcMove = false;
            // 结束游戏时，要关闭所有NPC显示
            SetInactiveInScene();
            if (npcMoveRoutine != null)
            {
                StopCoroutine(npcMoveRoutine);
            }
        }

        #endregion

        /// <summary>
        /// 判断 NPC 是否应该在当前场景显示，设置其可见状态
        /// </summary>
        private void CheckVisiable()
        {
            if (currentScene == SceneManager.GetActiveScene().name)
                SetActiveInScene();
            else SetInactiveInScene();
        }

        #region 设置NPC显示情况
        private void SetActiveInScene()
        {
            spriteRenderer.enabled = true;
            coll.enabled = true;
            transform.GetChild(0).gameObject.SetActive(true);
        }

        private void SetInactiveInScene()
        {
            spriteRenderer.enabled = false;
            coll.enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
        }

        /// <summary>
        /// 初始化 NPC 位置
        /// </summary>
        private void InitNPC()
        {
            targetScene = currentScene;

            currentGridPosition = grid.WorldToCell(transform.position);
            transform.position = new Vector3(currentGridPosition.x + Settings.gridCellSize / 2f, currentGridPosition.y + Settings.gridCellSize / 2f, 0);
            targetGridPosition = currentGridPosition;
        }

        #endregion

        /// <summary>
        /// 根据 Schedule 构建路径
        /// </summary>
        /// <param name="schedule"></param>
        public void BuildPath(ScheduleDetails schedule)
        {
            movementStepsStack.Clear();
            // 从 Schedule 获取信息
            currentSchedule = schedule;
            targetScene = schedule.targetScene;
            targetGridPosition = (Vector3Int)schedule.targetGridPosition;
            stopAnimationClip = schedule.clipAtStop;
            interactable = schedule.interactable;

            // 同场景移动
            if (schedule.targetScene == currentScene)
            {
                AStar.Instance.BuildPath(schedule.targetScene, (Vector2Int)currentGridPosition, schedule.targetGridPosition, movementStepsStack);
            }
            // 跨场景移动
            // FIXME: NPC 跨场景移动有问题，会直接瞬移到另一个场景，后面修复
            else if(schedule.targetScene != currentScene)
            {
                SceneRoute sceneRoute = NPCManager.Instance.GetSceneRoute(currentScene, schedule.targetScene);

                if(sceneRoute != null)
                {
                    for (int i = 0; i < sceneRoute.scenePathList.Count; i++)
                    {
                        Vector2Int fromPos, gotoPos;
                        ScenePath path = sceneRoute.scenePathList[i];

                        if (path.fromGridCell.x >= Settings.maxGridSize)
                        {
                            fromPos = (Vector2Int)currentGridPosition;
                        }
                        else
                        {
                            fromPos = path.fromGridCell;
                        }

                        if (path.gotoGridCell.x >= Settings.maxGridSize)
                        {
                            gotoPos = schedule.targetGridPosition;
                        }
                        else
                        {
                            gotoPos = path.gotoGridCell;
                        }

                        AStar.Instance.BuildPath(path.sceneName, fromPos, gotoPos, movementStepsStack);
                    }
                }
            }

            if (movementStepsStack.Count > 1)
            {
                //更新每一步对应的时间戳
                UpdateTimeOnPath();
            }
        }


        /// <summary>
        /// 计算行走路径的时间，更新时间戳
        /// </summary>
        private void UpdateTimeOnPath()
        {
            MovementStep previousSetp = null;

            TimeSpan currentGameTime = GameTime;

            // 遍历栈，计算每一步的时间
            foreach (MovementStep step in movementStepsStack)
            {
                if (previousSetp == null) previousSetp = step;

                step.hour = currentGameTime.Hours;
                step.minute = currentGameTime.Minutes;
                step.second = currentGameTime.Seconds;

                TimeSpan gridMovementStepTime;

                if (MoveInDiagonal(step, previousSetp))
                    gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold));
                else
                    gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellSize / normalSpeed / Settings.secondThreshold));

                //累加获得下一步的时间戳
                currentGameTime = currentGameTime.Add(gridMovementStepTime);
                //循环下一步
                previousSetp = step;
            }
        }

        private void Movement()
        {
            if (!npcMove)
            {
                if (movementStepsStack.Count > 0)
                {
                    MovementStep step = movementStepsStack.Pop();

                    currentScene = step.sceneName;

                    CheckVisiable();

                    nextGridPosition = (Vector3Int)step.gridCoordinate;
                    TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second);

                    MoveToGridPosition(nextGridPosition, stepTime);
                }
                else if (!isMoving && canPlayStopAnimaiton)
                {
                    StartCoroutine(SetStopAnimation());
                }
            }
        }

        /// <summary>
        /// 在一定时间内移动到指定格子位置
        /// </summary>
        /// <param name="gridPos"></param>
        /// <param name="stepTime"></param>
        private void MoveToGridPosition(Vector3Int gridPos, TimeSpan stepTime)
        {
            npcMoveRoutine = StartCoroutine(MoveRoutine(gridPos, stepTime));
        }

        /// <summary>
        /// 协程控制 NPC 每次移动一点点
        /// </summary>
        /// <param name="gridPos"></param>
        /// <param name="stepTime"></param>
        /// <returns></returns>
        private IEnumerator MoveRoutine(Vector3Int gridPos, TimeSpan stepTime)
        {
            npcMove = true;
            nextWorldPosition = GetWorldPosition(gridPos);

            // 还有时间用来移动
            if (stepTime > GameTime)
            {
                // 用来移动的时间差，以秒为单位
                float timeToMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
                // 实际移动距离
                float distance = Vector3.Distance(transform.position, nextWorldPosition);
                // 实际移动速度
                float speed = Mathf.Max(minSpeed, distance / timeToMove / Settings.secondThreshold);

                if (speed <= maxSpeed)
                {
                    while (Vector3.Distance(transform.position, nextWorldPosition) > Settings.pixelSize)
                    {
                        dir = (nextWorldPosition - transform.position).normalized;

                        Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime, dir.y * speed * Time.fixedDeltaTime);
                        rb.MovePosition(rb.position + posOffset);
                        yield return new WaitForFixedUpdate();
                    }
                }
            }
            // 如果时间已经到了就瞬移
            rb.position = nextWorldPosition;
            currentGridPosition = gridPos;
            nextGridPosition = currentGridPosition;

            npcMove = false;
        }

        /// <summary>
        /// 判断是否走斜方向
        /// </summary>
        /// <param name="currentStep"></param>
        /// <param name="previousStep"></param>
        /// <returns></returns>
        private bool MoveInDiagonal(MovementStep currentStep, MovementStep previousStep)
        {
            return (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y);
        }

        /// <summary>
        /// 返回一个网格的世界坐标
        /// </summary>
        /// <param name="gridPos"></param>
        /// <returns></returns>
        private Vector3 GetWorldPosition(Vector3Int gridPos)
        {
            Vector3 worldPos = grid.CellToWorld(gridPos);
            return new Vector3(worldPos.x + Settings.gridCellSize / 2f, worldPos.y + Settings.gridCellSize / 2f, 0);
        }

        /// <summary>
        /// 设置动画状态机变量
        /// </summary>
        private void SwitchAnimation()
        {
            isMoving = transform.position != GetWorldPosition(targetGridPosition);

            anim.SetBool("isMoving", isMoving);
            if (isMoving)
            {
                anim.SetBool("Exit", true);
                anim.SetFloat("DirX", dir.x);
                anim.SetFloat("DirY", dir.y);
            }
            else
            {
                anim.SetBool("Exit", false);
            }
        }

        /// <summary>
        /// 设置结束动画
        /// </summary>
        /// <returns></returns>
        private IEnumerator SetStopAnimation()
        {
            //强制面向镜头
            anim.SetFloat("DirX", 0);
            anim.SetFloat("DirY", -1);

            animationBreakTime = Settings.animationBreakTime;
            if (stopAnimationClip != null)
            {
                animOverride[blankAnimationClip] = stopAnimationClip;
                anim.SetBool("EventAnimation", true);
                yield return null;
                anim.SetBool("EventAnimation", false);
            }
            else
            {
                animOverride[stopAnimationClip] = blankAnimationClip;
                anim.SetBool("EventAnimation", false);
            }
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.characterPosDict = new Dictionary<string, SerializableVector3>
            {
                { "targetGridPosition", new SerializableVector3(targetGridPosition) },
                { "currentPosition", new SerializableVector3(transform.position) }
            };

            saveData.dataSceneName = currentScene;
            saveData.targetScene = this.targetScene;

            if (stopAnimationClip != null)
            {
                saveData.animationInstanceID = stopAnimationClip.GetInstanceID();
            }

            saveData.interactable = this.interactable;

            saveData.timeDict = new Dictionary<string, int>
            {
                { "currentSeason", (int)currentSeason }
            };

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            // 设置 NPC 已进行过初始化
            isInitialised = true;
            isFirstLoad = false;
            currentScene = saveData.dataSceneName;
            targetScene = saveData.targetScene;

            Vector3 pos = saveData.characterPosDict["currentPosition"].ToVector3();
            Vector3Int gridPos = (Vector3Int)saveData.characterPosDict["targetGridPosition"].ToVector2Int();

            transform.position = pos;
            targetGridPosition = gridPos;

            if(saveData.animationInstanceID != 0)
            {
                this.stopAnimationClip = Resources.InstanceIDToObject(saveData.animationInstanceID) as AnimationClip;
            }

            this.interactable = saveData.interactable;
            this.currentSeason = (Season)saveData.timeDict["currentSeason"];
        }
    }
}