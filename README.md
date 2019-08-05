# Unity Drawing
A script for drawing on a Texture2D in Unity.
Drawing is done on the CPU.

<img src="README_resources/hello.gif?raw=true" style="width: 40%;">

Features
-----
* Adjustable color.
* Adjustable thickness.
* Draw straight lines.
* Draw Bezier curves.

Usage
---------
* Add `DrawingTool.cs` to your Unity project.
* Call `DrawLine(Vector2 start, Vector2 end)` with start and end UV positions to draw. Note `start` of call `i` should be equal to `end` of call `i-1` to draw a continuous smooth path.
* Call `DrawStraightLine(Vector2 start, Vector2 end)` to draw a straight line.    
Note: You will need to call `currentDrawing.Apply()` manually to copy the texture to the GPU.
* Call `DrawBezierCurve(Vector2 start, Vector2 end, Vector2 control)` to draw a bezier curve.  
Note: You will need to call `currentDrawing.Apply()` manually to copy the texture to the GPU.
* See `DrawingToolExample.cs` for an example of how to call the tool.
