# Multiplayer Setup Guide

## Project Nay Dang Lam Gi?

Day la game platform 2D co 2 che do:

- Single player: 1 nguoi choi chon dino va vao man.
- Multiplayer online: 2 nguoi choi ket noi bang Unity Relay join code, moi nguoi chon dino, host bam Play de ca hai vao game.

Gameplay hien tai tap trung vao:

- Dino player di chuyen, nhay, animation chay.
- Chon skin/mau dino trong menu.
- Single player spawn tai `Spawn_1` khi vao level.
- Host tao phong Relay, client nhap code de join.
- Lobby hien thi dino cua host va client.
- Scene load dong bo qua Netcode.
- Trap co the dung chung cho single va multiplayer.

## Cong Nghe Dang Dung

- Unity 6000.x
- Unity Netcode for GameObjects `2.11.2`
- Unity Transport `2.6.0`
- Unity Services Multiplayer / Relay
- Unity Authentication anonymous sign-in
- Unity Input System
- TextMesh Pro
- ParrelSync de test nhieu instance trong Editor neu can

## Flow Multiplayer Hien Tai

1. Player 1 mo Multiplayer.
2. Player 1 bam Create Room.
3. `NetworkConfig.CreateRelay()` tao allocation tren Unity Relay.
4. Host nhan join code va hien trong UI.
5. Player 2 nhap join code.
6. `NetworkConfig.JoinRelay()` ket noi client vao host.
7. Host chon dino, client chon dino.
8. `LobbyManager` sync lua chon dino bang `NetworkVariable`.
9. Khi client da chon dino, host thay dino cua client va nut Play bat.
10. Host bam Play.
11. `NetworkManager.SceneManager.LoadScene()` load level cho ca hai may.
12. Player prefab spawn, moi may dieu khien player cua chinh minh.

## Cac Script Chinh

| Script | Vai tro |
|---|---|
| `NetworkConfig.cs` | Khoi tao Unity Services, tao/join Relay, giu NetworkManager song qua scene |
| `MainManager.cs` | Dieu khien UI menu, chon dino, tao/join phong, start game |
| `LobbyManager.cs` | Sync dino host/client trong lobby |
| `PlayerSpawner.cs` | Dat player vao spawn point dung trong level |
| `PlayerMovement.cs` | Di chuyen, nhay, local-control fallback cho single, sync animation chay/huong nhin, giam dinh canh platform |
| `PlayerSkinManager.cs` | Sync skin/mau dino cua moi player |
| `MobileControlButton.cs` | Nut UI mobile de di trai/phai/nhay |
| `KillZoneReload.cs` | Roi vao die zone thi reload scene |
| `LevelComplete.cs` | Qua cong ket thuc level va load scene tiep theo |
| `LevelCompleteDoorGroup.cs` | An/hien Door_2 tuy single hay multiplayer |
| `TrapAction.cs` | Action chung cho trap: move, active object, collider, reload |
| `TrapTrigger.cs` | Trigger kich hoat trap theo dieu kien |
| `TrapButton.cs` | Nut bam trong level: press once, toggle, hold |
| `PushBlock.cs` | Khoi day duoc de giu nut hold |
| `LevelManager.cs` | Simple scene load - copy logic from MainManager.StartGame |
| `SceneTransition.cs` | (Khong dung) |

## Trang Thai Hien Tai

Da hoat dong:

- Tao phong Relay.
- Join phong bang code.
- Host/client chon dino.
- Host thay client da vao/chon dino.
- Host bam Play, ca hai vao game.
- Spawn 2 player trong level.
- Single player spawn tai `Spawn_1`.
- Skin dino sync dung.
- Di chuyen va nhay tren host/client.
- Ben kia thay animation chay va huong nhin.
- Mobile control button da co code.
- KillZone reload scene dung cho single/multi.
- LevelComplete ho tro 1 scene dung chung: single an Door_2, multiplayer hien ca Door_1 va Door_2.
- Trap system co ban da co code.
- Push block co the giu nut hold neu dung tag `PushBlock` hoac `ButtonWeight`.

Can tiep tuc lam:

- Thiet ke them man choi.
- Dat trap/puzzle vao hierarchy.
- Test Android UI va kich thuoc nut mobile.
- Kiem tra lai tat ca scene trong Build Settings.
- Them am thanh, UI polish, feedback khi join fail.

## Setup Hierarchy Can Co

Scene `Menu`:

- `NetworkManager`
  - `NetworkManager`
  - `UnityTransport`
  - `NetworkConfig`
- `LobbyManager`
  - `NetworkObject`
  - `LobbyManager`
- `MenuManager`
  - `MainManager`
  - Gan day du panel, button, dino sprite, input field.

Scene gameplay, vi du `Level_1`:

- It nhat 2 object tag `Spawn`.
- Nen co object ten `Spawn_1` de single player spawn dung vi tri mac dinh.
- Nen to chuc hierarchy:
  - `Doors_Complete`
  - `Door_1`
  - `Door_2`
- Gan `LevelCompleteDoorGroup` vao `Doors_Complete`.
- Keo `Door_1` vao `primaryDoor`, `Door_2` vao `secondaryDoor`.
- Bat `hideSecondaryDoorInSingle` de single chi hien Door_1.
- Gan `LevelComplete` vao ca `Door_1` va `Door_2`, cung `groupId`; multiplayer moi player dung 1 door moi complete.
- Door multiplayer nen co `NetworkObject` de sync hieu ung truot/player stick.
- Cac platform, trap, DieZone.
- Push block nen co `Rigidbody2D`, `Collider2D`, `PushBlock`, va tag `PushBlock`/`ButtonWeight`.
- Neu push block can sync trong multi, them `NetworkObject` va `NetworkTransform`.
- Khong nen co player dat san trong scene.
- Neu co `NetworkManager` duplicate trong level thi code se destroy duplicate, nhung cach sach hon la chi giu NetworkManager o `Menu`.
- Kiem tra `Build Settings > Scenes In Build`: cac scene gameplay va menu phai duoc add va sap xep dung thu tu.
- `LevelComplete` goi `LevelManager.Instance.LoadLevel(sceneName)` de load scene tiep theo. LevelManager copy logic tu `MainManager.StartGame`: host dung `NetworkManager.SceneManager.LoadScene`, single dung local `SceneManager.LoadScene`.
## Build Android

Truoc khi build APK:

- `File > Build Settings`: Switch Platform sang Android.
- Them scene `Menu` va cac level vao `Scenes In Build`.
- `Project Settings > Player > Android > Resolution and Presentation`:
  - `Default Orientation`: `Landscape Left`
  - Khong dung Portrait neu game chi ngang.
- Test tren it nhat 2 thiet bi hoac 1 Android + 1 Editor/PC.

## Ghi Chu Quan Trong

- Trong multiplayer, host/server nen la ben xu ly logic quan trong: trap, reload scene, level complete.
- Client chi gui input/yeu cau, khong tu load scene hay tu quyet dinh trap.
- Neu object di chuyen can client thay duoc, them `NetworkObject` va `NetworkTransform`.
- Neu button OnClick trong scene con hien type cu `MenuManager`, hay remove event va gan lai object co `MainManager`.
