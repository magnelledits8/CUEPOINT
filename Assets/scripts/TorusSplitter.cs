using UnityEngine;

public class TorusSplitter : MonoBehaviour
{
    [Header("Inställningar för uppdelning")]
    [Tooltip("Dra in din hela ProBuilder Torus här (efter att du kört Strip ProBuilder Scripts)")]
    public GameObject torusObject;

    [ContextMenu("Dela upp Torusen i 7 bitar")]
    public void SplitTorusIntoSeven()
    {
        if (torusObject == null)
        {
            Debug.LogError("Du måste dra in ett Torus-objekt i fältet!");
            return;
        }

        MeshFilter originalMeshFilter = torusObject.GetComponent<MeshFilter>();
        MeshRenderer originalMeshRenderer = torusObject.GetComponent<MeshRenderer>();

        if (originalMeshFilter == null || originalMeshRenderer == null || originalMeshFilter.sharedMesh == null)
        {
            Debug.LogError("Objektet måste ha ett MeshFilter med en giltig Mesh och en MeshRenderer!");
            return;
        }

        // 1. Städa bort gamla försök automatisk
        Transform parentTransform = torusObject.transform.parent;
        Transform oldRoot = parentTransform != null ? parentTransform.Find("TorusBits7_Root") : null;
        if (oldRoot != null) DestroyImmediate(oldRoot.gameObject);
        else
        {
            GameObject globalOldRoot = GameObject.Find("TorusBits7_Root");
            if (globalOldRoot != null) DestroyImmediate(globalOldRoot);
        }

        // 2. Skapa den nya rena föräldern
        GameObject root = new GameObject("TorusBits7_Root");
        if (parentTransform != null) root.transform.SetParent(parentTransform, false);
        root.transform.position = torusObject.transform.position;
        root.transform.rotation = torusObject.transform.rotation;

        Mesh sourceMesh = originalMeshFilter.sharedMesh;
        Vector3[] vertices = sourceMesh.vertices;
        int[] triangles = sourceMesh.triangles;
        Vector2[] uvs = sourceMesh.uv;
        Vector3[] normals = sourceMesh.normals;

        int chunksCount = 7;
        float angleStep = 360f / chunksCount;

        // 3. Dela upp meshen i 7 faktiska tårtbitar baserat på vinklar
        for (int i = 0; i < chunksCount; i++)
        {
            float startAngle = i * angleStep;
            float endAngle = (i + 1) * angleStep;

            // Skapa listor för den här specifika bitens mesh-data
            System.Collections.Generic.List<Vector3> newVertices = new System.Collections.Generic.List<Vector3>();
            System.Collections.Generic.List<int> newTriangles = new System.Collections.Generic.List<int>();
            System.Collections.Generic.List<Vector2> newUvs = new System.Collections.Generic.List<Vector2>();
            System.Collections.Generic.List<Vector3> newNormals = new System.Collections.Generic.List<Vector3>();
            System.Collections.Generic.Dictionary<int, int> vertMap = new System.Collections.Generic.Dictionary<int, int>();

            // Loopa igenom alla trianglar i originalet
            for (int t = 0; t < triangles.Length; t += 3)
            {
                int i0 = triangles[t];
                int i1 = triangles[t + 1];
                int i2 = triangles[t + 2];

                // Beräkna triangelns mittpunkt i lokal rymd
                Vector3 center = (vertices[i0] + vertices[i1] + vertices[i2]) / 3f;

                // Räkna ut vinkeln runt Y-axeln (0 till 360 grader)
                float angle = Mathf.Atan2(center.x, center.z) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360f;

                // Om triangelns mittpunkt ligger inom den här bitens tårtbit, spara den!
                if (angle >= startAngle && angle < endAngle)
                {
                    int[] currentTriangleIndices = new int[3] { i0, i1, i2 };
                    for (int v = 0; v < 3; v++)
                    {
                        int origIndex = currentTriangleIndices[v];
                        if (!vertMap.ContainsKey(origIndex))
                        {
                            vertMap[origIndex] = newVertices.Count;
                            newVertices.Add(vertices[origIndex]);
                            if (uvs != null && uvs.Length > origIndex) newUvs.Add(uvs[origIndex]);
                            if (normals != null && normals.Length > origIndex) newNormals.Add(normals[origIndex]);
                        }
                        newTriangles.Add(vertMap[origIndex]);
                    }
                }
            }

            // Skapa ett nytt GameObject för den här specifika biten
            if (newTriangles.Count > 0)
            {
                GameObject bit = new GameObject($"TorusChunk_7_{i + 1}");
                bit.transform.SetParent(root.transform, false);

                MeshFilter mf = bit.AddComponent<MeshFilter>();
                MeshRenderer mr = bit.AddComponent<MeshRenderer>();
                mr.sharedMaterials = originalMeshRenderer.sharedMaterials;

                Mesh chunkMesh = new Mesh();
                chunkMesh.name = $"TorusChunkMesh_{i + 1}";
                chunkMesh.vertices = newVertices.ToArray();
                chunkMesh.triangles = newTriangles.ToArray();
                if (newUvs.Count > 0) chunkMesh.uv = newUvs.ToArray();
                if (newNormals.Count > 0) chunkMesh.normals = newNormals.ToArray();
                else chunkMesh.RecalculateNormals();
                chunkMesh.RecalculateBounds();

                mf.sharedMesh = chunkMesh;

                // Skapa en perfekt, konvex fysik-collider baserad på BARA den här bitens form
                MeshCollider meshCol = bit.AddComponent<MeshCollider>();
                meshCol.sharedMesh = chunkMesh;
                meshCol.convex = true;
            }
        }

        // Dölj originaltorusen
        torusObject.SetActive(false);
        Debug.Log("Torusen har klippts upp i 7 unika tårtbitar! Kom ihåg att tilldela 'TorusBits7_Root' till ditt Target-skript.");
    }
}
