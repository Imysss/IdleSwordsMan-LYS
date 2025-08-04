using System;
using Data;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class UIPartySlotItem : UIBase
{
    enum GameObjects
    {
        Indicator,
    }

    enum Images
    {
        PartyImage,
        PartyGradeImage,
    }

    private int index;
    private PartyState _state;
    private int _partyId;
    private Action _partySwapAction;
    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));
        
        GetObject((int)GameObjects.Indicator).SetActive(false);

        if (TryGetComponent(out Button button))
        {
            button.onClick.AddListener(OnSlotButtonClick);
        }
        
        
        return true;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PartySwapStartEvent>(OnPartySwapStartHandler);
        EventBus.Subscribe<PartySwapEndEvent>(OnPartySwapEndHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<PartySwapStartEvent>(OnPartySwapStartHandler);
        EventBus.UnSubscribe<PartySwapEndEvent>(OnPartySwapEndHandler);
    }

    public void SetInfo(int index, int dataId)
    {
        this.index = index;
        _partyId = dataId;
        _state = Managers.Inventory.PartyStates[_partyId];
        
        SetImageSize(120, 120);

        RefreshUI();
    }

    public void SetInfo(Define.SlotType type)
    {
        _partyId = -1;
        
        SetImageSize(64, 81);

        RefreshUI(type);
    }

    private void RefreshUI()
    {
        Sprite spr = Managers.Resource.Load<Sprite>(_partyId + ".sprite");
        GetImage((int)Images.PartyImage).sprite = spr;
        GetImage((int)Images.PartyGradeImage).gameObject.SetActive(true);
        switch (Managers.Data.PartyDataDic[_partyId].rarity)
        {
            case Define.RarityType.Normal:
                GetImage((int)Images.PartyGradeImage).color = Define.UIColors.Normal;
                break;
            case Define.RarityType.Rare:
                GetImage((int)Images.PartyGradeImage).color = Define.UIColors.Rare;
                break;
            case Define.RarityType.Epic:
                GetImage((int)Images.PartyGradeImage).color = Define.UIColors.Epic;
                break;
            case Define.RarityType.Unique:
                GetImage((int)Images.PartyGradeImage).color = Define.UIColors.Unique;
                break;
            case Define.RarityType.Legendary:
                GetImage((int)Images.PartyGradeImage).color = Define.UIColors.Legendary;
                break;
        }
    }

    private void RefreshUI(Define.SlotType type)
    {
        if (type == Define.SlotType.Empty)
        {
            Sprite empty = Managers.Resource.Load<Sprite>("emptyIcon.sprite"); 
            GetImage((int)Images.PartyImage).sprite = empty;
            GetImage((int)Images.PartyGradeImage).gameObject.SetActive(false);
        }
        else
        {
            Sprite locked = Managers.Resource.Load<Sprite>("lockIcon1.sprite");
            GetImage((int)Images.PartyImage).sprite = locked;
            GetImage((int)Images.PartyGradeImage).gameObject.SetActive(false);
        }
    }
    
    private void SetImageSize(float width, float height)
    {
        RectTransform rect = GetImage((int)Images.PartyImage).GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
    }

    private void OnSlotButtonClick()
    {
        
    }

    #region Event Handlers

    private void OnPartySwapStartHandler(PartySwapStartEvent evnt)
    {
        if (_partyId == -1) return;
        
        GetObject((int)GameObjects.Indicator).SetActive(true);

        _partySwapAction = () =>
        {
            Managers.Equipment.PartyEquipment.ReplaceEquip(_state, evnt.Data);
            EventBus.Raise(new PartySwapEndEvent(evnt.Data));
            EventBus.Raise(new PartyChangedEvent());
        };
        gameObject.BindEvent(_partySwapAction);
    }

    private void OnPartySwapEndHandler(PartySwapEndEvent evnt)
    {
        GetObject((int)GameObjects.Indicator).SetActive(false);
        gameObject.UnbindEvent(_partySwapAction);

    }

    #endregion
}
