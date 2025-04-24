using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6

}

public class ChessPiece : MonoBehaviour
{
    public int team;
    public int currentX;
    public int currentY;
    public ChessPieceType type;

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;

    private void Start()
    {
        transform.rotation = Quaternion.Euler((team == 0) ? Vector3.zero : new Vector3(0, 180, 0));
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public void PlayMaterializeEffect(float duration = 1.0f)
    {
        StartCoroutine(Materialize(duration));
    }

    private IEnumerator<WaitForEndOfFrame> Materialize(float duration)
    {
        // get the renderer, and list of materials
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        List<Material> materials = new List<Material>();

        // for all the renderers and materials, assign material variables
        foreach (var rend in renderers)
        {
            foreach (var mat in rend.materials)
            {
                // Ensure material has alpha support
                Color color = mat.color;
                color.a = 0;
                mat.color = color;
                materials.Add(mat);
            }
        }

        // over time, change the transparency alpha of the material for a duration
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            foreach (Material mat in materials)
            {
                Color color = mat.color;
                color.a = Mathf.Lerp(0, 1, t);
                mat.color = color;
            }

            elapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // make the transparency alpha 1 after the duration effect
        foreach (Material mat in materials)
        {
            Color color = mat.color;
            color.a = 1;
            mat.color = color;
        }
    }




    // teleport unique animation
    public void PlayTeleportRise(Vector3 targetPosition, float riseDistance = 1.0f, float duration = 0.5f)
    {
        StartCoroutine(RiseFromBelow(targetPosition, riseDistance, duration));
    }

    private IEnumerator<WaitForEndOfFrame> RiseFromBelow(Vector3 targetPos, float riseDist, float duration)
    {
        Vector3 startPos = targetPos - new Vector3(0, riseDist, 0);
        float elapsed = 0f;

        transform.position = startPos;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        transform.position = targetPos;
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        r.Add(new Vector2Int(3, 3));
        r.Add(new Vector2Int(3, 4));
        r.Add(new Vector2Int(4, 3));
        r.Add(new Vector2Int(4, 4));

        return r;
    }

    // Returns true if the tile can be moved to (empty or enemy), false if blocked by same team
    protected bool IsTileAvailable(ref ChessPiece[,] board, int x, int y)
    {
        return board[x, y] != null && board[x, y].team != team;
    }

    public virtual SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        return SpecialMove.None;
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
        {
            transform.position = desiredPosition;
        }
    }
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = desiredScale;
        }
    }

}