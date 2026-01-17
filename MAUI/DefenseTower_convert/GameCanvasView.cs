using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DefenseTower;

public class GameCanvasView : GraphicsView
{
    private readonly GameDrawable gameDrawable;
    private IDispatcherTimer? gameTimer;

    public GameCanvasView()
    {
        gameDrawable = new GameDrawable();
        Drawable = gameDrawable;

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnCanvasTapped;
        GestureRecognizers.Add(tapGesture);
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler != null)
        {
            gameTimer = Dispatcher.CreateTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0); // 60 FPS
            gameTimer.Tick += (s, e) =>
            {
                gameDrawable.Update();
                Invalidate();
            };
            gameTimer.Start();
        }
    }

    private void OnCanvasTapped(object? sender, TappedEventArgs e)
    {
        var point = e.GetPosition(this);
        if (point.HasValue)
        {
            gameDrawable.HandleClick((float)point.Value.X, (float)point.Value.Y);
            Invalidate();
        }
    }
}

public class GameDrawable : IDrawable
{
    private const int Width = 1000;
    private const int Height = 700;
    private const int FPS = 60;

    private readonly List<PointF> path;
    private readonly List<Tower> towers;
    private readonly List<Enemy> enemies;
    private readonly List<Tree> trees;

    private int money = 120;
    private int lives = 10;
    private int level = 1;
    private int wave = 0;
    private int nextLevel = 3;
    private int waveInit = 5;
    private int enemiesToSpawn = 0;
    private int spawnTimer = 0;
    private int spawnInterval = 40;

    private string? selectedTowerType;
    private object? selectedObject;
    private GameState gameState = GameState.Stopped;

    public GameDrawable()
    {
        path = new List<PointF>
        {
            new(50, 350), new(150, 350), new(150, 200), new(300, 200), new(300, 500),
            new(500, 500), new(500, 150), new(700, 150), new(700, 400), new(850, 400),
            new(850, 350), new(950, 350)
        };

        towers = new List<Tower>();
        enemies = new List<Enemy>();
        trees = new List<Tree>
        {
            new Tree(900, 300),
            new Tree(900, 400)
        };
    }

    public void Update()
    {
        if (gameState != GameState.Playing)
            return;

        foreach (var tree in trees)
        {
            money += tree.Update();
        }

        if (enemies.Count == 0 && enemiesToSpawn == 0)
        {
            SpawnWave();
        }

        if (enemiesToSpawn > 0)
        {
            spawnTimer++;
            if (spawnTimer >= spawnInterval)
            {
                enemies.Add(new Enemy(level, path[0], path));
                enemiesToSpawn--;
                spawnTimer = 0;
            }
        }

        foreach (var tower in towers)
        {
            tower.Update(enemies);
        }

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var enemy = enemies[i];
            bool reachedEnd = enemy.Update();

            if (reachedEnd)
            {
                lives--;
                enemies.RemoveAt(i);
            }
            else if (!enemy.Alive)
            {
                if (!enemy.ReturnPath)
                {
                    money += enemy.Reward;
                }
                enemies.RemoveAt(i);
            }
        }

        if (lives <= 0)
        {
            gameState = GameState.Stopped;
        }
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromRgb(240, 248, 255);
        canvas.FillRectangle(0, 0, Width, Height);

        // Dessiner le chemin
        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 30;
        for (int i = 0; i < path.Count - 1; i++)
        {
            canvas.DrawLine(path[i], path[i + 1]);
        }

        // Dessiner les arbres
        foreach (var tree in trees)
        {
            tree.Draw(canvas);
        }

        // Dessiner les tours
        foreach (var tower in towers)
        {
            tower.Draw(canvas);
        }

        // Dessiner les ennemis
        foreach (var enemy in enemies)
        {
            enemy.Draw(canvas);
        }

