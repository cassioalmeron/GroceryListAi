# Multiple Lists — Business Rules

Items are organised in lists (for example "General", "Pharmacy", "Party"). The user picks the current list from a dropdown on the Home page, and every action applies to that list.

## Lists

| Rule | Detail |
|---|---|
| There is always at least one list | The database migration creates a list named **General**. The last remaining list cannot be deleted (the API answers `400`, the UI shows an error toast). |
| Existing items | Items that existed before lists were introduced were moved into **General** by the migration. |
| Names | Required, 1–100 characters, surrounding spaces trimmed. Unique **ignoring case** — "pharmacy" and "Pharmacy" cannot coexist. |
| Renaming | A list can be renamed to any unused name, including its own name with different casing. |
| Deleting | Deleting a list **also deletes all its items**. The UI asks for confirmation naming the list first. |
| Order | Lists are shown in creation order. |

## Selected list

| Rule | Detail |
|---|---|
| Where it is kept | In the browser only (`localStorage`), per device. The server has no "current list". |
| First load / missing list | If no list was selected yet, or the saved list no longer exists, the first list is selected. |
| After creating | The new list becomes the selected one. |
| After deleting the selected list | The first remaining list becomes the selected one. |

## Items and the assistant

| Rule | Detail |
|---|---|
| Every item belongs to exactly one list | An item cannot exist without a list. |
| Typed actions | Add, delete, check and uncheck act on the items of the selected list. |
| Voice / chat commands | "Add milk", "remove milk", "check milk" act only on the selected list. The assistant cannot see, change or switch to another list. |
| Same item in two lists | Allowed. Checking "Milk" in one list leaves "Milk" in another list unchanged. |
| Switching lists | The item list reloads for the newly selected list. |

## Not supported

- Moving an item from one list to another.
- Creating, renaming, deleting or switching lists by voice.
- Lists per user (there are no user accounts).
