const API_BASE = "";

// helpers 
function setState(el, msg, kind = "info") {
    el.textContent = msg || "";
    el.style.padding = msg ? "8px 10px" : "0";
    el.style.margin = msg ? "10px 0" : "0";
    el.style.borderRadius = msg ? "6px" : "0";
    el.style.background =
        msg ? (kind === "error" ? "rgba(176,0,32,0.08)" : "rgba(44,107,237,0.08)") : "transparent";
    el.style.color = kind === "error" ? "#b00020" : "#222";
}

function escapeHtml(str) {
    return String(str ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}


function parseApiDate(value) {
    if (!value) return null;
    const s = String(value);
    const hasTz = /Z$|[+\-]\d{2}:\d{2}$/.test(s);
    return new Date(hasTz ? s : `${s}Z`);
}

function formatDateTime(iso) {
    const d = parseApiDate(iso);
    if (!d || Number.isNaN(d.getTime())) return "—";
    return d.toLocaleString("ru-RU", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
    });
}

async function apiFetch(path) {
    const res = await fetch(`${API_BASE}${path}`, {
        headers: { "Content-Type": "application/json" },
        method: "GET",
    });

    const text = await res.text();
    let data = null;
    try {
        data = text ? JSON.parse(text) : null;
    } catch {
        data = text;
    }

    return { ok: res.ok, status: res.status, data };
}

// DOM 
const els = {
    statusState: document.getElementById("status-report-state"),
    statusTbody: document.getElementById("status-report-tbody"),

    overdueState: document.getElementById("overdue-report-state"),
    overdueContent: document.getElementById("overdue-report-content"),

    avgState: document.getElementById("avg-report-state"),
    avgDays: document.getElementById("avg-completion-days"),
};

// render
 
function renderStatusSummary(dto) {
    // dto: { statusCounts: { New:2, InProgress:1, Done:0, Overdue:1 } }
    const counts = dto?.statusCounts || {};
    const rows = ["New", "InProgress", "Done", "Overdue"].map((s) => {
        const n = counts[s];
        return `<tr><td>${escapeHtml(s)}</td><td>${escapeHtml(n ?? 0)}</td></tr>`;
    });

    els.statusTbody.innerHTML = rows.join("");
}

function renderOverdueByAssignee(items) {
    // items: [{ assigneeName, overdueCount, tasks: [{id,title,dueDate}] }]
    if (!Array.isArray(items) || items.length === 0) {
        els.overdueContent.innerHTML = `<p>Нет данных</p>`;
        return;
    }

    const cards = items.map((a) => {
        const tasks = Array.isArray(a.tasks) ? a.tasks : [];
        const taskList = tasks.length
            ? `<ul class="report-task-list">
           ${tasks
                .map(
                    (t) => `<li>
                 <strong>#${escapeHtml(t.id)}</strong> — ${escapeHtml(t.title)}
                 <span class="muted"> (дедлайн: ${escapeHtml(formatDateTime(t.dueDate))})</span>
               </li>`
                )
                .join("")}
         </ul>`
            : `<p class="muted">Нет задач</p>`;

        return `
      <article class="report-card">
        <header class="report-card__header">
          <h3 class="report-card__title">${escapeHtml(a.assigneeName ?? "—")}</h3>
          <div class="report-card__badge">Просрочено: ${escapeHtml(a.overdueCount ?? tasks.length ?? 0)}</div>
        </header>
        ${taskList}
      </article>
    `;
    });

    els.overdueContent.innerHTML = cards.join("");
}

function renderAvgCompletion(dto) {
    // dto: { averageDays: number|null, message: string }
    if (!dto || typeof dto !== "object") {
        els.avgDays.textContent = "—";
        return;
    }

    if (dto.averageDays === null || dto.averageDays === undefined) {
        els.avgDays.textContent = "—";
    } else {
        els.avgDays.textContent = String(dto.averageDays);
    }

    if (dto.message) {
        setState(els.avgState, dto.message, dto.averageDays == null ? "info" : "info");
    }
}

// loaders 

async function loadStatusSummary() {
    setState(els.statusState, "Загрузка отчёта…");
    const r = await apiFetch("/api/reports/status-summary");
    if (!r.ok) {
        setState(els.statusState, `Ошибка (HTTP ${r.status})`, "error");
        return;
    }
    renderStatusSummary(r.data);
    setState(els.statusState, "Готово");
}

async function loadOverdueByAssignee() {
    setState(els.overdueState, "Загрузка отчёта…");
    const r = await apiFetch("/api/reports/overdue-by-assignee");
    if (!r.ok) {
        setState(els.overdueState, `Ошибка (HTTP ${r.status})`, "error");
        return;
    }
    renderOverdueByAssignee(r.data);
    setState(els.overdueState, "Готово");
}

async function loadAvgCompletionTime() {
    setState(els.avgState, "Загрузка отчёта…");
    const r = await apiFetch("/api/reports/avg-completion-time");
    if (!r.ok) {
        setState(els.avgState, `Ошибка (HTTP ${r.status})`, "error");
        return;
    }
    renderAvgCompletion(r.data);
    if (!r.data?.message) setState(els.avgState, "Готово");
}



(async function init() {
    await Promise.all([
        loadStatusSummary(),
        loadOverdueByAssignee(),
        loadAvgCompletionTime(),
    ]);
})();