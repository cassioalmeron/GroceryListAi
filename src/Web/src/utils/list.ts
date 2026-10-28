const LIST_STORAGE_KEY = 'selected_list_id';

export const getSelectedListId = (): number | null => {
  try {
    const saved = Number(localStorage.getItem(LIST_STORAGE_KEY));
    return Number.isInteger(saved) && saved > 0 ? saved : null;
  } catch {
    return null;
  }
};

export const setSelectedListId = (listId: number): void => {
  try {
    localStorage.setItem(LIST_STORAGE_KEY, String(listId));
  } catch {
    // Storage unavailable: the selection just will not be remembered.
  }
};
