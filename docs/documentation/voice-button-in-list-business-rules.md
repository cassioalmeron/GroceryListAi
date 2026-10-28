# Voice Button in List — Business Rules

## Feature Overview

Users can interact with the grocery list by voice directly from the list view, without opening a chat panel. Clicking the microphone icon, speaking a command, and releasing sends the spoken text to the AI backend, which processes it and updates the list. The result is immediately reflected by refreshing the list.

---

## Business Rules

### Voice Input

- The microphone button is only shown when the browser supports the Web Speech API (`SpeechRecognition` / `webkitSpeechRecognition`). If the API is not available, the button is hidden entirely and the feature is silently unavailable.
- A single click starts listening. Another click (or natural end of speech) stops listening.
- Only one utterance is captured per activation (`continuous = false`). The user must click again to issue a second command.
- The recognized language follows the user's language preference stored in `localStorage` (key: `microphone_language`, default: `en-US`).
- If the microphone permission is denied by the browser, a toast error is shown and the button returns to idle.
- If no speech is detected, a toast warning is shown and the button returns to idle.

### Command Processing

- The transcribed text is sent as a single message to the `/chat` AI endpoint (same endpoint used by the Chat component).
- The AI backend is responsible for interpreting the command (add, remove, check/uncheck an item) and applying it to the list. The frontend does not parse intent.
- The entire AI response stream is consumed silently — the response text is not shown in the UI.
- After the stream completes, the list is refreshed by calling `GET /items` to reflect any changes the AI made.

### Loading State

- While a voice command is being processed (from transcript received to list refresh complete), the list enters a loading state: all inputs and buttons are disabled and the `loading` flag is set.
- The microphone button is disabled during loading, preventing concurrent commands.

### Error Handling

- If the `/chat` call or the subsequent list refresh fails, a toast error is shown: "Voice command failed. Please try again."
- The loading state is always cleared in the `finally` block, so the list is never left stuck in a disabled state.

### Chat Panel

- The Chat panel is no longer shown on the Home page. The `Chat` component is preserved in the codebase for potential future reuse.
- The `VoiceButton` component is independent of the `Chat` component. Both use the same `/chat` endpoint but operate independently.

---

## Frontend Behavior Rules

| Situation | Behavior |
|---|---|
| Speech API not available | Mic button hidden, no error shown |
| Mic permission denied | Toast error, button returns to idle |
| No speech detected | Toast warning, button returns to idle |
| Listening | Button turns red with pulse animation; "Listening… Speak now" indicator shown |
| Processing command | List disabled (loading state), mic button disabled |
| Command succeeds | List refreshes silently, no extra notification |
| Command fails (network/API) | Toast error, list re-enabled |

---

## Key Implementation Files

| File | Role |
|---|---|
| `Web/src/components/VoiceButton/index.tsx` | Reusable voice button — manages `SpeechRecognition` lifecycle, calls `onTranscript` on result |
| `Web/src/components/VoiceButton/style.css` | Mic button and recording indicator styles |
| `Web/src/components/icons.tsx` | `MicrophoneIcon` and `StopRecordingIcon` used by VoiceButton |
| `Web/src/components/TodoList/index.tsx` | Hosts `VoiceButton`; handles transcript → chat call → list refresh |
| `Web/src/components/Chat/index.tsx` | Also uses `VoiceButton` for its own voice input (shared component) |
| `Web/api.ts` | `chat()` function — streams AI response chunks |

---

## Example Scenarios

**Add an item by voice:**
> User clicks mic → says "Add milk" → transcript sent to AI → AI adds "Milk" to the list → list refreshes showing the new item.

**Remove an item by voice:**
> User clicks mic → says "Remove bread" → transcript sent to AI → AI deletes the matching item → list refreshes without "Bread".

**Check an item by voice:**
> User clicks mic → says "Mark eggs as done" → transcript sent to AI → AI sets `checked = true` on "Eggs" → list refreshes with "Eggs" moved to the Completed section.
