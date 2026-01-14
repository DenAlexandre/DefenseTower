using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DefenseTower;

public class Enemy
{
    public int Level { get; }
    public int MaxHp { get; }
    public int Hp { get; private set; }
    public float Speed { get; }
    public int Reward { get; }
    public int PathIndex { get; private set; }
    public float X { get; private set; }
    public float Y { get; private set; }
    public bool HasTree { get; private set; }
    public bool ReturnPath { get; private set; }
    public bool Alive { get; private set; } = true;

    private readonly List<PointF> path;

    public Enemy(int level, PointF spawnPoint, List<PointF> gamePath)
    {
        Level = level;
        MaxHp = 80 + (level - 1) * 35;
        Hp = MaxHp;
        Speed = 1.2f + level * 0.1f;
        Reward = 15 + level * 5;
        PathIndex = 0;
        X = spawnPoint.X;
        Y = spawnPoint.Y;
        path = gamePath;
    }

    public bool Update()
    {
        if (!Alive)
            return false;

        var currentPath = ReturnPath ? path.AsEnumerable().Reverse().ToList() : path;

        if (PathIndex < currentPath.Count)
        {
            var target = currentPath[PathIndex];
            float dx = target.X - X;
            float dy = target.Y - Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            if (dist < Speed)
            {
                X = target.X;
                Y = target.Y;
                PathIndex++;
            }
            else
            {
                X += (dx / dist) * Speed;
                Y += (dy / dist) * Speed;
            }
        }
        else
        {
            if (!ReturnPath)
            {
                HasTree = true;
                ReturnPath = true;
                PathIndex = 0;
            }
            else
            {
                Alive = false;
                return true; // A atteint la fin
            }
        }
        return false;
    }

    public bool TakeDamage(int damage)
    {
        Hp = Math.Max(0, Hp - damage);
        if (Hp <= 0)
        {
            Alive = false;
            return true;
        }
        return false;
    }

    public void Draw(ICanvas canvas)
    {
        if (!Alive)
            return;

        // Corps
        Color bodyColor = HasTree ? Color.FromRgb(220, 20, 20) : Color.FromRgb(138, 43, 226);
        canvas.FillColor = bodyColor;
        canvas.FillCircle(X, Y, 12);
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(X, Y, 12);

        // Barre de vie
        float barWidth = 32;
        float barHeight = 6;
        float hpRatio = (float)Hp / MaxHp;
        float barX = X - barWidth / 2;
        float barY = Y - 25;
        float progressBar = barWidth * hpRatio;

        // Fond de la barre
        canvas.FillColor = Color.FromRgb(80, 0, 0);
        canvas.FillRectangle(barX, barY, barWidth, barHeight);

        // Barre de vie colorée
        Color hpColor;
        if (hpRatio > 0.5f)
            hpColor = Color.FromRgb(0, 200, 0);
        else if (hpRatio > 0.25f)
            hpColor = Color.FromRgb(255, 165, 0);
        else
            hpColor = Colors.Red;

        canvas.FillColor = hpColor;
        canvas.FillRectangle(barX, barY, progressBar, barHeight);
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 1;
        canvas.DrawRectangle(barX, barY, barWidth, barHeight);

        // Pourcentage
        int hpPercent = (int)(hpRatio * 100);
        canvas.FontColor = Colors.White;
        canvas.FontSize = 14;
        canvas.DrawString($"{hpPercent}%", X, barY - 6, HorizontalAlignment.Center);
    }
}