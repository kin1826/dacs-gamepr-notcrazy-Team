# Code Review Summary

## Trang Thai Chung

Project hien tai da co nen tang multiplayer online cho 2 nguoi bang Unity Relay.

Trang thai nen hieu la:

- Da san sang de build APK/PC test gameplay.
- Da chay duoc flow host/client co ban.
- Chua phai ban release cuoi.
- Van can tiep tuc design level, test Android, polish UI, va test nhieu truong hop loi.

## Nhung Viec Da Lam Duoc

### Multiplayer / Relay

- `NetworkConfig` khoi tao Unity Services va Authentication.
- Host tao Relay allocation va lay join code.
- Client join Relay bang code.
- Join code duoc normalize truoc khi join.
- `NetworkManager` duoc giu qua scene bang `DontDestroyOnLoad`.
- Duplicate `NetworkManager` bi destroy khi load scene moi.
- Host la ben load scene multiplayer bang `NetworkManager.SceneManager`.

### Lobby

- `LobbyManager` sync dino host/client bang `NetworkVariable`.
- Host/client chon dino trong lobby.
- Host thay client da chon dino.
- Client khong tu start game; host la nguoi bam Play.

### Player

- Player spawn vao spawn point trong gameplay scene.
- Single player tu spawn tai `Spawn_1` khi load gameplay scene.
- Player khong bi spawn sai trong menu nua.
- Player prefab co `NetworkObject` van dieu khien duoc trong single vi `PlayerMovement` co local-control fallback.
- Skin dino sync dung sau khi chon.
- Movement sync vi tri qua `NetworkTransform`.
- Running animation va facing direction sync qua `NetworkVariable`.
- Player collider tu dung no-friction material de tranh dinh canh platform.
- Client da nhay duoc.
- Mobile control code da co san.

### Scene / Level

- KillZone reload scene:
  - Single: reload local scene.
  - Multiplayer: host/server reload scene cho ca team.
- LevelComplete single: 1 player cham door la complete.
- LevelComplete multiplayer: can du player connected dung o cac door trong cung `groupId`.
- `LevelComplete` bay gio chi quan ly visual hoan thanh cho player (slide door, disable movement) va restore player sau do, roi goi `LevelManager.LoadLevel()` de load scene tiep theo.
- `LevelComplete` dung RPC neu door co `NetworkObject` de sync hieu ung door truot va player stick theo door.
- `LevelCompleteDoorGroup` giup mot scene dung chung: single an Door_2, multiplayer hien ca hai door.
- `LevelManager` singleton - copy logic scene load tu `MainManager.StartGame`, ko animation ko fade. Neu host se dung `NetworkManager.SceneManager.LoadScene`, neu single dung `SceneManager.LoadScene`.

### Trap System

Da them bo trap chung:

- `TrapAction`
- `TrapTrigger`
- `TrapButton`

Dung de tao:

- Trigger lam platform move.
- Trigger theo so lan di qua.
- Trigger can nhieu player cung dung trong vung.
- Button press once.
- Button toggle.
- Button hold cho co-op.
- Single co the auto convert hold thanh press once.
- `TrapButton` co the nhan them activator tag nhu `PushBlock` hoac `ButtonWeight`.
- `PushBlock` la khoi vat ly player co the day de giu nut, co option dung ngang khi khong con bi day.

## Files Da Sua / Them

