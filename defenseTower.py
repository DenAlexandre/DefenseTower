import pygame
import math
import random

pygame.init()

# Constantes
WIDTH, HEIGHT = 1000, 700
FPS = 60
WHITE = (255, 255, 255)
BLACK = (0, 0, 0)
RED = (255, 0, 0)
GREEN = (0, 200, 0)
DARK_GREEN = (0, 100, 0)
BLUE = (0, 0, 255)
YELLOW = (255, 255, 0)
GRAY = (128, 128, 128)
BROWN = (139, 69, 19)
SNOW = (240, 248, 255)

# Chemin des ennemis
PATH = [
    (50, 350), (150, 350), (150, 200), (300, 200), (300, 500),
    (500, 500), (500, 150), (700, 150), (700, 400), (850, 400),
    (850, 350), (950, 350)
]


# ---------------------------------------------------
# TREE
# ---------------------------------------------------
class Tree:
    """Sapin générateur d'argent"""
    def __init__(self, x, y):
        self.x = x
        self.y = y
        self.level = 1
        self.income = 10
        self.timer = 0
        self.interval = 180
        self.upgrade_cost = 50
        self.radius = 25

    def update(self):
        self.timer += 1
        if self.timer >= self.interval:
            self.timer = 0
            return self.income * self.level
        return 0

    def upgrade(self):
        self.level += 1
        cost = self.upgrade_cost * self.level
        self.income = 10 + (self.level - 1) * 5
        return cost

    def draw(self, screen):
        pygame.draw.rect(screen, BROWN, (self.x - 5, self.y, 10, 20))
        pygame.draw.polygon(screen, DARK_GREEN, [
            (self.x, self.y - 20),
            (self.x - 20, self.y),
            (self.x + 20, self.y)
        ])
        pygame.draw.polygon(screen, DARK_GREEN, [
            (self.x, self.y - 35),
            (self.x - 15, self.y - 15),
            (self.x + 15, self.y - 15)
        ])

        # Timer circulaire
        progress = self.timer / self.interval
        pygame.draw.circle(screen, (50, 50, 50), (self.x + 30, self.y - 20), 18)
        if progress > 0:
            start_angle = -math.pi / 2
            end_angle = start_angle + (2 * math.pi * progress)

            points = [(self.x + 30, self.y - 20)]
            for i in range(int(progress * 100) + 1):
                angle = start_angle + (end_angle - start_angle) * (i / 100)
                px = self.x + 30 + int(17 * math.cos(angle))
                py = self.y - 20 + int(17 * math.sin(angle))
                points.append((px, py))

            if len(points) > 2:
                pygame.draw.polygon(screen, YELLOW, points)
        
        pygame.draw.circle(screen, YELLOW, (self.x + 30, self.y - 20), 18, 2)

        font_small = pygame.font.Font(None, 24)
        dollar_text = font_small.render("$", True, WHITE)
        screen.blit(dollar_text, dollar_text.get_rect(center=(self.x + 30, self.y - 20)))

        font = pygame.font.Font(None, 20)
        text = font.render(f"Lv{self.level}", True, YELLOW)
        screen.blit(text, (self.x - 10, self.y - 50))


# ---------------------------------------------------
# PROJECTILE
# ---------------------------------------------------
class Projectile:
    def __init__(self, x, y, target, damage, speed):
        self.x = x
        self.y = y
        self.target = target
        self.damage = damage
        self.speed = speed
        self.hit = False

    def update(self):
        if not self.target or not self.target.alive:
            self.hit = True
            return

        dx = self.target.x - self.x
        dy = self.target.y - self.y
        dist = math.hypot(dx, dy)

        # Collision robuste par rayon
        if math.hypot(self.x - self.target.x, self.y - self.target.y) < 14:
            self.target.take_damage(self.damage)
            self.hit = True
            return

        self.x += (dx / dist) * self.speed
        self.y += (dy / dist) * self.speed

    def draw(self, screen):
        pygame.draw.circle(screen, YELLOW, (int(self.x), int(self.y)), 5)
        pygame.draw.circle(screen, (255, 200, 0), (int(self.x), int(self.y)), 5, 2)


