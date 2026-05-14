# 🚀 Unity Relay Setup Guide

## Prerequisites
- Unity 2021.3 or later
- Unity Netcode for GameObjects 2.11.2+
- Unity Services account

## 📦 1. Install Required Packages

### Via Unity Package Manager:
1. **Window** → **Package Manager**
2. **Click "+"** → **Add package by name**
3. **Install these packages:**
   - `com.unity.netcode.gametransport` (version 2.11.2)
   - `com.unity.services.multiplayer` (version 2.2.2)
   - `com.unity.services.authentication` (version 3.6.1)

## ⚙️ 2. Setup Unity Services

### Enable Services:
1. **Edit** → **Project Settings** → **Services**
2. **Create/Sign in** to Unity Services account
3. **Enable "Multiplayer"** service
4. **Enable "Relay"** in Multiplayer settings

### Get Project ID:
- Your Project ID will be auto-filled in Services window
- Note: `Project ID` for later reference

## 🎮 3. Scene Setup

### NetworkManager Setup:
1. **Create empty GameObject** named "NetworkManager"
2. **Add components:**
   - `NetworkManager` (from Netcode for GameObjects)
   - `UnityTransport` (from Netcode Game Transport)
   - `NetworkSetup` (our custom script)
   - `NetworkConfig` (our custom script)

### UI Setup:
1. **Create Canvas** with these elements:
   - **Host Button** → calls `NetworkUI.StartHost()`
   - **Join Button** → calls `NetworkUI.StartClient()`
   - **Join Code Input Field** (TMP_InputField)
   - **Status Text** (TMP_Text)
   - **Join Code Display** (TMP_Text)
   - **Use Relay Toggle** (set to ON by default)

2. **Add NetworkUI component** to Canvas
3. **Assign all UI references** in NetworkUI inspector

### MainManager Setup:
- Ensure MainManager is in scene
- Assign all panel references
- Assign dino sprite array
- Assign button references

## 🔧 4. Build Settings

### For Testing:
1. **File** → **Build Settings**
2. **Add both scenes:**
   - Main Menu scene
   - Level_01 scene
3. **Set Main Menu as Scene 0**

### Player Settings:
1. **Edit** → **Project Settings** → **Player**
2. **Other Settings** → **Configuration**
3. **Api Compatibility Level:** `.NET Standard 2.1`
4. **IL2CPP** (recommended for relay)

## 🧪 5. Testing Relay

### Local Testing (Same Machine):
1. **Build the game** (File → Build and Run)
2. **Run in Editor** as second instance
3. **Host creates room** → gets join code
4. **Client enters code** → joins successfully

### Online Testing:
1. **Build for target platform** (Windows/Mac)
2. **Run one instance as host**
3. **Run second instance as client**
4. **Use the join code** to connect

## 🔍 6. Troubleshooting

### Common Issues:

**"Relay service not available"**
- Check Unity Services are enabled
- Verify internet connection
- Check Project ID in Services window

**"Failed to create relay"**
- Ensure packages are installed correctly
- Check Unity version compatibility
- Verify authentication is working

**"Join code invalid"**
- Join codes expire after ~10 minutes
- Host must be running when client tries to join
- Check code is copied exactly (case-sensitive)

### Debug Logs:
- Enable **Development Build** in Build Settings
- Check **Player.log** for detailed errors
- Use **NetworkConfig** debug messages

## 📋 7. Code Flow

```
1. Player clicks "Host" → NetworkConfig.CreateRelay()
2. Relay service creates allocation → returns join code
3. Host displays join code to share
4. Client enters code → NetworkConfig.JoinRelay()
5. Relay connects client to host
6. Both players can see each other in game
```

## 🎯 8. Next Steps

- **Add player name system**
- **Implement chat system**
- **Add room browser**
- **Create matchmaking system**

---

**🎉 Relay setup complete! Your multiplayer game now supports online connections!**</content>
<parameter name="filePath">e:\DACS\DACS3\unity_main\notcrazy\RELAY_SETUP_GUIDE.md