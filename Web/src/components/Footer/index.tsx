import './style.css';

/**
 * Footer component that displays the last update timestamp
 * The timestamp is injected during build time via Vite's define plugin
 */
const Footer = () => {
  // Get build time from Vite's define plugin (injected at build time)
  const buildTime = typeof __BUILD_TIME__ !== 'undefined'
    ? new Date(__BUILD_TIME__)
    : new Date();

  /**
   * Formats the date to a user-friendly string
   * Example: "Nov 15, 2025 at 2:30 PM"
   */
  const formatDate = (date: Date): string => {
    const options: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
      hour12: true,
    };

    return date.toLocaleString('en-US', options);
  };

  return (
    <footer className="footer">
      <div className="footer-content">
        <p className="footer-text">
          Last updated: {formatDate(buildTime)}
        </p>
      </div>
    </footer>
  );
};

export default Footer;
