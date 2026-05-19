# TODO: Network Sync Fixes

## 1. Trap button and trap action synchronization
- [x] `TrapButton.cs`: button presses now send server RPC so the server owns the pressed state.
- [x] `TrapButton.cs`: server-side logic now updates and syncs visual `isPressed` state to all clients.
- [x] `TrapAction.cs`: changed `ActivateServerRpc` and `DeactivateServerRpc` to server-authoritative handler.

## 2. Player input ownership validation
- [x] `MobileControlButton.cs`: guard input calls so only the local owned player can respond.
- [x] `PlayerMovement.cs`: enforce `IsOwner` in `Jump()` and input methods.
- [ ] `PlayerMovement.cs`: consider local player prediction vs server authority for movement commands.

## 3. Player skin sync ownership
- [x] `PlayerSkinManager.cs`: guard `SetSkin()` so only the owner can submit `SubmitSkinServerRpc()`.

## 4. Level completion and door state sync
- [x] `LevelComplete.cs`: simplified and clarified the multiplayer vs single-player execution path in `OnTriggerEnter2D`/`OnTriggerExit2D`.
- [x] `LevelCompleteDoorGroup.cs`: network-sync secondary door visibility via `NetworkVariable`.

## 5. Platform and trap trigger RPC/ownership
- [x] `PlatformSequence.cs`: replaced custom `[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]` with standard `[ServerRpc(RequireOwnership = false)]` and server authority check.
- [x] `TrapTrigger.cs`: added null checks for `NetworkObject` and ensured valid player client ID lookup.

## 6. General checks
- [x] Validate all `NetworkBehaviour` methods use correct `IsOwner`/`IsServer` conditions in the core network scripts.
- [x] Confirm `NetworkManager.Singleton` is present before accessing client collections in the patched logic.
- [x] Review `NetworkVariable` usage for state that must be visible to all clients, including button press state and door visibility.
