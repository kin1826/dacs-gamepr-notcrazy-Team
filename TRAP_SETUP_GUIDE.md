# Trap Setup Guide

## Phan 1: Trigger Lam Platform Move Va Roi Xuong DieZone

Muc tieu trap nay:

- Player di toi mot trigger vo hinh.
- Platform phia truoc di chuyen ra khoi vi tri ban dau.
- Player roi xuong DieZone.
- Scene reload lai.
- Dung duoc cho ca single player va multiplayer.

## 1. Tao Platform Can Di Chuyen

1. Tao hoac chon platform trong scene.
2. Dat ten vi du: `TrapPlatform`.
3. Dam bao platform co collider de player dung len duoc.
4. Dat platform o vi tri ban dau trong man choi.

## 2. Tao Target Cho Platform

1. Tao mot Empty GameObject.
2. Dat ten vi du: `TrapPlatform_Target`.
3. Keo object nay toi vi tri platform se di chuyen den.
4. Vi du: dat lech sang phai, sang trai, hoac xuong duoi de tao khoang trong cho player roi.
5. Object target chi lam moc vi tri, khong can renderer hay collider.

## 3. Gan TrapAction Cho Platform

Chon `TrapPlatform`, Add Component `TrapAction`.

Set cac field:

- `Action Type`: `MoveToTarget`
- `Trigger Only Once`: bat
- `Object To Move`: keo chinh `TrapPlatform` vao
- `Target`: keo `TrapPlatform_Target` vao
- `Move Speed`: vi du `8`

Neu dung multiplayer, them vao `TrapPlatform`:

- `NetworkObject`
- `NetworkTransform`

`NetworkObject` giup object duoc Netcode quan ly. `NetworkTransform` giup client thay platform di chuyen.

## 4. Tao Trigger Vo Hinh

1. Tao Empty GameObject.
2. Dat ten vi du: `TrapTrigger_PlatformMove`.
3. Add Component `BoxCollider2D`.
4. Bat `Is Trigger`.
5. Keo va scale collider nay toi vi tri player se cham vao de kich hoat trap.
6. Add Component `TrapTrigger`.

Set `TrapTrigger`:

- `Trigger Mode`: `Once`
- `Required Count`: `1`
- `Trigger Only Once`: bat
- `Actions`: size `1`
- `Element 0`: keo object `TrapPlatform` co component `TrapAction` vao

## 5. Tao DieZone Ben Duoi

1. Tao object `DieZone`, hoac dung prefab `Assets/Prefab/DieZone.prefab`.
2. Dat DieZone ben duoi khu vuc player se roi xuong.
3. Dam bao DieZone co:
   - `BoxCollider2D`
   - `Is Trigger` bat
   - Component `KillZoneReload`

Khi player cham DieZone:

- Single player: reload scene hien tai.
- Multiplayer: host/server reload scene cho ca team.

## 6. Test

Single player:

1. Play scene.
2. Cho player di toi trigger.
3. Platform se move toi target.
4. Player roi xuong DieZone.
5. Scene reload.

Multiplayer:

1. Host tao phong.
2. Client join phong.
3. Host bam Play vao level.
4. Mot trong hai player cham trigger.
5. Platform move tren ca hai may.
6. Neu ai roi xuong DieZone, scene reload cho ca team.

## Luu Y

- Neu client khong thay platform move, kiem tra `TrapPlatform` da co `NetworkObject` va `NetworkTransform` chua.
- Neu trigger khong kich hoat, kiem tra player co tag `Player` hoac co component `PlayerMovement`.
- Neu player roi xuong nhung scene khong reload, kiem tra DieZone co `KillZoneReload` va collider da bat `Is Trigger`.
- Neu platform move sai vi tri, kiem tra `TrapPlatform_Target` da dat dung cho chua.
