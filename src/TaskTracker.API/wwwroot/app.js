const API_BASE = "https://localhost:7193";


const USERS = [
    { id: 1, name: "Иванов И.И." },
    { id: 2, name: "Петров П.П." },
    { id: 3, name: "Сидоров С.С." },
];

const STATUS_TEXT_TO_INT = { New: 0, InProgress: 1, Done: 2 };
const STATUS_INT_TO_TEXT = { 0: "New", 1: "InProgress", 2: "Done" };

const els = {
    filtersForm: document.getElementById("filters-form"),
    statusFilter: document.getElementById("status"),
    assigneeFilter: document.getElementById("assignee"),
    dueBefore: document.getElementById("dueBefore"),
    dueAfter: document.getElementById("dueAfter"),
    tagsFilter: document.getElementById("tags"),

    tbody: document.getElementById("tasks-tbody"),
    statusBox: document.getElementById("tasks-status"),

    taskForm: document.getElementById("task-form"),
    taskId: document.getElementById("taskId"),
    title: document.getElementById("title"),
    description: document.getElementById("description"),
    assigneeId: document.getElementById("assigneeId"),
    dueDate: document.getElementById("dueDate"),
    priority: document.getElementById("priority"),
    statusField: document.getElementById("statusField"),
    tagIds: document.getElementById("tagIds"),

    cancelEdit: document.getElementById("cancel-edit"),
};

// helpers

function setStatus(msg, kind = "info") {
    els.statusBox.textContent = msg || "";
    els.statusBox.style.padding = msg ? "8px 10px" : "0";
    els.statusBox.style.margin = msg ? "10px 0" : "0";
    els.statusBox.style.borderRadius = msg ? "6px" : "0";
    els.statusBox.style.background =
        msg ? (kind === "error" ? "rgba(176,0,32,0.08)" : "rgba(44,107,237,0.08)") : "transparent";
    els.statusBox.style.color = kind === "error" ? "#b00020" : "#222";
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

    // Если есть Z или смещение (+01:00 / -05:00) — парсим как есть
    const hasTz = /Z$|[+\-]\d{2}:\d{2}$/.test(s);
    return new Date(hasTz ? s : `${s}Z`); // иначе считаем, что это UTC
}


function toLocalDatetimeValue(iso) {
    if (!iso) return "";
    const d = parseApiDate(iso);
    const pad = (n) => String(n).padStart(2, "0");
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function formatDateTime(iso) {
    if (!iso) return "—";
    const d = parseApiDate(iso);
    return d.toLocaleString("ru-RU", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
    });
}

function formatPriority(p) {
    const map = { 1: "Low", 2: "Medium", 3: "High" };
    return map[Number(p)] ?? String(p ?? "—");
}

function statusClass(statusText) {
    switch (statusText) {
        case "New":
            return "status-new";
        case "InProgress":
            return "status-inprogress";
        case "Done":
            return "status-done";
        case "Overdue":
            return "status-overdue";
        default:
            return "";
    }
}

function getSelectedMultiValues(selectEl) {
    return Array.from(selectEl.selectedOptions).map((o) => o.value).filter((v) => v !== "");
}

function clearFieldErrors(formEl) {
    formEl.querySelectorAll(".field-error").forEach((e) => e.remove());
    formEl.querySelectorAll("[aria-invalid='true']").forEach((e) => e.setAttribute("aria-invalid", "false"));
}

function showFieldErrors(formEl, errorsObj) {
    // errorsObj: { Title: ["..."], DueDate: ["..."] }
    const nameMap = {
        Title: "title",
        Description: "description",
        AssigneeId: "assigneeId",
        DueDate: "dueDate",
        Status: "status",
        Priority: "priority",
        TagIds: "tagIds",
        Id: "id",
    };

    for (const [key, messages] of Object.entries(errorsObj || {})) {
        const fieldName = nameMap[key] || key;
        const input = formEl.querySelector(`[name="${CSS.escape(fieldName)}"]`);
        if (!input) continue;

        input.setAttribute("aria-invalid", "true");

        const div = document.createElement("div");
        div.className = "field-error";
        div.textContent = Array.isArray(messages) ? messages.join(" ") : String(messages);

        input.insertAdjacentElement("afterend", div);
    }
}

