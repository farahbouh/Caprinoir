using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class FishingSystem : NetworkBehaviour
{
    [Header("Animation")]
    public Animator playerAnimator;
    
    [Header("Poissons")]
    public GameObject fishPrefab;
    public Transform spawnPoint;
    public float fishLifetime = 5f;
    public Fish[] availableFishes;
    private Fish currentFish;
    
    [Header("Score")]
    public NetworkVariable<int> totalScore = new NetworkVariable<int>();
    public Text scoreText;
    private int localScore = 0;  // Score local pour affichage immédiat
    
    [Header("Pêche - Barre automatique")]
    public float barMoveSpeed = 60f;
    
    [Header("UI")]
    public RectTransform movileRect;
    public RectTransform fondRect;
    public RectTransform targetZoneRect;
    public GameObject fishingPanel;
    public Text statusText;
    
    [Header("Effets Splash")]
    public GameObject splashParent;
    public float frameDelay = 0.06f;
    public float splashDuration = 0.5f;
    
    [Header("Zone de réussite")]
    [Range(5, 30)] public float zoneSizePercent = 15f;
    public bool randomizeZonePosition = true;
    [Range(0, 100)] public float zonePositionPercent = 50f;
    
    private float fondMinY = -50f;
    private float fondMaxY = 50f;
    private float zoneMinY = 0f;
    private float zoneMaxY = 0f;
    private float currentZonePosition = 50f;
    
    private bool playerInZone = false;
    private bool isFishing = false;
    private GameObject player;
    private float currentBarValue = 50f;
    private bool hasCaught = false;
    private bool waitingForInput = true;
    private int barDirection = 1;
    
    private bool wasMovementEnabled = true;
    private bool wasMovementCaveEnabled = true;
    private MonoBehaviour movementScript;
    private MonoBehaviour movementCaveScript;
    private Rigidbody2D playerRb;
    
    private Vector2 originalMovilePosition;
    private Coroutine currentSplashCoroutine;
    private List<GameObject> splashTiles = new List<GameObject>();
    
    void Awake()
    {
        FindAnimator();
        // Initialisation correcte du NetworkVariable
        totalScore = new NetworkVariable<int>(0, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server);
    }
    
    void FindAnimator()
    {
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
            if (playerAnimator == null)
                playerAnimator = GetComponentInChildren<Animator>();
            if (playerAnimator == null && transform.parent != null)
                playerAnimator = transform.parent.GetComponent<Animator>();
        }
    }
    
    void Start()
    {
        CalculateFondLimits();
        
        if (movileRect != null)
            originalMovilePosition = movileRect.anchoredPosition;
        
        if (fishingPanel != null)
            fishingPanel.SetActive(false);
        
        UpdateMovilePosition();
        FindAllSplashTiles();
        
        if (randomizeZonePosition)
            RandomizeZonePosition();
        else
            UpdateZonePosition(zonePositionPercent);
        
        // Initialisation du score local et affichage
        localScore = totalScore.Value;
        UpdateScoreDisplay();
        
        totalScore.OnValueChanged += OnScoreChanged;
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        FindAnimator();
    }
    
    void OnDestroy()
    {
        if (totalScore != null)
            totalScore.OnValueChanged -= OnScoreChanged;
    }
    
    void OnScoreChanged(int oldValue, int newValue)
    {
        localScore = newValue;
        UpdateScoreDisplay();
    }
    
    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score : " + localScore.ToString();
        }
    }
    
    void FindAllSplashTiles()
    {
        splashTiles.Clear();
        if (splashParent != null)
        {
            foreach (Transform child in splashParent.transform)
            {
                splashTiles.Add(child.gameObject);
                child.gameObject.SetActive(false);
            }
            
            splashTiles.Sort((a, b) => 
            {
                int numA = ExtractNumber(a.name);
                int numB = ExtractNumber(b.name);
                return numA.CompareTo(numB);
            });
        }
    }
    
    int ExtractNumber(string name)
    {
        int start = name.IndexOf('(') + 1;
        int end = name.IndexOf(')');
        if (start > 0 && end > start)
        {
            string number = name.Substring(start, end - start);
            int result;
            if (int.TryParse(number, out result))
                return result;
        }
        return 0;
    }
    
    void DisableAllSplashTiles()
    {
        foreach (GameObject tile in splashTiles)
            if (tile != null) tile.SetActive(false);
    }
    
    void CalculateFondLimits()
    {
        if (fondRect == null) return;
        float centerY = fondRect.anchoredPosition.y;
        float height = fondRect.sizeDelta.y;
        fondMinY = centerY - (height / 2f);
        fondMaxY = centerY + (height / 2f);
    }
    
    void RandomizeZonePosition()
    {
        float fondHeight = fondMaxY - fondMinY;
        float zoneHeight = fondHeight * zoneSizePercent / 100f;
        
        float minPossibleCenter = fondMinY + (zoneHeight / 2f);
        float maxPossibleCenter = fondMaxY - (zoneHeight / 2f);
        
        float randomCenterY = Random.Range(minPossibleCenter, maxPossibleCenter);
        currentZonePosition = (randomCenterY - fondMinY) / fondHeight * 100f;
        
        UpdateZonePosition(currentZonePosition);
    }
    
    void UpdateZonePosition(float positionPercent)
    {
        if (targetZoneRect == null) return;
        
        float fondHeight = fondMaxY - fondMinY;
        float zoneHeight = fondHeight * zoneSizePercent / 100f;
        float zoneCenterY = fondMinY + (fondHeight * positionPercent / 100f);
        
        float minCenter = fondMinY + (zoneHeight / 2f);
        float maxCenter = fondMaxY - (zoneHeight / 2f);
        zoneCenterY = Mathf.Clamp(zoneCenterY, minCenter, maxCenter);
        
        zoneMinY = zoneCenterY - (zoneHeight / 2f);
        zoneMaxY = zoneCenterY + (zoneHeight / 2f);
        
        targetZoneRect.anchoredPosition = new Vector2(targetZoneRect.anchoredPosition.x, zoneCenterY);
        targetZoneRect.sizeDelta = new Vector2(targetZoneRect.sizeDelta.x, zoneHeight);
    }
    
    void UpdateMovilePosition()
    {
        if (movileRect == null) return;
        float t = currentBarValue / 100f;
        float newY = Mathf.Lerp(fondMinY, fondMaxY, t);
        Vector2 pos = movileRect.anchoredPosition;
        pos.y = newY;
        movileRect.anchoredPosition = pos;
    }
    
    void Update()
    {
        if (IsOwner)
        {
            if (playerInZone && Input.GetKeyDown(KeyCode.F))
            {
                if (!isFishing)
                    StartFishing();
                else
                {
                    StopFishing(false);
                    PlaySplashAnimation();
                }
            }
            
            if (isFishing && !hasCaught && waitingForInput && Input.GetKeyDown(KeyCode.Space))
            {
                AttemptCatch();
            }
        }
        
        if (isFishing && !hasCaught && waitingForInput)
        {
            currentBarValue += barMoveSpeed * Time.deltaTime * barDirection;
            
            if (currentBarValue >= 100) { currentBarValue = 100; barDirection = -1; }
            else if (currentBarValue <= 0) { currentBarValue = 0; barDirection = 1; }
            
            UpdateMovilePosition();
        }
    }
    
    void PlaySplashAnimation()
    {
        if (splashTiles.Count == 0) return;
        if (currentSplashCoroutine != null) { StopCoroutine(currentSplashCoroutine); DisableAllSplashTiles(); }
        currentSplashCoroutine = StartCoroutine(AnimateSplash());
    }
    
    IEnumerator AnimateSplash()
    {
        for (int i = 0; i < splashTiles.Count; i++)
        {
            if (splashTiles[i] != null)
            {
                splashTiles[i].SetActive(true);
                yield return new WaitForSeconds(frameDelay);
            }
        }
        yield return new WaitForSeconds(splashDuration);
        DisableAllSplashTiles();
        currentSplashCoroutine = null;
    }
    
    void AttemptCatch()
    {
        waitingForInput = false;
        float barY = movileRect.anchoredPosition.y;
        bool success = (barY >= zoneMinY && barY <= zoneMaxY);
        
        PlaySplashAnimation();
        
        if (success)
        {
            CatchFish();
            StopFishing(true);
        }
        else
        {
            StopFishing(false);
            StartCoroutine(ClearStatusText());
        }
    }
    
    IEnumerator ClearStatusText()
    {
        yield return new WaitForSeconds(1.5f);
        if (statusText != null && !isFishing) statusText.text = "";
    }
    
    void StartFishing()
    {
        isFishing = true;
        hasCaught = false;
        waitingForInput = true;
        currentBarValue = 50f;
        barDirection = Random.Range(0, 2) == 0 ? 1 : -1;
        
        UpdateMovilePosition();
        
        if (randomizeZonePosition) RandomizeZonePosition();
        if (fishingPanel != null) fishingPanel.SetActive(true);
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Fishing");
        }
        
        DisableMovement();
        PlaySplashAnimation();
    }
    
    void StopFishing(bool success)
    {
        if (!isFishing) return;
        
        isFishing = false;
        
        if (movileRect != null)
            movileRect.anchoredPosition = originalMovilePosition;
        
        if (fishingPanel != null)
            fishingPanel.SetActive(false);
        
        EnableMovement();
        
        if (playerAnimator != null)
        {
            playerAnimator.ResetTrigger("Fishing");
            playerAnimator.Play("Idle", 0, 0);
        }
        
        if (!success)
            StartCoroutine(ClearStatusText());
    }
    
    void CatchRandomFish()
    {
        if (availableFishes.Length > 0)
        {
            int randomIndex = Random.Range(0, availableFishes.Length);
            currentFish = availableFishes[randomIndex];
        }
    }
    
    void CatchFish()
    {
        hasCaught = true;
        CatchRandomFish();
        
        if (currentFish != null)
        {
            int pointsGagnes = currentFish.points;
            
            // Met à jour le score localement IMMÉDIATEMENT
            localScore += pointsGagnes;
            UpdateScoreDisplay();
            
            // Envoie la mise à jour au serveur
            ModifierScoreServerRpc(pointsGagnes);
            
            if (statusText != null)
                statusText.text = currentFish.fishName + " attrapé ! +" + pointsGagnes;
            
            if (InventaireGlobal.instance != null)
                InventaireGlobal.instance.AjouterObjet(currentFish.fishName, currentFish.fishSprite);
        }
        
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.right * 2f;
        if (fishPrefab != null)
        {
            GameObject fish = Instantiate(fishPrefab, pos, Quaternion.identity);
            Destroy(fish, fishLifetime);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void ModifierScoreServerRpc(int points)
    {
        totalScore.Value += points;
    }
    
    void DisableMovement()
    {
        if (player == null) return;
        movementScript = player.GetComponent("Movement") as MonoBehaviour;
        movementCaveScript = player.GetComponent("Movement_Cave") as MonoBehaviour;
        playerRb = player.GetComponent<Rigidbody2D>();
        
        if (movementScript != null) { wasMovementEnabled = movementScript.enabled; movementScript.enabled = false; }
        if (movementCaveScript != null) { wasMovementCaveEnabled = movementCaveScript.enabled; movementCaveScript.enabled = false; }
        if (playerRb != null) { playerRb.velocity = Vector2.zero; playerRb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation; }
    }
    
    void EnableMovement()
    {
        if (player == null) return;
        if (movementScript != null) movementScript.enabled = wasMovementEnabled;
        if (movementCaveScript != null) movementCaveScript.enabled = wasMovementCaveEnabled;
        if (playerRb != null) playerRb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkObject netObj = other.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                playerInZone = true;
                player = other.gameObject;
                
                playerAnimator = player.GetComponent<Animator>();
                if(playerAnimator == null) playerAnimator = player.GetComponentInChildren<Animator>();
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("ZonePeche"))
        {
            NetworkObject netObj = other.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                playerInZone = false;
                if (isFishing) StopFishing(false);
                player = null;
            }
        }
    }
}