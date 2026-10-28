# Task: Voice Button in List

**Status:** Completed
**Feature Name:** voice-button-in-list
**Scope:** Frontend only

---

## Overview

Users want to interact with their grocery list by voice without opening a chat panel. Clicking a microphone icon in the list sends the spoken command directly to the AI backend, which processes it and updates the list (add, remove, or check items). The chat UI is removed from the home page but the Chat component is preserved for potential future use.

---

## Requirements

- A reusable `VoiceButton` component must be extracted from the mic logic in `Chat/index.tsx`.
- The `VoiceButton` component is placed in the `TodoList` header area, next to the text input.
- Clicking the mic in the list: listens → transcribes → sends to `/chat` → refreshes the list.
- The Chat panel is removed from `Home/index.tsx`; the page shows only the `TodoList`.
- The `Chat` component itself is NOT deleted; it is only removed from the Home page layout.
- The `Chat/index.tsx` replaces its inline mic logic with the shared `VoiceButton` component.

---

## Acceptance Criteria

- [x] A `VoiceButton` component exists at `Web/src/components/VoiceButton/index.tsx`.
- [x] `VoiceButton` accepts `onTranscript`, `disabled`, and optional `className` props.
- [x] `VoiceButton` manages its own listening state and speech recognition lifecycle.
- [x] `VoiceButton` shows the mic icon when idle and a stop icon when recording.
- [x] `VoiceButton` shows the animated recording indicator below the button while listening.
- [x] `Chat/index.tsx` uses `VoiceButton` instead of its previous inline mic implementation.
- [x] `TodoList/index.tsx` renders a `VoiceButton` next to the text input.
- [x] When a voice transcript is received in `TodoList`, it is sent to `/chat` via the existing `chat()` API call, and `refreshTodos()` is called after the stream completes.
- [x] `Home/index.tsx` no longer renders `<Chat />` and no longer needs the `todoListRef` / `handleChatResponseComplete` wiring.
- [x] `Home/styles.css` is simplified to a single-panel layout (no `left-panel` / `right-panel` split).
- [x] All existing functionality (toggle, remove, text add, loading states, toasts) continues to work.
- [x] Speech not supported → mic button is hidden or disabled with no crash.

---

## Affected Files

| File | Change |
|------|--------|
| `Web/src/components/VoiceButton/index.tsx` | **New** — reusable voice button component |
| `Web/src/components/VoiceButton/style.css` | **New** — styles for VoiceButton (mic, recording indicator) |
| `Web/src/components/Chat/index.tsx` | Replace inline mic logic with `<VoiceButton>` |
| `Web/src/components/TodoList/index.tsx` | Add `<VoiceButton>` next to the text input; call `chat()` on transcript |
| `Web/src/components/TodoList/style.css` | Minor additions for voice button placement inside the add-todo row |
| `Web/src/Pages/Home/index.tsx` | Remove `<Chat>` and related ref/callback wiring |
| `Web/src/Pages/Home/styles.css` | Simplify to single-panel layout |

---

## Code Analysis

### VoiceButton Component (New)

```tsx
// Web/src/components/VoiceButton/index.tsx
interface VoiceButtonProps {
  onTranscript: (text: string) => void;
  disabled?: boolean;
  className?: string;
}
```

Logic extracted verbatim from `Chat/index.tsx`:
- `useRef<any>` for the recognition instance
- `useState` for `isListening` and `speechSupported`
- `useEffect` initializes `SpeechRecognition`, sets `lang` from `getLanguage()`, wires `onresult` / `onerror` / `onend`
- `onresult` calls `props.onTranscript(transcript)` then sets `isListening(false)`
- Button click toggles `start()` / `stop()`
- If `!speechSupported` → `return null` (button hidden) or render disabled

Styles moved from `Chat/style.css` into `VoiceButton/style.css`:
- `.microphone-button`, `.microphone-button.recording`
- `.recording-status`, `.recording-indicator`, `@keyframes recording-pulse`
- `@keyframes pulse-red`

### Chat/index.tsx Changes

Remove:
- `isListening`, `speechSupported` state
- `recognitionRef`
- `startVoiceRecording`, `stopVoiceRecording` functions
- The `useEffect` block that sets up `SpeechRecognition`
- The `<button className="microphone-button">` JSX
- The `{Boolean(isListening) && <div className="recording-status">}` JSX

Add:
```tsx
import VoiceButton from '../VoiceButton';
// ...
<VoiceButton
  onTranscript={(text) => handleSendMessageWithText(text)}
  disabled={isTyping}
/>
```

Keep `handleSendMessageWithText` — `VoiceButton` calls `onTranscript` and Chat calls its own send logic.

Remove mic-related CSS blocks from `Chat/style.css` (they move to `VoiceButton/style.css`).

### TodoList/index.tsx Changes

Add imports:
```tsx
import VoiceButton from '../VoiceButton';
import { chat } from '../../../api';
```

Add handler:
```tsx
const handleVoiceTranscript = async (text: string) => {
  try {
    setLoading(true);
    await chat(text, () => {}); // consume stream, ignore chunks
    await loadTodos();          // refresh list after AI processes the command
  } catch (err) {
    toast.error('Voice command failed. Please try again.');
  } finally {
    setLoading(false);
  }
};
```

Add `<VoiceButton>` inside `.add-todo`:
```tsx
<div className="add-todo">
  <input ... />
  <VoiceButton onTranscript={handleVoiceTranscript} disabled={loading} />
  <button onClick={addTodo} ...>Add</button>
</div>
```

### Home/index.tsx Changes

Before:
```tsx
const todoListRef = useRef<{ refreshTodos: () => void }>(null)
const handleChatResponseComplete = () => { todoListRef.current?.refreshTodos() }

<div className="home-panels">
  <div className="left-panel"><TodoList ref={todoListRef} /></div>
  <div className="right-panel"><Chat onResponseComplete={handleChatResponseComplete} /></div>
</div>
```

After:
```tsx
<div className="home-content">
  <TodoList />
</div>
```

`TodoList` no longer needs `forwardRef` / `useImperativeHandle` either — but keep them for now since the Chat component (if re-added in the future) could still call `refreshTodos`. The ref simply won't be used from Home.

### Home/styles.css Changes

Remove `.home-panels`, `.left-panel`, `.right-panel` and their media queries.
Keep `.home-container` and `.home-content` with appropriate `max-width` and padding.

---

## Implementation Notes

- The `VoiceButton` is self-contained: it manages its own `SpeechRecognition` lifecycle and never exposes recognition internals to the parent.
- `TodoList` does not maintain a chat-style conversation history — it always sends a single `message` to `/chat`. The AI backend already knows the current list state (it reads from the DB), so no history context is needed for simple voice commands.
- Do not delete `Chat/index.tsx` or `Chat/style.css` — they may be reused later.
- The `TodoList` `forwardRef` / `useImperativeHandle` can stay as-is; they just won't be called from Home anymore.
- Mic styles that are shared (button shape, recording indicator) move entirely to `VoiceButton/style.css`. The Chat component imports `VoiceButton` which brings its own styles.
