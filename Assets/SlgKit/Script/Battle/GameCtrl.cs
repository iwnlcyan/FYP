using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static GameDefine;

public class GameCtrl : MonoBehaviour
{
    public static GameCtrl instance { get; private set; }

    [Range(1,4)]
    public float speed = 1;


    public PlayerController[] players;
    private Bounds mapBounds;
    PlayerController curSelect;
    public Sect mySect;
    private bool battle;
    private PlayerController attacker;

    private void Awake()
    {
        instance = this;

        EffectCtrl.Init();

        SkillSys.Instance.Init();

        BehaviorCtrl.Init();

        if (AudioCtrl.instance==null)
        AudioCtrl.Init();



        


    }

    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        players = GameObject.FindObjectsOfType<PlayerController>();

        var gridGraph = (AstarPath.active.graphs[0] as GridGraph);
        var size = gridGraph.nodeSize;
        mapBounds = new Bounds(gridGraph.center, new Vector3(gridGraph.width * size, 10, size * gridGraph.depth));

        // UICtrl.instance.Init_HpImage();
        // StartCoroutine(this.C_StartGame());

        EventDispatcher.instance.Regist<PlayerController>(GameEventType.death, OnDeath);

        EventDispatcher.instance.Regist(GameEventType.GotoHomeClICK,this.OnGotoHomeClICK);
    }

    private void OnGotoHomeClICK()
    {
        SceneManagerExt.instance.LoadSceneShowProgress(SceneType.GameTilte);
    }

    IEnumerator C_StartGame()
    {

        yield return new WaitForEndOfFrame();
        NextOrder();
    }


    private void OnDeath(PlayerController p)
    {
        var n = new List<PlayerController>();
        n.AddRange(players);
        this.players = n.FindAll(s => s.isdeath == false).ToArray();
    }

    //包围盒范围测试，只在编辑器运行
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(mapBounds.center, mapBounds.size);
    }

    internal void CancelSelect()
    {
        UICtrl.instance.actionPanel.SetActive(false);
        curSelect.CancelMove();
        //关闭路径
        GridMeshManager.Instance.DespawnAllPath();

        curSelect = null;
    }

    public void ReleaseSelect()
    {

        UICtrl.instance.actionPanel.SetActive(false);
        //关闭路径
        GridMeshManager.Instance.DespawnAllPath();

        curSelect = null;
    }

    public bool auto = false;
    // Update is called once per frame
    void Update()
    {

        

        //托管不与允许选择人物
        if (auto) return;
        if (BehaviorCtrl.instance.myAutomationBehavior.executting) return;

        //不是自己回合不能点击人物
        if (this.orderSect != mySect) return;

        //释放技能时不能操作
        if (curSelect != null && curSelect.state == PlayerSate.skill) return;



        //防止点击待机UI时进行移动

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        //战斗中不执行操作
        if (this.battle) return;



        var hitWorldPoint = MouseRaycast();

        //跑过去攻击 敌人->记录人物状态（这个时候不再执行鼠标点击逻辑）
        if (curSelect != null && curSelect.state == PlayerSate.moveAttack)
        {
            return;
        }

        if (hitWorldPoint != null)
        {
            var hitMapNode = AstarPath.active.GetNearest((Vector3)hitWorldPoint).node;
            var hitMapPos = hitMapNode.position;

            if (showskillReleaseRange)
            {
                SkillSelectionTarget(this.curSelect, SelectPlayer(hitMapPos));
                return;
            }

            //超出A星图边界，不进行任何处理
            if (!mapBounds.Contains((Vector3)hitWorldPoint))
            {
                if (curSelect != null)
                {
                    this.CancelSelect();
                }
                return;
            }

            //通过地图坐标获取人物
            var hitPlayer = SelectPlayer(hitMapPos);


            //如果当前选择的人物是可控门派，且点击的对象是敌人，且在攻击距离内，则进行攻击
            if (hitPlayer != null && curSelect != null && curSelect.sect == mySect && hitPlayer.sect != curSelect.sect
                && curSelect.state == PlayerSate.idle)
            {
                if (curSelect.InAttackRang(hitPlayer))
                {
                    Debug.Log("在有效范围 直接攻击敌人");
                    SetBattleState();
                    curSelect.Active_Attack(hitPlayer);
                    this.ReleaseSelect();

                    return;
                }//并且在移动范围内，就跑过去攻击
                else if (curSelect.CanMoveAttack(hitPlayer))
                {
                    SetBattleState();
                    Debug.Log("不在有效范围 跑过去攻击");
                    curSelect.MoveAttack(hitPlayer);

                    return;
                }

            }


            //假如点击的玩家为不可控制的门派则仅显示移动范围
            if (hitPlayer != null && hitPlayer.sect != mySect)
            {
                if (curSelect != null)
                {
                    this.CancelSelect();
                }
                hitPlayer.ShowMoveRange();

                return;
            }

            if (curSelect == null)
            {
                curSelect = hitPlayer;
                if (curSelect != null)
                {
                    curSelect.ShowMoveRange();

                    //如果玩家可控则执行 准备指令
                    if (curSelect.sect == mySect&& curSelect.state == PlayerSate.idle)
                    {

                        curSelect.Ready();
                        this.UpdateAciotnPanel(curSelect);
                        UICtrl.instance.actionPanel.SetActive(true);
                    }
                }
            }
            else
            {
                //选择对象时候判断是不是当前指定的对象
                //如果不是则把当前人物的行动取消
                //切换人物后再进行范围移动显示
                var otherSelect = hitPlayer;

                if (otherSelect == curSelect || otherSelect == null)
                {
                    if (curSelect.goMapPos != hitMapPos && curSelect.state == PlayerSate.idle)
                        curSelect.Move(hitMapNode);
                }
                else if (otherSelect != curSelect)
                {


                    UpdateSelect(hitPlayer);



                }

            }

        }

    }

    public void ExchangeMyAutoAi()
    {
        auto = !auto;

        AutoMyScet();


    }

    void AutoMyScet()
    {
        var players = GetPlayersIdle(mySect);

        //要考虑

        if (auto)
        {
           
            BehaviorCtrl.instance.myAutomationBehavior.ReadyStop(false);
            //防止AI运行中重复执行出现不可控的情况
            if (BehaviorCtrl.instance.myAutomationBehavior.executting == false)
                BehaviorCtrl.instance.myAutomationBehavior.Start_Automation(players);
        }
        else
        {
            //防止AI执行到一半被停止
            //自动化进入准备停止阶段，直到当前的AI执行完毕才会真正停止
            BehaviorCtrl.instance.myAutomationBehavior.ReadyStop(true);
        }
    }

    public void AttackSelect_AI(PlayerController from,PlayerController to)
    {
        curSelect = from;
        if (curSelect.InAttackRang(to))
        {
            Debug.Log("在有效范围 直接攻击敌人");
            SetBattleState();
            curSelect.Active_Attack(to);
            this.ReleaseSelect();

            return;
        }//并且在移动范围内，就跑过去攻击
        else if (curSelect.CanMoveAttack(to))
        {
            SetBattleState();
            Debug.Log("不在有效范围 跑过去攻击");
            curSelect.MoveAttack(to);

            return;
        }
    }

    internal void cancel_skillSelectTarget()
    {
        // throw new NotImplementedException();

        showskillReleaseRange = false;

        this.curSelect.ShowMoveRange();
        this.curSelect.CloseActiveSkill_ActionRange();
        UICtrl.instance.showSkillSelectTarget(false);


    }

    internal void cancel_SkillConfirm()
    {
        //throw new NotImplementedException();
        this.ShowActiveSkill_ReleaseRange();

        UICtrl.instance.showSkillConfirm(false);

    }

    internal void confirmClick()
    {
        // throw new NotImplementedException();

        this.curSelect.Releaseskill(this.usingSkill, skillTarget);
        UICtrl.instance.actionPanel.SetActive(false);
        GridMeshManager.Instance.DespawnAllPath();
    }

    public void confirmUseSkill_AI(PlayerController from, PlayerController to)
    {
        this.curSelect = from;
         skillTarget = to;
        this.curSelect.Releaseskill(this.usingSkill, to);
        GridMeshManager.Instance.DespawnAllPath();
    }

    //等待人物选择释放范围内的目标
    public void SkillSelectionTarget(PlayerController from, PlayerController to)
    {
        // throw new NotImplementedException();

        if (to == null) return;
        skillTarget = to;
        if (to != null && this.usingSkill.activeSkillConfig.canSelect(from, to))
        {

            from.ShowActiveSkill_ActionRange(this.usingSkill, skillTarget);
            UICtrl.instance.showSkillConfirm(true);
        }
    }

    public void SkillSelectionTarget(PlayerController from, PlayerController to,Skill skill)
    {
        if (to == null) return;
        skillTarget = to;
        this.usingSkill = skill;
        if (to != null && this.usingSkill.activeSkillConfig.canSelect(from, to))
        {
            from.ShowActiveSkill_ActionRange(this.usingSkill, skillTarget);
          //  UICtrl.instance.showSkillConfirm(true);
        }
    }


    internal void UseSkill(int id)
    {
        if (curSelect.moving) return;

        if (curSelect.skill[id].activeSkillConfig.cd > 0)
        {
            Debug.Log("技能还没冷却");
            return;
        }

        lastSkillId = id;
        ShowActiveSkill_ReleaseRange();
    }

    private void ShowActiveSkill_ReleaseRange()
    {
        //  throw new NotImplementedException();

        this.usingSkill = curSelect.ShowActiveSkill_ReleaseRange(lastSkillId);
        this.showskillReleaseRange = true;
        GridMeshManager.Instance.DespawnAllPath();

        UICtrl.instance.showSkillSelectTarget(true);
    }

    void UpdateSelect(PlayerController playerController)
    {

        if (playerController.state == PlayerSate.idle)
        {
            this.UpdateAciotnPanel(playerController);
            UICtrl.instance.actionPanel.SetActive(true);
            playerController.Ready();
        }
        else
        {
            UICtrl.instance.actionPanel.SetActive(false);
        }
        //AI托管期间不需要限制控制UI
        if (orderSect!=mySect) UICtrl.instance.actionPanel.SetActive(false);

        if (curSelect!=null)
        curSelect.CancelMove();
        //关闭路径
        GridMeshManager.Instance.DespawnAllPath();

        playerController.ShowMoveRange();
        playerController.Ready();
        curSelect = playerController;
    }

    private void UpdateAciotnPanel(PlayerController player)
    {
        //throw new NotImplementedException();

        for (int i = 0; i < 3; i++)
        {
            var id = player.skillcfg[i];
            var showIcon = id != 0;
            if (showIcon) UICtrl.instance.skill_slot[i].button.image.sprite = GetSkillTexture(id);
            UICtrl.instance.skill_slot[i].button.gameObject.SetActive(showIcon);

            var skill = player.skill[i];
            var btn_enabled = (skill != null && skill.activeSkillConfig != null);
            UICtrl.instance.skill_slot[i].button.enabled = btn_enabled;

            //主动技能cd
            if (btn_enabled)
            {
                var cd = skill.activeSkillConfig.cd;
                UICtrl.instance.skill_slot[i].cdImage.gameObject.SetActive(cd > 0);
                UICtrl.instance.skill_slot[i].cdText.text = cd.ToString();
            }
            else
                UICtrl.instance.skill_slot[i].cdImage.gameObject.SetActive(false);
        }
    }

    //根据技能ID获取图片
    Dictionary<uint, Sprite> skillTextureMap = new Dictionary<uint, Sprite>();
    private int lastSkillId;
    private Skill usingSkill;
    private bool showskillReleaseRange;
    private PlayerController skillTarget;
    private Sect orderSect= Sect.SiXiangMen;

    public Sprite GetSkillTexture(uint id)
    {

        if (!skillTextureMap.ContainsKey(id))
        {
            var i = ResourcesExt.Load<Sprite>("Textures/skill/skill_"+ id);
            skillTextureMap.Add(id, i);
        }
        return skillTextureMap[id];
    }

    internal void ActionEnd()
    {
        //throw new NotImplementedException();

        this.showskillReleaseRange = false;
        this.curSelect.state = PlayerSate.wait;
        this.curSelect = null;
        this.battle = false;
        //this.Wait(true);

        StartCoroutine(C_EffectEnd(()=> { this.Wait(true); }));
    }




    private PlayerController SelectPlayer(Int3 hitMapPos)
    {
        foreach (PlayerController player in players)
        {
            if (player.mapPos == hitMapPos)
            {
                return player;
            }
        }

        return null;
    }





    Vector3? MouseRaycast()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //生成一条从摄像机发出的射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //用来存储射线打中物体的信息
            RaycastHit hit;
            //发射射线
            bool result = Physics.Raycast(ray, out hit);
            //如果为true说明打中物体了
            if (result)
            {
                Debug.Log(hit.point);

                return hit.point;

            }

        }

        return null;
    }

    public List<PlayerController> GetEnemy(Sect sect)
    {
        List<PlayerController> enemy = new List<PlayerController>();
        foreach (var item in this.players)
        {
            if (item.sect != sect) enemy.Add(item);
        }


        return enemy;
    }

    internal List<GraphNode> GetPlayersMapNode()
    {
        List<GraphNode> t = new List<GraphNode>();
        foreach (var item in this.players)
        {
            t.Add(item.mapNode);
        }


        return t;
    }

    public void Wait_AI(PlayerController playerC)
    {
        this.curSelect = playerC;
        Wait();
    }

    internal bool Wait(bool p_AttackRoundEnd=false)
    {
        
        if (this.curSelect!=null&&this.curSelect.moving) return false;
        //throw new NotImplementedException();
        if (!p_AttackRoundEnd)
        {
            this.curSelect.state = PlayerSate.wait;
            this.ReleaseSelect();
        }

        PlayerController idlePlayer = null;
        if (orderSect==mySect)
             idlePlayer = IdlePlayer(mySect);
        else
             idlePlayer = IdlePlayer(Sect.SiXiangMen);

        if (idlePlayer != null)
        {
            UpdateSelect(idlePlayer);
        }
        else
        {
            Debug.Log("回合结束！");

            StartCoroutine(OrderOvery());
        }
        return true;
    }


    IEnumerator C_EffectEnd(System.Action func)
    {
        //等待特效播放完成
        while (EffectCtrl.instance.playeffect)
        {
            yield return new WaitForEndOfFrame();
        }

        if (func != null) func.Invoke();
    }

    IEnumerator OrderOvery()
    {
        yield return StartCoroutine(C_EffectEnd(null));
        yield return new WaitForSeconds(1.5f);
        this.NextOrder();
        

    }



    void EnemyOrder()
    {
        //敌人由AI进行托管
        var enemys = GetEnemy(this.mySect);

        if (enemys.Count != 0)
            BehaviorCtrl.instance.enemyAutomationBehavior.Start_Automation(enemys);
        else
        {
            StartCoroutine(OrderOvery());
        }
    }

  
    IEnumerator ReadyNextOrder()
    {
        //等待特效播放完成
        while(EffectCtrl.instance.playeffect)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1.5f);

        this.NextOrder();
    }


    private PlayerController IdlePlayer(Sect sect)
    {
        foreach (PlayerController item in this.players)
        {
            if (item.sect == sect && item.state == PlayerSate.idle) return item;
        }

        return null;
    }

    //防止战斗中操作
    private void SetBattleState()
    {
        UICtrl.instance.actionPanel.SetActive(false);
        //throw new NotImplementedException();
        this.battle = true;
        attacker = curSelect;

    }

    internal void AttackRoundEnd()
    {
       
        attacker.state = PlayerSate.wait;

        this.battle = false;
        this.Wait(true);
    }


    private void NextOrder()
    {
        if (orderSect == Sect.SiXiangMen)
            orderSect = Sect.TianXuanMen;
        else if (orderSect == Sect.TianXuanMen)
            orderSect = Sect.SiXiangMen;

        if (orderSect == mySect)
            UICtrl.instance.ShowOrderTip("orderTipOpen_My");
        else
            UICtrl.instance.ShowOrderTip("orderTipOpen_Enemy");

        //把所有玩家设置为可行动

        var players = GetPlayers(orderSect);

        foreach (PlayerController item in players)
        {
            item.state = PlayerSate.idle;
            //技能CD减少1
            item.cd_Add(-1);

        }

        if (orderSect == mySect)
            UpdateSelect(players[0]);

        if (orderSect == mySect)
        {
            if (this.auto) AutoMyScet();
        }else
        {
            //开始敌人回合
            EnemyOrder();
        }
    }

    public List<PlayerController> GetPlayers(Sect sect)
    {
        List<PlayerController> i = new List<PlayerController>();
        foreach (var item in this.players)
        {
            if (item.sect == sect) i.Add(item);
        }
        return i;
    }

    public List<PlayerController> GetPlayersIdle(Sect sect)
    {
        List<PlayerController> i = new List<PlayerController>();
        foreach (var item in this.players)
        {
            if (item.sect == sect&& item.state== PlayerSate.idle) i.Add(item);
        }
        return i;
    }

    public void DoFuncByTime(System.Action func, float time)
    {
        this.StartCoroutine(C_WaitTimeToDo(func, time));
    }

    IEnumerator C_WaitTimeToDo(System.Action func, float startTime)
    {
        yield return new WaitForSeconds(startTime);

        func.Invoke();
    }

    void SetSpeed()
    {
        Time.timeScale = this.speed;
    }

    private void OnValidate()
    {
        SetSpeed();
    }

    private void OnDestroy()
    {
        GridMeshManager.Instance.SetNull();
        //单个移除，低效
        // EffectCtrl.instance.ReMove();
    
        //全部移除
        EventDispatcher.instance.ClearEventListener();
    }

}
