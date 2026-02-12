using UnityEngine;

namespace VRPhysicsFramework.Framework.Player
{
    public class PlayerSpawner : MonoBehaviour
    {
        public GameObject Player;

        public void Awake()
        {
            player();
        }

        public void player()
        {
            GameObject player = Instantiate(Player, transform.position, Quaternion.identity);
            player.GetComponent<PhysicsRig>().origin.transform.rotation = transform.rotation;
        }
    }
}