# ---------------------------------------------------
# TOWER
# ---------------------------------------------------
class Tower:
    def __init__(self, x, y, tower_type="basic"):
        self.x = x
        self.y = y
        self.type = tower_type
        self.level = 1

        if tower_type == "basic":
            self.damage = 12
            self.fire_rate = 35
            self.range = 140
            self.speed = 8
            self.color = BLUE
        elif tower_type == "sniper":
            self.damage = 80
            self.fire_rate = 80
            self.range = 320
            self.speed = 12
            self.color = RED
        elif tower_type == "rapid":
            self.damage = 7
            self.fire_rate = 12
            self.range = 110
            self.speed = 10
            self.color = YELLOW

        self.cooldown = 0
        self.target = None
        self.projectiles = []
        self.upgrade_cost = 50

    def upgrade(self):
        self.level += 1
        self.damage = int(self.damage * 1.4)
        self.range = int(self.range * 1.1)
        self.upgrade_cost = int(self.upgrade_cost * 1.6)
        return self.upgrade_cost

    def update(self, enemies):
        self.cooldown = max(0, self.cooldown - 1)

        if self.cooldown == 0:
            closest = None
            closest_dist = 9999

            for enemy in enemies:
                if not enemy.alive:
                    continue
                dist = math.hypot(enemy.x - self.x, enemy.y - self.y)
                if dist <= self.range and dist < closest_dist:
                    closest = enemy
                    closest_dist = dist

            if closest:
                self.target = closest
                self.shoot()

        for proj in self.projectiles[:]:
            proj.update()
            if proj.hit:
                self.projectiles.remove(proj)

    def shoot(self):
        if self.target:
            if len(self.projectiles) < 3:
                self.projectiles.append(Projectile(self.x, self.y, self.target, self.damage, self.speed))
                self.cooldown = self.fire_rate

    def draw(self, screen):
        s = pygame.Surface((self.range * 2, self.range * 2), pygame.SRCALPHA)
        pygame.draw.circle(s, (*self.color, 30), (self.range, self.range), self.range)
        screen.blit(s, (self.x - self.range, self.y - self.range))

        pygame.draw.circle(screen, self.color, (self.x, self.y), 17)
        pygame.draw.circle(screen, BLACK, (self.x, self.y), 17, 2)

        font = pygame.font.Font(None, 20)
        text = font.render(str(self.level), True, WHITE)
        screen.blit(text, (self.x - 5, self.y - 5))

        for proj in self.projectiles:
            proj.draw(screen)


# ---------------------------------------------------
# ENEMY
# ---------------------------------------------------
class Enemy:
    """Ennemi équilibré avec barre de vie améliorée"""
    def __init__(self, level, spawn_point):
        self.level = level
        self.max_hp = 80 + (level - 1) * 35
        self.hp = self.max_hp
        self.speed = 1.2 + level * 0.1
        self.reward = 15 + level * 5
        self.path_index = 0
        self.x, self.y = spawn_point
        self.has_tree = False
        self.return_path = False
        self.alive = True

    def update(self):
        if not self.alive:
            return

        path = PATH if not self.return_path else list(reversed(PATH))

        if self.path_index < len(path):
            tx, ty = path[self.path_index]
            dx = tx - self.x
            dy = ty - self.y
            dist = math.hypot(dx, dy)

            if dist < self.speed:
                self.x, self.y = tx, ty
                self.path_index += 1
            else:
                self.x += (dx / dist) * self.speed
                self.y += (dy / dist) * self.speed
        else:
            if not self.return_path:
                self.has_tree = True
                self.return_path = True
                self.path_index = 0
            else:
                self.alive = False
                return True
        return False

    def take_damage(self, damage):
        self.hp = max(0, self.hp - damage)
        if self.hp <= 0:
            self.alive = False
            return True
        return False

    def draw(self, screen):
        if not self.alive:
            return
            
        body_color = (220, 20, 20) if self.has_tree else (138, 43, 226)
        pygame.draw.circle(screen, body_color, (int(self.x), int(self.y)), 12)
        pygame.draw.circle(screen, BLACK, (int(self.x), int(self.y)), 12, 2)

        # Barre de vie améliorée
        bar_width = 32
        bar_height = 6
        hp_ratio = self.hp / self.max_hp
        bar_x = int(self.x) - bar_width // 2
        bar_y = int(self.y) - 25
        progress_bar = int(bar_width * hp_ratio)

        pygame.draw.rect(screen, (80, 0, 0), (bar_x, bar_y, bar_width, bar_height))

        if hp_ratio > 0.5:
            color = (0, 200, 0)
        elif hp_ratio > 0.25:
            color = (255, 165, 0)
        else:
            color = (255, 0, 0)

        pygame.draw.rect(screen, color, (bar_x, bar_y, progress_bar, bar_height))
        pygame.draw.rect(screen, BLACK, (bar_x, bar_y, bar_width, bar_height), 1)
        
        # Afficher le pourcentage de vie
        font_hp = pygame.font.Font(None, 16)
        hp_percent = int(hp_ratio * 100)
        hp_text = font_hp.render(f"{hp_percent}%", True, WHITE)
        text_rect = hp_text.get_rect(center=(int(self.x), bar_y - 8))
        screen.blit(hp_text, text_rect)
        


