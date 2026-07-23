using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private GameObject clearPanel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            clearPanel.SetActive(true);

            // ÉQÅ[ÉÄÇí‚é~
            Time.timeScale = 0f;
        }
    }
}