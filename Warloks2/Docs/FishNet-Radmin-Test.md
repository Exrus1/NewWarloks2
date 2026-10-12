# FishNet + Radmin — проверка

Порт по умолчанию: **7770** (UDP, Tugboat).

## Первый запуск в Editor

1. Откройте проект в Unity и дождитесь компиляции.
2. Меню **Warloks → FishNet → Run Migration Setup** (если auto-setup не сработал).
3. Убедитесь, что в `Menu` есть `NetworkManager` (DefaultScene: Menu ↔ Lobby).

## Host + Client (Radmin)

1. Оба ПК в одной сети Radmin VPN.
2. **Хост:** кнопка «Хост» — запоминает Radmin IP (из Radmin Viewer).
3. **Клиент:** в поле IP вставить Radmin-IP хоста → «Клиент».
4. Лобби: чат, список ников, хост жмёт Start → сцена Game.
5. Проверить движение, скиллы, урон.

## Dedicated Server

1. На машине A: «Сервер» (без локального игрока).
2. Клиенты B/C подключаются по IP A.

## Leave / Back

- Leave в лобби → disconnect → Menu.
- Back to Lobby на Game (только сервер) → Lobby.
