using UnityEngine;

public sealed class SaveableTransform : MonoBehaviour
{
    [SerializeField] private string saveId;

    public string SaveId
    {
        get
        {
            if (string.IsNullOrWhiteSpace(saveId))
            {
                saveId = gameObject.name;
            }

            return saveId;
        }
    }

    public void SetSaveId(string id)
    {
        saveId = id;
    }
}