| File | Trang thai | Ghi chu |
|---|---|---|
| `Assets/Script/NetworkConfig.cs` | Da sua | Relay, services, NetworkManager persistence, single spawn |
| `Assets/Script/manager/MainManager.cs` | Da sua | Menu, create/join room, dino select, start game |
| `Assets/Script/LobbyManager.cs` | Dang dung | Sync lobby dino |
| `Assets/Script/PlayerMovement.cs` | Da sua | Movement, jump, animation sync, mobile API, no-friction collider |
| `Assets/Script/PlayerSkinManager.cs` | Da sua | Sync selected dino skin |
| `Assets/Script/PlayerSpawner.cs` | Da sua | Spawn only outside Menu, spawn validation |
| `Assets/Script/KillZoneReload.cs` | Da sua | Host/server reload trong multiplayer |
| `Assets/Script/LevelComplete.cs` | Da sua | Single 1 door, multi nhieu door cung group, player stick khong parent |
| `Assets/Script/LevelManager.cs` | Moi | Simple scene load - copy StartGame logic, host use Netcode, single use local load |
| `Assets/Script/SceneTransition.cs` | Moi | (Khong dung - deprecated) |
| `Assets/Script/LevelCompleteDoorGroup.cs` | Moi | An/hien Door_2 tuy single/multiplayer |
| `Assets/Script/MobileControlButton.cs` | Moi | Nut mobile UI |
| `Assets/Script/TrapAction.cs` | Moi | Action chung cho trap |
| `Assets/Script/TrapTrigger.cs` | Moi | Trigger chung cho trap |
| `Assets/Script/TrapButton.cs` | Moi | Button trap press/toggle/hold |
| `Assets/Script/PushBlock.cs` | Moi | Khoi vat ly de day va de nut |

## Nhung Dieu Can Canh Giac

### 1. Scene reference trong Inspector

Nhieu loi co the khong nam trong code ma nam o Inspector:

- Button OnClick con tro toi class cu.
- `MainManager` thieu reference.
- `NetworkManager.PlayerPrefab` chua dung.
- Scene chua co trong Build Settings.

### 2. NetworkObject / NetworkTransform

Object nao can sync transform trong multiplayer thi nen co:

- `NetworkObject`
- `NetworkTransform`

Vi du:

- Player prefab.
- Platform trap di chuyen.

### 3. Trap co action SetActive

Neu disable object co `NetworkObject`, can test ky trong multiplayer. Mot so truong hop nen disable renderer/collider thay vi disable ca root object.

### 4. Mobile UI

Code mobile da co, nhung layout UI tren Android can test that:

- Nut co du lon khong.
- Co bi che gameplay khong.
- Touch co nhan dung khong.

## Test Can Lam Truoc Khi Noi La On

### Core Test

- [ ] Single vao level, chon dung dino.
- [ ] Single spawn tai `Spawn_1`.
- [ ] Single di chuyen/nhay.
- [ ] Single roi DieZone reload.
- [ ] Multiplayer host tao room.
- [ ] Multiplayer client join room.
- [ ] Host/client chon dino.
- [ ] Host start game.
- [ ] Ca hai spawn dung.
- [ ] Ca hai thay animation cua nhau.
- [ ] Ca hai nhay duoc.
- [ ] Single complete level voi 1 door.
- [ ] Multiplayer chi complete khi du player dung vao cac door.
- [ ] Single an Door_2 qua `LevelCompleteDoorGroup`.

### Trap Test

- [ ] Trigger move platform trong single.
- [ ] Trigger move platform trong multiplayer.
- [ ] DieZone reload ca team.
- [ ] Button hold hoat dong trong multi.
- [ ] Button hold auto thanh press once trong single neu bat option.
- [ ] Push block day duoc va giu duoc nut hold.

### Android Test

- [ ] APK mo ngang.
- [ ] Touch control di chuyen duoc.
- [ ] Touch jump duoc.
- [ ] Relay join duoc tu Android.

## Huong Phat Trien Tiep Theo

Gan nhat:

- Tao them level 1/2/3 voi trap co ban.
- Setup mobile UI dep hon.
- Them feedback khi create/join room thanh cong hoac that bai.
- Them loading/status text khi dang join Relay.

Sau do:

- Checkpoint/respawn thay vi reload scene neu muon game bot gat.
- Door/lever puzzle cho co-op.
- Player ready state trong lobby.
- More trap actions: rotate, fade, timed reset.
- Am thanh va VFX cho trap.

## Ket Luan

Project dang o giai doan playable prototype:

- Nen tang multiplayer da co.
- Relay da dung duoc.
- Player control da on.
- Trap framework da bat dau co.
- Viec tiep theo nen tap trung vao level design va test tren Android/thiet bi that.
