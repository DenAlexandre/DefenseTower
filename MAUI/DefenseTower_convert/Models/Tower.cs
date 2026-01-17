using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DefenseTower;

public class Tower
{
    public float X { get; }
    public float Y { get; }
    public string Type { get; }
    public int Level { get; private set; } = 1;
    public int Damage { get; private set; }
    public int FireRate { get; private set; }
    public float Range { get; private set; }
    public float Speed { get; private set; }
    public Color Color { get; }
    public int UpgradeCost { get; private set; } = 50;

    private int cooldown = 0;
    private Enemy? target;
    private readonly List<Projectile> projectiles = new();

    public Tower(float x, float y, string towerType)
    {
        X = x;
        Y = y;
        Type = towerType;

        switch (towerType)
        {
            case "basic":
                Damage = 12;
                FireRate = 35;
                Range = 140;
                Speed = 8;
                Color = Colors.Blue;
                break;
            case "sniper":
                Damage = 80;
                FireRate = 80;
                Range = 320;
                Speed = 12;
                Color = Colors.Red;
                break;
            case "rapid":
                Damage = 7;
                FireRate = 12;
                Range = 110;
                Speed = 10;
                Color = Colors.Yellow;
                break;
        }
    }

    public int Upgrade()
    {
        Level++;
        Damage = (int)(Damage * 1.4f);
        Range = (int)(Range * 1.1f);
        UpgradeCost = (int)(UpgradeCost * 1.6f);
        return UpgradeCost;
    }

    public void Update(List<Enemy> enemies)
    {
        cooldown = Math.Max(0, cooldown - 1);

        if (cooldown == 0)
        {
            Enemy? closest = null;
            float closestDist = 9999;

            foreach (var enemy in enemies)
            {
                if (!enemy.Alive)
                    continue;

                float dist = Distance(enemy.X, enemy.Y, X, Y);
                if (dist <= Range && dist < closestDist)
                {
                    closest = enemy;
                    closestDist = dist;
                }
            }

            if (closest != null)
            {
                target = closest;
                Shoot();
            }
        }

        for (int i = projectiles.Count - 1; i >= 0; i--)
        {
            projectiles[i].Update();
            if (projectiles[i].Hit)
            {
                projectiles.RemoveAt(i);
            }
        }
    }

    private void Shoot()
    {
        if (target != null && projectiles.Count < 3)
        {
            projectiles.Add(new Projectile(X, Y, target, Damage, Speed));
            cooldown = FireRate;
        }
    }

    public void Draw(ICanvas canvas)
    {
        // Zone de portée
        canvas.FillColor = Color.WithAlpha(0.12f);
        canvas.FillCircle(X, Y, Range);

        // Tour
        canvas.FillColor = Color;
        canvas.FillCircle(X, Y, 17);
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(X, Y, 17);

        // Niveau
        canvas.FontColor = Colors.White;
        canvas.FontSize = 16;
        canvas.DrawString(Level.ToString(), X, Y, HorizontalAlignment.Center);

        // Projectiles
        foreach (var proj in projectiles)
        {
            proj.Draw(canvas);
        }
    }

    private static float Distance(float x1, float y1, float x2, float y2)
    {
        return MathF.Sqrt(MathF.Pow(x2 - x1, 2) + MathF.Pow(y2 - y1, 2));
    }
}