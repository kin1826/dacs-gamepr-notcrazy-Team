# Unity Relay Setup Guide

## Muc Dich

Project nay dung Unity Relay de 2 may khac nhau co the ket noi online bang join code.

Host tao phong:

- Unity Services initialize.
- Authentication sign in anonymous.
- Relay tao allocation.
- Host lay join code.
- UnityTransport duoc set host relay data.
- `NetworkManager.StartHost()` chay.

Client join phong:

- Client nhap join code.
- Code duoc normalize: trim, uppercase, bo ky tu la.
- Relay join allocation.
- UnityTransport duoc set client relay data.
- `NetworkManager.StartClient()` chay.

## Package Dang Dung

Kiem tra trong `Packages/manifest.json`:

- `com.unity.netcode.gameobjects`
- `com.unity.services.multiplayer`
- `com.unity.services.authentication`
- `com.unity.transport`

## Unity Services Can Bat

Trong Unity:

1. `Edit > Project Settings > Services`
2. Dang nhap Unity account.
3. Link project voi Unity Cloud Project.
4. Bat Multiplayer/Relay service neu Unity yeu cau.
5. Dam bao ca host build va client build deu cung mot Unity Project ID.

## NetworkManager Setup

Object `NetworkManager` trong scene `Menu` nen co:

- `NetworkManager`
- `UnityTransport`
- `NetworkConfig`

Trong `NetworkManager`:

- `Network Transport`: tro toi `UnityTransport`
- `Player Prefab`: tro toi `Assets/Prefab/Player.prefab`
- `Enable Scene Management`: bat
- `Auto Spawn Player Prefab Client Side`: giu theo cau hinh hien tai neu dang chay on

## NetworkConfig

File: `Assets/Script/NetworkConfig.cs`

Nhiem vu:

- Tao singleton `NetworkConfig.Instance`
- Tu gan vao `NetworkManager` neu scene chua co
- `DontDestroyOnLoad` de NetworkManager song qua scene
- Khoi tao Unity Services
- Dang nhap anonymous
- Tao Relay allocation
- Join Relay allocation
- Destroy duplicate NetworkManager khi load gameplay scene

## Join Code

Khi host tao phong, xem Console dong:

```text
Share this Relay join code with the client: ABC123
```

Client nhap dung code nay.

Luu y:

- Join code chi ton tai khi host con dang chay.
- Neu host stop Play mode/build thi code het dung.
- Code co the expire sau mot thoi gian.
- Hai may phai dung cung Unity Cloud Project.
- Neu copy code co space/newline, project da normalize code truoc khi join.

## Loi Thuong Gap

### Not Found: join code not found

Nguyen nhan thuong gap:

- Nhap sai code.
- Host da tat game/stop Play mode.
- Host va client khac Unity Project ID.
- Code da expire.
- Client nhap code cu cua phong truoc.

Cach kiem tra:

1. Host tao lai phong moi.
2. Copy code moi trong Console hoac UI.
3. Client nhap lai code.
4. Dam bao host van dang o trong game.

### Failed to create relay

Kiem tra:

- Internet.
- Unity Services da link project.
- Authentication service co hoat dong.
- Package Unity Services Multiplayer/Authentication da cai.

### Client join duoc nhung khong vao scene

Kiem tra:

- Host moi duoc bam Play.
- Scene can load nam trong `Scenes In Build`.
- `NetworkManager.Enable Scene Management` da bat.

## Test Relay

Test 2 may:

1. May A mo game.
2. May A bam Multiplayer > Create Room.
3. May A chon dino.
4. May B mo game.
5. May B bam Multiplayer, nhap code cua May A.
6. May B chon dino.
7. May A thay dino cua May B.
8. May A bam Play.
9. Ca hai vao level.

Test Android:

- Co the test 1 Android + 1 Editor/PC.
- Android phai co internet.
- Neu UI bi doc, set orientation thanh Landscape Left trong Player Settings.

## Khi Nao Can Sua Relay Code?

Hien tai khong can sua neu chi choi 2 nguoi bang join code.

Chi can mo rong khi:

- Muon room browser.
- Muon matchmaking tu dong.
- Muon player name.
- Muon reconnect.
- Muon nhieu hon 2 nguoi.