# ---------------------------------------------------
# GAME
# ---------------------------------------------------
class Game:
    def __init__(self):
        self.screen = pygame.display.set_mode((WIDTH, HEIGHT))
        pygame.display.set_caption("Defense Tower")
        self.clock = pygame.time.Clock()

        self.money = 120
        self.lives = 10
        self.level = 1
        self.next_level = 3
        self.wave_init = 5
        self.wave = 0
        
        # Gestion du spawn progressif
        self.enemies_to_spawn = 0
        self.spawn_timer = 0
        self.spawn_interval = 40  # Frames entre chaque spawn

        self.towers = []
        self.enemies = []
        self.trees = [
            Tree(900, 300),
            Tree(900, 400)
        ]

        self.selected_tower_type = None
        self.selected_object = None
        self.running = True
        self.game_state = "stopped"

    def spawn_wave(self):
        count = self.wave_init + self.wave * 2
        self.enemies_to_spawn = count
        self.wave += 1
        if self.wave % self.next_level == 0:
            self.level += 1

    def handle_events(self):
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                self.running = False

            if event.type == pygame.MOUSEBUTTONDOWN:
                mx, my = pygame.mouse.get_pos()

                # Contrôles Start/Pause/Stop
                if 10 < my < 50:
                    if 600 < mx < 700:
                        self.game_state = "playing"
                    elif 710 < mx < 810:
                        self.game_state = "paused" if self.game_state == "playing" else "playing"
                    elif 820 < mx < 920:
                        self.game_state = "stopped"
                        self.reset_game()

                # Sélection des tours
                if 60 < my < 100:
                    if 10 < mx < 110:
                        self.selected_tower_type = "basic"
                    elif 120 < mx < 220:
                        self.selected_tower_type = "rapid"
                    elif 230 < mx < 330:
                        self.selected_tower_type = "sniper"
                    else:
                        self.selected_tower_type = None
                elif my > 100:
                    if self.selected_tower_type:
                        cost = {"basic": 50, "rapid": 90, "sniper": 140}[self.selected_tower_type]
                        if self.money >= cost:
                            self.towers.append(Tower(mx, my, self.selected_tower_type))
                            self.money -= cost
                            self.selected_tower_type = None

                    else:
                        self.selected_object = None
                        for tower in self.towers:
                            if math.hypot(tower.x - mx, tower.y - my) < 20:
                                self.selected_object = tower
                                break
                        if not self.selected_object:
                            for tree in self.trees:
                                if math.hypot(tree.x - mx, tree.y - my) < 25:
                                    self.selected_object = tree
                                    break

            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_u and self.selected_object:
                    cost = self.selected_object.upgrade()
                    if self.money >= cost:
                        self.money -= cost
                    else:
                        self.selected_object.level -= 1

    def reset_game(self):
        self.money = 120
        self.lives = 10
        self.level = 1
        self.wave = 0
        self.enemies_to_spawn = 0
        self.spawn_timer = 0
        self.towers = []
        self.enemies = []
        self.trees = [
            Tree(900, 300),
            Tree(900, 400)
        ]


    def update(self):
        if self.game_state != "playing":
            return

        for tree in self.trees:
            self.money += tree.update()

        # Spawn une nouvelle vague si tous les ennemis sont éliminés ET qu'il n'y en a plus à spawner
        if len(self.enemies) == 0 and self.enemies_to_spawn == 0:
            self.spawn_wave()
        
        # Spawn progressif des ennemis
        if self.enemies_to_spawn > 0:
            self.spawn_timer += 1
            if self.spawn_timer >= self.spawn_interval:
                self.enemies.append(Enemy(self.level, PATH[0]))
                self.enemies_to_spawn -= 1
                self.spawn_timer = 0

        for tower in self.towers:
            tower.update(self.enemies)

        for enemy in self.enemies[:]:
            result = enemy.update()
            if result:
                self.lives -= 1
                self.enemies.remove(enemy)
            elif not enemy.alive:
                if not enemy.return_path:
                    self.money += enemy.reward
                self.enemies.remove(enemy)

        if self.lives <= 0:
            self.game_state = "stopped"

    def draw(self):
        self.screen.fill(SNOW)

        for i in range(len(PATH) - 1):
            pygame.draw.line(self.screen, GRAY, PATH[i], PATH[i+1], 30)

        for tree in self.trees:
            tree.draw(self.screen)

        for tower in self.towers:
            tower.draw(self.screen)

        # Dessiner chaque ennemi en passant son index
        for enemy in self.enemies:
            enemy.draw(self.screen)

        self.draw_ui()
        pygame.display.flip()

    def draw_ui(self):
        pygame.draw.rect(self.screen, DARK_GREEN, (0, 0, WIDTH, 100))

        font = pygame.font.Font(None, 30)
        small_font = pygame.font.Font(None, 24)

        control_buttons = [
            ("START", 600, GREEN if self.game_state == "playing" else (0, 150, 0)),
            ("PAUSE", 710, YELLOW if self.game_state == "paused" else (200, 200, 0)),
            ("STOP", 820, RED if self.game_state == "stopped" else (150, 0, 0))
        ]

        for text, x, color in control_buttons:
            pygame.draw.rect(self.screen, color, (x, 10, 100, 40))
            pygame.draw.rect(self.screen, BLACK, (x, 10, 100, 40), 2)
            t = small_font.render(text, True, WHITE)
            self.screen.blit(t, (x + 20, 20))

        state_text = {
            "stopped": "Appuyez sur START",
            "playing": "EN COURS",
            "paused": "EN PAUSE"
        }

        self.screen.blit(
            small_font.render(state_text[self.game_state], True, WHITE),
            (400, 25)
        )

        self.screen.blit(font.render(f"Argent: ${self.money}", True, YELLOW), (10, 1))
        self.screen.blit(font.render(f"Vies: {self.lives}", True, RED), (10, 20))
        self.screen.blit(
            small_font.render(f"Niveau: {self.level} | Wave: {self.wave}", True, WHITE),
            (10, 40)
        )

        buttons = [
            ("Basic $50", 10, "basic", BLUE),
            ("Rapid $90", 120, "rapid", YELLOW),
            ("Sniper $140", 230, "sniper", RED)
        ]

        for text, x, t_type, color in buttons:
            selected = self.selected_tower_type == t_type
            btn_color = WHITE if selected else color
            pygame.draw.rect(self.screen, btn_color, (x, 60, 100, 35), 0 if selected else 2)
            txt = small_font.render(text, True, BLACK if selected else WHITE)
            self.screen.blit(txt, (x + 10, 72))

        if self.selected_object:
            info = f"Niveau {self.selected_object.level} | Améliorer (U): ${self.selected_object.upgrade_cost}"
            self.screen.blit(small_font.render(info, True, WHITE), (400, 70))

    def run(self):
        while self.running:
            self.handle_events()
            self.update()
            self.draw()
            self.clock.tick(FPS)
        pygame.quit()


# Lancement du jeu
if __name__ == "__main__":
    game = Game()
    game.run()
