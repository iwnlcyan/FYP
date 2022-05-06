using PathologicalGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class UICtrl : MonoBehaviour
{
    private Canvas canvas;
    private CanvasScaler _canvasScaler;
    private CanvasRenderer hudCanvas;
    private SpawnPool spawnPool;
    private bool _updateHpImage;
    public GameObject actionPanel;
    private GameObject skillSelectTarget;
    private GameObject skillConfirm;
    [HideInInspector]
    public SkillButton[] skill_slot = new SkillButton[4];

    [HideInInspector]
    public Button auto_AI;

    public static UICtrl instance { get; private set; }
    private void Awake()
    {
        instance = this;
        EventDispatcher.instance.Regist<int, Vector3>(GameEventType.showHudDamage, this.showHudDamage);

        canvas = this.GetComponent<Canvas>();
        _canvasScaler = this.GetComponent<CanvasScaler>();

        this.hudCanvas = this.transform.Find("hudCanvas").GetComponent<CanvasRenderer>();
        //创建对象池 伤害字体

        var i = ResourcesExt.Load<GameObject>("ui/hudItem");

        //var poolGo = new GameObject("HudText Pool");

        this.spawnPool = hudCanvas.gameObject.AddComponent<SpawnPool>();

        var prefabPool = new PrefabPool(i.transform);


        this.spawnPool.CreatePrefabPool(prefabPool);


        //绿色字体
        i = ResourcesExt.Load<GameObject>("ui/hudText_green");
        prefabPool = new PrefabPool(i.transform);
        this.spawnPool.CreatePrefabPool(prefabPool);

       
    }


    // Start is called before the first frame update
    void Start()
    {


        actionPanel = this.transform.Find("ActionPanel").gameObject;
        var waitBtn = actionPanel.transform.Find("wait").GetComponent<Button>();
        waitBtn.onClick.AddListener(this.onWaitBtnClick);
        actionPanel.SetActive(false);


        for (int i = 0; i < 3; i++)
        {
            var btn = new SkillButton();
            btn.button = actionPanel.transform.Find("skill_slot" + i).GetComponent<Button>();
            btn.cdImage = btn.button.transform.Find("cd_Image").GetComponent<Image>();
            btn.cdText = btn.cdImage.transform.Find("cdText").GetComponent<Text>();
            skill_slot[i] = btn;
        }

        //skill_slot[0] = actionPanel.transform.Find("skill_slot0").GetComponent<Button>();
        // skill_slot[1] = actionPanel.transform.Find("skill_slot1").GetComponent<Button>();
        // skill_slot[2] = actionPanel.transform.Find("skill_slot2").GetComponent<Button>();

        //订阅技能按钮
        for (int i = 0; i < 3; i++)
        {
            //闭包
            var tempIdx = i;
            // var d = actionPanel.transform.Find("skill_slot"+i).GetComponent<Button>();
            skill_slot[i].button.onClick.AddListener(() => { skill_slotClick(tempIdx); });

        }


        this.skillSelectTarget = this.transform.Find("skillSelectTarget").gameObject;

        this.skillConfirm = this.transform.Find("skillConfirm").gameObject;


        this.skillConfirm.transform.Find("confirm").GetComponent<Button>().onClick.AddListener(this.confirmClick);


        this.skillConfirm.transform.Find("cancel").GetComponent<Button>().onClick.AddListener(cancel_SkillConfirm);

        this.skillSelectTarget.transform.Find("cancel").GetComponent<Button>().onClick.AddListener(cancel_skillSelectTarget);

        this.auto_AI = transform.Find("auto_AI").GetComponent<Button>();
        auto_AI_Text = this.auto_AI.GetComponentInChildren<Text>();
        auto_AI.onClick.AddListener(onClick_auto_AI);


        this.settingBtn = transform.Find("setting").GetComponent<Button>();

        settingBtn.onClick.AddListener(OnSettingBtnClick);
    }

    private void OnSettingBtnClick()
    {
        //  throw new NotImplementedException();
       var go= ResourcesExt.Load<GameObject>("uiPanel/settingPanel");

       var rgo= MonoBehaviour.Instantiate(go);
        rgo.transform.SetParent(this.transform,false);


    }

    void onClick_auto_AI()
    {
        GameCtrl.instance.ExchangeMyAutoAi();
        EventDispatcher.instance.DispatchEvent(GameEventType.playButtonUiSound);
        if (!GameCtrl.instance.auto)
        {
            auto_AI_Text.text = "AI托管";
        }else
        {
            auto_AI_Text.text = "关闭托管";
        }
    }

    private void cancel_skillSelectTarget()
    {
        GameCtrl.instance.cancel_skillSelectTarget();
        EventDispatcher.instance.DispatchEvent(GameEventType.playButtonUiSound);
    }

    private void cancel_SkillConfirm()
    {
        GameCtrl.instance.cancel_SkillConfirm();
        EventDispatcher.instance.DispatchEvent(GameEventType.playButtonUiSound);
    }

    private void confirmClick()
    {
        //throw new NotImplementedException();

        GameCtrl.instance.confirmClick();

        skillConfirm.SetActive(false);

        EventDispatcher.instance.DispatchEvent(GameEventType.playButtonUiSound);
    }

    private void skill_slotClick(int id)
    {
        GameCtrl.instance.UseSkill(id);
    }

    internal void ShowRestoreHealth(int hp, PlayerController player)
    {
        //throw new NotImplementedException();

        var hudItem = spawnPool.Spawn("hudText_green", this.hudCanvas.transform);
        var screenPos = getScreenPos(Camera.main, player.transform.position + Vector3.up * 3);
        hudItem.position = screenPos;
        //设置最后渲染
        hudItem.transform.SetAsLastSibling();

        StartCoroutine(FloatUI(hudItem.gameObject));

        var text = hudItem.Find("Text").GetComponent<Text>();
        text.text = "+" + hp.ToString();
        spawnPool.Despawn(hudItem, 1.3f);
    }

    private void showHudDamage(int damage, Vector3 worldPos)
    {
        //throw new NotImplementedException();

        var hudItem = spawnPool.Spawn("hudItem", this.hudCanvas.transform);

        var screenPos = getScreenPos(Camera.main, worldPos);
        hudItem.position = screenPos;


        StartCoroutine(FloatUI(hudItem.gameObject));

        var text = hudItem.Find("Text").GetComponent<Text>();
        text.text = damage.ToString();
        spawnPool.Despawn(hudItem, 1.3f);


    }

    IEnumerator FloatUI(GameObject go)
    {
        //1.2秒 升高180 米
        var duration = 1.2f;
        var startTime = Time.time;

        var startPos = go.transform.position;
        var y_offset = 180;
        float t1 = 0;
        while (t1 < 1)
        {
            t1 = (Time.time - startTime) / duration;

            if (t1 >= 1f) t1 = 1;

            yield return new WaitForEndOfFrame();

            var y = Mathf.Lerp(0, y_offset, t1);

            go.transform.position = startPos + new Vector3(0, y, 0);
        }
    }


    //// 将精灵的世界坐标转换成屏幕坐标
    private Vector3 getScreenPos(Camera cam, Vector3 worldPos)
    {
        // throw new NotImplementedException();
        var resolutionX = this._canvasScaler.referenceResolution.x;
        var resolutionY = this._canvasScaler.referenceResolution.y;
        var offset = (Screen.width / this._canvasScaler.referenceResolution.x) * (1 - this._canvasScaler.matchWidthOrHeight) + (Screen.height / this._canvasScaler.referenceResolution.y) * this._canvasScaler.matchWidthOrHeight;
        var screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        return new Vector3(screenPos.x, screenPos.y, 0);

    }



    private void onWaitBtnClick()
    {
        // throw new NotImplementedException();
     
        if (GameCtrl.instance.Wait())
        {
            //actionPanel.SetActive(false);
        }

    }



    // Update is called once per frame
    void Update()
    {
        

    }

    private void LateUpdate()
    {
        //每帧更新血条位置
        if (_updateHpImage)
        {
            foreach (var player in GameCtrl.instance.players)
            {

                var screenPos = getScreenPos(Camera.main, player.transform.position + new Vector3(0, 4, 0));
                player.hpImageTrs.position = screenPos;


            }
        }
    }



    public void Init_HpImage()
    {
        //初始化血条预制体，利用对象池优化
        var i = ResourcesExt.Load<GameObject>("ui/hpImage_green");
        var prefabPool = new PrefabPool(i.transform);
        this.spawnPool.CreatePrefabPool(prefabPool);

        i = ResourcesExt.Load<GameObject>("ui/hpImage_red");
        prefabPool = new PrefabPool(i.transform);
        this.spawnPool.CreatePrefabPool(prefabPool);


        foreach (var player in GameCtrl.instance.players)
        {
            Transform hpImageTrs = null;
            if (player.sect == GameCtrl.instance.mySect)
                hpImageTrs = spawnPool.Spawn("hpImage_green", this.hudCanvas.transform);
            else
                hpImageTrs = spawnPool.Spawn("hpImage_red", this.hudCanvas.transform);

            var screenPos = getScreenPos(Camera.main, player.transform.position + new Vector3(0, 3, 0));
            hpImageTrs.position = screenPos;
            player.hpImageTrs = hpImageTrs;
            player.hpImage = hpImageTrs.Find("hp_front").GetComponent<Image>();

            player.viewHp = (int)player.attribute.hp;
            UpdateHp(player);
        }

        _updateHpImage = true;
    }

    internal void showSkillSelectTarget(bool show)
    {
        //throw new NotImplementedException();

        skillSelectTarget.SetActive(show);

        this.actionPanel.SetActive(!show);

    }

    internal void showSkillConfirm(bool show)
    {
        //throw new NotImplementedException();
        skillConfirm.SetActive(show);
        skillSelectTarget.SetActive(!show);

    }

    public void UpdateHp(PlayerController player)
    {
        // throw new NotImplementedException();
        player.hpImage.fillAmount = (float)player.viewHp / player.attribute.maxHp;
    }

    
    GameObject fastAttackImageGob;
    private Animator orderTip;
    private Text auto_AI_Text;
    private Button settingBtn;

    internal void ShowFastAttack(PlayerController playerController)
    {
        //throw new NotImplementedException();
        if (fastAttackImageGob == null)
        {
             var i = ResourcesExt.Load<GameObject>("ui/fastAttackImage");

            fastAttackImageGob = MonoBehaviour.Instantiate(i);

            fastAttackImageGob.transform.SetParent(this.hudCanvas.transform, false);
        }

      


        fastAttackImageGob.SetActive(false);
        fastAttackImageGob.SetActive(true);


        var screenPos = getScreenPos(Camera.main, playerController.transform.position + Vector3.up * 5);
        fastAttackImageGob.transform.position = screenPos;

    }

    internal void ShowOrderTip(string animationName)
    {
        //throw new NotImplementedException();
        if (orderTip == null) orderTip = this.transform.Find("orderTip").GetComponent<Animator>();

        orderTip.gameObject.SetActive(true);

        orderTip.Play(animationName);
    }
}
