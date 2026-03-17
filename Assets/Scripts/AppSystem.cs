using Arena.Combat;
using Arena.Dungeon;
using Arena.Enemies;
using Arena.Items;
using Arena.Loot;
using Arena.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Arena.Tavern;
using Arena.Requests;

public class AppSystem : MonoBehaviour
{
    static public AppSystem Instance;

    public GoogleSheetDownloader DataDownloader;
    public GameObject GameView;
    public GameObject TitleView;
    public GameObject ContinueGameView;
    public GameObject NewCharacterView;
    public TMP_InputField NameField;
    public Button CreatePlayerButton;
    public GameObject TitleViewLoadingText;

    private void Awake()
    {
        Instance = this;
        TitleViewLoadingText.SafeSetActive(true);
        ContinueGameView.SafeSetActive(false);
        NewCharacterView.SafeSetActive(false);
        GameView.SafeSetActive(false);
        TitleView.SafeSetActive(true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        WebGLInput.mobileKeyboardSupport = true;
        WebGLInput.captureAllKeyboardInput = false;
        TouchScreenKeyboard.hideInput = false;

        // Wait for data to be downloaded
        yield return new WaitUntil(() => DataDownloader.HasData);

        // Add data to all systems
        ItemSystem.Instance.SetData(DataDownloader.Data);
        EnemySystem.Instance.SetData(DataDownloader.Data);
        SkillSystem.Instance.SetData(DataDownloader.Data);
        DungeonSystem.Instance.SetData(DataDownloader.Data);
        LootSystem.Instance.SetData(DataDownloader.Data);
        TavernSystem.Instance.SetData(DataDownloader.Data);
        RequestSystem.Instance.SetData(DataDownloader.Data);

        // Init systems (creates references as needed, such as the loot tables referencing items)
        PlayerSystem.Instance.Init();
        ItemSystem.Instance.Init();
        EnemySystem.Instance.Init();
        SkillSystem.Instance.Init();
        DungeonSystem.Instance.Init();
        LootSystem.Instance.Init();
        TavernSystem.Instance.Init();
        RequestSystem.Instance.Init();

        // Finalize systems
        // Start game
        TitleViewLoadingText.SafeSetActive(false);

        if (PlayerSystem.Instance.Player != null)
        {
            ContinueGameView.SafeSetActive(true);
            NewCharacterView.SafeSetActive(false);
        }
        else
        {
            ContinueGameView.SafeSetActive(false);
            NewCharacterView.SafeSetActive(true);
            CreatePlayerButton.interactable = false;
        }
    }

    public void OnTextUpdated()
    {
        CreatePlayerButton.interactable = !string.IsNullOrEmpty(NameField.text);
    }

    public void SelectContinueGameButton()
    {
        TitleView.SafeSetActive(false);
        GameView.SafeSetActive(true);
        GameEvents.EnterTown();
    }

    public void SelectCreatePlayerButton()
    {
        PlayerSystem.Instance.CreatePlayer(NameField.text);
        TitleView.SafeSetActive(false);
        GameView.SafeSetActive(true);
        GameEvents.EnterTown();
    }

    public void SelectResetGameButton()
    {
        ContinueGameView.SafeSetActive(false);
        NewCharacterView.SafeSetActive(true);
        CreatePlayerButton.interactable = false;
    }
}
