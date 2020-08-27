using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    //单例
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }

    }
    //Grid的行列数
    public int xColumn;
    public int yRow;

    public Transform spawner;
    public GameObject chocolatePre;
    //方块中甜品的类型
    public enum SweetType
    {
        EMPTY,
        NORMAL,
        BARRIER,
        ROW_CLEAR,
        COLUMN_CLEAR,
        RAINBOWCANDY,
        COUNT
    }
    [System.Serializable]
    public struct sweetPrefab
    {
        public SweetType type;
        public GameObject prefab;
    }
    public sweetPrefab[] sweetPrefabs;
    private Dictionary<SweetType, GameObject> sweetDict;//通过类型来获取物体

    private GameSweet[,] sweets;//定义二维数组来存放甜品
    private float fillTime = 0.1f;

    private GameSweet pressSweet;//按下鼠标后选中的甜品
    private GameSweet enterSweet;//鼠标进入后选中的甜品


    #region 有关UI的定义
    public Text timeText;
    private float timer = 60;
    private bool gameOver = false;
    public int score;
    public Text scoreText;
    private float addScoreTimer = 0;
    private int currentScore=0;
    public Text finalScore;
    public GameObject gameoverPanel;
    #endregion
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    // Use this for initialization
    void Start () {
        score = 0;
        InitDict();
        InitGrid();
        InitSweet();
        Destroy(sweets[4, 4].gameObject);
        CreatNewSweet(4, 4, SweetType.BARRIER);
        StartCoroutine(AllFill());
    }

    void Update()
    {
        if (gameOver)
        {
            return;
        }
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = 0;
            gameOver = true;
            gameoverPanel.SetActive(true);
            finalScore.text = score.ToString();
        }
        timeText.text = timer.ToString("0");
        if (addScoreTimer <= 0.05f)
        {
            addScoreTimer += Time.deltaTime;
        }
        else//让分数慢慢上涨
        {
            if (currentScore < score)
            {
                currentScore++;
                scoreText.text = currentScore.ToString();
            }
        }
        
    }
    /// <summary>
    /// 初始化字典
    /// </summary>
    public void InitDict()
    {
        sweetDict = new Dictionary<SweetType, GameObject>();
        for (int i = 0; i < sweetPrefabs.Length; i++)
        {
            if (!sweetDict.ContainsKey(sweetPrefabs[i].type))
            {
                sweetDict.Add(sweetPrefabs[i].type, sweetPrefabs[i].prefab);
            }
        }
    }
    /// <summary>
    /// 生成游戏中初始的格子
    /// </summary>
    public void InitGrid()
    {
        for (int i = 0; i < xColumn; i++)
        {
            for (int j = 0; j < yRow; j++)
            {
                GameObject go = Instantiate(chocolatePre, CorrectPosition(i, j), Quaternion.identity);
                go.transform.SetParent(spawner);
            }
        }
    }
	public Vector3 CorrectPosition(float x,float  y)//设置正确的位置
    {
        return new Vector3(spawner.transform.position.x - xColumn / 2f + x, spawner.transform.position.y + yRow / 2 - y);
    }

    /// <summary>
    /// 初始化甜品的生成
    /// </summary>
    public void InitSweet()
    {
        sweets = new GameSweet[xColumn, yRow];
        for(int x = 0; x < xColumn; x++)
        {
            for(int y = 0; y < yRow; y++)
            {
                CreatNewSweet(x, y, SweetType.EMPTY);
            }
        }
    }
    //在指定位置创建一个指定类型的甜品
    public GameSweet CreatNewSweet(int x,int y,SweetType type)
    {
        GameObject newSweet = Instantiate(sweetDict[type], CorrectPosition(x, y), Quaternion.identity);
        newSweet.transform.SetParent(spawner);
        sweets[x, y] = newSweet.GetComponent<GameSweet>();
        sweets[x, y].Init(x, y, this, type);
        return sweets[x, y];
    }

    /// <summary>
    /// 全部填充
    /// </summary>
    public IEnumerator AllFill()
    {
        bool needReFill = true;
        while (needReFill)
        {
            while (Fill())
            {
                yield return new WaitForSeconds(fillTime);
            }
            needReFill = ClearAllMatchSweet();
        }
        
    }

    /// <summary>
    /// 分步填充
    /// </summary>
    public bool Fill()
    {
        bool filledNotFinish = false;//是否填充完成
        for (int y = yRow - 2; y >= 0; y--)
        {
            for(int x = 0; x < xColumn; x++)
            {  
                GameSweet sweet = sweets[x, y];
                if (sweet.CanMove())//如果可以移动，则可以向下继续填充
                {
                    GameSweet sweetBelow = sweets[x, y + 1];
                    if (sweetBelow.Type == SweetType.EMPTY)//如果当前甜品下方为空甜品，则优先垂直填充
                    {
                        Destroy(sweetBelow.gameObject);
                        sweet.MoveSweetComponent.Move(x, y + 1, fillTime);
                        sweets[x, y + 1] = sweet;
                        CreatNewSweet(x, y, SweetType.EMPTY);
                        filledNotFinish = true;
                    }
                    else//斜向填充
                    {
                        for(int down = -1; down <= 1; down++)//-1代表左下方，1代表右下方
                        {
                            if (down != 0)
                            {
                                int downX = x + down;
                                if (downX >= 0 && downX < xColumn)//排除边界
                                {
                                    GameSweet bevelSweet = sweets[downX, y + 1];//斜方向的甜品
                                    if (bevelSweet.Type == SweetType.EMPTY)//如果该甜品斜下方向为空甜品
                                    {
                                        bool canBevelFill = true;//用来表示是否可以斜向填充
                                        for(int aboveY = y; aboveY >= 0; aboveY--)
                                        {
                                            GameSweet bevelAboveSweet = sweets[downX, aboveY];//斜下方甜品的正上方的甜品
                                            if (bevelAboveSweet.Type != SweetType.EMPTY && !bevelAboveSweet.CanMove())
                                            {
                                                //如果不能移动且不为空，则可以进行斜向填充
                                                canBevelFill = false;
                                                break;
                                            }
                                            else if(bevelAboveSweet.CanMove())
                                            {
                                                //不做处理
                                                break;
                                            }
                                        }
                                        if (canBevelFill == false)
                                        {
                                            Destroy(bevelSweet.gameObject);
                                            sweets[x, y].MoveSweetComponent.Move(downX, y + 1, fillTime);
                                            sweets[downX, y + 1] = sweet;
                                            CreatNewSweet(x, y, SweetType.EMPTY);
                                            filledNotFinish = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        //最上面一排特殊情况
        for(int x = 0; x < xColumn; x++)
        {
            GameSweet sweet = sweets[x, 0];
            if (sweet.Type ==SweetType.EMPTY)//如果该甜品为空甜品，则进行实例化生成新的甜品
            {
                GameObject newSweet = Instantiate(sweetDict[SweetType.NORMAL], CorrectPosition(x, -1), Quaternion.identity);
                newSweet.transform.SetParent(spawner);
                sweets[x, 0] = newSweet.GetComponent<GameSweet>();
                sweets[x, 0].Init(x, -1, this, SweetType.NORMAL);
                sweets[x, 0].MoveSweetComponent.Move(x, 0, fillTime);
                sweets[x, 0].ColorSweetComponent.SetColor((ColorSweet.ColorType)Random.Range(0, sweets[x, 0].ColorSweetComponent.NumColor));
                Destroy(sweet.gameObject);

                filledNotFinish = true;
            }
        }
        return filledNotFinish;
    }

    /// <summary>
    /// 判断两个甜品是否相邻
    /// </summary>
    /// <param name="sweet1"></param>
    /// <param name="sweet2"></param>
    /// <returns></returns>
    private bool IsClose(GameSweet sweet1,GameSweet sweet2)
    {
        return (sweet1.X == sweet2.X && Mathf.Abs(sweet1.Y - sweet2.Y) == 1) || (sweet1.Y == sweet2.Y && Mathf.Abs(sweet1.X - sweet2.X) == 1);
    }

    /// <summary>
    /// 将两个甜品位置交换
    /// </summary>
    /// <param name="sweet1"></param>
    /// <param name="sweet2"></param>
    private void ExchangeSweets(GameSweet sweet1, GameSweet sweet2)
    {
        if (sweet1.CanMove() && sweet2.CanMove())
        {
            sweets[sweet1.X, sweet1.Y] = sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;
            int tempX = sweet1.X;
            int tempY = sweet1.Y;
            if (MatchSweets(sweet1,sweet2.X,sweet2.Y)!=null|| MatchSweets(sweet2, sweet1.X, sweet1.Y) != null || sweet1.Type == SweetType.RAINBOWCANDY || sweet2.Type == SweetType.RAINBOWCANDY)
            {
                //如果匹配到了最少三个相同的甜品则交换位置，且不能返回之前的位置
                sweet1.MoveSweetComponent.Move(sweet2.X, sweet2.Y, fillTime);//由于两个携程会影响甜品的位置，因此之前要先记录之前的位置
                sweet2.MoveSweetComponent.Move(tempX, tempY, fillTime);
                //如果两个甜品中有最少一个彩虹糖也可进入该语句
                if (sweet1.Type == SweetType.RAINBOWCANDY && sweet1.CanClear() && sweet2.CanClear())
                {
                    ClearSameSweet clearColor = sweet1.GetComponent<ClearSameSweet>();

                    if (clearColor != null)
                    {
                        clearColor.Color = sweet2.ColorSweetComponent.Color;
                    }
                    ClearSweet(sweet1.X, sweet1.Y);
                   
                }

                if (sweet2.Type == SweetType.RAINBOWCANDY && sweet2.CanClear() && sweet1.CanClear())
                {
                    ClearSameSweet clearColor = sweet2.GetComponent<ClearSameSweet>();

                    if (clearColor != null)
                    {
                        clearColor.Color = sweet1.ColorSweetComponent.Color;
                    }
                    ClearSweet(sweet2.X, sweet2.Y);
                   
                }

                ClearAllMatchSweet();
                StartCoroutine(AllFill());
            }
            else
            {
                //如果没匹配到三个相同的三个元素，则先交换后返回
                sweets[sweet1.X, sweet1.Y] = sweet1;
                sweets[sweet2.X, sweet2.Y] = sweet2;
               sweet1.MoveSweetComponent.Move(sweet2.X, sweet2.Y, fillTime);//由于两个携程会影响甜品的位置，因此之前要先记录之前的位置
               sweet2.MoveSweetComponent.Move(tempX, tempY, fillTime);
               StartCoroutine(Wait(0.5f, sweet1, sweet2));
            }
        }
    }
    //按下鼠标监听该事件
    public void PressSweet(GameSweet sweet)
    {
        if (gameOver)
        {
            return;
        }
        pressSweet = sweet;
    }
    //鼠标进入监听该事件
    public void EnterSweet(GameSweet sweet)
    {
        if (gameOver)
        {
            return;
        }
        enterSweet = sweet;
    }
    //松开鼠标监听该事件
    public void Release()
    {
        if (gameOver)
        {
            return;
        }
        if (IsClose(pressSweet,enterSweet))
        {
            ExchangeSweets(pressSweet, enterSweet);
        }
    }
    /// <summary>
    /// 判断是否能进行消除甜品
    /// </summary>
    /// <param name="sweet"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public List<GameSweet> MatchSweets(GameSweet sweet,int newX,int newY)
    {
        if (sweet.CanMove()&&sweet.CanColor())//如果当前甜品是能正常更换精灵的甜品（即不是空甜品或者不是障碍物等）
        {
            ColorSweet.ColorType color = sweet.ColorSweetComponent.Color;
            List<GameSweet> matchRowSweetsList = new List<GameSweet>();//用来做横向匹配的链表
            List<GameSweet> matchColumnSweetList = new List<GameSweet>();// 用来做纵向匹配的链表
            List<GameSweet> finishedMatchList = new List<GameSweet>();//匹配结束后的链表，同时返回该链表
            //行匹配
            matchRowSweetsList.Add(sweet);
            for(int i = 0; i <= 1; i++)//0代表往左匹配，1代表往右匹配
            {
                for (int xDistance = 1; xDistance < xColumn; xDistance++)
                {
                    int x;//和当前甜品要进行匹配的甜品的x坐标
                    if (i == 0)
                    {
                        x = newX - xDistance;
                    }
                    else
                    {
                        x = newX + xDistance;
                    }
                    if (x < 0 || x >= xColumn)//如果匹配完成则直接退出
                    {
                        break;
                    }
                    if (sweets[x, newY].CanColor() && color == sweets[x, newY].ColorSweetComponent.Color)//如果类型相同，则可以添加进链表
                    {
                        matchRowSweetsList.Add(sweets[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (matchRowSweetsList.Count >= 3)
            {
                for (int m = 0; m < matchRowSweetsList.Count; m++)
                {
                    finishedMatchList.Add(matchRowSweetsList[m]);
                }
            }

            if (matchRowSweetsList.Count >= 3)
            {
                for(int i=0;i< matchRowSweetsList.Count; i++)
                {
                   // finishedMatchList.Add(matchRowSweetsList[i]);
                    //行匹配完成后，再进行L，T型匹配，即对每一个二元素的列在进行匹配
                    for(int j=0;j<=1; j++)//j=0代表向上匹配，j=1代表向下匹配
                    {
                        for(int yDistance = 1; yDistance < yRow; yDistance++)
                        {
                            int y;
                            if (j == 0)
                            {
                                y = newY - yDistance;
                            }
                            else
                            {
                                y = newY + yDistance;
                            }
                            if (y < 0 || y >=yRow)
                            {
                                break;
                            }
                            if(sweets[matchRowSweetsList[i].X,y].CanColor()&& sweets[matchRowSweetsList[i].X, y].ColorSweetComponent.Color == color)
                            {
                                matchColumnSweetList.Add(sweets[matchRowSweetsList[i].X, y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (matchColumnSweetList.Count >= 2)//如果又在列方向找到两个或以上甜品，则添加到完成列表中
                    {
                        for (int k = 0; k < matchColumnSweetList.Count; k++)
                        {
                            finishedMatchList.Add(matchColumnSweetList[k]);
                        }
                        break;
                    }
                    else//否则进行清空
                    {
                        matchColumnSweetList.Clear();
                    }
                }
            }
            if (finishedMatchList.Count >= 3)
            {
                return finishedMatchList;
            }
            matchRowSweetsList.Clear();
            matchColumnSweetList.Clear();
           // finishedMatchList.Clear();





           //清空后再进行列匹配
            //否则进行列匹配
            matchColumnSweetList.Add(sweet);
            for (int i = 0; i <= 1; i++)//0代表往上匹配，1代表往下匹配
            {
                for (int YDistance = 1; YDistance < yRow; YDistance++)
                {
                    int y;//和当前甜品要进行匹配的甜品的x坐标
                    if (i == 0)
                    {
                        y = newY - YDistance;
                    }
                    else
                    {
                        y = newY + YDistance;
                    }
                    if (y < 0 || y >= yRow)//如果匹配完成则直接退出
                    {
                        break;
                    }
                    if (sweets[newX, y].CanColor() && color == sweets[newX, y].ColorSweetComponent.Color)//如果类型相同，则可以添加进链表
                    {
                        matchColumnSweetList.Add(sweets[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (matchColumnSweetList.Count >= 3)
            {
                for (int m = 0; m < matchColumnSweetList.Count; m++)
                {
                    finishedMatchList.Add(matchColumnSweetList[m]);
                }
            }

            if (matchColumnSweetList.Count >= 3)//列匹配完成后再进行L，T，型匹配，即再对每一个已经确定的甜品进行行遍历
            {
                for (int i = 0; i < matchColumnSweetList.Count; i++)
                {
                    //finishedMatchList.Add(matchColumnSweetList[i]);
                    
                    for (int j = 0; j <= 1; j++)//j=0代表向左匹配，j=1代表向右匹配
                    {
                        for (int xDistance = 1; xDistance < xColumn; xDistance++)
                        {
                            int x;
                            if (j == 0)
                            {
                                x = newX - xDistance;
                            }
                            else
                            {
                                x = newX + xDistance;
                            }
                            if (x < 0 || x >= yRow)
                            {
                                break;
                            }
                            if (sweets[x, matchColumnSweetList[i].Y].CanColor() && sweets[x, matchColumnSweetList[i].Y].ColorSweetComponent.Color == color)
                            {
                                matchRowSweetsList.Add(sweets[x, matchColumnSweetList[i].Y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (matchRowSweetsList.Count >= 2)//如果又在行方向找到两个或以上甜品，则添加到完成列表中
                    {
                        for (int k = 0; k < matchRowSweetsList.Count; k++)
                        {
                            finishedMatchList.Add(matchRowSweetsList[k]);
                        }
                        break;
                    }
                    else//否则进行清空
                    {
                        matchRowSweetsList.Clear();
                    }
                }
            }
            if (finishedMatchList.Count >= 3)
            {
                return finishedMatchList;
            }
        }
        return null;
    }

    private IEnumerator Wait(float time,GameSweet sweet1,GameSweet sweet2)
    {
        int tempX1 = sweet1.X;
        int tempY1 = sweet1.Y;
        int tempX2 = sweet2.X;
        int tempY2 = sweet2.Y;
        yield return new WaitForSeconds(time);
        sweet2.MoveSweetComponent.Move(tempX1, tempY1, fillTime);
        sweet1.MoveSweetComponent.Move(tempX2, tempY2, fillTime);
    }

    /// <summary>
    /// 清除甜品的方法
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool ClearSweet(int x,int y)
    {
        //如果该位置甜品有清除脚本并且不处于正在清除的状态
        if (sweets[x, y].CanClear() && !sweets[x, y].ClearSweetComponent.IsClearing)
        {
            sweets[x, y].ClearSweetComponent.Clear();
            CreatNewSweet(x, y, SweetType.EMPTY);
            ClearBarrier(x, y);//判断能否清除饼干
            return true;//ture代表清除完成
        }
        return false;
    }

    /// <summary>
    /// 清除饼干的方法
    /// </summary>
    /// <param name="x">甜品的x坐标</param>
    /// <param name="y">甜品的y坐标</param>
    private void ClearBarrier(int x,int y)
    {
        for(int friendX = x - 1; friendX <= x + 1; friendX++)//行项检测
        {
            if (friendX != x && friendX >= 0 && friendX < xColumn)
            {
                if (sweets[friendX, y].CanClear()&&sweets[friendX,y].Type==SweetType.BARRIER)
                {
                    sweets[friendX, y].ClearSweetComponent.Clear();
                    CreatNewSweet(friendX, y, SweetType.EMPTY);
                }
            }
        }

        for(int friendY = y - 1; friendY <= y + 1; friendY++)
        {
            if (friendY != y && friendY >= 0 && friendY < yRow)
            {
                if (sweets[x, friendY].Type == SweetType.BARRIER && sweets[x, friendY].CanClear())
                {
                    sweets[x, friendY].ClearSweetComponent.Clear();
                    CreatNewSweet(x, friendY, SweetType.EMPTY);
                }
            }
        }
    }
    /// <summary>
    /// 清除游戏中所有的符合清除条件的甜品 以及在清除时判断是否能生成特殊功能的甜品
    /// </summary>
    /// <returns></returns>
    public bool ClearAllMatchSweet()
    {
        bool needReFill = false;//是否需要再次填充
        for(int y = 0; y < yRow; y++)
        {
            for(int x = 0;x < xColumn; x++)
            {
                if (sweets[x,y].CanClear())//如果该位置可以被消除
                {
                    List<GameSweet> matchedSweets = MatchSweets(sweets[x, y], x, y);
                    if (matchedSweets != null)
                    {
                        SweetType specialSweetype = SweetType.COUNT;//该类型表示特殊甜品的类型：行消除列消除彩虹糖等甜品(匹配到的甜品数量大于4时才生成特殊甜品)
                        GameSweet speciaSweet = matchedSweets[Random.Range(0, matchedSweets.Count)];
                        int specialSweetX = speciaSweet.X;
                        int specialSweetY = speciaSweet.Y;
                        if (matchedSweets.Count == 4)//如果为4个，则随机生成行消除甜品或列消除甜品
                        {
                            specialSweetype = (SweetType)Random.Range((int)SweetType.ROW_CLEAR, (int)SweetType.COLUMN_CLEAR);
                        }
                        else if (matchedSweets.Count >= 5)//如股票大于等于5个，则生成彩虹糖
                        {
                            specialSweetype = SweetType.RAINBOWCANDY;
                        }
                        for(int i = 0; i < matchedSweets.Count; i++)
                        {
                           if( ClearSweet(matchedSweets[i].X, matchedSweets[i].Y))
                           {
                                needReFill = true;
                           }
                        }
                        if (specialSweetype != SweetType.COUNT)
                        {
                            Destroy(sweets[specialSweetX, specialSweetY]);
                            GameSweet newSweet=CreatNewSweet(specialSweetX, specialSweetY, specialSweetype);
                            if (specialSweetype == SweetType.COLUMN_CLEAR || specialSweetype == SweetType.ROW_CLEAR && newSweet.CanColor() && matchedSweets[0].CanColor())
                            {
                                newSweet.ColorSweetComponent.SetColor(matchedSweets[0].ColorSweetComponent.Color);
                            }
                            //彩虹糖的产生  
                            else if(specialSweetype == SweetType.RAINBOWCANDY && newSweet.CanColor())
                            {
                                newSweet.ColorSweetComponent.SetColor(ColorSweet.ColorType.ANY);
                            }
                        }
                    }
                }
            }
        }
        return needReFill;
    }

    /// <summary>
    /// 重玩按钮的监听事件
    /// </summary>
    public void RetryButtonClick()
    {
        SceneManager.LoadScene(1);
    }

    public void ReturnToStartButtonClick()
    {
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// 消除一整行甜品(包括饼干等障碍)
    /// </summary>
    /// <param name="row"></param>
    public void ClearRowSweets(int row)
    {
        for(int x = 0; x < xColumn; x++)
        {
            ClearSweet(x, row);
        }
    }
    /// <summary>
    /// 消除一整列甜品（包括饼干等障碍）
    /// </summary>
    /// <param name="column"></param>
    public void ClearColumnSweets(int column)
    {
        for(int y=0;y<yRow; y++)
        {
            ClearSweet(xColumn, y);
        }
    }
    /// <summary>
    /// 消除相同的甜品
    /// </summary>
    public void ClearSameSweets(ColorSweet.ColorType colorType)
    {
        for(int x = 0; x < xColumn; x++)
        {
            for (int y= 0; y < yRow; y++)
            {
                if (sweets[x, y].CanColor() && (sweets[x, y].ColorSweetComponent.Color == colorType || colorType == ColorSweet.ColorType.ANY))
                {
                    ClearSweet(x, y);
                }
            }
        }
    }
}
