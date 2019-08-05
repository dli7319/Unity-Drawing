using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using DateTime = System.DateTime;

public class DrawingTool : MonoBehaviour
{
    public Texture2D currentDrawing;
    public Color drawingColor = new Color(0.231f, 0.659f, 0.525f);

    private NativeArray<Color32> currentDrawingArr;
    private Vector2 previousPoint;
    private bool triggerDown = false;
    private static int[] resolution = { 4096, 2048 };
    private static Color32[] emptyColors;
    private int thickness = 8;

    private Vector2 prevPoint;
    private Vector2 prevPrevPoint;
    private Vector2 prevMidpoint;

    // Start is called before the first frame update
    void Start()
    {
        OnDestroy();
        currentDrawing = new Texture2D(resolution[0], resolution[1], TextureFormat.RGBA32, false);
        currentDrawing.filterMode = FilterMode.Point;
        currentDrawingArr = currentDrawing.GetRawTextureData<Color32>();
        ClearCurrentDrawing();
        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", currentDrawing);
    }

    private void OnDestroy()
    {
        if (currentDrawing != null)
        {
            Destroy(currentDrawing);
        }
        currentDrawing = null;
        currentDrawingArr = default;
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.C))
        // {
        //     ClearCurrentDrawing();
        // }
    }

    public void ClearCurrentDrawing()
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        if (emptyColors == null)
        {
            emptyColors = new Color32[currentDrawingArr.Length];
            for (int i = 0; i < currentDrawingArr.Length; i++)
            {
                emptyColors[i] = new Color32(0xff, 0xff, 0xff, 0);
            }
        }
        currentDrawingArr.CopyFrom(emptyColors);
        currentDrawing.Apply();
        Debug.Log($"time to fill empty texture {stopwatch.Elapsed.TotalMilliseconds}");
    }

    /// <summary>
    /// Draws a line using Xiaolin Wu's algorithm.
    /// https://en.wikipedia.org/wiki/Xiaolin_Wu%27s_line_algorithm
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void DrawLine(Vector2 start, Vector2 end)
    {
        Debug.Log("Draw line called");
        if (Vector2.Distance(start, prevPoint) < 0.01f)
        {
            Vector2 midPoint = Vector2.Lerp(start, end, 0.5f);
            DrawBezierCurve(prevMidpoint, midPoint, prevPoint);
            prevMidpoint = midPoint;
        }
        else
        {
            Vector2 midPoint = Vector2.Lerp(start, end, 0.5f);
            DrawBezierCurve(start, midPoint, prevPoint);
            prevMidpoint = midPoint;
        }

        prevPrevPoint = prevPoint;
        prevPoint = end;
        currentDrawing.Apply();
    }

    /// <summary>
    /// Draws a plain bezier curve without anti-aliasing.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="control"></param>
    public void DrawBezierCurve(Vector2 start, Vector2 end, Vector2 control)
    {
        start.x = resolution[0] * start.x;
        start.y = resolution[1] * start.y;
        end.x = resolution[0] * end.x;
        end.y = resolution[1] * end.y;
        control.x = resolution[0] * control.x;
        control.y = resolution[1] * control.y;

        int segments = Mathf.FloorToInt(Vector2.Distance(start, end));
        float stepSize = 1f / segments;
        for (float t = 0; t < 1; t += stepSize)
        {
            Vector2 drawPosition = Vector2.Lerp(Vector2.Lerp(start, control, t), Vector2.Lerp(control, end, t), t);
            DrawDot(drawPosition, 1f);
        }
    }

    public void DrawDot(Vector2 position, float opacity)
    {
        if (position.x < 0 || position.x >= resolution[0] || position.y < 0 || position.y >= resolution[1])
        {
            return;
        }
        if (opacity > 1.1)
        {
            for (int x = Mathf.RoundToInt(position.x - thickness / 2); x < position.x + thickness / 2; x++)
            {
                for (int y = Mathf.RoundToInt(position.y - thickness / 2); y < position.y + thickness / 2; y++)
                {
                    if (x >= 0 && x < resolution[0] && y >= 0 && y < resolution[1])
                    {
                        int index = y * resolution[0] + x;
                        Color newColor = Color.red;
                        currentDrawingArr[index] = newColor;
                    }
                }
            }
        }
        else
        {
            for (int x = Mathf.RoundToInt(position.x - thickness / 2); x < position.x + thickness / 2; x++)
            {
                for (int y = Mathf.RoundToInt(position.y - thickness / 2); y < position.y + thickness / 2; y++)
                {
                    if (x >= 0 && x < resolution[0] && y >= 0 && y < resolution[1])
                    {
                        int index = y * resolution[0] + x;
                        Color newColor = drawingColor;
                        // Might be faster to store the alphas in a float[] array instead
                        float currentAlpha = currentDrawingArr[index].a;
                        newColor.a = Mathf.Lerp(currentAlpha, 1, opacity);
                        currentDrawingArr[index] = newColor;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draws a line using Xiaolin Wu's algorithm.
    /// https://en.wikipedia.org/wiki/Xiaolin_Wu%27s_line_algorithm
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void DrawStraightLine(Vector2 start, Vector2 end)
    {
        float ipart(float x) => Mathf.Floor(x);
        float fpart(float x) => x - Mathf.Floor(x);
        float rfpart(float x) => 1f - fpart(x);

        start.x = resolution[0] * start.x;
        start.y = resolution[1] * start.y;
        end.x = resolution[0] * end.x;
        end.y = resolution[1] * end.y;

        bool steep = Mathf.Abs(end.y - start.y) > Mathf.Abs(end.x - start.x);

        if (steep)
        {
            float temp = start.x;
            start.x = start.y;
            start.y = temp;
            temp = end.x;
            end.x = end.y;
            end.y = temp;
        }
        if (start.x > end.x)
        {
            Vector2 temp = start;
            start = end;
            end = temp;
        }

        float dx = end.x - start.x;
        float dy = end.y - start.y;
        float gradient = dx == 0 ? 0f : (dy / dx);

        // handle first endpoint
        float xend = Mathf.Round(start.x);
        float yend = start.y + gradient * (xend - start.x);
        float xgap = rfpart(start.x + 0.5f);
        float xpxl1 = xend;
        float ypxl1 = ipart(yend);
        if (steep)
        {
            DrawPoint((int)ypxl1, (int)xpxl1, rfpart(yend) * xgap);
            DrawPoint((int)(ypxl1 + 1), (int)xpxl1, fpart(yend) * xgap);
        }
        else
        {
            DrawPoint((int)xpxl1, (int)ypxl1, rfpart(yend) * xgap);
            DrawPoint((int)xpxl1, (int)(ypxl1 + 1), fpart(yend) * xgap);
        }
        float intery = yend + gradient;

        // handle second endpoint
        xend = Mathf.Round(end.x);
        yend = end.y + gradient * (xend - end.x);
        xgap = fpart(end.x + 0.5f);
        float xpxl2 = xend;
        float ypxl2 = ipart(yend);
        if (steep)
        {
            DrawPoint((int)ypxl2, (int)xpxl2, rfpart(yend) * xgap);
            DrawPoint((int)(ypxl2 + 1), (int)xpxl2, fpart(yend) * xgap);
        }
        else
        {
            DrawPoint((int)xpxl2, (int)ypxl2, rfpart(yend) * xgap);
            DrawPoint((int)xpxl2, (int)(ypxl2 + 1), fpart(yend) * xgap);
        }

        if (steep)
        {
            for (float x = xpxl1 + 1; x <= xpxl2 - 1; x++)
            {
                DrawPoint((int)ipart(intery) - thickness / 2, (int)x, rfpart(intery));
                for (int i = -thickness / 2 + 1; i < thickness / 2; i++)
                {
                    DrawPoint((int)ipart(intery) + i, (int)x, 1);
                }
                DrawPoint((int)ipart(intery) + thickness / 2, (int)x, fpart(intery));
                intery = intery + gradient;
            }
        }
        else
        {
            for (float x = xpxl1 + 1; x <= xpxl2 - 1; x++)
            {
                DrawPoint((int)x, (int)ipart(intery) - thickness / 2, rfpart(intery));
                for (int i = -thickness / 2 + 1; i < thickness / 2; i++)
                {
                    DrawPoint((int)x, (int)ipart(intery) + i, 2);
                }
                DrawPoint((int)x, (int)ipart(intery) + thickness / 2, fpart(intery));
                intery = intery + gradient;
            }
        }

        currentDrawing.Apply();
    }

    private void DrawPoint(int x, int y, float c)
    {
        if (x < 0 || x >= resolution[0] || y < 0 || y >= resolution[1])
        {
            return;
        }
        if (c > 1f)
        {
            int index = y * resolution[0] + x;
            Color newColor = Color.red;
            currentDrawingArr[index] = newColor;
        }
        else
        {
            c = Mathf.Clamp(c, 0, 1);
            int index = y * resolution[0] + x;
            float currentAlpha = currentDrawingArr[index].a;
            Color newColor = drawingColor;
            newColor.a = Mathf.Lerp(currentAlpha, 1, c);
            currentDrawingArr[index] = newColor;
        }
    }
}
