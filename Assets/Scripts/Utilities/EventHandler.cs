using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Farm.Dialogue;
using Farm.Audio;
using Farm.Inventory;

public static class EventHandler
{
    public static event Action<InventoryLocation, List<InventoryItem>> UpdateInventoryUIEvent;      
    public static void CallUpdateInventoryUIEvent(InventoryLocation location, List<InventoryItem> list)      
    {   
        //Debug.Log("CallUpdateInventoryUI");
        UpdateInventoryUIEvent?.Invoke(location, list);      
    }

    public static event Action<int, Vector3> InstantiateItemInSceneEvent;

    public static void CallInstantiateItemInSceneEvent(int id, Vector3 pos)
    {
        //Debug.Log("CallInstantiateItemInScene");
        InstantiateItemInSceneEvent?.Invoke(id, pos);
    }

    public static event Action<int, Vector3, ItemType> DropItemEvent;

    public static void CallDropItemEvent(int id, Vector3 pos, ItemType itemType)
    {
        //Debug.Log("CallDropItemEvent");
        DropItemEvent?.Invoke(id, pos, itemType);
    }

    public static event Action<ItemDetails, bool> ItemSelectedEvent;

    public static void CallItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        // Debug.Log("CallItemSelectedEvent");
        ItemSelectedEvent?.Invoke(itemDetails, isSelected);
    }
    
    public static event Action<int, int, int, Season> GameMinuteEvent;

    public static void CallGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        // Debug.Log("CallGameMinuteEvent");
        GameMinuteEvent?.Invoke(minute, hour, day, season);
    }

    public static event Action<int, Season> GameDayEvent;

    public static void CallGameDayEvent(int day, Season season)
    {
        Debug.Log("CallGameDayEvent");
        GameDayEvent?.Invoke(day, season);
    }

    public static event Action<int, int, int, int, Season> GameDateEvent;

    public static void CallGameDateEvent(int hour, int day, int month, int year, Season season)
    {
        // Debug.Log("CallGameDateEvent");
        GameDateEvent?.Invoke(hour, day, month, year, season);
    }

    public static event Action<string, Vector3> TransitionEvent;

    public static void CallTransitionEvent(string sceneName, Vector3 pos)
    {
        TransitionEvent?.Invoke(sceneName, pos);
    }

    public static event Action BeforeSceneUnloadEvent;
    public static void CallBeforeSceneUnloadEvent()
    {
        Debug.Log("CallBeforeSceneUnloadEvent");
        BeforeSceneUnloadEvent?.Invoke();
    }

    public static event Action AfterSceneLoadedEvent;
    public static void CallAfterSceneLoadedEvent()
    {
        Debug.Log("CallAfterSceneLoadedEvent");
        AfterSceneLoadedEvent?.Invoke();
    }

    public static event Action<Vector3> MoveToPositionEvent;
    public static void CallMoveToPositionEvent(Vector3 targetPosition)
    {
        MoveToPositionEvent?.Invoke(targetPosition);
    }

    public static event Action<Vector3, ItemDetails> MouseClickedEvent;

    public static void CallMouseClickedEvent(Vector3 pos, ItemDetails itemDetails)
    {
        MouseClickedEvent?.Invoke(pos, itemDetails);
    }

    public static event Action<Vector3, ItemDetails> ExecuteActionAfterAnimationEvent;
    
    public static void CallExecuteActionAfterAnimationEvent(Vector3 pos, ItemDetails itemDetails)
    {
        ExecuteActionAfterAnimationEvent?.Invoke(pos, itemDetails);
    }

    public static event Action<int, TileDetails> PlantSeedEvent;
    
    public static void CallPlantSeedEvent(int ID, TileDetails tile)
    {
        //Debug.Log("CallPlantSeedEvent");
        PlantSeedEvent?.Invoke(ID, tile);
    }

    public static event Action<int> HarvestAtPlayerPositionEvent;

    public static void CallHarvestAtPlayerPositionEvent(int ID)
    {
        //Debug.Log("CallHarvestAtPlayerPositionEvent");
        HarvestAtPlayerPositionEvent?.Invoke(ID);
    }

    public static event Action RefreshCurrentMapEvent;

    public static void CallRefreshCurrentMapEvent()
    {
        //Debug.Log("CallRefreshCurrentMapEvent");
        RefreshCurrentMapEvent?.Invoke();
    }

    public static event Action<ParticalEffectType, Vector3> ParticalEffectEvent;

    public static void CallParticalEffectEvent(ParticalEffectType effectType, Vector3 pos)
    {
        ParticalEffectEvent?.Invoke(effectType, pos);
    }

    public static event Action GenerateCropEvent;

    public static void CallGenerateCropEvent()
    {
        GenerateCropEvent?.Invoke();
    }

    public static event Action<DialoguePiece> ShowDialogueEvent;

    public static void CallShowDialogueEvent(DialoguePiece piece)
    {
        ShowDialogueEvent?.Invoke(piece);
    }

    public static event Action<SlotType, InventoryBag_SO> BaseBagOpenEvent;

    public static void CallBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bag)
    {
        BaseBagOpenEvent?.Invoke(slotType, bag);
    }

    public static event Action<SlotType, InventoryBag_SO> BaseBagCloseEvent;

    public static void CallBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bag)
    {
        BaseBagCloseEvent?.Invoke(slotType, bag);
    }

    public static event Action<GameState> UpdateGameStateEvent;

    public static void CallUpdateGameStateEvent(GameState gameState)
    {
        UpdateGameStateEvent?.Invoke(gameState);
    }

    public static event Action<ItemDetails, bool> ShowTradeUIEvent;

    public static void CallShowTradeUIEvent(ItemDetails item, bool isSell)
    {
        ShowTradeUIEvent?.Invoke(item, isSell);
    }

    public static event Action<int, Vector3> BuildFurnitureEvent;

    public static void CallBuildFurnitureEvent(int ID, Vector3 pos)
    {
        BuildFurnitureEvent?.Invoke(ID, pos);
    }

    public static event Action<Season, LightShift, float> LightShiftChangeEvent;

    public static void CallLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        LightShiftChangeEvent?.Invoke(season, lightShift, timeDifference);
    }

    public static event Action<SoundDetails> InitSoundEffectEvent;

    public static void CallInitSoundEffectEvent(SoundDetails soundDetails)
    {
        InitSoundEffectEvent?.Invoke(soundDetails);
    }

    public static event Action<SoundName> PlaySoundEvent;

    public static void CallPlaySoundEvent(SoundName soundName)
    {
        PlaySoundEvent?.Invoke(soundName);
    }

    public static event Action<int> StartNewGameEvent;

    public static void CallStartNewGameEvent(int index)
    {
        StartNewGameEvent?.Invoke(index);
    }

    public static Action EndGameEvent;
    public static void CallEndGameEvent()
    {
        EndGameEvent?.Invoke();
    }

}
