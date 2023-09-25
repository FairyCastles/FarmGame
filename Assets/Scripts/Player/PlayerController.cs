using System.Collections;
using System.Collections.Generic;
using Farm.Save;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISaveable
{
    private Rigidbody2D playerRigidBody;

    [Header("Player Control")]
    public float speed;
    private float inputX;
    private float inputY;
    private bool isMoving;
    private Vector2 moveInput;
    private Animator[] animators;

    [Header("Animation")]
    private float mouseX;
    private float mouseY;
    private bool useTool;

    private bool inputDisable;

    public string GUID => GetComponent<DataGUID>().guid;

    #region Life Function

    private void Awake() 
    {
        playerRigidBody = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();
        inputDisable = true;
    }

    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }

    private void OnEnable() 
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnload;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoad;
        EventHandler.MoveToPositionEvent += OnMoveToPosition;
        EventHandler.MouseClickedEvent += OnMouseClicked;
        EventHandler.UpdateGameStateEvent += OnUpdateGameState;
        EventHandler.StartNewGameEvent += OnStartNewGame;
        EventHandler.EndGameEvent += OnEndGame;
    }

    private void OnDisable() 
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnload;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoad;
        EventHandler.MoveToPositionEvent -= OnMoveToPosition;
        EventHandler.MouseClickedEvent -= OnMouseClicked;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameState;
        EventHandler.StartNewGameEvent -= OnStartNewGame;
        EventHandler.EndGameEvent -= OnEndGame;
    }

    private void Update() 
    {
        if(!inputDisable)
        {
            PlayerInput();
        }
        else
        {
            isMoving = false;
        }
        SwitchAnimation();
    }

    private void FixedUpdate() 
    {
        if(!inputDisable)
        {
            Movement();
        }
    }

    #endregion

    #region Event Function

    private void OnBeforeSceneUnload()
    {
        inputDisable = true;
    }

    private void OnAfterSceneLoad()
    {
        inputDisable = false;
    }

    private void OnMoveToPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    private void OnMouseClicked(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        if (useTool)
            return;

        // 类型是工具，切换动画控制器，进行动画
        if (itemDetails.itemType != ItemType.Seed && itemDetails.itemType != ItemType.Commodity && itemDetails.itemType != ItemType.Furniture)
        {
            mouseX = mouseWorldPos.x - transform.position.x;
            // 玩家以脚底为锚点，所以计算距离差要增加身体高度一半的偏移
            mouseY = mouseWorldPos.y - (transform.position.y + 0.85f);

            // X 位移比 Y 的大，执行 X 轴的动画，将动画控制参数进行修改
            if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
            {
                mouseY = 0;
            }
            else
            {
                mouseX = 0;
            }
            StartCoroutine(UseToolRoutine(mouseWorldPos, itemDetails));
        }
        // 其他情况，不执行动画，直接调用事件
        else
        {
            EventHandler.CallExecuteActionAfterAnimationEvent(mouseWorldPos, itemDetails);
        }
    }

    private void OnUpdateGameState(GameState gameState)
    {
        switch(gameState)
        {
            case GameState.Gameplay:
                inputDisable = false;
                break;
            case GameState.Pause:
                inputDisable = true;
                break;
        }
    }

    private void OnStartNewGame(int index)
    {
        inputDisable = false;
        transform.position = Settings.playerStartPos;
    }

    private void OnEndGame()
    {
        inputDisable = true;
    }

    #endregion

    // 读取用户的输入
    private void PlayerInput()
    {
        inputX = Input.GetAxisRaw("Horizontal") * 0.5f;
        inputY = Input.GetAxisRaw("Vertical") * 0.5f;

        // 当斜向移动时，降低移动速度
        if (inputX != 0 && inputY != 0)
        {
            inputX *= 0.6f;
            inputY *= 0.6f;
        }

        // 按下 Shift 进行跑动
        if(Input.GetKey(KeyCode.LeftShift))
        {
            inputX *= 2f;
            inputY *= 2f;
        }

        // 用户的输入控制移动
        moveInput = new Vector2(inputX, inputY);
        // 判断是否移动
        isMoving = moveInput != Vector2.zero;
    }

    // 根据用户输入，控制玩家移动
    private void Movement()
    {
        playerRigidBody.MovePosition(playerRigidBody.position + moveInput * speed * Time.fixedDeltaTime);
    }

    private void SwitchAnimation()
    {
        foreach(Animator animator in animators)
        {
            animator.SetBool("isMoving", isMoving);
            animator.SetFloat("mouseX", mouseX);
            animator.SetFloat("mouseY", mouseY);
            if(isMoving)
            {
                animator.SetFloat("InputX", inputX);
                animator.SetFloat("InputY", inputY);
            }
        }
    }
    
    private IEnumerator UseToolRoutine(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        useTool = true;
        inputDisable = true;
        yield return null;

        foreach (var anim in animators)
        {
            anim.SetTrigger("useTool");
            // 人物的面朝方向
            anim.SetFloat("InputX", mouseX);
            anim.SetFloat("InputY", mouseY);
        }

        yield return new WaitForSeconds(0.45f);
        EventHandler.CallExecuteActionAfterAnimationEvent(mouseWorldPos, itemDetails);
        yield return new WaitForSeconds(0.25f);
        // 等待动画结束
        useTool = false;
        inputDisable = false;
    }

    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add(this.name, new SerializableVector3(transform.position));
        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        Vector3 targetPosition = saveData.characterPosDict[this.name].ToVector3();

        transform.position = targetPosition;
    }
}
