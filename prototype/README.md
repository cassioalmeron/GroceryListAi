# Grocery List AI - UI Prototypes

This folder contains static HTML prototypes of the Grocery List AI web interface. These prototypes are built using pure HTML, Tailwind CSS (via CDN), and vanilla JavaScript.

## Files

- **index.html** - Home page prototype with Todo List and AI Chat interface
- **settings.html** - Settings page prototype with configuration options
- **README.md** - Documentation (this file)

## Features

### Navigation

**Left Sidebar Menu:**
- Fixed sidebar on desktop (sticky positioning)
- Slide-out menu on mobile devices
- Home and Settings links with icons
- Active page highlighting (blue background)
- App branding/logo at the top
- Version information and last update timestamp at the bottom
- Responsive hamburger menu button on mobile
- Overlay background when mobile menu is open
- Smooth transitions and hover effects

### Home Page (index.html)

**Left Panel - Todo List:**
- Add new grocery items with input field
- Mark items as complete with checkboxes
- Remove items with delete buttons
- Visual feedback for completed items (opacity, strikethrough)
- Empty state message when no items exist
- Sample grocery items included

**Right Panel - AI Chat:**
- Chat interface with message bubbles
- User messages (blue, right-aligned)
- AI messages (gray, left-aligned)
- Typing indicator animation
- Welcome message for first-time users
- Voice recording button with recording status indicator
- Text input with send button
- Enter key support for sending messages
- Timestamps on all messages
- Online status indicator with pulse animation

### Settings Page (settings.html)

**Configuration Sections:**
- **Microphone Settings** - Language selection (English/Portuguese)
- **Notifications** - Toggle switches for notification preferences and sound effects
- **Appearance** - Theme selection (Dark/Light/Auto) and font size slider

**Actions:**
- Save Changes button with toast notification feedback

## Design System

### Color Scheme (Dark Theme)
- Background: `#1a1a1a`
- Panel/Card Background: `#2a2a2a`
- Secondary Background: `#3a3a3a`
- Border Colors: `#404040`, `#4a4a4a`, `#555`
- Primary Accent: `#646cff` (blue-purple)
- Chat User Message: `#3b82f6` (blue)
- Success/Online: `#10b981` (green)
- Danger/Error: `#dc3545`, `#ef4444` (red)
- Text: `rgba(255, 255, 255, 0.87)`
- Muted Text: `rgba(255, 255, 255, 0.5-0.6)`

### Typography
- Font Family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif
- Headings: Semibold weight
- Body: Regular weight

### Interactive Elements
- Buttons with hover states and transitions
- Focus states with ring indicators
- Smooth animations for state changes
- Loading/typing indicators
- Custom scrollbars (webkit)

## How to Use

1. **Open the prototypes:**
   - Simply open `index.html` in your web browser to view the home page
   - The sidebar navigation is visible on all pages

2. **Navigation:**
   - Use the left sidebar menu to navigate between Home and Settings pages
   - On desktop: Sidebar is always visible on the left
   - On mobile: Click the hamburger menu button (top left) to open/close the sidebar
   - Active page is highlighted in blue in the sidebar

3. **Interactive Features:**
   - Add todos by typing in the input field and clicking "Add" or pressing Enter
   - Toggle todo completion by clicking checkboxes
   - Remove todos with the "Remove" button
   - Send chat messages by typing and clicking send or pressing Enter
   - Try the voice recording button (simulated - shows recording UI for 3 seconds)
   - Change settings and click "Save Changes" to see a success notification
   - All interactions are client-side JavaScript simulations

## Browser Compatibility

These prototypes work in all modern browsers that support:
- ES6 JavaScript
- CSS Grid and Flexbox
- CSS Custom Properties
- Tailwind CSS via CDN

Recommended browsers:
- Chrome/Edge 90+
- Firefox 88+
- Safari 14+

## Differences from Production App

This is a **static prototype** for UI/UX demonstration purposes. Key differences from the actual React application:

- No backend API integration (uses simulated data)
- No persistent data storage (todos/messages reset on page reload)
- Voice recognition is simulated (not using Web Speech API)
- No real AI responses (shows placeholder responses)
- No routing library (uses simple HTML links)
- No build process required (standalone HTML files)
- Limited responsive testing (basic breakpoints included)

## Purpose

These prototypes serve as:
- Visual design reference
- UI/UX mockups for stakeholder review
- Design system documentation
- Quick iteration tool without build overhead
- Standalone demos that work without dependencies

## License

Part of the Grocery List AI project.
