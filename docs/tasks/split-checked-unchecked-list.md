# Task: Split Checked/Unchecked List

**Status:** Completed
**Feature Name:** split-checked-unchecked-list
**Scope:** Frontend only

---

## Overview

Currently the grocery list renders all items in a single flat list, regardless of their checked state. This task splits the list into two distinct sections so that unchecked (pending) items and checked (done) items are visually separated and items move between sections when their state changes.

---

## Requirements

- The list must be split into two sections: **Unchecked** (top) and **Checked** (bottom).
- When an item is checked, it moves immediately to the checked section.
- When an item is unchecked, it moves immediately back to the unchecked section.
- The unchecked section is always displayed first.
- Both sections remain functional: items can be toggled and removed from either section.

---

## Acceptance Criteria

- [x] Unchecked items are grouped in the top section.
- [x] Checked items are grouped in the bottom section.
- [x] Checking an item removes it from the unchecked section and places it in the checked section.
- [x] Unchecking an item removes it from the checked section and places it back in the unchecked section.
- [x] A clear visual separator or section heading distinguishes the two sections.
- [x] If one section is empty, it is either hidden or shows an appropriate empty state.
- [x] All existing functionality (toggle, remove, loading states, toasts) continues to work.

---

## Affected Files

| File | Change |
|------|--------|
| `Web/src/components/TodoList/index.tsx` | Split render into two filtered lists |
| `Web/src/components/TodoList/style.css` | Add section heading and divider styles |

---

## Skills Involved

- `frontend-design` — UI/UX implementation in React + CSS

---

## Code Analysis

### Affected Files

| File | Lines | Change Type |
|------|-------|-------------|
| `Web/src/components/TodoList/index.tsx` | 163–183 (render) | Modify render — split one `<ul>` into two filtered lists |
| `Web/src/components/TodoList/style.css` | New classes only | Add `.section-heading` and `.checked-section` styles |

### Current State (Before)

```tsx
// index.tsx lines 163–183
<ul className="todo-list">
  {todos.map(todo => (
    <li key={todo.id} className={`todo-item ${todo.checked ? 'completed' : ''}`}>
      ...
    </li>
  ))}
</ul>

{todos.length === 0 && !loading && !error && (
  <p className="empty-message">No todos yet. Add one above!</p>
)}
```

All items render in a single list. Checked state only affects CSS (opacity + strikethrough), not position.

### Proposed Change (After)

```tsx
// Derived arrays (no new state needed)
const uncheckedTodos = todos.filter(t => !t.checked)
const checkedTodos = todos.filter(t => t.checked)

// Render two sections
<div className="todo-sections">
  <ul className="todo-list">
    {uncheckedTodos.map(todo => <li .../>)}
  </ul>
  {uncheckedTodos.length === 0 && !loading && !error && (
    <p className="empty-message">No items yet. Add one above!</p>
  )}

  {checkedTodos.length > 0 && (
    <>
      <div className="section-divider">
        <span>Completed</span>
      </div>
      <ul className="todo-list checked-section">
        {checkedTodos.map(todo => <li .../>)}
      </ul>
    </>
  )}
</div>
```

### Why Changes Are Needed

The current flat list makes it hard to distinguish pending vs. completed items — they are mixed together and only differ visually by opacity. Separating them improves scannability and makes the grocery workflow clearer.

### Benefits

- Items to buy are always at the top, clearly separated from what's already in the cart.
- No new API calls or state changes required — only the render layer changes.

## Implementation Notes

- No new state or API changes required. The `todos` array is filtered into `uncheckedTodos` and `checkedTodos` directly in the render function using an IIFE.
- A shared `renderItem` helper avoids duplicating the `<li>` markup.
- The checked section (including divider) is conditionally rendered only when `checkedTodos.length > 0`.
- The `.todo-sections` wrapper takes over the `flex: 1 / overflow-y: auto` responsibility previously on `.todo-list`, allowing both lists to scroll together naturally.
- Business rules documented in: `docs/documentation/split-checked-unchecked-list-business-rules.md`
