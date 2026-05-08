using UnityEngine;

public abstract class GameMode
{
    public abstract bool IsBossFloor(int level);
    public abstract bool IsFinalFloor(int level);
    public abstract string GetModeName();
}

public class NormalMode : GameMode
{
    public override bool IsBossFloor(int level)
    {
        return level == 5;
    }

    public override bool IsFinalFloor(int level)
    {
        return level == 5;
    }

    public override string GetModeName()
    {
        return "Normal";
    }
}

public class EndlessMode : GameMode
{
    public override bool IsBossFloor(int level)
    {
        return level % 5 == 0;
    }

    public override bool IsFinalFloor(int level)
    {
        return false;
    }

    public override string GetModeName()
    {
        return "Infinito";
    }
}

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }
    public static GameMode CurrentMode { get; private set; }
    public static int CurrentSlot { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentMode = CreateNormalMode();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void SetMode(GameMode mode)
    {
        CurrentMode = mode;
    }

    public static void SetSlot(int slot)
    {
        CurrentSlot = slot;
    }

    public static GameMode CreateNormalMode()
    {
        return new NormalMode();
    }

    public static GameMode CreateEndlessMode()
    {
        return new EndlessMode();
    }
}