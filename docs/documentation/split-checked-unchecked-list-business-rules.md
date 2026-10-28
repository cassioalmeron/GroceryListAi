# Business Rules: Split Checked/Unchecked List

## Feature Overview

The grocery list is displayed in two clearly separated sections: **Pending** (unchecked) items at the top and **Completed** (checked) items at the bottom. Items move between sections automatically when their checked state changes.

---

## Business Rules

### 1. Section Order

- The **unchecked (pending)** section is always displayed first.
- The **checked (completed)** section is always displayed below the unchecked section.
- This order is fixed and cannot be changed by the user.

**Why:** Items still to be picked up are the primary concern during shopping. Completed items are secondary context.

### 2. Item Movement on Toggle

- When a user checks an item, it moves **immediately** from the unchecked section to the checked section.
- When a user unchecks an item, it moves **immediately** from the checked section back to the unchecked section.
- Movement is driven by the `checked` field in the local state after a successful API PATCH call.

**When it applies:** Any time the user clicks a checkbox on any item, in either section.

**Business outcome:** The user always sees an accurate, up-to-date view of what remains to be picked up vs. what is already in the cart.

### 3. Empty Sections

- If the unchecked section is empty and there are no loading or error states, the message _"No items pending. Add one above!"_ is shown.
- If the checked section is empty (no items have been checked), the checked section and its divider are **not rendered at all**.
- The unchecked section is always rendered (even when empty), as it contains the empty-state message.

**Why:** Hiding an empty completed section keeps the UI clean when the user hasn't started checking off items.

### 4. Loading State

- While an API operation is in progress (`loading = true`), all checkboxes and remove buttons are disabled in **both** sections.
- The item being acted upon is visually unchanged until the API call succeeds; state update (and therefore section move) happens only after success.

### 5. Error Handling

- If a toggle fails, the item does not move — the local state is not updated.
- The existing toast notification informs the user of the failure.

---

## Frontend Behavior Rules

| Situation | Behavior |
|-----------|----------|
| Item unchecked | Shown in top (pending) section |
| Item checked | Shown in bottom (completed) section with strikethrough text |
| No unchecked items | Empty state message shown in pending section |
| No checked items | Completed section and divider hidden entirely |
| API in progress | All checkboxes and remove buttons disabled in both sections |
| API toggle fails | Item stays in its current section; toast error shown |

---

## Technical Reference

### Data Model

```typescript
interface Todo {
  id: number
  description: string
  checked: boolean   // false → pending section, true → completed section
}
```

### Key Implementation Files

| File | Role |
|------|------|
| `Web/src/components/TodoList/index.tsx` | Derives `uncheckedTodos` and `checkedTodos` arrays; renders two `<ul>` elements |
| `Web/src/components/TodoList/style.css` | `.todo-sections`, `.section-divider`, `.checked-section` styles |
| `Web/api.ts` | `markItemAsChecked(id, checked)` — PATCH `/items/{id}/checked` |

### Relevant API Endpoint

| Method | Route | Description |
|--------|-------|-------------|
| PATCH | `/items/{id}/checked` | Updates the `checked` state of an item |

---

## User Scenario Examples

**Scenario 1 — Checking off an item:**
> The user has "Milk", "Eggs", and "Bread" in the pending list. They pick up Milk and check it. Milk moves to the completed section. The pending section now shows "Eggs" and "Bread".

**Scenario 2 — Undoing a check:**
> The user accidentally checked "Eggs". They uncheck it. "Eggs" moves back to the pending section.

**Scenario 3 — All items checked:**
> The user checks all items. The pending section shows "No items pending. Add one above!" and all items appear in the completed section below the divider.

**Scenario 4 — Fresh list:**
> No items have been checked. The pending section lists all items. The completed section and divider are not visible.
