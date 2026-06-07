# 🦕 Dino Platformer — Project Overview

> Game 2D Platformer dành cho 1-2 người chơi, xây dựng bằng Unity với hỗ trợ multiplayer online qua Unity Netcode & Unity Relay.

---

## 📌 Thông tin chung

| Mục | Chi tiết |
|-----|---------|
| Engine | Unity (2D) |
| Ngôn ngữ | C# |
| Multiplayer | Unity Netcode for GameObjects (NGO) + Unity Relay |
| Platform mục tiêu | PC / Mobile |
| Input | Unity Input System (New) |
| Số người chơi | 1 (Single Player) hoặc 2 (Co-op Online) |

---

## 🗂️ Cấu trúc thư mục Script

```
Assets/Script/
├── core/               # Dữ liệu toàn cục & lưu trữ
│   ├── GameData.cs         — Biến toàn cục (chế độ chơi, nhân vật, level)
│   ├── SaveData.cs         — Cấu trúc dữ liệu lưu
│   ├── SaveManager.cs      — Đọc/ghi file save (JSON)
│   ├── RoomData.cs         — Dữ liệu phòng multiplayer
│   ├── DinoSelectButton.cs — Nút chọn nhân vật
│   └── NetworkErrorHandler.cs
│
├── manager/            # Quản lý flow game
│   ├── MainManager.cs      — UI flow: menu, chọn nhân vật, tạo/join phòng
│   ├── LobbyManager.cs     — Đồng bộ lobby qua NetworkVariable
│   └── LevelManager.cs     — Tải scene (hỗ trợ cả SP và MP)
│
├── utils/
│   └── ColorConfig.cs
│
├── PlayerMovement.cs       — Di chuyển & nhảy (Physics + Network sync)
├── PlayerSkinManager.cs    — Đổi skin nhân vật, đồng bộ qua mạng
├── PlayerSpawner.cs        — Spawn người chơi vào đúng vị trí
├── NetworkConfig.cs        — Cấu hình Relay, xác thực Unity Services
├── NetworkSetup.cs         — Khởi tạo UnityTransport
├── NetworkUI.cs            — UI liên quan đến network
├── LevelComplete.cs        — Xử lý hoàn thành level (cả SP & MP)
├── LevelSelectManager.cs   — Màn hình chọn level
├── LevelButton.cs          — Nút chọn level riêng lẻ
├── LevelCompleteDoorGroup.cs
├── KillZoneReload.cs       — Vùng chết → reload scene
├── TrapAction.cs           — Cơ chế bẫy (di chuyển, bật/tắt, reload)
├── TrapButton.cs           — Nút kích hoạt bẫy
├── TrapTrigger.cs          — Trigger zone cho bẫy
├── TriggerZone.cs          — Zone trigger đa năng
├── PlatformSequence.cs     — Nền tảng di chuyển theo chuỗi
├── PushBlock.cs            — Khối đẩy được
├── SceneTransition.cs      — Chuyển cảnh
├── MobileControlButton.cs  — Nút điều khiển trên mobile
└── FunctionPanelController.cs
```

---

## 🎮 Tính năng chính

### 1. Di chuyển nhân vật (`PlayerMovement.cs`)
- Di chuyển trái/phải, nhảy với physics `Rigidbody2D`
- Kiểm tra mặt đất bằng `OverlapCircle`
- Hỗ trợ cả bàn phím (New Input System) và nút mobile
- **Multiplayer:** đồng bộ animation `isRunning` và hướng nhìn qua `NetworkVariable`
- Áp dụng vật liệu không ma sát (`NoFriction PhysicsMaterial2D`) tự động

### 2. Chế độ chơi
#### Single Player
- Người chơi chọn nhân vật → chọn level → chơi
- Spawn tự động tại điểm `Spawn_1` hoặc tag `Spawn`

#### Multiplayer (Co-op 2 người)
- **Host** tạo phòng → lấy mã Relay → chia sẻ mã
- **Client** nhập mã → join phòng
- Cả 2 chọn nhân vật khủng long khác nhau
- Host bấm Play → tải level đồng bộ cho cả 2 người

### 3. Hệ thống Relay/Network (`NetworkConfig.cs`)
```
Unity Services Authentication (Anonymous)
    → Unity Relay: CreateAllocation / JoinAllocation
        → UnityTransport: SetHostRelayData / SetClientRelayData
            → NetworkManager: StartHost / StartClient
```
- Xác thực ẩn danh qua `AuthenticationService`
- Kết nối online không cần IP (P2P qua Relay server của Unity)
- Mã phòng được chuẩn hóa: uppercase, chỉ chữ và số

### 4. Lobby & Chọn nhân vật (`LobbyManager.cs`, `MainManager.cs`)
- `LobbyManager` là `NetworkBehaviour` singleton, tồn tại xuyên scene
- Đồng bộ 3 giá trị qua `NetworkVariable`:
  - `HostDino` — nhân vật của host
  - `ClientDino` — nhân vật của client
  - `MinLevelLobby` — level tối thiểu giữa 2 người (mở level theo progress)
- Client không thể chọn cùng nhân vật với host

### 5. Hệ thống Level

