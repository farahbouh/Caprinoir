using System.Collections.Generic;

using UnityEngine;

using UnityEngine.Tilemaps;

using Unity.Netcode;




public class FarmCropGround : NetworkBehaviour

{

[Header("References")]

public Tilemap farmCropTilemap;

 

[Header("Toutes les Graines Disponibles")]

public GameObject[] cropPrefabs; 




private Dictionary<Vector3Int, GameObject>
plantedCrops = new Dictionary<Vector3Int,GameObject>();




void Update()

{

// On ne traite les touches que si on est le client local

if (!IsSpawned || NetworkManager.Singleton.LocalClient
== null) return; 

 

GameObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

Vector3Int cell = farmCropTilemap.WorldToCell(localPlayer.transform.position);




if (Input.GetKeyDown(KeyCode.P))

{

// 1. LE CLIENT regarde SON propre inventaire

int indexMain = InventaireGlobal.instance.indexSelectionne;

if (indexMain < InventaireGlobal.instance.objets.Count)

{

// 2. LE CLIENT récupère le nom de l'objet

string nomObjet = InventaireGlobal.instance.objets[indexMain];

 

// 3. LE CLIENT demande au serveur de planter CET objet précis

TryPlantServerRpc(cell, nomObjet);




// 4. LE CLIENT consomme l'objet dans son propre sac (Simulation locale)

InventaireGlobal.instance.ConsommerObjet(indexMain);

}

}

 

if (Input.GetKeyDown(KeyCode.G))
TryGrowServerRpc(cell);

if (Input.GetKeyDown(KeyCode.H))

{

// 1. On cherche la plante qui se trouve sur la case devant nous

Crop planteCible = null;

 

// Vu que plantedCrops est géré par le serveur, le client doit chercher dans sa scène locale

foreach (Crop c in FindObjectsOfType<Crop>())

{

if (farmCropTilemap.WorldToCell(c.transform.position)
== cell)

{

planteCible = c;

break;

}

}




// 2. Si on a trouvé une plante sur notre case, et qu'elle est prête à être récoltée

if (planteCible != null &&
planteCible.IsReady())

{

// 3. LA MAGIE EST ICI : Le client ajoute l'objet dans SON propre inventaire !

InventaireGlobal.instance.AjouterObjet(planteCible.nomDuFruit,
planteCible.iconeInventaire);




// 4. Le client demande au serveur de faire disparaître la plante pour tous les joueurs

TryHarvestServerRpc(cell);

}

}

}




// On ajoute le nom de l'objet en paramètre de la requête !

[Rpc(SendTo.Server, InvokePermission
= RpcInvokePermission.Everyone)]

void TryPlantServerRpc(Vector3Int cell,
string nomObjetEnMain, RpcParams rpcParams
= default)

{

// Vérifications de base (Côté Serveur)

if (farmCropTilemap == null
|| plantedCrops.ContainsKey(cell)
|| !farmCropTilemap.HasTile(cell))
return;




// Trouver le prefab

GameObject prefabAPlanter = null;

foreach (GameObject p in cropPrefabs)

{

if (p.name == nomObjetEnMain)

{

prefabAPlanter = p;

break;

}

}




// Instancier et Spawner sur le réseau

if (prefabAPlanter != null)

{

Vector3 spawnWorldPos = farmCropTilemap.GetCellCenterWorld(cell);

GameObject cropGO = Instantiate(prefabAPlanter,
spawnWorldPos, Quaternion.identity);

 

// SÉCURITÉ : On vérifie que le composant existe avant de l'appeler !

NetworkObject netObj = cropGO.GetComponent<NetworkObject>();

if (netObj != null)

{

netObj.Spawn(); 

}

else

{

Debug.LogError("CRITIQUE : Le prefab " +
prefabAPlanter.name + " n'a pas de NetworkObject !");

}

 

cropGO.name = $"Crop_{cell.x}_{cell.y}";

plantedCrops.Add(cell, cropGO);

}

}




[Rpc(SendTo.Server, InvokePermission
= RpcInvokePermission.Everyone)]

void TryGrowServerRpc(Vector3Int cell)

{

if (plantedCrops.TryGetValue(cell,
out GameObject cropGO) &&
cropGO != null)

{

Crop crop = cropGO.GetComponent<Crop>();

if (crop != null) crop.Grow();

}

}




[Rpc(SendTo.Server, InvokePermission
= RpcInvokePermission.Everyone)]

void TryHarvestServerRpc(Vector3Int cell)

{

if (plantedCrops.TryGetValue(cell,
out GameObject cropGO) &&
cropGO != null)

{

Crop crop = cropGO.GetComponent<Crop>();

if (crop == null ||
!crop.IsReady()) return;




cropGO.GetComponent<NetworkObject>().Despawn();

plantedCrops.Remove(cell);

}

}

}