        // Dessiner l'UI
        DrawUI(canvas);
    }

    private void DrawUI(ICanvas canvas)
    {
        canvas.FillColor = Color.FromRgb(0, 100, 0);
        canvas.FillRectangle(0, 0, Width, 100);

        canvas.FontSize = 24;
        canvas.FontColor = Colors.Yellow;
        canvas.DrawString($"Argent: ${money}", 10, 15, HorizontalAlignment.Left);

        canvas.FontColor = Colors.Red;
        canvas.DrawString($"Vies: {lives}", 10, 40, HorizontalAlignment.Left);

        canvas.FontColor = Colors.White;
        canvas.FontSize = 20;
        canvas.DrawString($"Niveau: {level} | Wave: {wave}", 10, 65, HorizontalAlignment.Left);

        // Boutons de contrôle
        DrawButton(canvas, "START", 600, 10, gameState == GameState.Playing ? Colors.Green : Color.FromRgb(0, 150, 0));
        DrawButton(canvas, "PAUSE", 710, 10, gameState == GameState.Paused ? Colors.Yellow : Color.FromRgb(200, 200, 0));
        DrawButton(canvas, "STOP", 820, 10, gameState == GameState.Stopped ? Colors.Red : Color.FromRgb(150, 0, 0));

        string stateText = gameState switch
        {
            GameState.Stopped => "Appuyez sur START",
            GameState.Playing => "EN COURS",
            GameState.Paused => "EN PAUSE",
            _ => ""
        };
        canvas.FontColor = Colors.White;
        canvas.FontSize = 20;
        canvas.DrawString(stateText, 400, 30, HorizontalAlignment.Left);

        // Boutons de sélection de tour
        DrawTowerButton(canvas, "Basic $50", 10, 60, "basic", Colors.Blue);
        DrawTowerButton(canvas, "Rapid $90", 120, 60, "rapid", Colors.Yellow);
        DrawTowerButton(canvas, "Sniper $140", 230, 60, "sniper", Colors.Red);

        // Info objet sélectionné
        if (selectedObject != null)
        {
            string info = "";
            int upgradeCost = 0;
            int objLevel = 0;

            if (selectedObject is Tower tower)
            {
                objLevel = tower.Level;
                upgradeCost = tower.UpgradeCost;
            }
            else if (selectedObject is Tree tree)
            {
                objLevel = tree.Level;
                upgradeCost = tree.UpgradeCost;
            }

            info = $"Niveau {objLevel} | Améliorer (U): ${upgradeCost}";
            canvas.FontSize = 20;
            canvas.FontColor = Colors.White;
            canvas.DrawString(info, 400, 75, HorizontalAlignment.Left);
        }
    }

    private void DrawButton(ICanvas canvas, string text, float x, float y, Color color)
    {
        canvas.FillColor = color;
        canvas.FillRectangle(x, y, 100, 40);
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 2;
        canvas.DrawRectangle(x, y, 100, 40);

        canvas.FontColor = Colors.White;
        canvas.FontSize = 20;
        canvas.DrawString(text, x + 50, y + 25, HorizontalAlignment.Center);
    }

    private void DrawTowerButton(ICanvas canvas, string text, float x, float y, string towerType, Color color)
    {
        bool selected = selectedTowerType == towerType;

        if (selected)
        {
            canvas.FillColor = Colors.White;
            canvas.FillRectangle(x, y, 100, 35);
        }
        else
        {
            canvas.StrokeColor = color;
            canvas.StrokeSize = 2;
            canvas.DrawRectangle(x, y, 100, 35);
        }

        canvas.FontColor = selected ? Colors.Black : Colors.White;
        canvas.FontSize = 18;
        canvas.DrawString(text, x + 50, y + 22, HorizontalAlignment.Center);
    }

    public void HandleClick(float x, float y)
    {
        // Boutons de contrôle
        if (y > 10 && y < 50)
        {
            if (x > 600 && x < 700)
            {
                gameState = GameState.Playing;
                return;
            }
            else if (x > 710 && x < 810)
            {
                gameState = gameState == GameState.Playing ? GameState.Paused : GameState.Playing;
                return;
            }
            else if (x > 820 && x < 920)
            {
                ResetGame();
                return;
            }
        }

        // Sélection de tour
        if (y > 60 && y < 100)
        {
            if (x > 10 && x < 110)
                selectedTowerType = "basic";
            else if (x > 120 && x < 220)
                selectedTowerType = "rapid";
            else if (x > 230 && x < 330)
                selectedTowerType = "sniper";
            else
                selectedTowerType = null;
            return;
        }

        // Placement de tour ou sélection d'objet
        if (y > 100)
        {
            if (selectedTowerType != null)
            {
                int cost = selectedTowerType switch
                {
                    "basic" => 50,
                    "rapid" => 90,
                    "sniper" => 140,
                    _ => 0
                };

                if (money >= cost)
                {
                    towers.Add(new Tower(x, y, selectedTowerType));
                    money -= cost;
                    selectedTowerType = null;
                }
            }
            else
            {
                selectedObject = null;

                foreach (var tower in towers)
                {
                    if (Distance(tower.X, tower.Y, x, y) < 20)
                    {
                        selectedObject = tower;
                        return;
                    }
                }

                foreach (var tree in trees)
                {
                    if (Distance(tree.X, tree.Y, x, y) < 25)
                    {
                        selectedObject = tree;
                        return;
                    }
                }
            }
        }
    }

    private void SpawnWave()
    {
        int count = waveInit + wave * 2;
        enemiesToSpawn = count;
        wave++;
        if (wave % nextLevel == 0)
        {
            level++;
        }
    }

    private void ResetGame()
    {
        money = 120;
        lives = 10;
        level = 1;
        wave = 0;
        enemiesToSpawn = 0;
        spawnTimer = 0;
        towers.Clear();
        enemies.Clear();
        trees.Clear();
        trees.Add(new Tree(900, 300));
        trees.Add(new Tree(900, 400));
        gameState = GameState.Stopped;
    }

    private static float Distance(float x1, float y1, float x2, float y2)
    {
        return MathF.Sqrt(MathF.Pow(x2 - x1, 2) + MathF.Pow(y2 - y1, 2));
    }
}

public enum GameState
{
    Stopped,
    Playing,
    Paused
}