# NotCrazy — Tài liệu kỹ thuật

> Game 2D Platformer Co-op cho 1–2 người chơi, xây dựng bằng Unity với multiplayer online real-time, backend .NET API và hệ thống kinh tế trong game.

---

## Mục lục

1. [Tổng quan](#1-tổng-quan)
2. [Công nghệ sử dụng](#2-công-nghệ-sử-dụng)
3. [Kiến trúc hệ thống](#3-kiến-trúc-hệ-thống)
4. [Nền tảng hỗ trợ](#4-nền-tảng-hỗ-trợ)
5. [Cấu trúc dự án](#5-cấu-trúc-dự-án)
6. [Cơ chế gameplay](#6-cơ-chế-gameplay)
7. [Hệ thống multiplayer](#7-hệ-thống-multiplayer)
8. [Hệ thống tài khoản](#8-hệ-thống-tài-khoản)
9. [Hệ thống kinh tế (Gold)](#9-hệ-thống-kinh-tế-gold)
10. [Hệ thống nạp tiền](#10-hệ-thống-nạp-tiền)
11. [Quà đăng nhập hàng ngày](#11-quà-đăng-nhập-hàng-ngày)
12. [Hệ thống skip level](#12-hệ-thống-skip-level)
13. [Lưu trữ dữ liệu](#13-lưu-trữ-dữ-liệu)
14. [Backend API](#14-backend-api)
15. [Cơ sở dữ liệu](#15-cơ-sở-dữ-liệu)
16. [Triển khai](#16-triển-khai)
17. [So sánh công nghệ](#17-so-sánh-công-nghệ)
18. [Hạn chế và hướng phát triển](#18-hạn-chế-và-hướng-phát-triển)

---

## 1. Tổng quan

| Mục | Chi tiết |
|-----|---------|
| Tên game | NotCrazy |
| Thể loại | 2D Platformer Co-op |
| Số người chơi | 1 (Single Player) hoặc 2 (Co-op Online) |
| Engine | Unity 2D |
| Ngôn ngữ | C# (Client + Server) |
| Platform | Windows, macOS, Android, iOS |
| Backend | ASP.NET Core 10 trên Railway |
| Database | MySQL 8.0 trên Railway |
| Nhân vật | Khủng long (nhiều skin) |
| Số level | Mở rộng linh hoạt qua Build Settings |

### Mô tả game

NotCrazy là game platformer 2D dành cho 1–2 người chơi. Người chơi điều khiển nhân vật khủng long vượt qua các màn chơi với bẫy, nền tảng di chuyển và vùng nguy hiểm. Game hỗ trợ chơi solo hoặc co-op online real-time với người khác thông qua mã phòng.

---

## 2. Công nghệ sử dụng

### Client (Unity)

| Công nghệ | Phiên bản | Mục đích |
|-----------|-----------|---------|
| Unity Engine | 2022 LTS+ | Game engine, rendering, physics |
| C# | 10+ | Ngôn ngữ lập trình |
| Unity Netcode for GameObjects (NGO) | 1.x | Framework multiplayer |
| Unity Relay | latest | Kết nối P2P không cần IP tĩnh |
| Unity Authentication | latest | Xác thực ẩn danh cho Relay |
| Unity Input System (New) | latest | Xử lý input đa nền tảng |
| TextMesh Pro | built-in | UI text chất lượng cao |
| Unity 2D SpriteShape | latest | Tạo địa hình 2D |
| ParrelSync | latest | Test multiplayer trong Editor |

### Backend (.NET API)

| Công nghệ | Phiên bản | Mục đích |
|-----------|-----------|---------|
| ASP.NET Core | 10.0 | Web API framework |
| Entity Framework Core | 9.0 | ORM, database access |
| Pomelo MySQL EF Core | 9.0 | MySQL driver cho EF Core |
| JWT Bearer | 9.0 | Xác thực API |
| BCrypt.Net | 4.2 | Mã hóa mật khẩu |
| Swashbuckle (Swagger) | 7.x | API documentation |
| Docker | latest | Container hóa ứng dụng |

### Infrastructure

| Dịch vụ | Mục đích |
|---------|---------|
| Railway | Hosting API + MySQL database |
| Docker Hub | Container registry |
| GitHub | Source control |
| Unity Gaming Services | Relay, Authentication |

---

## 3. Kiến trúc hệ thống

```
┌─────────────────────────────────────────────────────┐
│                  GAME CLIENT (Unity)                 │
│                                                      │
│  PlayerMovement → NGO Sync → Render                 │
│  AuthUIManager  → ApiManager → Railway API          │
│  SaveManager    → Local JSON File                   │
└───────────┬─────────────────────┬───────────────────┘
            │                     │
    ┌───────▼──────┐    ┌────────▼────────┐
    │ Unity Relay  │    │  Railway API    │
    │ (P2P bridge) │    │  ASP.NET Core   │
    └───────┬──────┘    └────────┬────────┘
            │                    │
    ┌───────▼──────┐    ┌────────▼────────┐
    │  Host máy    │    │  MySQL Database │
    │  người chơi1 │    │  Railway Cloud  │
    └──────────────┘    └─────────────────┘
```

### Phân tách trách nhiệm

- **Unity Client:** Gameplay, UI, input, local save, API calls
- **Unity Relay:** Cầu nối P2P giữa 2 máy (không chứa game logic)
- **NGO (Host):** Xử lý game logic multiplayer, sync object giữa các client
- **Railway API:** Auth, gold, progress, payment (dữ liệu bền vững)
- **MySQL:** Lưu trữ tài khoản, tiến độ, giao dịch

---

## 4. Nền tảng hỗ trợ

| Platform | Hỗ trợ | Ghi chú |
|----------|--------|---------|
| Windows | Có | Build Standalone x64 |
| macOS | Có | Build Standalone ARM64 (Apple Silicon) + x64 |
| Android | Có | Package: com.kin.notcrazy, min SDK 21 |
| iOS | Có | Package: com.kin.gamenotcrazy, min iOS 13 |
| iPadOS | Chưa hỗ trợ | Chưa tối ưu layout cho màn hình tablet |
| WebGL | Chưa hỗ trợ | NGO chưa hỗ trợ đầy đủ trên WebGL |
| Linux | Chưa kiểm thử | Unity hỗ trợ nhưng chưa test |

### Cấu hình xoay màn hình

Game chỉ hỗ trợ **landscape** (ngang), không hỗ trợ portrait (dọc):
- `Landscape Left` và `Landscape Right` được bật
- `Portrait` và `Portrait Upside Down` bị tắt

### Build iOS

- Yêu cầu Xcode Beta cho iOS 26.x beta
- Build từ Unity → export Xcode project → build trên Xcode → cài lên iPhone
- Signing: Apple Developer account (free tier cho test cá nhân, giới hạn 7 ngày)

---

## 5. Cấu trúc dự án

### Client (Unity)

```
Assets/Script/
├── API/
│   └── ApiModels.cs              — Request/Response DTOs
├── core/
│   ├── GameData.cs               — Biến toàn cục runtime
│   ├── SaveData.cs               — Cấu trúc dữ liệu save
│   ├── SaveManager.cs            — Đọc/ghi file JSON
│   ├── UserSession.cs            — Trạng thái đăng nhập runtime
│   ├── RoomData.cs               — Thông tin phòng multiplayer
│   ├── DinoSelectButton.cs       — Nút chọn nhân vật
│   └── NetworkErrorHandler.cs    — Xử lý lỗi mạng
├── manager/
│   ├── MainManager.cs            — UI flow, điều hướng panel
│   ├── LobbyManager.cs           — Đồng bộ lobby qua NetworkVariable
│   ├── LevelManager.cs           — Quản lý scene, respawn, skip
│   ├── AudioManager.cs           — Nhạc nền và SFX
│   └── ApiManager.cs             — HTTP client, gold polling
├── UI/
│   ├── AuthUIManager.cs          — Đăng nhập, đăng ký, profile
│   ├── TopUpPanel.cs             — Giao diện nạp xu
│   ├── DailyRewardPanel.cs       — Quà hàng ngày
│   ├── SkipChoicePanel.cs        — Chọn cách skip level
│   └── AdsPanel.cs               — Màn hình xem quảng cáo
├── utils/
│   ├── ButtonPressEffect.cs      — Hiệu ứng nhấn button
│   ├── ClickSFX.cs               — SFX khi click
│   ├── MuteButton.cs             — Toggle tắt/bật âm thanh
│   ├── ColorConfig.cs            — Hằng số màu sắc
│   └── CameraShake.cs            — Hiệu ứng rung camera
├── PlayerMovement.cs             — Di chuyển, nhảy, sync mạng
├── PlayerSkinManager.cs          — Đổi skin nhân vật
├── PlayerSpawner.cs              — Spawn người chơi
├── NetworkConfig.cs              — Cấu hình Relay, cleanup scene
├── LevelComplete.cs              — Xử lý hoàn thành level
├── LevelSelectManager.cs         — UI chọn level
├── TrapAction.cs                 — Cơ chế bẫy
├── TrapButton.cs                 — Nút kích hoạt bẫy
├── TrapTrigger.cs                — Zone kích hoạt bẫy
├── KillZoneReload.cs             — Vùng chết
├── PlatformSequence.cs           — Nền tảng di chuyển
├── PushBlock.cs                  — Khối đẩy được
├── SceneTransition.cs            — Animation chuyển scene
├── MobileControlButton.cs        — Nút điều khiển mobile
└── FunctionPanelController.cs    — Pause/Exit gameplay
```

### Backend (.NET API)

```
GameNotCrazy.API/
├── Controllers/
│   ├── AuthController.cs         — Đăng nhập, đăng ký, Google OAuth
│   ├── UserController.cs         — Tiến độ, gold
│   ├── TopUpController.cs        — Tạo yêu cầu nạp xu
│   └── AdminTopUpController.cs   — Duyệt/từ chối nạp xu
├── DTOs/
│   ├── AuthResponse.cs
│   ├── LoginRequest.cs
│   ├── RegisterRequest.cs
│   ├── GoogleLoginRequest.cs
│   ├── UpdateProgressRequest.cs
│   ├── UpdateGoldRequest.cs      — Delta gold + SetGold absolute
│   └── TopUpDTOs.cs
├── models/
│   ├── User.cs
│   └── PaymentRequest.cs
├── data/
│   └── AppDbContext.cs           — EF Core DbContext
├── Services/
│   └── TokenService.cs           — Tạo JWT token
├── Migrations/                   — EF Core database migrations
├── Program.cs                    — Cấu hình app, auto migrate
├── appsettings.json              — Cấu hình mặc định (localhost)
└── appsettings.Development.json  — Railway connection string (gitignored)
```

---

## 6. Cơ chế gameplay

### Di chuyển nhân vật (`PlayerMovement.cs`)

- Di chuyển trái/phải bằng `Rigidbody2D.velocity`
- Nhảy với lực `AddForce` khi đang trên mặt đất
- Kiểm tra mặt đất bằng `Physics2D.OverlapCircle` tại chân nhân vật
- Vật liệu `NoFriction PhysicsMaterial2D` tự động gán khi di chuyển, tháo ra khi dừng (chống trượt)
- Hỗ trợ đồng thời: New Input System (bàn phím/gamepad) và nút mobile (`MobileControlButton`)
- **Multiplayer:** sync `isRunning` và `facingSign` qua `NetworkVariable`, position sync qua `NetworkTransform`

### Hệ thống bẫy (`TrapAction.cs`)

Mỗi bẫy có một hoặc nhiều `ActionType`:

| ActionType | Mô tả |
|-----------|-------|
| MoveToTarget | Di chuyển vật thể đến vị trí đích |
| MoveBackToStart | Di chuyển vật thể về vị trí ban đầu |
| SetObjectActive | Bật/tắt GameObject |
| SetColliderEnabled | Bật/tắt Collider2D |
| ReloadScene | Reload scene hiện tại |

Bẫy được kích hoạt bởi:
- `TrapButton.cs` — người chơi chạm/nhấn nút
- `TrapTrigger.cs` — người chơi bước vào zone

Trong multiplayer tất cả bẫy xử lý qua `ServerRpc` để đảm bảo đồng bộ.

### KillZone (`KillZoneReload.cs`)

Người chơi chạm → reload scene hiện tại. Multiplayer: server phát lệnh reload cho tất cả client.

### Nền tảng di chuyển (`PlatformSequence.cs`)

Nền tảng di chuyển tuần tự theo danh sách waypoints. Người chơi đứng trên nền tảng sẽ bị mang theo (platform carry).

### Hoàn thành level (`LevelComplete.cs`)

**Single Player:**
```
Chạm trigger → animation slide → lưu progress → load scene tiếp theo
```

**Multiplayer:**
```
Người chơi 1 chạm trigger → đánh dấu sẵn sàng
Người chơi 2 chạm trigger → đánh dấu sẵn sàng
Cả 2 sẵn sàng → Server phát animation cho cả 2 → load scene đồng bộ
```

Dùng `groupId` để tránh kích hoạt nhiều lần khi cả 2 chạm cùng lúc.

### Chọn level (`LevelSelectManager.cs`)

- Hiển thị số button bằng `totalLevel` (cấu hình trong Inspector)
- Button chỉ tương tác được nếu `level <= SaveManager.Data.highestLevel`
- Multiplayer: giới hạn theo `MinLevelLobby` (level thấp hơn giữa 2 người)
- Level tiếp theo lấy theo `buildIndex + 1` trong Build Settings

---

## 7. Hệ thống multiplayer

### Relay vs NGO

| | Unity Relay | Unity NGO |
|--|------------|-----------|
| Nhiệm vụ | Kết nối 2 máy với nhau | Sync object/data giữa các máy |
| Tầng | Transport (mạng) | Application (logic) |
| Không có thì sao | 2 máy không tìm thấy nhau | Kết nối được nhưng không biết sync gì |

### Mô hình Host-Client (Listen Server)

Người chơi 1 làm **Host** — vừa là server, vừa là player. Không có dedicated server riêng.

```
Người chơi 2 (Client)          Người chơi 1 (Host = Server + Player)
        │                                    │
        │ ── ServerRpc (input) ──────────►  │ xử lý logic
        │                                    │
        │ ◄─ ClientRpc / NetworkVariable ──  │ broadcast kết quả
```

**Lý do dùng Listen Server thay vì Dedicated Server:** tiết kiệm chi phí (không cần server game chạy 24/7), phù hợp với game co-op 2 người không yêu cầu chống gian lận cao.

### Luồng kết nối

```
Host:
  UnityServices.InitializeAsync()
  AuthenticationService.SignInAnonymouslyAsync()
  RelayService.CreateAllocationAsync(maxPlayers)
  RelayService.GetJoinCodeAsync(allocation.AllocationId)
  transport.SetHostRelayData(...)
  NetworkManager.StartHost()

Client:
  UnityServices.InitializeAsync()
  AuthenticationService.SignInAnonymouslyAsync()
  RelayService.JoinAllocationAsync(joinCode)
  transport.SetClientRelayData(...)
  NetworkManager.StartClient()
```

### Đồng bộ dữ liệu

| Loại | Cơ chế | Dùng khi |
|------|--------|---------|
| Position nhân vật | NetworkTransform | Liên tục, mọi frame |
| Trạng thái chạy/hướng | NetworkVariable | Khi thay đổi |
| Chọn dino | NetworkVariable | Một lần trong lobby |
| Kích hoạt bẫy | ServerRpc | Sự kiện rời rạc |
| Hoàn thành level | ClientRpc | Sự kiện quan trọng |
| Load scene | NetworkManager.SceneManager | Phải đồng bộ 100% |

### Chọn nhân vật trong lobby (`LobbyManager.cs`)

- `HostDino`: NetworkVariable, chỉ host ghi
- `ClientDino`: NetworkVariable, chỉ client ghi
- `MinLevelLobby`: level thấp hơn giữa 2 người → giới hạn level select
- Client không được chọn dino trùng với host
- Nút Play chỉ enable khi cả 2 đã chọn (chỉ host thấy)

### Xử lý host disconnect

Khi host tắt, client nhận `OnClientDisconnectCallback` → tự load về Menu.

---

## 8. Hệ thống tài khoản

### Đăng ký / Đăng nhập

- Email + mật khẩu (tối thiểu 6 ký tự)
- Mật khẩu lưu dưới dạng BCrypt hash (không lưu plaintext)
- Trả về JWT token khi đăng nhập thành công
- Hỗ trợ Google OAuth2 (cần cài Google Sign-In plugin)
- Chế độ khách (guest): chơi không cần tài khoản, không lưu tiến độ server

### JWT Token

- Thuật toán HS256, symmetric key
- Chứa: userId, email, name
- Gửi kèm mọi API request qua header `Authorization: Bearer {token}`
- Token được lưu local vào save file để auto-login lần sau

### Auto-login

```
App khởi động
    ↓
UserSession.TryRestore()
    ↓
Đọc token từ SaveData
    ↓
Nếu token tồn tại → khôi phục session, lấy gold từ server
Nếu không → hiện màn đăng nhập
```

### Profile panel

Hiển thị tên, email người dùng. Nút đăng xuất xóa token local và về màn đăng nhập.

---

## 9. Hệ thống kinh tế (Gold)

### Nguyên tắc

- **Server là nguồn dữ liệu chính xác (source of truth)**
- Local chỉ là cache để hiển thị nhanh
- Mọi thay đổi gold đều đồng bộ lên server ngay lập tức

### Luồng đồng bộ

```
Mở app
    └── GetGold từ server → cập nhật local + UI

Tiêu xu (skip level)
    └── Trừ local → gọi UpdateGold(-amount) lên server ngay

Nhận quà hàng ngày
    └── Cộng local → gọi UpdateGold(+reward) lên server ngay

Polling mỗi 30 giây (chỉ khi ở menu)
    └── GetGold từ server → nếu khác local → cập nhật local + UI
```

### Polling 30 giây

Giải quyết trường hợp admin duyệt nạp xu khi người chơi đang mở game. Tối đa 30 giây sau khi duyệt, gold sẽ tự cập nhật mà không cần restart game.

Polling chỉ chạy khi ở menu, dừng khi vào gameplay để không tốn request thừa.

### API endpoints gold

| Method | Endpoint | Mô tả |
|--------|---------|-------|
| GET | /api/user/gold | Lấy gold hiện tại |
| POST | /api/user/update-gold | Cộng/trừ gold (delta) |
| POST | /api/user/set-gold | Ghi gold tuyệt đối |

---

## 10. Hệ thống nạp tiền

### Gói nạp

| Gói | Số xu | Giá (VNĐ) |
|-----|-------|-----------|
| Gói nhỏ | 10 xu | 29.999 đ |
| Gói vừa | 100 xu | 279.999 đ |
| Gói lớn | 500 xu | 1.399.999 đ |

### Quy trình

```
1. Người chơi chọn gói → gọi POST /api/topup/create
2. Server tạo PaymentRequest (status=pending)
3. Server trả về mã NC (VD: NC1234) và số tiền
4. Game hiển thị mã NC để người chơi chuyển khoản thủ công
5. Admin kiểm tra chuyển khoản → duyệt qua trang PHP admin
6. Server cập nhật status=approved, cộng gold cho user
7. Polling 30s của game tự phát hiện gold thay đổi → cập nhật UI
```

### Mã NC

- Format: "NC" + 4 chữ số ngẫu nhiên (VD: NC1234)
- Unique trong các request đang pending
- Người chơi ghi mã này vào nội dung chuyển khoản để admin nhận biết

### Admin panel (`admin/topup.php`)

- Chạy local bằng `php -S localhost:8080`
- Danh sách tất cả yêu cầu nạp, có filter theo trạng thái
- Nút Duyệt / Từ chối (kèm lý do)
- Tự động refresh hiển thị thời gian thực

---

## 11. Quà đăng nhập hàng ngày

### Phần thưởng theo chuỗi ngày

| Ngày | Phần thưởng |
|------|------------|
| Ngày 1 | 1 xu |
| Ngày 2 | 1 xu |
| Ngày 3 | 1 xu |
| Ngày 4 | 1 xu |
| Ngày 5 | 2 xu |
| Ngày 6 | 2 xu |
| Ngày 7 | 5 xu |

Sau ngày 7 reset về ngày 1.

### Luồng hoạt động

```
Sau khi đăng nhập thành công
    ↓
DailyRewardPanel.TryAutoShow()
    ↓
Đọc LastClaimDate và StreakDay từ PlayerPrefs
    ├── Hôm nay đã nhận → không hiện
    ├── Hôm qua nhận → tăng streak, hiện panel
    └── Bỏ ngày / lần đầu → reset streak về 1, hiện panel
    ↓
Người chơi nhấn Nhận
    ↓
Cộng gold local + gọi UpdateGold(reward) lên server
Lưu LastClaimDate = hôm nay, StreakDay = currentDay
```

### Lưu trữ

Dùng `PlayerPrefs` — lưu trên thiết bị, không cần server. Nếu xóa app thì mất streak.

---

## 12. Hệ thống skip level

### Điều kiện

Sau khi chết **3 lần trở lên** trong cùng một level, nút Skip xuất hiện trên màn respawn.

### Lựa chọn skip

| Cách | Điều kiện | Kết quả |
|------|-----------|---------|
| Xem video quảng cáo | Còn lượt (tối đa 3 lượt/ngày) | Xem video 5 giây → skip miễn phí |
| Dùng xu | Đủ 10 xu | Trừ 10 xu → skip ngay |

### Luồng xem video

```
Nhấn "Xem video"
    ↓
AdsPanel hiện ra, progress bar chạy 5 giây
    ↓
Nút thoát xuất hiện sau 5 giây
    ↓
Nhấn thoát → LevelManager.OnAdsFinished()
    ↓
Tải level tiếp theo + reset death count
```

Lượt xem video reset mỗi ngày (lưu `PlayerPrefs`).

### Luồng dùng xu

```
Nhấn "Dùng xu (10)"
    ↓
UserSession.SpendGold(10)
    ├── Kiểm tra gold >= 10
    ├── Trừ local
    ├── Gọi UpdateGold(-10) lên server
    └── Trả về true
    ↓
LevelManager.OnAdsFinished() → load level tiếp theo
```

---

## 13. Lưu trữ dữ liệu

### Local (thiết bị)

**File:** `Application.persistentDataPath/save.json`

Đường dẫn thực tế:
- Windows: `%APPDATA%/../LocalLow/Kin/NotCrazy/save.json`
- macOS: `~/Library/Application Support/Kin/NotCrazy/save.json`
- Android: `/data/data/com.kin.notcrazy/files/save.json`
- iOS: trong sandbox của app

**Cấu trúc:**

```json
{
  "highestLevel": 3,
  "selectedDino": 1,
  "savedUserId": 2,
  "savedEmail": "user@example.com",
  "savedName": "Tên người chơi",
  "savedToken": "eyJ...",
  "savedHighestLevel": 3,
  "savedGold": 10
}
```

### Server (Railway MySQL)

Xem mục [Cơ sở dữ liệu](#15-cơ-sở-dữ-liệu).

### PlayerPrefs (thiết bị)

| Key | Nội dung |
|-----|---------|
| DailyLastClaimDate | Ngày nhận quà gần nhất (yyyy-MM-dd) |
| DailyStreakDay | Ngày streak hiện tại (1–7) |
| AdViewCount | Số lượt xem quảng cáo hôm nay |
| AdLastResetDate | Ngày reset lượt xem (yyyy-MM-dd) |

---

## 14. Backend API

Base URL: `https://dacs-gamepr-notcrazy-team-production.up.railway.app`

### Authentication

| Method | Endpoint | Mô tả | Auth |
|--------|---------|-------|------|
| POST | /api/auth/register | Đăng ký tài khoản | Không |
| POST | /api/auth/login | Đăng nhập | Không |
| POST | /api/auth/google | Đăng nhập Google OAuth | Không |

### User

| Method | Endpoint | Mô tả | Auth |
|--------|---------|-------|------|
| GET | /api/user/gold | Lấy gold hiện tại | Bearer token |
| POST | /api/user/set-gold | Ghi gold tuyệt đối | Bearer token |
| POST | /api/user/update-gold | Cộng/trừ gold (delta) | Bearer token |
| POST | /api/user/update-progress | Cập nhật level cao nhất | Bearer token |

### Top-up

| Method | Endpoint | Mô tả | Auth |
|--------|---------|-------|------|
| POST | /api/topup/create | Tạo yêu cầu nạp xu | Bearer token |

### Admin

| Method | Endpoint | Mô tả | Auth |
|--------|---------|-------|------|
| GET | /api/admin/topup/list | Danh sách yêu cầu nạp | Không (TODO) |
| POST | /api/admin/topup/approve/{code} | Duyệt yêu cầu | Không (TODO) |
| POST | /api/admin/topup/reject/{code} | Từ chối yêu cầu | Không (TODO) |

---

## 15. Cơ sở dữ liệu

### Bảng `users`

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| id | BIGINT | PK, AUTO_INCREMENT | Mã người dùng |
| email | VARCHAR(255) | NOT NULL, UNIQUE | Email đăng nhập |
| password_hash | VARCHAR(255) | NOT NULL | BCrypt hash |
| name | VARCHAR(255) | NOT NULL | Tên hiển thị |
| highest_level | INT | DEFAULT 1 | Level cao nhất |
| gold | INT | DEFAULT 0 | Số xu hiện có |
| is_no_ads | TINYINT(1) | DEFAULT 0 | Đã mua tắt quảng cáo |
| created_at | DATETIME | NOT NULL | Thời gian tạo |
| updated_at | DATETIME | NOT NULL | Cập nhật gần nhất |

### Bảng `payment_requests`

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| id | BIGINT | PK, AUTO_INCREMENT | Mã yêu cầu |
| user_id | BIGINT | NOT NULL, FK | Người dùng |
| code | VARCHAR(20) | NOT NULL, UNIQUE | Mã NC (VD: NC1234) |
| gold_amount | INT | NOT NULL | Số xu muốn nạp |
| price | INT | NOT NULL | Số tiền VNĐ |
| status | VARCHAR(20) | DEFAULT 'pending' | pending/approved/rejected |
| created_at | DATETIME | NOT NULL | Thời gian tạo |
| approved_at | DATETIME | NULL | Thời gian duyệt |
| note | VARCHAR(255) | NULL | Lý do từ chối |

---

## 16. Triển khai

### Backend (Railway)

```
GitHub push
    ↓
Railway phát hiện thay đổi
    ↓
Docker build (ASP.NET 8.0 image)
    ↓
Container chạy trên Railway
    ↓
Program.cs: db.Database.Migrate() → tự áp dụng migration
    ↓
API online tại Railway URL
```

**Environment variables trên Railway:**

| Variable | Mô tả |
|----------|-------|
| ConnectionStrings__DefaultConnection | MySQL connection string |
| Jwt__Key | Secret key ký JWT |
| Jwt__Issuer | Issuer JWT |
| Jwt__Audience | Audience JWT |

### Game Client

1. Unity: File → Build Settings → chọn platform
2. Thêm tất cả scene theo thứ tự (Menu, Level_01, Level_02, ...)
3. Build → file executable (PC) hoặc .apk (Android) hoặc Xcode project (iOS)

### iOS Build

1. Build Settings → iOS → Switch Platform
2. Player Settings → landscape only, bundle ID: com.kin.gamenotcrazy
3. Build → tạo Xcode project
4. Mở Xcode → chọn team → cắm iPhone → Run

---

## 17. So sánh công nghệ

### Multiplayer Framework

| | Unity NGO + Relay | Mirror | Photon PUN2 |
|--|------------------|--------|-------------|
| Chi phí | Miễn phí (có giới hạn) | Miễn phí | Tính phí theo CCU |
| Hỗ trợ chính thức | Unity | Cộng đồng | Có |
| Dedicated server | Không cần (Relay) | Cần | Có |
| Độ phức tạp | Trung bình | Thấp | Thấp |
| Lý do chọn | Official Unity, tích hợp sẵn Relay, free tier đủ dùng |

### Database

| | MySQL | PostgreSQL | MongoDB | SQLite |
|--|-------|-----------|---------|--------|
| Loại | Quan hệ | Quan hệ | Document | Quan hệ |
| ACID | Có | Có | Hạn chế | Có |
| Railway | Hỗ trợ tốt | Có | Không tích hợp sẵn | Local only |
| Lý do chọn | Quen thuộc, Railway hỗ trợ tốt, phù hợp dữ liệu có quan hệ |

### Hosting

| | Railway | Heroku | AWS EC2 | VPS |
|--|---------|--------|---------|-----|
| Độ phức tạp | Thấp | Thấp | Cao | Cao |
| Chi phí | Thấp | Cao | Trung bình | Trung bình |
| Auto deploy | Có (git push) | Có | Không | Không |
| Lý do chọn | Đơn giản nhất, tích hợp MySQL, auto deploy từ GitHub |

### Mã hóa mật khẩu

| | BCrypt | MD5 | SHA-256 | Argon2 |
|--|--------|-----|---------|--------|
| Kháng brute force | Cao | Thấp | Trung bình | Rất cao |
| Adaptive cost | Có | Không | Không | Có |
| Phổ biến | Cao | Không nên dùng | Không nên dùng | Cao |
| Lý do chọn | Tiêu chuẩn ngành, adaptive cost, BCrypt.Net library dễ tích hợp |

---

## 18. Hạn chế và hướng phát triển

### Hạn chế hiện tại

| Hạn chế | Mô tả |
|---------|-------|
| Chưa hỗ trợ iPadOS | Chưa tối ưu UI cho màn hình lớn |
| Admin chưa có auth | API admin không yêu cầu xác thực |
| Daily reward lưu local | Mất streak nếu xóa app hoặc đổi thiết bị |
| Lượt xem quảng cáo local | Reset nếu xóa app |
| Gold polling 30s | Tối đa 30 giây mới thấy gold sau khi admin duyệt |
| Không có leaderboard | Không so sánh điểm số giữa người chơi |
| Không có chat | Multiplayer không có giao tiếp trong game |
| Max 2 người chơi | NGO hỗ trợ nhiều hơn nhưng game chỉ thiết kế cho 2 |

### Hướng phát triển

- Thêm nhiều level, thêm loại bẫy mới
- Leaderboard theo thời gian hoàn thành level
- Chat voice/text trong multiplayer
- Tối ưu cho iPadOS
- Admin dashboard web đầy đủ
- Sync daily streak lên server
- Hệ thống vũ khí (đã có placeholder trong code)
- Thêm chế độ chơi (race, versus)
- Push notification khi admin duyệt nạp xu
- Xác thực admin cho API admin endpoints
- Thêm nhóm 2 - 4 người chơi, mở rộng map

---

*Cập nhật lần cuối: 2026-06-21*
