using Microsoft.Maui.Graphics;
using System;

namespace DefenseTower;

public class Projectile
{
    public float X { get; private set; }
    public float Y { get; private set; }
    public Enemy Target { get; }
    public int Damage { get; }
    public float Speed { get; }
    public bool Hit { get; private set; }

    public Projectile(float x, float y, Enemy target, int damage, float speed)
    {
        X = x;
        Y = y;
        Target = target;
        Damage = damage;
        Speed = speed;
        Hit = false;
    }

    public void Update()
    {
        if (Target == null || !Target.Alive)
        {
            Hit = true;
            return;
        }

        float dx = Target.X - X;
        float dy = Target.Y - Y;
        float dist = MathF.Sqrt(dx * dx + dy * dy);

        // Collision
        if (Distance(X, Y, Target.X, Target.Y) < 14)
        {
            Target.TakeDamage(Damage);
            Hit = true;
            return;
        }

        X += (dx / dist) * Speed;
        Y += (dy / dist) * Speed;
    }

    public void Draw(ICanvas canvas)
    {
        canvas.FillColor = Colors.Yellow;
        canvas.FillCircle(X, Y, 5);
        canvas.StrokeColor = Color.FromRgb(255, 200, 0);
        canvas.StrokeSize = 2;
        canvas.DrawCircle(X, Y, 5);
    }

    private static float Distance(float x1, float y1, float x2, float y2)
    {
        return MathF.Sqrt(MathF.Pow(x2 - x1, 2) + MathF.Pow(y2 - y1, 2));
    }
}