async function apiFetch(path, options = {}) {
    const res = await fetch(`${API_BASE}${path}`, {
        headers: {
            "Content-Type": "application/json",
            ...(options.headers || {}),
        },
        ...options,
    });

    if (res.status === 204) return { ok: true, status: 204, data: null };

    const text = await res.text();
    let data = null;
    try {
        data = text ? JSON.parse(text) : null;
    } catch {
        data = text;
    }

    return { ok: res.ok, status: res.status, data };
}

// справочники в UI

function fillUserSelects() {
    USERS.forEach((u) => {
        const opt = document.createElement("option");
        opt.value = String(u.id);
        opt.textContent = u.name;
        els.assigneeFilter.appendChild(opt);
    });

    USERS.forEach((u) => {
        const opt = document.createElement("option");
        opt.value = String(u.id);
        opt.textContent = u.name;
        els.assigneeId.appendChild(opt);
    });
}

// query filters

function buildTasksQueryFromFilters() {
    const params = new URLSearchParams();

    const status = els.statusFilter.value.trim(); // string: New/InProgress/Done/Overdue
    const assigneeId = els.assigneeFilter.value.trim();
    const dueBefore = els.dueBefore.value; // yyyy-MM-dd
    const dueAfter = els.dueAfter.value;
    const tagIds = getSelectedMultiValues(els.tagsFilter); // tagId=1&tagId=3

    if (status) params.set("status", status);
    if (assigneeId) params.set("assigneeId", assigneeId);
    if (dueBefore) params.set("dueBefore", dueBefore);
    if (dueAfter) params.set("dueAfter", dueAfter);
    tagIds.forEach((id) => params.append("tagId", id));

    const qs = params.toString();
    return qs ? `?${qs}` : "";
}

// render 

function renderTasks(tasks) {
    els.tbody.innerHTML = "";

    if (!Array.isArray(tasks) || tasks.length === 0) {
        els.tbody.innerHTML = `<tr><td colspan="7">Задач пока нет</td></tr>`;
        return;
    }

    for (const t of tasks) {
        // TaskDto:
        // id, title, description, assigneeId, assigneeName, createdAt, dueDate, completedAt,
        // status (string), priority (int), tags([...]), isOverdue (bool)
        const statusForDisplay = t.isOverdue ? "Overdue" : (t.status ?? "—");
        const tagsText = Array.isArray(t.tags) ? t.tags.map((x) => x.name ?? String(x)).join(", ") : "—";

        const tr = document.createElement("tr");
        tr.innerHTML = `
      <td>${escapeHtml(t.title ?? "")}</td>
      <td>${escapeHtml(t.assigneeName ?? (t.assigneeId ? `#${t.assigneeId}` : "—"))}</td>
      <td><span class="${statusClass(statusForDisplay)}">${escapeHtml(statusForDisplay)}</span></td>
      <td>${escapeHtml(formatDateTime(t.dueDate))}</td>
      <td>${escapeHtml(formatPriority(t.priority))}</td>
      <td>${escapeHtml(tagsText)}</td>
      <td>
        <button type="button" data-action="edit" data-id="${escapeHtml(String(t.id))}">Редактировать</button>
        <button type="button" data-action="delete" data-id="${escapeHtml(String(t.id))}">Удалить</button>
      </td>
    `;
        els.tbody.appendChild(tr);
    }
}

async function loadTasks() {
    setStatus("Загрузка задач…");
    const qs = buildTasksQueryFromFilters();

    const r = await apiFetch(`/api/tasks${qs}`, { method: "GET" });
    if (!r.ok) {
        setStatus(`Ошибка загрузки задач (HTTP ${r.status})`, "error");
        els.tbody.innerHTML = `<tr><td colspan="7">Не удалось загрузить задачи</td></tr>`;
        return;
    }

    renderTasks(r.data);
    setStatus(`Загружено задач: ${Array.isArray(r.data) ? r.data.length : 0}`);
}

// editor 

function resetEditor() {
    els.taskForm.reset();
    els.taskId.value = "";
    clearFieldErrors(els.taskForm);
    els.assigneeId.value = "";
}

function fillEditorFromTask(task) {
    clearFieldErrors(els.taskForm);

    els.taskId.value = task.id ?? "";
    els.title.value = task.title ?? "";
    els.description.value = task.description ?? "";

    els.assigneeId.value = task.assigneeId ? String(task.assigneeId) : "";
    els.dueDate.value = toLocalDatetimeValue(task.dueDate);

    els.priority.value = String(task.priority ?? 2);

    // TaskDto.status = string -> editor expects int (0..2)
    const statusText = task.status ?? "New";
    els.statusField.value = String(STATUS_TEXT_TO_INT[statusText] ?? 0);

    // TaskDto.tags = [{id,name}, ...] -> editor expects TagIds
    const selected = new Set(Array.isArray(task.tags) ? task.tags.map((x) => Number(x.id)) : []);
    Array.from(els.tagIds.options).forEach((opt) => {
        opt.selected = selected.has(Number(opt.value));
    });

    els.taskForm.scrollIntoView({ behavior: "smooth", block: "start" });
}

function buildTaskPayloadFromForm() {
    const dueLocal = els.dueDate.value; // yyyy-MM-ddTHH:mm
    const dueIso = dueLocal ? new Date(dueLocal).toISOString() : null;

    return {
        title: els.title.value.trim(),
        description: els.description.value.trim() || null,
        assigneeId: Number(els.assigneeId.value),
        dueDate: dueIso,
        priority: Number(els.priority.value),
        status: Number(els.statusField.value), // int 0..2 (как в UpdateTaskDto)
        tagIds: getSelectedMultiValues(els.tagIds).map(Number),
    };
}

// CRUD

async function getTaskById(id) {
    return await apiFetch(`/api/tasks/${encodeURIComponent(id)}`, { method: "GET" });
}

async function createTask(payload) {
    return await apiFetch("/api/tasks", { method: "POST", body: JSON.stringify(payload) });
}

async function updateTask(id, payload) {
    return await apiFetch(`/api/tasks/${encodeURIComponent(id)}`, { method: "PUT", body: JSON.stringify(payload) });
}

async function deleteTask(id) {
    return await apiFetch(`/api/tasks/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// events

els.filtersForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    await loadTasks();
});

