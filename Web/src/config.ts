/**
 * Runtime configuration loader
 * This reads environment variables at runtime instead of build time,
 * allowing Docker and other environments to override values dynamically
 */

interface Config {
  apiUrl: string
}

function getConfig(): Config {
  // Try to get from import.meta.env first (build-time variables)
  let apiUrl = import.meta.env.VITE_API_URL

  // If not found, try window.__ENV__ (set by index.html)
  if (!apiUrl && typeof window !== 'undefined' && (window as any).__ENV__) {
    apiUrl = (window as any).__ENV__.VITE_API_URL
  }

  // Fallback to localhost
  if (!apiUrl) {
    apiUrl = 'http://localhost:8000'
  }

  return {
    apiUrl,
  }
}

export const config = getConfig()
