using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

public class MobileMoveButtonEditModeTests
{
    private GameObject playerObject;
    private PlayerController player;
    private GameObject buttonObject;
    private MobileMoveButton button;
    private EventSystem eventSystem;

    [SetUp]
    public void SetUp()
    {
        GameManager.instance = null;
        playerObject = new GameObject("Player");
        playerObject.AddComponent<Rigidbody2D>();
        playerObject.AddComponent<BoxCollider2D>();
        playerObject.AddComponent<AudioSource>();
        player = playerObject.AddComponent<PlayerController>();

        buttonObject = new GameObject("MoveButton");
        button = buttonObject.AddComponent<MobileMoveButton>();
        button.player = player;
        button.direction = -1f;

        eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
    }

    [TearDown]
    public void TearDown()
    {
        GameManager.instance = null;
        Object.DestroyImmediate(buttonObject);
        Object.DestroyImmediate(playerObject);
        Object.DestroyImmediate(eventSystem.gameObject);
    }

    [Test]
    public void OnPointerDown_SetsPlayerMobileInputToDirection()
    {
        button.OnPointerDown(new PointerEventData(eventSystem));

        Assert.AreEqual(-1f, player.MobileHorizontalInputForTest, 0.0001f);
    }

    [Test]
    public void OnPointerUp_ResetsPlayerMobileInputToZero()
    {
        button.OnPointerDown(new PointerEventData(eventSystem));

        button.OnPointerUp(new PointerEventData(eventSystem));

        Assert.AreEqual(0f, player.MobileHorizontalInputForTest, 0.0001f);
    }
}