els.filtersForm.addEventListener("reset", async () => {
    setTimeout(loadTasks, 0);
});

els.tbody.addEventListener("click", async (e) => {
    const btn = e.target.closest("button[data-action]");
    if (!btn) return;

    const action = btn.dataset.action;
    const id = btn.dataset.id;

    if (action === "edit") {
        setStatus("Загрузка задачи…");
        const r = await getTaskById(id);
        if (!r.ok) {
            setStatus(`Не удалось загрузить задачу (HTTP ${r.status})`, "error");
            return;
        }
        fillEditorFromTask(r.data);
        setStatus("Режим редактирования");
    }

    if (action === "delete") {
        if (!confirm("Удалить задачу? Действие необратимо.")) return;

        setStatus("Удаление…");
        const r = await deleteTask(id);
        if (!r.ok) {
            setStatus(`Не удалось удалить (HTTP ${r.status})`, "error");
            return;
        }
        setStatus("Задача удалена");
        await loadTasks();

        if (els.taskId.value && String(els.taskId.value) === String(id)) resetEditor();
    }
});

els.taskForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    clearFieldErrors(els.taskForm);

    const id = els.taskId.value.trim();
    const payload = buildTaskPayloadFromForm();

    setStatus(id ? "Сохранение изменений…" : "Создание задачи…");

    const r = id ? await updateTask(id, payload) : await createTask(payload);

    if (!r.ok) {
        if (r.status === 400 && r.data && typeof r.data === "object" && r.data.errors) {
            showFieldErrors(els.taskForm, r.data.errors);
            setStatus("Проверьте поля формы (ошибки подсвечены).", "error");
            return;
        }
        setStatus(`Ошибка сохранения (HTTP ${r.status})`, "error");
        return;
    }

    setStatus(id ? "Задача обновлена" : "Задача создана");
    resetEditor();
    await loadTasks();
});

els.cancelEdit.addEventListener("click", () => {
    resetEditor();
    setStatus("Редактор очищен");
});




(function init() {
    fillUserSelects();
    resetEditor();
    loadTasks();
})();
