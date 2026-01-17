using Microsoft.Maui.Graphics;
using System;

namespace DefenseTower;

public class Tree
{
    public float X { get; }
    public float Y { get; }
    public int Level { get; private set; } = 1;
    public int Income { get; private set; } = 10;
    public int Timer { get; private set; } = 0;
    public int Interval { get; } = 360;
    public int UpgradeCost { get; private set; } = 50;
    public float Radius { get; } = 25;

    public Tree(float x, float y)
    {
        X = x;
        Y = y;
    }

    public int Update()
    {
        Timer++;
        if (Timer >= Interval)
        {
            Timer = 0;
            return Income * Level;
        }
        return 0;
    }

    public int Upgrade()
    {
        Level++;
        int cost = UpgradeCost * Level;
        Income = 10 + (Level - 1) * 5;
        return cost;
    }

    public void Draw(ICanvas canvas)
    {
        // Tronc
        canvas.FillColor = Color.FromRgb(139, 69, 19);
        canvas.FillRectangle(X - 5, Y, 10, 20);

        // Feuillage
        canvas.FillColor = Color.FromRgb(0, 100, 0);
        var path1 = new PathF();
        path1.MoveTo(X, Y - 20);
        path1.LineTo(X - 20, Y);
        path1.LineTo(X + 20, Y);
        path1.Close();
        canvas.FillPath(path1);

        var path2 = new PathF();
        path2.MoveTo(X, Y - 35);
        path2.LineTo(X - 15, Y - 15);
        path2.LineTo(X + 15, Y - 15);
        path2.Close();
        canvas.FillPath(path2);

        // Timer circulaire
        float progress = (float)Timer / Interval;
        canvas.FillColor = Color.FromRgb(50, 50, 50);
        canvas.FillCircle(X + 30, Y - 20, 18);

        if (progress > 0)
        {
            canvas.FillColor = Colors.Yellow;
            float startAngle = -90;
            float sweepAngle = 360 * progress;
            canvas.FillArc(X + 30 - 17, Y - 20 - 17, 34, 34, startAngle, sweepAngle, true);
        }

        canvas.StrokeColor = Colors.Yellow;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(X + 30, Y - 20, 18);

        // Symbole $
        canvas.FontColor = Colors.White;
        canvas.FontSize = 20;
        canvas.DrawString("$", X + 30, Y - 20, HorizontalAlignment.Center);

        // Niveau
        canvas.FontColor = Colors.Yellow;
        canvas.FontSize = 16;
        canvas.DrawString($"Lv{Level}", X, Y - 50, HorizontalAlignment.Center);
    }
}