#### Hoàn thành level (`LevelComplete.cs`)
- **Single player:** chạm trigger → slide animation → tải scene tiếp theo
- **Multiplayer:** cả 2 người phải vào cùng trigger group → server xử lý → tải scene đồng bộ
- Tự động lưu tiến độ (level cao nhất đã qua)
- Tên scene tiếp theo: lấy tự động theo `buildIndex + 1` hoặc cấu hình tay

#### Level Select (`LevelSelectManager.cs`, `LevelButton.cs`)
- Hiển thị các level đã mở khóa
- Multiplayer: chỉ mở đến level thấp hơn giữa 2 người (`MinLevelLobby`)

#### Tải scene (`LevelManager.cs`)
- Single player: `SceneManager.LoadScene()`
- Multiplayer: `NetworkManager.Singleton.SceneManager.LoadScene()`

### 6. Bẫy & Cơ chế gameplay

#### Hệ thống bẫy (`TrapAction.cs`, `TrapButton.cs`, `TrapTrigger.cs`)
Các loại hành động bẫy:
| ActionType | Mô tả |
|-----------|-------|
| `MoveToTarget` | Di chuyển vật thể đến vị trí đích |
| `MoveBackToStart` | Di chuyển vật thể về vị trí ban đầu |
| `SetObjectActive` | Bật/tắt GameObject |
| `SetColliderEnabled` | Bật/tắt Collider2D |
| `ReloadScene` | Reload lại scene hiện tại |

- Tất cả hành động bẫy được đồng bộ qua `ServerRpc` trong multiplayer
- Hỗ trợ `triggerOnlyOnce` — chỉ kích hoạt 1 lần

#### KillZone (`KillZoneReload.cs`)
- Chạm → reload scene (đồng bộ cho multiplayer)

#### Platform di chuyển (`PlatformSequence.cs`)
- Nền tảng chuyển động theo chuỗi waypoints

### 7. Nhân vật & Skin (`PlayerSkinManager.cs`)
- Nhiều skin khủng long (RuntimeAnimatorController)
- Đổi skin realtime, đồng bộ qua `NetworkVariable<int>`
- Skin được lưu vào save file và tự động áp dụng khi vào game

### 8. Lưu trữ dữ liệu (`SaveManager.cs`, `SaveData.cs`)
```csharp
public class SaveData {
    public int highestLevel = 1;   // Level cao nhất đã hoàn thành
    public int selectedDino = 0;   // Nhân vật đã chọn lần cuối
}
```
- Lưu dưới dạng JSON (PlayerPrefs hoặc file)
- Tự động load khi khởi động (`MainManager.Awake`)
- Tự động save khi hoàn thành level

---

## 🔄 Luồng game

```
[Menu Scene]
    │
    ├── Single Player ──→ Chọn Dino ──→ Chọn Level ──→ [Level Scene]
    │
    └── Multiplayer
            │
            ├── Tạo phòng (Host) ──→ Unity Relay ──→ Lấy mã ──→ Chọn Dino
            │                                                          │
            └── Join phòng (Client) ──→ Nhập mã ──→ Chọn Dino ────────┘
                                                                       │
                                                            Host bấm Play
                                                                       │
                                                            [Level Scene - đồng bộ]
                                                                       │
                                              ┌─── Hoàn thành level ───┤
                                              │                        │
                                      Level tiếp theo          Chạm KillZone
                                                                       │
                                                               Reload scene
```

---

## 🌐 Multiplayer Architecture

```
HOST (Server + Client)              CLIENT
      │                                │
      │◄──── Unity Relay Server ──────►│
      │                                │
  NetworkManager.IsServer          NetworkManager.IsClient
      │                                │
  ServerRpc ◄──────────────────────── gửi input/action
      │
  Xử lý logic
      │
  ClientRpc ──────────────────────────► cập nhật visual
      │
  NetworkVariable ──────────────────── tự động sync
```

**Nguyên tắc:**
- Chỉ Server xử lý logic game (movement, trap, level load)
- Client gửi input lên Server qua `ServerRpc`
- Server broadcast kết quả xuống Client qua `ClientRpc` hoặc `NetworkVariable`
- Scene loading luôn do Host/Server khởi xướng

---

## 📦 Packages sử dụng

| Package | Mục đích |
|---------|---------|
| `com.unity.netcode.gameobjects` | Multiplayer networking |
| `com.unity.services.relay` | Kết nối online không cần IP |
| `com.unity.services.authentication` | Xác thực người dùng |
| `com.unity.services.core` | Unity Gaming Services core |
| `com.unity.inputsystem` | Input đa nền tảng |
| `com.unity.2d.spriteshape` | Tạo địa hình 2D |
| `ParrelSync` | Test multiplayer trong Editor (2 instance) |

---

## 🐛 Các điểm lưu ý khi phát triển

1. **Scene loading trong Multiplayer** phải dùng `NetworkManager.Singleton.SceneManager.LoadScene()`, không dùng `SceneManager.LoadScene()` trực tiếp
2. **ServerRpc** chỉ được gọi từ Owner, trừ khi có `RequireOwnership = false`
3. **NetworkVariable** chỉ được ghi từ Server
4. **LobbyManager** dùng `DontDestroyOnLoad` — cẩn thận duplicate khi reload scene
5. **KillZone** hiện tại reload ngay khi **bất kỳ** player chạm — có thể ảnh hưởng cả 2 người chơi trong multiplayer

---

*Tài liệu được tạo tự động từ source code — cập nhật lần cuối: 2026-05-27*
