using UnityEngine;

public class TorusSplitter : MonoBehaviour
{
    [Header("Split Settings")]
    [Tooltip("Drag your complete ProBuilder Torus here (after running 'Strip ProBuilder Scripts')")]
    public GameObject torusObject;

    [ContextMenu("Split Torus into 7 Pieces")]
    public void SplitTorusIntoSeven()
    {
        if (torusObject == null)
        {
            Debug.LogError("You must drag a Torus object into the slot!");
            return;
        }

        MeshFilter originalMeshFilter = torusObject.GetComponent<MeshFilter>();
        MeshRenderer originalMeshRenderer = torusObject.GetComponent<MeshRenderer>();

        if (originalMeshFilter == null || originalMeshRenderer == null || originalMeshFilter.sharedMesh == null)
        {
            Debug.LogError("The object must have a MeshFilter with a valid Mesh and a MeshRenderer!");
            return;
        }

        // 1. Clean up old attempts automatically
        Transform parentTransform = torusObject.transform.parent;
        Transform oldRoot = parentTransform != null ? parentTransform.Find("TorusBits7_Root") : null;
        if (oldRoot != null) DestroyImmediate(oldRoot.gameObject);
        else
        {
            GameObject globalOldRoot = GameObject.Find("TorusBits7_Root");
            if (globalOldRoot != null) DestroyImmediate(globalOldRoot);
        }

        // 2. Create the new clean parent
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

        // 3. Split the mesh into 7 actual wedge pieces based on angles
        for (int i = 0; i < chunksCount; i++)
        {
            float startAngle = i * angleStep;
            float endAngle = (i + 1) * angleStep;

            // Create lists for this specific piece's mesh data
            System.Collections.Generic.List<Vector3> newVertices = new System.Collections.Generic.List<Vector3>();
            System.Collections.Generic.List<int> newTriangles = new System.Collections.Generic.List<int>();
            System.Collections.Generic.List<Vector2> newUvs = new System.Collections.Generic.List<Vector2>();
            System.Collections.Generic.List<Vector3> newNormals = new System.Collections.Generic.List<Vector3>();
            System.Collections.Generic.Dictionary<int, int> vertMap = new System.Collections.Generic.Dictionary<int, int>();

            // Loop through all triangles in the original mesh
            for (int t = 0; t < triangles.Length; t += 3)
            {
                int i0 = triangles[t];
                int i1 = triangles[t + 1];
                int i2 = triangles[t + 2];

                // Calculate the triangle's center in local space
                Vector3 center = (vertices[i0] + vertices[i1] + vertices[i2]) / 3f;

                // Calculate the angle around the Y-axis (0 to 360 degrees)
                float angle = Mathf.Atan2(center.x, center.z) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360f;

                // If the triangle's center is within this slice's angle range, save it!
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

            // Create a new GameObject for this specific piece
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

                // Create a perfect, convex physics collider based strictly on this piece's shape
                MeshCollider meshCol = bit.AddComponent<MeshCollider>();
                meshCol.sharedMesh = chunkMesh;
                meshCol.convex = true;
            }
        }

        // Hide the original torus
        torusObject.SetActive(false);
        Debug.Log("The torus has been split into 7 unique wedge pieces! Remember to assign 'TorusBits7_Root' to your Target script.");
    }
}
