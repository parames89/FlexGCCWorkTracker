
import { useCallback, useEffect, useState } from "react";
import "./App.css";

const API_URL = "https://localhost:7099/api/WorkRequests";

const STATUSES = [
  { value: 0, label: "New" },
  { value: 1, label: "In Progress" },
  { value: 2, label: "Blocked" },
  { value: 3, label: "Completed" },
];

const PRIORITIES = [
  { value: 0, label: "Low" },
  { value: 1, label: "Medium" },
  { value: 2, label: "High" },
];

const EMPTY_FORM = {
  title: "",
  clientName: "",
  description: "",
  priority: 1,
  status: 0,
  dueDate: "",
};

function statusLabel(value) {
  return STATUSES.find((item) => item.value === Number(value))?.label ?? value;
}

function priorityLabel(value) {
  return PRIORITIES.find((item) => item.value === Number(value))?.label ?? value;
}

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : "—";
}

function toDateInput(value) {
  return value ? String(value).slice(0, 10) : "";
}

async function apiRequest(url, options = {}) {
  const response = await fetch(url, {
    ...options,
    headers: {
      ...(options.body ? { "Content-Type": "application/json" } : {}),
      ...options.headers,
    },
  });

  if (!response.ok) {
    let message = `Request failed (${response.status})`;

    try {
      const body = await response.json();
      message =
        body.message ||
        body.title ||
        (body.errors ? Object.values(body.errors).flat().join(" ") : "") ||
        message;
    } catch {
      // The response may have no JSON body.
    }

    throw new Error(message);
  }

  if (response.status === 204) return null;
  return response.json();
}

