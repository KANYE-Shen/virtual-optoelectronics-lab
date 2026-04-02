using UnityEngine;

public class ChangeChildrenMaterials : MonoBehaviour
{
    [Header("需要替换成的材质")]
    public Material targetMaterial;

    [Header("是否包含自身")]
    public bool includeSelf = false;

    [ContextMenu("替换子物体材质")]
    public void ChangeMaterials()
    {
        if (targetMaterial == null)
        {
            Debug.LogWarning("请先在 Inspector 中指定 targetMaterial!");
            return;
        }

        // 获取所有子物体的 Renderer（true表示包括未激活的子物体）
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in renderers)
        {
            if (!includeSelf && r.gameObject == this.gameObject)
                continue;

            // 只修改第一个材质，如果有多个材质槽你也可以扩展
            r.material = targetMaterial;

            // 如果要同时覆盖多个材质槽，可以用下面这段
            /*
            Material[] mats = new Material[r.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = targetMaterial;
            }
            r.materials = mats;
            */
        }

        Debug.Log("子物体材质替换完成！");
    }
}
