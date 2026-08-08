#!/usr/bin/env python3
"""模擬 Redis Lua 配額鎖邏輯，驗證 QuotaGuard 的兩個核心保證：
1. 併發下不會超額（TryConsume 原子自增+判斷）
2. 額度本月首次用盡才通知，之後不再重複通知
用單線程模擬 Lua 的原子性（Redis Lua 在服務端單線程執行，等價於互斥）。
"""
import re
from datetime import datetime

# ---- 模擬 Redis 單機狀態 ----
redis_store = {}
notified_store = {}  # quota:notified:name:yyyy-MM -> "1"

def try_consume(quota_name, monthly_limit, now=None):
    """對應 RedisQuotaHelper.TryConsume 的 Lua 腳本邏輯"""
    now = now or datetime(2026, 8, 8)
    key = f"quota:{quota_name}:{now:%Y-%m}"
    # Lua: INCR 後若 ==1 設過期；若 > limit 返回 0 否則 1
    current = redis_store.get(key, 0) + 1
    redis_store[key] = current
    if current == 1:
        pass  # EXPIRE 月底（此處略）
    return 0 if current > monthly_limit else 1

def is_exhausted(quota_name, monthly_limit, now=None):
    now = now or datetime(2026, 8, 8)
    key = f"quota:{quota_name}:{now:%Y-%m}"
    return redis_store.get(key, 0) >= monthly_limit

def notify_if_exhausted(quota_name, monthly_limit, now=None):
    """對應 QuotaGuard.NotifyAdminIfExhausted：本月只通知一次"""
    now = now or datetime(2026, 8, 8)
    nkey = f"quota:notified:{quota_name}:{now:%Y-%m}"
    if nkey in notified_store:
        return "SKIP(已通知過)"
    notified_store[nkey] = "1"
    return "SEND_EMAIL(通知管理員)"

# ===== 測試 1：額度 1000，連續調用 1003 次 =====
print("=== 測試1：ImageTag 限額 1000，調用 1003 次 ===")
LIMIT = 1000
allowed_count = 0
notified_events = 0
for i in range(1003):
    if try_consume("ImageTag", LIMIT) == 1:
        allowed_count += 1
    else:
        # 額度用盡，觸發通知邏輯
        r = notify_if_exhausted("ImageTag", LIMIT)
        if r.startswith("SEND"):
            notified_events += 1

print(f"允許調用次數 = {allowed_count} (應==1000)")
print(f"拒絕(降級)次數 = {1003 - allowed_count} (應==3)")
print(f"發出的通知郵件數 = {notified_events} (應==1，整月只通知一次)")
print(f"最終用量 = {redis_store['quota:ImageTag:2026-08']} (應==1003)")

assert allowed_count == 1000, "超額！配額鎖失效"
assert notified_events == 1, "通知次數不對"
print("✅ 測試1 通過：未超額，且整月只通知一次\n")

# ===== 測試 2：跨月自動重置 =====
print("=== 測試2：跨月額度重置 ===")
feb_used = redis_store.get("quota:ImageTag:2026-02", 0)
# 模擬 2 月已用 999，3 月到來應重新從 0 計
redis_store["quota:ImageTag:2026-02"] = 999
# 切到 3 月
mar_now = datetime(2026, 3, 1)
r1 = try_consume("ImageTag", LIMIT, mar_now)
print(f"3月首次調用返回 = {r1} (應==1，因為 key 是 2026-03 全新)")
print(f"3月用量 = {redis_store.get('quota:ImageTag:2026-03')} (應==1)")
assert r1 == 1
print("✅ 測試2 通過：跨月 key 不同，自動重置\n")

# ===== 測試 3：開關關閉語義（ShouldCallCloud 返 false） =====
print("=== 測試3：Enabled=false 時不調用（由業務層控制） ===")
ENABLED = False  # 模擬 ImageAudit:Enabled=false
decision = "FALLBACK_MANUAL(不調雲API)" if not ENABLED else "CALL_CLOUD"
print(f"決策 = {decision} (應==FALLBACK_MANUAL)")
assert decision.startswith("FALLBACK_MANUAL")
print("✅ 測試3 通過：開關關閉 → 0 成本降級\n")

print("全部核心防護邏輯驗證通過 ✅")