function App() {
  const [requests, setRequests] = useState([]);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [formOpen, setFormOpen] = useState(false);
  const [editingRequest, setEditingRequest] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);

  const [notesRequest, setNotesRequest] = useState(null);
  const [notes, setNotes] = useState([]);
  const [noteContent, setNoteContent] = useState("");
  const [notesLoading, setNotesLoading] = useState(false);
  const [noteSaving, setNoteSaving] = useState(false);

  const loadRequests = useCallback(async () => {
    setLoading(true);
    setError("");

    try {
      const params = new URLSearchParams({
        pageNumber: String(pageNumber),
        pageSize: String(pageSize),
        sortBy: "CreatedDate",
        descending: "true",
      });

      if (search.trim()) params.set("search", search.trim());
      if (statusFilter !== "") params.set("status", statusFilter);

      const result = await apiRequest(`${API_URL}?${params}`);

      setRequests(result.data ?? []);
      setTotalPages(result.totalPages ?? 0);
      setTotalCount(result.totalCount ?? 0);
    } catch (err) {
      setError(
        `${err.message}. Check that the API is running and CORS/HTTPS are configured.`
      );
    } finally {
      setLoading(false);
    }
  }, [search, statusFilter, pageNumber, pageSize]);

  useEffect(() => {
    const timer = setTimeout(() => {
      loadRequests();
    }, 250);

    return () => clearTimeout(timer);
  }, [loadRequests]);

  function openCreateForm() {
    setEditingRequest(null);
    setForm(EMPTY_FORM);
    setError("");
    setSuccess("");
    setFormOpen(true);
  }

  function openEditForm(request) {
    setEditingRequest(request);
    setForm({
      title: request.title ?? "",
      clientName: request.clientName ?? "",
      description: request.description ?? "",
      priority: Number(request.priority),
      status: Number(request.status),
      dueDate: toDateInput(request.dueDate),
    });
    setError("");
    setSuccess("");
    setFormOpen(true);
  }

  function closeForm() {
    setFormOpen(false);
    setEditingRequest(null);
    setForm(EMPTY_FORM);
  }

  function handleFormChange(event) {
    const { name, value } = event.target;
    setForm((previous) => ({
      ...previous,
      [name]: ["priority", "status"].includes(name) ? Number(value) : value,
    }));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setError("");
    setSuccess("");

    if (form.title.trim().length < 3) {
      setError("Title must contain at least 3 characters.");
      return;
    }

    if (!form.clientName.trim()) {
      setError("Client name is required.");
      return;
    }

    if (form.description.trim().length < 5) {
      setError("Description must contain at least 5 characters.");
      return;
    }

    if (!form.dueDate) {
      setError("Due date is required.");
      return;
    }

    const payload = {
      title: form.title.trim(),
      clientName: form.clientName.trim(),
      description: form.description.trim(),
      priority: Number(form.priority),
      status: Number(form.status),
      dueDate: `${form.dueDate}T00:00:00`,
    };

    setSaving(true);

    try {
      if (editingRequest) {
        await apiRequest(`${API_URL}/${editingRequest.id}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
        setSuccess("Work request updated successfully.");
      } else {
        await apiRequest(API_URL, {
          method: "POST",
          body: JSON.stringify(payload),
        });
        setSuccess("Work request created successfully.");
        setPageNumber(1);
      }

      closeForm();
      await loadRequests();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  async function handleStatusChange(request, newStatus) {
    const previousStatus = Number(request.status);
    const nextStatus = Number(newStatus);

    if (previousStatus === nextStatus) return;

    setError("");
    setSuccess("");

    setRequests((previous) =>
      previous.map((item) =>
        item.id === request.id ? { ...item, status: nextStatus } : item
      )
    );

    try {
      await apiRequest(`${API_URL}/${request.id}/status`, {
        method: "PATCH",
        body: JSON.stringify({ status: nextStatus }),
      });

      setSuccess(`Status updated for request #${request.id}.`);
      await loadRequests();
    } catch (err) {
      setError(err.message);
      await loadRequests();
    }
  }

  async function deleteRequest(request) {
    const confirmed = window.confirm(
      `Delete work request "${request.title}"? Its notes may also be deleted.`
    );

    if (!confirmed) return;

    setError("");
    setSuccess("");

    try {
      await apiRequest(`${API_URL}/${request.id}`, { method: "DELETE" });
      setSuccess("Work request deleted.");
      await loadRequests();
    } catch (err) {
      setError(err.message);
    }
  }

  async function openNotes(request) {
    setNotesRequest(request);
    setNotes([]);
    setNoteContent("");
    setError("");
    setNotesLoading(true);

    try {
      const result = await apiRequest(`${API_URL}/${request.id}/Notes`);
      setNotes(Array.isArray(result) ? result : []);
    } catch (err) {
      setError(`Could not load notes: ${err.message}`);
    } finally {
      setNotesLoading(false);
    }
  }

  async function addNote(event) {
    event.preventDefault();

    if (!notesRequest || !noteContent.trim()) {
      setError("Please enter a note.");
      return;
    }

    setNoteSaving(true);
    setError("");

    try {
      await apiRequest(`${API_URL}/${notesRequest.id}/Notes`, {
        method: "POST",
        body: JSON.stringify({ content: noteContent.trim() }),
      });

      setNoteContent("");
      const result = await apiRequest(
        `${API_URL}/${notesRequest.id}/Notes`
      );
      setNotes(Array.isArray(result) ? result : []);
      setSuccess("Note added successfully.");
    } catch (err) {
      setError(err.message);
    } finally {
      setNoteSaving(false);
    }
  }

  function changePage(nextPage) {
    setPageNumber(Math.max(1, Math.min(nextPage, totalPages)));
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand-mark">WR</div>
        <div className="brand-copy">
          <strong>FlexGCC</strong>
          <span>Work Request Tracker</span>
        </div>
        <div className="topbar-right">
          <span className="live-dot" />
          API workspace
        </div>
      </header>

      <main className="main-content">
        <section className="page-heading">
          <div>
            <span className="eyebrow">WORKSPACE / REQUESTS</span>
            <h1>Work requests</h1>
            <p>Manage client tasks, track progress, and keep notes in one place.</p>
          </div>
          <button className="primary-button" onClick={openCreateForm}>
            <span aria-hidden="true">＋</span> New request
          </button>
        </section>

        <section className="summary-grid">
          <article className="summary-card">
            <span className="summary-icon purple">▤</span>
            <span className="summary-label">Total matching requests</span>
            <strong>{totalCount}</strong>
          </article>
          <article className="summary-card">
            <span className="summary-icon blue">◷</span>
            <span className="summary-label">Current page</span>
            <strong>{totalPages ? pageNumber : 0} <small>/ {totalPages}</small></strong>
          </article>
          <article className="summary-card">
            <span className="summary-icon green">✓</span>
            <span className="summary-label">Page size</span>
            <strong>{pageSize}</strong>
          </article>
        </section>

        {error && (
          <div className="alert alert-error" role="alert">
            <strong>Something went wrong</strong>
            <span>{error}</span>
            <button className="alert-close" onClick={() => setError("")}>×</button>
          </div>
        )}

        {success && (
          <div className="alert alert-success" role="status">
            <span>{success}</span>
            <button className="alert-close" onClick={() => setSuccess("")}>×</button>
          </div>
        )}

        <section className="panel">
          <div className="panel-heading">
            <div>
              <h2>All requests</h2>
              <p>Search, filter, and manage your work items.</p>
            </div>
            <button className="secondary-button" onClick={loadRequests} disabled={loading}>
              ↻ Refresh
            </button>
          </div>

          <div className="toolbar">
            <label className="search-box">
              <span aria-hidden="true">⌕</span>
              <input
                value={search}
                onChange={(event) => {
                  setSearch(event.target.value);
                  setPageNumber(1);
                }}
                placeholder="Search title, client or description..."
                aria-label="Search work requests"
              />
              {search && (
                <button
                  type="button"
                  className="clear-search"
                  onClick={() => setSearch("")}
                  aria-label="Clear search"
                >
                  ×
                </button>
              )}
            </label>

            <label className="filter-box">
              <span>Status</span>
              <select
                value={statusFilter}
                onChange={(event) => {
                  setStatusFilter(event.target.value);
                  setPageNumber(1);
                }}
              >
                <option value="">All statuses</option>
                {STATUSES.map((item) => (
                  <option key={item.value} value={item.value}>
                    {item.label}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <div className="table-wrap">
            <table className="request-table">
              <thead>
                <tr>
                  <th>Request</th>
                  <th>Client</th>
                  <th>Priority</th>
                  <th>Status</th>
                  <th>Due date</th>
                  <th>Created</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {loading ? (
                  <tr>
                    <td colSpan="7" className="table-message">
                      <span className="spinner" /> Loading work requests...
                    </td>
                  </tr>
                ) : requests.length === 0 ? (
                  <tr>
                    <td colSpan="7" className="table-message">
                      <div className="empty-state">
                        <span>▤</span>
                        <strong>No requests found</strong>
                        <p>Try another search or create a new request.</p>
                        <button className="secondary-button" onClick={openCreateForm}>
                          Create request
                        </button>
                      </div>
                    </td>
                  </tr>
                ) : (
                  requests.map((request) => (
                    <tr key={request.id}>
                      <td>
                        <div className="request-title">
                          <span className="request-id">#{request.id}</span>
                          <strong>{request.title}</strong>
                          <span className="request-description">{request.description}</span>
                        </div>
                      </td>
                      <td>{request.clientName}</td>
                      <td>
                        <span className={`priority priority-${priorityLabel(request.priority).toLowerCase()}`}>
                          <span className="priority-dot" />
                          {priorityLabel(request.priority)}
                        </span>
                      </td>
                      <td>
                        <span className={`status-badge status-${statusLabel(request.status).toLowerCase().replace(/\s/g, "")}`}>
                          {statusLabel(request.status)}
                        </span>
                      </td>
                      <td>{formatDate(request.dueDate)}</td>
                      <td>{formatDate(request.createdDate)}</td>
                      <td>
                        <div className="action-buttons">
                          <select
                            className="status-select"
                            value={request.status}
                            onChange={(event) =>
                              handleStatusChange(request, event.target.value)
                            }
                            aria-label={`Change status for request ${request.id}`}
                            title="Change status"
                          >
                            {STATUSES.map((item) => (
                              <option key={item.value} value={item.value}>
                                {item.label}
                              </option>
                            ))}
                          </select>
                          <button
                            className="icon-button"
                            title="Edit request"
                            aria-label="Edit request"
                            onClick={() => openEditForm(request)}
                          >
                            ✎
                          </button>
                          <button
                            className="icon-button"
                            title="View notes"
                            aria-label="View notes"
                            onClick={() => openNotes(request)}
                          >
                            ▤
                          </button>
                          <button
                            className="icon-button danger-icon"
                            title="Delete request"
                            aria-label="Delete request"
                            onClick={() => deleteRequest(request)}
                          >
                            ×
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          <footer className="table-footer">
            <span>
              Showing {requests.length} of {totalCount} matching request(s)
            </span>
            <div className="pagination">
              <button
                className="secondary-button"
                disabled={pageNumber <= 1 || loading}
                onClick={() => changePage(pageNumber - 1)}
              >
                ← Previous
              </button>
              <span>Page {totalPages ? pageNumber : 0} of {totalPages}</span>
              <button
                className="secondary-button"
                disabled={pageNumber >= totalPages || loading}
                onClick={() => changePage(pageNumber + 1)}
              >
                Next →
              </button>
            </div>
          </footer>
        </section>

        <footer className="app-footer">
          FlexGCC Work Request Tracker <span>•</span> ASP.NET Core API + React
        </footer>
      </main>

      {formOpen && (
        <div className="modal-backdrop" onMouseDown={(event) => {
          if (event.target === event.currentTarget) closeForm();
        }}>
          <section className="modal" role="dialog" aria-modal="true" aria-labelledby="form-title">
            <div className="modal-heading">
              <div>
                <span className="eyebrow">{editingRequest ? "EDIT REQUEST" : "NEW ITEM"}</span>
                <h2 id="form-title">{editingRequest ? "Update request" : "Create work request"}</h2>
                <p>Enter the details for this work item.</p>
              </div>
              <button className="icon-button modal-close" onClick={closeForm} aria-label="Close form">×</button>
            </div>

            <form onSubmit={handleSubmit} className="request-form">
              <label>
                Title <span className="required">*</span>
                <input
                  name="title"
                  value={form.title}
                  onChange={handleFormChange}
                  required
                  minLength={3}
                  maxLength={200}
                  placeholder="e.g. Develop login API"
                />
              </label>

              <label>
                Client name <span className="required">*</span>
                <input
                  name="clientName"
                  value={form.clientName}
                  onChange={handleFormChange}
                  required
                  maxLength={150}
                  placeholder="Enter client name"
                />
              </label>

              <label>
                Description <span className="required">*</span>
                <textarea
                  name="description"
                  value={form.description}
                  onChange={handleFormChange}
                  required
                  minLength={5}
                  maxLength={2000}
                  rows={4}
                  placeholder="Describe the work request..."
                />
              </label>

              <div className="form-row">
                <label>
                  Priority
                  <select name="priority" value={form.priority} onChange={handleFormChange}>
                    {PRIORITIES.map((item) => (
                      <option key={item.value} value={item.value}>{item.label}</option>
                    ))}
                  </select>
                </label>

                <label>
                  Status
                  <select name="status" value={form.status} onChange={handleFormChange}>
                    {STATUSES.map((item) => (
                      <option key={item.value} value={item.value}>{item.label}</option>
                    ))}
                  </select>
                </label>
              </div>

              <label>
                Due date <span className="required">*</span>
                <input
                  type="date"
                  name="dueDate"
                  value={form.dueDate}
                  onChange={handleFormChange}
                  required
                />
              </label>

              <div className="modal-actions">
                <button type="button" className="secondary-button" onClick={closeForm} disabled={saving}>
                  Cancel
                </button>
                <button type="submit" className="primary-button" disabled={saving}>
                  {saving ? "Saving..." : editingRequest ? "Save changes" : "Create request"}
                </button>
              </div>
            </form>
          </section>
        </div>
      )}

      {notesRequest && (
        <div className="modal-backdrop" onMouseDown={(event) => {
          if (event.target === event.currentTarget) setNotesRequest(null);
        }}>
          <section className="modal notes-modal" role="dialog" aria-modal="true" aria-labelledby="notes-title">
            <div className="modal-heading">
              <div>
                <span className="eyebrow">REQUEST #{notesRequest.id}</span>
                <h2 id="notes-title">Notes</h2>
                <p>{notesRequest.title}</p>
              </div>
              <button className="icon-button modal-close" onClick={() => setNotesRequest(null)} aria-label="Close notes">×</button>
            </div>

            <form onSubmit={addNote} className="note-form">
              <label htmlFor="noteContent">Add a note</label>
              <textarea
                id="noteContent"
                value={noteContent}
                onChange={(event) => setNoteContent(event.target.value)}
                required
                minLength={1}
                maxLength={2000}
                rows={3}
                placeholder="Write an update or comment..."
              />
              <button className="primary-button" type="submit" disabled={noteSaving || !noteContent.trim()}>
                {noteSaving ? "Adding..." : "Add note"}
              </button>
            </form>

            <div className="notes-list">
              {notesLoading ? (
                <p className="muted">Loading notes...</p>
              ) : notes.length === 0 ? (
                <p className="muted">No notes yet. Add the first note above.</p>
              ) : (
                notes.map((note) => (
                  <article className="note-card" key={note.id}>
                    <p>{note.content}</p>
                    <span>{formatDate(note.createdDate)}</span>
                  </article>
                ))
              )}
            </div>
          </section>
        </div>
      )}
    </div>
  );
}

export default App;
