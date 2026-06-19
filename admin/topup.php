<?php
define('API_BASE', 'https://dacs-gamepr-notcrazy-team-production.up.railway.app');

// ── XỬ LÝ ACTION ─────────────────────────────────────────────────────────────
$message = '';
$messageType = '';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $action = $_POST['action'] ?? '';
    $code   = $_POST['code']   ?? '';
    $note   = $_POST['note']   ?? '';

    if ($action === 'approve') {
        $result = callApi("/api/admin/topup/approve/$code", 'POST');
        if ($result['ok']) {
            $message = "✅ Đã duyệt mã $code";
            $messageType = 'success';
        } else {
            $message = "❌ Lỗi: " . $result['body'];
            $messageType = 'error';
        }
    } elseif ($action === 'reject') {
        $result = callApi("/api/admin/topup/reject/$code", 'POST', ['note' => $note]);
        if ($result['ok']) {
            $message = "🚫 Đã từ chối mã $code";
            $messageType = 'success';
        } else {
            $message = "❌ Lỗi: " . $result['body'];
            $messageType = 'error';
        }
    }
}

// ── LẤY DANH SÁCH ────────────────────────────────────────────────────────────
$listResult = callApi('/api/admin/topup/list', 'GET');
$requests   = $listResult['ok'] ? json_decode($listResult['body'], true) : [];

// ── HÀM GỌI API ──────────────────────────────────────────────────────────────
function callApi(string $endpoint, string $method, array $data = []): array
{
    $ch = curl_init(API_BASE . $endpoint);
    curl_setopt_array($ch, [
        CURLOPT_RETURNTRANSFER => true,
        CURLOPT_TIMEOUT        => 10,
        CURLOPT_CUSTOMREQUEST  => $method,
    ]);
    if ($method === 'POST') {
        $json = json_encode($data);
        curl_setopt($ch, CURLOPT_POSTFIELDS, $json);
        curl_setopt($ch, CURLOPT_HTTPHEADER, [
            'Content-Type: application/json',
            'Content-Length: ' . strlen($json),
        ]);
    }
    $body = curl_exec($ch);
    $http = curl_getinfo($ch, CURLINFO_HTTP_CODE);
    return ['ok' => $http >= 200 && $http < 300, 'body' => $body];
}

function statusBadge(string $status): string
{
    return match ($status) {
        'pending'  => '<span class="badge pending">Chờ duyệt</span>',
        'approved' => '<span class="badge approved">Đã duyệt</span>',
        'rejected' => '<span class="badge rejected">Từ chối</span>',
        default    => "<span class=\"badge\">$status</span>",
    };
}

function formatPrice(int $price): string
{
    return number_format($price, 0, ',', '.') . ' đ';
}

