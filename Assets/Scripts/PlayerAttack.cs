using UnityEngine;
using Unity.Netcode;

public class PlayerAttack : NetworkBehaviour
{
    public Transform attackPoint; // Un objet vide devant le perso
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int attackDamage = 1;

    void Update() {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Space)) { // Touche Espace pour attaquer
            Attack();
        }
    }

    public int GetTotalDamage() 
{
    // On utilise la même formule que celle utilisée lors de l'attaque
    return attackDamage + (GetComponent<Health>().niveau.Value * 5); 
}
void Attack() {
        Debug.Log("Tentative d'attaque !");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach(Collider2D enemy in hitEnemies) {
            Health enemyHealth = enemy.GetComponentInParent<Health>();
            
            if (enemyHealth != null) {
                // On récupère les dégâts via ta formule
                // .Value est nécessaire parce qu'on a synchronisé le niveau
                int degatsJoueur = attackDamage + (GetComponent<Health>().niveau.Value * 3); 
                
                // On demande au serveur d'appliquer les dégâts
                // (Seul le serveur a le droit de modifier les PV en réseau)
                RequestDamageServerRpc(enemyHealth.NetworkObject.NetworkObjectId, degatsJoueur);
                
                Debug.Log("J'ai touché l'ennemi : " + enemy.name);
            }
        }
    }

    [ServerRpc]
    void RequestDamageServerRpc(ulong targetId, int damage) {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject target)) {
            target.GetComponent<Health>().TakeDamage(damage);
        }
    }

    void OnDrawGizmosSelected() {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

   /* void Attack() {
        Debug.Log("Tentative d'attaque !"); // pour voir si la touche Espace fonctionne
    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

    foreach(Collider2D enemy in hitEnemies) {
        // On cherche le script Health même si on a touché un enfant du parent
        Health enemyHealth = enemy.GetComponentInParent<Health>();
        
        if (enemyHealth != null) {
            int degatsJoueur = attackDamage + (GetComponent<Health>().niveau * 3); 
            enemyHealth.TakeDamage(degatsJoueur);
            Debug.Log("J'ai touché l'ennemi : " + enemy.name);
        }
    }
}

}*/