function formatDate(?string $date): string
{
    if (!$date) return '—';
    return date('d/m/Y H:i', strtotime($date));
}
?>
<!DOCTYPE html>
<html lang="vi">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Admin – Nạp xu</title>
<style>
  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

  body {
    font-family: 'Segoe UI', sans-serif;
    background: #0f172a;
    color: #e2e8f0;
    min-height: 100vh;
    padding: 24px;
  }

  h1 {
    font-size: 1.6rem;
    font-weight: 700;
    margin-bottom: 20px;
    color: #f8fafc;
  }

  .flash {
    padding: 12px 16px;
    border-radius: 8px;
    margin-bottom: 20px;
    font-weight: 500;
  }
  .flash.success { background: #166534; border: 1px solid #22c55e; }
  .flash.error   { background: #7f1d1d; border: 1px solid #ef4444; }

  .card {
    background: #1e293b;
    border-radius: 12px;
    overflow: hidden;
    border: 1px solid #334155;
  }

  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.9rem;
  }

  thead { background: #0f172a; }

  th {
    padding: 12px 16px;
    text-align: left;
    font-weight: 600;
    color: #94a3b8;
    text-transform: uppercase;
    font-size: 0.75rem;
    letter-spacing: 0.05em;
  }

  td {
    padding: 12px 16px;
    border-top: 1px solid #334155;
    vertical-align: middle;
  }

  tr:hover td { background: #263044; }

  .badge {
    display: inline-block;
    padding: 3px 10px;
    border-radius: 999px;
    font-size: 0.78rem;
    font-weight: 600;
  }
  .badge.pending  { background: #78350f; color: #fcd34d; }
  .badge.approved { background: #14532d; color: #86efac; }
  .badge.rejected { background: #7f1d1d; color: #fca5a5; }

  .code {
    font-family: monospace;
    background: #334155;
    padding: 3px 8px;
    border-radius: 4px;
    font-size: 0.9rem;
    color: #67e8f9;
  }

  form { display: inline; }

  .btn {
    display: inline-block;
    padding: 6px 14px;
    border-radius: 6px;
    border: none;
    font-size: 0.82rem;
    font-weight: 600;
    cursor: pointer;
    transition: opacity 0.15s;
  }
  .btn:hover { opacity: 0.85; }
  .btn-approve { background: #16a34a; color: #fff; }
  .btn-reject  { background: #dc2626; color: #fff; }

  .note-input {
    background: #0f172a;
    border: 1px solid #475569;
    border-radius: 6px;
    color: #e2e8f0;
    padding: 5px 10px;
    font-size: 0.82rem;
    width: 160px;
    margin-right: 6px;
  }

  .empty {
    text-align: center;
    padding: 48px;
    color: #64748b;
  }

  .refresh {
    float: right;
    font-size: 0.85rem;
    color: #64748b;
    margin-top: 4px;
  }

  .refresh a { color: #38bdf8; text-decoration: none; }
  .refresh a:hover { text-decoration: underline; }
</style>
</head>
<body>

<h1>🎮 Admin – Quản lý nạp xu
  <span class="refresh"><a href="">↻ Làm mới</a> — <?= date('H:i:s') ?></span>
</h1>

<?php if ($message): ?>
  <div class="flash <?= $messageType ?>"><?= htmlspecialchars($message) ?></div>
<?php endif; ?>

<div class="card">
<?php if (empty($requests)): ?>
  <div class="empty">Chưa có yêu cầu nạp xu nào.</div>
<?php else: ?>
  <table>
    <thead>
      <tr>
        <th>#</th>
        <th>Mã</th>
        <th>User ID</th>
        <th>Xu</th>
        <th>Số tiền</th>
        <th>Trạng thái</th>
        <th>Tạo lúc</th>
        <th>Duyệt lúc</th>
        <th>Ghi chú</th>
        <th>Hành động</th>
      </tr>
    </thead>
    <tbody>
    <?php foreach ($requests as $r): ?>
      <tr>
        <td><?= $r['id'] ?></td>
        <td><span class="code"><?= htmlspecialchars($r['code']) ?></span></td>
        <td><?= $r['userId'] ?></td>
        <td><strong><?= $r['goldAmount'] ?></strong></td>
        <td><?= formatPrice($r['price']) ?></td>
        <td><?= statusBadge($r['status']) ?></td>
        <td><?= formatDate($r['createdAt']) ?></td>
        <td><?= formatDate($r['approvedAt']) ?></td>
        <td><?= htmlspecialchars($r['note'] ?? '—') ?></td>
        <td>
        <?php if ($r['status'] === 'pending'): ?>
          <form method="POST">
            <input type="hidden" name="action" value="approve">
            <input type="hidden" name="code"   value="<?= htmlspecialchars($r['code']) ?>">
            <button class="btn btn-approve" type="submit"
              onclick="return confirm('Duyệt mã <?= $r['code'] ?>?')">Duyệt</button>
          </form>
          &nbsp;
          <form method="POST">
            <input type="hidden" name="action" value="reject">
            <input type="hidden" name="code"   value="<?= htmlspecialchars($r['code']) ?>">
            <input type="text" name="note" class="note-input" placeholder="Lý do từ chối...">
            <button class="btn btn-reject" type="submit"
              onclick="return confirm('Từ chối mã <?= $r['code'] ?>?')">Từ chối</button>
          </form>
        <?php else: ?>
          <span style="color:#475569">—</span>
        <?php endif; ?>
        </td>
      </tr>
    <?php endforeach; ?>
    </tbody>
  </table>
<?php endif; ?>
</div>

</body>
</html>
